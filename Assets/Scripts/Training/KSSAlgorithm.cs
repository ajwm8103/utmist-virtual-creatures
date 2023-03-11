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
    }

    [System.Serializable]
    public class CreatureGenotypeEval {
        public CreatureGenotype cg;
        public float fitness; // total reward
    }

    public class KSSAlgorithm : TrainingAlgorithm
    {

        public KSSSave save;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}