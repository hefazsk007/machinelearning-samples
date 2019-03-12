using System;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Data;
using MulticlassClassification_Iris.DataStructures;

namespace MulticlassClassification_Iris
{
    public static partial class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        private static string BaseDatasetsLocation = @"../../../../Data";
        private static string TrainDataPath = $"{BaseDatasetsLocation}/iris-train.txt";
        private static string TestDataPath = $"{BaseDatasetsLocation}/iris-test.txt";

        private static string BaseModelsPath = @"../../../../MLModels";
        private static string ModelPath = $"{BaseModelsPath}/IrisClassificationModel.zip";

        private static void Main(string[] args)
        {
            // Create MLContext to be shared across the model creation workflow objects 
            // Set a random seed for repeatable/deterministic results across multiple trainings.
            var mlContext = new MLContext(seed: 0);
            

            //1.
            BuildTrainEvaluateAndSaveModel(mlContext);

            //2.
            TestSomePredictions(mlContext);

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

        private static void BuildTrainEvaluateAndSaveModel(MLContext mlContext)
        {
            // STEP 1: Common data loading configuration
            var trainingDataView = mlContext.Data.ReadFromTextFile<IrisData>(TrainDataPath, hasHeader: true);
            var testDataView = mlContext.Data.ReadFromTextFile<IrisData>(TestDataPath, hasHeader: true);


            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.Concatenate(DefaultColumnNames.Features, nameof(IrisData.SepalLength),
                                                                                   nameof(IrisData.SepalWidth),
                                                                                   nameof(IrisData.PetalLength),
                                                                                   nameof(IrisData.PetalWidth))
                                                                       .AppendCacheCheckpoint(mlContext)
                                                                       // Use in-memory cache for small/medium datasets to lower training time. 
                                                                       // Do NOT use it (remove .AppendCacheCheckpoint()) when handling very large datasets. 
                                                                       .Append(mlContext.Transforms.Conversion.MapValueToKey(nameof(Iris.Label)));

            // STEP 3: Set the training algorithm, then append the trainer to the pipeline  
            var trainer = mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent(labelColumn: DefaultColumnNames.Label, featureColumn: DefaultColumnNames.Features);
            var trainingPipeline = dataProcessPipeline.Append(trainer).Append(mlContext.Transofrms.Conversion.MapKeyToValue("label", "PredictedLabel"));

            // STEP 4: Train the model fitting to the DataSet

            //Measure training time
            var watch = System.Diagnostics.Stopwatch.StartNew();

            Console.WriteLine("=============== Training the model ===============");
            ITransformer trainedModel = trainingPipeline.Fit(trainingDataView);

            //Stop measuring time
            watch.Stop();
            long elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"***** Training time: {elapsedMs/1000} seconds *****");


            // STEP 5: Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var predictions = trainedModel.Transform(testDataView);
            var metrics = mlContext.MulticlassClassification.Evaluate(predictions, "Label", "Score");

            Common.ConsoleHelper.PrintMultiClassClassificationMetrics(trainer.ToString(), metrics);

            // STEP 6: Save/persist the trained model to a .ZIP file
            using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(trainedModel, fs);

            Console.WriteLine("The model is saved to {0}", ModelPath);
        }

        private static void TestSomePredictions(MLContext mlContext)
        {
            //Test Classification Predictions with some hard-coded samples 
            
            Labeler labeler = new Labeler();
            ITransformer trainedModel;
            FullPrediction fullPredictions;

            using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                trainedModel = mlContext.Model.Load(stream);
            }

            // Create prediction engine related to the loaded trained model
            var predEngine = trainedModel.CreatePredictionEngine<IrisData, IrisPrediction>(mlContext);

            ////Score sample 1
            //var resultprediction1 = predEngine.Predict(SampleIrisData.Iris1);

            //Console.WriteLine($"Actual: setosa.     Predicted probability: setosa:      {resultprediction1.Score[0]:0.####}");
            //Console.WriteLine($"                                           versicolor:  {resultprediction1.Score[1]:0.####}");
            //Console.WriteLine($"                                           virginica:   {resultprediction1.Score[2]:0.####}");
            //Console.WriteLine();

