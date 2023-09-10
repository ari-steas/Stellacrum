using System;
using System.Collections.Generic;
using System.Net;
using Godot;
using Godot.NativeInterop;

public static class OptionsHelper
{
	public readonly static Dictionary<string, Option> Options = new()
	{
		["fullscreen"] =        new("Fullscreen", false),
		["mousesensitivityx"] = new("Mouse Sensitivity [X]", 1.0f),
		["mousesensitivityy"] = new("Mouse Sensitivity [Y]", 1.0f),
		["mouseinvertx"] =      new("Mouse Invert [X]", false),
		["mouseinverty"] =      new("Mouse Invert [Y]", false),
	};

	public static void SetOption(string option, object value)
	{
		if (value == null || value.GetType() != Options[option].Value.GetType())
		{
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

	public Option(string friendlyName, object value, Action<object>? action = null)
	{
		Value = value;
		Action = action ?? EmptyAction;
		FriendlyName = friendlyName;
	}

	static void EmptyAction(object o)
	{

	}
}
