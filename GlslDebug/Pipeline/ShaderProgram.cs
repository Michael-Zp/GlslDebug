using System;
using System.Collections.Generic;
using System.Numerics;

namespace GlslDebug.Pipeline
{
    public class ShaderProgram<V, R, F> where V : IVertexInData where R : FragmentInDataBase<R> where F : FragmentInDataBase<R>, R, new()
    {
        private VertexStage<V, R, F> _vertexShader;
        private Rasterizer<R, F> _rasterizer;
        private FragmentStage<F, R> _fragmentStage;

        public ShaderProgram(Func<V, FragmentInDataBase<R>> vertexShader, Func<F, Vector4> fragmentShader)
        {
            _vertexShader = new VertexStage<V, R, F>();
            _vertexShader.VertexShader = vertexShader;

            _rasterizer = new Rasterizer<R, F>();

            _fragmentStage = new FragmentStage<F, R>();
            _fragmentStage.FragmentShader = fragmentShader;
        }

        public void Invoke(List<V> vertexInData)
        {
            _vertexShader.Invoke(vertexInData, _rasterizer, _fragmentStage);
        }
    }
}
