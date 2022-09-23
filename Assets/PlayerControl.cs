using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player controller that works with touch controls
/// </summary>
public class PlayerControl : MonoBehaviour
{
    public Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        
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
        rb.angularVelocity = 200;
    }

    public void RotateRight(){
        rb.angularVelocity = -200;
    }
}
