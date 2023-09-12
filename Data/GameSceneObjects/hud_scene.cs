using Godot;
using System;
using System.Linq;

public partial class hud_scene : CanvasLayer
{
	[Signal]
	public delegate void LightToggleEventHandler(bool enabled);
	[Signal]
	public delegate void DampenersToggleEventHandler(bool enabled);
	[Signal]
	public delegate void ThirdPersonToggleEventHandler(bool enabled);

	private bool _lightEnabled = true, _dampenersEnabled = true, _thirdPerson = false;

	private Label dampenersLabel, speedLabel;

	private player_character player;
	public readonly TextureRect[] ToolbarIcons = new TextureRect[10];

	public string[] Toolbar = new string[] {
		"",
		"ArmorBlock",
		"ArmorBlockSlope",
		"ArmorBlockSlopeCorner",
		"ArmorBlockSlopeCornerInv",
		"ThrusterSmall",
		"Cockpit",
		"",
		"",
		"",
	};

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Visible = false;
		dampenersLabel = GetNode<Label>("DampenersLabel");
		speedLabel = GetNode<Label>("SpeedLabel");

		player = GetParent<player_character>();

		ToolbarIcons[0] = FindChild("Icon0") as TextureRect;
		ToolbarIcons[1] = FindChild("Icon1") as TextureRect;
		ToolbarIcons[2] = FindChild("Icon2") as TextureRect;
		ToolbarIcons[3] = FindChild("Icon3") as TextureRect;
		ToolbarIcons[4] = FindChild("Icon4") as TextureRect;
		ToolbarIcons[5] = FindChild("Icon5") as TextureRect;
		ToolbarIcons[6] = FindChild("Icon6") as TextureRect;
		ToolbarIcons[7] = FindChild("Icon7") as TextureRect;
		ToolbarIcons[8] = FindChild("Icon8") as TextureRect;
		ToolbarIcons[9] = FindChild("Icon9") as TextureRect;

		VisibilityChanged += OnVisibilityChanged;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Visible = player.Visible;

		if (Input.IsActionJustPressed("SpotlightToggle"))
		{
			_lightEnabled = !_lightEnabled;
			EmitSignal(SignalName.LightToggle, _lightEnabled);
		}

		if (Input.IsActionJustPressed("DampenersToggle"))
		{
			_dampenersEnabled = !player._dampenersEnabled;
			EmitSignal(SignalName.DampenersToggle, _dampenersEnabled);
			dampenersLabel.Text = "DAMPENERS " + (_dampenersEnabled ? "ENABLED" : "DISABLED");
		}

		if (Input.IsActionJustPressed("ThirdPersonToggle"))
		{
			_thirdPerson = !_thirdPerson;
			EmitSignal(SignalName.ThirdPersonToggle, _thirdPerson);
		}

		speedLabel.Text = "Speed: " + (int)((player.IsInCockpit ? player.currentGrid.Speed : player.Velocity.Length())*100)/100f;
	}

	public void SetToolbar(int slot, string subTypeId)
	{
		if (slot == 0 || Toolbar[slot].Equals(subTypeId))
			return;

		if (CubeBlockLoader.GetAllIds().Contains(subTypeId))
            for (int i = 0; i < 10; i++)
                if (Toolbar[i].Equals(subTypeId))
                    SetToolbar(i, "");

		Toolbar[slot] = subTypeId;
		UpdateToolbar(slot);
	}

	void UpdateToolbar(int slot)
	{
        if (!CubeBlockLoader.GetAllIds().Contains(Toolbar[slot]))
        {
            ToolbarIcons[slot].Texture = TextureLoader.Get("EmptyToolbar.png");
            return;
        }

        ToolbarIcons[slot].Texture = CubeBlockLoader.GetTexture(Toolbar[slot]);
	}

	private void OnVisibilityChanged()
	{
		if (Visible)
			for (int i = 0; i < 10; i++)
				UpdateToolbar(i);
	}
}
