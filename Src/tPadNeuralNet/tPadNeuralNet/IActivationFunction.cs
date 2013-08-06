using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tPadNeuralNet
{
    interface IActivationFunction
    {
        double Evaluate(double value);
        double[] Evaluate(double[] values);
        double EvaluateAtDerivative(double value);
    }
}
