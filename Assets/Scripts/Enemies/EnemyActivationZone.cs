using UnityEngine;

public class EnemyActivationZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && other.TryGetComponent(out EnemyActivation enemy))
        {
            enemy.SetVisible(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && other.TryGetComponent(out EnemyActivation enemy))
        {
            enemy.SetVisible(false);
        }
    }
}