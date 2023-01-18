using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class birdbanking : MonoBehaviour
{
    public Transform[] targets;
    public int currentTargetIndex = 0;
    public float turnSpeed = 10f;
    public float maxBank = 30f;
    public float moveSpeed = 2f;
    public float reachDistance = 1f;
    public Transform banker; // Child of this transform
    public Vector3 offsetter = new Vector3 (0,0,0);
    public bool drawGizmos = false;
    private Transform tempGO;

    void Start()
    {
        Shuffle();
    }

    public void Shuffle()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            int rnd = Random.Range(0, targets.Length);
            tempGO = targets[rnd];
            targets[rnd] = targets[i];
            targets[i] = tempGO;
        }
    }


    public Vector3 GetNodePos(int id)
    {
        return targets[id].position;
    }

    void Update()
    {

        Vector3 dest = GetNodePos(currentTargetIndex);
        Vector3 offset = dest - transform.position;
        if (offset.sqrMagnitude > reachDistance)
        {
            Transform currentTarget = targets[currentTargetIndex];
            Vector3 lookDirection = currentTarget.position + offsetter - transform.position;
            Vector3 normalizedLookDirection = lookDirection.normalized;
            float bank = maxBank * -Vector3.Dot(transform.right, normalizedLookDirection);
            Quaternion rot = Quaternion.LookRotation(normalizedLookDirection);
            banker.localRotation = Quaternion.AngleAxis(bank, Vector3.forward);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, Time.deltaTime * turnSpeed);
            transform.Translate(new Vector3(0, 0, moveSpeed) * Time.deltaTime);
        }
        else
        {
            ChangeDestNode();
        }

    }

    void ChangeDestNode()
    {
        currentTargetIndex++;
        if (currentTargetIndex >= targets.Length)
        {
            currentTargetIndex = 0;
        }
    }

    void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, GetNodePos(currentTargetIndex));
        }
    }

}

/*
// Update is called once per frame
void Update()
{
    // Get the current target.
    Transform currentTarget = targets[currentTargetIndex];

    // Get a vector from the current position to the target.
    // This is the vector we want to move our forward vector towards.
    Vector3 lookDirection = currentTarget.position - transform.position;

    // Calculate the rotation needed for this target look vector.
    Quaternion rot = Quaternion.LookRotation(lookDirection.normalized);

    // Rotate towards that target rotation based on the turn speed.
    transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * turnSpeed);
    // With the rotation complete, just move forward.
    transform.Translate(new Vector3(0, 0, moveSpeed) * Time.deltaTime);

}
*/