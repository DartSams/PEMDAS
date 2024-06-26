using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using TMPro;
using UnityEngine.InputSystem.HID;
using System.Timers;

public class playerMove : MonoBehaviour
{
    public CinemachineVirtualCamera[] VCameras; //list of possible virtual cameras
    private enum cameras { thirdPerson, isometricCam}
    public Transform cameraTransform; //the maincamera
    private GameObject weaponPickup; //placeholder gameobject set every time the player enters a pickupable weapon
    public Transform gunHolster; //transform position that holds the position for where the player holds their gun
    public List<GameObject> weapons = new List<GameObject>();
    public GameObject bullet; //objects to be shot out of gun
    public Transform gunBarrell; //position of where gun bullets should spawn at 
    Rigidbody rb; //the players rigidbody componenet
    private PlayerController input = null;
    public TMP_Text healthText; //player health text 
    public TMP_Text currencyText; //player currency text
    public TMP_Text roomTimerText; //player room timer text (the timer will be reset when entering a room everytime)
    public TMP_Text pickupText;
    private Vector2 moveVector; //vector 2 of player movement
    Vector3 playerScale; //the player original height scale
    public LayerMask groundMask; //the ground layer used to judge what the player can jump on
    RaycastHit hit; //raycast of what the players gun hits
    public GameObject portalIn; 
    public GameObject portalOut;
    public UIBar uibar;
    public Transform feet;
    public LayerMask jumpableGround;


    public string currentGunType = "portal";
    private bool isometric = true;
    private bool thirdPerson = false;
    private float rotationSpeed = 50f; //player rotation speed
    public float moveSpeed = 3f;
    private float maxHealth = 100f; //starting max health
    private float currentHealth; //variable for current health 
    public float maxStamina = 100f; //starting max stamina
    private float currentStamina; //variable for current stamina 
    public int ammo = 30;
    private float coins = 0; //variable for money
    private float groundRadius = 0.1f;
    private float jumpPower = 5;//with a gravity scale of 10 in the rigidbody
    private float boostSpeed = 5f; //variable to multiple moveSpeed by to simulate sprinting
    private float boostDuration = 0.5f; //how long sprint last until moveSpeed returns to normal
    private bool isBoosting = false;
    private bool isCrouch = false;
    private bool isSprinting = false;
    private bool isShoot = false;
    private bool isJumping = false;
    private bool grounded = false;
    private bool canPickupState = false; //variable to toggle when player enters a weaponPickup


    Timer t = new Timer();
    private string hour = "0";
    private string minute = "00";
    private string second = "00.0";
    private float timer = 0;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = new PlayerController(); //new instance of the new input system 
        currentHealth = maxHealth; //starts the scene by setting the current health to the max health
        currentStamina = maxStamina;
        uibar.updateHealthbar(maxHealth, currentHealth);
        uibar.updateStaminabar(maxStamina, currentStamina);

        healthText.text = currentHealth.ToString() + "/" + maxHealth.ToString();
        playerScale = transform.localScale;
        pickupText.enabled = false;

