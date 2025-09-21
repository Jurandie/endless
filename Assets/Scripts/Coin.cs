using UnityEngine;

public class Coin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.CollectCoin();
            gameObject.SetActive(false);
            AudioManager.Instance?.PlayCoin();
        }
    }
}
