using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Reflection;
/// <summary>
/// Instructions:
/// <list type="bullet">
/// <item>Attach this script to a standalone tilemap inside a grid, which should contain only fog tiles</item>
/// <item>Tag the tilemap with the "Fog" tag.</item>
/// <item>Adjust the center of defogging from the inspector</item>
/// <item>Reference this script to make use of DefogPercent and IsTileFogged().</item>
/// </list>
/// </summary>
[ExecuteInEditMode]
public class Fog : MonoBehaviour
{
	private Tilemap tilemap;

	[Range(0, 100)][SerializeField]
	float defogPercent;
	/// <summary>
	/// Add to, or remove from this property to increase or decrease the fog cleared.
	/// Should not be used in Update loops, since it refreshes all tiles in the tilemap
	/// </summary>
	
	public float DefogPercent
	{
		get
		{
			return defogPercent;
		}
		set
		{
			defogPercent = value;
			defogPercent = Mathf.Clamp(defogPercent, 0.0f, 100.0f);
			UpdateTiles();
		}
	}

	[SerializeField] private Vector2Int defogCellOrigin;
	private float defogUnit;

	private void Start()
	{
		defogPercent = 0.0f;
	}

	private void Awake()
	{
		tilemap = GetComponent<Tilemap>();
		InitializeDefogUnit();
	}

	/// <summary>
	/// This function calculates how much distance 1% of defogging should be.
	/// </summary>
	private void InitializeDefogUnit()
	{
		var greaterSize = Mathf.Max(tilemap.size.x, tilemap.size.y);
		defogUnit = greaterSize / 100.0f;
	}

	private void UpdateTiles()
	{
		for (int i = tilemap.cellBounds.min.x; i < tilemap.cellBounds.max.x; i++)
		{
			for (int j = tilemap.cellBounds.min.y; j < tilemap.cellBounds.max.y; j++)
			{
				if (Vector3Int.Distance((Vector3Int)defogCellOrigin, new Vector3Int(i, j)) <= DefogPercent * defogUnit)
				{
					DefogTile(new Vector3Int(i, j));
				}
				else
				{
					FogTile(new Vector3Int(i, j));
				}
			}
		}
	}

	private void DefogTile(Vector3Int tilePosition, bool defogPermanently = true)
	{
		var tileColor = tilemap.GetColor(tilePosition);
		if (tileColor.a <= 0.0)
		{
			//Tile is already defogged
			return;
		}

		//Unlock the tile color
		var tileFlags = tilemap.GetTileFlags(tilePosition);
		tileFlags &= ~TileFlags.LockColor;
		tilemap.SetTileFlags(tilePosition, tileFlags);
		//Set the alpha channel of the tile to 0 so it "defogs"
		tileColor.a = 0;
		tilemap.SetColor(tilePosition, tileColor);
		tilemap.SetTile(tilePosition, null);
		//Lock the tile color again if defogPermanently is enabled
		if (defogPermanently == true)
		{
			tileFlags |= TileFlags.LockColor;
			tilemap.SetTileFlags(tilePosition, tileFlags);
		}
	}

	private void FogTile(Vector3Int tilePosition)
	{
		var tileColor = tilemap.GetColor(tilePosition);
		if (tileColor.a > 0.0f)
		{
			//Tile is already fogged
			return;
		}

		//Note(Guillem): The color flag is left untouched here, so that only tiles with unlocked color flag can be "fogged" again.
		tileColor.a = 1.0f;
		tilemap.SetColor(tilePosition, tileColor);
	}

	public bool IsTileFogged(Vector3 position)
	{
		var tilePosition = tilemap.WorldToCell(position);
		var tileColor = tilemap.GetColor(tilePosition);
		if (tileColor.a <= 0.0f)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
}
