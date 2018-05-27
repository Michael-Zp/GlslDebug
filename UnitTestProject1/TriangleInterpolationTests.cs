using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using NUnit.Framework;



namespace UnitTestProject1
{

    public class Triangle<T> where T : FragmentInDataBase<T>
    {
        public readonly List<T> VertexData;

        public Triangle(List<T> data)
        {
            VertexData = data;
        }
    }

    public class SimpleFragmentInData : FragmentInDataBase<SimpleFragmentInData>
    {
        public SimpleFragmentInData(Vector3 position) : base(position)
        {

        }

        public SimpleFragmentInData()
        {

        }
    }

    public class FragmentInData : FragmentInDataBase<FragmentInData>
    {
        public float FloatVal { get; set; }
        public double DoubleVal { get; set; }
        public int IntVal { get; set; }
        public uint UintVal { get; set; }
        public Vector2 Vector2Val { get; set; }
        public Vector4 Vector4Val { get; set; }

        public FragmentInData(Vector3 position, float floatVal, double doubleVal, int intVal, uint uintVal, Vector2 vector2Val, Vector4 vector4Val) : base(position)
        {
            FloatVal = floatVal;
            DoubleVal = doubleVal;
            IntVal = intVal;
            UintVal = uintVal;
            Vector2Val = vector2Val;
            Vector4Val = vector4Val;
        }

        public FragmentInData()
        {

        }
    }

    /// <summary>
    /// Populates a method that is used to interpolate the child data in a triangle
    /// </summary>
    /// <typeparam name="T">Type of the child IFragmentInData</typeparam>
    public abstract class FragmentInDataBase<T> where T : FragmentInDataBase<T>
    {
        private static Action<T, Vector3, Triangle<T>> interpolationMethod;
        private static string InterpolationMethodName = "InterpolateData";

        public Vector3 Position { get; set; }

        static FragmentInDataBase()
        {
            string namespaceString = typeof(T).Namespace;
            string typeName = typeof(T).Name + @"";
            string extensionMethodClassName = typeName + @"ExtensionMethods";
            string triangleNamespace = typeof(Triangle<T>).Namespace;

            string interpolations = "";

            foreach(var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                string name = property.Name;
                string type = property.PropertyType.Name;
                interpolations += "\n" + @"
                    data." + name + @" = (" + type + @")(baryCoords.X * triangle.VertexData[0]." + name + @" +
                                         baryCoords.Y * triangle.VertexData[1]." + name + @" +
                                         baryCoords.Z * triangle.VertexData[2]." + name + @");
                    " + "\n";
            }
            

            string code =
                @"
using System;
using System.Numerics;
using " + triangleNamespace + @";

namespace " + namespaceString + @"
{
    public static class " + extensionMethodClassName + @"
    {
        public static void " + InterpolationMethodName + @"(this " + typeName + @" data, Vector3 baryCoords, Triangle<" + typeName + @"> triangle)
        {
            " + interpolations + @"
        }
    }
}";
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();

            parameters.ReferencedAssemblies.Add(typeof(Triangle<T>).Assembly.Location);
            parameters.ReferencedAssemblies.Add(typeof(T).Assembly.Location);
            parameters.ReferencedAssemblies.Add(typeof(Vector3).Assembly.Location);
            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = false;

            CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

            if (results.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();

                foreach (CompilerError error in results.Errors)
                {
                    sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                }

                throw new InvalidOperationException(sb.ToString());
            }

            Assembly assembly = results.CompiledAssembly;
            Type program = assembly.GetType(namespaceString + @"." + extensionMethodClassName);
            MethodInfo method = program.GetMethod(InterpolationMethodName);

            interpolationMethod = (obj, baryCoords, triangle) => { method.Invoke(obj, new object[] { obj, baryCoords, triangle }); };
        }

        public FragmentInDataBase(Vector3 position)
        {
            Position = position;
        }

        public FragmentInDataBase()
        {

        }

        public void Interpolate(Vector3 baryCoords, Triangle<T> triangle)
        {
            T child = (T)Convert.ChangeType(this, typeof(T));
            interpolationMethod.Invoke(child, baryCoords, triangle);
        }
    }

    public class TriangleInterpolationTests
    {
        



