using HarmonyLib;
using System.Reflection;

namespace NeuralSuperchargerOwnership;

#pragma warning disable CA1812
internal sealed class NeuralSuperchargerOwnershipMod : Mod
{
#pragma warning disable CS8618 // Set by constructor
    internal static NeuralSuperchargerOwnershipMod instance;
#pragma warning restore CS8618

    public NeuralSuperchargerOwnershipMod(ModContentPack content) : base(content)
    {
        instance = this;

        //Harmony.DEBUG = true;
        new Harmony(content.Name).PatchAll(Assembly.GetExecutingAssembly());
        //Harmony.DEBUG = false;
        //}

        //GetSettings<Settings>();
    }

    // public override void DoSettingsWindowContents(Rect inRect)
    // {
    //     base.DoSettingsWindowContents(inRect);
    //     Settings.DoSettingsWindowContents(inRect);
    // }

    // public override string SettingsCategory()
    // {
    //     return Content.Name;
    // }

    public static void Message(string msg)
    {
        Log.Message("[Neural Supercharger Ownership] " + msg);
    }

    public static void Dev(string msg)
    {
        if (Prefs.DevMode /*&& Settings._printDevMessages*/)
        {
            Log.Message("[Neural Supercharger Ownership][DEV] " + msg);
        }
    }

    public static void Dev(Func<string> produceMsg)
    {
        if (Prefs.DevMode /*&& Settings._printDevMessages*/)
        {
            Log.Message("[Neural Supercharger Ownership][DEV] " + produceMsg());
        }
    }

    public static void Warning(string msg)
    {
        Log.Warning("[Neural Supercharger Ownership] " + msg);
    }

    public static void Error(string msg)
    {
        Log.Error("[Neural Supercharger Ownership] " + msg);
    }

    public static void Exception(string msg, Exception? e = null)
    {
        Message(msg);
        if (e != null)
        {
            Log.Error(e.ToString());
        }
    }
}
#pragma warning restore CA1812

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class HotSwappableAttribute : Attribute
{
}
