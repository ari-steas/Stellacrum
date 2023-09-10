using Godot;
using System;

public partial class BlockMenu : CanvasLayer
{
	[Signal]
	public delegate void SwitchMenuEventHandler(int toShow);

	Button ReturnButton;
	GridContainer BlockContainer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ReturnButton = FindChild("ReturnButton") as Button;
		ReturnButton.Pressed += ReturnPress;

		BlockContainer = FindChild("BlockContainer") as GridContainer;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!Visible)
			return;

		if (BlockContainer.GetChildCount() == 0)
			foreach (var Id in CubeBlockLoader.GetAllIds())
				BlockContainer.AddChild(new TextureRect() { Texture = CubeBlockLoader.GetTexture(Id), ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspect, CustomMinimumSize = new(125, 125) });

		if(Input.IsActionPressed("Pause"))
		{
			Input.ActionRelease("Pause");
			ReturnPress();
		}
	}

	void ReturnPress()
	{
		EmitSignal(SignalName.SwitchMenu, 0);
	}
}
