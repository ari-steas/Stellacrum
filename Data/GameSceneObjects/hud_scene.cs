using Godot;
using System;

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
	private readonly TextureRect[] toolbar = new TextureRect[10];

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

		toolbar[0] = FindChild("Icon0") as TextureRect;
		toolbar[1] = FindChild("Icon1") as TextureRect;
		toolbar[2] = FindChild("Icon2") as TextureRect;
		toolbar[3] = FindChild("Icon3") as TextureRect;
		toolbar[4] = FindChild("Icon4") as TextureRect;
		toolbar[5] = FindChild("Icon5") as TextureRect;
		toolbar[6] = FindChild("Icon6") as TextureRect;
		toolbar[7] = FindChild("Icon7") as TextureRect;
		toolbar[8] = FindChild("Icon8") as TextureRect;
		toolbar[9] = FindChild("Icon9") as TextureRect;

		VisibilityChanged += OnVisibilityChanged;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Visible = ((Node3D)GetParent()).Visible;

		if (Input.IsActionJustPressed("SpotlightToggle"))
		{
			_lightEnabled = !_lightEnabled;
			EmitSignal(SignalName.LightToggle, _lightEnabled);
		}

		if (Input.IsActionJustPressed("DampenersToggle"))
		{
			_dampenersEnabled = !_dampenersEnabled;
			EmitSignal(SignalName.DampenersToggle, _dampenersEnabled);
			dampenersLabel.Text = "DAMPENERS " + (_dampenersEnabled ? "ENABLED" : "DISABLED");
		}

		if (Input.IsActionJustPressed("ThirdPersonToggle"))
		{
			_thirdPerson = !_thirdPerson;
			EmitSignal(SignalName.ThirdPersonToggle, _thirdPerson);
		}

		speedLabel.Text = "Speed: " + (int)((player.IsInCockpit ? (player.GetParent().GetParent() as CubeGrid).Speed : player.Velocity.Length())*100)/100f;
	}

	public void SetToolbar(int slot, string subTypeId)
	{
		if (subTypeId == "")
		{
			toolbar[slot].Texture = TextureLoader.Get("EmptyToolbar.png");
			return;
		}

		toolbar[slot].Texture = CubeBlockLoader.GetTexture(subTypeId);
	}

	private void OnVisibilityChanged()
	{
		if (Visible)
			for (int i = 0; i < 10; i++)
				SetToolbar(i, Toolbar[i]);
	}
}
