using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Goroh.RotationGame
{
    public class Deco : MonoBehaviour
    {
        [SerializeField] List<GameObject> _decorationPrefabs;
        [SerializeField] Score _score;
        public void Start()
        {
            GameObject prefab = _decorationPrefabs[Random.Range(0, _decorationPrefabs.Count)];
            IRotationgameDecoration deco = Instantiate(prefab).GetComponent<IRotationgameDecoration>();
            deco.Init(_score);
        }
    }
}
