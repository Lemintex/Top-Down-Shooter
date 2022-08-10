using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof(NavMeshAgent))]
public class Enemy : DamageableEntity
{
    // enum for state machine
    public enum State
    {
        IDLE,
        CHASING,
        ATTACKING
    }
    State state;
    
    NavMeshAgent pathFinder;
    Transform target;
    DamageableEntity targetEntity;

    float attackDistance = 2.5f;
    float attackCooldown = 2;
    float lastAttackTime;

    float myRadius;
    float targetRadius;
    protected override void Start()
    {
        base.Start();
        startingHealth = 50;
        // if there is no player there is nothing to chase
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            state = State.CHASING;
            pathFinder = GetComponent<NavMeshAgent>();
            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<DamageableEntity>();
            targetEntity.OnDeath.AddListener(OnTargetDeath);

            myRadius = GetComponent<CapsuleCollider>().radius;
            targetRadius = target.GetComponent<CapsuleCollider>().radius;

            StartCoroutine(UpdatePath());
        }
    }

    void Update()
    {
        if (state != State.IDLE)
        {
            float squareDistanceToTarget = (target.position - transform.position).sqrMagnitude;

            if (squareDistanceToTarget < Mathf.Pow(attackDistance + myRadius + targetRadius, 2))
            {
                // close enough to dash, so try it
                DashAttack();
            }
        }
    }

    // invoked when target dies
    void OnTargetDeath()
    {
        state = State.IDLE;
    }

    // coroutine avoids recalculating the path every frame
    IEnumerator UpdatePath()
    {
        float refreshRate = 0.2f;
        while (state != State.IDLE)
        {
            if (state == State.CHASING)
            {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Vector3 targetPos = new Vector3(target.position.x, 0, target.position.z);
                Vector3 positionToPathfindTo = targetPos - (dirToTarget * (myRadius + targetRadius));
                if (alive)
                {
                    pathFinder.SetDestination(positionToPathfindTo);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }

    // called when the enemy is close enough to the target to attack
    void DashAttack()
    {
        if (lastAttackTime + attackCooldown < Time.time)
        {
            state = State.ATTACKING;
            pathFinder.enabled = false;
            lastAttackTime = Time.time;
            StartCoroutine(Attack());
        }
    }

    // coroutine handles smooth attack animation
    IEnumerator Attack()
    {
        Material material = GetComponent<Renderer>().material;
        material.color = Color.red;
        Vector3 myPos = transform.position;// for some reason using transform.position directly in Vector3.Lerp doesn't work, so this is needed
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 positionToAttackTo = target.position - (dirToTarget * myRadius);
        float lungePercent = 0;
        float dashSpeed = 2;

        bool hasAppliedDamage = false;
        while(lungePercent <= 1)
        {
            if (lungePercent >= 0.5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.Damage(1);
            }
            lungePercent += Time.deltaTime * dashSpeed;
            float lerp = (-Mathf.Pow(lungePercent, 2) + lungePercent) * 4; // goes from 0 to 1 to 0
            Debug.Log(lerp);
            transform.position = Vector3.Lerp(myPos, positionToAttackTo, lerp);
            yield return null;
        }
        pathFinder.enabled = true;
        material.color = Color.black;
    }
}
