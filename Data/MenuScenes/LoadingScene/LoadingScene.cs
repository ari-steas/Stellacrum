using Godot;
using System;

/// <summary>
/// World loading screen. Starts world load if visible and WorldLoader.stage == LoadingStage.Starting
/// </summary>
public partial class LoadingScene : CanvasLayer
{
	Label text;
    menus menus;
	
	public override void _Ready()
	{
		text = GetChild<Label>(2);
        menus = GetParent<menus>();
	}

    string indicator = "";
    int i = 0;
	public override void _PhysicsProcess(double delta)
	{
        if (!Visible)
            return;

        text.Text = WorldLoader.stage switch
        {
            LoadingStage.Started => "Loading started",
            LoadingStage.ModelLoad => "Loading models",
            LoadingStage.BlockLoad => "Loading blocks",
            LoadingStage.DataLoad => "Loading save data",
            LoadingStage.ObjectSpawn => "Loading save objects",
            LoadingStage.Done => "Loading finished",
            _ => "",
        } + indicator;

        if (i == 0)
            indicator = "";
        if (i == 30)
            indicator = ".";
        if (i == 60)
            indicator = "..";
        if (i == 90)
            indicator = "...";
        if (i == 120)
            i = 0;

        if (WorldLoader.stage == LoadingStage.Done)
            menus._SwitchMenu(0);

        i++;
    }
}
