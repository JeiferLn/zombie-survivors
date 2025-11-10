using System;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    // Components to disable/enable
    private SpriteRenderer spr;
    private BoxCollider2D boxColl2D;
    private IDamageable _damageableImplementation;
    
    // Enemy active state (active/inactive)
    public bool isActive { get; private set; }
    
    private void Awake()
    {
        spr = GetComponent<SpriteRenderer>();
        boxColl2D = GetComponent<BoxCollider2D>();
    }

    public void SetVisible(bool isVisible)
    {
        isActive = isVisible;
        if(spr!=null) spr.enabled = isVisible;
        if(boxColl2D!=null) boxColl2D.enabled = isVisible;
    }

    public void TakeDamage(float amount)
    {
      Debug.Log($"aww shiet! {amount} Damage");
    }
}