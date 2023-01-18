using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.ML
{
    public class NeuralNetworkPSO
    {
        //private static Random rnd; // for BP to initialize wts, in PSO 
        private int numInput;
        private int numHidden;
        private int numOutput;
        private double[] inputs;
        private double[][] ihWeights; // input-hidden
        private double[] hBiases;
        private double[] hOutputs;
        private double[][] hoWeights; // hidden-output
        private double[] oBiases;
        private double[] outputs;

        double bestGlobalError;
        double[] bestGlobalPosition;

        public NeuralNetworkPSO(int numInput, int numHidden, int numOutput)
        {
            //rnd = new Random(16); // for particle initialization. 16 just gives nice demo
            this.numInput = numInput;
            this.numHidden = numHidden;
            this.numOutput = numOutput;
            this.inputs = new double[numInput];
            this.ihWeights = MakeMatrix(numInput, numHidden);
            this.hBiases = new double[numHidden];
            this.hOutputs = new double[numHidden];
            this.hoWeights = MakeMatrix(numHidden, numOutput);
            this.oBiases = new double[numOutput];
            this.outputs = new double[numOutput];
        } // ctor

        private static double[][] MakeMatrix(int rows, int cols) // helper for ctor
        {
            double[][] result = new double[rows][];
            for (int r = 0; r < result.Length; ++r)
                result[r] = new double[cols];
            return result;
        }

        //public override string ToString() // yikes
        //{
        //  string s = "";
        //  s += "===============================\n";
        //  s += "numInput = " + numInput + " numHidden = " + numHidden + " numOutput = " + numOutput + "\n\n";

        //  s += "inputs: \n";
        //  for (int i = 0; i < inputs.Length; ++i)
        //    s += inputs[i].ToString("F2") + " ";
        //  s += "\n\n";

        //  s += "ihWeights: \n";
        //  for (int i = 0; i < ihWeights.Length; ++i)
        //  {
        //    for (int j = 0; j < ihWeights[i].Length; ++j)
        //    {
        //      s += ihWeights[i][j].ToString("F4") + " ";
        //    }
        //    s += "\n";
        //  }
        //  s += "\n";

        //  s += "hBiases: \n";
        //  for (int i = 0; i < hBiases.Length; ++i)
        //    s += hBiases[i].ToString("F4") + " ";
        //  s += "\n\n";

        //  s += "hOutputs: \n";
        //  for (int i = 0; i < hOutputs.Length; ++i)
        //    s += hOutputs[i].ToString("F4") + " ";
        //  s += "\n\n";

        //  s += "hoWeights: \n";
        //  for (int i = 0; i < hoWeights.Length; ++i)
        //  {
        //    for (int j = 0; j < hoWeights[i].Length; ++j)
        //    {
        //      s += hoWeights[i][j].ToString("F4") + " ";
        //    }
        //    s += "\n";
        //  }
        //  s += "\n";

        //  s += "oBiases: \n";
        //  for (int i = 0; i < oBiases.Length; ++i)
        //    s += oBiases[i].ToString("F4") + " ";
        //  s += "\n\n";

        //  s += "outputs: \n";
        //  for (int i = 0; i < outputs.Length; ++i)
        //    s += outputs[i].ToString("F2") + " ";
        //  s += "\n\n";

        //  s += "===============================\n";
        //  return s;
        //}

        // ----------------------------------------------------------------------------------------

        public double Error(double[][] data, bool verbose)
        {
            // mean squared error using current weights & biases
            double sumSquaredError = 0.0;
            double[] xValues = new double[numInput]; // first numInput values in trainData
            double[] tValues = new double[numOutput]; // last numOutput values

            // walk thru each training case. looks like (6.9 3.2 5.7 2.3) (0 0 1)
            for (int i = 0; i < data.Length; ++i)
            {
                Array.Copy(data[i], xValues, numInput);
                Array.Copy(data[i], numInput, tValues, 0, numOutput); // get target values
                double[] yValues = this.ComputeOutputs(xValues); // outputs using current weights

                if (verbose == true)
                {
                    Console.WriteLine("");
                    Console.ReadLine();
                }


                for (int j = 0; j < numOutput; ++j)
                {
                    double err = tValues[j] - yValues[j];
                    sumSquaredError += err * err;
                }
            }
            return sumSquaredError / (data.Length * numOutput);  // average per item
        } // Error

        public void SetWeights(double[] weights)
        {
            // copy weights and biases in weights[] array to i-h weights, i-h biases, h-o weights, h-o biases
            int numWeights = (numInput * numHidden) + (numHidden * numOutput) + numHidden + numOutput;
            if (weights.Length != numWeights)
                throw new Exception("Bad weights array length: ");

            int k = 0; // points into weights param

            for (int i = 0; i < numInput; ++i)
                for (int j = 0; j < numHidden; ++j)
                    ihWeights[i][j] = weights[k++];
            for (int i = 0; i < numHidden; ++i)
                hBiases[i] = weights[k++];
            for (int i = 0; i < numHidden; ++i)
                for (int j = 0; j < numOutput; ++j)
                    hoWeights[i][j] = weights[k++];
            for (int i = 0; i < numOutput; ++i)
                oBiases[i] = weights[k++];
        }

        public double[] GetWeights()
        {
            // returns the current set of wweights, presumably after training
            int numWeights = (numInput * numHidden) + (numHidden * numOutput) + numHidden + numOutput;
            double[] result = new double[numWeights];
            int k = 0;
            for (int i = 0; i < ihWeights.Length; ++i)
                for (int j = 0; j < ihWeights[0].Length; ++j)
                    result[k++] = ihWeights[i][j];
            for (int i = 0; i < hBiases.Length; ++i)
                result[k++] = hBiases[i];
            for (int i = 0; i < hoWeights.Length; ++i)
                for (int j = 0; j < hoWeights[0].Length; ++j)
                    result[k++] = hoWeights[i][j];
            for (int i = 0; i < oBiases.Length; ++i)
                result[k++] = oBiases[i];
            return result;
        }

        // ----------------------------------------------------------------------------------------

        public double[] ComputeOutputs(double[] xValues)
        {
            if (xValues.Length != numInput)
                throw new Exception("Bad xValues array length");

            double[] hSums = new double[numHidden]; // hidden nodes sums scratch array
            double[] oSums = new double[numOutput]; // output nodes sums

            for (int i = 0; i < xValues.Length; ++i) // copy x-values to inputs
                this.inputs[i] = xValues[i];

            for (int j = 0; j < numHidden; ++j)  // compute i-h sum of weights * inputs
                for (int i = 0; i < numInput; ++i)
                    hSums[j] += this.inputs[i] * this.ihWeights[i][j]; // note +=

            for (int i = 0; i < numHidden; ++i)  // add biases to input-to-hidden sums
                hSums[i] += this.hBiases[i];

            for (int i = 0; i < numHidden; ++i)   // apply activation
                this.hOutputs[i] = HyperTanFunction(hSums[i]); // hard-coded

            for (int j = 0; j < numOutput; ++j)   // compute h-o sum of weights * hOutputs
                for (int i = 0; i < numHidden; ++i)
                    oSums[j] += hOutputs[i] * hoWeights[i][j];

            for (int i = 0; i < numOutput; ++i)  // add biases to input-to-hidden sums
                oSums[i] += oBiases[i];

            double[] softOut = Softmax(oSums); // softmax activation does all outputs at once for efficiency
            Array.Copy(softOut, outputs, softOut.Length);

            double[] retResult = new double[numOutput]; // could define a GetOutputs method instead
            Array.Copy(this.outputs, retResult, retResult.Length);
            return retResult;
        } // ComputeOutputs

        private static double HyperTanFunction(double x)
        {
            if (x < -20.0) return -1.0; // approximation is correct to 30 decimals
            else if (x > 20.0) return 1.0;
            else return Math.Tanh(x);
        }

        private static double[] Softmax(double[] oSums)
        {
            // does all output nodes at once so scale doesn't have to be re-computed each time
            // determine max output sum
            double max = oSums[0];
            for (int i = 0; i < oSums.Length; ++i)
                if (oSums[i] > max) max = oSums[i];

            // determine scaling factor -- sum of exp(each val - max)
            double scale = 0.0;
            for (int i = 0; i < oSums.Length; ++i)
                scale += Math.Exp(oSums[i] - max);

            double[] result = new double[oSums.Length];
            for (int i = 0; i < oSums.Length; ++i)
                result[i] = Math.Exp(oSums[i] - max) / scale;

            return result; // now scaled so that xi sum to 1.0
        }

        // ----------------------------------------------------------------------------------------

        public Particle[] InitializePopulation(Particle[] swarm,double[][]trainData, int numWeights,double minX, double maxX, Random rnd)
        {
            for (int i = 0; i < swarm.Length; ++i)
            {
                double[] randomPosition = new double[numWeights];
                for (int j = 0; j < randomPosition.Length; ++j)
                {
                    //double lo = minX;
                    //double hi = maxX;
                    //randomPosition[j] = (hi - lo) * rnd.NextDouble() + lo;
                    randomPosition[j] = (maxX - minX) * rnd.NextDouble() + minX;
                }

                // randomPosition is a set of weights; sent to NN
                double error = MeanCrossEntropy(trainData, randomPosition);
                //double error = MeanSquaredError(trainData, randomPosition);
                double[] randomVelocity = new double[numWeights];

                for (int j = 0; j < randomVelocity.Length; ++j)
                {
                    //double lo = -1.0 * Math.Abs(maxX - minX);
                    //double hi = Math.Abs(maxX - minX);
                    //randomVelocity[j] = (hi - lo) * rnd.NextDouble() + lo;
                    double lo = 0.1 * minX;
                    double hi = 0.1 * maxX;
                    randomVelocity[j] = (hi - lo) * rnd.NextDouble() + lo;

                }
                swarm[i] = new Particle(randomPosition, error, randomVelocity, randomPosition, error); // last two are best-position and best-error

                // does current Particle have global best position/solution?
                if (swarm[i].error < bestGlobalError)
                {
                    bestGlobalError = swarm[i].error;
                    swarm[i].position.CopyTo(bestGlobalPosition, 0);
                }
            }
            return swarm;
        }

        public double[] Train(double[][] trainData, int numParticles, int maxEpochs, double exitError, double probDeath)
        {
            // PSO version training. best weights stored into NN and returned
            // particle position == NN weights

            Random rnd = new Random(16); // 16 just gives nice demo

            int numWeights = (this.numInput * this.numHidden) + (this.numHidden * this.numOutput) +
              this.numHidden + this.numOutput;

            // use PSO to seek best weights
            int epoch = 0;
            double minX = -10.0; // for each weight. assumes data has been normalized about 0
            double maxX = 10.0;
            double w = 0.729; // inertia weight
            double c1 = 1.49445; // cognitive/local weight
            double c2 = 1.49445; // social/global weight
            double r1, r2; // cognitive and social randomizations

            Particle[] swarm = new Particle[numParticles];
            // best solution found by any particle in the swarm. implicit initialization to all 0.0
            bestGlobalPosition = new double[numWeights];
            bestGlobalError = double.MaxValue; // smaller values better

            //double minV = -0.01 * maxX;  // velocities
            //double maxV = 0.01 * maxX;

            // swarm initialization
            // initialize each Particle in the swarm with random positions and velocities
            swarm = InitializePopulation(swarm,trainData, numWeights, minX, maxX, rnd);
            // initialization



            //Console.WriteLine("Entering main PSO weight estimation processing loop");

            // main PSO algorithm

            int[] sequence = new int[numParticles]; // process particles in random order
            for (int i = 0; i < sequence.Length; ++i)
                sequence[i] = i;

            while (epoch < maxEpochs)
            {
                if (bestGlobalError < exitError) break; // early exit (MSE error)

                double[] newVelocity = new double[numWeights]; // step 1
                double[] newPosition = new double[numWeights]; // step 2
                double newError; // step 3

                Shuffle(sequence, rnd); // move particles in random sequence

                for (int pi = 0; pi < swarm.Length; ++pi) // each Particle (index)
                {
                    int i = sequence[pi];
                    Particle currP = swarm[i]; // for coding convenience

                    // 1. compute new velocity
                    for (int j = 0; j < currP.velocity.Length; ++j) // each x value of the velocity
                    {
                        r1 = rnd.NextDouble();
                        r2 = rnd.NextDouble();

                        // velocity depends on old velocity, best position of parrticle, and 
                        // best position of any particle
                        newVelocity[j] = (w * currP.velocity[j]) +
                          (c1 * r1 * (currP.bestPosition[j] - currP.position[j])) +
                          (c2 * r2 * (bestGlobalPosition[j] - currP.position[j]));
                    }

                    newVelocity.CopyTo(currP.velocity, 0);

                    // 2. use new velocity to compute new position
                    for (int j = 0; j < currP.position.Length; ++j)
                    {
                        newPosition[j] = currP.position[j] + newVelocity[j];  // compute new position
                        if (newPosition[j] < minX) // keep in range
                            newPosition[j] = minX;
                        else if (newPosition[j] > maxX)
                            newPosition[j] = maxX;
                    }

                    newPosition.CopyTo(currP.position, 0);

                    // 2b. optional: apply weight decay (large weights tend to overfit) 

                    // 3. use new position to compute new error
                    newError = MeanCrossEntropy(trainData, newPosition); // makes next check a bit cleaner
                    //newError = MeanSquaredError(trainData, newPosition);
                    currP.error = newError;

                    if (newError < currP.bestError) // new particle best?
                    {
                        newPosition.CopyTo(currP.bestPosition, 0);
                        currP.bestError = newError;
                    }

                    if (newError < bestGlobalError) // new global best?
                    {
                        newPosition.CopyTo(bestGlobalPosition, 0);
                        bestGlobalError = newError;
                    }

                    // 4. optional: does curr particle die?
                    double die = rnd.NextDouble();
                    if (die < probDeath)
                    {
                        // new position, leave velocity, update error
                        for (int j = 0; j < currP.position.Length; ++j)
                            currP.position[j] = (maxX - minX) * rnd.NextDouble() + minX;
                        currP.error = MeanCrossEntropy(trainData, currP.position);
                        //currP.error = MeanSquaredError(trainData, currP.position);
                        currP.position.CopyTo(currP.bestPosition, 0);
                        currP.bestError = currP.error;

                        if (currP.error < bestGlobalError) // global best by chance?
                        {
                            bestGlobalError = currP.error;
                            currP.position.CopyTo(bestGlobalPosition, 0);
                        }
                    }

                } // each Particle

                ++epoch;

            } // while

            this.SetWeights(bestGlobalPosition);  // best position is a set of weights
            double[] retResult = new double[numWeights];
            Array.Copy(bestGlobalPosition, retResult, retResult.Length);
            return retResult;

        } // Train

        private static void Shuffle(int[] sequence, Random rnd)
        {
            for (int i = 0; i < sequence.Length; ++i)
            {
                int r = rnd.Next(i, sequence.Length);
                int tmp = sequence[r];
                sequence[r] = sequence[i];
                sequence[i] = tmp;
            }
        }

        //private double MeanSquaredError(double[][] trainData, double[] weights)
        //{
        //    // assumes that centroids and widths have been set!
        //    this.SetWeights(weights); // copy the weights to evaluate in

        //    double[] xValues = new double[numInput]; // inputs
        //    double[] tValues = new double[numOutput]; // targets
        //    double sumSquaredError = 0.0;
        //    for (int i = 0; i < trainData.Length; ++i) // walk through each training data item
        //    {
        //        // following assumes data has all x-values first, followed by y-values!
        //        Array.Copy(trainData[i], xValues, numInput); // extract inputs
        //        Array.Copy(trainData[i], numInput, tValues, 0, numOutput); // extract targets
        //        double[] yValues = this.ComputeOutputs(xValues); // compute the outputs using centroids, widths, weights, bias values
        //        for (int j = 0; j < yValues.Length; ++j)
        //            sumSquaredError += ((yValues[j] - tValues[j]) * (yValues[j] - tValues[j]));
        //    }
        //    return sumSquaredError / trainData.Length;
        //}

        public double MeanCrossEntropy(double[][] trainData, double[] weights)
        {
            // (average) Cross Entropy for a given particle's position/weights
            // how good (cross entropy) are weights? CrossEntropy is error so smaller values are better
            this.SetWeights(weights); // load the weights and biases to examine into the NN

            double sce = 0.0; // sum of cross entropies of all data items
            double[] xValues = new double[numInput]; // inputs
            double[] tValues = new double[numOutput]; // targets

            // walk thru each training case. looks like (6.9 3.2 5.7 2.3) (0 0 1)
            for (int i = 0; i < trainData.Length; ++i)
            {
                Array.Copy(trainData[i], xValues, numInput); // extract inputs
                Array.Copy(trainData[i], numInput, tValues, 0, numOutput); // extract targets

                double[] yValues = this.ComputeOutputs(xValues); // run the inputs through the neural network
                                                                 // assumes outputs are 'softmaxed' -- all between 0 and 1, and sum to 1

                // CE = -Sum( t * log(y) )
                // see http://dame.dsf.unina.it/documents/softmax_entropy_VONEURAL-SPE-NA-0004-Rel1.0.pdf 
                // for an explanation of why cross tropy sometimes given as CE = -Sum( t * log(t/y) ), as in 
                // "On the Pairing of the Softmax Activation and Cross-Entropy Penalty Functions and
                // the Derivation of the Softmax Activation Function", Dunne & Campbell.
                double currSum = 0.0;
                for (int j = 0; j < yValues.Length; ++j)
                {
                    currSum += tValues[j] * Math.Log(yValues[j]); // diff between targets and y
                }
                sce += currSum; // accumulate
            }

            return -sce / trainData.Length;
        } // MeanCrossEntropy



        // ----------------------------------------------------------------------------------------

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
                Array.Copy(testData[i], xValues, numInput); // parse test data into x-values and t-values
                Array.Copy(testData[i], numInput, tValues, 0, numOutput);
                yValues = this.ComputeOutputs(xValues);
                int maxIndex = MaxIndex(yValues); // which cell in yValues has largest value?

                if (tValues[maxIndex] == 1.0) // ugly. consider AreEqual(double x, double y)
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

    } // NeuralNetwork

    // ==============================================================================================

    public class Particle
    {
        public double[] position; // equivalent to NN weights
        public double error; // measure of fitness
        public double[] velocity;

        public double[] bestPosition; // best position found so far by this Particle
        public double bestError;

        //public double age; // optional used to determine death-birth

        public Particle(double[] position, double error, double[] velocity,
          double[] bestPosition, double bestError)
        {
            this.position = new double[position.Length];
            position.CopyTo(this.position, 0);
            this.error = error;
            this.velocity = new double[velocity.Length];
            velocity.CopyTo(this.velocity, 0);
            this.bestPosition = new double[bestPosition.Length];
            bestPosition.CopyTo(this.bestPosition, 0);
            this.bestError = bestError;

            //this.age = 0;
        }

        //public override string ToString()
        //{
        //  string s = "";
        //  s += "==========================\n";
        //  s += "Position: ";
        //  for (int i = 0; i < this.position.Length; ++i)
        //    s += this.position[i].ToString("F2") + " ";
        //  s += "\n";
        //  s += "Error = " + this.error.ToString("F4") + "\n";
        //  s += "Velocity: ";
        //  for (int i = 0; i < this.velocity.Length; ++i)
        //    s += this.velocity[i].ToString("F2") + " ";
        //  s += "\n";
        //  s += "Best Position: ";
        //  for (int i = 0; i < this.bestPosition.Length; ++i)
        //    s += this.bestPosition[i].ToString("F2") + " ";
        //  s += "\n";
        //  s += "Best Error = " + this.bestError.ToString("F4") + "\n";
        //  s += "==========================\n";
        //  return s;
        //}

    } // class Particle
}
