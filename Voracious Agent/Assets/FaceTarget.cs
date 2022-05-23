using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceTarget : MonoBehaviour
{
    public Transform target;
    Rigidbody rigidbody;
    private void Awake()
    {
        rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
    }
}
