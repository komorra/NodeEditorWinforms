using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NodeEditor;

namespace MathSample
{
    // Main context of the sample, each
    // method corresponds to a node by attribute decoration
    public class MathContext : INodesContext
    {
        public NodeVisual CurrentProcessingNode { get; set; }
        public event Action<string, NodeVisual, FeedbackType, object, bool> FeedbackInfo;

        [Node("Value", "Input", "Basic", "Allows to output a simple value.",false)]
        public void InputValue(float inValue, out float outValue)
        {
            outValue = inValue;
        }

        [Node("Add","Operators","Basic","Adds two input values.",false)]
        public void Add(float a, float b, out float result)
        {
            result = a + b;
        }

        [Node("Substract", "Operators", "Basic", "Substracts two input values.", false)]
        public void Substract(float a, float b, out float result)
        {
            result = a - b;
        }

        [Node("Multiply", "Operators", "Basic", "Multiplies two input values.", false)]
        public void Multiplty(float a, float b, out float result)
        {
            result = a * b;
        }

        [Node("Divide", "Operators", "Basic", "Divides two input values.", false)]
        public void Divid(float a, float b, out float result)
        {
            result = a / b;
        }

        [Node("Show Value","Helper","Basic","Shows input value in the message box.")]
        public void ShowMessageBox(float x)
        {
            MessageBox.Show(x.ToString(), "Show Value", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        [Node("Starter","Helper","Basic","Starts execution",true,true)]
        public void Starter()
        {
            
        }
    }
}
