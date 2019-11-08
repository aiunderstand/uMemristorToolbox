using System;
using System.Collections;
using System.Collections.Generic;
using Cyotek.Collections.Generic;
using TMPro;
using UnityEngine;

public class Graph : MonoBehaviour
{
    CircularBuffer<double> bufferLine1;
    CircularBuffer<double> bufferLine2;
    LineRenderer line1;
    LineRenderer line2;
    Rect drawingBox;
    float deltaX; // scaled x offset for datapoint;
    double scaleFactor; // scale factor for datapoint in Y dimension;
    float middleY;
    public void Awake()
    {
        var lines = GetComponentsInChildren<LineRenderer>();

        line1 = lines[0];
        line2 = lines[1];

        //compute plotting dimensions
        var p1 = line1.GetPosition(0); //right point (eg: 2.3, 4.016)
        var p2 = line1.GetPosition(1); //left point(eg: 6.85, 3.208)

        drawingBox = new Rect(p1.x, p1.y, p2.x-p1.x, p2.y-p1.y); //Use box dimensions and bufferLength to scale datapoints
        middleY = drawingBox.yMax + ((drawingBox.yMin - drawingBox.yMax) / 2);

        //reset graph to zero data
        line1.positionCount = 0;
        line2.positionCount = 0;
    }

    public void Plot()
    {
        if (bufferLine1 != null)
        {
            var graphData = bufferLine1.ToArray();
            for (int i = 0; i < bufferLine1.Capacity; i++)
            {
                var x = drawingBox.xMin + (i * deltaX);

                var y = middleY - ScaleHeight(i < graphData.Length ? graphData[i] : 0);
                y = CheckOutOfBounds(y);
                line1.SetPosition(i, new Vector3(x, y, 0));
            }
        }

        if (bufferLine2 != null)
        {
            var graphData = bufferLine2.ToArray();
            for (int i = 0; i < bufferLine2.Capacity; i++)
            {
                var x = drawingBox.xMin + (i * deltaX);

                var y = middleY - ScaleHeight(i < graphData.Length ? graphData[i] : 0);
                y = CheckOutOfBounds(y);
                line2.SetPosition(i, new Vector3(x, y, 0));
            }
        }
    }

    private float CheckOutOfBounds(float y)
    {
        if (y > drawingBox.yMin)
            return drawingBox.yMin + 0.1f;

        if (y < drawingBox.yMax)
            return drawingBox.yMax - 0.1f;

        return y;

    }

    private float ScaleHeight(double v)
    {
        return (float) (scaleFactor* v);
    }

    public void AttachTo(CircularBuffer<double> graphBuffer, int lineId)
    {
        switch(lineId)
        {
            case 0:
                bufferLine1 = graphBuffer;
                line1.positionCount = bufferLine1.Capacity;
                break;
            case 1:
                bufferLine2 = graphBuffer;
                line2.positionCount = bufferLine2.Capacity;
                break;
        }
       
        //will get overwritten, potential bug in the future if buffers have differen sizes
        deltaX = drawingBox.width / graphBuffer.Capacity;
        scaleFactor = drawingBox.height / (2*Math.PI);        
    }

    public void Init(string title,string horizontalAxis, string verticalAxis, string line1Name = "", string line2Name ="")
    {
        var textElements = GetComponentsInChildren<TextMeshProUGUI>();

        foreach (var e in textElements)
        {
            switch (e.name)
            {
                case ("Title"):
                    e.text = title;
                    break;
                case ("Horizontal Axis"):
                    e.text = horizontalAxis;
                    break;
                case ("Vertical Axis"):
                    e.text = verticalAxis;
                    break;
                case ("Line1Name"):
                    if (line1Name == "")
                        e.gameObject.SetActive(false);
                    else
                        e.text = line1Name;
                    break;
                case ("Line2Name"):
                    if (line2Name == "")
                        e.gameObject.SetActive(false);
                    else
                        e.text = line2Name;
                    break;
                default:
                    break;
            }
        }
    }
}
