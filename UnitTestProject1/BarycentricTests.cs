using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;

namespace UnitTestProject1
{
    public class BarycentricTests
    {
        // Compute barycentric coordinates (u, v, w) for
        // point p with respect to triangle (a, b, c)
        // Stolen from: https://gamedev.stackexchange.com/questions/23743/whats-the-most-efficient-way-to-find-barycentric-coordinates
        public Vector3 Barycentric()
        {
            Vector3 p = new Vector3(0, 0, 0);

            Vector3 a = new Vector3(-1, -1, 0);
            Vector3 b = new Vector3( 1, -1, 0);
            Vector3 c = new Vector3( 0,  1, 0);
            
            Vector3 v0 = b - a, v1 = c - a, v2 = p - a;

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

        public class FragmentInStruct : FragmentInDataBase<FragmentInStruct>
        {
            public FragmentInStruct(Vector3 position) : base(position)
            {

            }

            public FragmentInStruct()
            {

            }
        }

        [Test]
        public void TestActualImplementation()
        {
            List<FragmentInStruct> verticies = new List<FragmentInStruct>();
            verticies.Add(new FragmentInStruct(new Vector3(-1, -1, 0)));
            verticies.Add(new FragmentInStruct(new Vector3( 1, -1, 0)));
            verticies.Add(new FragmentInStruct(new Vector3( 0,  1, 0)));
            Triangle<FragmentInStruct> triangle = new Triangle<FragmentInStruct>(verticies);

            Vector3 coords = Barycentric(0, 0, triangle);
            Vector3 correctCoords = Barycentric();

            Assert.AreEqual(coords.X, correctCoords.X);
            Assert.AreEqual(coords.Y, correctCoords.Y);
            Assert.AreEqual(coords.Z, correctCoords.Z);
        }

        public Vector3 Barycentric<T>(float xPos, float yPos, Triangle<T> triangle) where T : FragmentInDataBase<T>
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
