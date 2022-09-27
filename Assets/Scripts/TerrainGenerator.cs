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
    public int m_dropCount = 10;
    public float m_dropHeight = 10.0f;

    [Header("Seed")]
    public float m_seed = 0;
    public List<float> m_seeds = new List<float>();
    public bool m_randomSeed = true;

    
    [System.Serializable]
    public class Spawnable{
        public GameObject m_prefab;
        public float m_minDistance = 0.0f;
        public float m_maxDistance = 10.0f;
        public float m_distance = 0.0f;
        public float m_lastPosition = 0.0f;
    }

    [Header("Prefabs")]
    public List<Spawnable> m_spawnables = new List<Spawnable>();
    

    private void Update() {
        // get the top right corner of the screen in world space
        Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));

        // add move 20 units to the right
        topRight.x += 20;

        foreach (Spawnable spawn in m_spawnables){
            // if m_lastTreePosition is 0, set it to the top right
            if (spawn.m_lastPosition == 0) {
                spawn.m_lastPosition = topRight.x;
            }

            // if greater than tree distance, spawn a tree
            if (spawn.m_lastPosition + spawn.m_distance < topRight.x) {
                // raycast down to find the ground, then spawn a tree aligned to the normal
                int mask = LayerMask.GetMask("Ground");
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(spawn.m_lastPosition + spawn.m_distance, topRight.y), Vector2.down, 1000.0f, mask);
                if (hit.collider != null && hit.collider == m_edgeCollider) {
                    GameObject tree = Instantiate(spawn.m_prefab, hit.point, Quaternion.identity);
                    tree.transform.up = hit.normal;

                    // set the parent to this
                    tree.transform.parent = transform;

                    // set the last tree position
                    spawn.m_lastPosition = spawn.m_lastPosition + spawn.m_distance;

                    // set the next tree distance
                    spawn.m_distance = Random.Range(spawn.m_minDistance, spawn.m_maxDistance);
                }
            }
        }
    }

    /// <summary>
    /// Uses perlin noise to generate a terrain with SpriteShapeController
    /// </summary>
    public void GenerateTerrain(){
        m_spriteShapeController = GetComponent<SpriteShapeController>();
        m_edgeCollider = GetComponent<EdgeCollider2D>();

        // randomize the seed for perlin noise
        if (m_randomSeed) NewSeed();

        // clear the sprite shape controller
        m_spriteShapeController.spline.Clear();

        // create a list of points
        List<Vector3> points = new List<Vector3>();

        // random drop locations
        int dropsDone = 0;
        // choose 10 random points to drop
        List<int> dropPoints = new List<int>();
        while (dropsDone < m_dropCount){
            int dropPoint = Random.Range(0, m_numPoints);
            if (!dropPoints.Contains(dropPoint)){
                dropPoints.Add(dropPoint);
                dropsDone++;
            }
        }

        // add the rest of the points
        for (int i = 0; i < m_numPoints; i++){

            // get the next point
            Vector3 nextPoint =  (i > 0 ? points[i - 1] : Vector3.zero) + new Vector3(i > 0 ? m_distanceBetweenPoints : 0, 0, 0);

            // get the perlin noise value
            float perlinNoise = Mathf.PerlinNoise((float)i * m_noiseScale + (float)m_seed, 0.0f + (float)m_seed);

            // set the y value of the next point
            nextPoint.y = perlinNoise * m_scale;

            // apply the gradient
            nextPoint.y += i * (float)m_distanceBetweenPoints * m_gradient;

            // drop the point if it comes after a drop point
            int dropTimes = dropPoints.FindAll(x => i >= x).Count;
            nextPoint.y -= dropTimes * m_dropHeight;

            // if this point is RIGHT BEFORE the drop point, increase its Y value
            if (dropPoints.Contains(i + 1)){
                nextPoint.y += 2;
            }

            // add the next point to the list
            points.Add(nextPoint);
        }

        // add edge points (far right and far left below)
        points.Add(new Vector3(points[points.Count - 1].x, points[points.Count - 1].y - 100.0f, 0));
        points.Add(new Vector3(points[0].x, points[0].y - 100.0f, 0));

        // choose one random point to increase by 10
        // int randomPoint = Random.Range(0, points.Count);
        // points[randomPoint] += new Vector3(0, 10, 0);

        // add the points to the sprite shape controller
        m_spriteShapeController.spline.Clear();
        foreach (Vector3 point in points){
            m_spriteShapeController.spline.InsertPointAt(m_spriteShapeController.spline.GetPointCount(), point);
        }

        // set tanget mode to continuous for all points
        for (int i = 0; i < m_spriteShapeController.spline.GetPointCount(); i++){
            m_spriteShapeController.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        
            // get direction between prev and next point (if they exist)
            Vector3 direction = Vector3.one;
            if (i > 0 && i < m_spriteShapeController.spline.GetPointCount() - 1){
                direction = (m_spriteShapeController.spline.GetPosition(i + 1) - m_spriteShapeController.spline.GetPosition(i - 1)).normalized;
            }

            direction = direction * m_distanceBetweenPoints / 2.5f;
            // set z to 1
            direction = new Vector3(direction.x, direction.y, 1);

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

    private void NewSeed()
    {
        m_seed = Random.Range(0.0f, 10000.0f);
        m_seeds.Add(m_seed);

        // only keep the last 30 seeds
        if (m_seeds.Count > 30) m_seeds.RemoveAt(0);
    }

    private bool forceGenerateOnce = true;

    void OnGUI()
    {

        var sc = GetComponent<SpriteShapeController>();

        var sr = GetComponent<SpriteShapeRenderer>();

        if (sr != null)

        {

            if (!sr.isVisible && forceGenerateOnce)

            {

                sc.BakeMesh();

                UnityEngine.Rendering.CommandBuffer rc = new UnityEngine.Rendering.CommandBuffer();

                var rt = RenderTexture.GetTemporary(256, 256, 0, RenderTextureFormat.ARGB32);

                Graphics.SetRenderTarget(rt);

                rc.DrawRenderer(sr, sr.sharedMaterial);

                Graphics.ExecuteCommandBuffer(rc);

                Debug.Log("SpriteShape Generated");

                forceGenerateOnce = false;

            }

        }

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
