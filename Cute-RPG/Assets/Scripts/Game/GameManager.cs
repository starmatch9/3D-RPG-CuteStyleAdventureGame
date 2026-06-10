using System;
using UnityEngine;

// 使用单例模式
public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }


    #region 玩家

    [Header("Player")] 
    public Player player;
    public Transform playerTransform;

    private Vector3 startPosition;

    Vector3 GetPlayerPosition()
    {
        return playerTransform.position;
    }

    void SetPlayerPosition(Vector3 position)
    {
        playerTransform.position = position;
    }

    #endregion


    #region 生命周期函数

    void Awake()
    {
        // 单例设置
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        // 记录此关卡的开始位置
        startPosition = GetPlayerPosition();
        

    }

    private void Update()
    {
        Debug.Log(GetPlayerPosition());
        if (GetPlayerPosition().y <= -50)
        {
            ResetLevel();
        }
    }

    #endregion

    #region 需要暴露的方法（外部使用）

    // 重置关卡
    public void ResetLevel()
    {
        // 把玩家放回原位
        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
        // 设置位置
        SetPlayerPosition(startPosition);
        // 重新启用
        if (controller != null)
        {
            controller.enabled = true;
        }
    }

    #endregion
}