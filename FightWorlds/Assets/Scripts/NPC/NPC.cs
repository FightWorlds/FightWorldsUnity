using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class NPC : MonoBehaviour
{
    //[SerializeField] private StatsController statsController;
    //[SerializeField] private AnimatorController animator;
    //[SerializeField] private Slider hpBar;
    [SerializeField] private CharacterController character;
    [SerializeField] private float attackRadius;
    [SerializeField] private float speed;
    [SerializeField] private LayerMask targets;

    public event Action<float> DamageTaken;
    public event Action<NPC> Died;

    private const float rotationSpeed = 5f;
    private const float attackDelay = 4f;
    private const float searchDelayMultiplier = 0.1f;

    private float currentHp;
    private bool isAttacking;
    private Collider target;
    private Vector3 destination;
    Vector3 currentPosition => transform.position;
    private bool inAttackRadius =>
        Vector3.Distance(destination, currentPosition) < attackRadius;
    private float distance => Vector3.Distance(destination, currentPosition);
    // move realization out of class:
    private Collider[] buildings => Physics.OverlapBox(Vector3.zero,
                new Vector3(40, 4, 40), Quaternion.identity, targets);

    private void Awake() => destination = Vector3.positiveInfinity;

    private void Update()
    {
        if (target == null)
            if (buildings.Length != 0)
                StartCoroutine(SearchTarget());
            else return;
        if (destination != Vector3.positiveInfinity && !inAttackRadius)
        {
            Vector3 direction = (destination - currentPosition)
                .normalized;
            character.Move(direction * speed * Time.deltaTime);
            Quaternion rotation = Quaternion.LookRotation(direction);
            float blend = Mathf.Pow(0.5f, Time.deltaTime * rotationSpeed);
            transform.rotation = Quaternion.Slerp(rotation, transform.rotation, blend);
        }
    }

    private IEnumerator SearchTarget()
    {
        while (!inAttackRadius)
        {
            Collider[] hitColliders = buildings;
            if (hitColliders.Length == 0)
            {
                Debug.Log("GG YOU LOOSE");
                destination = Vector3.positiveInfinity;
                yield break;
            }
            foreach (var collider in hitColliders)
            {
                if (Vector3.Distance(collider.transform.position,
                    transform.position) < distance)
                {
                    target = collider;
                    destination = target.transform.position;
                }
            }
            yield return
            new WaitForSeconds(distance * searchDelayMultiplier);
        }
        StartCoroutine(AttackTarget());
    }

    private IEnumerator AttackTarget()
    {
        isAttacking = true;
        int counter = 0;
        while (target != null)
        {
            Debug.Log($"BAM BAM {target.name} by unit {transform.name}");
            counter++;
            if (counter == 3)
                Destroy(target);
            else
                //if (TryGetAttackTarget(out IDamageable target))
                //    target.TakeDamage(StatsController.GetStatValue(StatType.Damage));
                yield return new WaitForSeconds(attackDelay);
        }
        destination = Vector3.positiveInfinity;
        isAttacking = false;
        yield return null;
    }

    public void TakeDamage(float damage) => DamageTaken?.Invoke(damage);
    public void SubscribeOnEvents() => DamageTaken += OnDamageTaken;
    public void UnsubscribeFromEvents() => DamageTaken -= OnDamageTaken;
    public void OnDamageTaken(float damage)
    {
        //damage -= statsController.GetStatValue(StatType.Defence);
        if (damage < 0)
            return;

        currentHp = Mathf.Clamp(currentHp - damage, 0, currentHp);
        //VisualiseHp(currentHp);
        if (currentHp <= 0)
            Debug.Log($"{gameObject.name} dead");
        //Died?.Invoke();

    }
    private void VisualizeHp(float currentHp)
    {
        //if (hpBar.maxValue < currentHp)
        //    hpBar.maxValue = currentHp;
        //
        //hpBar.value = currentHp;
    }
}