        t.Elapsed += new ElapsedEventHandler(onTimer);
        t.Interval = 100;
        t.Start(); //starts room timer
        foreach (Transform child in gunHolster.transform)
        {
            //Destroy(child.gameObject); //removes the current gun from the gunholster
            //child.gameObject.SetActive(false);
            weapons.Add(child.gameObject);
        } //checks if a weapon is already equiped at the start of the gun 
    }

    // Update is called once per frame
    void Update()
    {
        roomTimerText.text = makeTime(timer); //updates room timer

        Move();
        if (isometric) rotatePlayerToMouse();
        if (thirdPerson) rotatePlayerToCamera();
        if (Input.GetKeyUp(KeyCode.Alpha1)) {
            VCameras[(int)cameras.isometricCam].Priority = 10;
            VCameras[(int)cameras.thirdPerson].Priority = 50;
            isometric = false;
            thirdPerson = true;
        } if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            VCameras[(int)cameras.thirdPerson].Priority = 10;
            VCameras[(int)cameras.isometricCam].Priority = 50;
            isometric = true;
            thirdPerson = false;
        } //changes camera priority levels to make transitions

        if (Input.GetKeyUp(KeyCode.P))
        {
            if (t.Enabled)
            {
                t.Stop();
            } else
            {
                t.Start();
            }
        } //pause the game

        if (Input.GetKeyUp(KeyCode.E) && canPickupState)
        {
            Debug.Log("picking up weapon");
            Debug.Log(transform.childCount);
            foreach (Transform child in gunHolster.transform)
            {
                //Destroy(child.gameObject); //removes the current gun from the gunholster
                child.gameObject.SetActive(false);
            }
            weaponPickup.transform.parent = gunHolster; //adds the pickupable weapon to the players gunholster so now the player can utilize the new wapon
            weaponPickup.transform.position = gunHolster.transform.position; //sets the position of the new pickupable weapon
            weaponPickup.transform.rotation = gunHolster.transform.rotation; //sets the rotation of the new pickupable weapon
            currentGunType = weaponPickup.name; //updates the current weapon name to the new pickupable weapon name
            weaponPickup.tag = "weapon";
            canPickupState = false; //player has left the pickupable weapon area
            pickupText.enabled = false;
            weapons.Add(weaponPickup);
        } //checks if the player presses E and is near a pickupable weapon 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "healthPickup")
        {
            Debug.Log("Pickup (health) collected");
            currentHealth += 10;
            uibar.updateHealthbar(maxHealth, currentHealth);
            healthText.text = currentHealth.ToString() + "/" + maxHealth.ToString();
            Destroy(collision.gameObject);
        }

        if (collision.collider.tag == "coinPickup")
        {
            Debug.Log("Pickup (coin) collected");
            coins += 1;
            currencyText.text = "Coins: " + coins.ToString();
            Destroy(collision.gameObject);
        }

        if (collision.collider.tag == "damagePickup")
        {
            Debug.Log("Pickup (damage) collected");
            Destroy(collision.gameObject);
        }
        if (collision.collider.tag == "ground")
        {
            grounded = true;
        }

        if (collision.collider.tag == "portalIn")
        {
            if (portalOut.transform.rotation.y > 0) //checks the opposite portal y rotation (unity rotation goes from 0-1 but in coordinates its 0-90 degrees when a object is rotated 90 degress code reads it as 0.707)
            {
                transform.position = new Vector3(portalOut.transform.position.x - 1, portalOut.transform.position.y, portalOut.transform.position.z);
            }
            else
            {
                transform.position = new Vector3(portalOut.transform.position.x, portalOut.transform.position.y, portalOut.transform.position.z - 1.3f);
            }

        } //sends the player to the opposite portal 

        if (collision.collider.tag == "portalOut")
        {
            if (portalIn.transform.rotation.y > 0)
            {
                transform.position = new Vector3(portalIn.transform.position.x - 1, portalIn.transform.position.y, portalIn.transform.position.z);
            }
            else
            {
                transform.position = new Vector3(portalIn.transform.position.x, portalIn.transform.position.y, portalIn.transform.position.z + 1.3f);
            }
        } //sends the player to the opposite portal 

        if (collision.collider.tag == "weaponPickup")
        {
            pickupText.enabled = true;
            canPickupState = true; 
            weaponPickup = collision.gameObject; //creates a placeholder gameobject variable so can check if the player presses E to pickup
        } //toggle when the player enters a pickupable weapon
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.tag == "weaponPickup")
        {
            canPickupState = false;
            pickupText.enabled = false;
        }
    }

    private void Move()
    {
        //Debug.Log(moveVector);
        Vector3 moveDirection = cameraTransform.forward * moveVector.y + cameraTransform.right * moveVector.x; // Calculate the movement direction based on camera orientation
        moveDirection.y = 0; // freezes the y coordinate

        moveDirection.Normalize(); // Normalize the direction to prevent faster movement diagonally
        // rb.MovePosition(transform.position + moveDirection * moveSpeed * Time.deltaTime); //causes conflict when using with rb.addforce for jumping
        //rb.position += moveDirection * moveSpeed * Time.deltaTime; //its too laggy because of not using interpolation
        //transform.position += moveDirection * moveSpeed * Time.deltaTime;

        Vector3 force = moveDirection * moveSpeed * rb.mass * Time.deltaTime; 

        rb.AddForce(force, ForceMode.VelocityChange); //using rb.addforce here for movement because am also using it at the same time for jump
    }

    private bool IsGrounded()
    {
        //return true;
        return Physics.CheckSphere(feet.position, groundRadius, jumpableGround);

    }



    void rotatePlayerToMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayLength;

        if (groundPlane.Raycast(cameraRay, out rayLength))
        {
            Vector3 pointToLook = cameraRay.GetPoint(rayLength);
            Debug.DrawLine(cameraRay.origin, pointToLook, Color.blue); //draws a line from camera to where the mouse is positioned

            Vector3 directionToLook = (new Vector3(pointToLook.x, transform.position.y, pointToLook.z) - transform.position).normalized;

            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.forward, directionToLook); //converts forward direction to the mouse target direction 

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime); //interpolation (smooth transitioning)
        }
    } //rotates the player to the direction of the mouse when in isometric view

    private void rotatePlayerToCamera()
    {
        //rotate camera
        //float targetAngle = cameraTransform.eulerAngles.y; //camera current y rotation (Vector3)
        Quaternion targetRotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
        rb.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

    } //rotates the player to the main camera camera when in third person

    void onTimer(object source, ElapsedEventArgs e)
    {
        timer++;
    }

    private string makeTime(float timeFunction)
    {
        hour = "" + (int)(timeFunction / 3600) % 24;
        if ((int)timeFunction / 600 % 60 < 10)
        {
            minute = "0" + (int)(timeFunction / 600) % 60;
        }
        else
        {
            minute = "" + (int)(timeFunction / 600) % 60;
        }
        if (Mathf.RoundToInt(timeFunction % 600) / 10f < 10)
        {
            second = "0" + ((float)Mathf.RoundToInt(timeFunction % 600) / 10f);
        }
        else
        {
            second = "" + ((float)Mathf.RoundToInt(timeFunction % 600) / 10f);
        }
        return ("" + hour + ":" + minute + ":" + second);
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Move.performed += OnMovePerformed;
        input.Player.Move.canceled += OnMoveCancelled;


        input.Player.Jump.performed += OnJumpPerformed;
        input.Player.Jump.canceled += OnJumpCanceled;
        input.Player.Sprint.performed += OnSprintPerformed;
        input.Player.Crouch.performed += OnCrouchPerformed;
        input.Player.Crouch.canceled += OnCrouchCanceled;


        input.Player.Shoot.performed += OnShootPerformed;
        input.Player.Shoot.performed += OnShootCanceled;

        input.Player.SwitchPrimary.performed += OnSwitchPrimary;
        input.Player.SwitchSecondary.performed += OnSwitchSecondary;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Player.Move.performed -= OnMovePerformed;
        input.Player.Move.canceled -= OnMoveCancelled;
        input.Player.Jump.performed -= OnJumpPerformed;
        input.Player.Jump.canceled -= OnJumpCanceled;
        input.Player.Sprint.performed -= OnSprintPerformed;
        input.Player.Sprint.canceled -= OnSprintCanceled;
        input.Player.Crouch.performed -= OnCrouchPerformed;
        input.Player.Crouch.canceled -= OnCrouchCanceled;


        input.Player.Shoot.performed -= OnShootPerformed;
        input.Player.Shoot.canceled -= OnShootCanceled;

        input.Player.SwitchPrimary.performed -= OnSwitchPrimary;
        input.Player.SwitchSecondary.performed -= OnSwitchSecondary;
    }

    private void OnMovePerformed(InputAction.CallbackContext value)
    {
        moveVector = value.ReadValue<Vector2>();
        //Debug.Log(moveVector);


    }

    private void OnMoveCancelled(InputAction.CallbackContext value)
    {
        moveVector = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext value)
    {
        isJumping = true;
        //rb.velocity = new Vector3(rb.velocity.x, jumpPower, rb.velocity.z);
        if (value.performed)
        {
            jump();
            Debug.Log("jumping");
        }
        
    }//if spacebar is pressed and player is on the ground then jump

    void jump()
    {
        Debug.Log(IsGrounded());
        //transform.Translate(Vector3.up); //doesnt smoothly transition use rb instead
        //rb.velocity = new Vector3(rb.velocity.x, jumpPower, rb.velocity.z);
        if (IsGrounded())
        {
            rb.AddForce(jumpPower * Vector3.up, ForceMode.Impulse);

        }
        grounded = false;
    }

    private void OnJumpCanceled(InputAction.CallbackContext value)
    {
        isJumping = false;
    }

    private void OnSprintPerformed(InputAction.CallbackContext value)
    {
        Debug.Log("Sprinting");
        isSprinting = true;
        currentStamina = currentStamina - 10;
        uibar.updateStaminabar(maxStamina, currentStamina);
        moveSpeed *= boostSpeed; //converts the regular movement speed to be multiplied by the boost speed
        //speed = Mathf.Clamp(speed, 5f, 30);
        Invoke("StopBoost", boostDuration); //calls function (stopboost) after a certain amount of time (boostduration)
    }

    private void OnSprintCanceled(InputAction.CallbackContext value)
    {
        isSprinting = false;
    }

    void StopBoost()
    {
        // Stop boosting.
        isSprinting = false;
        moveSpeed /= boostSpeed; //return the speed to the original amount
    }

    private void OnCrouchPerformed(InputAction.CallbackContext value)
    {
        transform.localScale = new Vector3(1,0.5f,1); //change scale down to half
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z); //move player dw
        //rb.AddForce(transform.forward * 10);
        isCrouch = true;
    }

    private void OnCrouchCanceled(InputAction.CallbackContext value)
    {
        transform.localScale = playerScale; //changes scale back to original size
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        isCrouch = false;
    }

    private void OnShootPerformed(InputAction.CallbackContext value)
    {
        float val = value.ReadValue<float>();
        //print(val);
        Physics.Raycast(gunBarrell.position, transform.TransformDirection(Vector3.forward.normalized), out hit, Mathf.Infinity);
        Debug.DrawRay(gunBarrell.position, transform.TransformDirection(Vector3.forward.normalized) * hit.distance, Color.green); //makes the ray visible in scene

        if (val == 1 && ammo > 0)
        {
            shoot();
            Debug.Log(hit.collider.name);

        }
        else if (val == 0) { isShoot = false; }
                                                                                                       

        //Debug.Log(value.isPressed);
    }
    private void shoot()
    {
        isShoot = true;
        ammo -= 1;
        currentStamina -= 5;
        uibar.updateStaminabar(maxStamina, currentStamina);
        if (currentGunType == "pistol")
        {
            Instantiate(bullet, gunBarrell.position, Quaternion.LookRotation(gunBarrell.transform.TransformDirection(Vector3.forward.normalized)));
        } else if (currentGunType == "portalGun")
        {
            // refer to the rangedAttacks script
        }
    }

    private void OnShootCanceled(InputAction.CallbackContext value)
    {
        Debug.Log("Stopped shooting");
        //isShoot = false;

    }

    private void OnSwitchPrimary(InputAction.CallbackContext value)
    {
        weapons[1].gameObject.SetActive(false);
        weapons[0].gameObject.SetActive(true);
        weapons[0].transform.parent = gunHolster; //adds the pickupable weapon to the players gunholster so now the player can utilize the new wapon
        weapons[0].transform.position = gunHolster.transform.position; //sets the position of the new pickupable weapon
        weapons[0].transform.rotation = gunHolster.transform.rotation; //sets the rotation of the new pickupable weapon
        currentGunType = weapons[0].name; //updates the current weapon name to the new pickupable weapon name
    }

    private void OnSwitchSecondary(InputAction.CallbackContext value)
    {
        weapons[0].gameObject.SetActive(false);
        weapons[1].gameObject.SetActive(true);
        weapons[1].transform.parent = gunHolster; //adds the pickupable weapon to the players gunholster so now the player can utilize the new wapon
        weapons[1].transform.position = gunHolster.transform.position; //sets the position of the new pickupable weapon
        weapons[1].transform.rotation = gunHolster.transform.rotation; //sets the rotation of the new pickupable weapon
        currentGunType = weapons[1].name; //updates the current weapon name to the new pickupable weapon name
    }
}
