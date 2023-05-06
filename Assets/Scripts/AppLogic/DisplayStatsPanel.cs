using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayStatsPanel : MonoBehaviour
{
    private Text text;
    public static DisplayStatsPanel instance;
    public Creature currentCreature;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject); // Can't have two panels active at once
        }
        else
        {
            instance = this;
        }
    }

    public void UpdateCreatureStats(Creature c){
        currentCreature = c;
    }

    public void Start()
    {
        text = GetComponent<Text>();
    }

    public void Update(){
        if (currentCreature == null) {
            text.text = "No Selected Creature.";
            return;
        }

        string name = currentCreature.cg.name;
        string totalReward = currentCreature.totalReward.ToString();

        text.text = string.Format("{0}, Total Reward: {1}", name, totalReward);
    }
}
