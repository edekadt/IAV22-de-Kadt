using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    bool immune = false;
    ParticleSystem particles;
    public Transform knight;
    private void Awake()
    {
        particles = gameObject.GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && (knight.position - transform.position).magnitude < 8f)
            ReceiveHit();
    }

    private void ReceiveHit()
    {
        Debug.Log("BossHit");
        if (!immune)
            TakeDamage();
    }

    private void TakeDamage()
    {
        Debug.Log("Hit");
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
