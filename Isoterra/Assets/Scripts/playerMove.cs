using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class playerMove : MonoBehaviour
{
    public CinemachineVirtualCamera vcam;
    public CinemachineVirtualCamera[] VCameras;
    private enum cameras { thirdPerson, isometricCam}
    public Transform cameraTransform;
    Rigidbody rb;
    private Vector2 moveInput;


    public float moveSpeed = 10f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1)) {
            VCameras[(int)cameras.isometricCam].Priority = 10;
            VCameras[(int)cameras.thirdPerson].Priority = 50;
        } if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            VCameras[(int)cameras.thirdPerson].Priority = 10;
            VCameras[(int)cameras.isometricCam].Priority = 50;
        } //changes camera priority levels to make transitions
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        Debug.Log(moveInput);

        Vector3 moveDirection = cameraTransform.forward * moveInput.y + cameraTransform.right * moveInput.x;
        moveDirection.y = 0;
        moveDirection.Normalize();


        rb.velocity = moveDirection * moveSpeed;
    }

}
