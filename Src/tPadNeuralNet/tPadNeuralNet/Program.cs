using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tPadNeuralNet
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int r = 0; r < 1; r++)
            {
                //Console.WriteLine("\nBegin building neural network");
                //Console.WriteLine("Reading in data");
                double[][] allData = GetAllData(@"../../Data/tpad encoded.csv");
                

                //Console.WriteLine("\nFirst 6 rows of entire 150-item data set:");
                //ShowMatrix(allData, 6, 1, true);

                //Console.WriteLine("Creating 80% training and 20% test data matrices");
                double[][] trainData = null;
                double[][] testData = null;
                MakeTrainTest(allData, out trainData, out testData);

                //Console.WriteLine("\nFirst 5 rows of training data:");
                //ShowMatrix(trainData, 5, 1, true);
                //Console.WriteLine("First 3 rows of test data:");
                //ShowMatrix(testData, 3, 1, true);

                double bestAccuracy = double.MinValue;
                int bestNumHidden = -1;
                for (int numHidden = 0; numHidden < 20; numHidden++)
                {
                    const int numInput = 3;
                    const int numOutput = 1;
                    //Console.WriteLine("\nCreating a {0}-input, {1}-hidden, {2}-output neural network", numInput, numHidden, numOutput);
                    //Console.Write("Hard-coded tanh function for input-to-hidden and softmax for ");
                    //Console.WriteLine("hidden-to-output activations");
                    
                    NeuralNetwork nn = new NeuralNetwork(numInput, numHidden, numOutput, new SigmoidFunction(), new SigmoidFunction());

                    //Console.WriteLine("\nInitializing weights and bias to small random values");
                    nn.InitializeWeights();

                    int maxEpochs = 10;
                    double learnRate = 0.05;
                    double momentum = 0.01;
                    double weightDecay = 0.0001;
                    //Console.WriteLine("Setting maxEpochs = {0}, learnRate = {1}, momentum = {2}, weightDecay = {3}", maxEpochs, learnRate, momentum, weightDecay);
                    //Console.WriteLine("Training has hard-coded mean squared error < 0.020 stopping condition");

                    //Console.WriteLine("\nBeginning training using incremental back-propagation\n");
                    nn.Train(trainData, maxEpochs, learnRate, momentum, weightDecay);
                    //Console.WriteLine("Training complete");

                    //double[] weights = nn.GetWeights();
                    //Console.WriteLine("Final neural network weights and bias values:");
                    //ShowVector(weights, 10, 3, true);

                    double trainAcc = nn.Accuracy(trainData);
                    Console.WriteLine("Accuracy on training data = " + trainAcc.ToString("F4"));

                    double testAcc = nn.Accuracy(testData);
                    Console.WriteLine("Accuracy on test data = " + testAcc.ToString("F4"));

                    Console.WriteLine("End neural network \n");


                    if(testAcc > bestAccuracy)
                    {
                        bestAccuracy = testAcc;
                        bestNumHidden = numHidden;
                    }
                }

                Console.WriteLine("Best accuracy of {0} found with {1} hidden nodes", bestAccuracy, bestNumHidden);
                Console.ReadLine();
            }

        } // Main

        private static double[][] GetAllData(string fileName)
        {
            double[][] result;

            //get the line and data count
            var lineCount = File.ReadLines(fileName).Count();

            //read in the data
            result = new double[lineCount][];
            using (StreamReader reader = new StreamReader(fileName))
            {
                for (int i = 0; i < lineCount; i++)
                {
                    var data = reader.ReadLine().Split(',');

                    result[i] = new double[data.Count()];
                    for (int j = 0; j < data.Count(); j++ )
                        result[i][j] = double.Parse(data[j]);
                    
                }
            }
            

            return result;
        }

        static void MakeTrainTest(double[][] allData, out double[][] trainData, out double[][] testData)
        {
            // split allData into 80% trainData and 20% testData
            Random rnd = new Random(0);
            int totRows = allData.Length;
            int numCols = allData[0].Length;

            int trainRows = (int)(totRows * 0.80); // hard-coded 80-20 split
            int testRows = totRows - trainRows;

            trainData = new double[trainRows][];
            testData = new double[testRows][];

            int[] sequence = new int[totRows]; // create a random sequence of indexes
            for (int i = 0; i < sequence.Length; ++i)
                sequence[i] = i;

            for (int i = 0; i < sequence.Length; ++i)
            {
                int r = rnd.Next(i, sequence.Length);
                int tmp = sequence[r];
                sequence[r] = sequence[i];
                sequence[i] = tmp;
            }

            int si = 0; // index into sequence[]
            int j = 0; // index into trainData or testData

            for (; si < trainRows; ++si) // first rows to train data
            {
                trainData[j] = new double[numCols];
                int idx = sequence[si];
                Array.Copy(allData[idx], trainData[j], numCols);
                ++j;
            }

            j = 0; // reset to start of test data
            for (; si < totRows; ++si) // remainder to test data
            {
                testData[j] = new double[numCols];
                int idx = sequence[si];
                Array.Copy(allData[idx], testData[j], numCols);
                ++j;
            }
        } // MakeTrainTest

        static void Normalize(double[][] dataMatrix, int[] cols)
        {
            // normalize specified cols by computing (x - mean) / sd for each value
            foreach (int col in cols)
            {
                double sum = 0.0;
                for (int i = 0; i < dataMatrix.Length; ++i)
                    sum += dataMatrix[i][col];
                double mean = sum / dataMatrix.Length;
                sum = 0.0;
                for (int i = 0; i < dataMatrix.Length; ++i)
                    sum += (dataMatrix[i][col] - mean) * (dataMatrix[i][col] - mean);
                double sd = sum / (dataMatrix.Length - 1);
                for (int i = 0; i < dataMatrix.Length; ++i)
                    dataMatrix[i][col] = (dataMatrix[i][col] - mean) / sd;
            }
        }

        static void ShowVector(double[] vector, int valsPerRow, int decimals, bool newLine)
        {
            for (int i = 0; i < vector.Length; ++i)
            {
                if (i % valsPerRow == 0) Console.WriteLine("");
                Console.Write(vector[i].ToString("F" + decimals).PadLeft(decimals + 4) + " ");
            }
            if (newLine == true) Console.WriteLine("");
        }

        static void ShowMatrix(double[][] matrix, int numRows, int decimals, bool newLine)
        {
            for (int i = 0; i < numRows; ++i)
            {
                Console.Write(i.ToString().PadLeft(3) + ": ");
                for (int j = 0; j < matrix[i].Length; ++j)
                {
                    if (matrix[i][j] >= 0.0) Console.Write(" "); else Console.Write("-"); ;
                    Console.Write(Math.Abs(matrix[i][j]).ToString("F" + decimals) + " ");
                }
                Console.WriteLine("");
            }
            if (newLine == true) Console.WriteLine("");
        }

    } // class Program
}
