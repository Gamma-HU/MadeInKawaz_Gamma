using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player_mamama : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Vector3 mousePosi;
    private Vector3 screenPosi;
    private int canmove;
    private int gaming;
    public GameObject Manager;
    public GameObject engine;
    public Sprite sorry;
    public Sprite chuuni;
    void Start()
    {
        canmove = 0;
        gaming = 1;
        gameObject.GetComponent<SpriteRenderer>().sprite = chuuni;
        engine.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {
        
        if (canmove * gaming == 1)
        {
            mousePosi = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);//マウスポジを取得
            screenPosi = Camera.main.ScreenToWorldPoint(mousePosi);//スクリーン座標に変換（？）
            transform.position = screenPosi;
        }
    }
    private void OnMouseDown()
    {
        canmove = 1;
    }
    private void OnMouseUp()
    {
        canmove = 0;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Finish") 
        {
            Manager.GetComponent<Manager>().count++;
            gaming = 0;
            gameObject.GetComponent<SpriteRenderer>().sprite = sorry;
            GetComponent<AudioSource>().Play();
            engine.SetActive(false);
        }
    }
}
