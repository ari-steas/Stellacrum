using Godot;
using Stellacrum.Data.ObjectLoaders;

public partial class ToolbarObject : AspectRatioContainer
{
    [Export]
    public string SelectAction { get; set; } = "";

    private bool _selected = false;
    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            _selectedRect.Visible = _selected;
        }
    }

    private string _blockSubtype = "";
    public string BlockSubtype
    {
        get => _blockSubtype;
        set
        {
            if (_blockSubtype == value)
                return;
            _blockSubtype = value;
            _icon.Texture = _blockSubtype == "" ? TextureLoader.Get("EmptyToolbar.png") : CubeBlockLoader.GetTexture(_blockSubtype);

            if (_blockSubtype == "")
                Selected = false;
        }
    }

    private TextureRect _icon, _selectedRect;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        _icon = (TextureRect) FindChild("Icon", false);
        _selectedRect = (TextureRect) FindChild("Selected", false);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void SetBind(int number)
    {
        GetChild<Label>(2).Text = number + " ";
    }

    public void Refresh()
    {
        _icon.Texture = _blockSubtype == "" ? TextureLoader.Get("EmptyToolbar.png") : CubeBlockLoader.GetTexture(_blockSubtype);
    }
}