        [Test]
        public void TestReflectionToCreateInterpolateMethod()
        {
            List<FragmentInData> verticies = new List<FragmentInData>();
            verticies.Add(new FragmentInData(new Vector3(-1, -1, 0), 0, 0, 0, 0, new Vector2(0, 0), new Vector4(0)));
            verticies.Add(new FragmentInData(new Vector3(1, -1, 0), 1, 1, 1, 1, new Vector2(1, 1), new Vector4(1)));
            verticies.Add(new FragmentInData(new Vector3(0, 1, 0), 2, 2, 2, 2, new Vector2(2, 2), new Vector4(2)));
            Triangle<FragmentInData> triangle = new Triangle<FragmentInData>(verticies);


            List<SimpleFragmentInData> simpleVerticies = new List<SimpleFragmentInData>();
            simpleVerticies.Add(new SimpleFragmentInData(new Vector3(-1, -1, 0)));
            simpleVerticies.Add(new SimpleFragmentInData(new Vector3(1, -1, 0)));
            simpleVerticies.Add(new SimpleFragmentInData(new Vector3(0, 1, 0)));
            Triangle<SimpleFragmentInData> simpleTriangle = new Triangle<SimpleFragmentInData>(simpleVerticies);
            
            BarycentricTests barycentricTests = new BarycentricTests();

            Vector3 baryCoords = barycentricTests.Barycentric(0, 0, triangle);

            FragmentInData data = new FragmentInData();
            SimpleFragmentInData simpleData = new SimpleFragmentInData();

            data.Interpolate(baryCoords, triangle);
            simpleData.Interpolate(baryCoords, simpleTriangle);

            for(int i = 0; i < 1; i++)
            {
                simpleData.Interpolate(baryCoords, simpleTriangle);
            }

            Console.WriteLine("Simple");
            Console.WriteLine(simpleData.Position);

            Console.WriteLine("Normal");
            Console.WriteLine(data.Position);
            Console.WriteLine(data.FloatVal);
            Console.WriteLine(data.DoubleVal);
            Console.WriteLine(data.IntVal);
            Console.WriteLine(data.UintVal);
            Console.WriteLine(data.Vector2Val);
            Console.WriteLine(data.Vector4Val);




            verticies = new List<FragmentInData>();
            verticies.Add(new FragmentInData(new Vector3(-1, -1, 0), 0, 0, 0, 0, new Vector2(0, 0), new Vector4(0)));
            verticies.Add(new FragmentInData(new Vector3(1, -1, 0), 1, 1, 1, 1, new Vector2(1, 1), new Vector4(1)));
            verticies.Add(new FragmentInData(new Vector3(0, 1, 0), 2, 2, 2, 2, new Vector2(2, 2), new Vector4(2)));
            triangle = new Triangle<FragmentInData>(verticies);


            FragmentInData correctData = new FragmentInData();

            correctData.Position = baryCoords.X * triangle.VertexData[0].Position +
                                   baryCoords.Y * triangle.VertexData[1].Position +
                                   baryCoords.Z * triangle.VertexData[2].Position;


            correctData.FloatVal = baryCoords.X * triangle.VertexData[0].FloatVal +
                                   baryCoords.Y * triangle.VertexData[1].FloatVal +
                                   baryCoords.Z * triangle.VertexData[2].FloatVal;


            correctData.DoubleVal = baryCoords.X * triangle.VertexData[0].DoubleVal +
                                    baryCoords.Y * triangle.VertexData[1].DoubleVal +
                                    baryCoords.Z * triangle.VertexData[2].DoubleVal;


            correctData.IntVal = (int)(baryCoords.X * triangle.VertexData[0].IntVal +
                                       baryCoords.Y * triangle.VertexData[1].IntVal +
                                       baryCoords.Z * triangle.VertexData[2].IntVal);


            correctData.UintVal = (uint)(baryCoords.X * triangle.VertexData[0].UintVal +
                                         baryCoords.Y * triangle.VertexData[1].UintVal +
                                         baryCoords.Z * triangle.VertexData[2].UintVal);


            correctData.Vector2Val = baryCoords.X * triangle.VertexData[0].Vector2Val +
                                     baryCoords.Y * triangle.VertexData[1].Vector2Val +
                                     baryCoords.Z * triangle.VertexData[2].Vector2Val;


            correctData.Vector4Val = baryCoords.X * triangle.VertexData[0].Vector4Val +
                                     baryCoords.Y * triangle.VertexData[1].Vector4Val +
                                     baryCoords.Z * triangle.VertexData[2].Vector4Val;


            Console.WriteLine("Correct");
            Console.WriteLine(correctData.Position);
            Console.WriteLine(correctData.FloatVal);
            Console.WriteLine(correctData.DoubleVal);
            Console.WriteLine(correctData.IntVal);
            Console.WriteLine(correctData.UintVal);
            Console.WriteLine(correctData.Vector2Val);
            Console.WriteLine(correctData.Vector4Val);
        }



