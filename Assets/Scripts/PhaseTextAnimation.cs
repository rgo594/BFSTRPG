using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseTextAnimation : MonoBehaviour
{
    public static bool PhaseTextPresent;

    public void PhaseTextFinished()
    {
        PhaseTextPresent = false;
    }

}
