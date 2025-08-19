using System;
using System.Collections;
using Assets.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class EnemyController : Entity
{


    [SerializeField] GameObject player;
    [SerializeField] Transform[] patrollPoints;
    [SerializeField] private WeaponController weaponController1;
    [SerializeField] private WeaponController weaponController2;

    private int patrollPointCounter = 0;

    private NavMeshAgent agent;
    private Animator animator;
    private enum StateEnum { idle, follow, patroll, attack1, attack2 };
    private StateEnum state;

    [SerializeField] private float stopFollowDistance = 2.5f;
    [SerializeField] private float followDistance = 6f;
    [SerializeField]private float playerDistance;
    private float velocity;
    [SerializeField] private bool canMove = true;

    [SerializeField] private int randomAttack = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        state = StateEnum.patroll;
        agent.destination = patrollPoints[patrollPointCounter].position;
        currentHealth = maxHealth;
        //weaponController = GetComponent<WeaponController>();
    }


    void Update()
    {
        if (canMove)
        {
            if (state == StateEnum.patroll)
            {
                agent.speed = 1.5f;
            }
            else if (state == StateEnum.follow)
            {
                agent.speed = 3.5f;
            }
        }
        else 
        { 
            agent.speed = 0f;
            velocity = 0f;
        }


        velocity = agent.velocity.magnitude;
        playerDistance = Vector3.Distance(transform.position, player.transform.position);

        if(playerDistance < stopFollowDistance)
        {
            agent.ResetPath();

            Attack();
        }
        else if (playerDistance <= followDistance && playerDistance >= stopFollowDistance)
        {

            FollowPlayer();
            state = StateEnum.follow;
        }
        else if (playerDistance > followDistance)
        {

            if (state != StateEnum.patroll)
                BeginPatroll();

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && state == StateEnum.patroll)
                NextPatrollPoint();


        }

        animator.SetInteger("state", (int)state);

    }

    void Attack()
    {
        switch (randomAttack)
        {
            case 0:
                state = StateEnum.attack1; break;
            case 1:
                state = StateEnum.attack2; break;
        }
    }


    void SetNextRandomAttack()
    {
        randomAttack = UnityEngine.Random.Range(0, 2);

        Debug.Log("Random attack: " +  randomAttack);

    }


    void FollowPlayer()
    {
        agent.SetDestination(player.transform.position);

    }

    void BeginPatroll()
    {
        Debug.Log("Patroll Begun");
        agent.SetDestination(patrollPoints[patrollPointCounter].position);
        state = StateEnum.patroll;
    }

    void NextPatrollPoint()
    {

        if(patrollPointCounter < patrollPoints.Length - 1)
        {
            patrollPointCounter++;
        }
        else
        {
            patrollPointCounter = 0;
        }

        Debug.Log("Patroll point set to: " + patrollPointCounter);

        agent.destination = patrollPoints[patrollPointCounter].position;      

        state = StateEnum.patroll;
    }

    public void BeginAttack()
    {
        StartCoroutine(SmoothLookAt(player));
        canMove = false;
    }

    public void EndAttack()
    {
        StopCoroutine(SmoothLookAt(player));
        canMove = true;
        
    }

    public void AttackEnableCollider()
    {
        if(state == StateEnum.attack1)
            weaponController1.EnableCollider();
        else if(state == StateEnum.attack2)
            weaponController2.EnableCollider();
    }
    public void AttackDisableCollider()
    {
        weaponController1.DisableCollider();
        weaponController2.DisableCollider();
    }

    private IEnumerator SmoothLookAt(GameObject target)
    {
        float rotationSpeed = 5f; // adjust for smoothness
        Vector3 targetDirection = target.transform.position - transform.position;
        targetDirection.y = 0f; // keep the rotation on the Y axis only

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.5f)
        {
            targetDirection = target.transform.position - transform.position;
            targetDirection.y = 0f;
            targetRotation = Quaternion.LookRotation(targetDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed);

            yield return null;
        }
    }

    protected override void Die()
    {
        if (agent != null) agent.isStopped = true;
        animator.SetTrigger("Death");
        canMove = false;
        this.enabled = false;
    }

    public void DestroyObject()
    {
        Destroy(gameObject);
    }
}
