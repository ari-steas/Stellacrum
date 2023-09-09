using System;
using System.Collections.Generic;
using Godot;

public static class OptionsHelper
{
    public readonly static Dictionary<string, Option> Options = new()
    {
        ["fullscreen"] =        new("Fullscreen", false),
        ["mousesensitivityx"] = new("Mouse Sensitivity [X]", 1f),
        ["mousesensitivityy"] = new("Mouse Sensitivity [Y]", 1f),
        ["mouseinvertx"] =      new("Mouse Invert [X]", false),
        ["mouseinverty"] =      new("Mouse Invert [Y]", false),
    };

    public static void SetOption(string option, object value)
    {
        Options[option].Value = value;
        Options[option].Action.Invoke(value);
    }

    #nullable enable
    public static object? GetOption(string option)
    {
        return Options[option];
    }

    public static void AddOption(string option, Option value)
    {
        Options[option] = value;
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
        Action = action;
        FriendlyName = friendlyName;
    }
}