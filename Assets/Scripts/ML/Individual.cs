using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ML
{
    public class Individual : IComparable<Individual>
    {
        public double[] chromosome; // represents a solution
        public double error; // smaller values are better for minimization

        private int numGenes; // problem dimension (numWeights)
        private double minGene; // smallest value for a chromosome cell
        private double maxGene;
        private double mutateRate; // used during reproduction by Mutate
        private double mutateChange; // used during reproduction

        static Random rnd = new Random(0); // used by ctor for random genes

        public Individual(int numGenes, double minGene, double maxGene,
          double mutateRate, double mutateChange)
        {
            this.numGenes = numGenes;
            this.minGene = minGene;
            this.maxGene = maxGene;
            this.mutateRate = mutateRate;
            this.mutateChange = mutateChange;
            this.chromosome = new double[numGenes];
            for (int i = 0; i < this.chromosome.Length; ++i)
                this.chromosome[i] = (maxGene - minGene) * rnd.NextDouble() + minGene;
            // this.error supplied after calling ctor!
        }

        public int CompareTo(Individual other) // smallest error to largest
        {
            if (this.error < other.error) return -1;
            else if (this.error > other.error) return 1;
            else return 0;
        }
    }
}
