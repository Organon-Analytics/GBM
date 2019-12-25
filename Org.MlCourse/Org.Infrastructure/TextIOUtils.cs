using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Org.Infrastructure.Data;
using Org.Infrastructure.Collections;
using System.Linq;

namespace Org.Infrastructure
{
    public class TextIOUtils
    {
        public static long GetNumberOfLines(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream))
                {
                    var line = String.Empty;
                    long cnt = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        ++cnt;
                    }
                    return cnt;
                }
            }
        }
        public static IDictionary<string, int> GetColumnOrder(string line, char delimiter)
        {
            if (String.IsNullOrEmpty(line))
                throw new ArgumentNullException("Input is null");
            var parsed = line.Split(delimiter);
            var dict = new Dictionary<string, int>();
            for (int i = 0; i < parsed.Length; i++)
            {
                var columnName = parsed[i].Trim();
                dict.Add(columnName, i);
            }
            return dict;
        }

        public static IDictionary<string, int> GetColumnOrder(int numCols, string prefix)
        {
            var dict = new Dictionary<string, int>();
            for (int i = 0; i < numCols; i++)
            {
                var columnName = String.Format("{0}_{1}", prefix, i);
                dict.Add(columnName, i);
            }
            return dict;
        }

        public static KeyValuePair<bool, ColumnNativeType> InferNativeType(IDictionary<ColumnNativeType, int> histogram)
        {
            if (histogram == null || histogram.Count == 0)
                return new KeyValuePair<bool, ColumnNativeType>(true, ColumnNativeType.Other);
            if (histogram.Count == 1)
                return new KeyValuePair<bool, ColumnNativeType>(true, histogram.ElementAt(0).Key);
            if (histogram.Count == 2)
            {
                if (histogram.ContainsKey(ColumnNativeType.FloatingPoint) && histogram.ContainsKey(ColumnNativeType.Integer))
                {
                    return new KeyValuePair<bool, ColumnNativeType>(true, ColumnNativeType.FloatingPoint);
                }
            }
            return new KeyValuePair<bool, ColumnNativeType>(false, ColumnNativeType.Other);
        }

        public static DataColumnReport CreateColumnReport(string fileName, bool containsHeader, char delimiter, int firstK)
        {
            if (!File.Exists(fileName))
                throw new ArgumentException(String.Format("File {0} does not exist", fileName));
            IDictionary<string, int> columnToOrder = null;
            IDictionary<int, Histogram<ColumnNativeType>> histograms;
            var cnt = 0;
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream))
                {
                    var colCount = -1;
                    var line = String.Empty;
                    if (containsHeader)
                    {
                        line = reader.ReadLine();
                        if (!String.IsNullOrEmpty(line))
                        {
                            columnToOrder = GetColumnOrder(line, delimiter);
                            colCount = columnToOrder.Count;
                        }
                    }
                    else
                    {
                        line = reader.ReadLine();
                        if (!String.IsNullOrEmpty(line))
                        {
                            colCount = line.Split(delimiter).Length;
                            if (stream.CanSeek)
                            {
                                stream.Seek(0, SeekOrigin.Begin);
                            }
                            else
                            {
                                throw new InvalidOperationException(String.Format("File {0} cant not be seeked", fileName));
                            }
                        }
                    }
                    if (colCount <= 0)
                    {
                        throw new ArgumentException(String.Format("File {0} is empty", fileName));
                    }
                    histograms = InitializeHistograms(colCount);
                    cnt = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parsed = line.Split(delimiter);
                        if (parsed == null || parsed.Length != colCount)
                        {
                            throw new InvalidOperationException(String.Format("There is a non-conformant record in line {0} of file {1} ", cnt, fileName));
                        }
                        for (int i = 0; i < colCount; i++)
                        {
                            var nativeType = DataColumn.GetNativeType(parsed[i]);
                            var hist = histograms[i];
                            if (hist.ContainsKey(nativeType))
                            {
                                ++hist[nativeType];
                            }
                            else
                            {
                                hist.Add(nativeType, 1);
                            }
                        }
                        ++cnt;
                        if (cnt >= firstK)
                        {
                            break;
                        }
                    }
                }
            }
            var orderToColumn = new Dictionary<int, string>();
            foreach (var item in columnToOrder)
            {
                orderToColumn.Add(item.Value, item.Key);
            }
            var collection = new DataColumnCollection();
            var listInvalid = new List<string>();
            foreach (var item in histograms)
            {
                var order = item.Key;
                var colName = orderToColumn[order];
                var hist = item.Value;
                var kv = InferNativeType(hist);
                if (kv.Key)
                {
                    var nativeType = kv.Value;
                    var measurementType = DataColumn.GetDefaultMeasurementType(nativeType);
                    var dc = new DataColumn(colName, order) { NativeType = nativeType, MeasurementType = measurementType };
                    collection.Add(colName, dc);
                }
                else
                {
                    listInvalid.Add(colName);
                }
            }
            return new DataColumnReport { ColumnCollection = collection, InvalidColumns = listInvalid, ScannedNumOfLines = cnt };
        }

        private static IDictionary<int, Histogram<ColumnNativeType>> InitializeHistograms(int colCount)
        {
            var histograms = new Dictionary<int, Histogram<ColumnNativeType>>();
            for (int i = 0; i < colCount; i++)
            {
                histograms.Add(i, new Histogram<ColumnNativeType>());
            }
            return histograms;
        }
    }
}
