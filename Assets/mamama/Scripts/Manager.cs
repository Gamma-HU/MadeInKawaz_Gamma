using UnityEngine;

public class Manager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int count;
    int zombiNum = 3;
    public GameObject[] zombi;

    Color color;
    Color firecolor;
    void Start()
    {
        color = new Color(255, 255, 255, 255);
        firecolor = new Color(255, 0, 0, 255);
        count = zombiNum;
        int fire = Random.Range(0, zombiNum);
        for (int i = 0; i < zombiNum; i++) {
            if (i != fire)
            {
                zombi[i].tag = "Untagged";
                zombi[i].GetComponent<SpriteRenderer>().color = color;
            }
            else {
                zombi[i].tag = "Finish";
                zombi[i].GetComponent <SpriteRenderer>().color = firecolor;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (count == 0)
        {
            Debug.Log("clear!!");
            GameManager.Clear();
        }
    }
}
