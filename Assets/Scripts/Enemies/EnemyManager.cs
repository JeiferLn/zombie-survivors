using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
   [Header("Pool Enemy Reference")]
   [SerializeField] private Transform poolEnemyContainer;
   
   [Header("ActivationZone Reference")]
   [SerializeField] private EnemyActivationZone activationZone;
   
   [Header("Targets")]
   [SerializeField] private Transform mainTarget;
   [SerializeField] private Transform secondaryTarget;
   
   // list of active enemies
   private List<Enemy> activeEnemies = new List<Enemy>();


   private void OnEnable()
   {
      activationZone.OnEnemyActivated += GetActiveEnemies;
   }
   
   
   private void OnDisable()
   {
      activationZone.OnEnemyActivated -= GetActiveEnemies;
   }

   
   private void Start()
   {
      GetActiveEnemies();
   }

   
   private void GetActiveEnemies()
   {
      activeEnemies = new List<Enemy>();
      foreach (Transform child in poolEnemyContainer)
      {
         if (child.TryGetComponent(out Enemy enemy))
         {
            activeEnemies.Add(enemy);
         }
      }
   }
   
   
   private void Update()
   {
      if (activeEnemies.Count > 0 )
      {
         foreach (Enemy enemy in activeEnemies)
         {
            enemy.transform.position = 
               Vector2.MoveTowards(enemy.transform.position, mainTarget.position, 5f * Time.deltaTime);
         }
      }
   }
}
