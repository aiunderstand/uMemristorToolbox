using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LineGraphContinuous2D : MonoBehaviour
{
    //16 optimal colors http://alumni.media.mit.edu/~wad/color/numbers.html
    public static Color32[] ColorPalette16 = {
        new Color32 (0,0,0,255),     //black
        new Color32 (87,87,87,255),  //dk. gray
        new Color32 (173,35,35,255), //red
        new Color32 (42,75,215,255), //blue
        new Color32 (29,105,20,255), //green
        new Color32 (129,74,25,255), //brown
        new Color32 (129,38,192,255), //purple
        new Color32 (160,160,160,255), //lt. gray
        new Color32 (129,197,122,255), //lt. green
        new Color32 (157,175,255,255), //lt. blue
        new Color32 (41,208,208,255), //cyan
        new Color32 (255,146,51,255), //orange
        new Color32 (255,238,51,255), //yellow
        new Color32 (233,222,187,255), //tan
        new Color32 (255,205,243,255), //pink
        new Color32 (227,255,0,255) //lt. lt. green (was white, but using white bg)
    };


    public TextMeshProUGUI Title;
    public TextMeshProUGUI Subtitle;
    public TextMeshProUGUI HorizontalLabel;
    public TextMeshProUGUI VerticalLabel;
    public LineRenderer[] Lines; //contains data points,line settings and color
    public TextMeshProUGUI[] LineLabel; //legend item with reference to color
    public bool setLoopLine = true;

    private static float deltaX = 800 / 16;
    private static float deltaY = 600 / 17; //in a refactor this should be an even number
    private static float offsetX = 850; //in a refactor this should be 0
    private static float offsetY = 600; //in a refactor this should be 0
    private static float rangeXmin = -2;
    private static float rangeXmax = 2;
    private static float rangeYmin = -50;
    private static float rangeYmax = 120;


    private List<List<Vector2>> LineData = new List<List<Vector2>>();  
    public void Awake()
    {
        //for every line we need to:
        //-clear data in line renderer
        //-set data reference
        //-set color of line
        //-set color of line label
        //-set loop

        for (int i = 0; i < Lines.Length; i++)
        {
            Lines[i].positionCount = 0; //clear data

            var data = new List<Vector2>();
            LineData.Add(data); //set data reference

            Lines[i].material.color = ColorPalette16[i]; //set color of line

            LineLabel[i].GetComponentInChildren<Image>().color = Lines[i].material.color; //set color of label

            Lines[i].loop = setLoopLine;
        }
    }
    
    public void Init(string title, string subtitle, string horizontalLabel, string verticalLabel, string[] lineLabels) {
        Title.text = title;
        Subtitle.text = subtitle;
        HorizontalLabel.text = horizontalLabel;
        VerticalLabel.text = verticalLabel;

        for (int i = 0; i < lineLabels.Length; i++)
        {
            LineLabel[i].text = lineLabels[i];
        }

    }

    public void AddDataPointToLine(int index, Vector2 data)
    {
        LineData[index].Add(data);
        Lines[index].positionCount++;

        var dataPoint = ConvertDataPointToGraphPoint(data);

        Lines[index].SetPosition(Lines[index].positionCount-1, dataPoint);
    }

    public static Vector3 ConvertDataPointToGraphPoint(Vector2 data)
    {
        //convert from range -2 to 2 to range 16 to 0 , by shifting to 0-4 range, multiply to 0-16 range and inverting 
        var x = 16 - (data.x + Mathf.Abs(rangeXmin)) *4;

        //convert from range -50 to 120 to range 0 to 17 , by shifting to 0-170, divide to 0-17 range 
        var y = (data.y + Mathf.Abs(rangeYmin)) /10;

        var xGraph = offsetX - (x*deltaX);
        var yGraph = offsetY - (y*deltaY);

        Vector3 result = new Vector3(xGraph,yGraph,0);
        
        return result;
    }

    public void ClearAll()
    {
        for (int i = 0; i < LineData.Count; i++)
        {
            LineData[i].Clear();
            Lines[i].positionCount = 0;
        }
    }
}
