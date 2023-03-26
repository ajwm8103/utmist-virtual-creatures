using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentContact : MonoBehaviour
{
    private Segment segment;

    // Start is called before the first frame update
    void Start()
    {
        segment = transform.parent.parent.GetComponent<Segment>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (segment == null)
        {
            segment = transform.parent.parent.GetComponent<Segment>();
        }
        else
        {
            Debug.Log(gameObject.name + " " + other.gameObject.name);
            segment.HandleStay(other, gameObject.name);
        }

    }

}
