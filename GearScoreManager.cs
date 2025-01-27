using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using ProjectM;
using ProjectM.Network;
using ProjectM.Physics;
using UnarmedGearScore;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

class GearScoreManager
{
    World world;
    MonoBehaviour monoBehaviour;
    EntityQuery userQuery;
    Dictionary<ulong, float> playerWeaponLevels = [];

    public GearScoreManager()
    {
        world = World.s_AllWorlds.ToArray().FirstOrDefault(world => world.Name == "Server");

        userQuery = world.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<User>());

        StartCoroutine(Update());
    }

    void StartCoroutine(IEnumerator routine)
    {
        if (monoBehaviour == null)
        {
            monoBehaviour = new GameObject("UnarmedGearScore").AddComponent<IgnorePhysicsDebugSystem>();
            UnityEngine.Object.DontDestroyOnLoad(monoBehaviour.gameObject);
        }
        monoBehaviour.StartCoroutine(routine.WrapToIl2Cpp());
    }

    IEnumerator Update()
    {
        while (true)
        {
            try
            {
                var userEntities = userQuery.ToEntityArray(Allocator.Temp);

                // Check each player's gear score and update it
                foreach (var entity in userEntities)
                {
                    var user = world.EntityManager.GetComponentData<User>(entity);
                    if (!user.IsConnected) continue;
                    var characterEntity = user.LocalCharacter.GetEntityOnServer();
                    var equipment = world.EntityManager.GetComponentData<Equipment>(characterEntity);
                    var weaponLevel = equipment.WeaponLevel;

                    if (weaponLevel == 0f)
                    {
                        // Player is unarmed
                        // Set their weapon level to the previously recorded weapon level
                        var previousWeaponLevel = playerWeaponLevels.GetValueOrDefault(user.PlatformId, 0f);

                        equipment.WeaponLevel._Value = previousWeaponLevel;

                        world.EntityManager.SetComponentData(characterEntity, equipment);
                    }
                    else
                    {
                        // Player is armed
                        // Save their current weapon level
                        playerWeaponLevels[user.PlatformId] = weaponLevel;
                    }
                }
            }
            catch (System.Exception e)
            {
                Plugin._log.LogError($"Error in GearScoreManager: {e}");
            }

            yield return new WaitForSeconds(0.2f);
        }
    }
}