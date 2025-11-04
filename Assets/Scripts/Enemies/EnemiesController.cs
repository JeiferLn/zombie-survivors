
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class EnemiesController : MonoBehaviour
{
  [SerializeField] private float speed;
  [SerializeField] private List<Transform> targetPlayers;
  
  private Transform mainTarget= null;
  
  private void Start()
  {
    SelectTarget();
    SetRandomSpeed();
  }

  /// <summary>
  /// Select target to follow
  /// </summary>
  private void SelectTarget()
  {
    mainTarget = targetPlayers[Random.Range(0,targetPlayers.Count)].transform;
  }

  /// <summary>
  /// Set Speed Randomly
  /// </summary>
  private void SetRandomSpeed()
  {
    speed = Random.Range(2f, 5f);
  }

  private void Update()
  {
    if (!mainTarget) return;  
    
    if (Vector2.Distance(mainTarget.position, transform.position) > 0.5f)
      transform.position = Vector2.MoveTowards(transform.position, mainTarget.position, speed * Time.deltaTime);
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.CompareTag("Bullet"))
    {
      Destroy(gameObject);
      Destroy(other.gameObject);
    }
  }
}
