using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class controller : MonoBehaviour
{
    [Header("Game-Play")]
    public float moveSpeed = 5f;
    public bool canMove;
    public bool levelComplete;
    public Rigidbody2D rb;
    public GameObject Manager;
    public Animator playerAnimator;
    public GameObject retryButton;

    [Header("Robot")]
    public bool controlRobot;
    public bool nearComputer;
    public GameObject controllingRobot;
    public GameObject robotLights;
    public float switchTimer;

    [Header("Door")]    
    public Sprite openDoor;
    public Sprite closeDoor;
    public GameObject door;
    public GameObject doorLight;
    public GameObject doorBlocker;

    [Header("Audio")]
    public AudioSource robotPowerDown;    
    public AudioSource running;
    public bool runningSound;




    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R key pressed");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        // Get input from the player
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(horizontalInput, verticalInput);
        movement.Normalize();
        
        if (!controlRobot)
        {

            if (canMove)
            {
                switchTimer -= Time.deltaTime;
                rb = GetComponent<Rigidbody2D>();
                moveSpeed = 5f;
                MovePlayer(this.gameObject, movement);
            }
            else
            {
                rb.velocity = new Vector2(0, 0);
            }          
        }
        else
        {
            if (Manager.GetComponent<UIController>().introLevel)
            {
                Manager.GetComponent<UIController>().robotAnimator.SetBool("In", true);
            }

            switchTimer -= Time.deltaTime;
            rb = controllingRobot.GetComponent<Rigidbody2D>();
            moveSpeed = 3.5f;
            MovePlayer(controllingRobot, movement);

            if ((Input.GetKeyDown(KeyCode.E) && switchTimer<=0) || controllingRobot.GetComponent<robotController>().destroyed)
            {
                switchTimer = 0.2f;
                if (!controllingRobot.GetComponent<robotController>().destroyed)
                {
                    robotPowerDown.Play();

                    robotLights.SetActive(false);

                    
                }
                
                controlRobot = false;
                canMove = true;

                // Sets the player icon active
                for (int i = 0; i < Manager.GetComponent<manager>().playerIcons.Length; i++)
                {
                    Manager.GetComponent<manager>().fadeIcons(Manager.GetComponent<manager>().playerIcons[i]);
                }
                Manager.GetComponent<manager>().unfadeIcons(Manager.GetComponent<manager>().playerIcons[0]);
            }
        }
        
        if (levelComplete)
        {
            door.GetComponent<SpriteRenderer>().sprite = openDoor;
            doorLight.SetActive(false);
            doorBlocker.SetActive(false);
            if (Manager.GetComponent<UIController>().introLevel)
            {
                Manager.GetComponent<UIController>().robotAnimator.SetBool("Out", true);
                Manager.GetComponent<UIController>().endLevel.SetBool("In", true);
            }
        }
        else
        {
            door.GetComponent<SpriteRenderer>().sprite = closeDoor;
            doorLight.SetActive(true);
            doorBlocker.SetActive(true);
        }

        if (controllingRobot && controllingRobot.GetComponent<robotController>().destroyed)
        {
            retryButton.SetActive(true);
        }

        
    }
    void MovePlayer(GameObject controllingPlayer, Vector2 movement)
    {
        rb.velocity = new Vector2(movement.x * moveSpeed, movement.y * moveSpeed);

        if (!controlRobot)
        {            
            playerAnimator.SetFloat("speed", Mathf.Abs(rb.velocity.magnitude));
            if (Mathf.Abs(rb.velocity.magnitude) == 0)
            {
                running.Stop();
                runningSound = false;

                // Sets all animations false before assigning one
                setBoolsFalse();
                if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.A))
                {
                    playerAnimator.SetBool("idleSide", true);
                }
                else if (Input.GetKeyUp(KeyCode.S))
                {
                    playerAnimator.SetBool("idleDown", true);
                }
                else if (Input.GetKeyUp(KeyCode.W))
                {
                    playerAnimator.SetBool("idleUp", true);
                }
            }
            else
            {
                if (!runningSound)
                {
                    running.Play();
                    runningSound = true;
                }

                // Sets all animations false before assigning one
                setBoolsFalse();
                if (Input.GetKey(KeyCode.D))
                {
                    FlipPlayer(true);
                    playerAnimator.SetBool("runSide", true);
                }
                else if (Input.GetKey(KeyCode.A))
                {
                    FlipPlayer(false);
                    playerAnimator.SetBool("runSide", true);
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    playerAnimator.SetBool("runDown", true);
                }
                else if (Input.GetKey(KeyCode.W))
                {
                    playerAnimator.SetBool("runUp", true);
                }
            }
        }
        else
        {
            running.Stop();
            runningSound = false;
        }
    }

    void FlipPlayer(bool facingLeft)
    {
        transform.localScale = new Vector3(facingLeft ? -1 : 1, 1, 1);
    }


    public void setBoolsFalse()
    {
        playerAnimator.SetBool("idleDown", false);
        playerAnimator.SetBool("idleUp", false);
        playerAnimator.SetBool("idleSide", false);
        playerAnimator.SetBool("runSide", false);
        playerAnimator.SetBool("runDown", false);
        playerAnimator.SetBool("runUp", false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("door"))
        {           
            Manager.GetComponent<manager>().levels[SceneManager.GetActiveScene().buildIndex - 1] = true;
            Debug.Log(SceneManager.GetActiveScene().buildIndex - 1);
            Manager.GetComponent<manager>().levelNum++;
            Manager.GetComponent<manager>().SavePlayer();
            StartCoroutine(changeLevel());
        }
    }

    IEnumerator changeLevel()
    {
        
        if(SceneManager.GetActiveScene().buildIndex == 24)
        {
            yield return new WaitForSeconds(2f);
            Manager.GetComponent<manager>().fade.SetTrigger("fade");
            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene(0);
        }
        else
        {
            canMove = false;
            Manager.GetComponent<manager>().fade.SetTrigger("fade");
            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        
    }
}
