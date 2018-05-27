using System;
using System.Collections.Generic;

namespace GlslDebug.Pipeline
{
    public interface IVertexInData
    { }

    public class VertexStage<V, R, F> where V : IVertexInData where R : FragmentInDataBase<R> where F : FragmentInDataBase<R>, R, new()
    {
        public Func<V, FragmentInDataBase<R>> VertexShader 
        {
            private get;
            set;
        }

        public void SetFunc(Func<V, FragmentInDataBase<R>> func)
        {
            VertexShader = func;
        }

        public void Invoke(List<V> vertexData, Rasterizer<R, F> rasterizer, FragmentStage<F, R> fragmentShader)
        {
            List<FragmentInDataBase<R>> outData = new List<FragmentInDataBase<R>>();

            foreach(var data in vertexData)
            {
                outData.Add(VertexShader.Invoke(data));
            }

            rasterizer.Invoke(outData, fragmentShader);
        }
    }
}
