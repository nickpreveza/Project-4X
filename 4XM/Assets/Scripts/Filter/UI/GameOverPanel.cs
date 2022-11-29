using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class GameOverPanel : UIPanel
{
    [SerializeField] Volume globalVolume;
    DepthOfField dof;
    ColorAdjustments colorAdj;
    [SerializeField] Gradient colorChange;
    float colorTimer;
    bool postSetup;
    Color ColorFromGradient(float value)  // float between 0-1
    {
        return colorChange.Evaluate(value);
    }
    void Start()
    {
        if (HB_UIManager.Instance != null)
        {
            HB_UIManager.Instance.gameOverPanel = this;
            HB_UIManager.Instance.AddPanel(this);
        }

        globalVolume = HB_GameManager.Instance.globalVolume;

       
    }

    void PostSetup()
    {
        /*
        DepthOfField _dof;
        if (globalVolume.profile.TryGet<DepthOfField>(out _dof))
        {
            dof = _dof;
            dof.active = true;
        }

        ColorAdjustments _color;
        if (globalVolume.profile.TryGet<ColorAdjustments>(out _color))
        {
            colorAdj = _color;
            colorAdj.active = true;
        } */
    }

    public override void Activate()
    {
        if (!postSetup)
        {
            //PostSetup();
        }
        //dof.active = true;

        base.Activate();
    }

    public override void Disable()
    {
        if (!postSetup)
        {
            PostSetup();
        }

        if (dof != null)
        {
            dof.active = false;
        }
       
        base.Disable();
    }

    public override void Setup()
    {
        base.Setup();
    }

    public void Update()
    {
        if (this.isActive)
        {
            colorTimer += Time.deltaTime;
            colorTimer = Mathf.Clamp(colorTimer, 0, 1);

            if (colorTimer < 1)
            {
                //colorAdj.colorFilter.value = ColorFromGradient(colorTimer);
            }
          
        }
    }

    public void ActionStartover()
    {
        HB_GameManager.Instance.ReloadScene();    
    }
}
