using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class FollowAndRotate : MonoBehaviour
{
    public float rotationsPerSecond;
    float angle = 5;

    void Update()
    {
        transform.localPosition = new Vector3(0f, 0f, 0f);
        transform.Rotate(Vector3.up, rotationsPerSecond * Time.deltaTime * 360);
    }
}
