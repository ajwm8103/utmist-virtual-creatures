using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace KSS
{
    [System.Serializable]
    public class KSSSave : TrainingSave
    {
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

    [System.Serializable]
    public class EnvTracker {
        public Environment env;
        public int? idx;

        public EnvTracker(Environment env, int? idx){
            this.env = env;
            this.idx = idx;
        }
    }

    public class KSSAlgorithm : TrainingAlgorithm
    {
        public KSSSave saveK;
        private KSSSettings optimizationSettings;

        private int currentGenotypeIndex = 0;
        private int currentGenerationIndex = 0;
        private Generation currentGeneration;
        private int currentGenerationSize;
        private List<EnvTracker> trackers;
        //private bool generationInProgress = false;

        public override void Setup(TrainingManager tm)
        {
            base.Setup(tm);
            saveK = (KSSSave)save;
            optimizationSettings = (KSSSettings)saveK.ts.optimizationSettings;

            trackers = new List<EnvTracker>();
            foreach (Environment env in tm.environments)
            {
                trackers.Add(new EnvTracker(env, null));
            }
        }

        // Update is called once per frame
        void Update()
        {
            //if (generationInProgress)
            //    return;

            if (currentGenotypeIndex < optimizationSettings.populationSize)
            {
                // Haven't spawned all, so loop through genotypes
                bool completed = true;
                for (int i = currentGenotypeIndex; i < optimizationSettings.populationSize; i++)
                {
                    CreatureGenotypeEval currentEval = currentGeneration.cgEvals[currentGenotypeIndex];
                    EnvTracker envTracker = FindAvailableEnvironment();
                    if (envTracker != null)
                    {
                        envTracker.idx = i; // Storing genotype index
                        envTracker.env.SpawnCreature(currentEval.cg);
                        envTracker.env.PingReset(this);
                    } else {
                        // All envs are full, set currentGenotypeIndex and wait until next Update() call
                        currentGenotypeIndex = i;
                        completed = false;
                        break;
                    }
                }

                if (completed){
                    currentGenotypeIndex = optimizationSettings.populationSize;
                }
            }
            else
            {
                CreateNextGeneration();
            }
        }

        public override void ResetPing(Environment env, float fitness){
            // Set fitness info
            EnvTracker result = trackers.First(t => t.env == env);
            if (result == null || result.idx == null){
                throw new System.Exception();
            }
            currentGeneration.cgEvals[(int)result.idx].fitness = fitness;
        }

        private EnvTracker FindAvailableEnvironment()
        {
            foreach (EnvTracker et in trackers)
            {
                if (!et.env.busy)
                {
                    return et;
                }
            }
            return null;
        }

        private void CreateNextGeneration()
        {
            currentGenotypeIndex = 0;
            currentGenerationIndex++;
            CreatureGenotype bestGenotype = SelectBestGenotype();
            MutateGenotype.MutationPreferenceSetting mp = new MutateGenotype.MutationPreferenceSetting(); // Set your mutation preferences
            Generation nextGeneration = new Generation(saveK.ts.generationSize, bestGenotype, mp);
            saveK.generations.Add(nextGeneration);
        }

        private CreatureGenotype SelectBestGenotype()
        {
            CreatureGenotypeEval bestEval = saveK.generations.Last().cgEvals.OrderByDescending(cgEval => cgEval.fitness).FirstOrDefault();
            return bestEval.cg;
        }
    }
}