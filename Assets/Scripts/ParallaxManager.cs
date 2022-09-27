using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxManager : MonoBehaviour
{
    public GameObject m_camera;

    [System.Serializable]
    public class ParallaxLayer
    {
        public GameObject m_layer;
        public GameObject m_layerLEFT;
        public GameObject m_layerRIGHT;
        public float m_speed;
    }

    [SerializeField] public List<ParallaxLayer> m_layers = new List<ParallaxLayer>();

    // Start is called before the first frame update
    void Start()
    {
        // duplicate layers LEFT and RIGHT
        foreach (ParallaxLayer layer in m_layers)
        {
            layer.m_layerLEFT = Instantiate(layer.m_layer, layer.m_layer.transform.position, layer.m_layer.transform.rotation, layer.m_layer.transform.parent);
            layer.m_layerRIGHT = Instantiate(layer.m_layer, layer.m_layer.transform.position, layer.m_layer.transform.rotation, layer.m_layer.transform.parent);
        }

        // set layers LEFT and RIGHT
        foreach (ParallaxLayer layer in m_layers)
        {
            layer.m_layerLEFT.transform.position = new Vector3(layer.m_layerLEFT.transform.position.x - layer.m_layerLEFT.GetComponent<SpriteRenderer>().bounds.size.x, layer.m_layerLEFT.transform.position.y, layer.m_layerLEFT.transform.position.z);
            layer.m_layerRIGHT.transform.position = new Vector3(layer.m_layerRIGHT.transform.position.x + layer.m_layerRIGHT.GetComponent<SpriteRenderer>().bounds.size.x, layer.m_layerRIGHT.transform.position.y, layer.m_layerRIGHT.transform.position.z);
        }
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        foreach (ParallaxLayer layer in m_layers)
        {
            // move layers
            layer.m_layer.transform.position = new Vector3(m_camera.transform.position.x * 1.0f/layer.m_speed, m_camera.transform.position.y, layer.m_layer.transform.position.z);
            layer.m_layerLEFT.transform.position = new Vector3(m_camera.transform.position.x * 1.0f/layer.m_speed - layer.m_layerLEFT.GetComponent<SpriteRenderer>().bounds.size.x, m_camera.transform.position.y, layer.m_layerLEFT.transform.position.z);
            layer.m_layerRIGHT.transform.position = new Vector3(m_camera.transform.position.x * 1.0f/layer.m_speed + layer.m_layerRIGHT.GetComponent<SpriteRenderer>().bounds.size.x, m_camera.transform.position.y, layer.m_layerRIGHT.transform.position.z);

            List<GameObject> layers = new List<GameObject>();
            layers.Add(layer.m_layer);
            layers.Add(layer.m_layerLEFT);
            layers.Add(layer.m_layerRIGHT);

            // if the right side of a layer is left of the camera's view, move it to the right-most side (check all layers)
            foreach (GameObject l in layers)
            {
                if (l.transform.position.x + l.GetComponent<SpriteRenderer>().bounds.size.x / 2 < m_camera.transform.position.x - m_camera.GetComponent<Camera>().orthographicSize * m_camera.GetComponent<Camera>().aspect)
                {
                    // get the right-most layer
                    GameObject rightMostLayer = l;
                    foreach (GameObject l2 in layers)
                    {
                        if (l2.transform.position.x > rightMostLayer.transform.position.x)
                        {
                            rightMostLayer = l2;
                        }
                    }

                    // move the layer to the right-most side
                    l.transform.position = new Vector3(rightMostLayer.transform.position.x + rightMostLayer.GetComponent<SpriteRenderer>().bounds.size.x, l.transform.position.y, l.transform.position.z);
                }
            }
        }
    }
}
