using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfusionMatrix : MonoBehaviour
{
    public List<Button> matrix = new List<Button>();
    public TextMeshProUGUI maxValueLabel;
    public Gradient colorRange;
   
    public void UpdateMatrix(List<int> groundTruth, List<int> actual)
    {
        int[,] m = new int[3,3];
        //for each of the 9 cells, add result
        for (int i = 0; i < groundTruth.Count; i++)
        {
            int g = groundTruth[i];
            int a = actual[i];
            m[g, a]++;
        }

        //update numbers in matrix labels 
        int index = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                matrix[index].GetComponentInChildren<TextMeshProUGUI>().text = m[i,j].ToString();
                index++;
            }
        }
       
        //find maxValue and update gradient 
        float maxValue = 0;
        for (int i = 0; i < matrix.Count; i++)
        {
            var v= int.Parse(matrix[i].GetComponentInChildren<TextMeshProUGUI>().text);
            if (v > maxValue)
                maxValue = v;
        }

        maxValueLabel.text = maxValue.ToString();

        //update colors in matrix
        for (int i = 0; i < matrix.Count; i++)
        {
            var v = int.Parse(matrix[i].GetComponentInChildren<TextMeshProUGUI>().text);
            float percentage = v / maxValue;

            var colors = matrix[i].colors;
            colors.normalColor = colorRange.Evaluate(percentage);
            matrix[i].colors = colors;
        }
    }
}
