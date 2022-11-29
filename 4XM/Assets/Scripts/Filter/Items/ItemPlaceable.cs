using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(RectTransform), typeof(SpriteRenderer))]
public class ItemPlaceable : MonoBehaviour
{
	public static GameObject Create(Item item)
	{
		GameObject gameObject = new GameObject("Placeable Item", typeof(RectTransform), typeof(SpriteRenderer), typeof(ItemPlaceable));
		gameObject.GetComponent<ItemPlaceable>().item = item;
		return gameObject;
	}

	public Item item;

	private bool isPlaced;

	private SpriteRenderer spriteRenderer;

	private Camera mainCamera;
	private Tilemap terrainTilemap;
	private Vector3Int lastPosition;

	private void Start()
	{
		isPlaced = false;

		spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.sprite = item.placedSprite;
		spriteRenderer.sortingLayerName = "MovingObjects";
		spriteRenderer.sortingOrder = 1;

		mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
		terrainTilemap = GameObject.FindWithTag("Terrain Tilemap").GetComponent<Tilemap>();

		lastPosition = Vector3Int.zero;

		UpdateItemPreview();

		//TODO(Guillem): Add here logic to apply a restricted set of controls.
		//The player should be unable to use the inventory or any other items/weapons while trying to place an item.
	}

	private void Update()
	{
		if (isPlaced == false)
		{
			//Logic for handling the GameObject preview before placing
			UpdateItemPreview();
		}
		else
		{
			//Logic for handling the GameObject once it has been placed
			UpdatePlacedItem();
		}
	}

	private void UpdateItemPreview()
	{
		//TODO(Guillem): Call input manager for the "quit" key to remove this gameobject, effectively cancelling the preview

		var mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
		var cellPosition = terrainTilemap.WorldToCell(mousePosition);
		//TODO(Guillem): Add logic to prevent displaying the preview and placing the item too far away.
		//Note(Guillem): Maybe we don't actually need a "range" and we can place items anywhere we can see on screen. Could add a check to see if mouse is in the screen.
		if (cellPosition == lastPosition)
		{
			return;
		}
		else
		{
			lastPosition = cellPosition;
		}
		transform.position = terrainTilemap.GetCellCenterWorld(cellPosition);

		var targetTile = (ScriptableAnimatedTile)terrainTilemap.GetTile(cellPosition);
		if (targetTile.CanItemBePlaced == true)
		{
			spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
		}
		else
		{
			spriteRenderer.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
		}
	}

	private void UpdatePlacedItem()
	{

	}

	private void OnPlace()
	{
		spriteRenderer.color = Color.white;

		//TODO(Guillem): Add here logic to revert the restriction of controls

		isPlaced = true;
	}
}
