using System;
using System.Collections;
using UnityEngine;

public abstract class Damageable : MonoBehaviour
{
    //[SerializeField] protected AnimatorController animator;
    [SerializeField] protected LayerMask mask;
    [SerializeField] protected float attackRadius;
    [SerializeField] private int damage;
    [SerializeField] private int startHp;
    [SerializeField] private float attackDelay;
    //[SerializeField] private Slider hpBar;

    public PlacementSystem placement;
    public event Action<int> DamageTaken;

    protected const float rotationSpeed = 5f;

    protected int currentHp;
    protected bool isAttacking;
    protected bool isDead;
    protected Vector3 destination;
    protected Collider target;
    protected abstract Collider[] Detections();

    protected Vector3 currentPosition => transform.position;
    public int Hp => currentHp;

    protected void OnEnable() => SubscribeOnEvents();
    protected void OnDisable() => UnsubscribeFromEvents();

    protected virtual void Awake()
    {
        destination = Vector3.positiveInfinity;
        currentHp = startHp;
    }

    protected abstract IEnumerator SearchTarget();

    protected virtual IEnumerator AttackTarget()
    {
        isAttacking = true;
        while (target != null)
        {
            target.TryGetComponent<Damageable>(out Damageable damageable);
            if (damageable)
                damageable.TakeDamage(damage);
            Debug.Log($"BAM BAM {target.name} by unit {transform.name}");
            yield return new WaitForSeconds(attackDelay);
        }
        isAttacking = false;
        destination = Vector3.positiveInfinity;
    }

    public void TakeDamage(int damage) => DamageTaken?.Invoke(damage);
    private void SubscribeOnEvents() => DamageTaken += OnDamageTaken;
    private void UnsubscribeFromEvents() => DamageTaken -= OnDamageTaken;
    protected virtual void OnDamageTaken(int damage)
    {
        if (damage < 0)
            return;

        currentHp = (int)Mathf.Clamp(currentHp - damage, 0, currentHp);
        // VisualizeHp();
        Debug.Log($"{gameObject.name} hp: {currentHp}");
        if (currentHp <= 0)
            Die();
    }

    protected virtual void Die() => Debug.Log($"{gameObject.name} dead");

    protected Vector3 RotateIntoTarget()
    {
        if (destination == Vector3.positiveInfinity)
            return destination;
        Vector3 direction = (destination - currentPosition)
            .normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);
        float blend = Mathf.Pow(0.5f, Time.deltaTime * rotationSpeed);
        Transform rotateTransform = transform.GetChild(0);
        rotateTransform.rotation =
            Quaternion.Slerp(rotation, rotateTransform.rotation, blend);
        return direction;
    }

    private void VisualizeHp()
    {
        //if (hpBar.maxValue < currentHp)
        //    hpBar.maxValue = currentHp;
        //
        //hpBar.value = currentHp;
    }
}