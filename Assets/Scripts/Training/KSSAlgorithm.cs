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
                    CreatureGenotypeEval cgEval = new CreatureGenotypeEval(initialGenotype);
                    // CreatureGenotypeEval cgEval = new CreatureGenotypeEval(MutateGenotype.MutateCreatureGenotype(initialGenotype, mp));
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
        [SerializeField]
        private int untestedRemaining;
        private Generation currentGeneration;
        private List<EnvTracker> trackers;
        private bool isSetup = false;
        //private bool generationInProgress = false;

        public override void Setup(TrainingManager tm)
        {
            base.Setup(tm);
            saveK = (KSSSave)save;
            optimizationSettings = (KSSSettings)saveK.ts.optimizationSettings;


            if (saveK.isNew){
                Debug.Log("New save, first generation...");
                CreateNextGeneration(true);
            } else {
                // setup indexes, TODO
                // CreateNextGeneration(false); probably not this actually
            }

            trackers = new List<EnvTracker>();
            foreach (Environment env in tm.environments)
            {
                trackers.Add(new EnvTracker(env, null));
            }

            isSetup = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (!isSetup)
                return;

            if (currentGenotypeIndex < optimizationSettings.populationSize)
            {
                // Haven't spawned all, so loop through genotypes
                bool completed = true;
                for (int i = currentGenotypeIndex; i < optimizationSettings.populationSize; i++)
                {
                    CreatureGenotypeEval currentEval = currentGeneration.cgEvals[i];
                    EnvTracker envTracker = FindAvailableEnvironment();
                    if (envTracker != null)
                    {
                        envTracker.idx = i; // Storing genotype index
                        envTracker.env.StartEnv(currentEval.cg);
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
            else if (untestedRemaining <= 0)
            {
                CreateNextGeneration(false);
            }
        }

        public override void ResetPing(Environment env, float fitness){
            // Set fitness info
            EnvTracker result = trackers.First(t => t.env == env);
            if (result == null || result.idx == null){
                throw new System.Exception();
            }
            currentGeneration.cgEvals[(int)result.idx].fitness = fitness;
            untestedRemaining--;
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

        private void CreateNextGeneration(bool first)
        {
            currentGenotypeIndex = 0;
            
            untestedRemaining = optimizationSettings.populationSize;
            if (first){
                currentGenerationIndex = 0;
                currentGeneration = new Generation(optimizationSettings.populationSize, optimizationSettings.initialGenotype, optimizationSettings.mp);
                saveK.generations = new List<Generation>();
                saveK.generations.Add(currentGeneration);
            } else {
                currentGenerationIndex++;
                CreatureGenotype bestGenotype = SelectBestGenotype();
                currentGeneration = new Generation(optimizationSettings.populationSize, bestGenotype, optimizationSettings.mp);
                saveK.generations.Add(currentGeneration);
            }
        }

        private CreatureGenotype SelectBestGenotype()
        {
            CreatureGenotypeEval bestEval = saveK.generations.Last().cgEvals.OrderByDescending(cgEval => cgEval.fitness).FirstOrDefault();
            return bestEval.cg;
        }
    }
}