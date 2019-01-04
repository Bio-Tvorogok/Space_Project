using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO Добавить разброс редкости по цвету и эффектам

/*
 * Здесь происходит генерация заданных объектов в различных диапазонах и установка их начальных параметров:
 * расположение, размер, скорость...
 * Два вида генерации: на граници диапазона/внутри него
 */
// Автоматическое подключение нужного компонента
[RequireComponent(typeof(CircleCollider2D))]
public class AsteroidGenerator : MonoBehaviour {
    // Префаб объекта
    [Tooltip("Генерируемый объект")]
    [SerializeField]
    private Transform shape_prefab;

    [Tooltip("Объект предка")]
    [SerializeField]
    private Transform parent;

    // Максимально значение в радиусе и позиция по z
    [Tooltip("Максимальное значение объектов")]
    [SerializeField]
    private int max_count = 30;

    [Tooltip("Позиция по оси Z")]
    [SerializeField]
    private int z_position = 1;

    // Размеры объектов
    [Tooltip("Минимальный размер объекта")]
    [SerializeField]
    private Vector2 min_values;

    [Tooltip("Максимальный размер объекта")]
    [SerializeField]
    private Vector2 max_values;

    private float radius;
    // Генерация объектов внутри диапазона
	void Start()
    {
        for (var i = 0; i < max_count; i++)
        {
            radius = GetComponent<CircleCollider2D>().radius;
            Transform shape;
            shape = Instantiate(shape_prefab, parent) as Transform;
            var scale_x = Random.Range(min_values.x, max_values.x);
            var scale_y = Random.Range(min_values.y, max_values.y);
            var new_pos = new Vector3();
            while (true)
            {
                new_pos = GetNewPosition(false);
                if (IsPositionEmpty(new_pos, Mathf.Max(scale_x, scale_y)))
                    break;                    
            }
            shape.position = new_pos;
            shape.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f));

            shape.GetComponent<ShapeGenerete>().MatherialColor = new Color(Random.Range(1f, 10f), Random.Range(1f, 10f), Random.Range(1f, 10f));
            shape.GetComponent<ShapeGenerete>().Scale = new Vector3(scale_x, scale_y);
            shape.name = i.ToString();
        }
    }

    // Удаление вышедшего за предел обьекта и его генерация в доступном месте на окраине диапазона
    private void OnTriggerExit2D(Collider2D collision)
    {

        if ((collision.tag == "Asteroid") || (collision.tag == "Splinter"))
        {
            var shape = collision.GetComponent<ShapeGenerete>();
            if (shape != null)
                Destroy(collision.gameObject);
            if (NoSimilar(collision))
            {
                var asteroid = Instantiate(shape_prefab, parent) as Transform;
                var scale_x = Random.Range(min_values.x, max_values.x);
                var scale_y = Random.Range(min_values.y, max_values.y);
                var new_pos = new Vector3();
                while (true)
                {
                    new_pos = GetNewPosition(true);
                    if (IsPositionEmpty(new_pos, Mathf.Max(scale_x, scale_y)))
                        break;
                }
                asteroid.position = new_pos;
                //asteroid.localScale = new Vector3(scale_x, scale_y, 1);
                asteroid.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-3f, 3f), Random.Range(-3f, 3f));

                asteroid.GetComponent<ShapeGenerete>().MatherialColor = new Color(Random.Range(1f, 10f), Random.Range(1f, 10f), Random.Range(1f, 10f));
                asteroid.GetComponent<ShapeGenerete>().Scale = new Vector3(scale_x, scale_y);
                asteroid.name = collision.name;
            }
        }
        //else if (collision.tag == "Splinter")
        //{
        //    Destroy(collision.gameObject);
        //} 
    }

    private bool NoSimilar(Collider2D collision)
    {
        var objects = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var body in objects)
            if (body.name == collision.name) return false;
        return true;
    }

    // Возвращает рандомную позицию на окраине диапазона/внутри него
    private Vector3 GetNewPosition(bool isOnSurface)
    {
        var angle = Random.Range(0, 2 * Mathf.PI);
        var x = Mathf.Cos(angle) * (isOnSurface ? radius : Random.Range(0, radius));
        var y = Mathf.Sin(angle) * (isOnSurface ? radius : Random.Range(0, radius));
        var vector_pos = transform.position - new Vector3(x, y, 0);
        vector_pos.z = z_position;
        return vector_pos;
    }

    // Проверка позиции для генарации на пересечения с другими коллайдерами
    private bool IsPositionEmpty(Vector3 prefab_pos, float max_scale)
    {
        var immediateColliders = Physics2D.OverlapCircleAll(prefab_pos, max_scale);
        if (immediateColliders.Length > 1)
            return false;
        else return true;
    }
}
