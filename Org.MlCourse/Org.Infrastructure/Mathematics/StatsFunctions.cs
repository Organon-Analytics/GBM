using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Org.Infrastructure.Mathematics
{
    public class StatsFunctions
    {
        private readonly IBlas _blas;
        public StatsFunctions(IBlas blas)
        {
            _blas = blas;
        }
        public double[] GetQuantiles(IList<float> data, int numQuantiles)
        {
            if (data == null || data.Count == 0) return null;
            var length = data.Count;
            var copy = new float[length];
            var lengthNaN = _blas.CopyNaN(data, copy, data.Count);
            if (lengthNaN == 0) return null;
            _blas.Sort(copy, 0, lengthNaN);
            var rank = 0.0;
            var delta = 1.0 / numQuantiles;
            var step = 1.0 / lengthNaN;
            var curr = Double.NaN;
            var next = Double.NaN;
            var list = new List<double>();
            for (var i = 0; i < (lengthNaN - 1); i++)
            {
                curr = copy[i];
                next = copy[i + 1];
                rank += step;
                if (next.CompareTo(curr) == 0) continue;
                if (rank > delta)
                {
                    list.Add(curr);
                    rank = 0.0;
                }
                curr = next;
            }
            if (list.Count == 0)
            {
                list.Add(Double.PositiveInfinity);
            }
            else if (list.Count == numQuantiles)
            {
                list[numQuantiles - 1] = Double.PositiveInfinity;
            }
            else
            {
                if (!list.Contains(Double.PositiveInfinity))
                {
                    list.Add(Double.PositiveInfinity);
                }
            }
            return list.ToArray();
        }

        public unsafe double GetMean(double[] array)
        {
            fixed (double* pArray = array)
            {
                var p = pArray;
                var w = 0.0;
                var m = 0.0;
                var d = Double.NaN;
                var length = array.Length;
                for (var i = 0; i < length; i++)
                {
                    d = *p++ - m;
                    ++w;
                    m += d / w;
                }
                return m;
            }
        }

        public unsafe double GetMean(float[] array)
        {
            fixed (float* pArray = array)
            {
                var p = pArray;
                var w = 0.0;
                var m = 0.0;
                var d = Double.NaN;
                var length = array.Length;
                for (var i = 0; i < length; i++)
                {
                    d = *p++ - m;
                    ++w;
                    m += d / w;
                }
                return m;
            }
        }

        public unsafe double GetMean(double[] array, int[] indices)
        {
            fixed (double* pArray = array)
            {
                var p = pArray;
                var w = 0.0;
                var m = 0.0;
                var d = Double.NaN;
                var length = indices.Length;
                for (var i = 0; i < length; i++)
                {
                    p = pArray + indices[i];
                    d = *p - m;
                    ++w;
                    m += d / w;
                }
                return m;
            }
        }

        public unsafe double GetMean(float[] array, int[] indices)
        {
            fixed (float* pArray = array)
            {
                var p = pArray;
                var w = 0.0;
                var m = 0.0;
                var d = Double.NaN;
                var length = indices.Length;
                for (var i = 0; i < length; i++)
                {
                    p = pArray + indices[i];
                    d = *p - m;
                    ++w;
                    m += d / w;
                }
                return m;
            }
        }

        public unsafe double GetMean(double[] array, double[] weights)
        {
            fixed (double* pArray = array, wArray = weights)
            {
                var pA = pArray;
                var pW = wArray;
                var w = 0.0;
                var m = 0.0;
                var c = Double.NaN;
                var d = Double.NaN;
                var length = array.Length;
                for (var i = 0; i < length; i++)
                {
                    d = *pA++ - m;
                    c = *pW++;
                    w += c;
                    m += d * (c / w);
                }
                return m;
            }
        }

        public unsafe double GetMean(float[] array, float[] weights)
        {
            fixed (float* pArray = array, wArray = weights)
            {
                var pA = pArray;
                var pW = wArray;
                var w = 0.0;
                var m = 0.0;
                var c = Double.NaN;
                var d = Double.NaN;
                var length = array.Length;
                for (var i = 0; i < length; i++)
                {
                    d = *pA++ - m;
                    c = *pW++;
                    w += c;
                    m += d * (c / w);
                }
                return m;
            }
        }

        public unsafe double GetMean(double[] array, double[] weights, int[] indices)
        {
            fixed (double* pArray = array, wArray = weights)
            {
                var pA = pArray;
                var pW = wArray;
                var w = 0.0;
                var m = 0.0;
                var idx = -1;
                var c = Double.NaN;
                var d = Double.NaN;
                var length = indices.Length;
                for (var i = 0; i < length; i++)
                {
                    idx = indices[i];
                    pA = pArray + idx;
                    pW = wArray + idx;
                    d = *pA - m;
                    c = *pW;
                    w += c;
                    m += d * (c / w);
                }
                return m;
            }
        }

        public unsafe double GetMean(float[] array, float[] weights, int[] indices)
        {
            fixed (float* pArray = array, wArray = weights)
            {
                var pA = pArray;
                var pW = wArray;
                var w = 0.0;
                var m = 0.0;
                var idx = -1;
                var c = Double.NaN;
                var d = Double.NaN;
                var length = indices.Length;
                for (var i = 0; i < length; i++)
                {
                    idx = indices[i];
                    pA = pArray + idx;
                    pW = wArray + idx;
                    d = *pA - m;
                    c = *pW;
                    w += c;
                    m += d * (c / w);
                }
                return m;
            }
        }

        public double[] GetQuantiles(float[] data, int numQuantiles)
        {
            if (data == null || data.Length == 0) return null;
            var length = data.Length;
            var copy = new float[length];
            var lengthNaN = _blas.CopyNaN(data, copy, data.Length);
            if (lengthNaN == 0) return null;
            _blas.Sort(copy, 0, lengthNaN);
            var rank = 0.0;
            var delta = 1.0 / numQuantiles;
            var step = 1.0 / lengthNaN;
            var curr = Double.NaN;
            var next = Double.NaN;
            var list = new List<double>();
            for (var i = 0; i < (lengthNaN - 1); i++)
            {
                curr = copy[i];
                next = copy[i + 1];
                rank += step;
                if (next.CompareTo(curr) == 0) continue;
                if (rank > delta)
                {
                    list.Add(curr);
                    rank = 0.0;
                }
                curr = next;
            }
            if (list.Count == 0)
            {
                list.Add(Double.PositiveInfinity);
            }
            else if (list.Count == numQuantiles)
            {
                list[numQuantiles - 1] = Double.PositiveInfinity;
            }
            else
            {
                if (!list.Contains(Double.PositiveInfinity))
                {
                    list.Add(Double.PositiveInfinity);
                }
            }
            return list.ToArray();
        }

        public double[] GetQuantiles(double[] data, int numQuantiles)
        {
            if (data == null || data.Length == 0) return null;
            var length = data.Length;
            var copy = new double[length];
            var lengthNaN = _blas.CopyNaN(data, copy, data.Length);
            if (lengthNaN == 0) return null;
            _blas.Sort(copy, 0, lengthNaN);
            var rank = 0.0;
            var delta = 1.0 / numQuantiles;
            var step = 1.0 / lengthNaN;
            var curr = Double.NaN;
            var next = Double.NaN;
            var list = new List<double>();
            for (var i = 0; i < (lengthNaN - 1); i++)
            {
                curr = copy[i];
                next = copy[i + 1];
                rank += step;
                if (next.CompareTo(curr) == 0) continue;
                if (rank > delta)
                {
                    list.Add(curr);
                    rank = 0.0;
                }
                curr = next;
            }
            if (list.Count == 0)
            {
                list.Add(Double.PositiveInfinity);
            }
            else if (list.Count == numQuantiles)
            {
                list[numQuantiles - 1] = Double.PositiveInfinity;
            }
            else
            {
                if (!list.Contains(Double.PositiveInfinity))
                {
                    list.Add(Double.PositiveInfinity);
                }
            }
            return list.ToArray();
        }

        public unsafe BivariateMeanCovariance GetMeansAndCovariances(double[] first, double[] second)
        {
            fixed (double* pFirst = first, pSecond = second)
            {
                var p1 = pFirst;
                var p2 = pSecond;
                var m1 = 0.0;
                var m2 = 0.0;
                var s1 = 0.0;
                var s2 = 0.0;
                var s12 = 0.0;
                var w = 0.0;
                var c = 0.0;
                var d1 = 0.0;
                var d2 = 0.0;
                var delta1 = 0.0;
                var delta2 = 0.0;
                for (var i = 0; i < first.Length; i++)
                {
                    ++w;
                    c = 1.0 - 1.0 / w;
                    d1 = *p1++;
                    d2 = *p2++;
                    delta1 = d1 - m1;
                    delta2 = d2 - m2;
                    s1 += delta1 * delta1 * c;
                    s2 += delta2 * delta2 * c;
                    s12 += delta1 * delta2 * c;

                    m1 += delta1 / w;
                    m2 += delta2 / w;
                }
                s1 /= w;
                s2 /= w;
                s12 /= w;
                var sigma1 = Math.Sqrt(s1);
                var sigma2 = Math.Sqrt(s2);
                return new BivariateMeanCovariance(m1, m2, sigma1, sigma2, s12);
            }
        }

        public UnivariateEstimate GetRSquaredEstimate(double[] input, double[] target)
        {
            //var n = input.Length;
            //const int k = 1;
            //var dof = n - k - 1;
            var bivariateEstimate = GetMeansAndCovariances(input, target);
            var estimate = Math.Pow(bivariateEstimate.Correlation, 2);
            return new UnivariateEstimate(estimate, 0.05, Double.NaN, Double.NaN);

            //var denominator = 4.0 * estimate * Math.Pow(1.0 - estimate, 2) * Math.Pow(dof, 2);
            //var numerator = (n * n - 1.0) * (3 + n);
            //var stdErr = Math.Sqrt(denominator / numerator);
            //var tDist = new TDistribution(dof);
            //var delta = tDist.InverseCDF(0.975) * stdErr;
            //return new UnivariateEstimate(estimate, 0.05, estimate - delta, estimate + delta);
        }

        public unsafe LeastSquaresOutput GetLeastSquaresOutput(double[] first, double[] second)
        {
            fixed (double* pFirst = first, pSecond = second)
            {
                var p1 = pFirst;
                var p2 = pSecond;
                var m1 = 0.0;
                var m2 = 0.0;
                var s1 = 0.0;
                var s2 = 0.0;
                var s12 = 0.0;
                var w = 0.0;
                var c = 0.0;
                var d1 = 0.0;
                var d2 = 0.0;
                var delta1 = 0.0;
                var delta2 = 0.0;
                for (var i = 0; i < first.Length; i++)
                {
                    ++w;
                    c = 1.0 - 1.0 / w;
                    d1 = *p1++;
                    d2 = *p2++;
                    delta1 = d1 - m1;
                    delta2 = d2 - m2;
                    s1 += delta1 * delta1 * c;
                    s2 += delta2 * delta2 * c;
                    s12 += delta1 * delta2 * c;

                    m1 += delta1 / w;
                    m2 += delta2 / w;
                }
                s1 /= w;
                s2 /= w;
                s12 /= w;
                var sigma1 = Math.Sqrt(s1);
                var sigma2 = Math.Sqrt(s2);
                var bivariateMeanCovariance = new BivariateMeanCovariance(m1, m2, sigma1, sigma2, s12);
                var corr = bivariateMeanCovariance.Correlation;
                var l2Output = new LeastSquaresOutput
                {
                    SampleWeight = w,
                    RSquared = Double.IsNaN(corr) ? Double.NaN : corr * corr,
                    ResidualSumOfSquares = Double.NaN
                };
                return l2Output;
            }
        }

        //TODO: Pointerize below
        public unsafe LeastSquaresOutput GetLeastSquaresOutput(double[] first, double[] second, int[] indices)
        {
            var m1 = 0.0;
            var m2 = 0.0;
            var s1 = 0.0;
            var s2 = 0.0;
            var s12 = 0.0;
            var s3 = 0.0;
            var w = 0.0;
            var c = 0.0;
            var d1 = 0.0;
            var d2 = 0.0;
            var delta1 = 0.0;
            var delta2 = 0.0;
            var delta3 = 0.0;
            foreach (var idx in indices)
            {
                ++w;
                c = 1.0 - 1.0 / w;
                d1 = first[idx];
                d2 = second[idx];
                delta1 = d1 - m1;
                delta2 = d2 - m2;
                delta3 = d1 - d2;
                s1 += delta1 * delta1 * c;
                s2 += delta2 * delta2 * c;
                s12 += delta1 * delta2 * c;
                s3 += delta3 * delta3;

                m1 += delta1 / w;
                m2 += delta2 / w;
            }
            s1 /= w;
            s2 /= w;
            s12 /= w;
            s3 /= w;
            var sigma1 = Math.Sqrt(s1);
            var sigma2 = Math.Sqrt(s2);
            var bivariateMeanCovariance = new BivariateMeanCovariance(m1, m2, sigma1, sigma2, s12);
            var corr = bivariateMeanCovariance.Correlation;
            var l2Output = new LeastSquaresOutput
            {
                SampleWeight = w,
                RSquared = Double.IsNaN(corr) ? Double.NaN : corr * corr,
                ResidualSumOfSquares = s3
            };
            return l2Output;
        }


        public unsafe LeastSquaresOutput GetLeastSquaresOutput(double[] first, float[] second, int[] indices)
        {
            var m1 = 0.0;
            var m2 = 0.0;
            var s1 = 0.0;
            var s2 = 0.0;
            var s12 = 0.0;
            var s3 = 0.0;
            var w = 0.0;
            var c = 0.0;
            var d1 = 0.0;
            var d2 = 0.0;
            var delta1 = 0.0;
            var delta2 = 0.0;
            var delta3 = 0.0;
            foreach (var idx in indices)
            {
                ++w;
                c = 1.0 - 1.0 / w;
                d1 = first[idx];
                d2 = second[idx];
                delta1 = d1 - m1;
                delta2 = d2 - m2;
                delta3 = d1 - d2;
                s1 += delta1 * delta1 * c;
                s2 += delta2 * delta2 * c;
                s12 += delta1 * delta2 * c;
                s3 += delta3 * delta3;

                m1 += delta1 / w;
                m2 += delta2 / w;
            }
            s1 /= w;
            s2 /= w;
            s12 /= w;
            s3 /= w;
            var sigma1 = Math.Sqrt(s1);
            var sigma2 = Math.Sqrt(s2);
            var bivariateMeanCovariance = new BivariateMeanCovariance(m1, m2, sigma1, sigma2, s12);
            var corr = bivariateMeanCovariance.Correlation;
            var l2Output = new LeastSquaresOutput
            {
                SampleWeight = w,
                RSquared = Double.IsNaN(corr) ? Double.NaN : corr * corr,
                ResidualSumOfSquares = s3
            };
            return l2Output;
        }

        //TODO: Consumes unnecessary memory. Should be made faster
        public AreaUnderTheCurve ComputeAuc<T>(double[] scores, T[] targets, T pCat, T nCat)
        {
            var length = scores.Length;
            var list = new List<Tuple<double, T>>(length);
            for (var i = 0; i < length; i++)
            {
                list.Add(new Tuple<double, T>(scores[i], targets[i]));
            }
            list.Sort();
            var populationCounts = new Dictionary<T, long> { { pCat, 0 }, { nCat, 0 } };
            var dataMap = new SortedDictionary<double, Dictionary<T, int>>();
            foreach (var pair in list)
            {
                var inputVal = pair.Item1;
                var targetVal = pair.Item2;
                ++populationCounts[targetVal];

                if (!dataMap.ContainsKey(inputVal))
                {
                    dataMap.Add(inputVal, new Dictionary<T, int>());
                    dataMap[inputVal].Add(pCat, 0);
                    dataMap[inputVal].Add(nCat, 0);
                }
                ++dataMap[inputVal][targetVal];
            }
            var auc = AreaUnderTheCurve(pCat, nCat, populationCounts, dataMap);
            return auc;
        }
        public AreaUnderTheCurve ComputeAuc<T>(double[] scores, T[] targets, T pCat, T nCat, int[] indices)
        {
            var length = indices.Length;
            var list = new List<Tuple<double, T>>(length);
            list.AddRange(indices.Select(idx => new Tuple<double, T>(scores[idx], targets[idx])));
            list.Sort();
            var populationCounts = new Dictionary<T, long> { { pCat, 0 }, { nCat, 0 } };
            var dataMap = new SortedDictionary<double, Dictionary<T, int>>();
            foreach (var pair in list)
            {
                var inputVal = pair.Item1;
                var targetVal = pair.Item2;
                ++populationCounts[targetVal];

                if (!dataMap.ContainsKey(inputVal))
                {
                    dataMap.Add(inputVal, new Dictionary<T, int>());
                    dataMap[inputVal].Add(pCat, 0);
                    dataMap[inputVal].Add(nCat, 0);
                }
                ++dataMap[inputVal][targetVal];
            }
            var auc = AreaUnderTheCurve(pCat, nCat, populationCounts, dataMap);
            return auc;
        }

        private static AreaUnderTheCurve AreaUnderTheCurve<T>(T pCat, T nCat, Dictionary<T, long> populationCounts,
                                                              SortedDictionary<double, Dictionary<T, int>> dataMap)
        {
            const double degenerateRocValue = 0.5;
            var degenerateRoc = new AreaUnderTheCurve(degenerateRocValue, 0.0, 0.0, degenerateRocValue, degenerateRocValue);

            var isValid = populationCounts.ContainsKey(pCat) &&
                          populationCounts.ContainsKey(nCat) && (populationCounts.Count == 2);
            if (!isValid)
            {
                return degenerateRoc;
            }

            var positiveCount = populationCounts[pCat];
            var negativeCount = populationCounts[nCat];
            if (positiveCount <= 0 || negativeCount <= 0)
            {
                return degenerateRoc;
            }

            long nCumPositive = 0;
            var unscaledArea = 0.0;
            foreach (var pair in dataMap)
            {
                long nCurrPositive = pair.Value.ContainsKey(pCat) ? pair.Value[pCat] : 0;
                long nCurrNegative = pair.Value.ContainsKey(nCat) ? pair.Value[nCat] : 0;

                nCumPositive += nCurrPositive;
                var nRestPositive = populationCounts[pCat] - nCumPositive;

                unscaledArea += nCurrNegative * nRestPositive + 0.5 * nCurrNegative * nCurrPositive;
            }
            var auc = unscaledArea / (populationCounts[nCat] * populationCounts[pCat]);
            return new AreaUnderTheCurve(auc, 0.0, 0.0, auc, auc);
        }
    }
}
