using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Org.Infrastructure.Data
{
    public class TextDataFrameReader : DataFrameReader
    {
        public override DataFrame Read(IDataSourceLocator source, DataFrameSamplingParameters samplingParameters)
        {
            var textSource = source as TextDataSourceLocator;
            if (textSource == null)
            {
                throw new ArgumentException("Data source should be a text file");
            }
            var fileName = textSource.FullName;
            var headerFlag = textSource.ContainsHeaderWithColumnNames;
            var delim = textSource.Delimiter;
            var firstK = textSource.NumLinesForTypeInference;
            var columnReport = TextIOUtils.CreateColumnReport(fileName, headerFlag, delim, firstK);
            var columnCollection = columnReport.ColumnCollection;

            var numLines = TextIOUtils.GetNumberOfLines(fileName);
            var capacity = samplingParameters.MaxSampleSize;
            var ratio = (numLines <= capacity) ? 1.0 : ((double)capacity) / numLines;

            var frame = new DataFrame();
            frame.Initialize(columnCollection, capacity);

            var indices = columnCollection.GetValidIndices();
            var length = indices.Length;
            var rand = new Random();
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream))
                {
                    //var colCount = -1;
                    var line = String.Empty;
                    if (headerFlag)
                    {
                        line = reader.ReadLine();
                    }
                    var cnt = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        ++cnt;
                        if (rand.NextDouble() > ratio) continue;
                        var parsed = line.Split(delim);
                        //if (parsed == null || parsed.Length != colCount)
                        //{
                        //    throw new InvalidOperationException(String.Format("There is a non-conformant record in line {0} of file {1} ", cnt, fileName));
                        //}
                        for (int i = 0; i < length; i++)
                        {
                            var idx = indices[i];
                            frame.Add(idx, parsed[idx]);
                        }
                    }
                }
            }
            //frame.Cleanup(new List<string>());
            return frame;
        }

        public override DataFrame Read(IDataSourceLocator source, IList<string> excludedColumnList, DataFrameSamplingParameters samplingParameters)
        {
            throw new NotImplementedException();
        }
    }
}
