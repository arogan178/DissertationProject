using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ML
{
    public class NeuralNetworkEO
    {
        private int numInput;
        private int numHidden;
        private int numOutput;
        private double[] inputs;
        private double[][] ihWeights;
        private double[] hBiases;
        private double[] hOutputs;
        private double[][] hoWeights;
        private double[] oBiases;
        private double[] outputs;

        private double[][] chWeights; // context-hidden
        private double[] cNodes; // context nodes

        private Random rnd; // 

        public double bestError = 0.0f;
        public NeuralNetworkEO(int numInput, int numHidden,
          int numOutput)
        {
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

            this.chWeights = MakeMatrix(numHidden, numHidden);
            this.cNodes = new double[numHidden];
            rnd = new Random(0);
        } // ctor

        private static double[][] MakeMatrix(int rows, int cols)
        {
            // helper for NN ctor
            double[][] result = new double[rows][];
            for (int r = 0; r < result.Length; ++r)
                result[r] = new double[cols];
            return result;
        }

        public void SetWeights(double[] weights)
        {
            // sets weights and biases from weights[]
            int numWeights = (numInput * numHidden) +
              (numHidden * numOutput) +
              (numHidden * numHidden) + numHidden + numOutput;
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
            for (int c = 0; c < numHidden; ++c) // ch
                for (int j = 0; j < numHidden; ++j)
                    chWeights[c][j] = weights[k++];
        }

        public double[] GetWeights()
        {
            // returns current weights and biases
            int numWeights = (numInput * numHidden) +
              (numHidden * numOutput) + numHidden + numOutput;
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

        public void SetContext(double[] values)
        {
            if (values.Length != this.numHidden)
                throw new Exception("Bad array in SetContext");
            for (int c = 0; c < numHidden; ++c)
                cNodes[c] = values[c];
        }

        public double[] ComputeOutputs(double[] xValues)
        {
            // feed-forward mechanism for NN classifier
            if (xValues.Length != numInput)
                throw new Exception("Bad xValues array length");

            double[] hSums = new double[numHidden];
            double[] oSums = new double[numOutput];

            for (int i = 0; i < xValues.Length; ++i)
                this.inputs[i] = xValues[i];

            for (int j = 0; j < numHidden; ++j)
                for (int i = 0; i < numInput; ++i)
                    hSums[j] += this.inputs[i] * this.ihWeights[i][j];

            for (int j = 0; j < numHidden; ++j)
                for (int c = 0; c < numHidden; ++c)
                    hSums[j] += this.cNodes[c] * chWeights[c][j];

            for (int i = 0; i < numHidden; ++i)
                hSums[i] += this.hBiases[i];

            for (int i = 0; i < numHidden; ++i)
                this.hOutputs[i] = HyperTanFunction(hSums[i]);

            for (int j = 0; j < numOutput; ++j)
                for (int i = 0; i < numHidden; ++i)
                    oSums[j] += hOutputs[i] * hoWeights[i][j];

            for (int i = 0; i < numOutput; ++i)
                oSums[i] += oBiases[i];

            double[] softOut = Softmax(oSums);
            Array.Copy(softOut, outputs, softOut.Length);

            for (int j = 0; j < numHidden; ++j)
                cNodes[j] = hOutputs[j];

            double[] retResult = new double[numOutput];
            Array.Copy(this.outputs, retResult, retResult.Length);
            return retResult;
        } // ComputeOutputs

        private static double HyperTanFunction(double x)
        {
            if (x < -20.0) return -1.0;
            else if (x > 20.0) return 1.0;
            else return Math.Tanh(x);
        }

        private static double[] Softmax(double[] oSums)
        {
            double max = oSums[0];
            for (int i = 0; i < oSums.Length; ++i)
                if (oSums[i] > max) max = oSums[i];

            // determine scaling factor
            double scale = 0.0;
            for (int i = 0; i < oSums.Length; ++i)
                scale += Math.Exp(oSums[i] - max);

            double[] result = new double[oSums.Length];
            for (int i = 0; i < oSums.Length; ++i)
                result[i] = Math.Exp(oSums[i] - max) / scale;

            return result; // scaled so xi sum to 1.0
        }

        public double[] Train(double[][] trainData,
          int popSize, int maxGeneration, double exitError,
          double mutateRate, double mutateChange, double tau)
        {
            // use Evolutionary Optimization to train NN

            int numWeights = (numInput * numHidden) +
              (numHidden * numOutput) +
              (numHidden * numHidden) + numHidden + numOutput; // = numGenes

            double minX = -1.0; // could be parameters. = minGene
            double maxX = 1.0;

            // initialize population
            Individual[] population = new Individual[popSize];
            double[] bestSolution = new double[numWeights]; // best solution any individual
            bestError = double.MaxValue; // smaller values better

            for (int i = 0; i < population.Length; ++i)
            {
                population[i] = new Individual(numWeights, minX, maxX, mutateRate,
                  mutateChange); // random values
                double error = MeanSquaredError(trainData, population[i].chromosome);
                population[i].error = error;
                if (population[i].error < bestError)
                {
                    bestError = population[i].error;
                    Array.Copy(population[i].chromosome, bestSolution, numWeights);
                }
            }

            // main EO processing loop
            int gen = 0; bool done = false;
            while (gen < maxGeneration && done == false)
            {
                Individual[] parents = Select(2, population, tau); // 2 good Individuals
                Individual[] children = Reproduce(parents[0], parents[1], minX, maxX,
                  mutateRate, mutateChange); // create 2 children
                children[0].error = MeanSquaredError(trainData, children[0].chromosome);
                children[1].error = MeanSquaredError(trainData, children[1].chromosome);

                Place(children[0], children[1], population); // sort pop, replace two worst 

                // immigration
                // kill off third-worst Individual and replace with new Individual
                // assumes population is sorted (via Place()
                Individual immigrant = new Individual(numWeights, minX, maxX, mutateRate, mutateChange);
                immigrant.error = MeanSquaredError(trainData, immigrant.chromosome);
                population[population.Length - 3] = immigrant; // replace third worst individual

                for (int i = popSize - 3; i < popSize; ++i) // check the 3 new Individuals
                {
                    if (population[i].error < bestError)
                    {
                        bestError = population[i].error;
                        population[i].chromosome.CopyTo(bestSolution, 0);
                        if (bestError < exitError)
                        {
                            done = true;
                            Console.WriteLine("\nEarly exit at generation " + gen);
                        }
                    }
                }
                ++gen;
            }
            return bestSolution;
        } // Train

        private Individual[] Select(int n, Individual[] population, double tau)
        {
            // tau is selection pressure = % of population to grab
            int popSize = population.Length;
            int[] indexes = new int[popSize];
            for (int i = 0; i < indexes.Length; ++i)
                indexes[i] = i;

            for (int i = 0; i < indexes.Length; ++i) // shuffle
            {
                int r = rnd.Next(i, indexes.Length);
                int tmp = indexes[r]; indexes[r] = indexes[i]; indexes[i] = tmp;
            }

            int tournSize = (int)(tau * popSize);
            if (tournSize < n) tournSize = n;
            Individual[] candidates = new Individual[tournSize];

            for (int i = 0; i < tournSize; ++i)
                candidates[i] = population[indexes[i]];
            Array.Sort(candidates);

            Individual[] results = new Individual[n];
            for (int i = 0; i < n; ++i)
                results[i] = candidates[i];

            return results;
        }

        private Individual[] Reproduce(Individual parent1, Individual parent2,
          double minGene, double maxGene, double mutateRate, double mutateChange)
        {
            int numGenes = parent1.chromosome.Length;

            int cross = rnd.Next(0, numGenes - 1); // crossover point. 0 means 'between 0 and 1'.

            Individual child1 = new Individual(numGenes, minGene, maxGene,
              mutateRate, mutateChange); // random chromosome
            Individual child2 = new Individual(numGenes, minGene, maxGene,
              mutateRate, mutateChange);

            for (int i = 0; i <= cross; ++i)
                child1.chromosome[i] = parent1.chromosome[i];
            for (int i = cross + 1; i < numGenes; ++i)
                child2.chromosome[i] = parent1.chromosome[i];
            for (int i = 0; i <= cross; ++i)
                child2.chromosome[i] = parent2.chromosome[i];
            for (int i = cross + 1; i < numGenes; ++i)
                child1.chromosome[i] = parent2.chromosome[i];

            Mutate(child1, maxGene, mutateRate, mutateChange);
            Mutate(child2, maxGene, mutateRate, mutateChange);

            Individual[] result = new Individual[2];
            result[0] = child1;
            result[1] = child2;

            return result;
        } // Reproduce

        private void Mutate(Individual child, double maxGene, double mutateRate,
          double mutateChange)
        {
            double hi = mutateChange * maxGene;
            double lo = -hi;
            for (int i = 0; i < child.chromosome.Length; ++i)
            {
                if (rnd.NextDouble() < mutateRate)
                {
                    double delta = (hi - lo) * rnd.NextDouble() + lo;
                    child.chromosome[i] += delta;
                }
            }
        }

        private static void Place(Individual child1, Individual child2,
          Individual[] population)
        {
            // place child1 and child2 replacing two worst individuals
            int popSize = population.Length;
            Array.Sort(population);
            population[popSize - 1] = child1;
            population[popSize - 2] = child2;
            return;
        }

        private double MeanSquaredError(double[][] trainData,
          double[] weights)
        {
            // how far off are computed values from desired values
            this.SetWeights(weights);

            double[] xValues = new double[numInput]; // inputs
            double[] tValues = new double[numOutput]; // targets
            double sumSquaredError = 0.0;
            for (int i = 0; i < trainData.Length; ++i)
            {
                // assumes data has x-values followed by y-values
                Array.Copy(trainData[i], xValues, numInput);
                Array.Copy(trainData[i], numInput, tValues, 0,
                  numOutput);
                double[] yValues = this.ComputeOutputs(xValues);
                for (int j = 0; j < yValues.Length; ++j)
                    sumSquaredError += ((yValues[j] - tValues[j]) *
                                        (yValues[j] - tValues[j]));
            }
            return sumSquaredError / trainData.Length;
        }

        public double Accuracy(double[][] testData)
        {
            // percentage correct using 'winner-takes all'
            int numCorrect = 0;
            int numWrong = 0;
            double[] xValues = new double[numInput]; // inputs
            double[] tValues = new double[numOutput]; // targets
            double[] yValues; // computed outputs

            for (int i = 0; i < testData.Length; ++i)
            {
                Array.Copy(testData[i], xValues, numInput);
                Array.Copy(testData[i], numInput, tValues, 0,
                  numOutput);
                yValues = this.ComputeOutputs(xValues);

                int maxIndex = MaxIndex(yValues);

                if (tValues[maxIndex] == 1.0) // not so nice
                    ++numCorrect;
                else
                    ++numWrong;
            }
            return (numCorrect * 1.0) / (numCorrect + numWrong);
        }

        private static int MaxIndex(double[] vector)
        {
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
}
