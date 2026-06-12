using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UI_Temp : MonoBehaviour
{
        public TextMeshProUGUI waterCount;

        void Update()
        {
            waterCount.text = "" + GameManager.instance.GetCoin();
        }
}