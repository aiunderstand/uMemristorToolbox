using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ExperimentManager;

public class UIPanel : MonoBehaviour
{
    public Experiments experimentPanel;

    // Start is called before the first frame update
    void Awake()
    {
        UIManager.Panels.Add(experimentPanel, this.gameObject);
    }

    public void Show()
    {
        //get Panel object containing UI
        transform.GetChild(0).gameObject.SetActive(true);
    }

    public void Hide()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }
}
