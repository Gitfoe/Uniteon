using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class responsible for disabling the filtering on fonts for 8 and 16-bit style fonts.
public class DisableFontFiltering : MonoBehaviour
{
    [SerializeField] private Font[] fonts;
    private void Start()
    {
        foreach (var t in fonts)
        {
            t.material.mainTexture.filterMode = FilterMode.Point;
        }
    }
}
