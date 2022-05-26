using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookTowardsMovement : MonoBehaviour
{
    Rigidbody rb;
    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }
    void Update()
    {
        Vector3 look = transform.position + (rb.velocity).normalized;
        look.y = transform.position.y;
        transform.LookAt(look);
    }
}
