using System;
using System.Collections.Generic;
using System.Numerics;

namespace GlslDebug.Pipeline
{
    public class ShaderProgram<V, F> where V : IVertexInData where F : FragmentInDataBase<F>, new()
    {
        private VertexStage<V, F> _vertexStage;
        private Rasterizer<F> _rasterizer;
        private FragmentStage<F> _fragmentStage;

        public ShaderProgram(Func<V, F> vertexShader, Func<F, Vector4> fragmentShader)
        {
            _vertexStage = new VertexStage<V, F> { VertexShader = vertexShader };

            _rasterizer = new Rasterizer<F>();

            _fragmentStage = new FragmentStage<F> { FragmentShader = fragmentShader };
        }

        public void Invoke(List<V> vertexInData)
        {
            _vertexStage.Invoke(vertexInData, _rasterizer, _fragmentStage);
        }
    }
}
