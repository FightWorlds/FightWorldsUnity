using System;
using System.Collections;
using UnityEngine;

public abstract class Damageable : MonoBehaviour
{
    //[SerializeField] protected AnimatorController animator;
    [SerializeField] protected LayerMask mask;
    [SerializeField] protected float attackRadius;
    [SerializeField] protected int damage;
    [SerializeField] protected int startHp;
    [SerializeField] private float particleOffset;
    [SerializeField] private float attackDelay;
    [SerializeField] private GameObject boom;
    [SerializeField] private ParticleSystem fire;
    [SerializeField] private ParticleSystem hitParticle;
    //[SerializeField] private Slider hpBar;

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
        StopCoroutine(searchCoroutine);
        while (target != null)
        {
            fire.Play();
            target.TryGetComponent(out Damageable damageable);
            if (damageable)
                damageable.TakeDamage(damage, currentPosition);
            Debug.Log($"BAM BAM {target.name} by {transform.name}");
            yield return new WaitForSeconds(attackDelay);
        }
        isAttacking = false;
        destination = Vector3.positiveInfinity;
        searchCoroutine = StartCoroutine(SearchTarget());
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
        // VisualizeHp();
        Debug.Log($"{gameObject.name} hp: {currentHp}");
        if (currentHp <= 0 && !isDestroyed)
            Die();
        //SPAWN PARTICLES OF FIRE (from SerializeField) from target side
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

    private void VisualizeHp()
    {
        //if (hpBar.maxValue < currentHp)
        //    hpBar.maxValue = currentHp;
        //
        //hpBar.value = currentHp;
    }

    protected abstract void Process();

    public virtual void UpdateLevel(float levelModifier)
    {
        damage = (int)(damage * levelModifier);
        startHp = (int)(startHp * levelModifier);
        currentHp = startHp;
    }
}