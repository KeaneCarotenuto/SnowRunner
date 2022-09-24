using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadCollider : MonoBehaviour
{
    public System.Action<Collision2D> m_onCollisionEnter;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (m_onCollisionEnter != null){
            m_onCollisionEnter?.Invoke(other);
        }
    }
}
