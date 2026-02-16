/*
*Streamerbot Action: StoreEvent
*/
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

public class CPHInline
{
    public bool Execute()
    {
        string user = "Unknown User";
        if (CPH.TryGetArg("user", out string u1))
            user = u1;
        else if (CPH.TryGetArg("userName", out string u2))
            user = u2;

        string detail = "Alert Received";
        string type = "Event";
        string subType = null; // Track sub vs resub

        if (CPH.TryGetArg("bits", out object b1) || CPH.TryGetArg("bitsAmount", out b1))
        {
            detail = $"{b1} Bits";
            type = "Cheer";
        }
        else if (CPH.TryGetArg("totalGifts", out int bombCount))
        {
            detail = $"Gift Bomb ({bombCount} Subs)";
            type = "SubBomb";
        }
        else if (CPH.TryGetArg("eventDetail", out string customDetail))
        {
            detail = customDetail;
            type = "Sub";
            subType = "GiftSub"; // Gifted subs
        }
        else if (CPH.TryGetArg("tier", out string tierName))
        {
            // Check if this is a resub (has cumulative months)
            if (CPH.TryGetArg("cumulative", out int cumulativeMonths))
            {
                detail = $"{tierName} Resub ({cumulativeMonths} months)";
                type = "Sub";
                subType = "Resub";
            }
            else
            {
                detail = $"{tierName} Sub";
                type = "Sub";
                subType = "NewSub";
            }
        }

        var safeArgs = new Dictionary<string, object>();
        foreach (var arg in args)
        {
            if (arg.Value is string || arg.Value is int || arg.Value is long || arg.Value is bool || arg.Value is double)
                safeArgs[arg.Key] = arg.Value;
        }

        string json = CPH.GetGlobalVar<string>("pendingCelebrations", true) ?? "[]";
        var events = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json);

        var eventData = new Dictionary<string, string>
        {
            { "Id", Guid.NewGuid().ToString() },
            { "User", user },
            { "Type", type },
            { "Detail", detail },
            { "Timestamp", DateTime.Now.ToString("h:mm tt") }, // display only
            { "CreatedAt", DateTime.UtcNow.ToString("o") },    // true sortable timestamp
            { "RawArgs", JsonConvert.SerializeObject(safeArgs) }
        };

        // Add SubType if it exists
        if (!string.IsNullOrEmpty(subType))
        {
            eventData["SubType"] = subType;
        }

        events.Add(eventData);

        CPH.SetGlobalVar("pendingCelebrations", JsonConvert.SerializeObject(events), true);
        return true;
    }
}
