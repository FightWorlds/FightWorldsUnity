using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Damageable : MonoBehaviour
{
    [SerializeField] protected LayerMask mask;
    [SerializeField] protected float attackRadius;
    [SerializeField] protected int damage;
    [SerializeField] protected int startHp;
    [SerializeField] private float particleOffset;
    [SerializeField] private float attackDelay;
    [SerializeField] private GameObject boom;
    [SerializeField] private ParticleSystem fire;
    [SerializeField] private ParticleSystem hitParticle;

    public PlacementSystem placement;
    public event Action<int, Vector3> DamageTaken;

    private const int rotationAngle = 60;
    private const int rotationSpeed = 5;
    private const float hitPlayTime = 1f;

    protected int currentHp;
    protected bool isDestroyed;
    protected bool isAttacking;
    protected Vector3 destination;
    protected Collider target;
    protected Coroutine searchCoroutine;
    protected abstract List<Collider> Detections();
    protected bool inAttackRadius =>
        Vector3.Distance(destination, currentPosition) < attackRadius;

    protected float distance => Vector3.Distance(destination, currentPosition);
    protected Vector3 currentPosition => transform.position;
    public int Hp => currentHp;

    protected void OnEnable() => SubscribeOnEvents();
    protected void OnDisable() => UnsubscribeFromEvents();

    protected virtual void Awake()
    {
        destination = Vector3.positiveInfinity;
        currentHp = startHp;
        isAttacking = false;
        isDestroyed = false;
        target = null;
    }

    protected abstract IEnumerator SearchTarget();

    protected virtual IEnumerator AttackTarget(Func<bool> Usage = null)
    {
        isAttacking = true;
        StopCoroutine(searchCoroutine);
        while (target != null && target.enabled)
        {
            if (Usage == null)
                Attack();
            else
            {
                if (Usage())
                    Attack();
            }

            yield return new WaitForSeconds(attackDelay);
        }
        isAttacking = false;
        destination = Vector3.positiveInfinity;
        target = null;
        searchCoroutine = StartCoroutine(SearchTarget());
    }

    private void Attack()
    {
        fire.Play();
        target.TryGetComponent(out Damageable damageable);
        if (damageable)
            damageable.TakeDamage(damage, currentPosition);
        Debug.Log($"BAM BAM {target.name} by {transform.name}");
    }
    public void TakeDamage(int damage, Vector3 fromPos) =>
            DamageTaken?.Invoke(damage, fromPos);
    private void SubscribeOnEvents() => DamageTaken += OnDamageTaken;
    private void UnsubscribeFromEvents() => DamageTaken -= OnDamageTaken;
    protected virtual void OnDamageTaken(int damage, Vector3 fromPos)
    {
        if (damage < 0)
            return;

        StartCoroutine(PlayHit(fromPos));
        currentHp = (int)Mathf.Clamp(currentHp - damage, 0, currentHp);
        Debug.Log($"{gameObject.name} hp: {currentHp}");
        if (currentHp <= 0 && !isDestroyed)
            Die();
    }

    private IEnumerator PlayHit(Vector3 hitFromPos)
    {
        Vector3 direction = (hitFromPos - transform.position).normalized;
        direction *= particleOffset;
        direction.y = 1f; //vertical offset
        hitParticle.transform.localPosition = direction;
        Quaternion newRotation = Quaternion.LookRotation(direction);
        hitParticle.transform.Rotate(Vector3.up, newRotation.eulerAngles.y);
        hitParticle.gameObject.SetActive(true);
        hitParticle.Play();
        yield return new WaitForSeconds(hitPlayTime);
        hitParticle.gameObject.SetActive(false);
    }
    protected virtual void Die()
    {
        isDestroyed = true;
        isAttacking = false;
        target = null;
        Instantiate(boom, transform.position, Quaternion.identity);
    }

    public void Rotate(float angle = rotationAngle)
    {
        transform.GetChild(0).Rotate(Vector3.up, angle);
    }

    protected Vector3 RotateIntoTarget()
    {
        if (destination == Vector3.positiveInfinity)
            return destination;
        Vector3 direction = (destination - currentPosition)
            .normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);
        rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, 0);
        float blend = Mathf.Pow(0.5f, Time.deltaTime * rotationSpeed);
        Transform rotateTransform = transform.GetChild(0);
        rotateTransform.rotation =
            Quaternion.Slerp(rotation, rotateTransform.rotation, blend);
        return direction;
    }

    protected void FindTargetInDetections()
    {
        List<Collider> hitColliders = Detections();
        if (hitColliders == null || target == null)
            destination = Vector3.positiveInfinity;
        foreach (var collider in hitColliders)
        {
            if (collider == null || !collider.enabled) return;
            if (Vector3.Distance(collider.transform.position,
                transform.position) < distance)
            {
                target = collider;
                destination = target.transform.position;
            }
        }
    }

    protected abstract void Process();

    public virtual void UpdateStats(FiringStats stats)
    {
        damage = stats.Damage;
        attackDelay = 1f / stats.Rate;
        startHp = currentHp = stats.Strength;
    }
}