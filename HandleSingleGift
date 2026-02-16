/*
Streamerbot Action: HandleSingleGift
*/
using System;

public class CPHInline
{
    public bool Execute()
    {
        // 1. Check if this is part of a bomb to avoid duplicate entries
        if (CPH.TryGetArg("isGiftBomb", out bool isPartByBomb) && isPartByBomb)
        {
            return false;
        }

        // 2. Capture the giver and the recipient
        // In a gift event, 'user' is the giver, 'recipientUser' is the receiver
        if (CPH.TryGetArg("recipientUser", out string recipient))
        {
            // We set a custom argument that StoreEvent will look for
            // We format the detail here so it shows up nicely on the card
            CPH.SetArgument("eventDetail", "Gifted to " + recipient);
        }

        // 3. Trigger the storage logic
        CPH.RunAction("StoreEvent");
        return true;
    }
}
