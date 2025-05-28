using UnityEngine;

public class Manager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int count;
    public int zombiNum = 5;
    public GameObject[] zombi;
    public Sprite old;
    public Sprite zom;

    Color color;
    Color firecolor;
    void Start()
    {
        color = new Color(255, 255, 255, 255);
        firecolor = new Color(255, 0, 0, 255);
        count = zombiNum - 1;
        int fire = Random.Range(0, zombiNum);
        for (int i = 0; i < zombiNum; i++) {
            if (i != fire)
            {
                zombi[i].tag = "Untagged";
                zombi[i].GetComponent<SpriteRenderer>().sprite = zom;
            }
            else {
                zombi[i].tag = "Finish";
                zombi[i].GetComponent <SpriteRenderer>().sprite = old;
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
        Debug.Log(count);
    }
}
