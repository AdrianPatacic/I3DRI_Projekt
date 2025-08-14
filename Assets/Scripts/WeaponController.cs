using System.Collections.Generic;
using Assets.Models;
using UnityEngine;

public class WeaponController : MonoBehaviour
{

    private Collider weaponCollider;
    private HashSet<Collider> enemiesHit = new HashSet<Collider>();

    [SerializeField] int currDamage = 25;
    [SerializeField] string targetTag = "Enemy";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag) && !enemiesHit.Contains(other))
        {
            Entity enemy = other.GetComponent<Entity>();
            if (enemy != null)
            {
                enemy.TakeDamage(currDamage);
                enemiesHit.Add(other);
                Debug.Log(other.name + " hit for: " + currDamage);
            }
        }
    }



    void Start()
    {
        weaponCollider = GetComponent<Collider>();
        weaponCollider.enabled = false;
    }

    void Update()
    {
        
    }

    public void EnableCollider()
    {
        weaponCollider.enabled = true;
        enemiesHit.Clear();
    }
    public void DisableCollider() => weaponCollider.enabled = false;
}
