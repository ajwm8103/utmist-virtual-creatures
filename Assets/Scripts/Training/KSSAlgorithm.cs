using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KSS
{
    [System.Serializable]
    public class KSSSave
    {
        public string saveName = "Unnamed KSS Save";
        public TrainingSettings ts;

        public List<Generation> generations;

        // Data on creatures goes here TODO
    }

    [System.Serializable]
    public class Generation {
        public List<CreatureGenotypeEval> cgEvals;

        /// <summary>
        /// Creates a new generation w/ size and mutation
        /// </summary>
        /// <param name="size"></param>
        /// <param name="initialGenotype"></param>
        /// <param name="mps"></param>
        public Generation(int size, CreatureGenotype initialGenotype, MutateGenotype.MutationPreferenceSetting mp){
            cgEvals = new List<CreatureGenotypeEval>();
            if (initialGenotype == null){
                // Random generation
                for (int i = 0; i < size; i++)
                {
                    CreatureGenotypeEval cgEval = new CreatureGenotypeEval(MutateGenotype.GenerateRandomCreatureGenotype());
                    cgEvals.Add(cgEval);
                }
            } else {
                // Mutated generation
                for (int i = 0; i < size; i++)
                {
                    CreatureGenotypeEval cgEval = new CreatureGenotypeEval(MutateGenotype.MutateCreatureGenotype(initialGenotype, mp));
                    cgEvals.Add(cgEval);
                }
            }
        }
    }

    [System.Serializable]
    public class CreatureGenotypeEval {
        public CreatureGenotype cg;
        public float fitness; // total reward
        public bool evaluated;

        public CreatureGenotypeEval(CreatureGenotype cg){
            this.cg = cg;
            fitness = 0;
            evaluated = false;
        }
    }

    public class KSSAlgorithm : TrainingAlgorithm
    {

        public KSSSave save;
                                                                      
        public override void Setup(TrainingManager tm)
        {
            base.Setup(tm);

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}