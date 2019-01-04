using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Скрипт генерации многоугольников/астероидов с использованием мешей, триангуляции и коллайдеров
 * На выходе получается многоугольник с заданным колличеством вершин, частично округлой формы.
 * Дальнейшей растяжкой, размещением и приданием ускорения объекту занимается генератор
 */

// Автоматическое подкоючение нужных компонентов для астероида
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class ShapeGenerete : MonoBehaviour {

    [Tooltip("Материал")]
    [SerializeField]
    private Material material_;
    public Color MatherialColor { get; set; }
    private bool isDestroy = false;
    // Вызов непосредственно всех функций
    private void Start()
    {
        MeshGenerate(10);
        PolygonGenerate();
    }

    private Vector3 scale = new Vector3(1, 1, 1);
    public Vector3 Scale { get { return scale; } set { scale = value; } }

    private List<Vector3> VertCorvert(List<Vector3> vertices)
    {
        var new_vertices = new List<Vector3>();
        for (int i = 0; i < vertices.Count; i++)
            new_vertices.Add(new Vector3(vertices[i].x * Scale.x, vertices[i].y * Scale.y));
        return new_vertices;
    }

    public void SetVertices(List<Vector3> vertices)
    {
        var new_vertices = VertCorvert(vertices);
        var triangles_list = Triangulate(new_vertices);
        var mesh = new Mesh
        {
            vertices = new_vertices.ToArray(),
            triangles = triangles_list.ToArray()
        };
        GetComponent<MeshFilter>().mesh = mesh;
        isDestroy = true;
    }

    // Генерация меша объекта по колличесту переданных вершин
    private void MeshGenerate(int vertices)
    {
        if (!isDestroy)
        {
            var vertices_list = VerticesGenerate(vertices);
            vertices_list = VertCorvert(vertices_list);
            var triangles_list = Triangulate(vertices_list);
            var mesh = new Mesh
            {
                vertices = vertices_list.ToArray(),
                triangles = triangles_list.ToArray()
            };
            GetComponent<MeshFilter>().mesh = mesh;
        }
        GetComponent<MeshRenderer>().material = material_;
        GetComponent<MeshRenderer>().material.color = MatherialColor;
    }


    // Растягивание коллайдера по мешу
    private void PolygonGenerate()
    {
        var vertices = GetComponent<MeshFilter>().mesh.vertices;
        var polygon = GetComponent<PolygonCollider2D>();
        List<Vector2> points = new List<Vector2>();
        foreach (var vertex in vertices)
            points.Add(new Vector2(vertex.x, vertex.y));
        polygon.points = points.ToArray();
    }

    // Рандомная генерация вершин по окружности и радиус вектору
    private List<Vector3> VerticesGenerate(int vertices)
    {
        List<Vector3> vertices_list = new List<Vector3>();
        var range = 2 * Mathf.PI / vertices;
        float iter = 0;
        for (var i = 0; i < vertices; i++)
        {           
            var radian_angle = Random.Range(iter, iter + range);
            var x = Mathf.Cos(radian_angle);
            var y = Mathf.Sin(radian_angle);
            var isMax = Random.Range(0, 2);
            if (isMax == 0)
            {
                var scale_down = Random.Range(0.5f, 1);
                x *= scale_down;
                y *= scale_down;
            }
            vertices_list.Add(new Vector3(x, y));
            iter += range;
        }
        vertices_list.Reverse();
        return vertices_list;
    }

    // Триангуляция и ее подфункции
    public List<int> Triangulate(List<Vector3> vertices)
    {
        var taken = new bool[vertices.Count];
        List<int> triangels = new List<int>();

        int leftVertex = vertices.Count;
        int ai = 0;
        int bi = ai + 1;
        int ci = bi + 1;

        int count = 0;

        while (leftVertex > 3)
        {
            if (IsLeft(vertices[ai], vertices[bi], vertices[ci]) &&
                IsTriangleExist(vertices, ai, bi, ci))
            {
                triangels.Add(ai);
                triangels.Add(bi);
                triangels.Add(ci);
                taken[bi] = true;
                leftVertex--;
                bi = FindNotTaken(bi + 1, vertices.Count, taken);
                ci = FindNotTaken(ci + 1, vertices.Count, taken);
            }
            else
            {
                ai = FindNotTaken(ai + 1, vertices.Count, taken);
                bi = FindNotTaken(ai + 1, vertices.Count, taken);
                ci = FindNotTaken(bi + 1, vertices.Count, taken);
            }

            if (count > vertices.Count * vertices.Count)
            {
                triangels = null;
                break;
            }   
            count++;
        }
        if ((triangels.Count != 0) || (vertices.Count == 3))
        {
            triangels.Add(ai);
            triangels.Add(bi);
            triangels.Add(ci);
        }
        return triangels;
    }
    

    private int FindNotTaken(int pos, int count, bool[] taken)
    {
        pos %= count;
        if (!taken[pos])
            return pos;

        int i = (pos + 1) % count;
        while (i != pos)
        {
            if (!taken[i])
                return i;
            i = (i + 1) % count;
        }
        return -1;
    }

    private bool IsLeft(Vector2 point_a, Vector2 point_b, Vector2 point_c)
    {
        float abX = point_b.x - point_a.x;
        float abY = point_b.y - point_a.y;
        float acX = point_c.x - point_a.x;
        float acY = point_c.y - point_a.y;

        return abX * acY - acX * abY < 0;
    }

    private bool IsTriangleExist(List<Vector3> vertices, int ai, int ci, int bi)
    {
        for (var i = 0; i < vertices.Count; i++)
            if ((i != ai) && (i != bi) && (i != ci))
                if (IsPointInside(vertices[ai], vertices[bi], vertices[ci], vertices[i]))
                    return false;
        return true;
    }

    private bool IsPointInside(Vector2 Point_A, Vector2 Point_B, Vector2 Point_C, Vector2 Point_I)
    {
        float ab = (Point_A.x - Point_I.x) * (Point_B.y - Point_A.y) -
            (Point_B.x - Point_A.x) * (Point_A.y - Point_I.y);
        float bc = (Point_B.x - Point_I.x) * (Point_C.y - Point_B.y) -
            (Point_C.x - Point_B.x) * (Point_B.y - Point_I.y);
        float ca = (Point_C.x - Point_I.x) * (Point_A.y - Point_C.y) -
            (Point_A.x - Point_C.x) * (Point_C.y - Point_I.y);

        return (ab >= 0 && bc >= 0 && ca >= 0) || (ab <= 0 && bc <= 0 && ca <= 0);
    }
}

