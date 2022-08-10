using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof(NavMeshAgent))]
public class Enemy : DamageableEntity
{
    public enum State
    {
        IDLE,
        CHASING,
        ATTACKING
    }
    State state;
    
    NavMeshAgent pathFinder;
    Transform target;

    float attackDistance = 2.5f;
    float attackCooldown = 2;
    float lastAttackTime;

    float myRadius;
    float targetRadius;
    protected override void Start()
    {
        base.startingHealth = 50;
        base.Start();
        state = State.CHASING;
        pathFinder = GetComponent<NavMeshAgent>();
        target = GameObject.FindGameObjectWithTag("Player").transform;

        myRadius = GetComponent<CapsuleCollider>().radius;
        targetRadius = target.GetComponent<CapsuleCollider>().radius;

        StartCoroutine(UpdatePath());
    }

    void Update()
    {
        float squareDistanceToTarget = (target.position - transform.position).sqrMagnitude;

        if (squareDistanceToTarget < Mathf.Pow(attackDistance + myRadius + targetRadius, 2))
        {
            // close enough to dash, so try it
            DashAttack();
        }
    }

    IEnumerator UpdatePath()
    {
        float refreshRate = 0.2f;
        while(target != null)
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

    IEnumerator Attack()
    {
        Material material = GetComponent<Renderer>().material;
        material.color = Color.red;
        Vector3 myPos = transform.position;// for some reason using transform.position directly in Vector3.Lerp doesn't work, so this is needed
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 positionToAttackTo = target.position - (dirToTarget * myRadius);
        float lungePercent = 0;
        float dashSpeed = 2;
        while(lungePercent <= 1)
        {
            lungePercent += Time.deltaTime * dashSpeed;
            float lerp = (-Mathf.Pow(lungePercent, 2) + lungePercent) * 4; // goes from 0 to 1 to 0
            Debug.Log(lerp);
            transform.position = Vector3.Lerp(myPos, positionToAttackTo, lerp);
            yield return null;
        }
        pathFinder.enabled = true;
        material.color = Color.black;
        state = State.CHASING; 
    }
}
