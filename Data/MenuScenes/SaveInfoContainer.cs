using Godot;
using System;

public partial class SaveInfoContainer : VBoxContainer
{
	private Label _dateModifiedLabel, _dateCreatedLabel, _sizeLabel;
	public LineEdit _nameLabel, _descriptionLabel;

	private TextureRect _thumbRect;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Node IC = GetChild(2);
		_nameLabel = GetChild<LineEdit>(1);
		_thumbRect = GetChild<TextureRect>(3);

		_dateModifiedLabel = IC.GetChild<Label>(0);
		_dateCreatedLabel = IC.GetChild<Label>(1);
		_sizeLabel = IC.GetChild<Label>(2);
		_descriptionLabel = IC.GetChild<LineEdit>(3);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _ShowWorld()
	{
		WorldSave cs = WorldLoader.CurrentSave;
		GD.Print("Displaying world " + cs.Name + "...");
		_nameLabel.Text = cs.Name;
		_nameLabel.Editable = true;
        _descriptionLabel.Editable = true;

        _dateModifiedLabel.Text = "MODIFIED ON: " + cs.ModifiedDate.ToLocalTime();
		_dateCreatedLabel.Text = "CREATED ON: " + cs.CreationDate.ToLocalTime();
		_sizeLabel.Text = "SIZE: " + cs.Size + "kb";
		_descriptionLabel.Text = cs.Description;
		_thumbRect.Texture = cs.Thumbnail;
	}
}
