using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PointsManager : MonoBehaviour
{
    public enum TrickType{
        Backflip,
        Frontflip,
        Stoppie
    }

    [System.Serializable]
    public class Trick{
        public TrickType m_type;
        public int m_timesDone;

        public string ToText(){
            return m_timesDone + "x\u00A0" + m_type.ToString() + " " + GetPoints(this) + "pts\n";
        }
    }

    public TextMeshProUGUI m_pointsText;
    public TextMeshProUGUI m_trickText;

    public List<Trick> m_trickPerformed = new List<Trick>();


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // update UI
        m_pointsText.text = "Total: " + GetTotalPoints() + "pts";

        // update trick text
        string trickText = "";
        for (int i = m_trickPerformed.Count - 1; i >= 0; i--){
            trickText += m_trickPerformed[i].ToText();
        }
        m_trickText.text = trickText;
    }

    public void AddTrick(TrickType _type, int _timesDone = 1)
    {
        // add the trick to the list, if most recent trick is the same, increment the times done
        if (m_trickPerformed.Count > 0 && m_trickPerformed[m_trickPerformed.Count - 1].m_type == _type){
            m_trickPerformed[m_trickPerformed.Count - 1].m_timesDone += _timesDone;
        } else {
            m_trickPerformed.Add(new Trick()
            {
                m_type = _type,
                m_timesDone = _timesDone
            });
        }
    }

    static public int GetPoints(Trick _trick)
    {
        switch (_trick.m_type)
        {
            case TrickType.Backflip:
                return _trick.m_timesDone * 10;
            case TrickType.Frontflip:   
                return _trick.m_timesDone * 10;
            case TrickType.Stoppie: 
                return _trick.m_timesDone * 1;
            default:
                return 0;
        }
    }

    private int GetTotalPoints()
    {
        int points = 0;
        foreach (Trick trick in m_trickPerformed)
        {
            points += GetPoints(trick);
        }
        return points;
    }
}
