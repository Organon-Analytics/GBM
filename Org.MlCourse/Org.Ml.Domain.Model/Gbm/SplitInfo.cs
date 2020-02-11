using Org.Infrastructure.Data;
using Org.Infrastructure.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    public class SplitInfo
    {
        public SplitInfo()
        {
            Feature = String.Empty;
            IsNumerical = false;
            IsNull = true;
            IntegerThreshold = -1;
            LeftInfoTraining = new BinInfo();
            RightInfoTraining = new BinInfo();
            LeftCategoryIndices = null;
            LeftPrediction = Double.NaN;
            RightPrediction = Double.NaN;
            Gain = Double.MinValue;
            DefaultOnLeft = true;
            DoesDefaultExist = false;
        }

        /*! \brief Feature index */
        public string Feature { get; set; }
        public bool IsNumerical { get; set; }
        public bool IsNull { get; set; }
        /*! \brief Split threshold */
        public int IntegerThreshold { get; set; }
        public IList<int> LeftCategoryIndices { get; set; }
        //public IList<int> RightCategoryIndices { get; set; }

        public BinInfo LeftInfoTraining { get; set; }
        public BinInfo RightInfoTraining { get; set; }
        public BinInfo LeftInfoValidation { get; set; }
        public BinInfo RightInfoValidation { get; set; }

        /*! \brief Left output after split */
        public double LeftPrediction { get; set; }
        /*! \brief Right output after split */
        public double RightPrediction { get; set; }
        /*! \brief Split gain */
        public double Gain { get; set; }
        public int DefaultIdx { get; set; }
        public bool DefaultOnLeft { get; set; }
        public bool DoesDefaultExist { get; set; }
        public double OrphanPrediction { get; set; }

        public BinInfo GetLeftBinInfoTraining()
        {
            return new BinInfo(LeftInfoTraining);
        }

        public BinInfo GetRightBinInfoTraining()
        {
            return new BinInfo(RightInfoTraining);
        }

        public BinInfo GetLeftBinInfoValidation()
        {
            return new BinInfo(LeftInfoValidation);
        }

        public BinInfo GetRightBinInfoValidation()
        {
            return new BinInfo(RightInfoValidation);
        }

        public IndicatorFunction GetLeftIndicator()
        {
            if (IsNumerical)
            {
                return new NumericalIndicator(Feature, DoesDefaultExist, DefaultOnLeft, IntegerThreshold, ComparisonOperator.LesserOrEqual);
            }
            else
            {
                return new CategoricalIndicator(Feature, LeftCategoryIndices, DoesDefaultExist, DefaultOnLeft, false);
            }
        }

        public IndicatorFunction GetRightIndicator()
        {
            if (IsNumerical)
            {
                return new NumericalIndicator(Feature, DoesDefaultExist, DefaultOnLeft, IntegerThreshold, ComparisonOperator.Greater);
            }
            else
            {
                return new CategoricalIndicator(Feature, LeftCategoryIndices, DoesDefaultExist, DefaultOnLeft, true);
            }
        }

    }
}
