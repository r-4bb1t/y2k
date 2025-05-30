using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixSpriteRotation : MonoBehaviour
{
    Transform parentTransform;
    private void Start()
    {
        parentTransform = transform.parent;
    }

    private void Update()
    {
        if(Mathf.Approximately(parentTransform.eulerAngles.z, 180f))
        {
            transform.localScale = new Vector3(4.180771f, 4.180771f, 4.180771f);
        }
        if (Mathf.Approximately(parentTransform.eulerAngles.z, 0f))
        {
            transform.localScale = new Vector3(-4.180771f, 4.180771f, 4.180771f);
        }
    }

    // LateUpdate���� SpriteRenderer�� ���� ȸ���� �ʱ�ȭ
    void LateUpdate()
    {
        transform.localRotation = Quaternion.Inverse(parentTransform.localRotation);
    }
}
