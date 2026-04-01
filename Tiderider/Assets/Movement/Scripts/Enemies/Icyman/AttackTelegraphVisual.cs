using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AttackTelegraphVisual : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;
    private Material materialInstance;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        mesh = new Mesh
        {
            name = "IcymanSwipeTelegraph"
        };
        meshFilter.sharedMesh = mesh;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            materialInstance = new Material(shader);
            meshRenderer.sharedMaterial = materialInstance;
        }

        Hide();
    }

    public void SetSorting(int sortingLayerId, int sortingOrder)
    {
        meshRenderer.sortingLayerID = sortingLayerId;
        meshRenderer.sortingOrder = sortingOrder;
    }

    public void SetColor(Color color)
    {
        if (materialInstance != null)
        {
            materialInstance.color = color;
        }
    }

    public void Show(Vector2 direction, float radius, float angle)
    {
        BuildSector(radius, angle, 24);

        float zAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.localRotation = Quaternion.Euler(0f, 0f, zAngle);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void BuildSector(float radius, float angle, int segments)
    {
        if (mesh == null) return;

        int vertexCount = segments + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;

        float halfAngle = angle * 0.5f;
        float step = angle / segments;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = -halfAngle + (step * i);
            float radians = currentAngle * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * radius;
        }

        for (int i = 0; i < segments; i++)
        {
            int tri = i * 3;
            triangles[tri] = 0;
            triangles[tri + 1] = i + 1;
            triangles[tri + 2] = i + 2;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    private void OnDestroy()
    {
        if (mesh != null)
        {
            Destroy(mesh);
        }

        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
}
