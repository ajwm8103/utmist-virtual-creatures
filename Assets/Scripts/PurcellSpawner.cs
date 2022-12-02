using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurcellSpawner : MonoBehaviour
{
    public GameObject purcellPrefab;
    public int swimmerCount;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < swimmerCount; i++)
        {
            GameObject purcellSpawned = Instantiate(purcellPrefab,
            new Vector3(Random.Range(-3f, 3f), Random.Range(0.5f, 4f), Random.Range(-3f, 3f)), purcellPrefab.transform.rotation);
            Destroy(purcellSpawned, 20f);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
