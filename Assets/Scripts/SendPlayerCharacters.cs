﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendPlayerCharacters : MonoBehaviour
{
    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("CharacterList");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }
}
