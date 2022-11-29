using System.Linq;
using UnityEngine;

public class HempPlant : ItemInteractable, IDamagable
{
	private const float GROWTH_DURATION = 60.0f; //In seconds
	private const float HB_POINT_GAIN_INTERVAL = 10.0f; //In seconds
	private const float GROWTH_CHECK_INTERVAL = 0.25f;  //In seconds
	private const int MAX_HEALTH = 1000;

	[Header("Plant-specific data")]
	[SerializeField, Range(0.0f, 100.0f)] private float growthPercent;
	[SerializeField] private int health;
	public bool overrideInteractable;
	public int Health
	{
		get
		{
			return health;
		}
		set
		{
			health = Mathf.Clamp(value, 0, MAX_HEALTH);
			if (health <= 0)
			{
				OnDeath();
			}
		}
	}

	[SerializeField] private Sprite[] hempPlantSprites;
	private SpriteRenderer spriteRenderer;
	private int totalHempPlantSprites;

	private bool isPlanted;

	private Fog fog;
	private uint fogClearingStage;
	[SerializeField, Range(0.0f, 100.0f)] private float fogClearedPerStage = 1.0f; //In percentage
	[SerializeField, Range(0.0f, 100.0f)] private float growthPerFogStage = 25.0f; //In percentage

    private void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (!overrideInteractable)
        {
			ResetHempPlant();
		}
        else
        {
			spriteRenderer.sprite = null;
			isPlanted = false;
			growthPercent = 0.0f;
			health = 1000;
			fogClearingStage = 0;
			CancelInvoke("Grow");
			CancelInvoke("GainHBPoint");
		}
		
	}

	public void ResetHempPlant()
	{
		spriteRenderer.sprite = null;
		isPlanted = false;
		isInteractable = true;
		growthPercent = 0.0f;
		health = 1000;
		fogClearingStage = 0;
		CancelInvoke("Grow");
		CancelInvoke("GainHBPoint");
	}

	private void Awake()
	{
		fog = GameObject.FindWithTag("Fog").GetComponent<Fog>();

		totalHempPlantSprites = hempPlantSprites.Count();

		if (isPlanted == true)
		{
			isInteractable = false;
			InvokeRepeating("Grow", 0, GROWTH_CHECK_INTERVAL);
		}
	}

	public override void Interact()
	{
		if (isPlanted == false)
		{
			if (ItemManager.Instance.DoesPlayerHaveItem("hempseeds", 1))
			{
				isInteractable = false;
				HB_EventManager.Instance.OnHempPlanted();
				StartGrowth();
				ItemManager.Instance.UseItem("hempseeds", 1, false);
			}
			else
			{
				return;
			}
		}
		else
		{
			if (growthPercent < 100.0f)
			{
				return;
			}
			else
			{
				OnHarvest();
			}
		}
	}

	private void Update()
	{
		if (isPlanted == true)
		{
			//Growth logic
			if (growthPercent < 100.0f)
			{
				growthPercent += Time.deltaTime * 100.0f / GROWTH_DURATION; //Converting seconds into percentage
			}
			else if (growthPercent > 100.0f)
			{
				growthPercent = 100.0f;
				isInteractable = true;
			}

			//Fog clearing logic
			bool hasAdvancedFogClearingStage = growthPercent > (growthPerFogStage * (fogClearingStage + 1));
			if (hasAdvancedFogClearingStage == true)
			{
				fog.DefogPercent += fogClearedPerStage;
				fogClearingStage++;
			}
		}
	}

	private void StartGrowth()
	{
		isPlanted = true;
		isInteractable = false;
		InvokeRepeating("Grow", 0, GROWTH_CHECK_INTERVAL);
		InvokeRepeating("GainHBPoint", 0, HB_POINT_GAIN_INTERVAL);
	}

	private void Grow()
	{
		if (growthPercent >= 100.0f)
		{
			CancelInvoke("Grow");
		}

		int growthStage = Mathf.RoundToInt(growthPercent * (totalHempPlantSprites - 1) / 100.0f);	//Convert percentage into the appropriate "growth stage"
		growthStage = Mathf.Clamp(growthStage, 0, totalHempPlantSprites - 1);	//Clamp it between 0 and the sprites count minus one

		spriteRenderer.sprite = hempPlantSprites[growthStage];
	}

	public void OnDamage(int amount)
	{
		Health -= amount;
	}

	private void OnDeath()
	{
		ResetHempPlant();
	}

	private void OnHarvest()
	{
		SpawnDrops();
		ResetHempPlant();
	}

	public void SpawnDrops()
    {
		Instantiate(ItemManager.Instance.GetHempDrop(), transform.position, Quaternion.identity);
    }

	private void GainHBPoint()
	{
		if (growthPercent < 100.0f)
		{
			HB_GameManager.Instance.RewardScore(1);
		}
		else
		{
			CancelInvoke("GainHBPoint");
			return;
		}
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		Gizmos.DrawSphere(transform.position + new Vector3(0.0f, -0.5f), 0.2f);
	}
#endif
}
