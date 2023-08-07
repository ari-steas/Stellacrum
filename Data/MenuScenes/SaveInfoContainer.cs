using Godot;
using System;

public partial class SaveInfoContainer : VBoxContainer
{
	private Label _nameLabel, _dateModifiedLabel, _dateCreatedLabel, _sizeLabel, _descriptionLabel;
	private TextureRect _thumbRect;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Node IC = GetChild(2);
		_nameLabel = GetChild<Label>(1);
		_thumbRect = GetChild<TextureRect>(3);

		_dateModifiedLabel = IC.GetChild<Label>(0);
		_dateCreatedLabel = IC.GetChild<Label>(1);
		_sizeLabel = IC.GetChild<Label>(2);
		_descriptionLabel = IC.GetChild<Label>(3);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _ShowWorld()
	{
		WorldSave cs = WorldLoader.CurrentSave;
		GD.Print("Displaying world " + cs.Name + "...");
		_nameLabel.Text = "NAME: " + cs.Name;

		_dateModifiedLabel.Text = "MODIFIED ON: " + cs.ModifiedDate.ToLocalTime();
		_dateCreatedLabel.Text = "CREATED ON: " + cs.CreationDate.ToLocalTime();
		_sizeLabel.Text = "SIZE: " + cs.Size + "kb";
		_descriptionLabel.Text = "DESCRIPTION: " + cs.Description;
		_thumbRect.Texture = cs.Thumbnail;
	}
}
