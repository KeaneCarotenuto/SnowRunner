using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GooglePlayGames;

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
    public BodyCollider m_headCollider;
    public BodyCollider m_leftHandCollider;
    public BodyCollider m_rightHandCollider;
    public List<GameObject> m_feet;

    [Header("UI")]
    public TextMeshProUGUI m_currentSpeedText;
    public TextMeshProUGUI m_maxSpeedText;
    public Image m_speedo;
    public Image m_needle;
    public TextMeshProUGUI m_highscoreText;
    public TextMeshProUGUI m_leftText;
    public TextMeshProUGUI m_rightText;

    [Header("Stats")]
    public float m_maxSpeed = 35f;
    private float m_initialMaxSpeed = 0f;
    public float m_topSpeed = 0f;
    public float m_maxIncreaseRate = 0.1f;

    public bool m_isDead = false;
    public bool m_invincible = false;

    public bool m_isBoosting = false;
    private float m_boostStartTime = 0f;

    [Header("Input")]
    public bool m_leftPressed = false;
    public bool m_rightPressed = false;

    public bool m_leftHeld = false;
    public bool m_rightHeld = false;

    
    [Header("Tricks")]
    public PointsManager m_pointsManager;
    public float m_touchRadius = 0.1f;
    public bool m_leftBoardOnGround = false;
    public bool m_middleBoardOnGround = false;
    public bool m_rightBoardOnGround = false;
    public float m_startAngle = 0f;
    public bool isInAir {
        get { return !m_leftBoardOnGround && !m_middleBoardOnGround && !m_rightBoardOnGround; }
    }
    public bool m_groundedLastFrame = false;
    

    /// <summary>
    /// The frequency to check for continuous tricks like manuals or stoppies in times per second
    /// </summary>
    public float m_contTrickFrequency = 10f;
    private float m_contTrickTimer = 0f;

    /// <summary>
    /// The time it takes to start counting continuous tricks as tricks
    /// </summary>
    public float m_contTrickDelay = 0.5f;
    private float m_contTrickDelayTimer = 0f;

    
    [Header("Jumping")]
    public float m_jumpForce = 10f;
    public float m_jumpCooldown = 0.1f;
    private float m_jumpTimer = 0f;

    [Header("Assets")]
    public AudioSource m_snowSound;
    public float m_snowVol = 0.25f;
    public AudioClip m_deathSound;


    [Serializable]
    private class BodyTracker{
        public GameObject m_bodyPart;
        public Vector3 m_initialPosition;
        public Quaternion m_initialRotation;
    }
    private List<BodyTracker> m_bodyTrackers = new List<BodyTracker>();
    private Vector3 m_deathPosition;



    // Start is called before the first frame update
    void Start()
    {
        // track all children, to reset them when the player revives
        m_bodyTrackers = new List<BodyTracker>();
        foreach (Transform child in transform)
        {
            m_bodyTrackers.Add(new BodyTracker(){
                m_bodyPart = child.gameObject,
                m_initialPosition = child.localPosition,
                m_initialRotation = child.localRotation
            });
        }

        // reset time scale
        Time.timeScale = 1f;

        m_initialMaxSpeed = m_maxSpeed;
    }

    private void OnEnable() {
        m_headCollider.m_onCollisionEnter += OnHeadCollision;
        m_leftHandCollider.m_onCollisionEnter += OnLeftHandCollision;
        m_rightHandCollider.m_onCollisionEnter += OnRightHandCollision;
    }

    private void OnDisable() {
        m_headCollider.m_onCollisionEnter -= OnHeadCollision;
        m_leftHandCollider.m_onCollisionEnter -= OnLeftHandCollision;
        m_rightHandCollider.m_onCollisionEnter -= OnRightHandCollision;
    }

    private void OnHeadCollision(Collision2D other)
    {
        // if both left and right board are on the ground, return, as they are safe
        if (m_leftBoardOnGround && m_rightBoardOnGround) return;

        Die();
    }

    private void OnLeftHandCollision(Collision2D other)
    {
        if (m_isDead) return;

        if (m_leftBoardOnGround && m_rightBoardOnGround){
            return;
        }

        // Groundtouch!
        Debug.Log("Groundtouch!");

        // add to trick list
        m_pointsManager.AddTrick(PointsManager.TrickType.Groundtouch);
    }

    private void OnRightHandCollision(Collision2D other)
    {
        if (m_isDead) return;

        if (m_leftBoardOnGround && m_rightBoardOnGround){
            return;
        }

        // Groundtouch!
        Debug.Log("Groundtouch!");

        // add to trick list
        m_pointsManager.AddTrick(PointsManager.TrickType.Groundtouch);
    }

    private void Die()
    {
        if (m_invincible || m_isDead) return;

        m_pointsManager.StopCombo();

        m_isDead = true;

        m_deathPosition = m_boardRB.transform.position;

        // set time to slowmo
        Time.timeScale = 0.2f;

        // play death sound
        m_snowSound.Stop();
        GetComponent<AudioSource>().PlayOneShot(m_deathSound, 0.25f * PlayerPrefs.GetFloat("Volume", 1f));

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
        Camera.main.GetComponent<CameraFollow>().m_target = m_headCollider.transform;

        // DeathManager
        DeathManager dm = FindObjectOfType<DeathManager>();
        dm.PlayerDied();

        // save highscore and top speed
        string highscoreString = "";
        if (m_topSpeed > PlayerPrefs.GetFloat("TopSpeed", 0f))
        {
            PlayerPrefs.SetFloat("TopSpeed", m_topSpeed);
            highscoreString += "New Top Speed!";
        }
        if (m_pointsManager.GetTotalPoints() > PlayerPrefs.GetInt("Highscore", 0))
        {
            PlayerPrefs.SetInt("Highscore", m_pointsManager.GetTotalPoints());
            highscoreString += (highscoreString == "" ? "" : " ,") + "New Highscore!";
        }
        m_highscoreText.text = highscoreString;
    }

    public void Revive(){
        m_isDead = false;

        // reset time
        Time.timeScale = 1f;

        // reset max speed
        m_maxSpeed = m_maxSpeed / 2f;
        m_maxSpeed = Mathf.Max(m_maxSpeed, m_initialMaxSpeed);

        // move this to the current head position
        Vector3 currentPos = m_headCollider.transform.position;
        // raycast down to find the ground
        int layer = LayerMask.GetMask("Ground");
        RaycastHit2D hit = Physics2D.Raycast(currentPos, Vector2.down, 100f, layer);
        if (hit.collider != null){
            // move this to the ground
            transform.position = hit.point + Vector2.up * 1.5f;
        }
        else {
            // move this to the death position
            transform.position = currentPos;
        }

        // reset all body parts to their initial positions
        foreach (BodyTracker tracker in m_bodyTrackers)
        {
            tracker.m_bodyPart.transform.localPosition = tracker.m_initialPosition;
            tracker.m_bodyPart.transform.localRotation = tracker.m_initialRotation;
        }

        // enable hinge joints on all feet
        foreach (GameObject foot in m_feet)
        {
            foot.GetComponent<HingeJoint2D>().enabled = true;
        }

        // set all joint limits to -10,10
        HingeJoint2D[] joints = GetComponentsInChildren<HingeJoint2D>();
        foreach (HingeJoint2D joint in joints)
        {
            JointAngleLimits2D limits = joint.limits;
            limits.min = -10;
            limits.max = 10;
            joint.limits = limits;
        }

        Rigidbody2D[] rigidbodies = GetComponentsInChildren<Rigidbody2D>();
        foreach (Rigidbody2D rb in rigidbodies)
        {
            // set all rigidbodies to mass 0.2 (excluding the board)
            if (rb.gameObject != m_boardRB.gameObject)
            {
                rb.mass = 0.2f;
            }

            // remove all velocity and angular velocity
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // get main cam and set follow target to head
        Camera.main.GetComponent<CameraFollow>().m_target = m_boardRB.transform;
    }

    private void FixedUpdate()
    {
        GetInput();
        
        CalculateScore();

        CalculateSpeed();

        UpdateUI();

        m_groundedLastFrame = !isInAir;

        // update timers (Time.deltaTime surprisingly DOES work in FixedUpdate, thanks unity!)
        m_jumpTimer -= Time.deltaTime;
        m_contTrickTimer -= Time.deltaTime;
    }

    private void Update() {
        // if R is pressed, reset the player
        if (Input.GetKeyDown(KeyCode.R))
        {
            Revive();
        }
    }

    private void CalculateScore()
    {
        if (m_isDead) return;

        // add to max speed
        //m_maxSpeed += m_maxIncreaseRate * Time.fixedDeltaTime;

        // check left, middle, right touch:
        // spherecast from touch position to ground
        // if hit, set touching to true
        // if not hit, set touching to false

        RaycastHit2D leftHit = Physics2D.CircleCast(m_leftTouch.position, m_touchRadius, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
        RaycastHit2D middleHit = Physics2D.CircleCast(m_middleTouch.position, m_touchRadius, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
        RaycastHit2D rightHit = Physics2D.CircleCast(m_rightTouch.position, m_touchRadius, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));

        m_leftBoardOnGround = leftHit;
        m_middleBoardOnGround = middleHit;
        m_rightBoardOnGround = rightHit;

        if (m_groundedLastFrame && isInAir){
            // just jumped

            // get total rotation of board
            m_startAngle = m_boardRB.rotation;
        }

        // check for full flips midair
        if (isInAir){
            // is in air

            // get current angle
            float currentAngle = m_boardRB.rotation;

            // get angle difference
            float angleDiff = currentAngle - m_startAngle;

            // backflip
            if (angleDiff > 360){
                // backflip
                Debug.Log("Backflip!");

                // add to trick list
                m_pointsManager.AddTrick(PointsManager.TrickType.Backflip);

                m_startAngle = currentAngle;
            }

            // frontflip
            if (angleDiff < -360){
                // frontflip
                Debug.Log("Frontflip!");

                // add to trick list
                m_pointsManager.AddTrick(PointsManager.TrickType.Frontflip);

                m_startAngle = currentAngle;
            }
        }

        // check for flips upon landing
        if (!m_groundedLastFrame && !isInAir){
            // just landed

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

        if (m_leftBoardOnGround && !m_rightBoardOnGround && !m_middleBoardOnGround && m_contTrickTimer <= 0f){
            // cont delay timer
            m_contTrickDelayTimer += Time.fixedDeltaTime;
            if (m_contTrickDelayTimer >= m_contTrickDelay){
                // Manual
                Debug.Log("Manual!");

                // add to trick list
                m_pointsManager.AddTrick(PointsManager.TrickType.Manual);

                m_contTrickTimer = 1f / m_contTrickFrequency;
            }
        }

        if (m_rightBoardOnGround && !m_leftBoardOnGround && !m_middleBoardOnGround && m_contTrickTimer <= 0f){
            // cont delay timer
            m_contTrickDelayTimer += Time.fixedDeltaTime;
            if (m_contTrickDelayTimer >= m_contTrickDelay)
            {
                // Stoppie
                Debug.Log("Stoppie!");

                // add to trick list
                m_pointsManager.AddTrick(PointsManager.TrickType.Stoppie);

                // Slow down the board
                m_boardRB.velocity = new Vector2(Mathf.Lerp(m_boardRB.velocity.x, 0f, Time.deltaTime * 0.5f), m_boardRB.velocity.y);

                // reset timer
                m_contTrickTimer = 1f / m_contTrickFrequency;
            }
        }

        // if both left and right are on ground, stop combo
        if (m_leftBoardOnGround && m_rightBoardOnGround){
            m_pointsManager.StopCombo();
            m_contTrickDelayTimer = 0f;
        }
    }

    private void GetInput()
    {
        if (m_isDead) return;

        bool swapControls = PlayerPrefs.GetInt("SwapControls", 0) == 1;

        bool inputLeft = Input.GetKey(KeyCode.A) || (Input.touchCount > 0 && Input.GetTouch(0).position.x < Screen.width / 2);
        bool inputRight = Input.GetKey(KeyCode.D) || (Input.touchCount > 0 && Input.GetTouch(0).position.x > Screen.width / 2);

        // update left and right text
        m_leftText.color = inputLeft ? Color.green : Color.white;
        m_rightText.color = inputRight ? Color.green : Color.white;

        if (swapControls)
        {
            bool temp = inputLeft;
            inputLeft = inputRight;
            inputRight = temp;
        }

        // if A is down OR if left side of screen is touched, rotate left
        m_leftHeld = m_leftPressed;
        m_leftPressed = inputLeft;
        if (m_leftHeld && m_leftPressed) m_leftHeld = true;
        else m_leftHeld = false;

        // if D is down OR if right side of screen is touched, rotate right
        m_rightHeld = m_rightPressed;
        m_rightPressed = inputRight;
        if (m_rightHeld && m_rightPressed) m_rightHeld = true;
        else m_rightHeld = false;

        // if ONLY left is pressed, rotate left
        if (m_leftPressed && !m_rightPressed && !(m_leftBoardOnGround && m_rightBoardOnGround))
        {
            RotateLeft();
        }
        // if ONLY right is pressed, rotate right
        else if (m_rightPressed && !m_leftPressed && !(m_leftBoardOnGround && m_rightBoardOnGround))
        {
            RotateRight();
        }
        // if both pressed and NOT held, jump
        else if (m_leftBoardOnGround && m_rightBoardOnGround && ((m_leftPressed && !m_leftHeld) || (m_rightPressed && !m_rightHeld)) /* && (!m_leftHeld || !m_rightHeld) */)
        {
            TryJump();
        }

        // held checks
    }

    private void CalculateSpeed()
    {
        if (m_isDead) return;

        // limit x velocity to max speed
        if (m_boardRB.velocity.x > m_maxSpeed)
        {
            m_boardRB.velocity = new Vector2(m_maxSpeed, m_boardRB.velocity.y);
        }

        // if moving backwards, start boosting
        if (!m_isBoosting && m_boardRB.velocity.x < 0.0f)
        {
            m_isBoosting = true;
            m_boostStartTime = Time.time;
        }

        // if boosting, move forwards slowly
        if (m_isBoosting)
        {
            // min velocity is 4.5f (lerp from 0-4.5f over 1 second)
            m_boardRB.velocity = new Vector2(Mathf.Max(m_boardRB.velocity.x, Mathf.Lerp(0f,4.5f, Time.time - m_boostStartTime)), m_boardRB.velocity.y);

            // if x velocity is greater than 5, stop boosting
            if (m_boardRB.velocity.x > 5)
            {
                m_isBoosting = false;
            }
        }

        m_topSpeed = Mathf.Max(m_topSpeed, m_boardRB.velocity.x);

        if (isInAir){
            // lerp AudioSource volume to 0
            m_snowSound.volume = Mathf.Lerp(m_snowSound.volume, 0f, Time.deltaTime * 10.0f);
        }
        else{
            // play m_snowSound if not playing
            if (!m_snowSound.isPlaying){
                m_snowSound.Play();
            }

            // for each part of board not on ground, reduce volume
            float tempVol = m_snowVol;
            if (!m_leftBoardOnGround) tempVol *= 0.5f;
            if (!m_middleBoardOnGround) tempVol *= 0.5f;

            // scale tempVol by PlayerPerf volume
            tempVol *= PlayerPrefs.GetFloat("Volume", 1f);

            // lerp AudioSource volume to 0.25
            m_snowSound.volume = Mathf.Lerp(m_snowSound.volume, tempVol, Time.deltaTime * 10.0f);
        }

        float particleSpeed = m_boardRB.velocity.x / 5.0f;
        particleSpeed = Mathf.Max(particleSpeed, 1.0f);

        // left particles
        if (m_leftBoardOnGround){
            // play the particle system
            m_leftTouch.GetComponent<ParticleSystem>().Play();

            // set the speed of the particle system
            var main = m_leftTouch.GetComponent<ParticleSystem>().main;
            main.startSpeed = new ParticleSystem.MinMaxCurve(main.startSpeed.constantMin, particleSpeed);
        }
        else{
            // stop the particle system
            m_leftTouch.GetComponent<ParticleSystem>().Stop();
        }

        // right particles
        if (m_rightBoardOnGround){
            // play the particle system
            m_rightTouch.GetComponent<ParticleSystem>().Play();

            // set the speed of the particle system
            var main = m_rightTouch.GetComponent<ParticleSystem>().main;
            main.startSpeed = new ParticleSystem.MinMaxCurve(main.startSpeed.constantMin, particleSpeed);
        }
        else{
            // stop the particle system
            m_rightTouch.GetComponent<ParticleSystem>().Stop();
        }

        // achievement check
        if (m_topSpeed >= 30.0f){
            Social.ReportProgress(SnowRunnerAchievements.achievement_30kmh, 100.0f, (bool success) => {
                // handle success or failure
            });
        }
        if (m_topSpeed >= 40.0f){
            Social.ReportProgress(SnowRunnerAchievements.achievement_40kmh, 100.0f, (bool success) => {
                // handle success or failure
            });
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

    private void TryJump()
    {
        if (m_jumpTimer > 0.0f) return;
        
        // add force to board
        m_boardRB.AddForce(Vector2.up * m_jumpForce, ForceMode2D.Impulse);

        // reset timer
        m_jumpTimer = m_jumpCooldown;
    }
}
