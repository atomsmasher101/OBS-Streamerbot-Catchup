/*
*Streamerbot Action: CelebrationManager
*/
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

public class CPHInline
{
    public bool Execute()
    {
        // Determine which operation to perform based on arguments
        string operation = "alert"; // default
        
        if (CPH.TryGetArg("operation", out string op))
        {
            operation = op.ToLower();
        }

        string json = CPH.GetGlobalVar<string>("pendingCelebrations", true) ?? "[]";
        var list = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json);

        switch (operation)
        {
            case "alert":
                return HandleAlert(list);
            
            case "skip":
                return HandleSkip(list);
            
            case "sort":
                return HandleSort(list);
            
            default:
                CPH.LogError($"Unknown operation: {operation}");
                return false;
        }
    }

    private bool HandleAlert(List<Dictionary<string, string>> list)
    {
        if (!CPH.TryGetArg("eventId", out string id)) return false;

        var item = list.FirstOrDefault(x => x.ContainsKey("Id") && x["Id"] == id);
        if (item != null)
        {
            // Restore arguments safely
            if (item.ContainsKey("RawArgs"))
            {
                var savedArgs = JsonConvert.DeserializeObject<Dictionary<string, object>>(item["RawArgs"]);
                foreach (var arg in savedArgs)
                {
                    CPH.SetArgument(arg.Key, arg.Value?.ToString());
                }
            }

            string actionName = "";
            string eventType = "";
            string type = item["Type"];

            // Determine which action to run
            if (type == "Sub")
            {
                if (item.ContainsKey("SubType"))
                {
                    string subType = item["SubType"];
                    
                    if (subType == "NewSub")
                    {
                        eventType = "NewSub";
                        actionName = CPH.GetGlobalVar<string>("newSubAction", true) ?? "";
                    }
                    else if (subType == "Resub")
                    {
                        eventType = "Resub";
                        actionName = CPH.GetGlobalVar<string>("resubAction", true) ?? "";
                    }
                    else if (subType == "GiftSub")
                    {
                        eventType = "GiftSub";
                        actionName = CPH.GetGlobalVar<string>("giftSubAction", true) ?? "";
                    }
                }
                else
                {
                    eventType = "NewSub";
                    actionName = CPH.GetGlobalVar<string>("newSubAction", true) ?? "";
                }
            }
            else if (type == "SubBomb")
            {
                eventType = "SubBomb";
                actionName = CPH.GetGlobalVar<string>("subBombAction", true) ?? "";
            }
            else if (type == "Cheer")
            {
                eventType = "Cheer";
                actionName = CPH.GetGlobalVar<string>("cheerAction", true) ?? "";
            }

            // Check if action is configured
            if (string.IsNullOrEmpty(actionName))
            {
                CPH.SetArgument("eventId", id);
                CPH.SetArgument("eventType", eventType);
                CPH.SetArgument("user", item["User"]);
                CPH.SetArgument("detail", item["Detail"]);
                CPH.TriggerEvent("CelebrationConfigNeeded", true);
                
                CPH.LogWarn($"No action configured for {eventType}. Waiting for user configuration.");
                return false;
            }

            if (!string.IsNullOrEmpty(actionName))
            {
                CPH.RunAction(actionName);
            }

            // Remove and Update
            list.Remove(item);
            CPH.SetGlobalVar("pendingCelebrations", JsonConvert.SerializeObject(list), true);
        }
        return true;
    }

    private bool HandleSkip(List<Dictionary<string, string>> list)
    {
        if (!CPH.TryGetArg("eventId", out string id))
            return false;

        var item = list.FirstOrDefault(x => x.ContainsKey("Id") && x["Id"] == id);
        if (item != null)
        {
            list.Remove(item);
            CPH.SetGlobalVar("pendingCelebrations", JsonConvert.SerializeObject(list), true);
            CPH.LogInfo($"Skipped celebration for {item["User"]}");
        }

        return true;
    }

    private bool HandleSort(List<Dictionary<string, string>> list)
    {
        // Manual Reorder Mode (Drag & Drop)
        if (CPH.TryGetArg("draggedId", out string draggedId) &&
            CPH.TryGetArg("targetId", out string targetId))
        {
            var dragged = list.FirstOrDefault(x => x["Id"] == draggedId);
            var target = list.FirstOrDefault(x => x["Id"] == targetId);

            if (dragged == null || target == null)
                return false;

            list.Remove(dragged);
            int targetIndex = list.IndexOf(target);
            
            bool insertAfter = CPH.TryGetArg("insertAfter", out bool after) && after;
            
            if (insertAfter)
            {
                list.Insert(targetIndex + 1, dragged);
            }
            else
            {
                list.Insert(targetIndex, dragged);
            }
        }
        // Chronological Sort Mode
        else
        {
            list = list
                .OrderBy(x => DateTime.Parse(x["CreatedAt"]))
                .ToList();
        }

        CPH.SetGlobalVar("pendingCelebrations", JsonConvert.SerializeObject(list), true);
        return true;
    }
}
