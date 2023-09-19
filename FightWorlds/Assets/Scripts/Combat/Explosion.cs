using System;
using System.Collections;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float boomTime;
    public Action<GameObject> UnActive;
    private void OnEnable() =>
        StartCoroutine(DestroyMyself());

    IEnumerator DestroyMyself()
    {
        yield return new WaitForSeconds(boomTime);
        UnActive(gameObject);
    }
}
