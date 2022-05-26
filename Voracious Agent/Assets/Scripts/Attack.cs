using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public GameObject boss;
    public GameObject sword;
    BossHealth bossHealth;
    Transform bossPos;

    void Start()
    {
        bossHealth = boss.GetComponent<BossHealth>();
        bossPos = boss.GetComponent<Transform>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SwingNail();
            if ((bossPos.position - transform.position).magnitude < 8f)
            {
                bossHealth.ReceiveHit();
            }
        }
    }

    private void SwingNail()
    {
        int swing = (int)Random.Range(0f, 2.99f);
        switch (swing)
        {
            case 0:
                sword.transform.eulerAngles = new Vector3(16f, -39f, 18f);
                sword.transform.localPosition = new Vector3(-0.05f, 0.2f, 1.55f);
                break;
            case 1:
                sword.transform.eulerAngles = new Vector3(36f, -100f, 17f);
                sword.transform.localPosition = new Vector3(-0.22f, 0.5f, 1.83f);
                break;
            case 2:
                sword.transform.eulerAngles = new Vector3(1f, -39f, 6f);
                sword.transform.localPosition = new Vector3(0.38f, 0.26f, 1.65f);
                break;
        }
        
        sword.active = true;
        StartCoroutine(HideSword());
    }
    IEnumerator HideSword()
    {
        yield return new WaitForSeconds(0.2f);
        sword.active = false;
    }
}
