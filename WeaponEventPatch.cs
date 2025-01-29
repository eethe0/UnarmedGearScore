using System.Collections.Generic;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Systems;
using Unity.Collections;
using Unity.Entities;

namespace UnarmedGearScore;

[HarmonyPatch(typeof(WeaponLevelSystem_Spawn), nameof(WeaponLevelSystem_Spawn.OnUpdate))]
static class WeaponEventPatch
{
    static Dictionary<Entity, float> characterWeaponLevels = [];

    static void Postfix(WeaponLevelSystem_Spawn __instance)
    {
        var em = __instance.EntityManager;

        var entities = __instance.__query_1111682356_0.ToEntityArray(Allocator.Temp);

        try
        {
            // Check each player's gear score and update it
            foreach (var entity in entities)
            {
                if (!em.HasComponent<EntityOwner>(entity)) continue;
                var characterEntity = em.GetComponentData<EntityOwner>(entity).Owner;

                if (!em.Exists(characterEntity)) continue;
                if (!em.HasComponent<Equipment>(characterEntity)) continue;
                var equipment = em.GetComponentData<Equipment>(characterEntity);
                var weaponLevel = equipment.WeaponLevel;

                if (weaponLevel == 0f)
                {
                    // Player is unarmed
                    // Set their weapon level to the previously recorded weapon level
                    var previousWeaponLevel = characterWeaponLevels.GetValueOrDefault(characterEntity, 0f);

                    equipment.WeaponLevel._Value = previousWeaponLevel;

                    em.SetComponentData(characterEntity, equipment);
                }
                else
                {
                    // Player is armed
                    // Save their current weapon level
                    characterWeaponLevels[characterEntity] = weaponLevel;
                }
            }
        }
        catch (System.Exception e)
        {
            Plugin._instance.Log.LogError($"Error in UnarmedGearScore: {e}");
        }
        finally
        {
            entities.Dispose();
        }
    }
}