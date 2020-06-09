using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace Diffraction
{
    public enum InterferenceType
    {
        PositiveConstructive,
        NegativeConstructive,
        Destructive
    }

    public struct WaveIntersection
    {
        public readonly List<Vector3> IntersectionPoints;
        public InterferenceType InterferenceType;

        public WaveIntersection(List<Vector3> ip, InterferenceType it)
        {
            IntersectionPoints = ip;
            InterferenceType = it;
        }
    }

    public struct WaveFront
    {
        public GameObject WaveGameObject;
        public bool IsPositiveWave;

        public WaveFront(GameObject go, bool ipw)
        {
            WaveGameObject = go;
            IsPositiveWave = ipw;
        }
    }

    public class Wave2D : MonoBehaviour
    {
        public GameObject otherWaveSource;

        public float amplitude = 1;

        public bool useWavelength = true; // False means frequency
        public float wfValue = 500; // Wavelength (nm) or frequency (THz)
        public float localIor = 1;

        public bool interferenceCalculator;
        public float maxRadius = 5;
        public float animationSpeedScale = 1;

        public bool toDrawWaves;
        public bool toDrawPoints = true;
        public bool toDrawOnlyHit;

        public GameObject destinationWall;

        public GameObject waveFrontGameObject;
        public Material positiveMaterial;
        public Material negativeMaterial;

        // SECTION: private vars
        private Transform _myTransform;
        private Vector3 _myPos;

        private readonly List<WaveFront> _waveFronts = new List<WaveFront>();

        private bool _isPositiveWave = true;

        private float _waveSpacing = float.MaxValue;

        // Intersection points
        private Wave2D _otherHandler;

        private List<List<WaveIntersection>> _intersectionPoints = new List<List<WaveIntersection>>();

        private List<WaveIntersection> _pointsToDraw = new List<WaveIntersection>();
        private List<GameObject> _drawnPoints = new List<GameObject>();

        // private GameObject _intersectionDraws;
        //
        // private ParticleSystem _drawsParticleSystem;
        //
        // private NativeArray<ParticleSystem.Particle> _particles; 

        // Start is called before the first frame update
        void Start()
        {
            _myTransform = transform;

            _otherHandler = otherWaveSource.GetComponent<Wave2D>();

            // Convert frequency to wavelength
            if (!useWavelength)
            {
                float c = 2.99792f * Mathf.Pow(10, 8);
                float actC = c / localIor;
                wfValue = actC / (wfValue * Mathf.Pow(10, 14)); // Wavelength in meters
                wfValue *= Mathf.Pow(10, 9); // Convert to 3 digit (nm)

                useWavelength = true;
            }

            // Scale wavelength (nm) to Unity cm
            wfValue *= Mathf.Pow(10, -3);

            // Configure particle system for displaying points
            // _intersectionDraws = new GameObject("Intersection Visuals");
            // _drawsParticleSystem = _intersectionDraws.AddComponent<ParticleSystem>();
            // _drawsParticleSystem.GetComponent<ParticleSystemRenderer>().material = new Material(Shader.Find("Particles/Standard Unlit"));
            // ParticleSystem.MainModule mainModule = _drawsParticleSystem.main;
            // mainModule.startSize = 0.1f;
            // mainModule.duration = Single.MaxValue;
            // mainModule.startLifetime = 1f;
            // mainModule.startLifetime = new ParticleSystem.MinMaxCurve(1f);
            //
            // InvokeRepeating(nameof(RefreshDrawIntersectionPoints), 0f, 1f);
        }

        // Update is called once per frame
        void Update()
        {
            _myPos = _myTransform.position;

            // SECTION: create new wave
            if (_waveSpacing >= wfValue / 2)
            {
                _waveFronts.Insert(0, new WaveFront(CreateNewWave(0.05f), _isPositiveWave));

                _isPositiveWave = !_isPositiveWave;
                _waveSpacing = 0;
            }

            // SECTION: increase radius or remove
            float radiusIncrease = Time.deltaTime * animationSpeedScale;
            _waveSpacing += radiusIncrease;

            for (int i = _waveFronts.Count - 1; i >= 0; i--)
            {
                _waveFronts[i].WaveGameObject.transform.localScale += new Vector3(radiusIncrease, radiusIncrease);

                if (_waveFronts[i].WaveGameObject.transform.localScale.x > maxRadius)
                {
                    Destroy(_waveFronts[i].WaveGameObject);
                    _waveFronts.RemoveAt(i);
                }
            }

            if (!interferenceCalculator)
            {
                // SECTION: calculate intersections
                List<WaveFront> otherWaveFronts = _otherHandler._waveFronts;
                float sourceDist = Vector3.Distance(_myPos, otherWaveSource.transform.position);

                if (Mathf.Approximately(sourceDist, 0f))
                {
                    print("Wave sources are the same, full constructive interference (they add)");
                }
                else
                {
                    _intersectionPoints.Clear();
                    int myWaveIndex = 0;
                    foreach (WaveFront curWaveFront in _waveFronts)
                    {
                        int otherWaveIndex = 0;
                        GameObject curWaveGameObject = curWaveFront.WaveGameObject;
                        List<WaveIntersection> curWaveIntersections = new List<WaveIntersection>();
                        foreach (WaveFront otherWaveFront in otherWaveFronts)
                        {
                            print("Checking myWave " + myWaveIndex + " against otherWave " + otherWaveIndex);
                            GameObject otherWaveFrontGameObject = otherWaveFront.WaveGameObject;
                            // first, check if these waves will collide
                            if (curWaveGameObject.transform.localScale.x >
                                otherWaveFrontGameObject.transform.localScale.x + sourceDist)
                            {
                                print("This wave will be past the other wave (will not collide)");
                            }
                            else if (otherWaveFrontGameObject.transform.localScale.x >
                                     curWaveGameObject.transform.localScale.x + sourceDist)
                            {
                                print("Other wave will be past the this wave (will not collide)");
                            }
                            else if (curWaveGameObject.transform.localScale.x +
                                     otherWaveFrontGameObject.transform.localScale.x >=
                                     sourceDist)
                            {
                                print("These waves will collide");
                                // Get constants
                                float r1 = curWaveGameObject.transform.localScale.x;
                                float r2 = otherWaveFrontGameObject.transform.localScale.x;

                                float x1 = _myPos.x;
                                float y1 = _myPos.y;

                                Vector3 otherPosition = otherWaveFrontGameObject.transform.position;
                                float x2 = otherPosition.x;
                                float y2 = otherPosition.y;

                                // shift one of the y coordinates slightly to prevent divide by zero
                                if (Mathf.Approximately(y1, y2))
                                {
                                    y1 += Mathf.Pow(10, -5);
                                }

                                // Building the line of intersection
                                float yCoefficient = -2f * (y1 - y2);
                                float linearSlope = (2f * (x1 - x2)) / yCoefficient;
                                // print("Linear Slope: "+linearSlope);
                                float linearConstant =
                                    -1f * ((Squared(x1) + Squared(y1) - Squared(r1)) -
                                           (Squared(x2) + Squared(y2) - Squared(r2))) / yCoefficient;
                                // print("Linear Constant: "+linearConstant);

                                // Solving for x -> ax^2+bx+c
                                float aCoefficient = 1f + Squared(linearSlope);
                                // print("a Coefficient: "+aCoefficient);
                                float bCoefficient = -2f * x1 + 2f * linearSlope * (linearConstant - y1);
                                print("B Coefficient: " + bCoefficient);
                                float quadConstant = Squared(x1) + Squared(linearConstant - y1) - Squared(r1);

                                float xPlus =
                                    (-bCoefficient +
                                     Mathf.Sqrt(Squared(bCoefficient) - 4f * aCoefficient * quadConstant)) /
                                    (2f * aCoefficient);
                                float xMinus =
                                    (-bCoefficient -
                                     Mathf.Sqrt(Squared(bCoefficient) - 4f * aCoefficient * quadConstant)) /
                                    (2f * aCoefficient);

                                List<Vector3> curIntersectionPoints = new List<Vector3>
                                {
                                    new Vector3(xPlus, linearSlope * xPlus + linearConstant),
                                    new Vector3(xMinus, linearSlope * xMinus + linearConstant)
                                };

                                print("They intersect at " + curIntersectionPoints[0] + " and " +
                                      curIntersectionPoints[1]);

                                // Determine the type of interference
                                InterferenceType interferenceType;
                                print("Current Wave is " +
                                      (curWaveFront.IsPositiveWave ? "positive" : "negative"));
                                print("Other Wave is " +
                                      (otherWaveFront.IsPositiveWave ? "positive" : "negative"));

                                bool curWaveIsPositive = curWaveFront.IsPositiveWave;
                                bool otherWaveIsPositive = otherWaveFront.IsPositiveWave;

                                if (curWaveIsPositive && otherWaveIsPositive)
                                {
                                    interferenceType = InterferenceType.PositiveConstructive;
                                }
                                else if (!curWaveIsPositive && !otherWaveIsPositive)
                                {
                                    interferenceType = InterferenceType.NegativeConstructive;
                                }
                                else
                                {
                                    interferenceType = InterferenceType.Destructive;
                                }

                                print("They will create a " + interferenceType + " interference");

                                // Add to the list
                                curWaveIntersections.Add(new WaveIntersection(curIntersectionPoints, interferenceType));
                            }
                            else
                            {
                                print("These waves are too far and will not collide");
                            }

                            otherWaveIndex++;
                        }

                        _intersectionPoints.Add(curWaveIntersections);

                        myWaveIndex++;
                        print("================");
                    }
                }

                // SECTION: draw intersections
                RefreshDrawIntersectionPoints();
                for (int i = 0; i < _pointsToDraw.Count; i++)
                {
                    _drawnPoints[i].SetActive(toDrawPoints);
                    Vector3 drawPoint = _pointsToDraw[i].IntersectionPoints[0];
                    print("Draw Position: " + drawPoint);
                    if (drawPoint.x > destinationWall.transform.position.x)
                    {
                        drawPoint.x = destinationWall.transform.position.x;
                        _drawnPoints[i].SetActive(toDrawOnlyHit);
                    }

                    _drawnPoints[i].transform.position = drawPoint;
                    _drawnPoints[i].GetComponent<Renderer>().material =
                        _pointsToDraw[i].InterferenceType == InterferenceType.PositiveConstructive
                            ? positiveMaterial
                            : negativeMaterial;
                }
                // for (int i = 0; i < _particles.Length; i++)
                // {
                //     ParticleSystem.Particle particle = _particles[i];
                //
                //     Vector3 givenIntersectionPoint = _pointsToDraw[i].IntersectionPoints[0];
                //     if (givenIntersectionPoint.x > 4)
                //     {
                //         givenIntersectionPoint.x = 4;
                //     }
                //
                //     particle.position = givenIntersectionPoint;
                //     particle.startColor = _pointsToDraw[i].InterferenceType == InterferenceType.PositiveConstructive
                //         ? positiveMaterial.color
                //         : negativeMaterial.color;
                // }
                // _drawsParticleSystem.SetParticles(_particles,_particles.Length);
                // foreach (List<WaveIntersection> intersectionPoint in _intersectionPoints)
                // {
                //     foreach (WaveIntersection waveIntersection in intersectionPoint)
                //     {
                //         foreach (Vector3 waveIntersectionIntersectionPoint in waveIntersection.IntersectionPoints)
                //         {
                //             Debug.DrawLine(_myPos, waveIntersectionIntersectionPoint, Color.red, 0.01f);
                //             
                //         }
                //     }
                // }

                print("======================\n\n======================");
            }
        }

        // SECTION: Private functions
        private void RefreshDrawIntersectionPoints()
        {
            // Count number of intersections
            _pointsToDraw.Clear();
            foreach (List<WaveIntersection> waves in _intersectionPoints)
            {
                foreach (WaveIntersection waveIntersection in waves)
                {
                    foreach (Vector3 intersectionPoint in waveIntersection.IntersectionPoints)
                    {
                        if (intersectionPoint.x < 0)
                        {
                            continue;
                        }

                        if (waveIntersection.InterferenceType == InterferenceType.Destructive)
                        {
                            continue;
                        }

                        List<Vector3> intPointList = new List<Vector3> {intersectionPoint};
                        _pointsToDraw.Add(new WaveIntersection(intPointList, waveIntersection.InterferenceType));
                    }
                }
            }

            // emits that many particles
            print("Current drawPoints: "+_drawnPoints.Count+"; Current pointsToDraw: "+_pointsToDraw.Count);
            if (_drawnPoints.Count < _pointsToDraw.Count)
            {
                for (int i = _drawnPoints.Count; i < _pointsToDraw.Count; i++)
                {
                    GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    point.transform.localScale = Vector3.one * 0.15f;
                    _drawnPoints.Add(point);
                }
            }
            else
            {
                print("Removing "+(_pointsToDraw.Count - _drawnPoints.Count)+" spheres");
                _drawnPoints.RemoveRange(0, _drawnPoints.Count - _pointsToDraw.Count);
            }

            // _drawsParticleSystem.Emit(_pointsToDraw.Count);
            //
            // // resets the particle NativeArray and reloads it
            // _particles.Dispose();
            // _particles = new NativeArray<ParticleSystem.Particle>();
            //
            // _drawsParticleSystem.GetParticles(_particles);
        }

        private GameObject CreateNewWave(float drawWidth = 0.1f)
        {
            GameObject wave = Instantiate(waveFrontGameObject, _myPos, Quaternion.identity);
            wave.transform.localScale = Vector3.zero;
            // wave.transform.parent = _myTransform;

            WaveFront2D waveFrontHandler = wave.GetComponent<WaveFront2D>();
            waveFrontHandler.lineWidth = drawWidth;
            waveFrontHandler.isPositiveWave = _isPositiveWave;

            wave.SetActive(toDrawWaves);

            return wave;
        }

        private float Squared(float input)
        {
            return Mathf.Pow(input, 2);
        }
    }
}