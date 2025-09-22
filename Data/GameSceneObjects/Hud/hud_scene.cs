using GameSceneObjects;
using Godot;
using System;
using System.Linq;
using System.Text;

public partial class hud_scene : CanvasLayer
{
	[Signal]
	public delegate void LightToggleEventHandler(bool enabled);
	[Signal]
	public delegate void DampenersToggleEventHandler(bool enabled);
	[Signal]
	public delegate void ThirdPersonToggleEventHandler(bool enabled);

	private bool _lightEnabled = true, _dampenersEnabled = true, _thirdPerson = false;

	private Label speedLabel, _tooltipLabel;

	private player_character player;
	public ToolbarObject[] ToolbarIcons = Array.Empty<ToolbarObject>();

    private TooltipFlags _tooltip = TooltipFlags.None;
	public TooltipFlags Tooltip
    {
		get => _tooltip;
        set
        {
            if (_tooltip == value)
                return;

            _tooltip = value;
            StringBuilder tooltipBuilder = new StringBuilder();
            if ((_tooltip & TooltipFlags.Cockpit) == TooltipFlags.Cockpit)
                tooltipBuilder.Append('[').Append(string.Join("/", InputMap.ActionGetEvents("Interact").Select(e => e.AsText()))).AppendLine("] Enter Cockpit");
            if ((_tooltip & TooltipFlags.Terminal) == TooltipFlags.Terminal)
                tooltipBuilder.Append('[').Append(string.Join("/", InputMap.ActionGetEvents("OpenTerminal").Select(e => e.AsText()))).AppendLine("] Grid Terminal");
            if ((_tooltip & TooltipFlags.Inventory) == TooltipFlags.Inventory)
                tooltipBuilder.Append('[').Append(string.Join("/", InputMap.ActionGetEvents("OpenInventory").Select(e => e.AsText()))).AppendLine("] Grid Inventory");
            _tooltipLabel.Text = tooltipBuilder.ToString();
        }
    }

	public readonly string[] DefaultToolbar = { // TODO move to a more sensible location
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
		speedLabel = GetNode<Label>("SpeedLabel");
		_tooltipLabel = GetNode<Label>("TooltipLabel");
        _tooltipLabel.Text = "";

		player = GetParent<player_character>();

        var toolbarContainer = FindChild("ToolbarContainer");
        ToolbarIcons = new ToolbarObject[toolbarContainer.GetChildCount()];
        ToolbarIcons[0] = (ToolbarObject) toolbarContainer.GetChild(ToolbarIcons.Length-1);
        for (int i = 0; i < ToolbarIcons.Length - 1; i++)
        {
            ToolbarIcons[i+1] = (ToolbarObject) toolbarContainer.GetChild(i);
        }

		for (int i = 0; i < ToolbarIcons.Length; i++)
			ToolbarIcons[i].SetBind(i);

        WorldLoader.OnLoad += RefreshToolbar;
        //VisibilityChanged += OnVisibilityChanged;
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
		}

		if (Input.IsActionJustPressed("ThirdPersonToggle"))
		{
			_thirdPerson = !_thirdPerson;
			EmitSignal(SignalName.ThirdPersonToggle, _thirdPerson);
		}

		speedLabel.Text = $"Position: {player.GlobalPosition.ToString("N")}\n" +
                          $"Orientation: {player.GlobalRotation.ToString("N")}\n" +
                          $"Speed: {(player.IsInCockpit ? player.currentGrid.Speed : player.Velocity.Length()):F1}m/s\n" + 
                          $"Dampeners: {player._dampenersEnabled}";
	}

	public void SetToolbar(int slot, string subTypeId)
	{
		if (slot == 0 || ToolbarIcons[slot].BlockSubtype == subTypeId)
			return;

        if (subTypeId != "")
        {
            foreach (var toolbar in ToolbarIcons)
            {
                if (!toolbar.BlockSubtype.Equals(subTypeId)) continue;
                toolbar.BlockSubtype = "";
                break;
            }
        }

		ToolbarIcons[slot].BlockSubtype = subTypeId;
	}

    public string SelectSlot(int slot)
    {
        for (var i = 0; i < ToolbarIcons.Length; i++)
            ToolbarIcons[i].Selected = i == slot && ToolbarIcons[i].BlockSubtype != "";
        return ToolbarIcons[slot].BlockSubtype;
    }

    public string[] SerializedToolbar()
    {
		string[] toolbar = new string[ToolbarIcons.Length];
        for (int i = 0; i < ToolbarIcons.Length; i++)
            toolbar[i] = ToolbarIcons[i].BlockSubtype;
		return toolbar;
    }

    //void UpdateToolbar(int slot) => ToolbarIcons[slot].Block = Toolbar[slot];
	//
	//private void OnVisibilityChanged()
	//{
	//	if (Visible)
	//		for (int i = 0; i < 10; i++)
	//			UpdateToolbar(i);
	//}

    private void RefreshToolbar()
    {
        foreach (var icon in ToolbarIcons)
            icon.Refresh();
    }

	[Flags]
    public enum TooltipFlags
    {
		None = 0,
		Terminal = 1,
		Inventory = 2,
		Cockpit = 4,
    }
}
