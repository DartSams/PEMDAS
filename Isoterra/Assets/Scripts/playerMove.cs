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
    public CinemachineVirtualCamera vcam;
    public CinemachineVirtualCamera[] VCameras; //list of possible virtual cameras
    private enum cameras { thirdPerson, isometricCam}
    public Transform cameraTransform; //the maincamera
    public GameObject bullet;
    public Transform gunBarrell;
    Rigidbody rb;
    private PlayerController input = null;
    public TMP_Text healthText;
    public TMP_Text currencyText;
    public TMP_Text roomTimerText;
    private Vector2 moveVector;
    Vector3 playerScale;

    public LayerMask groundMask;

    public bool isometric = true;
    public bool thirdPerson = false;
    public float rotationSpeed = 50f; //player rotation speed
    public float moveSpeed = 3f;
    public float maxHealth = 100f; //starting max health
    public float currentHealth; //variable for current health 
    public int ammo = 30;
    public float coins = 0; //variable for money
    private float groundRadius = 0.1f;
    private float jumpPower = 5;//with a gravity scale of 10 in the rigidbody
    public bool isBoosting = false;
    public float boostSpeed = 5f;
    public float boostDuration = 0.5f;
    private bool isCrouch = false;
    private bool isSprinting = false;
    private bool isShoot = false;
    public bool isJumping = false;
    public bool grounded = false;

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
        currentHealth = maxHealth;
        healthText.text = currentHealth.ToString() + "/" + maxHealth.ToString();
        playerScale = transform.localScale;

        t.Elapsed += new ElapsedEventHandler(onTimer);
        t.Interval = 100;
        t.Start();
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
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "healthPickup")
        {
            Debug.Log("Pickup (health) collected");
            currentHealth += 10;
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
        return true;
        ////return Physics.CheckSphere(feet.position, groundRadius, jumpableGround);

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
        //transform.Translate(Vector3.up); //doesnt smoothly transition use rb instead
        //rb.velocity = new Vector3(rb.velocity.x, jumpPower, rb.velocity.z);
        rb.AddForce(jumpPower * Vector3.up, ForceMode.Impulse);
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
        RaycastHit hit;
        Physics.Raycast(gunBarrell.position, transform.TransformDirection(Vector3.forward.normalized), out hit, Mathf.Infinity);
        Debug.DrawRay(gunBarrell.position, transform.TransformDirection(Vector3.forward.normalized) * hit.distance, Color.green); //makes the ray visible in scene

        if (val == 1 && ammo > 0)
        {
            isShoot = true;
            GameObject b = Instantiate(bullet, gunBarrell.position, Quaternion.LookRotation(gunBarrell.transform.TransformDirection(Vector3.forward.normalized)));
            ammo -= 1;
            //Physics.IgnoreCollision(b.GetComponent<Collider>(),gunBarrell.GetComponent<Collider>());

        }
        else if (val == 0) { isShoot = false; }
                                                                                                       

        //Debug.Log(value.isPressed);


    }

    private void OnShootCanceled(InputAction.CallbackContext value)
    {
        Debug.Log("Stopped shooting");
        //isShoot = false;

    }
}
