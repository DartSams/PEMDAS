using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using TMPro;
using UnityEngine.InputSystem.HID;
using System.Timers;

public class rangedAttacks : MonoBehaviour
{
    RaycastHit mouseHit; //raycast of what the mouse touches
    private PlayerController input = null;
    public playerMove pm;
    public GameObject portalIn;
    public GameObject portalOut;
    Vector3 getMousePosition()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(cameraRay, out hit))
        {
            mouseHit = hit;
            Debug.DrawLine(cameraRay.origin, hit.point, Color.blue); // Draw a line from camera to the point where the ray hits
            return hit.point;
        }
        else
        {
            mouseHit = hit;
            // If the ray doesn't hit anything, return a default Vector3 value
            return Vector3.zero;
        }
    }
    // Start is called before the first frame update
    void Awake()
    {
        input = new PlayerController(); //new instance of the new input system 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.PortalIn.performed += OnShootPortalIn;
        input.Player.PortalOut.performed += OnShootPortalOut;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Player.PortalIn.performed -= OnShootPortalIn;
        input.Player.PortalOut.performed -= OnShootPortalOut;
    }


    void OnShootPortalIn(InputAction.CallbackContext value)
    {
        if (pm.currentGunType == "portalGun")
        {
            Vector3 mousePos = getMousePosition();
            //Instantiate(portalIn, hit.transform.position, Quaternion.LookRotation(gunBarrell.transform.TransformDirection(Vector3.forward.normalized)));
            portalIn.transform.position = new Vector3(mousePos.x, mousePos.y, mousePos.z - 0.5f);
            portalIn.transform.rotation = mouseHit.transform.rotation; //rotates the portal to match the rotation of what the 
        }

    } //moves the portal to where the mouse is 

    void OnShootPortalOut(InputAction.CallbackContext value)
    {
        if (pm.currentGunType == "portalGun")
        {
            Vector3 mousePos = getMousePosition();
            Debug.Log(portalOut.transform.rotation.y);
            //Instantiate(portalIn, hit.transform.position, Quaternion.LookRotation(gunBarrell.transform.TransformDirection(Vector3.forward.normalized)));
            portalOut.transform.position = new Vector3(mousePos.x, mousePos.y, mousePos.z - 0.5f);
            portalOut.transform.rotation = mouseHit.transform.rotation; //rotates the portal to match the rotation of what the 
        }

    } //moves the portal to where the mouse is 
}
