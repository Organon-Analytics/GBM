using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Data
{
    [Serializable]
    public class IntegerGrid
    {
        private readonly int[] _grid;
        private readonly int _lowerBound;
        private readonly int _stepSize;
        private readonly int _numSamples;
        public IntegerGrid(int[] grid)
        {
            if (grid == null || grid.Length == 0)
            {
                throw new ArgumentNullException("grid");
            }
            _grid = new int[grid.Length];
            Array.Copy(grid, _grid, grid.Length);
        }

        public IntegerGrid(int lowerBound, int stepSize, int numSamples)
        {
            _lowerBound = lowerBound;
            _stepSize = stepSize;
            _numSamples = numSamples;
            _grid = CreateGrid(_lowerBound, _stepSize, _numSamples);
        }

        private int[] CreateGrid(int lowerBound, int stepSize, int numSamples)
        {
            var list = new List<int>();
            for (var i = 0; i < numSamples; i++)
            {
                var d = lowerBound + i * stepSize;
                list.Add(d);
            }
            return list.ToArray();
        }

        public int LowerBound { get { return _lowerBound; } }
        public int StepSize { get { return _stepSize; } }
        public int NumberOfSamples { get { return _numSamples; } }
        public int[] Grid { get { return _grid; } }
    }
}
