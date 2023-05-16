using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

public class PhenotypeGraph : EditorWindow
{

    // Start is called before the first frame update
    [MenuItem("Graph/Phenotype Graph")]
    public static void OpenPhenotypeGraphWindow()
    {
        var window = GetWindow<PhenotypeGraph>();
        window.titleContent = new GUIContent(text: "Phenotype Graph");
    }

    private void OnEnable()
    {
        AddGraphView();
        AddStyles();
    }

    private void AddGraphView()
    {
        PhenotypeGraphView graphView = new PhenotypeGraphView();
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    private void AddStyles()
    {
        StyleSheet styleSheet = (StyleSheet)EditorGUIUtility.Load("Assets/" +
            "Scripts/Visualization/PhenotypeVariables.uss");

        rootVisualElement.styleSheets.Add(styleSheet);
    }
}
