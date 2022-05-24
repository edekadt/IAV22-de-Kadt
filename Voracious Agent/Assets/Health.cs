using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Health : MonoBehaviour
{
    bool immune = false;
    ParticleSystem particles;
    private void Awake()
    {
        particles = gameObject.GetComponent<ParticleSystem>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 6 || collision.gameObject.layer == 7)
            ReceiveHit();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6 || other.gameObject.layer == 7)
            ReceiveHit();
    }

    private void ReceiveHit()
    {
        if (!immune)
            TakeDamage();
    }

    private void TakeDamage()
    {
        particles.Play();
        StartCoroutine(AddImmunity(0.8f));
    }

    IEnumerator AddImmunity(float duration)
    {
        immune = true;
        yield return new WaitForSeconds(duration);
        immune = false;
    }
}
