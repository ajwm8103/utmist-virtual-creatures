using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public class PhenotypeNode : Node
{
    public string NodeName { get; set; }
    public string Text { get; set; }

    public void Draw()
    {
        /* TITLE CONTAINER */

        TextField nodeNameTextField = new TextField()
        {
            value = NodeName
        };

        titleContainer.Insert(0, nodeNameTextField);

        Port inputPortA = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
        inputPortA.portName = "a";
        inputContainer.Add(inputPortA);

        Port inputPortB = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
        inputPortB.portName = "b";
        inputContainer.Add(inputPortB);

        Port inputPortC = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
        inputPortC.portName = "c";
        inputContainer.Add(inputPortC);


        Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
        outputPort.portName = "out";
        outputContainer.Add(outputPort);
    }
}