            ////Score sample 2
            //var resultprediction2 = predEngine.Predict(SampleIrisData.Iris2);

            //Console.WriteLine($"Actual: Virginica.     Predicted probability: setosa:      {resultprediction2.Score[0]:0.####}");
            //Console.WriteLine($"                                           versicolor:  {resultprediction2.Score[1]:0.####}");
            //Console.WriteLine($"                                           virginica:   {resultprediction2.Score[2]:0.####}");
            //Console.WriteLine();

            ////Score sample 3
            //var resultprediction3 = predEngine.Predict(SampleIrisData.Iris3);

            //Console.WriteLine($"Actual: setosa.     Predicted probability: setosa:      {resultprediction3.Score[0]:0.####}");
            //Console.WriteLine($"                                           versicolor:  {resultprediction3.Score[1]:0.####}");
            //Console.WriteLine($"                                           virginica:   {resultprediction3.Score[2]:0.####}");
            //Console.WriteLine();
            labeler.TestSomePredictions(predEngine);

        }

        

    public partial class Labeler
    {
        private FullPrediction[] _fullPredictions;
        

            public void TestSomePredictions(PredictionEngine<IrisData, IrisPrediction> predEngine)
            {
                Labeler labeler = new Labeler();

                var prediction = predEngine.Predict(SampleIrisData.Iris1);

                _fullPredictions = GetBestThreePredictions(prediction, predEngine);

                Console.WriteLine("1st Label: " + _fullPredictions[0].PredictedLabel + " with score: " + _fullPredictions[0].Score);
                Console.WriteLine("2nd Label: " + _fullPredictions[1].PredictedLabel + " with score: " + _fullPredictions[1].Score);
                Console.WriteLine("3rd Label: " + _fullPredictions[2].PredictedLabel + " with score: " + _fullPredictions[2].Score);


            }

            private FullPrediction[] GetBestThreePredictions(IrisPrediction prediction, PredictionEngine<IrisData, IrisPrediction> predEngine)
            {
                float[] scores = prediction.Score;
                int size = scores.Length;
                int index0, index1, index2 = 0;

                //VBuffer<ReadOnlyMemory<char>> slotNames = default;
                //var l= predEngine.OutputSchema[nameof(IrisPrediction.Score)].HasSlotNames();
                //predEngine.OutputSchema[nameof(IrisPrediction.Score)].GetSlotNames(ref slotNames);
                VBuffer<ReadOnlyMemory<char>> keys = default;
                //predEngine.OutputSchema[nameof(IrisPrediction.PredictedLabel)].GetKeyValues(ref keys);

                GetIndexesOfTopThreeScores(scores, size, out index0, out index1, out index2);

                //_fullPredictions = new FullPrediction[]
                //    {
                //    new FullPrediction(slotNames.GetItemOrDefault(index0).ToString(),scores[index0],index0),
                //    new FullPrediction(slotNames.GetItemOrDefault(index1).ToString(),scores[index1],index1),
                //    new FullPrediction(slotNames.GetItemOrDefault(index2).ToString(),scores[index2],index2)
                //    };

                return _fullPredictions;
            }

            private void GetIndexesOfTopThreeScores(float[] scores, int n, out int index0, out int index1, out int index2)
            {
                int i;
                float first, second, third;
                index0 = index1 = index2 = 0;
                if (n < 3)
                {
                    Console.WriteLine("Invalid Input");
                    return;
                }
                third = first = second = 000;
                for (i = 0; i < n; i++)
                {
                    // If current element is  
                    // smaller than first 
                    if (scores[i] > first)
                    {
                        third = second;
                        second = first;
                        first = scores[i];
                    }
                    // If arr[i] is in between first 
                    // and second then update second 
                    else if (scores[i] > second)
                    {
                        third = second;
                        second = scores[i];
                    }

                    else if (scores[i] > third)
                        third = scores[i];
                }
                var scoresList = scores.ToList();
                index0 = scoresList.IndexOf(first);
                index1 = scoresList.IndexOf(second);
                index2 = scoresList.IndexOf(third);
            }
        }
    }
}
