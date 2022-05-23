using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seek : MonoBehaviour
{
    public Transform target;
    public float maxVel;
    public float acceleration;
    Rigidbody rigidbody;
    void Awake()
    {
        rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (rigidbody.velocity.magnitude < maxVel)
        {
            rigidbody.AddForce((target.position - transform.position).normalized * acceleration, ForceMode.Acceleration);
        }
    }
}
