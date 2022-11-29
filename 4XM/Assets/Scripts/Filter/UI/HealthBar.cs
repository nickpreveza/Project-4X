using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This implementation makes my skin crawl. - Nick 
/// </summary>
public class HealthBar : MonoBehaviour
{

    Image image;
    [SerializeField] GameObject leaf;
    Animator leafAnimator;
    [SerializeField] Sprite[] sprites = new Sprite[10];
    [SerializeField] float maxValue = 100;
    [SerializeField] float currentValue = 100; //Really doesn't work with anything else 
    //AnimationFlag animFlag;
    bool animationPlaying;

    bool hasBeenSetup;
    private void Awake()
    {
        //animFlag = GetComponentInChildren<AnimationFlag>();
        leafAnimator = leaf.GetComponent<Animator>();
        leaf.SetActive(false);
        image = GetComponent<Image>();
    }

    public void UpdateHealthbar(float newValue, bool skipAnimation = false)
    {
        if (newValue == currentValue) { return; }

        bool throwLeaf = false;

        if (newValue < currentValue && !skipAnimation)
        {
            throwLeaf = true;
        }

        currentValue = newValue;
        int spriteIndex = (int)currentValue;
        spriteIndex = Mathf.Clamp(spriteIndex, 0, 10);
        UpdateSprite(sprites[spriteIndex], throwLeaf);
    }

    private void UpdateSprite(Sprite newSprite, bool throwLeaf)
    {
        if (!hasBeenSetup)
        {
            //animFlag = GetComponentInChildren<AnimationFlag>();
            leafAnimator = leaf.GetComponent<Animator>();
            leaf.SetActive(false);
            image = GetComponent<Image>();

            hasBeenSetup = true;
        }

        if (image.sprite != newSprite)
        {
            image.sprite = newSprite;

            if (throwLeaf)
            {
                StopAllCoroutines();
                leaf.SetActive(true);
                leafAnimator.Play("LeafFalling");
                StartCoroutine(DisableLeaf());
            }
          
        }
    }

    IEnumerator DisableLeaf()
    {
        yield return new WaitForSeconds(3f);
        leaf.SetActive(false);
    }
}
