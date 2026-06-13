using System;
using System.Collections;
using UnityEngine;

public class GameFinish : MonoBehaviour
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

            }
            if (flag.activeSelf == false)
            {
                flag.SetActive(true);
                GlobalData.effectManager.PlayEffect(GlobalData.effectManager.Victory);
                StartCoroutine(ScaleFlagCoroutine(1f, 10f, 0.5f));
            }
        }
    }
    
    private IEnumerator ScaleFlagCoroutine(float from, float to, float duration)
    {
        float elapsed = 0f;
        Vector3 originalScale = flag.transform.localScale;
        Vector3 startScale = originalScale * from;
        Vector3 endScale = originalScale * to;
        
        flag.transform.localScale = startScale;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // 使用 SmoothStep 让动画更平滑
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            flag.transform.localScale = Vector3.Lerp(startScale, endScale, smoothT);
            yield return null;
        }
        
        flag.transform.localScale = endScale;
        
        yield return new WaitForSeconds(2f);
        
        GameManager.instance.GameVictory();
    }
}