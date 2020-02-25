using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Data
{
    public struct BinInfo
    {
        private double _sumWeights;
        private double _sumGradients;
        private double _sumHessians;
        public BinInfo(double sumWeight, double sumGradient, double sumHessian)
        {
            _sumWeights = sumWeight;
            _sumGradients = sumGradient;
            _sumHessians = sumHessian;
        }

        public BinInfo(BinInfo other)
        {
            _sumWeights = other.SumWeights;
            _sumGradients = other.SumGradients;
            _sumHessians = other.SumHessians;
        }

        public double SumWeights { get { return _sumWeights; } }
        public double SumGradients { get { return _sumGradients; } }
        public double SumHessians { get { return _sumHessians; } }

        public void Add(double weights, double gradients, double hessians)
        {
            _sumWeights += weights;
            _sumGradients += gradients;
            _sumHessians += hessians;
        }

        public void Add(BinInfo other)
        {
            Add(other.SumWeights, other.SumGradients, other.SumHessians);
        }

        public void Subtract(double weights, double gradients, double hessians)
        {
            _sumWeights -= weights;
            _sumGradients -= gradients;
            _sumHessians -= hessians;
        }

        public void Subtract(BinInfo other)
        {
            Subtract(other.SumWeights, other.SumGradients, other.SumHessians);
        }

        public void Overwrite(double sumWeights, double sumGradients, double sumHessians)
        {
            _sumWeights = sumWeights;
            _sumGradients = sumGradients;
            _sumHessians = sumHessians;
        }

        public void Overwrite(BinInfo other)
        {
            _sumWeights = other.SumWeights;
            _sumGradients = other.SumGradients;
            _sumHessians = other.SumHessians;
        }

        public BinInfo DeepCopy()
        {
            return new BinInfo(SumWeights, SumGradients, SumHessians);
        }
    }
}
