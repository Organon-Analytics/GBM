using Org.Infrastructure.Data;
using Org.Ml.Domain.Model;
using Org.Ml.Domain.Model.Gbm;
using Org.Ml.Domain.Service.Gbm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Org.Ml.Tests
{
    public class GbmModelBuildServiceTest
    {
        private GbmAlgorithmSettings GetAlgorithmSettings()
        {
            var settings = new GbmAlgorithmSettings();
            settings.LossFunctionType = LossFunctionType.CrossEntropy;
            settings.SearchForHyperParameters = true;
            settings.UseLineSearchForLearningRate = false;


            settings.GridForLearningRate = new FloatingPointGrid(new double[] { 0.1, 0.01});
            settings.GridForRowSamplingRate = new FloatingPointGrid(new double[] { 0.5});
            settings.GridForColumnSamplingRate = new FloatingPointGrid(new double[] { 0.5});
            settings.GridForMaxTreeLeaves = new IntegerGrid(new int[] { 5 });
            settings.GridForMaxIterations = new IntegerGrid(new int[] { 250 });

            //settings.GridForLearningRate = new FloatingPointGrid(new double[] { 0.1, 0.01, 0.001});
            //settings.GridForRowSamplingRate = new FloatingPointGrid(new double[] { 0.5, 0.6, 0.8 });
            //settings.GridForColumnSamplingRate = new FloatingPointGrid(new double[] { 0.5, 0.6, 0.8 });
            //settings.GridForMaxTreeLeaves = new IntegerGrid(new int[] { 2, 4, 6 });
            //settings.GridForMaxIterations = new IntegerGrid(new int[] { 250, 500, 1000 });

            return settings;
        }

        private ModellingDataSettings GetDataSettings()
        {
            var settings = new ModellingDataSettings();
            var textSourceLocator = new TextDataSourceLocator(@"C:\Git\ML-Course\Data\Census.txt")
            {
                ContainsHeaderWithColumnNames = true,
                Delimiter = ';',
                NumLinesForTypeInference = 10000
            };
            settings.DevelopmentDataSource = textSourceLocator;
            settings.DevelopmentFrameSamplingParameters = new DataFrameSamplingParameters();
            settings.ExcludedColumns = new List<string> { "TARGET" };
            settings.TargetColumnName = "PROXY";
            settings.PositiveCategory = ">50K";
            settings.NegativeCategory = "<=50K";
            settings.TrainingRatio = 0.7;

            settings.ScoringInputDataSource = new SqlDataSourceLocator("CONN", "SCHEMA", "ADULT_TRAIN");
            settings.ScoringOutputDataSource = new SqlDataSourceLocator("CONN", "SCHEMA", "ADULT_TRAIN");
            settings.IncludedColumnsForScoring = new List<string>();
            settings.ScoreColumnName = "GBM_SCORE";

            return settings;
        }

        private GbmOutputSettings GetOutputSettings()
        {
            var settings = new GbmOutputSettings()
            {
                BaseDirectory = @"C:\Git\ML-Course\Data",
                Identifier = "Trial01"
            };
            return settings;
        }

        public void Execute()
        {
            var algorithmSettings = GetAlgorithmSettings();
            var dataSettings = GetDataSettings();
            var outputSettings = GetOutputSettings();
            var service = new GbmModelBuildService(algorithmSettings, dataSettings, outputSettings);
            service.Execute(1);
        }
    }
}
