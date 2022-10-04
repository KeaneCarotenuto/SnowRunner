using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferencePlayer : MonoBehaviour
{
    public BodyCollider m_headCollider;
    public Rigidbody2D m_boardRigidbody;

    // Start is called before the first frame update
    void Start()
    {
        // move to root in hierarchy
        //transform.SetParent(null);
    }

    private void OnEnable() {
        // bind the head collider
        m_headCollider.m_onCollisionEnter += OnHeadCollision;
    }

    private void OnDisable() {
        // unbind the head collider
        m_headCollider.m_onCollisionEnter -= OnHeadCollision;
    }

    private void OnHeadCollision(Collision2D other)
    {
        // throw the board upwards
        m_boardRigidbody.AddForce(Vector2.up * 1000.0f);

        // throw the board right
        m_boardRigidbody.AddForce(Vector2.right * 200.0f);

        // apply a random torque
        m_boardRigidbody.AddTorque(Random.Range(0, 100.0f));
    }

    // Update is called once per frame
    void Update()
    {
        // if the board is moving left, throw it right
        if (m_boardRigidbody.velocity.x < 0) {
            m_boardRigidbody.AddForce(Vector2.right * 500.0f);
        }

        // try to keep the board upright
        if (m_boardRigidbody.rotation > 45) {
            m_boardRigidbody.AddTorque(-1.0f);
        } else if (m_boardRigidbody.rotation < -45) {
            m_boardRigidbody.AddTorque(1.0f);
        }

        // if Y velocity is too high, destroy this
        if (Mathf.Abs(m_boardRigidbody.velocity.y) > 100.0f) {
            Destroy(gameObject);
        }
    }
}
