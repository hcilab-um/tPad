using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tPadNeuralNet
{
    public class NeuralNetwork
    {
        private static Random rnd;

        private int _numInput;
        private int _numHidden;
        private int _numOutput;

        private double[] _inputs;

        private double[][] _ihWeights; // input-hidden
        private double[] _hBiases;
        private double[] _hOutputs;

        private double[][] _hoWeights; // hidden-output
        private double[] _oBiases;

        private double[] _outputs;

        // back-prop specific arrays (these could be local to method UpdateWeights)
        private double[] _oGrads; // output gradients for back-propagation
        private double[] _hGrads; // hidden gradients for back-propagation

        // back-prop momentum specific arrays (could be local to method Train)
        private double[][] _ihPrevWeightsDelta;  // for momentum with back-propagation
        private double[] _hPrevBiasesDelta;
        private double[][] _hoPrevWeightsDelta;
        private double[] _oPrevBiasesDelta;

        private IActivationFunction _hiddenActivationFunction, _outputActivationFunction;

        /// <summary>
        /// Constructor. Defaults to the sigmoid function for both the hidden and output activation functions
        /// </summary>
        /// <param name="numInput">The number of input nodes for the neural network</param>
        /// <param name="numHidden">The number of hidden nodes for the neural network</param>
        /// <param name="numOutput">The number of output nodes for the neural networks</param>
        public NeuralNetwork(int numInput, int numHidden, int numOutput)
        {
            rnd = new Random(0); // for InitializeWeights() and Shuffle()

            this._numInput = numInput;
            this._numHidden = numHidden;
            this._numOutput = numOutput;

            this._inputs = new double[numInput];

            this._ihWeights = MakeMatrix(numInput, numHidden);
            this._hBiases = new double[numHidden];
            this._hOutputs = new double[numHidden];

            this._hoWeights = MakeMatrix(numHidden, numOutput);
            this._oBiases = new double[numOutput];

            this._outputs = new double[numOutput];

            // back-prop related arrays below
            this._hGrads = new double[numHidden];
            this._oGrads = new double[numOutput];

            this._ihPrevWeightsDelta = MakeMatrix(numInput, numHidden);
            this._hPrevBiasesDelta = new double[numHidden];
            this._hoPrevWeightsDelta = MakeMatrix(numHidden, numOutput);
            this._oPrevBiasesDelta = new double[numOutput];

            this._hiddenActivationFunction = new SigmoidFunction();
            this._hiddenActivationFunction = new SigmoidFunction();
        } // ctor

        /// <summary>
        /// Overload that lets you specify the activation functions for the hidden and output layers
        /// </summary>
        /// <param name="numInput"></param>
        /// <param name="numHidden"></param>
        /// <param name="numOutput"></param>
        /// <param name="hiddenFunction"></param>
        /// <param name="outputFunction"></param>
        public NeuralNetwork(int numInput, int numHidden, int numOutput, IActivationFunction hiddenFunction, IActivationFunction outputFunction)
            : this(numInput, numHidden, numOutput)
        {
            this._hiddenActivationFunction = hiddenFunction;
            this._outputActivationFunction = outputFunction;
        }

        private static double[][] MakeMatrix(int rows, int cols) // helper for ctor
        {
            double[][] result = new double[rows][];
            for (int r = 0; r < result.Length; ++r)
                result[r] = new double[cols];
            return result;
        }

        public override string ToString() // yikes
        {
            string s = "";
            s += "===============================\n";
            s += "numInput = " + _numInput + " numHidden = " + _numHidden + " numOutput = " + _numOutput + "\n\n";

            s += "inputs: \n";
            for (int i = 0; i < _inputs.Length; ++i)
                s += _inputs[i].ToString("F2") + " ";
            s += "\n\n";

            s += "ihWeights: \n";
            for (int i = 0; i < _ihWeights.Length; ++i)
            {
                for (int j = 0; j < _ihWeights[i].Length; ++j)
                {
                    s += _ihWeights[i][j].ToString("F4") + " ";
                }
                s += "\n";
            }
            s += "\n";

            s += "hBiases: \n";
            for (int i = 0; i < _hBiases.Length; ++i)
                s += _hBiases[i].ToString("F4") + " ";
            s += "\n\n";

            s += "hOutputs: \n";
            for (int i = 0; i < _hOutputs.Length; ++i)
                s += _hOutputs[i].ToString("F4") + " ";
            s += "\n\n";

            s += "hoWeights: \n";
            for (int i = 0; i < _hoWeights.Length; ++i)
            {
                for (int j = 0; j < _hoWeights[i].Length; ++j)
                {
                    s += _hoWeights[i][j].ToString("F4") + " ";
                }
                s += "\n";
            }
            s += "\n";

            s += "oBiases: \n";
            for (int i = 0; i < _oBiases.Length; ++i)
                s += _oBiases[i].ToString("F4") + " ";
            s += "\n\n";

            s += "hGrads: \n";
            for (int i = 0; i < _hGrads.Length; ++i)
                s += _hGrads[i].ToString("F4") + " ";
            s += "\n\n";

            s += "oGrads: \n";
            for (int i = 0; i < _oGrads.Length; ++i)
                s += _oGrads[i].ToString("F4") + " ";
            s += "\n\n";

            s += "ihPrevWeightsDelta: \n";
            for (int i = 0; i < _ihPrevWeightsDelta.Length; ++i)
            {
                for (int j = 0; j < _ihPrevWeightsDelta[i].Length; ++j)
                {
                    s += _ihPrevWeightsDelta[i][j].ToString("F4") + " ";
                }
                s += "\n";
            }
            s += "\n";

            s += "hPrevBiasesDelta: \n";
            for (int i = 0; i < _hPrevBiasesDelta.Length; ++i)
                s += _hPrevBiasesDelta[i].ToString("F4") + " ";
            s += "\n\n";

            s += "hoPrevWeightsDelta: \n";
            for (int i = 0; i < _hoPrevWeightsDelta.Length; ++i)
            {
                for (int j = 0; j < _hoPrevWeightsDelta[i].Length; ++j)
                {
                    s += _hoPrevWeightsDelta[i][j].ToString("F4") + " ";
                }
                s += "\n";
            }
            s += "\n";

            s += "oPrevBiasesDelta: \n";
            for (int i = 0; i < _oPrevBiasesDelta.Length; ++i)
                s += _oPrevBiasesDelta[i].ToString("F4") + " ";
            s += "\n\n";

            s += "outputs: \n";
            for (int i = 0; i < _outputs.Length; ++i)
                s += _outputs[i].ToString("F2") + " ";
            s += "\n\n";

            s += "===============================\n";
            return s;
        }

        // ----------------------------------------------------------------------------------------

        public void SetWeights(double[] weights)
        {
            // copy weights and biases in weights[] array to i-h weights, i-h biases, h-o weights, h-o biases
            int numWeights = (_numInput * _numHidden) + (_numHidden * _numOutput) + _numHidden + _numOutput;
            if (weights.Length != numWeights)
                throw new Exception("Bad weights array length: ");

            int k = 0; // points into weights param

            for (int i = 0; i < _numInput; ++i)
                for (int j = 0; j < _numHidden; ++j)
                    _ihWeights[i][j] = weights[k++];
            for (int i = 0; i < _numHidden; ++i)
                _hBiases[i] = weights[k++];
            for (int i = 0; i < _numHidden; ++i)
                for (int j = 0; j < _numOutput; ++j)
                    _hoWeights[i][j] = weights[k++];
            for (int i = 0; i < _numOutput; ++i)
                _oBiases[i] = weights[k++];
        }

        public void InitializeWeights()
        {
            // initialize weights and biases to small random values
            int numWeights = (_numInput * _numHidden) + (_numHidden * _numOutput) + _numHidden + _numOutput;
            double[] initialWeights = new double[numWeights];
            double lo = -0.01;
            double hi = 0.01;
            for (int i = 0; i < initialWeights.Length; ++i)
                initialWeights[i] = (hi - lo) * rnd.NextDouble() + lo;
            this.SetWeights(initialWeights);
        }

        public double[] GetWeights()
        {
            // returns the current set of wweights, presumably after training
            int numWeights = (_numInput * _numHidden) + (_numHidden * _numOutput) + _numHidden + _numOutput;
            double[] result = new double[numWeights];
            int k = 0;
            for (int i = 0; i < _ihWeights.Length; ++i)
                for (int j = 0; j < _ihWeights[0].Length; ++j)
                    result[k++] = _ihWeights[i][j];
            for (int i = 0; i < _hBiases.Length; ++i)
                result[k++] = _hBiases[i];
            for (int i = 0; i < _hoWeights.Length; ++i)
                for (int j = 0; j < _hoWeights[0].Length; ++j)
                    result[k++] = _hoWeights[i][j];
            for (int i = 0; i < _oBiases.Length; ++i)
                result[k++] = _oBiases[i];
            return result;
        }

        // ----------------------------------------------------------------------------------------

        private double[] ComputeOutputs(double[] xValues)
        {
            if (xValues.Length != _numInput)
                throw new Exception("Bad xValues array length");

            double[] hSums = new double[_numHidden]; // hidden nodes sums scratch array
            double[] oSums = new double[_numOutput]; // output nodes sums

            for (int i = 0; i < xValues.Length; ++i) // copy x-values to inputs
                this._inputs[i] = xValues[i];

            for (int j = 0; j < _numHidden; ++j)  // compute i-h sum of weights * inputs
                for (int i = 0; i < _numInput; ++i)
                    hSums[j] += this._inputs[i] * this._ihWeights[i][j]; // note +=

            for (int i = 0; i < _numHidden; ++i)  // add biases to input-to-hidden sums
                hSums[i] += this._hBiases[i];

            for (int i = 0; i < _numHidden; ++i)   // apply activation
                this._hOutputs[i] = this._hiddenActivationFunction.Evaluate(hSums[i]); // hard-coded

            for (int j = 0; j < _numOutput; ++j)   // compute h-o sum of weights * hOutputs
                for (int i = 0; i < _numHidden; ++i)
                    oSums[j] += _hOutputs[i] * _hoWeights[i][j];

            for (int i = 0; i < _numOutput; ++i)  // add biases to input-to-hidden sums
                oSums[i] += _oBiases[i];

            //double[] softOut = Softmax(oSums); // softmax activation does all outputs at once for efficiency
            //Array.Copy(softOut, outputs, softOut.Length);
            for (int i = 0; i < this._outputs.Length; i++)
                this._outputs[i] = this._outputActivationFunction.Evaluate(oSums[i]);

            double[] retResult = new double[_numOutput]; // could define a GetOutputs method instead
            Array.Copy(this._outputs, retResult, retResult.Length);
            return retResult;
        } // ComputeOutputs

        // ----------------------------------------------------------------------------------------

        private void UpdateWeights(double[] tValues, double learnRate, double momentum, double weightDecay)
        {
            // update the weights and biases using back-propagation, with target values, eta (learning rate),
            // alpha (momentum).
            // assumes that SetWeights and ComputeOutputs have been called and so all the internal arrays
            // and matrices have values (other than 0.0)
            if (tValues.Length != _numOutput)
                throw new Exception("target values not same Length as output in UpdateWeights");

            // 1. compute output gradients
            for (int i = 0; i < _oGrads.Length; ++i)
            {
                // derivative of softmax = (1 - y) * y (same as log-sigmoid)
                double derivative = this._outputActivationFunction.EvaluateAtDerivative(_outputs[i]);
                // 'mean squared error version' includes (1-y)(y) derivative
                _oGrads[i] = derivative * (tValues[i] - _outputs[i]);
            }

            // 2. compute hidden gradients
            for (int i = 0; i < _hGrads.Length; ++i)
            {
                // derivative of tanh = (1 - y) * (1 + y)
                double derivative = this._hiddenActivationFunction.EvaluateAtDerivative(_hOutputs[i]);
                double sum = 0.0;
                for (int j = 0; j < _numOutput; ++j) // each hidden delta is the sum of numOutput terms
                {
                    double x = _oGrads[j] * _hoWeights[i][j];
                    sum += x;
                }
                _hGrads[i] = derivative * sum;
            }

            // 3a. update hidden weights (gradients must be computed right-to-left but weights
            // can be updated in any order)
            for (int i = 0; i < _ihWeights.Length; ++i) // 0..2 (3)
            {
                for (int j = 0; j < _ihWeights[0].Length; ++j) // 0..3 (4)
                {
                    double delta = learnRate * _hGrads[j] * _inputs[i]; // compute the new delta
                    _ihWeights[i][j] += delta; // update. note we use '+' instead of '-'. this can be very tricky.
                    // now add momentum using previous delta. on first pass old value will be 0.0 but that's OK.
                    _ihWeights[i][j] += momentum * _ihPrevWeightsDelta[i][j];
                    _ihWeights[i][j] -= (weightDecay * _ihWeights[i][j]); // weight decay
                    _ihPrevWeightsDelta[i][j] = delta; // don't forget to save the delta for momentum 
                }
            }

            // 3b. update hidden biases
            for (int i = 0; i < _hBiases.Length; ++i)
            {
                double delta = learnRate * _hGrads[i] * 1.0; // t1.0 is constant input for bias; could leave out
                _hBiases[i] += delta;
                _hBiases[i] += momentum * _hPrevBiasesDelta[i]; // momentum
                _hBiases[i] -= (weightDecay * _hBiases[i]); // weight decay
                _hPrevBiasesDelta[i] = delta; // don't forget to save the delta
            }

            // 4. update hidden-output weights
            for (int i = 0; i < _hoWeights.Length; ++i)
            {
                for (int j = 0; j < _hoWeights[0].Length; ++j)
                {
                    // see above: hOutputs are inputs to the nn outputs
                    double delta = learnRate * _oGrads[j] * _hOutputs[i];
                    _hoWeights[i][j] += delta;
                    _hoWeights[i][j] += momentum * _hoPrevWeightsDelta[i][j]; // momentum
                    _hoWeights[i][j] -= (weightDecay * _hoWeights[i][j]); // weight decay
                    _hoPrevWeightsDelta[i][j] = delta; // save
                }
            }

            // 4b. update output biases
            for (int i = 0; i < _oBiases.Length; ++i)
            {
                double delta = learnRate * _oGrads[i] * 1.0;
                _oBiases[i] += delta;
                _oBiases[i] += momentum * _oPrevBiasesDelta[i]; // momentum
                _oBiases[i] -= (weightDecay * _oBiases[i]); // weight decay
                _oPrevBiasesDelta[i] = delta; // save
            }
        } // UpdateWeights

        // ----------------------------------------------------------------------------------------

        public void Train(double[][] trainData, int maxEprochs, double learnRate, double momentum,
          double weightDecay)
        {
            // train a back-prop style NN classifier using learning rate and momentum
            // weight decay reduces the magnitude of a weight value over time unless that value
            // is constantly increased
            int epoch = 0;
            double[] inputs = new double[_numInput]; // inputs
            double[] targetValues = new double[_numOutput]; // target values

            int[] sequence = new int[trainData.Length];
            for (int i = 0; i < sequence.Length; ++i)
                sequence[i] = i;

            while (epoch < maxEprochs)
            {
                double mse = MeanSquaredError(trainData);
                if (mse < 0.020) break; // consider passing value in as parameter
                //if (mse < 0.001) break; // consider passing value in as parameter

                Shuffle(sequence); // visit each training data in random order
                for (int i = 0; i < trainData.Length; ++i)
                {
                    int index = sequence[i];
                    Array.Copy(trainData[index], inputs, _numInput);
                    Array.Copy(trainData[index], _numInput, targetValues, 0, _numOutput);
                    ComputeOutputs(inputs); // copy xValues in, compute outputs (store them internally)
                    UpdateWeights(targetValues, learnRate, momentum, weightDecay); // find better weights
                } // each training tuple
                ++epoch;
            }
        } // Train

        private static void Shuffle(int[] sequence)
        {
            for (int i = 0; i < sequence.Length; ++i)
            {
                int r = rnd.Next(i, sequence.Length);
                int tmp = sequence[r];
                sequence[r] = sequence[i];
                sequence[i] = tmp;
            }
        }

        private double MeanSquaredError(double[][] trainData) // used as a training stopping condition
        {
            // average squared error per training tuple
            double sumSquaredError = 0.0;
            double[] xValues = new double[_numInput]; // first numInput values in trainData
            double[] tValues = new double[_numOutput]; // last numOutput values

            // walk thru each training case. looks like (6.9 3.2 5.7 2.3) (0 0 1)
            for (int i = 0; i < trainData.Length; ++i)
            {
                Array.Copy(trainData[i], xValues, _numInput);
                Array.Copy(trainData[i], _numInput, tValues, 0, _numOutput); // get target values
                double[] yValues = this.ComputeOutputs(xValues); // compute output using current weights
                for (int j = 0; j < _numOutput; ++j)
                {
                    double err = tValues[j] - yValues[j];
                    sumSquaredError += err * err;
                }
            }

            return sumSquaredError / trainData.Length;
        }

        // ----------------------------------------------------------------------------------------

        public double Accuracy(double[][] testData)
        {
            // percentage correct using winner-takes all
            int numCorrect = 0;
            int numWrong = 0;
            double[] xValues = new double[_numInput]; // inputs
            double[] tValues = new double[_numOutput]; // targets
            double[] yValues; // computed Y

            for (int i = 0; i < testData.Length; ++i)
            {
                Array.Copy(testData[i], xValues, _numInput); // parse test data into x-values and t-values
                Array.Copy(testData[i], _numInput, tValues, 0, _numOutput);
                yValues = this.ComputeOutputs(xValues); //this should be a entry array

                if (Math.Round(yValues[0], MidpointRounding.AwayFromZero).Equals(tValues[0])) // ugly. consider AreEqual(double x, double y)
                    ++numCorrect;
                else
                    ++numWrong;
            }
            return (numCorrect * 1.0) / (numCorrect + numWrong); // ugly 2 - check for divide by zero
        }

        private static int MaxIndex(double[] vector) // helper for Accuracy()
        {
            // index of largest value
            int bigIndex = 0;
            double biggestVal = vector[0];
            for (int i = 0; i < vector.Length; ++i)
            {
                if (vector[i] > biggestVal)
                {
                    biggestVal = vector[i]; bigIndex = i;
                }
            }
            return bigIndex;
        }

    }
}
