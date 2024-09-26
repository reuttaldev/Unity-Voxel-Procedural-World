using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    [SerializeField]
    private GameObject chunkPrefab;

    private void Start()
    {
        var chunk = Instantiate(chunkPrefab);
        
    }
}
