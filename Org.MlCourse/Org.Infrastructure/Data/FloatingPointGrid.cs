using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Data
{
    [Serializable]
    public class FloatingPointGrid
    {
        private readonly double[] _grid;
        private readonly double _lowerBound;
        private readonly double _stepSize;
        private readonly int _numSamples;
        public FloatingPointGrid(double[] grid)
        {
            if (grid == null || grid.Length == 0)
            {
                throw new ArgumentNullException("grid");
            }
            _grid = new double[grid.Length];
            Array.Copy(grid, _grid, grid.Length);
        }

        public FloatingPointGrid(double lowerBound, double stepSize, int numSamples)
        {
            _lowerBound = lowerBound;
            _stepSize = stepSize;
            _numSamples = numSamples;
            _grid = CreateGrid(_lowerBound, _stepSize, _numSamples);
        }

        private double[] CreateGrid(double lowerBound, double stepSize, int numSamples)
        {
            var list = new List<double>();
            for (var i = 0; i < numSamples; i++)
            {
                var d = lowerBound + i * stepSize;
                list.Add(d);
            }
            return list.ToArray();
        }

        public double LowerBound { get { return _lowerBound; } }
        public double StepSize { get { return _stepSize; } }
        public int NumberOfSamples { get { return _numSamples; } }
        public double[] Grid { get { return _grid; } }
    }
}