        [Test]
        public void Interpolation()
        {
            List<FragmentInData> verticies = new List<FragmentInData>();
            verticies.Add(new FragmentInData(new Vector3(-1, -1, 0), 0, 0, 0, 0, new Vector2(0, 0), new Vector4(0)));
            verticies.Add(new FragmentInData(new Vector3(1, -1, 0), 1, 1, 1, 1, new Vector2(1, 1), new Vector4(1)));
            verticies.Add(new FragmentInData(new Vector3(0, 1, 0), 2, 2, 2, 2, new Vector2(2, 2), new Vector4(2)));
            Triangle<FragmentInData> triangle = new Triangle<FragmentInData>(verticies);

            BarycentricTests barycentricTests = new BarycentricTests();

            Vector3 baryCoords = barycentricTests.Barycentric(0, 0, triangle);

            FragmentInData correctData = new FragmentInData();

            correctData.Position = baryCoords.X * triangle.VertexData[0].Position +
                                   baryCoords.Y * triangle.VertexData[1].Position +
                                   baryCoords.Z * triangle.VertexData[2].Position;


            correctData.FloatVal = baryCoords.X * triangle.VertexData[0].FloatVal +
                                   baryCoords.Y * triangle.VertexData[1].FloatVal +
                                   baryCoords.Z * triangle.VertexData[2].FloatVal;


            correctData.DoubleVal = baryCoords.X * triangle.VertexData[0].DoubleVal +
                                    baryCoords.Y * triangle.VertexData[1].DoubleVal +
                                    baryCoords.Z * triangle.VertexData[2].DoubleVal;


            correctData.IntVal = (int)(baryCoords.X * triangle.VertexData[0].IntVal +
                                       baryCoords.Y * triangle.VertexData[1].IntVal +
                                       baryCoords.Z * triangle.VertexData[2].IntVal);


            correctData.UintVal = (uint)(baryCoords.X * triangle.VertexData[0].UintVal +
                                         baryCoords.Y * triangle.VertexData[1].UintVal +
                                         baryCoords.Z * triangle.VertexData[2].UintVal);


            correctData.Vector2Val = baryCoords.X * triangle.VertexData[0].Vector2Val +
                                     baryCoords.Y * triangle.VertexData[1].Vector2Val +
                                     baryCoords.Z * triangle.VertexData[2].Vector2Val;


            correctData.Vector4Val = baryCoords.X * triangle.VertexData[0].Vector4Val +
                                     baryCoords.Y * triangle.VertexData[1].Vector4Val +
                                     baryCoords.Z * triangle.VertexData[2].Vector4Val;


            FragmentInData temp0 = InterpolateWithReflection(baryCoords, triangle);
            FragmentInData temp1 = InterpolateWithoutReflection(baryCoords, triangle);


            int sampleSize = 10000;
            Stopwatch reflectionSw = new Stopwatch();

            reflectionSw.Start();

            for (int i = 0; i < sampleSize; i++)
            {
                FragmentInData interpolatedData = InterpolateWithReflection(baryCoords, triangle);
            }

            reflectionSw.Stop();


            Stopwatch compileRuntimeSw = new Stopwatch();
            {
                compileRuntimeSw.Start();

                FragmentInData interpolatedData = new FragmentInData();

                for (int i = 0; i < sampleSize; i++)
                {
                    interpolatedData.Interpolate(baryCoords, triangle);
                }

                compileRuntimeSw.Stop();
            }


            Stopwatch withoutReflectionSw = new Stopwatch();

            withoutReflectionSw.Start();

            for (int i = 0; i < sampleSize; i++)
            {
                FragmentInData interpolatedData = InterpolateWithoutReflection(baryCoords, triangle);
            }

            withoutReflectionSw.Stop();


            Console.WriteLine("Reflection: Ticks = " + reflectionSw.Elapsed.Ticks + " ; Ms = " + reflectionSw.Elapsed.Milliseconds);
            Console.WriteLine("CompMethod: Ticks = " + compileRuntimeSw.Elapsed.Ticks + " ; Ms = " + compileRuntimeSw.Elapsed.Milliseconds);
            Console.WriteLine("WoReflecti: Ticks = " + withoutReflectionSw.Elapsed.Ticks + " ; Ms = " + withoutReflectionSw.Elapsed.Milliseconds);



            Assert.AreEqual(temp0.Position.X, correctData.Position.X);
            Assert.AreEqual(temp0.Position.Y, correctData.Position.Y);
            Assert.AreEqual(temp0.Position.Z, correctData.Position.Z);
            
            Assert.AreEqual(temp0.FloatVal, correctData.FloatVal);
            Assert.AreEqual(temp0.DoubleVal, correctData.DoubleVal);
            Assert.AreEqual(temp0.IntVal, correctData.IntVal);
            Assert.AreEqual(temp0.UintVal, correctData.UintVal);

            Assert.AreEqual(temp0.Vector2Val.X, correctData.Vector2Val.X);
            Assert.AreEqual(temp0.Vector2Val.Y, correctData.Vector2Val.Y);

            Assert.AreEqual(temp0.Vector4Val.X, correctData.Vector4Val.X);
            Assert.AreEqual(temp0.Vector4Val.Y, correctData.Vector4Val.Y);
            Assert.AreEqual(temp0.Vector4Val.Z, correctData.Vector4Val.Z);
            Assert.AreEqual(temp0.Vector4Val.W, correctData.Vector4Val.W);
        }

