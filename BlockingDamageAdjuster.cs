using System;
using System.IO;
using System.Reflection;
using BepInEx;
using Harmony;
using UnityEngine;

namespace BlockingDamageAdjuster
{
    [BepInPlugin("com.gnivler.BlockingDamageAdjuster", "BlockingDamageAdjuster", "1.0")]
    public class BlockingDamageAdjuster : BaseUnityPlugin
    {
        private static Settings modSettings = new Settings();

        public class Settings
        {
            public float axe1h = 0f;
            public float axe2h = 0f;
            public float sword1h = 0f;
            public float sword2h = 0f;
            public float mace1h = 0f;
            public float mace2h = 0f;
            public float halberd = 0f;
            public float shield = 0f;
        }

        public void Awake()
        {
            try
            {
                using (StreamReader reader = new StreamReader(
                    @"BepInEx\plugins\BlockingDamageAdjuster\BlockingDamageAdjuster.json"))
                {
                    var json = reader.ReadToEnd();
                    modSettings = JsonUtility.FromJson<Settings>(json);
                }
            }
            catch (Exception e)
            {
                //FileLog.Log(e.Message);
            }

            var harmony = HarmonyInstance.Create("com.gnivler.BlockingDamageAdjuster");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(Character), "ReceiveBlock", MethodType.Normal)]
        [HarmonyPatch(new[]
        {
            typeof(MonoBehaviour),
            typeof(float),
            typeof(Vector3),
            typeof(float),
            typeof(float),
            typeof(Character),
            typeof(float)
        })]
        public class ReceiveBlockPatch
        {
            public static void Prefix(Character __instance, Vector3 _hitDir, float _damage, Character _dealerChar)
            {
                var blockDamageModifier = 0f;
                if (__instance.ShieldEquipped)
                {
                    blockDamageModifier = modSettings.shield;
                }
                else
                {
                    switch (__instance.CurrentWeapon.Type)
                    {
                        case (Weapon.WeaponType.Sword_1H):
                            blockDamageModifier = modSettings.sword1h;
                            break;
                        case (Weapon.WeaponType.Sword_2H):
                            blockDamageModifier = modSettings.sword2h;
                            break;
                        case (Weapon.WeaponType.Axe_1H):
                            blockDamageModifier = modSettings.axe1h;
                            break;
                        case (Weapon.WeaponType.Axe_2H):
                            blockDamageModifier = modSettings.axe2h;
                            break;
                        case (Weapon.WeaponType.Mace_1H):
                            blockDamageModifier = modSettings.mace1h;
                            break;
                        case (Weapon.WeaponType.Mace_2H):
                            blockDamageModifier = modSettings.mace2h;
                            break;
                        case (Weapon.WeaponType.Halberd_2H):
                            blockDamageModifier = modSettings.halberd;
                            break;
                    }
                }

                __instance.VitalityHit(_dealerChar, _damage * blockDamageModifier, _hitDir);
            }
        }
    }
}