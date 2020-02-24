using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class CameraController : MonoBehaviour
{
    public float flySpeed = 3;
    public float lookSpeed = 5;

    private float _xCamRot;
    private float _yCamRot;

    private bool active = true;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _yCamRot = 30;
        _xCamRot = -40;
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            float up = Input.GetAxis("YVertical");

            _xCamRot += lookSpeed * mouseX;
            _yCamRot -= lookSpeed * mouseY;
            transform.eulerAngles = new Vector3(_yCamRot, _xCamRot, 0f);

            Vector3 movement = horizontal * flySpeed * Time.deltaTime * transform.right +
                               vertical * flySpeed * Time.deltaTime * transform.forward +
                               up * flySpeed * Time.deltaTime * transform.up;

            // Debug.Log(movement);
            transform.position += movement;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            active = false;
            Cursor.lockState = CursorLockMode.Confined;
        }
        else if (Input.GetMouseButton(0))
        {
            active = true;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}