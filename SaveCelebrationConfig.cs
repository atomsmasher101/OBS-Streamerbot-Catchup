/*
Streamerbot Action: SaveCelebrationConfig
*/
using System;
using System.Collections.Generic;

public class CPHInline
{
    public bool Execute()
    {
        CPH.LogInfo("=== SaveCelebrationConfig START ===");

        CPH.LogInfo($"Total arguments received: {args.Count}");
        foreach (var arg in args)
        {
            CPH.LogInfo($"  Arg: {arg.Key} = {arg.Value} (type: {arg.Value?.GetType().Name ?? "null"})");
        }

        string operation = "celebration";
        if (TryGetStringArg(new[] { "operation", "op", "mode" }, out string opArg))
        {
            operation = opArg.Trim().ToLowerInvariant();
        }

        bool success = operation == "visuals"
            ? SaveVisualSettings()
            : SaveCelebrationAction();

        CPH.LogInfo("=== SaveCelebrationConfig END ===");
        return success;
    }

    private bool SaveCelebrationAction()
    {
        var varName = ResolveVarName();
        if (string.IsNullOrWhiteSpace(varName))
        {
            CPH.LogError("SaveCelebrationConfig: Missing varName/eventType argument");
            return false;
        }

        var celebrationAction = ResolveCelebrationAction();
        if (celebrationAction == null)
        {
            CPH.LogError("SaveCelebrationConfig: Missing celebration action argument");
            return false;
        }

        CPH.LogInfo($"Resolved varName: '{varName}'");
        CPH.LogInfo($"Resolved celebrationAction: '{celebrationAction}'");

        var existingValue = CPH.GetGlobalVar<string>(varName, true);
        if (string.IsNullOrWhiteSpace(existingValue))
            CPH.LogInfo($"Creating new global variable: {varName}");
        else
            CPH.LogInfo($"Updating existing global variable: {varName} (was: {existingValue})");

        CPH.SetGlobalVar(varName, celebrationAction, true);

        var savedValue = CPH.GetGlobalVar<string>(varName, true);
        if (savedValue == celebrationAction)
        {
            CPH.LogInfo($"✓ Successfully saved celebration config: {varName} = {celebrationAction}");
            return true;
        }

        CPH.LogError($"✗ Failed to save {varName}. Expected: {celebrationAction}, Got: {savedValue}");
        return false;
    }

    private string ResolveVarName()
    {
        if (TryGetStringArg(new[] { "varName", "globalVarName", "settingName" }, out string varName))
            return varName;

        if (!TryGetStringArg(new[] { "eventType", "type", "celebrationType" }, out string eventType))
            return string.Empty;

        var eventTypeToVarName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "NewSub", "newSubAction" },
            { "Resub", "resubAction" },
            { "GiftSub", "giftSubAction" },
            { "SubBomb", "subBombAction" },
            { "Cheer", "cheerAction" }
        };

        return eventTypeToVarName.ContainsKey(eventType)
            ? eventTypeToVarName[eventType]
            : string.Empty;
    }

    private string ResolveCelebrationAction()
    {
        if (TryGetStringArg(new[] { "celebrationAction", "selectedAction", "configuredAction", "targetAction" }, out string action))
            return action;

        // Legacy fallback for clients that only send `actionName`.
        if (CPH.TryGetArg("actionName", out string actionName) && !string.Equals(actionName, "SaveCelebrationConfig", StringComparison.OrdinalIgnoreCase))
            return actionName;

        return null;
    }

    private bool SaveVisualSettings()
    {
        var defaultTheme = "neon";
        var defaultTextSize = "normal";
        var defaultDensity = "comfortable";

        var mode = "save";
        if (TryGetStringArg(new[] { "mode", "saveMode" }, out string requestedMode))
            mode = requestedMode.Trim().ToLowerInvariant();

        var theme = defaultTheme;
        var textSize = defaultTextSize;
        var density = defaultDensity;

        if (TryGetStringArg(new[] { "theme", "visualTheme" }, out string themeArg))
            theme = themeArg;

        if (TryGetStringArg(new[] { "textSize", "fontSize", "visualTextSize" }, out string textSizeArg))
            textSize = textSizeArg;

        if (TryGetStringArg(new[] { "density", "spacing", "visualDensity" }, out string densityArg))
            density = densityArg;

        var existingTheme = CPH.GetGlobalVar<string>("catchupVisualTheme", true);
        var existingTextSize = CPH.GetGlobalVar<string>("catchupVisualTextSize", true);
        var existingDensity = CPH.GetGlobalVar<string>("catchupVisualDensity", true);

        if (mode == "ensure")
        {
            if (string.IsNullOrWhiteSpace(existingTheme))
            {
                CPH.SetGlobalVar("catchupVisualTheme", theme, true);
                CPH.LogInfo($"Created catchupVisualTheme with default value '{theme}'");
            }

            if (string.IsNullOrWhiteSpace(existingTextSize))
            {
                CPH.SetGlobalVar("catchupVisualTextSize", textSize, true);
                CPH.LogInfo($"Created catchupVisualTextSize with default value '{textSize}'");
            }

            if (string.IsNullOrWhiteSpace(existingDensity))
            {
                CPH.SetGlobalVar("catchupVisualDensity", density, true);
                CPH.LogInfo($"Created catchupVisualDensity with default value '{density}'");
            }

            return true;
        }

        CPH.SetGlobalVar("catchupVisualTheme", theme, true);
        CPH.SetGlobalVar("catchupVisualTextSize", textSize, true);
        CPH.SetGlobalVar("catchupVisualDensity", density, true);

        CPH.LogInfo($"Saved visual settings: theme={theme}, textSize={textSize}, density={density}");
        return true;
    }

    private bool TryGetStringArg(IEnumerable<string> argNames, out string value)
    {
        foreach (var name in argNames)
        {
            if (CPH.TryGetArg(name, out string found) && !string.IsNullOrWhiteSpace(found))
            {
                value = found;
                return true;
            }
        }

        value = string.Empty;
        return false;
    }
}
