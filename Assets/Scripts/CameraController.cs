using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public float speed = 0.06f;

    private void Update()
    {
        float xAxisValue = Input.GetAxis("Horizontal"); /// speed;
        float zAxisValue = Input.GetAxis("Vertical"); /// speed;

        if (Camera.current != null)
        {
            Camera.current.transform.Translate(new Vector3(xAxisValue, zAxisValue, 0.0f));
        }
    }

}
