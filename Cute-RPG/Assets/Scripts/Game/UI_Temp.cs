using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UI_Temp : MonoBehaviour
{
        public TextMeshProUGUI waterCount;

        public GameObject Victory;

        // 三颗星星
        public GameObject star_1;
        public GameObject star_2;
        public GameObject star_3;

        private void Start()
        {
            GameManager.instance.uiTemp = this;
        }

        void Update()
        {
            waterCount.text = "" + GameManager.instance.GetCoin();
        }

        public void Finish()
        {
            Victory.SetActive(true);
        }

        public void LightStar(int idx)
        {
            if (idx == 1)
            {
                star_1.SetActive(true);
            }
            else if (idx == 2)
            {
                star_2.SetActive(true); 
            }
            else
            {
                star_3.SetActive(true);
            }
        }
    
}