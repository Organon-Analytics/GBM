using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Mathematics
{
    public interface IBlas
    {
        void Copy(int[] source, int[] dest, int length);
        void Copy(float[] source, float[] dest, int length);
        void Copy(double[] source, double[] dest, int length);
        int CopyNaN(IList<float> source, float[] dest, int length);
        int CopyNaN(float[] source, float[] dest, int length);
        int CopyNaN(double[] source, double[] dest, int length);
        void Sort(float[] array);
        void Sort(float[] array, int index, int length);
        void Sort(double[] array);
        void Sort(double[] array, int index, int length);
        void Initialize(int[] array, int val);
        void Initialize(float[] array, float val);
        void Initialize(double[] array, double val);
        bool IsConstant(int[] array);
        bool IsConstant(float[] array);
        bool IsConstant(double[] array);
        void Axpy(double[] x, double[] y);
        void Axpy(double alpha, double[] x, double[] y);
        void Axpy(double alpha, int offset, int length, double[] x, double[] y);
        int Count(int val, int[] array);
    }
}
