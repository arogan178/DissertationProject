using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ML
{
    public class NeuralNetworkBackProp
    {
        private int numInput; // number input nodes
        private int numHidden;
        private int numOutput;

        private double[] inputs;
        private double[][] ihWeights; // input-hidden
        private double[] hBiases;
        private double[] hOutputs;

        private double[][] hoWeights; // hidden-output
        private double[] oBiases;
        private double[] outputs;

        public double finalErr;

        private Random rnd;

        public NeuralNetworkBackProp(int numInput, int numHidden, int numOutput)
        {
            this.numInput = numInput;
            this.numHidden = numHidden;
            this.numOutput = numOutput;
            inputs = new double[numInput];
            ihWeights = MakeMatrix(numInput, numHidden, 0.0);
            hBiases = new double[numHidden];
            hOutputs = new double[numHidden];
            hoWeights = MakeMatrix(numHidden, numOutput, 0.0);
            oBiases = new double[numOutput];
            outputs = new double[numOutput];
            finalErr = 0.0;
            rnd = new Random(0);
            InitializeWeights(); // all weights and biases
        } // ctor

        private static double[][] MakeMatrix(int rows, int cols, double v)
        {
            double[][] result = new double[rows][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new double[cols];
            }
            for (int j = 0; j < rows; j++)
            {
                for (int k = 0; k < cols; k++)
                {
                    result[j][k] = v;
                }
            }
            return result;
        }

        private void InitializeWeights() // helper for ctor
        {
            // initialize weights and biases to small random values
            int numWeights =
                (numInput * numHidden) + (numHidden * numOutput) + numHidden + numOutput;
            double[] initialWeights = new double[numWeights];

            for (int i = 0; i < initialWeights.Length; i++)
                initialWeights[i] = 0.0009 * rnd.NextDouble() + 0.0001;

            SetWeights(initialWeights);
        }

        private static void ShowVector(double[] vector, int dec)
        {
            for (int i = 0; i < vector.Length; ++i)
            {
                UnityEngine.Debug.Log(vector[i].ToString("F" + dec) + " ");
            }
        }

        public double Error(double[][] data, bool verbose)
        {
            // mean squared error using current weights & biases
            double sumSquaredError = 0.0;
            double[] xValues = new double[numInput]; // first numInput values in trainData
            double[] tValues = new double[numOutput]; // last numOutput values

            // walk thru each training case. looks like (6.9 3.2 5.7 2.3) (0 0 1)
            for (int i = 0; i < data.Length; i++)
            {
                Array.Copy(data[i], xValues, numInput);
                Array.Copy(data[i], numInput, tValues, 0, numOutput); // get target values
                double[] yValues = ComputeOutputs(xValues); // outputs using current weights

                if (verbose == true)
                {
                    ShowVector(yValues, 4);
                    ShowVector(tValues, 4);
                    UnityEngine.Debug.Log("");
                }

                for (int j = 0; j < numOutput; j++)
                {
                    double err = tValues[j] - yValues[j];
                    sumSquaredError += err * err;
                }
            }
            return sumSquaredError / (double)(data.Length * numOutput); // average per item
        } // Error

        public double CrossEntropy(double[][] data, bool verbose)
        {
            double crossEntropyError = 0.0;
            double[] xValues = new double[numInput]; // first numInput values in trainData
            double[] tValues = new double[numOutput]; // last numOutput values

            // walk thru each training case. looks like (6.9 3.2 5.7 2.3) (0 0 1)
            for (int i = 0; i < data.Length; i++)
            {
                Array.Copy(data[i], xValues, numInput);
                Array.Copy(data[i], numInput, tValues, 0, numOutput); // get target values
                double[] yValues = ComputeOutputs(xValues); // outputs using current weights

                if (verbose == true)
                {
                    ShowVector(yValues, 4);
                    ShowVector(tValues, 4);
                    UnityEngine.Debug.Log("");
                }

                for (int j = 0; j < numOutput; j++)
                {
                    double t = tValues[j];
                    double y = yValues[j];

                    if (y > 0)
                        crossEntropyError -= t * Math.Log(y) + (1 - t) * Math.Log(1 - y);
                }
            }

            return crossEntropyError / data.Length;
        }

        public void SetWeights(double[] weights)
        {
            // copy serialized weights and biases in weights[] array
            // to i-h weights, i-h biases, h-o weights, h-o biases
            int numWeights = numInput * numHidden + numHidden * numOutput + numHidden + numOutput;
            if (weights.Length != numWeights)
            {
                throw new Exception("Bad weights array in SetWeights");
            }
            int k = 0; // points into weights param

            for (int i = 0; i < numInput; ++i)
            {
                for (int j = 0; j < numHidden; j++)
                {
                    ihWeights[i][j] = weights[k++];
                }
            }
            for (int i = 0; i < numHidden; i++)
            {
                hBiases[i] = weights[k++];
            }
            for (int i = 0; i < numHidden; i++)
            {
                for (int j = 0; j < numOutput; j++)
                {
                    hoWeights[i][j] = weights[k++];
                }
            }
            for (int i = 0; i < numOutput; i++)
            {
                oBiases[i] = weights[k++];
            }
        }

        public double[] GetWeights()
        {
            double[] array = new double[
                numInput * numHidden + numHidden * numOutput + numHidden + numOutput
            ];
            int num = 0;

            for (int i = 0; i < ihWeights.Length; i++)
            {
                for (int j = 0; j < ihWeights[0].Length; j++)
                {
                    array[num++] = ihWeights[i][j];
                }
            }
            for (int k = 0; k < hBiases.Length; k++)
            {
                array[num++] = hBiases[k];
            }
            for (int l = 0; l < hoWeights.Length; l++)
            {
                for (int m = 0; m < hoWeights[0].Length; m++)
                {
                    array[num++] = hoWeights[l][m];
                }
            }
            for (int n = 0; n < oBiases.Length; n++)
            {
                array[num++] = oBiases[n];
            }
            return array;
        }

        public double[] ComputeOutputs(double[] xValues)
        {
            double[] hSums = new double[numHidden]; // hidden nodes sums scratch array
            double[] oSums = new double[numOutput]; // output nodes sums

            for (int i = 0; i < xValues.Length; ++i)
            { // copy x-values to inputs
                inputs[i] = xValues[i];
            }
            for (int j = 0; j < numHidden; j++)
            {
                for (int k = 0; k < numInput; k++)
                {
                    hSums[j] += inputs[k] * ihWeights[k][j];
                }
            }
            for (int l = 0; l < numHidden; l++)
            {
                hSums[l] += hBiases[l];
            }
            for (int m = 0; m < numHidden; m++)
            {
                hOutputs[m] = HyperTan(hSums[m]);
            }
            for (int n = 0; n < numOutput; n++)
            {
                for (int num = 0; num < numHidden; num++)
                {
                    oSums[n] += hOutputs[num] * hoWeights[num][n];
                }
            }
            for (int num2 = 0; num2 < numOutput; num2++)
            {
                oSums[num2] += oBiases[num2];
            }
            double[] array3 = Softmax(oSums);
            Array.Copy(array3, outputs, array3.Length);
            double[] array4 = new double[numOutput];
            Array.Copy(outputs, array4, array4.Length);
            return array4;
        }

        private static double HyperTan(double x)
        {
            if (x < -20.0)
            {
                return -1.0;
            }
            if (x > 20.0)
            {
                return 1.0;
            }
            return Math.Tanh(x);
        }

        private static double[] Softmax(double[] oSums)
        {
            // does all output nodes at once so scale
            // doesn't have to be re-computed each time
            double sum = 0.0;
            for (int i = 0; i < oSums.Length; i++)
            {
                sum += Math.Exp(oSums[i]);
            }
            double[] result = new double[oSums.Length];
            for (int i = 0; i < oSums.Length; ++i)
            {
                result[i] = Math.Exp(oSums[i]) / sum;
            }
            return result;
        }

        public double[] Train(
            double[][] trainData,
            int maxEpochs,
            double learnRate,
            double momentum
        )
        {
            double[][] hoGrads = MakeMatrix(numHidden, numOutput, 0.0); // hidden-to-output weight gradients
            double[] obGrads = new double[numOutput]; // output bias gradients
            double[][] ihGrads = MakeMatrix(numInput, numHidden, 0.0); // input-to-hidden weight gradients
            double[] hbGrads = new double[numHidden]; // hidden bias gradients
            double[] oSignals = new double[numOutput]; // local gradient output signals - gradients w/o associated input terms
            double[] hSignals = new double[numHidden]; // local gradient hidden node signals
            // back-prop momentum specific arrays
            double[][] ihPrevWeightsDelta = MakeMatrix(numInput, numHidden, 0.0);
            double[] hPrevBiasesDelta = new double[numHidden];
            double[][] hoPrevWeightsDelta = MakeMatrix(numHidden, numOutput, 0.0);
            double[] oPrevBiasesDelta = new double[numOutput];
            int epoch = 0;
            double[] xValues = new double[numInput]; // inputs
            double[] tValues = new double[numOutput]; // target values
            double derivative = 0.0;
            double errorSignal = 0.0;
            int[] sequence = new int[trainData.Length];

            for (int i = 0; i < sequence.Length; i++)
            {
                sequence[i] = i;
            }
            int errInterval = maxEpochs / 10; // interval to check error
            while (epoch < maxEpochs)
            {
                epoch++;
                if (epoch % errInterval == 0 && epoch < maxEpochs)
                {
                    double trainErr = finalErr = Error(trainData);
                    UnityEngine.Debug.Log("epoch = " + epoch + "  error = " + trainErr.ToString("F4"));
                }
                Shuffle(sequence); // visit each training data in random order
                for (int ii = 0; ii < trainData.Length; ii++)
                {
                    int idx = sequence[ii];
                    Array.Copy(trainData[idx], xValues, numInput);
                    Array.Copy(trainData[idx], numInput, tValues, 0, numOutput);
                    ComputeOutputs(xValues); // copy xValues in, compute outputs

                    // indices: i = inputs, j = hiddens, k = outputs

                    // 1. compute output node signals (assumes softmax)
                    for (int k = 0; k < numOutput; k++)
                    {
                        errorSignal = tValues[k] - outputs[k]; // Wikipedia uses (o-t)
                        derivative = (1.0 - outputs[k]) * outputs[k]; // for softmax
                        oSignals[k] = errorSignal * derivative;
                    }

                    // 2. compute hidden-to-output weight gradients using output signals
                    for (int j = 0; j < numHidden; j++)
                        for (int k = 0; k < numOutput; k++)
                            hoGrads[j][k] = oSignals[k] * hOutputs[j];

                    // 2b. compute output bias gradients using output signals
                    for (int k = 0; k < numOutput; k++)
                        obGrads[k] = oSignals[k] * 1.0; // dummy assoc. input value

                    // 3. compute hidden node signals
                    for (int j = 0; j < numHidden; j++)
                    {
                        derivative = (1 + hOutputs[j]) * (1 - hOutputs[j]); // for tanh
                        double sum = 0.0; // need sums of output signals times hidden-to-output weights
                        for (int k = 0; k < numOutput; k++)
                        {
                            sum += oSignals[k] * hoWeights[j][k]; // represents error signal
                        }
                        hSignals[j] = derivative * sum;
                    }

                    // 4. compute input-hidden weight gradients
                    for (int i = 0; i < numInput; i++)
                        for (int j = 0; j < numHidden; j++)
                            ihGrads[i][j] = hSignals[j] * inputs[i];

                    // 4b. compute hidden node bias gradients
                    for (int j = 0; j < numHidden; j++)
                        hbGrads[j] = hSignals[j] * 1.0; // dummy 1.0 input

                    // == update weights and biases

                    // update input-to-hidden weights
                    for (int i = 0; i < numInput; i++)
                    {
                        for (int j = 0; j < numHidden; j++)
                        {
                            double delta = ihGrads[i][j] * learnRate;
                            ihWeights[i][j] += delta; // would be -= if (o-t)
                            ihWeights[i][j] += ihPrevWeightsDelta[i][j] * momentum;
                            ihPrevWeightsDelta[i][j] = delta; // save for next time
                        }
                    }

                    // update hidden biases
                    for (int j = 0; j < numHidden; j++)
                    {
                        double delta = hbGrads[j] * learnRate;
                        hBiases[j] += delta;
                        hBiases[j] += hPrevBiasesDelta[j] * momentum;
                        hPrevBiasesDelta[j] = delta;
                    }

                    // update hidden-to-output weights
                    for (int j = 0; j < numHidden; j++)
                    {
                        for (int k = 0; k < numOutput; k++)
                        {
                            double delta = hoGrads[j][k] * learnRate;
                            hoWeights[j][k] += delta;
                            hoWeights[j][k] += hoPrevWeightsDelta[j][k] * momentum;
                            hoPrevWeightsDelta[j][k] = delta;
                        }
                    }

                    // update output node biases
                    for (int k = 0; k < numOutput; k++)
                    {
                        double delta = obGrads[k] * learnRate;
                        oBiases[k] += delta;
                        oBiases[k] += oPrevBiasesDelta[k] * momentum;
                        oPrevBiasesDelta[k] = delta;
                    }
                } // each training item
            } // while
            return GetWeights();
        } // Train

        private void Shuffle(int[] sequence) // instance method
        {
            for (int i = 0; i < sequence.Length; i++)
            {
                int r = rnd.Next(i, sequence.Length);
                int tmp = sequence[r];
                sequence[r] = sequence[i];
                sequence[i] = tmp;
            }
        } // Shuffle

        public double Error(double[][] trainData)
        {
            // average squared error per training item
            double sumSquaredError = 0.0;
            double[] xValues = new double[numInput]; // first numInput values in trainData
            double[] tValues = new double[numOutput]; // last numOutput values

            // walk thru each training case. looks like (6.9 3.2 5.7 2.3) (0 0 1)
            for (int i = 0; i < trainData.Length; ++i)
            {
                Array.Copy(trainData[i], xValues, numInput);
                Array.Copy(trainData[i], numInput, tValues, 0, numOutput); // get target values
                double[] yValues = this.ComputeOutputs(xValues); // outputs using current weights
                for (int j = 0; j < numOutput; ++j)
                {
                    double err = tValues[j] - yValues[j];
                    sumSquaredError += err * err;
                }
            }
            return sumSquaredError / (double)trainData.Length;
        } // MeanSquaredError

        public double Error(double[][] trainData, double[] weights)
        {
            this.SetWeights(weights);
            // average squared error per training item
            double sumSquaredError = 0.0;
            double[] xValues = new double[numInput]; // first numInput values in trainData
            double[] tValues = new double[numOutput]; // last numOutput values

            // walk thru each training case. looks like (6.9 3.2 5.7 2.3) (0 0 1)
            for (int i = 0; i < trainData.Length; ++i)
            {
                Array.Copy(trainData[i], xValues, numInput);
                Array.Copy(trainData[i], numInput, tValues, 0, numOutput); // get target values
                double[] yValues = this.ComputeOutputs(xValues); // outputs using current weights
                for (int j = 0; j < numOutput; ++j)
                {
                    double err = tValues[j] - yValues[j];
                    sumSquaredError += err * err;
                }
            }
            return sumSquaredError / trainData.Length;
        } // MeanSquaredError

        public double Accuracy(double[][] testData)
        {
            // percentage correct using winner-takes all
            int numCorrect = 0;
            int numWrong = 0;
            double[] xValues = new double[numInput]; // inputs
            double[] tValues = new double[numOutput]; // targets
            double[] yValues; // computed Y

            for (int i = 0; i < testData.Length; ++i)
            {
                Array.Copy(testData[i], xValues, numInput); // get x-values
                Array.Copy(testData[i], numInput, tValues, 0, numOutput); // get t-values
                yValues = this.ComputeOutputs(xValues);
                int maxIndex = MaxIndex(yValues); // which cell in yValues has largest value?
                int tMaxIndex = MaxIndex(tValues);

                if (maxIndex == tMaxIndex)
                    ++numCorrect;
                else
                    ++numWrong;
            }
            return (numCorrect * 1.0) / (numCorrect + numWrong);
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
                    biggestVal = vector[i];
                    bigIndex = i;
                }
            }
            return bigIndex;
        }
    } // NeuralNetwork
}
