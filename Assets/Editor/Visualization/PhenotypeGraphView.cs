using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
public class PhenotypeGraphView : GraphView
{
    public PhenotypeGraphView()
    {
        AddManipulators();
        AddGridBackground();

        CreateNode();

        AddStyles();
    }

    private void CreateNode()
    {
        PhenotypeNode node = new PhenotypeNode();
        node.Draw();
        AddElement(node);

        PhenotypeNode node2 = new PhenotypeNode();
        node2.Draw();
        AddElement(node2);
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> compatiblePorts = new List<Port>();

        // something about this function seems fishy, i feel like there shouldn't be a need to loop through all ports??
        ports.ForEach(port =>
        {
            if (startPort.node == port.node)
            {
                return;
            }

            if (startPort.direction == port.direction)
            {
                return;
            }

            compatiblePorts.Add(port);
        });

        return compatiblePorts;
    }

    private void AddManipulators()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
    }

    private void AddGridBackground()
    {
        GridBackground gridBackground = new GridBackground();
        gridBackground.StretchToParentSize();
        Insert(0, gridBackground);
    }

    private void AddStyles()
    {
        StyleSheet styleSheet = (StyleSheet)EditorGUIUtility.Load("Assets/" +
            "Scripts/Visualization/PhenotypeGraphViewStyleSheet.uss");

        styleSheets.Add(styleSheet);
    }
}
