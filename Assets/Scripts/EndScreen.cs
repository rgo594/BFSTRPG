using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    Canvas canvas;

    private void Start()
    {
        canvas = FindObjectOfType<Canvas>();
    }
    // Update is called once per frame
    void Update()
    {
        if (FindObjectOfType<AiMove>() == null)
        {
            gameObject.transform.GetChild(2).gameObject.SetActive(true);
        }
    }
}
