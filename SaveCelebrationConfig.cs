/*
Streamerbot Action: SaveCelebrationConfig
*/
using System;

public class CPHInline
{
    public bool Execute()
    {
        CPH.LogInfo("=== SaveCelebrationConfig START ===");
        
        // Log all arguments received
        CPH.LogInfo($"Total arguments received: {args.Count}");
        foreach (var arg in args)
        {
            CPH.LogInfo($"  Arg: {arg.Key} = {arg.Value} (type: {arg.Value?.GetType().Name ?? "null"})");
        }
        
        // Get the variable name and celebration action name from arguments
        // NOTE: We use 'celebrationAction' instead of 'actionName' because
        // Streamerbot automatically adds 'actionName' with the calling action's name
        if (!CPH.TryGetArg("varName", out string varName))
        {
            CPH.LogError("SaveCelebrationConfig: Missing varName argument");
            return false;
        }

        if (!CPH.TryGetArg("celebrationAction", out string celebrationAction))
        {
            CPH.LogError("SaveCelebrationConfig: Missing celebrationAction argument");
            return false;
        }

        CPH.LogInfo($"Received varName: '{varName}'");
        CPH.LogInfo($"Received celebrationAction: '{celebrationAction}'");

        // Check if the global variable exists
        var existingValue = CPH.GetGlobalVar<string>(varName, true);
        
        if (existingValue == null)
        {
            CPH.LogInfo($"Creating new global variable: {varName}");
        }
        else
        {
            CPH.LogInfo($"Updating existing global variable: {varName} (was: {existingValue})");
        }

        // Save/update the global variable (persisted)
        CPH.SetGlobalVar(varName, celebrationAction, true);
        
        // Verify it was saved
        var savedValue = CPH.GetGlobalVar<string>(varName, true);
        if (savedValue == celebrationAction)
        {
            CPH.LogInfo($"✓ Successfully saved celebration config: {varName} = {celebrationAction}");
        }
        else
        {
            CPH.LogError($"✗ Failed to save {varName}. Expected: {celebrationAction}, Got: {savedValue}");
            return false;
        }
        
        CPH.LogInfo("=== SaveCelebrationConfig END ===");
        return true;
    }
}
