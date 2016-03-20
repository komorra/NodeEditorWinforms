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

### Serialization



