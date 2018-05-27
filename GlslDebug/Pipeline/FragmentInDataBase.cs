using System;
using System.CodeDom.Compiler;
using System.Numerics;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace GlslDebug.Pipeline
{

    /// <summary>
    /// Populates a method that is used to interpolate the child data in a triangle
    /// </summary>
    /// <typeparam name="T">Type of the child IFragmentInData</typeparam>
    public abstract class FragmentInDataBase<T> where T : FragmentInDataBase<T>
    {
        private static Action<T, Vector3, Triangle<T>> interpolationMethod;
        private static readonly string InterpolationMethodName = "InterpolateData";

        public Vector3 Position { get; set; }

        static FragmentInDataBase()
        {
            string namespaceString = typeof(T).Namespace;
            string typeName = typeof(T).Name + @"";
            string extensionMethodClassName = typeName + @"ExtensionMethods";
            string triangleNamespace = typeof(Triangle<T>).Namespace;

            string interpolations = "";

            foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
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
}
