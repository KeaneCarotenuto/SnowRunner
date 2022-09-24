using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class TerrainManager : MonoBehaviour
{
    public GameObject board;

    public GameObject m_currentTerrain;

    public List<GameObject> m_possibleTerrain;

    public List<GameObject> m_oldTerrain;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // if end point of terrain is in view of camera, spawn new terrain:
        Vector3 screenRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0));
        Vector3 screenLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));

        if (m_currentTerrain.GetComponent<Terrain>().m_endPoint.transform.position.x < screenRight.x + 10){
            SpawnTerrain();
        }

        // delete and remove old terrain if end point is off screen
        if (m_oldTerrain.Count > 0 && m_oldTerrain[0].GetComponent<Terrain>().m_endPoint.transform.position.x < screenLeft.x - 10){
            Destroy(m_oldTerrain[0]);
            m_oldTerrain.RemoveAt(0);
        }
    }

    private void SpawnTerrain()
    {
        // get a random terrain from the list of possible terrains
        GameObject newTerrainPrefab = m_possibleTerrain[UnityEngine.Random.Range(0, m_possibleTerrain.Count)];

        // instantiate the terrain
        GameObject newTerrain = Instantiate(newTerrainPrefab, Vector3.zero, Quaternion.identity);

        // set the pos of new terrain so start point is at end point of current terrain
        Vector3 newPosToStart = newTerrain.GetComponent<Terrain>().m_startPoint.transform.position - newTerrain.transform.position;
        newTerrain.transform.position = m_currentTerrain.GetComponent<Terrain>().m_endPoint.transform.position - newPosToStart;

        // set the current terrain to the new terrain
        m_oldTerrain.Add(m_currentTerrain);
        m_currentTerrain = newTerrain;
    }
}
