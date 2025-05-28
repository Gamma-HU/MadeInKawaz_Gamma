using UnityEngine;
using UnityEngine.EventSystems;

public class zombi : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject Manager;
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
       
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Manager.GetComponent<Manager>().count--;
            if (gameObject.tag != "Finish") {
                GetComponent<AudioSource>().Play();
            }
            else
            {
                Destroy(gameObject);
            }
            Destroy(gameObject, 0.3f);
        }
    }
}
