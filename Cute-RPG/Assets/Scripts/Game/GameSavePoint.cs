using System;
using UnityEngine;

public class GameSavePoint : MonoBehaviour
{
    public Transform point;

    public GameObject flag;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 更新 GameManager 中的出生点
            if (GameManager.instance != null)
            {
                GameManager.instance.SetSpawnPoint(point != null ? point.position : transform.position);
                if (flag.activeSelf == false)
                {
                    flag.SetActive(true);
                }
            }
        }
    }
}