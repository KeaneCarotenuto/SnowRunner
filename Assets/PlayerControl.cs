using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Player controller that works with touch controls
/// </summary>
public class PlayerControl : MonoBehaviour
{
    [Header("References")]
    [Header("Board")]
    public Rigidbody2D m_boardRB;
    public Transform m_leftTouch;
    public Transform m_middleTouch;
    public Transform m_rightTouch;

    [Header("Body")]
    public HeadCollider m_head;
    public List<GameObject> m_feet;

    [Header("UI")]
    public TextMeshProUGUI m_currentSpeedText;
    public TextMeshProUGUI m_maxSpeedText;
    public Image m_speedo;
    public Image m_needle;

    [Header("Stats")]
    public float m_maxSpeed = 35f;
    public float m_maxIncreaseRate = 0.1f;

    public bool m_isDead = false;

    public bool m_isWalking = false;
    private float m_walkStartTime = 0f;

    
    [Header("Tricks")]
    public PointsManager m_pointsManager;
    public float m_touchRadius = 0.1f;
    public bool m_touchingLeft = false;
    public bool m_touchingMiddle = false;
    public bool m_touchingRight = false;
    public float m_startAngle = 0f;
    public bool isInAir {
        get { return !m_touchingLeft && !m_touchingMiddle && !m_touchingRight; }
    }
    public bool m_groundedLastFrame = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable() {
        m_head.m_onCollisionEnter += OnHeadCollision;
    }

    private void OnDisable() {
        m_head.m_onCollisionEnter -= OnHeadCollision;
    }

    private void OnHeadCollision(Collision2D other)
    {
        Die();
    }

    private void Die()
    {
        m_isDead = true;

        // disable hinge joints on all feet
        foreach (GameObject foot in m_feet)
        {
            foot.GetComponent<HingeJoint2D>().enabled = false;
        }

        // set all joint limits to -75,75
        HingeJoint2D[] joints = GetComponentsInChildren<HingeJoint2D>();
        foreach (HingeJoint2D joint in joints)
        {
            JointAngleLimits2D limits = joint.limits;
            limits.min = -75;
            limits.max = 75;
            joint.limits = limits;
        }

        // set all rigidbodies to mass 2 (excluding the board)
        Rigidbody2D[] rigidbodies = GetComponentsInChildren<Rigidbody2D>();
        foreach (Rigidbody2D rb in rigidbodies)
        {
            if (rb.gameObject != m_boardRB.gameObject)
            {
                rb.mass = 2;
            }
        }

        // get main cam and set follor target to head
        Camera.main.GetComponent<CameraFollow>().m_target = m_head.transform;
    }

    private void FixedUpdate()
    {
        GetInput();
        
        CalculateScore();

        CalculateSpeed();

        UpdateUI();

        m_groundedLastFrame = !isInAir;
    }

