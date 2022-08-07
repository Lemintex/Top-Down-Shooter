using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    NavMeshAgent pathFinder;
    Transform target;
    // Start is called before the first frame update
    void Start()
    {
        pathFinder = GetComponent<NavMeshAgent>();
        target = GameObject.FindGameObjectWithTag("Player").transform;

        StartCoroutine(UpdatePath());
    }

    IEnumerator UpdatePath()
    {
        float refreshRate = 0.2f;
        while(target != null)
        {
            Vector3 targetPos = new Vector3(target.position.x, 0, target.position.z);
            pathFinder.SetDestination(targetPos);
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
