using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tPadNeuralNet
{
    class HyperbolicTangent: IActivationFunction
    {
        public double Evaluate(double value)
        {
            if (value < -20.0) return -1.0; // approximation is correct to 30 decimals
            else if (value > 20.0) return 1.0;
            else return (1.0 - 2.0 / (Math.Exp(2.0 * value) + 1.0)); //faster than calling Math.tanh(value)
        }

        public double EvaluateAtDerivative(double value)
        {
            return 1 - Math.Pow(Math.Tan(value), 2);
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
