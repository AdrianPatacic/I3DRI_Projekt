using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Models
{
    public abstract class Entity : MonoBehaviour
    {
        protected int maxHealth = 100;
        [SerializeField] protected bool invulnreable = false;
        [SerializeField] protected int currentHealth;


        public virtual void TakeDamage(int damage)
        {
            if (!invulnreable)
            {
                currentHealth -= damage;
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected abstract void Die();
    }
}
