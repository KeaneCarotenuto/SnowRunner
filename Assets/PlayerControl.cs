using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player controller that works with touch controls
/// </summary>
public class PlayerControl : MonoBehaviour
{
    public Rigidbody2D m_boardRB;

    public HeadCollider m_head;

    public List<GameObject> m_feet;

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
        // disable hinge joints on all feet
        foreach (GameObject foot in m_feet){
            foot.GetComponent<HingeJoint2D>().enabled = false;
        }

        // set all joint limits to -75,75
        HingeJoint2D[] joints = transform.parent.GetComponentsInChildren<HingeJoint2D>();
        foreach (HingeJoint2D joint in joints){
            JointAngleLimits2D limits = joint.limits;
            limits.min = -75;
            limits.max = 75;
            joint.limits = limits;
        }

        // set all rigidbodies to mass 2 (excluding the board)
        Rigidbody2D[] rigidbodies = transform.parent.GetComponentsInChildren<Rigidbody2D>();
        foreach (Rigidbody2D rb in rigidbodies){
            if (rb.gameObject != m_boardRB.gameObject){
                rb.mass = 2;
            }
        }

        // get main cam and set follor target to head
        Camera.main.GetComponent<CameraFollow>().m_target = m_head.transform;
    }

    private void FixedUpdate() {
        // if A is down, rotate left
        if (Input.GetKey(KeyCode.A)){
            RotateLeft();
        }

        // if D is down, rotate right
        if (Input.GetKey(KeyCode.D)){
            RotateRight();
        }

        // if left side of screen is touched, rotate left
        if (Input.touchCount > 0 && Input.GetTouch(0).position.x < Screen.width / 2){
            RotateLeft();
        }

        // if right side of screen is touched, rotate right
        if (Input.touchCount > 0 && Input.GetTouch(0).position.x > Screen.width / 2){
            RotateRight();
        }

    }

    public void RotateLeft(){
        m_boardRB.angularVelocity = 200;
    }

    public void RotateRight(){
        m_boardRB.angularVelocity = -200;
    }
}
