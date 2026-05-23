using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class GameManager_IronSand : MonoBehaviour
{
    private enum AttachAxis
    {
        X,
        Y
    }

    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private BoxCollider2D spawnArea;
    [SerializeField] private GameObject magnetObject;
    [SerializeField] private BoxCollider2D collectorCollider;
    [SerializeField] private AudioSource seSource;
    [SerializeField] private AudioClip collectClip;
    [SerializeField] private ParticleSystem collectParticle;
    [SerializeField] private GameObject ironSandPrefab;
    [SerializeField] private int sandCount = 35;
    [SerializeField] private Vector2 sandScaleRange = new Vector2(0.08f, 0.16f);
    [SerializeField] private Transform sandParent;
    [SerializeField] private float sandZ = 0f;
    [SerializeField] private Color fallbackSandColor = new Color(0.05f, 0.05f, 0.05f, 1f);
    [SerializeField] private bool createMissingObjects = true;
    [SerializeField] private Vector2 defaultSpawnAreaSize = new Vector2(12f, 6f);
    [SerializeField] private AttachAxis attachAxis = AttachAxis.Y;
    [SerializeField] private float attachLocalY = 0f;
    [SerializeField] private float attachGizmoWidth = 4f;
    [SerializeField] private float attachPerpendicularNoise = 0.08f;
    [SerializeField] private Color attachGizmoColor = new Color(0.1f, 0.8f, 1f, 1f);

    private readonly List<IronSandPiece> ironSands = new List<IronSandPiece>();
    private Camera targetCamera;
    private Transform collectorRoot;
    private int collectedCount;
    private bool cleared;

    private void Start()
    {
        targetCamera = Camera.main;
        EnsureSpawnArea();
        EnsureMagnet();
        EnsureCollector();
        EnsureAudioSource();
        SpawnIronSands();
        UpdateDebugText();
    }

    private void Update()
    {
        MoveMagnetToMouse();
    }

    public void NotifyCollected(IronSandGrain grain, IronSandMagnet collectedBy)
    {
        if (grain != null)
        {
            CollectIronSand(grain.gameObject);
        }
    }

    public void CollectIronSand(GameObject sandObject)
    {
        if (cleared || sandObject == null)
        {
            return;
        }

        IronSandPiece ironSand = sandObject.GetComponent<IronSandPiece>();
        if (ironSand == null || ironSand.IsCollected || !ironSands.Contains(ironSand))
        {
            return;
        }

        ironSand.Collect();
        collectedCount++;

        if (seSource != null && collectClip != null)
        {
            seSource.PlayOneShot(collectClip);
        }

        if (collectParticle != null)
        {
            collectParticle.transform.position = sandObject.transform.position;
            collectParticle.Play();
        }

        if (magnetObject != null)
        {
            sandObject.transform.SetParent(magnetObject.transform, true);
            Vector3 localPosition = sandObject.transform.localPosition;
            float halfAttachLineLength = attachGizmoWidth * 0.5f;
            float perpendicularNoise = Random.Range(-attachPerpendicularNoise, attachPerpendicularNoise);
            if (attachAxis == AttachAxis.X)
            {
                localPosition.x = attachLocalY + perpendicularNoise;
                localPosition.y = Mathf.Clamp(localPosition.y, -halfAttachLineLength, halfAttachLineLength);
            }
            else
            {
                localPosition.x = Mathf.Clamp(localPosition.x, -halfAttachLineLength, halfAttachLineLength);
                localPosition.y = attachLocalY + perpendicularNoise;
            }
            sandObject.transform.localPosition = localPosition;
        }
        else if (collectorRoot != null)
        {
            sandObject.transform.SetParent(collectorRoot, true);
        }

        UpdateDebugText();

        if (collectedCount >= ironSands.Count)
        {
            cleared = true;
            GameManager.Clear();
        }
    }

    private void EnsureSpawnArea()
    {
        if (spawnArea != null || !createMissingObjects)
        {
            return;
        }

        GameObject areaObject = new GameObject("IronSand Spawn Area");
        areaObject.transform.SetParent(transform, false);
        spawnArea = areaObject.AddComponent<BoxCollider2D>();
        spawnArea.isTrigger = true;
        spawnArea.size = defaultSpawnAreaSize;
    }

    private void EnsureMagnet()
    {
        if (magnetObject == null)
        {
            Debug.LogError("magnetObject is not assigned.", this);
            return;
        }

        IronSandMagnet oldMagnet = magnetObject.GetComponent<IronSandMagnet>();
        if (oldMagnet != null)
        {
            oldMagnet.enabled = false;
        }

        SpriteRenderer spriteRenderer = magnetObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = magnetObject.AddComponent<SpriteRenderer>();
        }
        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = IronSandRuntimeSprites.GetBarMagnetSprite();
            spriteRenderer.sortingOrder = 10;
        }

        Rigidbody2D magnetRigidbody = magnetObject.GetComponent<Rigidbody2D>();
        if (magnetRigidbody == null)
        {
            magnetRigidbody = magnetObject.AddComponent<Rigidbody2D>();
        }
        magnetRigidbody.bodyType = RigidbodyType2D.Kinematic;
        magnetRigidbody.gravityScale = 0f;
        magnetRigidbody.useFullKinematicContacts = true;
    }

    private void EnsureCollector()
    {
        if (collectorCollider == null && magnetObject != null)
        {
            collectorCollider = FindCollectorColliderInMagnetChildren();
        }

        if (collectorCollider == null)
        {
            Debug.LogError("collectorCollider is not assigned. Add a child object with BoxCollider2D under the magnet.", this);
            return;
        }

        collectorCollider.isTrigger = true;
        collectorRoot = collectorCollider.transform;

        IronSandCollector collector = collectorCollider.GetComponent<IronSandCollector>();
        if (collector == null)
        {
            collector = collectorCollider.gameObject.AddComponent<IronSandCollector>();
        }
        collector.Initialize(this);
    }

    private BoxCollider2D FindCollectorColliderInMagnetChildren()
    {
        BoxCollider2D[] colliders = magnetObject.GetComponentsInChildren<BoxCollider2D>(true);
        foreach (BoxCollider2D collider in colliders)
        {
            if (collider.gameObject != magnetObject)
            {
                return collider;
            }
        }

        return null;
    }

    private void MoveMagnetToMouse()
    {
        if (targetCamera == null || magnetObject == null)
        {
            return;
        }

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Mathf.Abs(targetCamera.transform.position.z - magnetObject.transform.position.z);
        Vector3 worldPosition = targetCamera.ScreenToWorldPoint(mousePosition);
        worldPosition.z = magnetObject.transform.position.z;
        magnetObject.transform.position = worldPosition;
    }

    private void EnsureAudioSource()
    {
        if (seSource == null)
        {
            seSource = GetComponent<AudioSource>();
        }
    }

    private void SpawnIronSands()
    {
        ironSands.Clear();
        collectedCount = 0;

        if (ironSandPrefab == null)
        {
            Debug.LogError("ironSandPrefab is not assigned.", this);
            return;
        }

        for (int i = 0; i < Mathf.Max(0, sandCount); i++)
        {
            GameObject sandObject = CreateIronSandObject(i);
            sandObject.transform.position = GetRandomSpawnPosition();
            sandObject.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            sandObject.transform.localScale = Vector3.one * Random.Range(sandScaleRange.x, sandScaleRange.y);

            if (sandParent != null)
            {
                sandObject.transform.SetParent(sandParent, true);
            }

            IronSandPiece ironSand = sandObject.GetComponent<IronSandPiece>();
            if (ironSand == null)
            {
                ironSand = sandObject.AddComponent<IronSandPiece>();
            }
            ironSand.Initialize();
            ironSands.Add(ironSand);
        }
    }

    private GameObject CreateIronSandObject(int index)
    {
        GameObject sandObject = Instantiate(ironSandPrefab);
        sandObject.name = "Iron Sand " + (index + 1).ToString("00");

        SpriteRenderer spriteRenderer = sandObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = IronSandRuntimeSprites.GetCircleSprite();
            spriteRenderer.color = fallbackSandColor;
            spriteRenderer.sortingOrder = 5;
        }

        BoxCollider2D sandCollider = sandObject.GetComponent<BoxCollider2D>();
        if (sandCollider == null)
        {
            sandCollider = sandObject.AddComponent<BoxCollider2D>();
        }
        sandCollider.isTrigger = true;

        Rigidbody2D sandRigidbody = sandObject.GetComponent<Rigidbody2D>();
        if (sandRigidbody == null)
        {
            sandRigidbody = sandObject.AddComponent<Rigidbody2D>();
        }
        sandRigidbody.bodyType = RigidbodyType2D.Kinematic;
        sandRigidbody.gravityScale = 0f;
        sandRigidbody.useFullKinematicContacts = true;

        return sandObject;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        if (spawnArea == null)
        {
            return new Vector3(Random.Range(-5f, 5f), Random.Range(-2f, 2f), sandZ);
        }

        Bounds bounds = spawnArea.bounds;
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            sandZ
        );
    }

    private void UpdateDebugText()
    {
        if (debugText != null)
        {
            debugText.text = collectedCount + " / " + ironSands.Count;
        }
    }

    private void OnDrawGizmos()
    {
        if (magnetObject == null)
        {
            return;
        }

        Gizmos.color = attachGizmoColor;

        if (attachAxis == AttachAxis.X)
        {
            Vector3 bottom = magnetObject.transform.TransformPoint(new Vector3(attachLocalY, -attachGizmoWidth * 0.5f, 0f));
            Vector3 top = magnetObject.transform.TransformPoint(new Vector3(attachLocalY, attachGizmoWidth * 0.5f, 0f));
            Gizmos.DrawLine(bottom, top);
            Gizmos.DrawWireSphere(bottom, 0.08f);
            Gizmos.DrawWireSphere(top, 0.08f);
        }
        else
        {
            Vector3 left = magnetObject.transform.TransformPoint(new Vector3(-attachGizmoWidth * 0.5f, attachLocalY, 0f));
            Vector3 right = magnetObject.transform.TransformPoint(new Vector3(attachGizmoWidth * 0.5f, attachLocalY, 0f));
            Gizmos.DrawLine(left, right);
            Gizmos.DrawWireSphere(left, 0.08f);
            Gizmos.DrawWireSphere(right, 0.08f);
        }
    }

}

sealed class IronSandCollector : MonoBehaviour
{
    private GameManager_IronSand gameManager;

    public void Initialize(GameManager_IronSand owner)
    {
        gameManager = owner;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        gameManager?.CollectIronSand(other.gameObject);
    }
}

sealed class IronSandPiece : MonoBehaviour
{
    private Collider2D sandCollider;
    private Rigidbody2D sandRigidbody;

    public bool IsCollected { get; private set; }

    public void Initialize()
    {
        IsCollected = false;
        sandCollider = GetComponent<Collider2D>();
        sandRigidbody = GetComponent<Rigidbody2D>();
    }

    public void Collect()
    {
        if (IsCollected)
        {
            return;
        }

        IsCollected = true;

        if (sandCollider != null)
        {
            sandCollider.enabled = false;
        }

        if (sandRigidbody != null)
        {
            sandRigidbody.simulated = false;
        }
    }
}
