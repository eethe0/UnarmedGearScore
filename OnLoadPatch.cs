using HarmonyLib;
using Unity.Scenes;

namespace UnarmedGearScore;

[HarmonyPatch]
internal static class OnLoadPatch
{
    [HarmonyPatch(typeof(SceneSystem), nameof(SceneSystem.ShutdownStreamingSupport))]
    [HarmonyPostfix]
    static void ShutdownStreamingSupportPostfix()
    {
        try
        {
            _ = new GearScoreManager();
            Plugin._harmony.Unpatch(typeof(SceneSystem).GetMethod("ShutdownStreamingSupport"), typeof(OnLoadPatch).GetMethod("ShutdownStreamingSupportPostfix"));
        }
        catch
        {
            Plugin._log.LogError("Failed to initialize UnarmedGearScore");
        }
    }
}