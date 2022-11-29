using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Tilemaps;
#endif

[CreateAssetMenu(fileName = "Scriptable Animated Tile", menuName = "Scriptable Tiles/Animated Tile", order = 1)]
public class ScriptableAnimatedTile : AnimatedTile
{
	[field: SerializeField]	public bool CanItemBePlaced { get; private set; }

#if UNITY_EDITOR
	[ContextMenu("Change filename to Sprite name")]
	private void FileNameToSpriteName()
	{
		string assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
		if (m_AnimatedSprites != null)
		{
			var name = m_AnimatedSprites[0].name;
			name = name.Substring(0, name.Length - 2);

			AssetDatabase.RenameAsset(assetPath, name + " Tile");
		}
		else
		{
			Debug.Log("No sprite could be found");
		}
	}

	[CreateTileFromPalette]
	public static TileBase ScriptableTile(Sprite sprite)
	{
		var tile = CreateInstance<ScriptableAnimatedTile>();
		tile.m_AnimatedSprites = new Sprite[1] { sprite };
		tile.name = sprite.name;
		return tile;
	}
#endif
}
