using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : ScriptableObject
{
    public int BaseSize = 4;
    public int Height = 4;
    public float autoDelayTime = 0.5f;
    public float LevelTime;

    private void OnValidate()
    {
        BaseSize = Mathf.Max(1, BaseSize);

        // Auto-calculate Height so total cells is divisible by 3
        Height = CalculateValidHeight(BaseSize);
    }

    private int CalculateTotalCells(int baseSize, int height)
    {
        int total = 0;
        for (int i = 0; i < height; i++)
        {
            int size = baseSize - i;
            total += size * size;
        }
        return total;
    }

    private int CalculateValidHeight(int baseSize)
    {
        for (int h = baseSize; h >= 1; h--)
        {
            int totalCells = CalculateTotalCells(baseSize, h);

            if (totalCells % 3 == 0)
            {
                return h;
            }
        }

        return 1;
    }
}