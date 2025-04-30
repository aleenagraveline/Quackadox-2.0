using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_TitleScreen : MonoBehaviour
{
    public GameObject panel;
    public bool panelStatus;
    public Animator PanelAnimator;


    public void Start()
    {
        panel.SetActive(false);
        panelStatus = false;
    }

    public void OnClickforControls()
    {
        Debug.Log("clicked");
        if (!panelStatus)
        {
            panel.SetActive(true);
            panelStatus = true;
            PanelAnimator.SetTrigger("PanelIn");
            //PanelSlideIn.Play();
            Debug.Log("con");
        }
        else if (panelStatus)
        {
            //PanelSlideOut.Play();
            PanelAnimator.SetTrigger("PanelOut");
            //panel.SetActive(false);
            panelStatus = false;
        }
    }

    public void Disable()
    {
        panel.SetActive(false);
    }
}
