using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TerrainGenerator : MonoBehaviour
{
    public SpriteShapeController m_spriteShapeController;
    public EdgeCollider2D m_edgeCollider;

    [Header("Params")]
    public int m_numPoints = 150;
    public float m_distanceBetweenPoints = 0.5f;
    public float m_gradient = 0.25f;
    public float m_scale = 40f;
    public float m_noiseScale = 0.1f;

    [Header("Seed")]
    public int m_seed = 0;
    public bool m_randomSeed = true;

    // Start is called before the first frame update
    void Start()
    {
    }

    /// <summary>
    /// Uses perlin noise to generate a terrain with SpriteShapeController
    /// </summary>
    public void GenerateTerrain(){
        m_spriteShapeController = GetComponent<SpriteShapeController>();
        m_edgeCollider = GetComponent<EdgeCollider2D>();

        // randomize the seed for perlin noise
        if (m_randomSeed) m_seed = Random.Range(0,10000);

        // clear the sprite shape controller
        m_spriteShapeController.spline.Clear();

        // create a list of points
        List<Vector3> points = new List<Vector3>();

        // add the rest of the points
        for (int i = 0; i < m_numPoints; i++){

            // get the next point
            Vector3 nextPoint =  (i > 0 ? points[i - 1] : Vector3.zero) + new Vector3(i > 0 ? m_distanceBetweenPoints : 0, 0, 0);

            // get the perlin noise value
            float perlinNoise = Mathf.PerlinNoise(i * m_noiseScale + m_seed, 0 + m_seed);

            // set the y value of the next point
            nextPoint.y = perlinNoise * m_scale;

            // apply the gradient
            nextPoint.y += i * m_distanceBetweenPoints * m_gradient;

            // add the next point to the list
            points.Add(nextPoint);
        }

        // add edge points (far right and far left below)
        points.Add(new Vector3(points[points.Count - 1].x, points[points.Count - 1].y - 100.0f, 0));
        points.Add(new Vector3(points[0].x, points[0].y - 100.0f, 0));

        // choose one random point to increase by 10
        int randomPoint = Random.Range(0, points.Count);
        points[randomPoint] += new Vector3(0, 10, 0);

        // add the points to the sprite shape controller
        m_spriteShapeController.spline.Clear();
        foreach (Vector3 point in points){
            m_spriteShapeController.spline.InsertPointAt(m_spriteShapeController.spline.GetPointCount(), point);
        }

        // set tanget mode to continuous for all points
        for (int i = 0; i < m_spriteShapeController.spline.GetPointCount(); i++){
            m_spriteShapeController.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        
            // get direction between prev and next point (if they exist)
            Vector3 direction = Vector3.zero;
            if (i > 0 && i < m_spriteShapeController.spline.GetPointCount() - 1){
                direction = (m_spriteShapeController.spline.GetPosition(i + 1) - m_spriteShapeController.spline.GetPosition(i - 1)).normalized;
            }

            direction = direction * m_distanceBetweenPoints / 2.5f;

            m_spriteShapeController.spline.SetLeftTangent(i, -direction);
            m_spriteShapeController.spline.SetRightTangent(i, direction);
        }

        //update the edge collider (convert to vector2)
        List<Vector2> edgePoints = new List<Vector2>();
        foreach (Vector3 point in points){
            edgePoints.Add(new Vector2(point.x, point.y));
        }
        m_edgeCollider.points = edgePoints.ToArray();

        // set start and end points of Terrain
        GetComponent<Terrain>().m_startPoint.transform.position = points[0];
        GetComponent<Terrain>().m_endPoint.transform.position = points[points.Count - 3];
    }

    // custom editor
    #if UNITY_EDITOR
    [CustomEditor(typeof(TerrainGenerator))]
    public class TerrainGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TerrainGenerator terrainGenerator = (TerrainGenerator)target;
            if (GUILayout.Button("Generate Terrain")){
                terrainGenerator.GenerateTerrain();
            }
        }
    }
    #endif
}