        private FragmentInData InterpolateWithReflection(Vector3 baryCoords, Triangle<FragmentInData> triangle)
        {

            PropertyInfo[] pi = typeof(FragmentInData).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            FragmentInData interpolatedData = new FragmentInData();

            foreach (var p in pi)
            {
                var value0 = p.GetValue(triangle.VertexData[0], null);
                var value1 = p.GetValue(triangle.VertexData[1], null);
                var value2 = p.GetValue(triangle.VertexData[2], null);


                if (value0 is float)
                {
                    float correctVal = (float)value0 * baryCoords.X + (float)value1 * baryCoords.Y + (float)value2 * baryCoords.Z;
                    p.SetValue(interpolatedData, correctVal);
                }
                else if (value0 is double)
                {
                    double correctVal = (double)value0 * baryCoords.X + (double)value1 * baryCoords.Y + (double)value2 * baryCoords.Z;
                    p.SetValue(interpolatedData, correctVal);
                }
                else if (value0 is int)
                {
                    int correctVal = (int)((int)value0 * baryCoords.X + (int)value1 * baryCoords.Y + (int)value2 * baryCoords.Z);
                    p.SetValue(interpolatedData, correctVal);
                }
                else if (value0 is uint)
                {
                    uint correctVal = (uint)((uint)value0 * baryCoords.X + (uint)value1 * baryCoords.Y + (uint)value2 * baryCoords.Z);
                    p.SetValue(interpolatedData, correctVal);
                }
                else if (value0 is Vector2)
                {
                    Vector2 correctVal = (Vector2)value0 * baryCoords.X + (Vector2)value1 * baryCoords.Y + (Vector2)value2 * baryCoords.Z;
                    p.SetValue(interpolatedData, correctVal);
                }
                else if (value0 is Vector3)
                {
                    Vector3 correctVal = (Vector3)value0 * baryCoords.X + (Vector3)value1 * baryCoords.Y + (Vector3)value2 * baryCoords.Z;
                    p.SetValue(interpolatedData, correctVal, null);
                }
                else if (value0 is Vector4)
                {
                    Vector4 correctVal = (Vector4)value0 * baryCoords.X + (Vector4)value1 * baryCoords.Y + (Vector4)value2 * baryCoords.Z;
                    p.SetValue(interpolatedData, correctVal);
                }
            }

            return interpolatedData;
        }

        private FragmentInData InterpolateWithoutReflection(Vector3 baryCoords, Triangle<FragmentInData> triangle)
        {
            FragmentInData interpolatedData = new FragmentInData();
            
            interpolatedData.Position = baryCoords.X * triangle.VertexData[0].Position +
                                   baryCoords.Y * triangle.VertexData[1].Position +
                                   baryCoords.Z * triangle.VertexData[2].Position;
            
            interpolatedData.FloatVal = baryCoords.X * triangle.VertexData[0].FloatVal +
                                   baryCoords.Y * triangle.VertexData[1].FloatVal +
                                   baryCoords.Z * triangle.VertexData[2].FloatVal;


            interpolatedData.DoubleVal = baryCoords.X * triangle.VertexData[0].DoubleVal +
                                    baryCoords.Y * triangle.VertexData[1].DoubleVal +
                                    baryCoords.Z * triangle.VertexData[2].DoubleVal;


            interpolatedData.IntVal = (int)(baryCoords.X * triangle.VertexData[0].IntVal +
                                       baryCoords.Y * triangle.VertexData[1].IntVal +
                                       baryCoords.Z * triangle.VertexData[2].IntVal);


            interpolatedData.UintVal = (uint)(baryCoords.X * triangle.VertexData[0].UintVal +
                                         baryCoords.Y * triangle.VertexData[1].UintVal +
                                         baryCoords.Z * triangle.VertexData[2].UintVal);


            interpolatedData.Vector2Val = baryCoords.X * triangle.VertexData[0].Vector2Val +
                                     baryCoords.Y * triangle.VertexData[1].Vector2Val +
                                     baryCoords.Z * triangle.VertexData[2].Vector2Val;


            interpolatedData.Vector4Val = baryCoords.X * triangle.VertexData[0].Vector4Val +
                                     baryCoords.Y * triangle.VertexData[1].Vector4Val +
                                     baryCoords.Z * triangle.VertexData[2].Vector4Val;

            return interpolatedData;
        }
        
    }
}
