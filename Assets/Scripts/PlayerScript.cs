using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Модуль управления объектом игрока.
 * Расчет поведения при различных ситуациях/командах создаваемых игроком.
 */

// Автоматическое подключение нужного компонента
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerScript : MonoBehaviour {
    // Поля для ротации
    [Tooltip("Максимальна ротация")]
    [SerializeField]
    private float rotation_max;

    [Tooltip("Минимальная ротация")]
    [SerializeField]
    private float rotation_inc;

    // Поля для скорости
    [Tooltip("Максимальна скорость")]
    [SerializeField]
    private float speed_max;

    [Tooltip("Ускорение")]
    [SerializeField]
    private float speed_inc;

    // Точка для расчета направления оюъекта
    [Tooltip("Направление объекта")]
    [SerializeField]
    private Transform ship_direction_point;

    // Модуль частиц
    [Tooltip("Система частиц")]
    [SerializeField]
    private ParticleScript ship_particle;

    // Расчитанное, настоящее направление объекта 
    private Vector2 direction_main;

    private void Start()
    {
        direction_main = ship_direction_point.position - transform.position;       
    }

    private void FixedUpdate()
    {
        direction_main = ship_direction_point.position - transform.position;
        if (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            // Расчет точки касания и направлений
            var rbody = GetComponent<Rigidbody2D>();
            Vector2 position_touch = rbody.position;
            if (Input.touchCount > 0)
                position_touch = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            else if (Input.GetMouseButton(0))
                position_touch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            position_touch = -(rbody.position - position_touch);
            var direction = GetDirection(position_touch);
            var angle = Vector2.SignedAngle(direction, direction_main);

            // Расчитывание углов для погашения вращения/ускорения
            var rotation_angle = Mathf.Abs(angle) / 180;
            var velocity_angle = 1 - rotation_angle;
            // Применение физики к игроку
            if ((Mathf.Abs(rbody.angularVelocity) < rotation_max) && (angle != 0))
                rbody.AddTorque(-rotation_inc * Mathf.Sign(angle) * rotation_angle);
            if (rbody.velocity.magnitude < speed_max)
                rbody.AddForce(direction_main * speed_inc * velocity_angle);

            // Генерация частиц
            ship_particle.Generate(angle, true);
        }
        else
        {
            // Нет газа - холостой ход
            ship_particle.Generate(0, false);
        }
    }

    // Расчет указанного игроком направления от точки касания дисплея
    private Vector2 GetDirection(Vector2 touch_position)
    {
        var hypotenuse = touch_position.magnitude;
        var x = -touch_position.x / hypotenuse;
        var y = -touch_position.y / hypotenuse;
        return new Vector2(x, y);
    }
}
