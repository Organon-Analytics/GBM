using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    public class GbmOutputSettings
    {
        public GbmOutputSettings()
        {
            GenerateFileOutput = false;
            BaseDirectory = String.Empty;
            Identifier = String.Empty;
        }
        public bool GenerateFileOutput { get; set; }
        public string BaseDirectory { get; set; }
        public string Identifier { get; set; }

        public string GetOutputFolder()
        {
            return String.Format(@"{0}\GbmOutput_{1}", BaseDirectory, Identifier);
        }

        public string GetLossHistoryFileName()
        {
            var outputFolder = GetOutputFolder();
            return String.Format(@"{0}\{1}.txt", outputFolder, "LossHistory");
        }
        public string GetHyperParameterReportFileName()
        {
            var outputFolder = GetOutputFolder();
            return String.Format(@"{0}\{1}.txt", outputFolder, "HyperParameterReport");
        }

        public string GetSqlFileName()
        {
            var outputFolder = GetOutputFolder();
            return String.Format(@"{0}\{1}.txt", outputFolder, "ModelSql");
        }

        public string GetScoresFileName()
        {
            var outputFolder = GetOutputFolder();
            return String.Format(@"{0}\{1}.txt", outputFolder, "Scores");
        }

        public string GetRuntimeSummaryFileName()
        {
            var outputFolder = GetOutputFolder();
            return String.Format(@"{0}\{1}.txt", outputFolder, "RuntimeSummary");
        }
    }
}
