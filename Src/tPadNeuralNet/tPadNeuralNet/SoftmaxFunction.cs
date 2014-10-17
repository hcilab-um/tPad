using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tPadNeuralNet
{
    public class SoftmaxFunction:IActivationFunction
    {
        public double Evaluate(double value)
        {
            throw new NotImplementedException("Must evaluate all values at once. Use the overload that accepts an array");
        }

        public double[] Evaluate(double[] values)
        {
            // determine max output sum
            // does all output nodes at once so scale doesn't have to be re-computed each time
            double max = values[0];
            for (int i = 0; i < values.Length; ++i)
                if (values[i] > max) max = values[i];

            // determine scaling factor -- sum of exp(each val - max)
            double scale = 0.0;
            for (int i = 0; i < values.Length; ++i)
                scale += Math.Exp(values[i] - max);

            double[] result = new double[values.Length];
            for (int i = 0; i < values.Length; ++i)
                result[i] = Math.Exp(values[i] - max) / scale;

            return result; // now scaled so that xi sum to 1.0
        }

        /// <summary>
        /// Evaluate a value at the derivative of the function.
        /// </summary>
        /// <param name="value">The value to be evaluated. **NOTE** this function assumes that this value is one of the results in the array returned from the EvaluateAtDerivative function</param>
        /// <returns></returns>
        public double EvaluateAtDerivative(double value)
        {
            return value * (1 - value);
        }
    }
}
