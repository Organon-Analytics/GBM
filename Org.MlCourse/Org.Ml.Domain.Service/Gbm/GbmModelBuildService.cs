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
        public GbmModelBuildService(GbmAlgorithmSettings algoSettings, ModellingDataSettings dataSettings)
        {
            _algoSettings = algoSettings;
            _dataSettings = dataSettings;
        }
        public void Execute(int numOfThreads)
        {
            ValidateAlgorithmSettings(_algoSettings);
            ValidateDataSettings(_dataSettings);
            var trainingFrame = ReadDataFrame(_dataSettings);
            var validationFrame = ReadDataFrame(_dataSettings);
            var testFrame = ReadDataFrame(_dataSettings);
            PrepareData(trainingFrame);
            var model = BuildModel(trainingFrame, validationFrame);

        }

        private void ValidateAlgorithmSettings(GbmAlgorithmSettings algoSettings)
        {
            throw new NotImplementedException();
        }

        private void ValidateDataSettings(ModellingDataSettings dataSettings)
        {
            throw new NotImplementedException();
        }

        private DataFrame ReadDataFrame(ModellingDataSettings dataSettings)
        {
            throw new NotImplementedException();
        }

        private void PrepareData(DataFrame frame)
        {
            throw new NotImplementedException();
        }

        private GbmModel BuildModel(DataFrame trainingFrame, DataFrame validationFrame)
        {
            throw new NotImplementedException();
        }
    }
}
