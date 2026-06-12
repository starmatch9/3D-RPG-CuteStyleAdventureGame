using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class StopGame : MonoBehaviour
{
    public void PlayEffectUI()
    {
        GlobalData.effectManager.PlayEffect(GlobalData.effectManager.UI);
    }
    
    public void StopAll()
    {
        GlobalData.effectManager.PlayEffect(GlobalData.effectManager.UI);
        //会暂停物理系统、动画系统、粒子系统、协程
        Time.timeScale = 0f;
    }

    public void ResumeAll()
    {
        GlobalData.effectManager.PlayEffect(GlobalData.effectManager.UI);
        Time.timeScale = 1f;
    }

    public void ReStart()
    {
        //回到主菜单也要Resume一次！！
        ResumeAll();
        //重新加载当前场景
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    

    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }

    //退出游戏
    public void ExitGame()
    {
        Application.Quit();
    }

    //回到主菜单
    public void BackStart()
    {
        ResumeAll();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
    }

}
