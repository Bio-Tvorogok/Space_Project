﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Скрипт отвечающий за разрушение сталкивающихся обьектов
 * На выходе получается осколки данного обьекта
 */
public class DestoyContact : MonoBehaviour
{
    public Transform shape_prefab;

    [Tooltip("Отключение разрушаемости")]
    [SerializeField]
    private bool enable_failure = false;

    [Tooltip("Максимальное количество изломов в прямой")]
    [SerializeField]
    [Range(0, 6)]
    private int max_nodes = 3;

    [Tooltip("Порог разрушения по массе")]
    [SerializeField]
    private float mass_block = 2;

    [Tooltip("Минимальная энергия для детектирования удара")]
    [SerializeField]
    private float min_energy = 10;

    [Tooltip("Максимальная энергия для детектирования удара")]
    [SerializeField]
    private float max_energy = 40;

    [Tooltip("Порог по длине ломаной")]
    [SerializeField]
    private float magnitude_block = 2;

    // Индексы вершин пересечений первых точек по ходу итерации
    private Pair<int, int> global_intersection = new Pair<int, int>();
    private class Pair<TFirst, TSecond>
        where TFirst : new()
        where TSecond : new()
    {
        public TFirst First { get; set; }
        public TSecond Second { get; set; }

        public Pair()
        {
            First = new TFirst();
            Second = new TSecond();
        }
        public Pair(TFirst first = default(TFirst), TSecond second = default(TSecond))
        {
            First = first;
            Second = second;
        }
    }

    private void Awake()
    {
        if (min_energy >= max_energy)
            Debug.LogError("Critical energy error!");
    }

