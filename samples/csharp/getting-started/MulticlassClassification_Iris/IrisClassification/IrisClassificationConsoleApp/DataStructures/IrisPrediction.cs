
using Microsoft.ML.Data;

namespace MulticlassClassification_Iris.DataStructures
{
    public class IrisPrediction
    {
        [ColumnName("label")]
        public float Label;
       
        public float[] Score;
    }
}