using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeneratorTest : MonoBehaviour {
    public Transform shape_prefab; 

	// Use this for initialization
	void Start () {
        var list_vert2 = new List<Vector3>();
        var list_vert1 = new List<Vector3>();


        //list_vert1.Add(new Vector3(0.6f, 0, 0));
        //list_vert1.Add(new Vector3(0.8f, -0.6f, 0));
        //list_vert1.Add(new Vector3(-0.1f, -0.6f, 0));
        //list_vert1.Add(new Vector3(-0.5f, -0.9f, 0));
        //list_vert1.Add(new Vector3(-0.9f, 0, 0));
        //list_vert1.Add(new Vector3(-1f, 0.2f, 0));
        //list_vert1.Add(new Vector3(-0.5f, 0.9f, 0));
        //list_vert1.Add(new Vector3(-0.3f, 1f, 0));
        //list_vert1.Add(new Vector3(0.4f, 0.4f, 0));
        //list_vert1.Add(new Vector3(0.9f, 0.4f, 0));

        list_vert1.Add(new Vector3(0.5f, 0.5f, 0));
        list_vert1.Add(new Vector3(0.5f, -0.5f, 0));
        list_vert1.Add(new Vector3(-0.5f, -0.5f, 0));
        list_vert1.Add(new Vector3(-0.8f, 0f, 0));
        list_vert1.Add(new Vector3(-0.5f, 0.5f, 0));

        list_vert2.Add(new Vector3(2f, 5f, 0));
        list_vert2.Add(new Vector3(2f, -5f, 0));
        list_vert2.Add(new Vector3(0f, -2.4f, 0));
        list_vert2.Add(new Vector3(-2f, -5f, 0));
        list_vert2.Add(new Vector3(-2f, 5f, 0));

        /*
        list_vert2.Add(new Vector3(0.5f, 0.5f, 0));
       // list_vert.Add(new Vector3(0.7f, 0.2f, 0));
        list_vert2.Add(new Vector3(0.5f, -0.5f, 0));
        list_vert2.Add(new Vector3(-0.5f, -0.5f, 0));
        list_vert2.Add(new Vector3(-0.8f, 0.2f, 0));
        list_vert2.Add(new Vector3(-0.5f, 0.6f, 0));

        */
        //shape.position = new Vector3(0, -3.8f, 1);

        var shape = Instantiate(shape_prefab, transform) as Transform;
        shape.position = new Vector3(20, -5, 1);
        shape.GetComponent<ShapeGenerete>().MatherialColor = new Color(Random.Range(1f, 10f), Random.Range(1f, 10f), Random.Range(1f, 10f));
        shape.GetComponent<ShapeGenerete>().Scale = new Vector3(1, 1, 1);
        shape.GetComponent<ShapeGenerete>().SetVertices(list_vert1);
        shape.GetComponent<Rigidbody2D>().velocity = new Vector2(-10, 0);
        shape.name = "other";
       // shape.GetComponent<Rigidbody2D>().isKinematic = true;

       
         var shape2 = Instantiate(shape_prefab, transform) as Transform;
        shape2.position = new Vector3(0, 0f, 1);
        shape2.GetComponent<ShapeGenerete>().MatherialColor = new Color(Random.Range(1f, 10f), Random.Range(1f, 10f), Random.Range(1f, 10f));
        shape2.GetComponent<ShapeGenerete>().Scale = new Vector3(1, 2, 1);
        shape2.GetComponent<ShapeGenerete>().SetVertices(list_vert2);
        shape2.name = "main";
        shape2.GetComponent<Rigidbody2D>().isKinematic = true;

    }
}
