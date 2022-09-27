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

    public Color m_tricksColorDoing;
    public Color m_tricksColorDone;

    // Start is called before the first frame update
    void Start()
    {
        // set text to bold
        // m_tricksText.fontStyle = FontStyles.Bold;
        // m_pointsText.fontStyle = FontStyles.Bold;

        // set colour to DOING
        m_tricksText.color = m_tricksColorDoing;
        m_pointsText.color = m_tricksColorDoing;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_doFade){
            // set text to normal
            // m_tricksText.fontStyle = FontStyles.Normal;
            // m_pointsText.fontStyle = FontStyles.Normal;

            // set colour to DONE
            m_tricksText.color = Color.Lerp(m_tricksColorDoing, m_tricksColorDone, m_fadeDelayTimer / m_fadeDelay*2.0f);
            m_pointsText.color = Color.Lerp(m_tricksColorDoing, m_tricksColorDone, m_fadeDelayTimer / m_fadeDelay*2.0f);

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
