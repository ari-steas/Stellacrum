using Godot;
using System;

public partial class ItemList : Godot.ItemList
{
	[Signal]
	public delegate void ShowWorldEventHandler();

	int[] selectedItems;
	int currentlySelected = -1;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Update();
	}

	public void Update()
	{
		Clear();
		
		
		currentlySelected = -1;

		WorldLoader.ScanWorlds();

		foreach (var world in WorldLoader.Worlds)
		{
			AddItem(world.Name);
			GD.Print("AddItem " + world.Name);
		}
		AddItem("NEW SAVE");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		selectedItems = GetSelectedItems();
		
		if (selectedItems.Length == 0)
			return;

		if (currentlySelected != selectedItems[0])
		{
			if (selectedItems[0] == ItemCount - 1)
			{
				WorldLoader.SetWorld(-1);
				EmitSignal(SignalName.ShowWorld);
			}
			else
			{
				WorldLoader.SetWorld(selectedItems[0]);
				EmitSignal(SignalName.ShowWorld);
			}
			
			currentlySelected = selectedItems[0];
		}
	}
}
