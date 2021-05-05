using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float speed = 6f;

    //defaults are level 1 camera borders
    [Header("Screen Border Values")]
    [SerializeField] float minX = 2.3f;
    [SerializeField] float maxX = 14.735f;
    [SerializeField] float minY = -3f;
    [SerializeField] float maxY = -8f;

    private void Update()
    {
        float xAxisValue = Input.GetAxis("Horizontal") * speed;//* 0.035f;  //* (speed * Time.deltaTime);
        float yAxisValue = Input.GetAxis("Vertical") * speed; //* 0.035f; //* (speed * Time.deltaTime);

        if (Camera.current != null)
        {
            Camera.current.transform.Translate(new Vector3(xAxisValue, yAxisValue, 0.0f) * Time.deltaTime);
        }

        if (transform.position.x <= minX)
        {
            transform.position = new Vector3(minX, transform.position.y, transform.position.z);
        }
        else if (transform.position.x >= maxX)
        {
            transform.position = new Vector3(maxX, transform.position.y, transform.position.z);
        }

        if (transform.position.y >= minY)
        {
            transform.position = new Vector3(transform.position.x, minY, transform.position.z);
        }
        else if (transform.position.y <= maxY)
        {
            transform.position = new Vector3(transform.position.x, maxY, transform.position.z);
        }
            
    }

}
