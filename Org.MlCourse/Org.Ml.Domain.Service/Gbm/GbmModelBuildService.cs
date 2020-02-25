using System;
using System.Collections.Generic;
using System.Text;
using Org.Infrastructure.Data;
using Org.Ml.Domain.Model;
using Org.Ml.Domain.Model.Gbm;

namespace Org.Ml.Domain.Service.Gbm
{
    public class GbmModelBuildService
    {
        private readonly GbmAlgorithmSettings _algoSettings;
        private readonly ModellingDataSettings _dataSettings;
        private readonly GbmOutputSettings _outputSettings;
        public GbmModelBuildService(GbmAlgorithmSettings algoSettings, ModellingDataSettings dataSettings, GbmOutputSettings outputSettings)
        {
            _algoSettings = algoSettings;
            _dataSettings = dataSettings;
            _outputSettings = outputSettings;
        }
        public void Execute(int numOfThreads)
        {
            //ValidateAlgorithmSettings(_algoSettings);
            //ValidateDataSettings(_dataSettings);
            var developmentFrame = ReadDevelopmentDataFrame(_dataSettings);
            var testFrame = ReadTestDataFrame(_dataSettings);
            PrepareData(_algoSettings, _dataSettings, developmentFrame);
            var model = BuildModel(_algoSettings, _dataSettings, _outputSettings, developmentFrame, testFrame);

        }

        private void ValidateAlgorithmSettings(GbmAlgorithmSettings algoSettings)
        {
            throw new NotImplementedException();
        }

        private void ValidateDataSettings(ModellingDataSettings dataSettings)
        {
            throw new NotImplementedException();
        }

        private DataFrame ReadDevelopmentDataFrame(ModellingDataSettings dataSettings)
        {
            var source = dataSettings.DevelopmentDataSource;
            var parameters = dataSettings.DevelopmentFrameSamplingParameters;
            var developmentFrameReader = DataFrameReader.GetDataFrameReader(source);
            var developmentFrame = developmentFrameReader.Read(source, parameters);
            return developmentFrame;
        }

        private DataFrame ReadTestDataFrame(ModellingDataSettings dataSettings)
        {
            var source = dataSettings.TestDataSource;
            if (source == null) return null;
            var parameters = dataSettings.TestFrameSamplingParameters;
            var developmentFrameReader = DataFrameReader.GetDataFrameReader(source);
            var developmentFrame = developmentFrameReader.Read(source, parameters);
            return developmentFrame;
        }

        private void PrepareData(GbmAlgorithmSettings algorithmSettings, ModellingDataSettings dataSettings, DataFrame developmentFrame)
        {
            developmentFrame.CreateBins(algorithmSettings.MaxBins);
            developmentFrame.Partition(dataSettings.TrainingRatio);
        }

        private GbmModelBuilderResults BuildModel(GbmAlgorithmSettings algorithmSettings, ModellingDataSettings dataSettings,
                                                  GbmOutputSettings outputSettings, DataFrame developmentFrame, DataFrame testFrame)
        {
            var modelBuilder = new GbmModelBuilder(algorithmSettings, dataSettings, outputSettings);
            modelBuilder.SetFrame(SampleType.Development, developmentFrame);
            if(testFrame != null)
            {
                modelBuilder.SetFrame(SampleType.Test, testFrame);
            }
            modelBuilder.Execute(1);
            return modelBuilder.GbmModelBuilderResults;
        }
    }
}
