/*
Streamerbot Action: SaveCelebrationConfig
*/
using System;

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
        if (CPH.TryGetArg("operation", out string opArg) && !string.IsNullOrWhiteSpace(opArg))
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
        if (!CPH.TryGetArg("varName", out string varName) || string.IsNullOrWhiteSpace(varName))
        {
            CPH.LogError("SaveCelebrationConfig: Missing varName argument");
            return false;
        }

        // NOTE: We use 'celebrationAction' instead of 'actionName' because
        // Streamerbot automatically adds 'actionName' with the calling action's name
        if (!CPH.TryGetArg("celebrationAction", out string celebrationAction))
        {
            CPH.LogError("SaveCelebrationConfig: Missing celebrationAction argument");
            return false;
        }

        CPH.LogInfo($"Received varName: '{varName}'");
        CPH.LogInfo($"Received celebrationAction: '{celebrationAction}'");

        var existingValue = CPH.GetGlobalVar<string>(varName, true);
        if (string.IsNullOrWhiteSpace(existingValue))
        {
            CPH.LogInfo($"Creating new global variable: {varName}");
        }
        else
        {
            CPH.LogInfo($"Updating existing global variable: {varName} (was: {existingValue})");
        }

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

    private bool SaveVisualSettings()
    {
        var defaultTheme = "neon";
        var defaultTextSize = "normal";
        var defaultDensity = "comfortable";

        var mode = "save";
        if (CPH.TryGetArg("mode", out string requestedMode) && !string.IsNullOrWhiteSpace(requestedMode))
        {
            mode = requestedMode.Trim().ToLowerInvariant();
        }

        var theme = defaultTheme;
        var textSize = defaultTextSize;
        var density = defaultDensity;

        if (CPH.TryGetArg("theme", out string themeArg) && !string.IsNullOrWhiteSpace(themeArg))
            theme = themeArg;

        if (CPH.TryGetArg("textSize", out string textSizeArg) && !string.IsNullOrWhiteSpace(textSizeArg))
            textSize = textSizeArg;

        if (CPH.TryGetArg("density", out string densityArg) && !string.IsNullOrWhiteSpace(densityArg))
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
}
