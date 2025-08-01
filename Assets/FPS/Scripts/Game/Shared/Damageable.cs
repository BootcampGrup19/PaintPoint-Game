using UnityEngine;
using Unity.Netcode;

namespace Unity.FPS.Game
{
    public class Damageable : NetworkBehaviour
    {
        [Tooltip("Multiplier to apply to the received damage")]
        public float DamageMultiplier = 1f;

        [Range(0, 1)]
        [Tooltip("Multiplier to apply to self damage")]
        public float SensibilityToSelfdamage = 0.5f;

        public Health Health { get; private set; }

        void Awake()
        {
            // find the health component either at the same level, or higher in the hierarchy
            Health = GetComponent<Health>();
            if (!Health)
            {
                Health = GetComponentInParent<Health>();
            }
        }

        public void InflictDamage(float damage, bool isExplosionDamage, NetworkObjectReference damageSourceRef)
        {
            if (!IsServer)
            {
                // İstemcideysek, isteği sunucuya bildir
                InflictDamageServerRpc(damage, isExplosionDamage, damageSourceRef);
            }
            else
            {
                // Zaten sunucudaysak, doğrudan işleyebiliriz
                ProcessDamage(damage, isExplosionDamage, damageSourceRef);
            }
        }
        [ServerRpc]
        private void InflictDamageServerRpc(float damage, bool isExplosionDamage, NetworkObjectReference damageSourceRef)
        {
            ProcessDamage(damage, isExplosionDamage, damageSourceRef);
        }

        private void ProcessDamage(float damage, bool isExplosionDamage, NetworkObjectReference damageSourceRef)
        {
            if (Health)
            {
                var totalDamage = damage;

                // skip the crit multiplier if it's from an explosion
                if (!isExplosionDamage)
                {
                    totalDamage *= DamageMultiplier;
                }

                // potentially reduce damages if inflicted by self
                if (damageSourceRef.TryGet(out NetworkObject sourceNetObj))
                {
                    if (Health.gameObject == sourceNetObj.gameObject)
                        totalDamage *= SensibilityToSelfdamage;

                    Health.TakeDamageServerRpc(totalDamage, true, damageSourceRef);
                }
                else
                {
                    Health.TakeDamageServerRpc(totalDamage, false, default); // Kaynağı belirlenemeyen hasar
                }
            }
        }
    }
}
