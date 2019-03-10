using System;
using System.Collections.Generic;

namespace GlslDebug.Pipeline
{
    public interface IVertexInData
    { }

    public class VertexStage<V, F> where V : IVertexInData where F : FragmentInDataBase<F>, new()
    {
        public Func<V, F> VertexShader
        {
            private get;
            set;
        }

        public void Invoke(List<V> vertexData, Rasterizer<F> rasterizer, FragmentStage<F> fragmentStage)
        {
            List<F> outData = new List<F>();

            foreach (var data in vertexData)
            {
                outData.Add(VertexShader.Invoke(data));
            }

            rasterizer.Invoke(outData, fragmentStage);
        }
    }
}
