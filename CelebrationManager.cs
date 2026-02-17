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
        var operation = GetOperation();
        var list = LoadPendingCelebrations();

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

    private string GetOperation()
    {
        if (TryGetStringArg(new[] { "operation", "op", "mode", "action" }, out string operation))
            return operation.Trim().ToLowerInvariant();

        return "alert";
    }

    private List<Dictionary<string, string>> LoadPendingCelebrations()
    {
        string json = CPH.GetGlobalVar<string>("pendingCelebrations", true) ?? "[]";
        var list = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json);

        return list ?? new List<Dictionary<string, string>>();
    }

    private bool HandleAlert(List<Dictionary<string, string>> list)
    {
        if (!TryGetStringArg(new[] { "eventId", "id", "celebrationId" }, out string id))
            return false;

        var item = list.FirstOrDefault(x => x.ContainsKey("Id") && x["Id"] == id);
        if (item == null)
            return true;

        RestoreSavedArgs(item);

        var eventType = ResolveEventType(item);
        var actionName = ResolveConfiguredAction(eventType);

        if (string.IsNullOrWhiteSpace(actionName))
        {
            var safeUser = item.ContainsKey("User") ? item["User"] : "Unknown User";
            var safeDetail = item.ContainsKey("Detail") ? item["Detail"] : "Alert Received";

            CPH.SetArgument("eventId", id);
            CPH.SetArgument("eventType", eventType);
            CPH.SetArgument("user", safeUser);
            CPH.SetArgument("detail", safeDetail);
            CPH.TriggerEvent("CelebrationConfigNeeded", true);

            CPH.LogWarn($"No action configured for {eventType}. Waiting for user configuration.");
            return false;
        }

        CPH.RunAction(actionName);

        list.Remove(item);
        CPH.SetGlobalVar("pendingCelebrations", JsonConvert.SerializeObject(list), true);
        return true;
    }

    private void RestoreSavedArgs(Dictionary<string, string> item)
    {
        if (!item.ContainsKey("RawArgs"))
            return;

        var savedArgs = JsonConvert.DeserializeObject<Dictionary<string, object>>(item["RawArgs"]);
        if (savedArgs == null)
            return;

        foreach (var arg in savedArgs)
        {
            CPH.SetArgument(arg.Key, arg.Value?.ToString());
        }
    }

    private string ResolveEventType(Dictionary<string, string> item)
    {
        var type = item.ContainsKey("Type") ? item["Type"] : "Sub";
        var subType = item.ContainsKey("SubType") ? item["SubType"] : "NewSub";

        if (type == "Sub")
        {
            if (subType == "Resub")
                return "Resub";

            if (subType == "GiftSub")
                return "GiftSub";

            return "NewSub";
        }

        if (type == "SubBomb")
            return "SubBomb";

        if (type == "Cheer")
            return "Cheer";

        return type;
    }

    private string ResolveConfiguredAction(string eventType)
    {
        var varNamesByType = new Dictionary<string, string[]>
        {
            { "NewSub", new[] { "newSubAction", "newsubAction", "NewSubAction" } },
            { "Resub", new[] { "resubAction", "ReSubAction" } },
            { "GiftSub", new[] { "giftSubAction", "giftsubAction", "GiftSubAction" } },
            { "SubBomb", new[] { "subBombAction", "subbombAction", "SubBombAction" } },
            { "Cheer", new[] { "cheerAction", "CheerAction" } }
        };

        if (!varNamesByType.ContainsKey(eventType))
            return string.Empty;

        foreach (var varName in varNamesByType[eventType])
        {
            var configured = CPH.GetGlobalVar<string>(varName, true);
            if (!string.IsNullOrWhiteSpace(configured))
                return configured;
        }

        return string.Empty;
    }

    private bool HandleSkip(List<Dictionary<string, string>> list)
    {
        if (!TryGetStringArg(new[] { "eventId", "id", "celebrationId" }, out string id))
            return false;

        var item = list.FirstOrDefault(x => x.ContainsKey("Id") && x["Id"] == id);
        if (item != null)
        {
            list.Remove(item);
            CPH.SetGlobalVar("pendingCelebrations", JsonConvert.SerializeObject(list), true);

            var safeUser = item.ContainsKey("User") ? item["User"] : "Unknown User";
            CPH.LogInfo($"Skipped celebration for {safeUser}");
        }

        return true;
    }

    private bool HandleSort(List<Dictionary<string, string>> list)
    {
        if (TryGetStringArg(new[] { "draggedId", "dragId", "sourceId" }, out string draggedId) &&
            TryGetStringArg(new[] { "targetId", "dropTargetId", "destinationId" }, out string targetId))
        {
            var dragged = list.FirstOrDefault(x => x.ContainsKey("Id") && x["Id"] == draggedId);
            var target = list.FirstOrDefault(x => x.ContainsKey("Id") && x["Id"] == targetId);

            if (dragged == null || target == null)
                return false;

            list.Remove(dragged);
            int targetIndex = list.IndexOf(target);

            bool insertAfter = CPH.TryGetArg("insertAfter", out bool after) && after;

            if (insertAfter)
                list.Insert(targetIndex + 1, dragged);
            else
                list.Insert(targetIndex, dragged);
        }
        else
        {
            list = list
                .OrderBy(x => ParseCreatedAt(x))
                .ToList();
        }

        CPH.SetGlobalVar("pendingCelebrations", JsonConvert.SerializeObject(list), true);
        return true;
    }

    private DateTime ParseCreatedAt(Dictionary<string, string> item)
    {
        if (item.ContainsKey("CreatedAt") && DateTime.TryParse(item["CreatedAt"], out DateTime createdAt))
            return createdAt;

        return DateTime.MinValue;
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
