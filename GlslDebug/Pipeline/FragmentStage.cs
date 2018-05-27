using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace GlslDebug.Pipeline
{

    public class SimpleFragmentInData : FragmentInDataBase<SimpleFragmentInData>
    {
        public SimpleFragmentInData(Vector3 position) : base(position)
        {

        }

        public SimpleFragmentInData()
        {

        }
    }

    public class FragmentStage<F, T> where F : FragmentInDataBase<T>, T, new() where T : FragmentInDataBase<T>
    {
        public Func<F, Vector4> FragmentShader {
            private get;
            set;
        }

        public void Invoke(List<F>[,] inData)
        {
            Vector4[,] outColor = new Vector4[inData.GetLength(0),inData.GetLength(1)];

            for(int i = 0; i < inData.GetLength(0); i++)
            {
                for (int k = 0; k < inData.GetLength(1); k++)
                {
                    //No depth sorting yet
                    if(inData[i, k].Count > 0)
                    {
                        outColor[i, k] = FragmentShader.Invoke(inData[i, k][0]);
                    }
                }
            }

            ScreenOutput.Output(outColor);
        }
    }
}
