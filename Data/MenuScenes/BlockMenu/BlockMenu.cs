using Godot;
using Stellacrum.Data.ObjectLoaders;
using System;
using System.Collections.Generic;

public partial class BlockMenu : CanvasLayer
{
	[Signal]
	public delegate void SwitchMenuEventHandler(int toShow);

	Button ReturnButton;
	GridContainer BlockContainer;
	Dictionary<DraggableItem, string> BlockIcons = new();
	GameScene scene;

	hud_scene HUD;

	public override void _Ready()
	{
		ReturnButton = FindChild("ReturnButton") as Button;
		ReturnButton.Pressed += ReturnPress;

		BlockContainer = FindChild("BlockContainer") as GridContainer;
		scene = GameScene.GetGameScene(this);
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

            HUD = scene.playerCharacter.HUD;
        }

		// Return if esc or tab pressed
		if(Input.IsActionPressed("Pause") || Input.IsActionJustPressed("BlockInventory"))
		{
			Input.ActionRelease("Pause");
            Input.ActionRelease("BlockInventory");
            ReturnPress();
		}

		// Unset block if right-clicked
		if (Input.IsActionJustPressed("MousePressR"))
			OnRightClick();
	}

	void OnIconRelease(DraggableItem blockIcon, Vector2 pos)
	{
        for (int i = 0; i < 10; i++)
			if (GetSlotBox(HUD.ToolbarIcons[i]).HasPoint(pos))
				HUD.SetToolbar(i, BlockIcons[blockIcon]);
	}

	void OnRightClick()
	{
        for (int i = 0; i < 10; i++)
            if (GetSlotBox(HUD.ToolbarIcons[i]).HasPoint(GetViewport().GetMousePosition()))
                HUD.SetToolbar(i, "");
    }

    static Rect2 GetSlotBox(TextureRect rect)
    {
        return new(rect.GlobalPosition, rect.Size);
    }

    void ReturnPress()
	{
		EmitSignal(SignalName.SwitchMenu, 0);
	}
}
