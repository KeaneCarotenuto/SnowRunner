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
        Manual,
        Stoppie
    }

    [System.Serializable]
    public class Trick{
        public TrickType m_type;
        public int m_timesDone;

        public string ToText(){
            return m_timesDone + "x\u00A0" + m_type.ToString();
        }
    }

    public TextMeshProUGUI m_pointsText;
    public TextMeshProUGUI m_trickText;

    public List<Trick> m_currentCombo = new List<Trick>();
    public List<List<Trick>> m_oldCombos = new List<List<Trick>>();

    public Transform m_trickFeed;
    public GameObject ComboUIPrefab;
    public GameObject m_currentComboUI;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // update UI
        m_pointsText.text = GetTotalPoints() + "pts";

        // update current combo UI
        UpdateCurrentComboUI();
    }

    private void UpdateCurrentComboUI()
    {
        if (m_currentComboUI != null)
        {
            m_currentComboUI.GetComponent<ComboUI>().m_tricksText.text = GetComboText(m_currentCombo);
            m_currentComboUI.GetComponent<ComboUI>().m_pointsText.text = GetComboPoints(m_currentCombo) + "pts";
        }
    }

    private string GetComboText(List<Trick> m_currentCombo)
    {
        string trickText = "";
        for (int i = m_currentCombo.Count - 1; i >= 0; i--){
            trickText += m_currentCombo[i].ToText() + " ";
        }
        return trickText;
    }

    public void AddTrick(TrickType _type, int _timesDone = 1)
    {
        // if no current comboUI
        if (m_currentComboUI == null){
            // create a new combo UI
            m_currentComboUI = Instantiate(ComboUIPrefab, m_trickFeed);
        }

        // add the trick to the list, if most recent trick is the same, increment the times done
        if (m_currentCombo.Count > 0 && m_currentCombo[m_currentCombo.Count - 1].m_type == _type){
            m_currentCombo[m_currentCombo.Count - 1].m_timesDone += _timesDone;
        } else {
            m_currentCombo.Add(new Trick()
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
            case TrickType.Manual:
                return _trick.m_timesDone * 1;
            case TrickType.Stoppie: 
                return _trick.m_timesDone * 2;
            default:
                return 0;
        }
    }

    private int GetComboPoints(List<Trick> _combo)
    {
        int totalPoints = 0;
        if (_combo == null){
            _combo = m_currentCombo;
        }
        for (int i = 0; i < _combo.Count; i++){
            totalPoints += GetPoints(_combo[i]);
        }
        return totalPoints;
    }

    private int GetTotalPoints()
    {
        int totalPoints = 0;
        for (int i = 0; i < m_oldCombos.Count; i++){
            totalPoints += GetComboPoints(m_oldCombos[i]);
        }
        return totalPoints;
    }

    public void StopCombo()
    {
        if (m_currentComboUI == null || m_currentCombo.Count == 0){
            return;
        }

        UpdateCurrentComboUI();

        m_currentComboUI.GetComponent<ComboUI>().m_doFade = true;
        m_currentComboUI = null;

        // add the current combo to the list of old combos
        m_oldCombos.Add(new List<Trick>(m_currentCombo));
        int points = GetComboPoints(m_currentCombo);
        m_currentCombo.Clear();

        // add maxSpeed based on points
        float addition = points / 10f;
        PlayerControl pc = FindObjectOfType<PlayerControl>();
        pc.m_maxSpeed += addition;
    }
}
