using UnityEngine;

public class Coin : MonoBehaviour
{
    private bool exist = true;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (exist)
            {
                exist = false;
                EatCoin();
            }
        }
    }

    public void EatCoin()
    {
        GlobalData.effectManager.PlayEffect(GlobalData.effectManager.eat);
        GameManager.instance.AddCoin();
        Destroy(gameObject);
    }
}