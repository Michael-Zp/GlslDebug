using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace GlslDebug.Pipeline
{
    public class VertexInData : IVertexInData
    {
        public Vector3 Position { get; set; } = new Vector3(0, 0, 0);
        public float FloatVal { get; set; } = 0;

        public VertexInData(Vector3 position, float floatVal)
        {
            Position = position;
            FloatVal = floatVal;
        }
    }

    public class FragmentInData : FragmentInDataBase<FragmentInData>
    {
        public float FloatVal { get; set; } = 0;
    }

    public class Programm
    {
        public static void Main()
        {
            Console.WriteLine("Hello world!");


            ShaderProgram<VertexInData, FragmentInData> shaderProgram = new ShaderProgram<VertexInData, FragmentInData>(VertexShader, FragmentShader);

            List<VertexInData> vertexInData = new List<VertexInData>
            {
                new VertexInData(new Vector3(-1, -1, 0), 0),
                new VertexInData(new Vector3(1, -1, 0), 0),
                new VertexInData(new Vector3(-1f, 1, 0), 0)
            };

            for (int i = 0; i < 0; i++)
            {
                vertexInData.Add(new VertexInData(new Vector3(-1, -1, 0), 0));
                vertexInData.Add(new VertexInData(new Vector3(1, -1, 0), 0));
                vertexInData.Add(new VertexInData(new Vector3(.5f, 1, 0), 0));
            }

            float avgTime = 0;
            float count = 0;

            Stopwatch stopwatch = new Stopwatch();
            while (true)
            {
                stopwatch.Reset();

                stopwatch.Start();

                shaderProgram.Invoke(vertexInData);

                stopwatch.Stop();

                count++;
                avgTime = (avgTime * 0.85f) + (stopwatch.Elapsed.Milliseconds * 0.15f);
                Console.WriteLine("Time: Ticks = " + stopwatch.Elapsed.Ticks + " ; Ms = " + stopwatch.Elapsed.Milliseconds + " ; Avg = " + avgTime);
            }
        }

        private static FragmentInData VertexShader(VertexInData inData)
        {
            FragmentInData outData = new FragmentInData
            {
                Position = inData.Position,
                FloatVal = inData.FloatVal
            };

            return outData;
        }

        private static Vector4 FragmentShader(FragmentInData inData)
        {
            return new Vector4(1, 0, 0, 1);
        }
    }
}
