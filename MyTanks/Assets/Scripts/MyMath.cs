using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyMath
{
    public const int HideNode = 4;
    public const int InNode = 6;
    public const int OutNode = 3;

    public static float Relu(float x)
    {
        if (x < 0) return 0;
        else return x;
    }

    public class MatrixInToHide
    {
        public float[,] matrix = new float[InNode, HideNode];

        public float[] Multi(float[] inputs)
        {
            float[] outputs = new float[HideNode];

            for (int i = 0; i < HideNode; i++) outputs[i] = 0;

            for(int i=0;i< InNode; i++)
            {
                for(int j=0;j< HideNode; j++)
                {
                    outputs[j] += inputs[i] * matrix[i, j];
                }
            }

            return outputs;
        }
    }

    public class MatrixHideToOut
    {
        public float[,] matrix = new float[HideNode, OutNode];

        public float[] Multi(float[] inputs)
        {
            float[] outputs = new float[OutNode];

            for (int i = 0; i < OutNode; i++) outputs[i] = 0;

            for (int i = 0; i < HideNode; i++)
            {
                for (int j = 0; j < OutNode; j++)
                {
                    outputs[j] += inputs[i] * matrix[i, j];
                }
            }

            return outputs;
        }
    }
}
