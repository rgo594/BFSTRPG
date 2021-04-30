using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public float speed = 0.15f;

    private void Update()
    {
        float xAxisValue = Input.GetAxis("Horizontal") * 0.035f;  //* (speed * Time.deltaTime);
        float zAxisValue = Input.GetAxis("Vertical") * 0.035f; //* (speed * Time.deltaTime);

        if (Camera.current != null)
        {
            Camera.current.transform.Translate(new Vector3(xAxisValue, zAxisValue, 0.0f));
        }
    }

}
