using Godot;
using System;
using System.Collections.Generic;

public partial class BlockMenu : CanvasLayer
{
	[Signal]
	public delegate void SwitchMenuEventHandler(int toShow);

	Button ReturnButton;
	GridContainer BlockContainer;
	Dictionary<DraggableItem, string> BlockIcons = new();

	hud_scene HUD;

	public override void _Ready()
	{
		ReturnButton = FindChild("ReturnButton") as Button;
		ReturnButton.Pressed += ReturnPress;

		BlockContainer = FindChild("BlockContainer") as GridContainer;

		// kinda cringe code
		HUD = (GetParent().FindChild("GameScene") as GameScene).playerCharacter.HUD;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (!Visible)
			return;

		// Populate blocks
		if (BlockContainer.GetChildCount() == 0)
        {
            foreach (string Id in CubeBlockLoader.GetAllIds())
            {
				DraggableItem icon = new()
				{
					Texture = CubeBlockLoader.GetTexture(Id),
					ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
					StretchMode = TextureRect.StretchModeEnum.KeepAspect,
					CustomMinimumSize = new(125, 125),
					OnRelease = OnIconRelease,
				};

				BlockIcons.Add(icon, Id);

                BlockContainer.AddChild(icon);
            }
		}

		// Return if esc pressed
		if(Input.IsActionPressed("Pause"))
		{
			Input.ActionRelease("Pause");
			ReturnPress();
		}
	}

	void OnIconRelease(DraggableItem blockIcon, Vector2 pos)
	{
		foreach (var icon in HUD.ToolbarIcons)
		{
			if (icon._HasPoint(pos))
			{
                GD.Print("Release " + BlockIcons[blockIcon]);
            }
		}
	}

    void ReturnPress()
	{
		EmitSignal(SignalName.SwitchMenu, 0);
	}
}
