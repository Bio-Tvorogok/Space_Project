using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Работа с частицами.
 * Два массива для различных режимов работы и один для задания рабочего диапазона.
 * Скрипт запускается из другого скрипта, который расчитывает маневрирование объекта и использует визуальные функции этого модуля.
 */

// Это будет исправляться
public class ParticleScript : MonoBehaviour {
    // Важно чтобы идексы всех трех массивов соответствовали друг другу попарно
    // Трастеры при включении
    [Tooltip("Трастеры при включении")]
    [SerializeField]
    private ParticleSystem[] particle_arr_enable_set;
    // Трастеры на холостом ходу
    [Tooltip("Трастеры на холостом ходу")]
    [SerializeField]
    private ParticleSystem[] particle_arr_disable_set;
    // Диапазон рабочего угла трастеров
    // x - минимум, y - максимум
    [Tooltip("Диапазон рабочего угла трастеров")]
    [SerializeField]
    private Vector2[] particle_angles;

    // Проверка на ошибки в заданных массивах
    private void Awake()
    {
        if (particle_angles.Length != particle_arr_enable_set.Length)
            Debug.LogError("Error! particle_arr and particle_angle do not same!");
        if (particle_arr_disable_set.Length != particle_arr_enable_set.Length)
            Debug.LogError("Error! particle_disable and particle_bust do not same!");
    }

    // Непосредственно внешняя функция для генерации частиц
    public void Generate(float angle, bool isBoost)
    {
        for (var i = 0; i < particle_arr_enable_set.Length; i++)
        {
            var emission_bust = particle_arr_enable_set[i].emission;
            var emission_disble = particle_arr_disable_set[i].emission;
            if ((angle >= particle_angles[i].x) && (angle <= particle_angles[i].y) && isBoost)
            {
                emission_bust.enabled = true;
                emission_disble.enabled = false;
            }
            else
            {
                emission_bust.enabled = false;
                emission_disble.enabled = true;
            }
        }
    }
}
