using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private GameObject health;
    
    // Start is called before the first frame update
    void Start()
    {
        health.transform.localScale = new Vector3(0.5f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
