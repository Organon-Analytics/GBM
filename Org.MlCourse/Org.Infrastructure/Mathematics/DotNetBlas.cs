using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Mathematics
{
    public class DotNetBlas : IBlas
    {
        public unsafe void Copy(int[] source, int[] dest, int length)
        {
            fixed (int* pSource = source, pDest = dest)
            {
                var pS = pSource;
                var pD = pDest;
                for (var i = 0; i < length; i++)
                {
                    *pD = *pS;
                    pD++;
                    pS++;
                }
            }
        }

        public unsafe void Copy(float[] source, float[] dest, int length)
        {
            fixed (float* pSource = source, pDest = dest)
            {
                var pS = pSource;
                var pD = pDest;
                for (var i = 0; i < length; i++)
                {
                    *pD = *pS;
                    pD++;
                    pS++;
                }
            }
        }

        public unsafe void Copy(double[] source, double[] dest, int length)
        {
            fixed (double* pSource = source, pDest = dest)
            {
                var pS = pSource;
                var pD = pDest;
                for (var i = 0; i < length; i++)
                {
                    *pD = *pS;
                    pD++;
                    pS++;
                }
            }
        }

        public unsafe int CopyNaN(IList<float> source, float[] dest, int length)
        {
            var support = 0;
            for (var i = 0; i < source.Count; i++)
            {
                var f = source[i];
                if (Single.IsNaN(f)) continue;
                dest[i++] = f;
                ++support;
            }
            return support;
        }

        public unsafe int CopyNaN(float[] source, float[] dest, int length)
        {
            fixed (float* pSource = source, pDest = dest)
            {
                var pS = pSource;
                var pD = pDest;
                var support = 0;
                for (var i = 0; i < length; i++)
                {
                    var val = *pS++;
                    if (Single.IsNaN(val)) continue;
                    *pD++ = val;
                    ++support;
                }
                return support;
            }
        }

        public unsafe int CopyNaN(double[] source, double[] dest, int length)
        {
            fixed (double* pSource = source, pDest = dest)
            {
                var pS = pSource;
                var pD = pDest;
                var support = 0;
                for (var i = 0; i < length; i++)
                {
                    var val = *pS++;
                    if (Double.IsNaN(val)) continue;
                    *pD++ = val;
                    ++support;
                }
                return support;
            }
        }

        public unsafe void Initialize(int[] array, int val)
        {
            fixed (int* pArray = array)
            {
                var length = array.Length;
                var p = pArray;
                for (var i = 0; i < length; i++)
                {
                    *p = val;
                    p++;
                }
            }
        }

        public unsafe void Initialize(float[] array, float val)
        {
            fixed (float* pArray = array)
            {
                var length = array.Length;
                var p = pArray;
                for (var i = 0; i < length; i++)
                {
                    *p = val;
                    p++;
                }
            }
        }

        public unsafe void Initialize(double[] array, double val)
        {
            fixed (double* pArray = array)
            {
                var length = array.Length;
                var p = pArray;
                for (var i = 0; i < length; i++)
                {
                    *p = val;
                    p++;
                }
            }
        }

        public void Sort(float[] array)
        {
            Array.Sort(array);
        }

        public void Sort(float[] array, int index, int length)
        {
            Array.Sort(array, index, length);
        }

        public void Sort(double[] array)
        {
            Array.Sort(array);
        }

        public void Sort(double[] array, int index, int length)
        {
            Array.Sort(array, index, length);
        }

        public unsafe bool IsConstant(int[] array)
        {
            fixed (int* pArray = array)
            {
                var p = pArray;
                var mark = *p;
                for (var i = 0; i < array.Length; i++)
                {
                    if ((*p++) != mark) return false;
                }
                return true;
            }
        }

        public unsafe bool IsConstant(float[] array)
        {
            fixed (float* pArray = array)
            {
                var p = pArray;
                var mark = *p;
                var d = mark;
                for (var i = 0; i < array.Length; i++)
                {
                    d = *p;
                    if (d.CompareTo(mark) != 0) return false;
                    ++p;
                }
                return true;
            }
        }

        public unsafe bool IsConstant(double[] array)
        {
            fixed (double* pArray = array)
            {
                var p = pArray;
                var mark = *p;
                var d = mark;
                for (var i = 0; i < array.Length; i++)
                {
                    d = *p;
                    if (d.CompareTo(mark) != 0) return false;
                    ++p;
                }
                return true;
            }
        }


        public unsafe void Axpy(double[] x, double[] y)
        {
            fixed (double* xP = x, yP = y)
            {
                var pX = xP;
                var pY = yP;
                var length = x.Length;
                for (var i = 0; i < length; i++)
                {
                    *pY += *pX;
                    ++pX;
                    ++pY;
                }
            }
        }

        public unsafe void Axpy(double alpha, double[] x, double[] y)
        {
            fixed (double* xP = x, yP = y)
            {
                var pX = xP;
                var pY = yP;
                var length = x.Length;
                for (var i = 0; i < length; i++)
                {
                    *pY += alpha * (*pX);
                    ++pX;
                    ++pY;
                }
            }
        }


        public unsafe void Axpy(double alpha, int offset, int length, double[] x, double[] y)
        {
            fixed (double* xP = x, yP = y)
            {
                var pX = xP + offset;
                var pY = yP + offset;
                for (var i = 0; i < length; i++)
                {
                    *pY += alpha * (*pX);
                    ++pX;
                    ++pY;
                }
            }
        }


        public unsafe int Count(int val, int[] array)
        {
            var sum = 0;
            fixed (int* iP = array)
            {
                var pI = iP;
                for (var i = 0; i < array.Length; i++)
                {
                    sum += (*pI++ == 0) ? 1 : 0;
                }
            }
            return sum;
        }
    }
}
