using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class DraggableItem : TextureRect
{
    bool BeingDragged = false;
    Vector2 basePosition = Vector2.Zero;
    public Action<DraggableItem, Vector2> OnRelease;

    public override void _Ready()
    {
        base._Ready();
        GuiInput += OnInputEvent;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (basePosition == Vector2.Zero)
            basePosition = GlobalPosition;

        if (BeingDragged)
        {
            if (Input.IsActionPressed("MousePressL"))
            {
                GlobalPosition = GetGlobalMousePosition() - Size/2f;
            }
            else
            {
                OnRelease(this, GetGlobalMousePosition());
                BeingDragged = false;
                GlobalPosition = basePosition;
            }
        }
    }

    void OnInputEvent(InputEvent e)
    {
        if (e is InputEventMouse em)
            if (em.ButtonMask == MouseButtonMask.Left)
                BeingDragged = true;
    }
}
