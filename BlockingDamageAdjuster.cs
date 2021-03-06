﻿using System;
using System.IO;
using System.Reflection;
using BepInEx;
using Harmony;
using UnityEngine;
using static BlockingDamageAdjuster.Logger;

namespace BlockingDamageAdjuster
{
    [BepInPlugin("com.gnivler.BlockingDamageAdjuster", "BlockingDamageAdjuster", "1.1")]
    public class BlockingDamageAdjuster : BaseUnityPlugin
    {
        internal static Settings modSettings = new Settings();

        public class Settings
        {
            public float axe1h = 0f;
            public float axe2h = 0f;
            public float sword1h = 0f;
            public float sword2h = 0f;
            public float mace1h = 0f;
            public float mace2h = 0f;
            public float halberd = 0f;
            public float spear = 0f;
            public float shield = 0f;
            public bool enableDebug = false;
        }

        public void Awake()
        {
            var harmony = HarmonyInstance.Create("com.gnivler.BlockingDamageAdjuster");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            try
            {
                using (StreamReader reader = new StreamReader(
                    Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName +
                    "\\BlockingDamageAdjuster.json"))
                {
                    var json = reader.ReadToEnd();
                    modSettings = JsonUtility.FromJson<Settings>(json);
                }
            }
            catch (Exception e)
            {
                Error(e);
            }

            LogDebug($"{DateTime.Now.ToShortTimeString()} BlockingDamageAdjuster Starting up");
        }

        [HarmonyPatch(typeof(Character), "ReceiveBlock", MethodType.Normal)]
        [HarmonyPatch(new[]
        {
            typeof(Weapon),
            typeof(DamageList),
            typeof(Vector3),
            typeof(float),
            typeof(float),
            typeof(Character),
            typeof(float)
        })]
        public class ReceiveBlockPatch
        {
            public static void Postfix(
                Character __instance,
                Vector3 _hitDir,
                DamageList _damage,
                Character _dealerChar)
            {
                var character = __instance;
                var blockDamageModifier = 0f;
                if (character.ShieldEquipped)
                {
                    blockDamageModifier = modSettings.shield;
                }
                else
                {
                    switch (character.CurrentWeapon.Type)
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
                        case (Weapon.WeaponType.Spear_2H):
                            blockDamageModifier = modSettings.spear;
                            break;
                        case (Weapon.WeaponType.Halberd_2H):
                            blockDamageModifier = modSettings.halberd;
                            break;
                    }
                }

                character.VitalityHit(_dealerChar, _damage.TotalDamage * blockDamageModifier, _hitDir);
            }
        }
    }
}