using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tPadNeuralNet
{
    class SigmoidFunction: IActivationFunction
    {
        public double Evaluate(double value)
        {
            return 1.0 / (1.0 + Math.Exp(-1 * value));
        }

        public double EvaluateAtDerivative(double value)
        {
            return Math.Exp(value) / Math.Pow(1.0 + Math.Exp(value), 2);
        }


        public double[] Evaluate(double[] values)
        {
            double[] result = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
                result[i] = Evaluate(values[i]);

            return result;
        }
    }
}
