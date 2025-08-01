using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

namespace Unity.FPS.Game
{
    public class Health : NetworkBehaviour
    {
        [Tooltip("Maximum amount of health")] public float MaxHealth = 10f;

        [Tooltip("Health ratio at which the critical health vignette starts appearing")]
        public float CriticalHealthRatio = 0.3f;

        public UnityAction<float, GameObject> OnDamaged;
        public UnityAction<float> OnHealed;
        public UnityAction OnDie;

        public NetworkVariable<float> CurrentHealth { get; set; } = new NetworkVariable<float>();
        public bool Invincible { get; set; }
        public bool CanPickup() => CurrentHealth.Value < MaxHealth;

        public float GetRatio() => CurrentHealth.Value / MaxHealth;
        public bool IsCritical() => GetRatio() <= CriticalHealthRatio;

        bool m_IsDead;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
                CurrentHealth.Value = MaxHealth;
        }

        public void Heal(float healAmount)
        {
            float healthBefore = CurrentHealth.Value;
            CurrentHealth.Value += healAmount;
            CurrentHealth.Value = Mathf.Clamp(CurrentHealth.Value, 0f, MaxHealth);

            // call OnHeal action
            float trueHealAmount = CurrentHealth.Value - healthBefore;
            if (trueHealAmount > 0f)
            {
                OnHealed?.Invoke(trueHealAmount);
            }
        }

        [ServerRpc]
        public void TakeDamageServerRpc(float damage, bool hasSource, NetworkObjectReference damageSourceRef)
        {
            if (Invincible)
                return;

            float healthBefore = CurrentHealth.Value;
            CurrentHealth.Value -= damage;
            CurrentHealth.Value = Mathf.Clamp(CurrentHealth.Value, 0f, MaxHealth);

            // call OnDamage action
            float trueDamageAmount = healthBefore - CurrentHealth.Value;

            GameObject damageSource = null;
            if (hasSource && damageSourceRef.TryGet(out NetworkObject sourceNetObj))
            {
                damageSource = sourceNetObj.gameObject;
            }

            if (trueDamageAmount > 0f)
            {
                OnDamaged?.Invoke(trueDamageAmount, damageSource);
            }

            HandleDeath();
        }

        public void Kill()
        {
            CurrentHealth.Value = 0f;

            // call OnDamage action
            OnDamaged?.Invoke(MaxHealth, null);

            HandleDeath();
        }

        void HandleDeath()
        {
            if (m_IsDead)
                return;

            // call OnDie action
            if (CurrentHealth.Value <= 0f)
            {
                m_IsDead = true;
                OnDie?.Invoke();
            }
        }
    }
}