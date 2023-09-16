using System.Collections;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] float boomTime;
    private void Awake()
    {
        StartCoroutine(DestroyMyself());
    }

    IEnumerator DestroyMyself()
    {
        yield return new WaitForSeconds(boomTime);
        Destroy(gameObject);
    }
}
