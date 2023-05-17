using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class RLSpawner : MonoBehaviour
{
    public GameObject purcellPrefab;
    private GameObject purcellSpawned;
    public int swimmerCount;

    float timeout = 21f;
    private Stopwatch SW = new Stopwatch();

    // Start is called before the first frame update
    void Start()
    {
        SW.Start();
        Spawn_obj(purcellPrefab);
    }

    void Spawn_obj(GameObject purcellPrefab)
    {
        for (int i = 0; i < swimmerCount; i++)
        {
            purcellSpawned = Instantiate(purcellPrefab,
            new Vector3(0f, -0.5f, 0f), purcellPrefab.transform.rotation);
            
        }
        return;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        // UnityEngine.Debug.Log(SW.ElapsedMilliseconds);
        SW.Stop();
        if (SW.ElapsedMilliseconds >= timeout * 1000f)
        {
            SW.Reset();
            Spawn_obj(purcellPrefab);
        }
        SW.Start();
    }
}
