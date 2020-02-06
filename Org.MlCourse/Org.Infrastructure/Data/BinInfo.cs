using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Data
{
    public class BinInfo
    {
        private double _sumWeights;
        private double _sumGradients;
        private double _sumHessians;

        public BinInfo()
        {
        }

        public BinInfo(double sumWeight, double sumGradient, double sumHessian)
        {
            _sumWeights = sumWeight;
            _sumGradients = sumGradient;
            _sumHessians = sumHessian;
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

        public void Subtract(double weights, double gradients, double hessians)
        {
            _sumWeights -= weights;
            _sumGradients -= gradients;
            _sumHessians -= hessians;
        }

        public void Overwrite(double sumWeights, double sumGradients, double sumHessians)
        {
            _sumWeights = sumWeights;
            _sumGradients = sumGradients;
            _sumHessians = sumHessians;
        }
    }
}
