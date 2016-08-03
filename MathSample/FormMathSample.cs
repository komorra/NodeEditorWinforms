using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MathSample
{
    public partial class FormMathSample : Form
    {
        //Context that will be used for our nodes
        MathContext context = new MathContext();

        public FormMathSample()
        {
            InitializeComponent();
        }

        private void FormMathSample_Load(object sender, EventArgs e)
        {
            //Context assignment
            controlNodeEditor.nodesControl.Context = context;
            controlNodeEditor.nodesControl.OnNodeContextSelected += NodesControlOnOnNodeContextSelected; 
            
            //Loading sample from file
            controlNodeEditor.nodesControl.Deserialize(File.ReadAllBytes("default.nds"));
        }

        private void NodesControlOnOnNodeContextSelected(object o)
        {
            controlNodeEditor.propertyGrid.SelectedObject = o;
        }
    }
}
