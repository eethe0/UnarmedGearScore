using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Systems;
using Unity.Collections;
using Unity.Entities;

namespace UnarmedGearScore;

[HarmonyPatch(typeof(WeaponLevelSystem_Spawn), nameof(WeaponLevelSystem_Spawn.OnUpdate))]
static class WeaponEventPatch
{
    static World world = World.s_AllWorlds.ToArray().FirstOrDefault(world => world.Name == "Server");
    static Dictionary<Entity, float> characterWeaponLevels = [];

    static void Postfix(WeaponLevelSystem_Spawn __instance)
    {
        var entities = __instance.__query_1111682356_0.ToEntityArray(Allocator.Temp);

        try
        {
            // Check each player's gear score and update it
            foreach (var entity in entities)
            {
                if (!world.EntityManager.HasComponent<EntityOwner>(entity)) continue;
                var characterEntity = world.EntityManager.GetComponentData<EntityOwner>(entity).Owner;

                if (!world.EntityManager.Exists(characterEntity)) continue;
                if (!world.EntityManager.HasComponent<Equipment>(characterEntity)) continue;
                var equipment = world.EntityManager.GetComponentData<Equipment>(characterEntity);
                var weaponLevel = equipment.WeaponLevel;

                if (weaponLevel == 0f)
                {
                    // Player is unarmed
                    // Set their weapon level to the previously recorded weapon level
                    var previousWeaponLevel = characterWeaponLevels.GetValueOrDefault(characterEntity, 0f);

                    equipment.WeaponLevel._Value = previousWeaponLevel;

                    world.EntityManager.SetComponentData(characterEntity, equipment);
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