using System;

public interface IDamageable
{
    event Action<float> DamageTaken;
    event Action<NPC> Died;
    void TakeDamage(float damage);
    void SubscribeOnEvents();
    void UnsubscribeFromEvents();
    void OnDamageTaken(float damage);
}
//TODO decide: force developer to (not forget) implement these methods (left)
//or restrict dev to call methods Subscribe/Visualize as public (move to npc.cs)