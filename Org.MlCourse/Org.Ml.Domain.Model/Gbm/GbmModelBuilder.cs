using Org.Infrastructure.Data;
using Org.Infrastructure.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    /// <summary>
    /// GbmModelBuilder is a Domain Service that builds a GbmModel and returns it
    /// It accepts two inputs: GbmAlgorithmSettings and ModellingDataSettings
    /// GbmAlgorithmSettings contains the input parameters that are used to build model-building process:
    /// Some of these parameters are hyper-parameters that needs to be optimized during model building process
    /// ModellingDataSettings contains all the necessary information about the input data
    /// </summary>
    public class GbmModelBuilder
    {
        private readonly GbmAlgorithmSettings _algorithmSettings;
        private readonly ModellingDataSettings _dataSettings;
        /// <summary>
        /// Multiple data frames are needed: 
        /// Development sample is used to build and validate the model
        /// Test sample is used to measure the final performance of the built&validated model
        /// </summary>
        private readonly IDictionary<SampleType, DataFrame> _frames;
        /// <summary>
        /// Various results are produced as a result of the computation: model, run-time report, predictions on development sample
        /// These data are embedded in a GbmModelBuilderResults object
        /// </summary>
        private GbmModelBuilderResults _results;
        /// <summary>
        /// A single constructor to the class. Accepts the two input objects
        /// </summary>
        /// <param name="algorithmSettings"></param>
        /// <param name="dataSettings"></param>
        public GbmModelBuilder(GbmAlgorithmSettings algorithmSettings, ModellingDataSettings dataSettings)
        {
            _algorithmSettings = algorithmSettings;
            _dataSettings = dataSettings;

            _frames = new Dictionary<SampleType, DataFrame>();
        }
        /// <summary>
        /// Returns the results for further referencing after model has been built
        /// </summary>
        public GbmModelBuilderResults GbmModelBuilderResults
        {
            get { return _results; }
        }
        /// <summary>
        /// Sets the related frame that will be used for development or testing
        /// One can set DevelopmentFrame and TestFrame
        /// </summary>
        /// <param name="sampleType"></param>
        /// <param name="frame"></param>
        public void SetFrame(SampleType sampleType, DataFrame frame)
        {
            if (_frames.ContainsKey(sampleType))
            {
                _frames[sampleType] = frame;
            }
            else
            {
                _frames.Add(sampleType, frame);
            }
        }
        /// <summary>
        /// This is the method that does the following:
        /// 1 Build a GBM model
        /// 2 Validate the model. Carry out hyper-parameter search if necessary
        /// 3 Measure test performance if specified
        /// 4 Produce and package results in a GbmModelBuilderResults object for later referencing
        /// 5 Produce logs and reports
        /// It takes a numOfThreads argument. The program becomes multi-threaded if (numOfThreads > 1)
        /// </summary>
        /// <param name="numOfThreads"></param>
        public void Execute(int numOfThreads)
        {
            // _frames should contain a development sample. Throw an exception otherwise
            if (!_frames.ContainsKey(SampleType.Development))
            {
                throw new ArgumentException("Development frame could not be found as input to Gbm algorithm");
            }
            // Fetch the development sample
            var developmentFrame = _frames[SampleType.Development];
            // GbmTreeLearner returns a single tree of a Gbm model
            var treeLearner = new GbmTreeLearner(_algorithmSettings, _dataSettings, developmentFrame);
            // Initialize the private fields in treeLearner object
            treeLearner.Initialize();

            var loss = LossFunction.CreateLossFunction(_algorithmSettings.LossFunctionType);
            loss.Initialize(_algorithmSettings, _dataSettings, developmentFrame);
            var rowCount = developmentFrame.GetRowCount();
            var binCollection = developmentFrame.GetBinCollection();
            var bestScores = new double[rowCount];

            var container = new GbmGlobalArrays(rowCount);
            var blas = new DotNetBlas();
            var rng = new Random();

            GbmModelDetail modelDetail = null;
            var predictions = new double[rowCount];
            var parameterSearchResults = new List<GbmHyperParameterSearchResult>();
            if (_algorithmSettings.SearchForHyperParameters)
            {
                var hyperParameterSets = GenerateHyperParameterSets(_algorithmSettings);
                var trial = 0;

                var minimumLoss = Double.MaxValue;
                foreach (var parameters in hyperParameterSets)
                {
                    var loopWatch = new Stopwatch();
                    loopWatch.Start();

                    UpdateSettings(_algorithmSettings, parameters);
                    var result = BuildASingleModel(treeLearner, _algorithmSettings, container, loss, binCollection, rowCount, blas);
                    var searchResult = result.Item1;
                    parameterSearchResults.Add(searchResult);

                    var currentModelDetail = result.Item2;
                    var currentLoss = Double.IsNaN(searchResult.ValidationLoss)
                        ? searchResult.TrainingLoss
                        : searchResult.ValidationLoss;
                    if (currentLoss < minimumLoss)
                    {
                        minimumLoss = currentLoss;
                        modelDetail = currentModelDetail;
                        blas.Copy(container.Scores, bestScores, container.Scores.Length);
                    }
                    Console.WriteLine("Computation for the hyper-parameter set: {0} finished in {1} seconds", trial++, loopWatch.Elapsed.TotalSeconds);
                }
            }
            else
            {
                var result = BuildASingleModel(treeLearner, _algorithmSettings, container, loss, binCollection, rowCount, blas);
                modelDetail = result.Item2;
                loss.Convert(container.Scores, predictions);
            }
            loss.Convert(container.Scores, predictions);

            _results = new GbmModelBuilderResults
            {
                GbmModelDetail = modelDetail,
                ParameterSearchResults = parameterSearchResults,
                Predictions = predictions
            };
        }


        private Tuple<GbmHyperParameterSearchResult, GbmModelDetail> BuildASingleModel(GbmTreeLearner treeLearner, GbmAlgorithmSettings algorithmSettings, GbmGlobalArrays container, LossFunction loss, Dictionary<string, Bin> binCollection, int rowCount, IBlas blas)
        {
            var watch = new Stopwatch();
            watch.Start();

            loss.SetInitialScore(container.Scores);
            var fixedRate = algorithmSettings.LearningRate;
            //var useLineSearch = algorithmSettings.UseLineSearchForLearningRate;

            var modelDetail = new GbmModelDetail(loss.GetLossFunctionType(), binCollection);
            modelDetail.SetBias(loss.GetInitialScore());
            for (var i = 0; i < algorithmSettings.NumIterations; i++)
            {
                //if (((i + 1) % 100) == 0)
                //{
                //    LogHelper.LogInfo(
                //        String.Format("Building Gbm model: {0} iterations completed in {1} seconds",
                //            i + 1, modelWatch.Elapsed.TotalSeconds), taskId);
                //}
                loss.UpdateGradients(container.Scores, container.Gradients, container.Hessians);
                var tree = treeLearner.Train(container.Gradients, container.Hessians, container.Delta);
                if (tree == null) continue;

                tree.Scale(fixedRate);
                blas.Axpy(fixedRate, 0, rowCount, container.Delta, container.Scores);
                var losses = loss.GetLoss(container.Scores);
                modelDetail.AddLoss(new LossPerIteration { TrainingLoss = losses.Item1, ValidationLoss = losses.Item2 });
                modelDetail.Add(tree);
            }
            var searchResult = GetSearchResult(algorithmSettings, watch.Elapsed.TotalSeconds, loss, container.Scores);
            return new Tuple<GbmHyperParameterSearchResult, GbmModelDetail>(searchResult, modelDetail);
        }

        #region Hyper-parameter search
        private void UpdateSettings(GbmAlgorithmSettings settings, GbmHyperParameters hyperParameters)
        {
            settings.LearningRate = hyperParameters.LearningRate;
            settings.RowSamplingRate = hyperParameters.RowSamplingRate;
            settings.ColumnSamplingRate = hyperParameters.ColumnSamplingRate;
            settings.MaxLeaves = hyperParameters.MaxTreeLeaves;
            settings.NumIterations = hyperParameters.MaxIterations;
        }

        private GbmHyperParameterSearchResult GetSearchResult(GbmAlgorithmSettings settings, double durationInSeconds,
                                                              LossFunction loss, double[] scores)
        {
            var hyperParameters = new GbmHyperParameters
            {
                LearningRate = settings.LearningRate,
                RowSamplingRate = settings.RowSamplingRate,
                ColumnSamplingRate = settings.ColumnSamplingRate,
                MaxTreeLeaves = settings.MaxLeaves,
                MaxIterations = settings.NumIterations
            };

            var searchResult = new GbmHyperParameterSearchResult
            {
                HyperParameters = hyperParameters,
                RuntimeDurationInSeconds = durationInSeconds
            };
            if (settings.LossFunctionType == LossFunctionType.CrossEntropy)
            {
                var finalLoss = loss.GetLoss(scores);
                searchResult.TrainingLoss = finalLoss.Item1;
                searchResult.ValidationLoss = finalLoss.Item2;
                var aucLoss = loss.GetAreaUnderTheCurve(scores);
                searchResult.TrainingAuc = aucLoss.Item1.Auc;
                searchResult.ValidationAuc = aucLoss.Item2 != null ? aucLoss.Item2.Auc : Double.NaN;
            }
            else if (settings.LossFunctionType == LossFunctionType.LeastSquares)
            {
                var l2Loss = loss.GetLeastSquaresLoss(scores);
                var tOutput = l2Loss.Item1;
                var vOutput = l2Loss.Item2;
                searchResult.TrainingLoss = tOutput.ResidualSumOfSquares;
                searchResult.ValidationLoss = vOutput != null ? vOutput.ResidualSumOfSquares : Double.NaN;
                searchResult.TrainingR2 = tOutput.RSquared;
                searchResult.ValidationR2 = vOutput != null ? vOutput.RSquared : Double.NaN;
            }
            else
            {
                var finalLoss = loss.GetLoss(scores);
                searchResult.TrainingLoss = finalLoss.Item1;
                searchResult.ValidationLoss = finalLoss.Item2;
            }
            return searchResult;
        }

        private IList<GbmHyperParameters> GenerateHyperParameterSets(GbmAlgorithmSettings settings)
        {
            var arrayLearningRate = GetArrayOfObjects("LearningRate", settings.GridForLearningRate,
                settings.LearningRate);
            var arrayRowSamplingRate = GetArrayOfObjects("RowSamplingRate", settings.GridForRowSamplingRate,
                settings.RowSamplingRate);
            var arrayColumnSamplingRate = GetArrayOfObjects("ColumnSamplingRate", settings.GridForColumnSamplingRate,
                settings.ColumnSamplingRate);
            var arrayMaxTreeLeaves = GetArrayOfObjects("MaxTreeLeaves", settings.GridForMaxTreeLeaves, settings.MaxLeaves);
            var arrayMaxIterations = GetArrayOfObjects("NumIterations", settings.GridForMaxIterations, settings.NumIterations);

            var sets = new List<object[]>
            {
                arrayLearningRate,
                arrayRowSamplingRate,
                arrayColumnSamplingRate,
                arrayMaxTreeLeaves,
                arrayMaxIterations
            };

            var combinations = GenerateCartesianProducts(sets);
            var list = new List<GbmHyperParameters>();
            foreach (var array in combinations)
            {
                var parameters = new GbmHyperParameters
                {
                    LearningRate = (double)array[0],
                    RowSamplingRate = (double)array[1],
                    ColumnSamplingRate = (double)array[2],
                    MaxTreeLeaves = (int)array[3],
                    MaxIterations = (int)array[4]
                };
                list.Add(parameters);
            }
            return list;
        }

        public IList<object[]> GenerateCartesianProducts(IList<object[]> sets)
        {
            var fullCardinality = GetCardinality(sets, 0);
            var size = sets.Count;
            var list = new List<object[]>(fullCardinality);
            for (var i = 0; i < fullCardinality; i++)
            {
                list.Add(new object[size]);
            }
            for (var cursor = 0; cursor < size; cursor++)
            {
                var set = sets[cursor];
                var cardinality = GetCardinality(sets, cursor);
                var rep = fullCardinality / cardinality;
                var step = cardinality / set.Length;
                var mark = 0;
                for (var i = 0; i < rep; i++)
                {
                    for (var j = 0; j < set.Length; j++)
                    {
                        for (var k = 0; k < step; k++)
                        {
                            list[mark++][cursor] = set[j];
                        }
                    }
                }
            }
            return list;
        }

        public int GetCardinality(IList<object[]> sets, int offset)
        {
            var cardinality = 1;
            for (var i = offset; i < sets.Count; i++)
            {
                var set = sets[i];
                cardinality *= set.Length;
            }
            return cardinality;
        }

        public object[] GetArrayOfObjects(string parameterName, FloatingPointGrid fGrid, double baseValue)
        {
            if (fGrid != null && fGrid.Grid != null)
            {
                var length = fGrid.Grid.Length;
                var objectArray = new object[length];
                for (int i = 0; i < length; i++)
                {
                    objectArray[i] = fGrid.Grid[i];
                }
                return objectArray;
            }
            if (Double.IsNaN(baseValue))
            {
                throw new ArgumentNullException(String.Format("Reference value is null for the parameter {0}", parameterName));
            }
            return new object[] { baseValue };
        }

        public object[] GetArrayOfObjects(string parameterName, IntegerGrid iGrid, int baseValue)
        {
            if (iGrid != null && iGrid.Grid != null)
            {
                var length = iGrid.Grid.Length;
                var objectArray = new object[length];
                for (int i = 0; i < length; i++)
                {
                    objectArray[i] = iGrid.Grid[i];
                }
                return objectArray;
            }
            return new object[] { baseValue };
        }
        #endregion
    }
}
