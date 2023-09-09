using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shuttle : MonoBehaviour
{
    [SerializeField] Animator animator;
    private bool isEvacuating;

    public void StartCollecting()
    {
    }

    public void Evacuate()
    {
        if (!isEvacuating)
        {
            isEvacuating = true;
            animator.SetBool("Evacuating", isEvacuating);
        }
    }
}
