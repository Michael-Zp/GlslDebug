using System.Collections.Generic;
using System.Numerics;

namespace GlslDebug.Pipeline
{
    public class Triangle<T> where T : FragmentInDataBase<T>
    {
        public readonly List<T> VertexData;

        public Triangle(List<T> data)
        {
            VertexData = data;
        }
    }

    public class Rasterizer<F> where F : FragmentInDataBase<F>, new()
    {
        public void Invoke(List<F> vertexOutData, FragmentStage<F> fragmentStage)
        {
            var triangles = GenerateTriangles(vertexOutData);

            const float xDim = 400;
            const float xPixelStep = 2.0f / xDim;
            const float yDim = 400;
            const float yPixelStep = 2.0f / yDim;

            var fragments = new List<F>[(int)xDim, (int)yDim];


            for (int x = 0; x < xDim; x++)
            {
                float xPos = -1.0f + x * xPixelStep;

                for (int y = 0; y < yDim; y++)
                {
                    float yPos = -1.0f + y * yPixelStep;

                    fragments[x, y] = new List<F>();

                    foreach (var triangle in triangles)
                    {
                        Vector3 coords = Barycentric(xPos, yPos, triangle);

                        if (coords.X >= 0 && coords.Y >= 0 && coords.Z >= 0)
                        {
                            F data = new F();
                            data.Interpolate(coords, triangle);
                            fragments[x, y].Add(data);
                        }
                    }
                }
            }

            fragmentStage.Invoke(fragments);
        }

        private List<Triangle<F>> GenerateTriangles(List<F> verticies)
        {
            List<Triangle<F>> primitives = new List<Triangle<F>>();

            for (int i = 0; i + 2 < verticies.Count; i += 3)
            {
                /*
                //Acceleration
                bool isVisible = false;
                for(int k = i; k < i + 3; k++)
                {
                    isVisible |= ((verticies[k].Position.X > -1 || verticies[k].Position.X < 1) && (verticies[k].Position.Y > -1 || verticies[k].Position.Y < 1));
                }

                if(isVisible)
                {
                    primitives.Add(new Triangle<T>(new List<T> { (T)verticies[i], (T)verticies[i + 1], (T)verticies[i + 2] }));
                }
                */

                primitives.Add(new Triangle<F>(new List<F> { (F)verticies[i], (F)verticies[i + 1], (F)verticies[i + 2] }));
            }

            return primitives;
        }


        private Vector3 Barycentric(float xPos, float yPos, Triangle<F> triangle)
        {
            Vector3 p = new Vector3(xPos, yPos, 0);

            Vector3 v0 = triangle.VertexData[1].Position - triangle.VertexData[0].Position,
                    v1 = triangle.VertexData[2].Position - triangle.VertexData[0].Position,
                    v2 = p - triangle.VertexData[0].Position;

            float d00 = Vector3.Dot(v0, v0);
            float d01 = Vector3.Dot(v0, v1);
            float d11 = Vector3.Dot(v1, v1);
            float d20 = Vector3.Dot(v2, v0);
            float d21 = Vector3.Dot(v2, v1);

            float denom = d00 * d11 - d01 * d01;

            float v = (d11 * d20 - d01 * d21) / denom;
            float w = (d00 * d21 - d01 * d20) / denom;
            float u = 1.0f - v - w;

            return new Vector3(v, u, w);
        }
    }
}
