using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
// using UnityEditor;

public abstract class TrainingAlgorithm : MonoBehaviour
{
    [SerializeField]
    protected TrainingManager tm;
    [SerializeField]
    protected TrainingSave save;
    
    // Setup is called after all vars are updated and sent to the TrainingAlgorithm
    public virtual void Setup(TrainingManager tm){
        this.tm = tm;
        save = tm.save;
    }

    public abstract void ResetPing(Environment env, float fitness, bool isDQ);

    public void SaveTraining(){
        Debug.Log("Saving Current TrainingSave");
        // string path = EditorUtility.SaveFilePanel("Save Training Save As", "C:", save.saveName + ".save", "save");
        string path = Path.Combine(OptionsPersist.instance.VCSaves, save.saveName + ".save");
        save.SaveData(path, true);
        Debug.Log(Application.persistentDataPath);
    }
}
