using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraBounds : MonoBehaviour
{
	private PolygonCollider2D polygonCollider2D;
	private Tilemap terrainTilemap;

	private void Start()
	{
		var terrainTilemapGameObject = GameObject.FindWithTag("Terrain Tilemap");
		transform.position = terrainTilemapGameObject.transform.position;
		terrainTilemap = terrainTilemapGameObject.GetComponent<Tilemap>();

		terrainTilemap.CompressBounds();

		polygonCollider2D = GetComponent<PolygonCollider2D>();
		Vector2 topRight = terrainTilemap.transform.TransformPoint(terrainTilemap.localBounds.max);
		Vector2 bottomLeft = terrainTilemap.transform.TransformPoint(terrainTilemap.localBounds.min);
		Vector2 topLeft = new Vector2(bottomLeft.x, topRight.y);
		Vector2 bottomRight = new Vector2(topRight.x, bottomLeft.y);
		Vector2[] colliderBounds = new Vector2[4]
		{
			topRight,
			bottomRight,
			bottomLeft,
			topLeft,
		};
		polygonCollider2D.points = colliderBounds;
	}
}
