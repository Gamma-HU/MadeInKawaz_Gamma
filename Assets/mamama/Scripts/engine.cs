using DG.Tweening;
using UnityEngine;

public class engine : MonoBehaviour
{
    private float Length = 17.0f;
    private float Phase = 7.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, Length * Mathf.Sin(Phase * Time.time));
    }
}
