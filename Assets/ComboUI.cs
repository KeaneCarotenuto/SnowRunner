using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ComboUI : MonoBehaviour
{
    public TextMeshProUGUI m_tricksText;
    public TextMeshProUGUI m_pointsText;

    public bool m_doFade = false;
    public float m_fadeTime = 1.0f;
    private float m_fadeTimer = 0.0f;
    public float m_fadeDelay = 1.0f;
    private float m_fadeDelayTimer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (m_doFade){
            m_fadeDelayTimer += Time.deltaTime;
            if (m_fadeDelayTimer > m_fadeDelay){
                m_fadeTimer += Time.deltaTime;
                if (m_fadeTimer > m_fadeTime){
                    Destroy(gameObject);
                }
                else{
                    float alpha = Mathf.Lerp(1.0f, 0.0f, m_fadeTimer / m_fadeTime);
                    m_tricksText.alpha = alpha;
                    m_pointsText.alpha = alpha;
                }
            }
        }
    }
}
