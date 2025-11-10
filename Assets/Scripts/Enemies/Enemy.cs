using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Transform mainTarget;
    [SerializeField] private Transform secondaryTarget;
    
    [Header("Settings")]
    
    
    private SpriteRenderer spr;
    private BoxCollider2D boxColl2D;
   
    
    private void Awake()
    {
        spr = GetComponent<SpriteRenderer>();
        boxColl2D = GetComponent<BoxCollider2D>();
    }

    public void SetVisible(bool isVisible)
    {
        if(spr!=null) spr.enabled = isVisible;
        if(boxColl2D!=null) boxColl2D.enabled = isVisible;
    }
    
}