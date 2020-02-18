using Org.Infrastructure.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    public class GbmGlobalArrays
    {
        private readonly int[] _rowIndices;
        private readonly int[] _nodeIndices;
        private readonly double[] _weights;
        private readonly double[] _gradients;
        private readonly double[] _hessians;
        private readonly double[] _scores;
        private readonly double[] _delta;

        private readonly IBlas _blas;
        public GbmGlobalArrays(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentException("Length should be a positive integer");
            }
            _blas = new DotNetBlas();
            _rowIndices = new int[length];
            _nodeIndices = new int[length];
            _weights = new double[length];
            _gradients = new double[length];
            _hessians = new double[length];
            _scores = new double[length];
            _delta = new double[length];
        }

        public int[] Indices { get { return _rowIndices; } }
        public int[] NodeIndices { get { return _nodeIndices; } }
        public double[] Weights { get { return _weights; } }
        public double[] Gradients { get { return _gradients; } }
        public double[] Hessians { get { return _hessians; } }
        public double[] Scores { get { return _scores; } }
        public double[] Delta { get { return _delta; } }

        public void CopyRowIndices(int[] rowIndices)
        {
            var length = rowIndices.Length;
            _blas.Copy(rowIndices, _rowIndices, length);
        }

        public void CopyNodeIndices(int[] nodeIndices)
        {
            var length = nodeIndices.Length;
            _blas.Copy(nodeIndices, _nodeIndices, length);
        }

        public void CopyWeights(double[] weights)
        {
            var length = weights.Length;
            _blas.Copy(weights, _weights, length);
        }

        public void CopyGradients(double[] gradients)
        {
            var length = gradients.Length;
            _blas.Copy(gradients, _gradients, length);
        }

        public void CopyHessians(double[] hessians)
        {
            var length = hessians.Length;
            _blas.Copy(hessians, _hessians, length);
        }

        public void CopyDelta(double[] delta)
        {
            var length = delta.Length;
            _blas.Copy(delta, _delta, length);
        }
    }
}
