using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class EightPuzzle : MonoBehaviour
{
    [SerializeField]
    private Material mat;
    [SerializeField]
    private int[,] board = new int[3, 3]
    {
        { 0, 1, 2 },
        { 3, 4, 5 },
        { 6, 7, 8 }
    };

    public GameObject[] pieces;

    public Vector2Int mousePiecePos;

    public Vector2Int blankPiecePos;

    public int shuffleCount = 1;

    public Vector2Int prevBlankPiecePos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        blankPiecePos = new Vector2Int(2, 0);
        prevBlankPiecePos = blankPiecePos;

        // ƒVƒƒƒbƒtƒ‹
        for (int i = 0; i < shuffleCount; i++)
        {
            List<Vector2Int> nearbyPosList = GetBlankNearbyPiecePoses();
            nearbyPosList.Remove(prevBlankPiecePos);
            Vector2Int swapPos = nearbyPosList[Random.Range(0, nearbyPosList.Count)];            
            Swap(swapPos.x, swapPos.y, blankPiecePos.x, blankPiecePos.y);
            prevBlankPiecePos = blankPiecePos;
            blankPiecePos = swapPos;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        int pieceX = Mathf.FloorToInt(mousePos.x);
        int pieceY = Mathf.FloorToInt(mousePos.y);
        pieceX = Mathf.Clamp(pieceX, 0, 2);
        pieceY = Mathf.Clamp(pieceY, 0, 2);
        mousePiecePos = new Vector2Int(pieceX, pieceY);

        if (Input.GetMouseButtonDown(0))
        {
            if(CanSwap(mousePiecePos.x, mousePiecePos.y, blankPiecePos.x, blankPiecePos.y))
            {
                Swap(mousePiecePos.x, mousePiecePos.y, blankPiecePos.x, blankPiecePos.y);
                blankPiecePos = mousePiecePos;
                if(IsSuccessed() && !GameManager.ClearFlag)
                {
                    GameManager.Clear();
                }
            }
        }
    }

    List<Vector2Int> GetBlankNearbyPiecePoses()
    {
        List<Vector2Int> nearbyPosList = new List<Vector2Int>();
        if(blankPiecePos.x - 1 >= 0)
        {
            nearbyPosList.Add(new Vector2Int(blankPiecePos.x - 1, blankPiecePos.y));
        }

        if (blankPiecePos.x + 1 < 3)
        {
            nearbyPosList.Add(new Vector2Int(blankPiecePos.x + 1, blankPiecePos.y));
        }
        if (blankPiecePos.y - 1 >= 0)
        {
            nearbyPosList.Add(new Vector2Int(blankPiecePos.x, blankPiecePos.y - 1));
        }
        if (blankPiecePos.y + 1 < 3)
        {
            nearbyPosList.Add(new Vector2Int(blankPiecePos.x, blankPiecePos.y + 1));
        }
        return nearbyPosList;
    }

    public bool CanSwap(int x1, int y1, int x2, int y2)
    {
        return (Mathf.Abs(x1 - x2) == 1 && y1 == y2) || (x1 == x2 && Mathf.Abs(y1 - y2) == 1);
    }

    public void Swap(int x1, int y1, int x2, int y2)
    {
        int temp = board[y1, x1];
        board[y1, x1] = board[y2, x2];
        board[y2, x2] = temp;
        pieces[board[y1, x1]].transform.position = new Vector3(x1, y1, 0);
        pieces[board[y2, x2]].transform.position = new Vector3(x2, y2, 0);
    }

    public bool IsSuccessed()
    {
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                if (board[y, x] != y * 3 + x)
                {
                    return false;
                }
            }
        }
        return true;
    }

    [ContextMenu("Create")]
    public void CreateMesh()
    {
        pieces = new GameObject[9];
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                GameObject pieceObj = new GameObject($"{x}, {y}");
                pieceObj.transform.position = new Vector3(x, y, 0);
                var mf = pieceObj.AddComponent<MeshFilter>();
                var mr = pieceObj.AddComponent<MeshRenderer>();
                mr.material = mat;
                Mesh mesh = new Mesh();
                mesh.vertices = new Vector3[]
                {
                        new Vector3(0, 0, 0),
                        new Vector3(1, 0, 0),
                        new Vector3(1, 1, 0),
                        new Vector3(0, 1, 0)
                };
                mesh.triangles = new int[]
                {
                        2, 1, 0,
                        0, 3, 2
                };
                Vector2 offset = new Vector2(x / 3f, y / 3f);
                mesh.uv = new Vector2[]
                {
                        offset + new Vector2(0, 0),
                        offset + new Vector2(1f/3f, 0),
                        offset + new Vector2(1f/3f, 1f/3f),
                        offset + new Vector2(0f, 1f/3f),
                };
                mesh.RecalculateNormals();
                mf.mesh = mesh;
                pieces[board[y, x]] = pieceObj;
                if (board[y, x] == 2)
                {
                    mesh.colors = new Color[]
                    {
                        Color.black,
                        Color.black,
                        Color.black,
                        Color.black
                    };
                }
            }
        }
    }
}
