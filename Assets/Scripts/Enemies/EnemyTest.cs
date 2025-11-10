using UnityEngine;

public class EnemyTest : MonoBehaviour, IDamageable
{
    public void TakeDamage(float amount)
    {
        Debug.Log("Enemy took damage: " + amount);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
