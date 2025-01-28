using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace UnarmedGearScore;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    Harmony _harmony;
    internal static Plugin _instance;

    public override void Load()
    {
        _instance = this;

        // Plugin startup logic
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");

        // Harmony patching
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
    }

    public override bool Unload()
    {
        _harmony?.UnpatchSelf();
        return true;
    }

}
