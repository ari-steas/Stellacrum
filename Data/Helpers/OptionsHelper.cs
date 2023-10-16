using System;
using System.Collections.Generic;
using System.Net;
using Godot;
using Godot.NativeInterop;

public static class OptionsHelper
{
	public readonly static Dictionary<string, Option> Options = new()
	{
		["fullscreen"] =		new("Fullscreen", false),
		["fov"] =				new("Field of View", 90, null, new(30, 120)),
		["fps"] =				new("FPS Limit", 250, null, new(0, 1000)),
		["vsync"] =				new("Vertical Sync", false),
		["mousesensitivityx"] = new("Mouse Sensitivity [X]", 1.0f, null, new(0, 5)),
		["mousesensitivityy"] = new("Mouse Sensitivity [Y]", 1.0f, null, new(0, 5)),
		["mouseinvertx"] =      new("Mouse Invert [X]", false),
		["mouseinverty"] =      new("Mouse Invert [Y]", false),
	};

	public static void SetOption(string option, object value)
	{
		if (value == null)
			return;

		if (value.GetType() != Options[option].Value.GetType())
		{
            // gross way to fix mismatched input number types
            switch (Options[option].Value)
            {
                case int:
                    int v;
                    if (int.TryParse(value.ToString(), out v))
                        SetOption(option, v);
                    return;
                case float:
                    float f;
                    if (float.TryParse(value.ToString(), out f))
                        SetOption(option, f);
                    return;
                case double:
                    double d;
                    if (double.TryParse(value.ToString(), out d))
                        SetOption(option, d);
                    return;
            }

            GD.PrintErr("Invalid type for " + option);
			return;
		}

		if (!Options.ContainsKey(option))
		{
			GD.PrintErr("Invalid optionID for " + option);
			return;
		}

		Options[option].Value = value;
		Options[option].Action.Invoke(value);
	}

	#nullable enable
	public static object? GetOption(string option)
	{
		return Options[option].Value;
	}

	/// <summary>
	/// Add new option (for settings that need a specific action)
	/// </summary>
	/// <param name="option"></param>
	/// <param name="value"></param>
	public static void AddOption(string option, Option value)
	{
		Options[option] = value;
	}

	public static Godot.Collections.Dictionary<string, Variant> Save()
	{
		Godot.Collections.Dictionary<string, Variant> dict = new();

		foreach (var option in Options)
			dict.Add(option.Key, JsonHelper.ObjToVariant(option.Value.Value));
		
		return dict;
	}

	public static void Load(Godot.Collections.Dictionary<string, Variant> dict)
	{
		if (dict == null)
			return;

		foreach (var option in dict)
			SetOption(option.Key, JsonHelper.VariantToObj(option.Value));
	}
}

public class Option
{
	public object Value { get; set; }
	public Action<object>? Action { get; set; }
	public string FriendlyName { get; private set; }
	public Vector2 sliderRange { get; private set; }

	public Option(string friendlyName, object value, Action<object>? action = null, Vector2 sliderRange = new())
	{
		Value = value;
		Action = action ?? EmptyAction;
		FriendlyName = friendlyName;
		if (sliderRange.Equals(Vector2.Zero))
			this.sliderRange = new Vector2(0, 10);
		else
			this.sliderRange = sliderRange;
	}

	static void EmptyAction(object o)
	{

	}
}
