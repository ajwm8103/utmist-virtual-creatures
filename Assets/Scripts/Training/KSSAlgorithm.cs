using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace KSS
{
    [System.Serializable]
    public class KSSSave : TrainingSave
    {
        [HideInInspector]
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
        public Generation(){
            cgEvals = new List<CreatureGenotypeEval>();
        }

        public static Generation FromInitial(int size, CreatureGenotype initialGenotype, MutateGenotype.MutationPreferenceSetting mp){
            Generation g = new Generation();
            g.cgEvals = new List<CreatureGenotypeEval>();
            if (initialGenotype == null)
            {
                // Random generation
                for (int i = 0; i < size; i++)
                {
                    CreatureGenotypeEval cgEval = new CreatureGenotypeEval(MutateGenotype.GenerateRandomCreatureGenotype(mp));
                    g.cgEvals.Add(cgEval);
                }
            }
            else
            {
                // Mutated generation
                for (int i = 0; i < size; i++)
                {
                    // CreatureGenotypeEval cgEval = new CreatureGenotypeEval(initialGenotype);
                    CreatureGenotypeEval cgEval = new CreatureGenotypeEval(MutateGenotype.MutateCreatureGenotype(initialGenotype, mp));
                    g.cgEvals.Add(cgEval);
                }
            }
            return g;
        }

        public static Generation FromMutation(int size, List<CreatureGenotypeEval> topEvals, MutateGenotype.MutationPreferenceSetting mp)
        {
            Generation g = new Generation();

            int topCount = topEvals.Count;
            int remainingCount = size - topEvals.Count;

            List<CreatureGenotypeEval> topSoftmaxEvals = new List<CreatureGenotypeEval>();
            float maxFitness = topEvals.Max(x => (float)x.fitness);
            float denom = topEvals.Select(x => Mathf.Exp((float)x.fitness - maxFitness)).Sum();
            
            foreach (CreatureGenotypeEval topEval in topEvals)
            {
                g.cgEvals.Add(new CreatureGenotypeEval(topEval.cg));
                CreatureGenotypeEval topSoftmaxEval = topEval.ShallowCopy();
                topSoftmaxEval.fitness = Mathf.Exp((float)topSoftmaxEval.fitness - maxFitness);
                topSoftmaxEvals.Add(topSoftmaxEval);
            }

            topSoftmaxEvals = topSoftmaxEvals.OrderByDescending(x => x.fitness).ToList();
            foreach (CreatureGenotypeEval topSoftmaxEval in topSoftmaxEvals)
            {
                int childrenCount = Mathf.RoundToInt(remainingCount * (float)topSoftmaxEval.fitness / denom);
                //Debug.Log(remainingCount + " " + childrenCount + " " + topSoftmaxEval.fitness + " " + denom);
                if (remainingCount == 0) break;
                remainingCount -= childrenCount;
                denom -= (float)topSoftmaxEval.fitness;
                //Debug.Log(remainingCount + " " + denom);

                for (int i = 0; i < childrenCount; i++)
                {
                    g.cgEvals.Add(new CreatureGenotypeEval(MutateGenotype.MutateCreatureGenotype(topSoftmaxEval.cg, mp)));
                }
            }

            if (g.cgEvals.Count != size){
                Debug.Log(remainingCount);
                Debug.Log(topSoftmaxEvals.Select(x => x.fitness).ToList());

                foreach (CreatureGenotypeEval topSoftmaxEval in topSoftmaxEvals)
                {
                    Debug.Log(topSoftmaxEval.fitness);
                }

                throw new System.Exception("Wrong generation size!");
            }

            return g;

        }
    }

    public enum EvalStatus { NOT_EVALUATED, EVALUATED, DISQUALIFIED };
    [System.Serializable]
    public class CreatureGenotypeEval {
        public CreatureGenotype cg;
        public float? fitness; // total reward
        public EvalStatus evalStatus = EvalStatus.NOT_EVALUATED;

        public CreatureGenotypeEval(CreatureGenotype cg){
            this.cg = cg;
            fitness = 0;
            evalStatus = EvalStatus.NOT_EVALUATED;
        }

        public CreatureGenotypeEval ShallowCopy(){
            CreatureGenotypeEval cgEval = new CreatureGenotypeEval(cg);
            cgEval.fitness = fitness;
            cgEval.evalStatus = evalStatus;
            return cgEval;
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

        public override void ResetPing(Environment env, float fitness, bool isDQ)
        {
            // Set fitness info
            EnvTracker result = trackers.First(t => t.env == env);
            if (result == null || result.idx == null){
                throw new System.Exception();
            }
            CreatureGenotypeEval eval = currentGeneration.cgEvals[(int)result.idx];
            eval.fitness = isDQ ? null : fitness;
            eval.evalStatus = isDQ ? EvalStatus.DISQUALIFIED : EvalStatus.EVALUATED;
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
                currentGeneration = Generation.FromInitial(optimizationSettings.populationSize, optimizationSettings.initialGenotype, optimizationSettings.mp);
                saveK.generations = new List<Generation>();
                saveK.generations.Add(currentGeneration);
            } else {
                currentGenerationIndex++;
                List<CreatureGenotypeEval> topEvals = SelectTopEvals(currentGeneration, optimizationSettings.mp);
                currentGeneration = Generation.FromMutation(optimizationSettings.populationSize, topEvals, optimizationSettings.mp);
                //CreatureGenotype bestGenotype = SelectBestGenotype(currentGeneration);
                //currentGeneration = Generation.FromInitial(optimizationSettings.populationSize, bestGenotype, optimizationSettings.mp);
                saveK.generations.Add(currentGeneration);
            }
        }

        private List<CreatureGenotypeEval> SelectTopEvals(Generation g, MutateGenotype.MutationPreferenceSetting mp)
        {
            List<CreatureGenotypeEval> topEvals = new List<CreatureGenotypeEval>();
            List<CreatureGenotypeEval> sortedEvals = g.cgEvals.OrderByDescending(x => x.fitness).ToList();
            int topCount = Mathf.RoundToInt(optimizationSettings.populationSize * optimizationSettings.survivalRatio);
            int positiveCount = 0;
            for (int i = 0; i < topCount; i++)
            {
                CreatureGenotypeEval eval = g.cgEvals[i];
                if (eval.fitness != null && eval.fitness >= 0) {
                    topEvals.Add(eval);
                    positiveCount++;
                } else {
                    CreatureGenotypeEval cgEval = new CreatureGenotypeEval(optimizationSettings.initialGenotype.Clone());
                    //CreatureGenotypeEval cgEval = new CreatureGenotypeEval(MutateGenotype.GenerateRandomCreatureGenotype());
                    topEvals.Add(cgEval);
                }
            }

            Debug.Log(positiveCount + " Creatures with >=0 fitness.");
            Debug.Log("Best: " + topEvals.Max(x => x.fitness));
            return topEvals;
        }

        private CreatureGenotype SelectBestGenotype(Generation g)
        {
            CreatureGenotypeEval bestEval = g.cgEvals.OrderByDescending(cgEval => cgEval.fitness).FirstOrDefault();
            Debug.Log("Best: " + bestEval.fitness);
            return bestEval.cg;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(KSSAlgorithm))]
    public class KSSAlgorithmEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            KSSAlgorithm alg = target as KSSAlgorithm;

            if (GUILayout.Button("Save Current KSSSave"))
            {
                Debug.Log("Saving Current KSSSave");
                KSSSave save = alg.saveK;
                save.SaveData("/" + save.saveName + ".saveK", false);
                Debug.Log(Application.persistentDataPath);
            }
        }
    }
#endif

}