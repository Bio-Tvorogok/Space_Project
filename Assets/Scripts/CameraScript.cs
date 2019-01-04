using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Плавное перемещение камеры за движущимся объектов
 * Скорость преследования возрастает пропорционально увеличению удаления камеры от объекта
 */
public class CameraScript : MonoBehaviour {

    [Tooltip("Обьект слежения")]
    [SerializeField]
    private Transform player;

    // Так как все связанно с физикой, то перемещаем в данной функции
	void FixedUpdate () {
        transform.position = Vector2.Lerp(transform.position, player.position, Time.deltaTime * 
            Vector2.Distance(transform.position, player.position));
	}
}
