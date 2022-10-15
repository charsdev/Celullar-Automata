using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace WorldGenerator
{
	public class World : MonoBehaviour
	{
		private const short _dungeonWidth = 32;
		private const short _dungeonHeight = 32;
		private Dictionary<CelullarAutomata.TileType, GameObject> prefabs;
		private const float _tileSize = 0.64f;
		[SerializeField] private GameObject _floorTilePrefab;
		[SerializeField] private GameObject _wallTilePrefab;
		[SerializeField] private GameObject _borderTilerPrefab;
		private List<GameObject> _tiles = new List<GameObject>();

		private void Awake()
		{
			prefabs = new Dictionary<CelullarAutomata.TileType, GameObject>();
			prefabs.Add(CelullarAutomata.TileType.Floor, _floorTilePrefab);
			prefabs.Add(CelullarAutomata.TileType.Wall, _wallTilePrefab);
			prefabs.Add(CelullarAutomata.TileType.Border ,_borderTilerPrefab);

			CelullarAutomata.Instance.Width = _dungeonWidth;
			CelullarAutomata.Instance.Height = _dungeonHeight;
			CelullarAutomata.Instance.Map = new CelullarAutomata.TileType[
				CelullarAutomata.Instance.Width,
				CelullarAutomata.Instance.Height
			];

			CelullarAutomata.Instance.MakeMap();
			PutTiles();
		}

        private void Update()
        {
			if (_tiles.Count == 0)
				return;

			if (Input.GetKeyDown(KeyCode.Return))
            {
                DestroyWorld();
				CelullarAutomata.Instance.MakeMap();
				PutTiles();
			}
        }

		private void PutTiles()
		{
			for (int x = 0; x < CelullarAutomata.Instance.Width; x++)
			{
				for (int y = 0; y < CelullarAutomata.Instance.Height; y++)
				{
					CelullarAutomata.TileType type = CelullarAutomata.Instance.Map[x, y];
					GameObject tileObject = Instantiate(prefabs[type]);
					tileObject.transform.position = new Vector2(_tileSize * x, _tileSize * y);
					tileObject.transform.SetParent(transform);
					_tiles.Add(tileObject);
				}
			}
		}

		private void DestroyWorld()
        {
            foreach (var tile in _tiles)
            {
				Destroy(tile);
            }
		}
	}
}