    private void CalculateScore()
    {
        if (m_isDead) return;

        // add to max speed
        m_maxSpeed += m_maxIncreaseRate * Time.fixedDeltaTime;

        // check left, middle, right touch:
        // spherecast from touch position to ground
        // if hit, set touching to true
        // if not hit, set touching to false

        RaycastHit2D leftHit = Physics2D.CircleCast(m_leftTouch.position, m_touchRadius, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
        RaycastHit2D middleHit = Physics2D.CircleCast(m_middleTouch.position, m_touchRadius, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
        RaycastHit2D rightHit = Physics2D.CircleCast(m_rightTouch.position, m_touchRadius, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));

        m_touchingLeft = leftHit;
        m_touchingMiddle = middleHit;
        m_touchingRight = rightHit;

        if (m_groundedLastFrame && isInAir){
            // just jumped

            // get total rotation of board
            m_startAngle = m_boardRB.rotation;
        }

        if (isInAir){
            // check if we're doing a trick
            // get current angle
            float currentAngle = m_boardRB.rotation;

            // get angle difference
            float angleDiff = currentAngle - m_startAngle;

            // backflip
            if (angleDiff > 180){
                // backflip
                Debug.Log("Backflip!");

                // add to trick list
                m_pointsManager.AddTrick(PointsManager.TrickType.Backflip);

                m_startAngle = currentAngle;
            }

            // frontflip
            if (angleDiff < -180){
                // frontflip
                Debug.Log("Frontflip!");

                // add to trick list
                m_pointsManager.AddTrick(PointsManager.TrickType.Frontflip);

                m_startAngle = currentAngle;
            }
        }
    }

    private void GetInput()
    {
        if (m_isDead) return;

        // if A is down, rotate left
        if (Input.GetKey(KeyCode.A))
        {
            RotateLeft();
        }

        // if D is down, rotate right
        if (Input.GetKey(KeyCode.D))
        {
            RotateRight();
        }

        // if left side of screen is touched, rotate left
        if (Input.touchCount > 0 && Input.GetTouch(0).position.x < Screen.width / 2)
        {
            RotateLeft();
        }

        // if right side of screen is touched, rotate right
        if (Input.touchCount > 0 && Input.GetTouch(0).position.x > Screen.width / 2)
        {
            RotateRight();
        }
    }

    private void CalculateSpeed()
    {
        if (m_isDead) return;

        // limit x velocity to max speed
        if (m_boardRB.velocity.x > m_maxSpeed)
        {
            m_boardRB.velocity = new Vector2(m_maxSpeed, m_boardRB.velocity.y);
        }

        // if moving backwards, start walking
        if (!m_isWalking && m_boardRB.velocity.x < 0.0f)
        {
            m_isWalking = true;
            m_walkStartTime = Time.time;
        }

        // if walking, move forwards slowly
        if (m_isWalking)
        {
            // min velocity is 4.5f (lerp from 0-4.5f over 1 second)
            m_boardRB.velocity = new Vector2(Mathf.Max(m_boardRB.velocity.x, Mathf.Lerp(0f,4.5f, Time.time - m_walkStartTime)), m_boardRB.velocity.y);

            // if x velocity is greater than 5, stop walking
            if (m_boardRB.velocity.x > 5)
            {
                m_isWalking = false;
            }
        }

        if (m_touchingRight && !m_touchingLeft && !m_touchingMiddle){
            // Stoppie Trick
            // Slow down the board
            m_boardRB.velocity = new Vector2(Mathf.Lerp(m_boardRB.velocity.x, 0f, Time.deltaTime * 0.5f), m_boardRB.velocity.y);
        }
    }

    private void UpdateUI()
    {
        // get km/h (from m/s)
        float maxKMH = m_maxSpeed;
        float currentKMH = m_boardRB.velocity.x;
        // update m_speedText with max speed (0 decimal place)
        m_maxSpeedText.text = "Max " + maxKMH.ToString("0") + " km/h";
        m_currentSpeedText.text = currentKMH.ToString("0") + " km/h";

        // rotate needle (180 degrees = 0 km/h, 0 degrees = maxKMH)
        m_needle.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(180, 0, currentKMH / maxKMH));

        // set colour of UI based on speed (white = 0%-50%, yellow = 50%-75%, red = 75%-100%)
        Color col = Color.white;
        // lerp from white to yellow
        if (currentKMH / maxKMH < 0.5f)
        {
            col = Color.Lerp(Color.white, Color.yellow, currentKMH / maxKMH / 0.5f);
        }
        // lerp from yellow to red
        else
        {
            col = Color.Lerp(Color.yellow, Color.red, (currentKMH / maxKMH - 0.5f) / 0.5f);
        }
        // set colour
        m_speedo.color = col;
        m_needle.color = col;
        m_currentSpeedText.color = col;
    }

    public void RotateLeft(){
        m_boardRB.angularVelocity = 200;
    }

    public void RotateRight(){
        m_boardRB.angularVelocity = -200;
    }
}
