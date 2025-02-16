using Godot;
using System;
using System.Collections.Generic;

public static class Debug
{
    private static uint logCount = 0;

    private static bool isPrint = true;

    private static object[] GetPrefixMessage(params object[] messages) 
    {
         string prefix = $"[{DateTime.Now.ToString("HH:mm:ss:fff")}][{logCount}]";

        List<object> tempMessages = new List<object>();
        tempMessages.Add(prefix);
        tempMessages.AddRange(messages);
        return tempMessages.ToArray();
    }

    public static void Print(params object[] messages) {
        if(isPrint)
        {
            object[] messagesWithPrefix = GetPrefixMessage(messages);
            AddLogCount();
            GD.Print(messagesWithPrefix);
        }
    }

    public static void PrintWarn(params object[] messages) 
    {
        if (isPrint)
        {
            object[] messagesWithPrefix = GetPrefixMessage(messages);
            AddLogCount();
            GD.PushWarning(messagesWithPrefix);
        }
    }

    public static void PrintErr(params object[] messages) {
        if(isPrint) 
        {
            object[] messagesWithPrefix = GetPrefixMessage(messages);
            AddLogCount();
            GD.PushError(messagesWithPrefix);
        }
    }

    public static void AddLogCount() 
    {
        if(logCount + 1 > 10000) 
        {
            logCount = 0;
        }
        else 
        {
            logCount++;
        }
    }
}
