using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KSS;

public class EvolutionViewMenu : MonoBehaviour
{
    KSSSave save;
    public CreatureGenotype best;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ExportCSV(){
        if (save == null) return;
        save.ExportCSV();
    }

    public void SetEvolution(KSSSave save){
        this.save = save;
        best = save.best.cg;
    }
}
