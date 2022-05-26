using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    bool immune = false;
    ParticleSystem particles;
    private void Awake()
    {
        particles = gameObject.GetComponent<ParticleSystem>();
    }

    public void ReceiveHit()
    {
        if (!immune)
            TakeDamage();
    }

    private void TakeDamage()
    {
        particles.Play();
        StartCoroutine(AddImmunity(0.3f));
    }

    IEnumerator AddImmunity(float duration)
    {
        immune = true;
        yield return new WaitForSeconds(duration);
        immune = false;
    }
}