    // При столкновении
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (enable_failure)
        {
            var contact_energy = collision.relativeVelocity.magnitude;
            if ((contact_energy > min_energy) && (GetComponent<Rigidbody2D>().mass > mass_block))
            {
               // Gizmos.color = Color.blue;
               // Gizmos.matrix.

                var contact = LocalContact(collision.contacts[0].point, false);
                var mesh = GetComponent<MeshFilter>().mesh;
                var vertices = new List<Vector3>(mesh.vertices);
                var direction = LocalContact(collision.relativeVelocity, true);

                var step = (max_energy - min_energy) / max_nodes;
                if (contact_energy > max_energy) contact_energy = max_energy;
                contact_energy -= min_energy;
                var start_nodes = (int)Mathf.Ceil(contact_energy / step);

                var list = new List<Pair<Vector3, Pair<Vector3, bool?>>> { new Pair<Vector3, Pair<Vector3, bool?>>
                (contact, new Pair<Vector3, bool?>(direction, null)) };
                Destructor(list, new List<Pair<Vector3, Pair<Vector3, bool?>>>(), vertices, start_nodes);
            }
        }
    }

    // Создание нового осколка
    private void CreateNewObjectDev(List<Vector3> vertices)
    {
        if (vertices != null)
        {
            if (vertices.Count > 2)
            {
                var new_object = Instantiate(shape_prefab) as Transform;
                var shape_generate = new_object.GetComponent<ShapeGenerete>();
                var rigid_body = new_object.GetComponent<Rigidbody2D>();

                //rigid_body.isKinematic = true;

                shape_generate.SetVertices(vertices);
                shape_generate.MatherialColor = GetComponent<ShapeGenerete>().MatherialColor;
                rigid_body.velocity = GetComponent<Rigidbody2D>().velocity;
                rigid_body.angularVelocity = GetComponent<Rigidbody2D>().angularVelocity;
                new_object.position = transform.position;
                new_object.tag = "Splinter";
                new_object.name = gameObject.name;
                new_object.parent = gameObject.transform.parent;
            }
        }
    }

    // Рекурсивное функция рабивающая обьект
    private List<Vector3> Destructor(List<Pair<Vector3, Pair<Vector3, bool?>>> new_nodes, List<Pair<Vector3, Pair<Vector3, bool?>>> prev_nodes, List<Vector3> vertices, int nodes, bool? isSide = null)
    {
        var current_new = new_nodes;
        var current_prev = prev_nodes;
        if (isSide != null)
        {
            current_new = GetSidePoints(new_nodes, vertices, isSide);
            current_prev = GetSidePoints(prev_nodes, vertices, null);
        }
        current_new.AddRange(current_prev);

        if ((current_new.Count == 0) && (isSide != null))
            return vertices;
        else
        {
            foreach (var node in current_new)
            {
                if ((isSide == null) || (node.Second.Second != null))
                {
                    var broken_list = BrokenLineDestructor(node.First, node.Second.First, vertices, nodes, 0.3f, node.Second.Second);
                    if (broken_list != null)
                    {
                        var asteroids = AsteroidSeparetor(vertices, broken_list);
                        if (asteroids != null)
                        {
                            if (nodes > 0) nodes--;
                            broken_list.RemoveAt(broken_list.Count - 1);
                            broken_list.RemoveAt(0);
                            current_new.Remove(node);
                            try
                            {
                                var vertices_one = Destructor(broken_list, current_new, asteroids.First, nodes, true);
                                var vertices_two = Destructor(broken_list, current_new, asteroids.Second, nodes, false);
                                if (vertices_one != null)
                                    CreateNewObjectDev(vertices_one);
                                if (vertices_two != null)
                                    CreateNewObjectDev(vertices_two);
                            } catch (UnityException)
                            {
                                CreateNewObjectDev(vertices);
                            }
                            break;
                        }
                        else throw new UnityException("RecursionError");
                    }
                    else throw new UnityException("RecursionError");
                }
            }
        }
        if (isSide == null)
            Destroy(gameObject);
        return null;
    }

    // Фильтрация узловых точек для конкретного астероида
    private List<Pair<Vector3, Pair<Vector3, bool?>>> GetSidePoints(List<Pair<Vector3, Pair<Vector3, bool?>>> nodes, List<Vector3> vertices, bool? isSide)
    {
        var indecent = new List<Pair<Vector3, Pair<Vector3, bool?>>>();
        foreach (var node in nodes)
            if (vertices.Contains(node.First) && node.Second != null)
            {
                if (isSide != null)
                {
                    if (isSide.Value == node.Second.Second.Value)
                        indecent.Add(node);
                }
                else indecent.Add(node);
            }
        return indecent;
    }

    // Подбор точек для ломаной линии
    private List<Pair<Vector3, Pair<Vector3, bool?>>> BrokenLineDestructor(Vector3 start_point, Vector2 direction, List<Vector3> vertices, int nodes, float angular_shift = 0.1f, bool? isTwisted = false)
    {
        var current_start = start_point;
        Vector2 current_direction = direction;
        if (isTwisted == true) current_direction = RandomDirection(direction, 1.4f, -0.4f);
        else if (isTwisted == false) current_direction = RandomDirection(direction, -0.4f, 1.4f);
        var contact = GetTwoPoint(new Pair<Vector3, Vector3>(current_start, current_direction), vertices);
        if ((contact.First != null) && (contact.Second != null))
        {
            var node_list = new List<Pair<Vector3, Pair<Vector3, bool?>>> { new Pair<Vector3, Pair<Vector3, bool?>>(current_start, null) };
            if ((contact.Second.Value - contact.First.Value).magnitude < magnitude_block)
                nodes = 0;
            //var custom_nodes = (int)Mathf.Round((contact.Second.Value - contact.First.Value).magnitude * 1.5f);
            //if (nodes > custom_nodes) nodes = custom_nodes;
            for (int i = 0; i < nodes; i++)
            {
                var node_vector = contact.Second.Value - contact.First.Value;
                var node_count = node_vector.magnitude / (nodes - i);
                node_count = node_count / node_vector.magnitude;
                //node_count = Random.Range(node_count * 0.1f, node_count * 0.9f);
                node_count = node_count * 0.5f;
                node_vector = node_vector * node_count;
                node_vector = contact.First.Value + node_vector;

                var old_direction = current_direction;
                current_direction = RandomDirection(current_direction, angular_shift, angular_shift);
                node_list.Add(new Pair<Vector3, Pair<Vector3, bool?>>(node_vector, new Pair<Vector3, bool?>(old_direction,
                    !IsSide(Vector3.zero, old_direction, current_direction))));
                contact = GetTwoPoint(new Pair<Vector3, Vector3>(node_vector, current_direction), vertices, false);

            }
            node_list.Add(new Pair<Vector3, Pair<Vector3, bool?>>(contact.Second.Value, null));

            for (int i = 1; i < node_list.Count; i++)
                Debug.DrawLine(node_list[i - 1].First, node_list[i].First, Color.blue, 1000000, false);

            //if (node_list.Count > 2)
            //    GetTwoPointEx(new Pair<Vector3, Vector3>(node_list[0].First, node_list[NextIterator(0, node_list.Count)].Second.First),
            //       new Pair<Vector3, Vector3>(node_list[node_list.Count - 2].First, current_direction), vertices);
            //else
            //{
            current_direction = node_list[node_list.Count - 1].First - node_list[0].First;
             GetTwoPoint(new Pair<Vector3, Vector3>(node_list[0].First, current_direction), vertices);
            //}
            return node_list;
        }
        return null;
    }

    // Разделяем общие вершины на два астероида
    private Pair<List<Vector3>, List<Vector3>> AsteroidSeparetor(List<Vector3> vertices, List<Pair<Vector3, Pair<Vector3, bool?>>> node_vertices)
    {
        if (node_vertices.Count >= 2)
        {
            var results_vertices = new Pair<List<Vector3>, List<Vector3>>();
            results_vertices.First = new List<Vector3>();
            results_vertices.Second = new List<Vector3>();

            int iterator = NextIterator(global_intersection.First, vertices.Count);
            while (iterator != NextIterator(global_intersection.Second, vertices.Count))
            {
                results_vertices.First.Add(vertices[iterator]);
                iterator = NextIterator(iterator, vertices.Count);
            }
            node_vertices.Reverse();
            foreach (var node in node_vertices)
            {
                if (!results_vertices.First.Contains(node.First))
                    results_vertices.First.Add(node.First);
            }
            while (iterator != NextIterator(global_intersection.First, vertices.Count))
            {
                results_vertices.Second.Add(vertices[iterator]);
                iterator = NextIterator(iterator, vertices.Count);
            }
            node_vertices.Reverse();
            foreach (var node in node_vertices)
            {
                if (!results_vertices.Second.Contains(node.First))
                    results_vertices.Second.Add(node.First);
            }
            return results_vertices;
        }
        return null;
    }

    // Получение двух точек пересечения
    private Pair<Vector3?, Vector3?> GetTwoPoint(Pair<Vector3, Vector3> start, List<Vector3> vertices,
        bool isStartIntersected = true)
    {
        vertices.Remove(start.First);
        var two_points = new Pair<Vector3?, Vector3?>(null, null);
        // Две точеки отсчета для нахождения пересечений
        // Новое направление, учитывая погрешность
        var end_current = (start.First + start.Second * 10);
        var start_current = new Vector3();
        if (isStartIntersected)
            start_current = start.First - start.Second;
        else
            start_current = start.First;

        for (var i = 0; i < vertices.Count; i++)
        {
            var start_new = vertices[i];
            var end_new = vertices[NextIterator(i, vertices.Count)];
            var target = IntersectionPoint(start_current, end_current, start_new, end_new);
            if (target != null)
            {
                // Добавление близжайших точек персечения и их индексы
                if (two_points.First == null)
                {
                    two_points.First = target.Value;
                    global_intersection.First = i;
                }
                else if ((target.Value - start.First).magnitude < (two_points.First.Value - start.First).magnitude)
                {
                    two_points.Second = two_points.First;
                    two_points.First = target.Value;
                    global_intersection.Second = global_intersection.First;
                    global_intersection.First = i;
                }
                else if (two_points.Second == null)
                {
                    two_points.Second = target.Value;
                    global_intersection.Second = i;
                }
                else if ((target.Value - start.First).magnitude < (two_points.Second.Value - start.First).magnitude)
                {
                    two_points.Second = target.Value;
                    global_intersection.Second = i;
                }
            }
        }
        //Debug.Log("============== " + two_points.First.Value.x + ", " + two_points.First.Value.y);
        if (!isStartIntersected)
        {
            two_points.Second = two_points.First;
            two_points.First = start.First;
        }
        return two_points;
    }

    /*
    private void GetTwoPointEx(Pair<Vector3, Vector3> start, Pair<Vector3, Vector3> end, List<Vector3> vertices)
    {
        //vertices.Remove(start.First);
        var two_points = new Pair<Vector3?, Vector3?>(null, null);

        var start_one_current = start.First - start.Second;
        var end_one_current = start.First + start.Second;

        var start_two_current = end.First;
        var end_two_current = end.First + end.Second * 10;

        for (int i = 0; i < vertices.Count - 1; i++)
        {
            var start_new = vertices[i];
            var end_new = vertices[NextIterator(i, vertices.Count)];
            var target = IntersectionPoint(start_one_current, end_one_current, start_new, end_new);
            if (target != null)
            {
                if (two_points.First == null)
                {
                    two_points.First = target.Value;
                    global_intersection.First = i;
                }
                else if ((target.Value - start.First).magnitude < (two_points.First.Value - start.First).magnitude)
                {
                    two_points.First = target.Value;
                    global_intersection.First = i;
                }
            }
            target = IntersectionPoint(start_two_current, end_two_current, start_new, end_new);
            if (target != null)
            {
                if (two_points.Second == null)
                {
                    two_points.Second = target.Value;
                    global_intersection.Second = i;
                }
                else if ((target.Value - end.First).magnitude < (two_points.Second.Value - end.First).magnitude)
                {
                    two_points.Second = target.Value;
                    global_intersection.Second = i;
                }
            }
        }
    }
    */

    // Точка пересечения
    private Vector3? IntersectionPoint(Vector3 start_point_1, Vector3 end_point_1, Vector3 start_point_2, Vector3 end_point_2)
    {
        if (IsSide(start_point_1, end_point_1, start_point_2) != IsSide(start_point_1, end_point_1, end_point_2))
            if (IsSide(start_point_2, end_point_2, start_point_1) != IsSide(start_point_2, end_point_2, end_point_1))
            {
                var x = -((start_point_1.x * end_point_1.y - end_point_1.x * start_point_1.y) * (end_point_2.x - start_point_2.x)
                    - (start_point_2.x * end_point_2.y - end_point_2.x * start_point_2.y) * (end_point_1.x - start_point_1.x)) /
                    ((start_point_1.y - end_point_1.y) * (end_point_2.x - start_point_2.x) - (start_point_2.y - end_point_2.y) *
                    (end_point_1.x - start_point_1.x));
                var y = ((start_point_2.y - end_point_2.y) * (-x) - (start_point_2.x * end_point_2.y - end_point_2.x * start_point_2.y)) /
                    (end_point_2.x - start_point_2.x);
                return new Vector3(x, y);
            }
        return null;
    }

    // Положение относительно прямой
    private bool? IsSide(Vector3 start, Vector3 end, Vector3 point)
    {
        var result = (end.x - start.x) * (point.y - start.y) - (end.y - start.y) * (point.x - start.x);
        if (result < 0) return false; // right
        else if (result > 0) return true; // left
        else return null;
    }

    // Следующий итератор
    private int NextIterator(int iterator, int maxLength)
    {
        return (iterator + 1) % maxLength;
    }

    // Расчет точки контакта относительно локальных координат, ротации корабля и растяжки объекта
    private Vector2 LocalContact(Vector2 space_contact, bool isVelocity)
    {
        Vector2 pos = transform.position;
        var local_contact = isVelocity ? space_contact : -(pos - space_contact);
        var change_angle = transform.rotation.eulerAngles.z * Mathf.PI / 180f;
        var x = local_contact.x;
        var y = local_contact.y;

        var angle = Mathf.Atan2(y, x) < 0 ? (2 * Mathf.PI + Mathf.Atan2(y, x)) : Mathf.Atan2(y, x);
        var new_angle = (angle - change_angle) % (Mathf.PI * 2);
        x = Mathf.Cos(new_angle) * local_contact.magnitude;
        y = Mathf.Sin(new_angle) * local_contact.magnitude;
        local_contact = new Vector2(x, y);

        return local_contact;
    }

    // Создание нового направления для ломаной
    private Vector3 RandomDirection(Vector2 direction, float angular_left = 0.1f, float angular_right = 0.1f)
    {
        var angle = Vector2.Angle(Vector2.right, direction) * Mathf.PI / 180;
        if (direction.y < 0) angle = 2 * Mathf.PI - angle;
        angle += Random.Range(-angular_right, angular_left);
        //angle += angular;
        angle = angle % (2 * Mathf.PI);
        if (angle < 0) angle = 2 * Mathf.PI + angle;

        return new Vector2(Mathf.Cos(angle) * direction.magnitude, Mathf.Sin(angle) * direction.magnitude);
    }
}