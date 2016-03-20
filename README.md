Node Editor Winforms
=======

Node Editor Winforms is a Windows Forms class library project, that provides nodal editor control for general usage - e.g. for sound processing applications, graphics editors, logic or control flow editors and many other. It contains Windows Forms user control that after referencing it to your project could be added via UI designer in Visual Studio.

![Example of Node Editor usage in 3D application](http://i.imgur.com/GDJG8pf.png)

## Features

* Automatic context menu creation based on methods with Node attribute detected in INodeContext class instance
* Nodes context menu could be hierarchical by providing additional menu parameter in Node attribute e.g. menu:"Filters/LFO"
* Node graph could be executed or resolved automatically by calling Execute or Resolve method in NodesControl instance
* Custom editors (other Winforms controls) inside nodes by specifying customEditor Node attribute parameter e.g. customEditor:typeof(MyUserControl)
* Nodes could be selected, multi-selected by shift click or by dragging selection area around
* Flexible connections attractive visually
* Easy integration with property grid

## Usage

### Step 1 - Building

To use Node Editor first obtain source code from here, then open it in Visual Studio 2015 and finally build. The Release configuration may be considered when using in production - it really has much better rendering speed. Also good information here is that the Nodes Editor is written on top only .NET Framework 4.0 - no other third party libraries are involved. However you could use Node Editor with other third party libraries as well - it accepts types from other assemblies too ad node inputs/outputs.

### Step 2 - Referencing to your project

Within the Winforms designer right click on the Toolbox and select Choose Items ... in context menu. In the Choose Toolbox Items dialog window choose .NET Framework Components tab page, then browse to the built library DLL. After accepting there should be User Control named NodesControl somewhere in the toolbox. Now you can easily drag-drop the control into your UI.

### Step 3 - Adding & extending Node Editor context type

When you have NodesControl in your form or user control you should do first some minimal preparation. This step involves creating special context class that should implement INodesContext interface. In this context class you can put methods that will be exposed automatically as nodes by adding Node attribute to each method. Look at the example:

```cs
    // Our context class that implements INodeContext interface
    public class FContext : INodesContext
    {
        // This is implementation of INodesContext.
        // CurrentProcessingNode is the node that is being actually executed.
        public NodeVisual CurrentProcessingNode { get; set; }
        
        // Some your project specific methods, events, properties and so on
        public event Action<Model3D, Matrix> Placement = delegate { };
        public event Action<Bounding> ShowBounding = delegate { }; 
        public event Action Clear = delegate { };

        // Starter node - it has isExecutionInitiator as true, so it will have one execution path output
        // name is the node name displayed in node caption
        // menu is the path splitted by '/' character if you want to build hierarchical menu
        // description is a node description that will be sent in special event at some situations
        [Node(name:"Starter", menu:"General", isExecutionInitiator:true, description:"Node from which processing begins.")]
        public void Starter()
        {
            Clear();
        }

        // Node with one input (object obj)
        [Node(name:"Display Object", menu:"Debug", description:"Allows to show any output in popup message box.")]
        public void ShowMessage(object obj)
        {
            MessageBox.Show(obj.ToString(), "Nodes Debug: " + obj.GetType().Name, MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // Node with custom editor that has two inputs (string path and bool useCage) and one output socket (out Model3D model)
        [Node(name: "Load Model", menu:"General", customEditor:typeof(NELoadModel3D), description:"Node that loads 3D model from disk and also textures together.")]
        public void LoadModel(string path, bool useCage, out Model3D model)
        {
            model = new Model3D();
            var fileNames = path.Split(';');
            InitAsset(fileNames, model.Asset);
            model.Asset.MakeObject(useCage);
        }
      }
```

Next thing is to create our context object and put it in our NodesControl:

```cs
  var context = new FContext();
  nodesControl1.Context = context;
```

Now you have right setup and during application runtime, there context menu will appear after right clicking on NodesControl.

### Serialization

NodesControl has byte[] Serialize() and Deserialize(byte[] data) methods that allow you to save and load node graph state. However you should design your classes that you use as inputs/outputs of nodes with some additional code due to proper serialization and deserialization.

* The class for node input/output should have [Serializable] attribute
* It is good to have also  [TypeConverter(typeof(ExpandableObjectConverter))] attribute in order to work properly with property grids
* The class should implement ISerializable interface
* The class should has public constructor and private one, that has parameters: (SerializationInfo info, StreamingContext ctx)
* The class should implement GetObjectData method of ISerializable

This is example of such a class:

```cs
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Vector3W : ISerializable
    {
        public Vector3 Value;

        public float X { get { return Value.X; } set { Value.X = value; } }
        public float Y { get { return Value.Y; } set { Value.Y = value; } }
        public float Z { get { return Value.Z; } set { Value.Z = value; } }

        public override string ToString()
        {
            return Value.ToString();
        }

        public Vector3W()
        {
            
        }

        private Vector3W(SerializationInfo info, StreamingContext ctx)
        {
            X = info.GetSingle("X");
            Y = info.GetSingle("Y");
            Z = info.GetSingle("Z");
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("X", X);
            info.AddValue("Y", Y);
            info.AddValue("Z", Z);
        }
    }
```

If you are unable to follow this rules (e.g. you would to use third party library types) you should write wrapper for that classes, that meets the above conditions.

### Custom Node Editors

Custom node editor can be any object that is subclass of System.Windows.Forms.Control class. It is maintained automatically (created and handled) by NodesControl. You can specify custom editor for each node by giving its type as customEditor parameter of Node attribute. Keep in mind that custom editor will get NodeVisual object on its Tag property, so you can easily interact with node state.

NodeVisual object has a method named GetNodeContext() by which you can obtain current node state - which is dynamic object, so you can just type member name and if it is not present, the member will be created (property). Node state is a set of properties related to its inputs and outputs.

Here is an example, inside your UserControl class:

```cs
    var node = (Tag as NodeVisual);
    dynamic context = node.GetNodeContext();
    var myVariable = context.model;
    var myVariable2 = context.transform;
```

### Execution / Resolving

To start execution just simply call nodesControl1.Execute() (without parameters), which will call the starter node (the node you had marked as isExecutionInitiator:true). After that, the graph will execute through execution path (yellow connections). While the execution process is running, each node that is being actually executed will be put into the CurrentProcessingNode property of your context.

To resolve any node just call nodesControl1.Resolve(yourNodeHere) where yourNodeHere is NodeVisual class object (it must be node that comes from graph). 

Resolving differs from execution, that it not need execution path to be provided. It just resolve a node with all its dependant nodes.


### User Interaction

* Right click on NodesControl to bring context menu with nodes available to put
* Click on node to select it, shift-click to multi select
* Ctrl+LMB on connection socket to unpin it and drag elsewhere

### Credits & Contact

Programming: Mariusz Komorowski - [@komorra86](https://twitter.com/komorra86)
