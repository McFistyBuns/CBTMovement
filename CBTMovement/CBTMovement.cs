using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using BattleTech;
using BattleTech.UI;
using Harmony;
using HBS.DebugConsole;
using UnityEngine;
using Newtonsoft.Json;
using JetBrains.Annotations;

namespace CBTMovement
{
    [HarmonyPatch(typeof(EncounterLayerData))]
    [HarmonyPatch("ContractInitialize")]
    public static class EncounterLayerData_ContractInitialize_Patch
    {
        static void Prefix(EncounterLayerData __instance)
        {
            try
            {
                __instance.turnDirectorBehavior = TurnDirectorBehaviorType.AlwaysInterleaved;
            }
            catch (Exception e)
            {
            }
        }
    }

    //public float GetAllModifiers(AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot)
    [HarmonyPatch(typeof(ToHit), "GetAllModifiers")]
    public static class ToHit_GetAllModifiers_Patch
    {
        private static void Postfix(ToHit __instance, ref float __result, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot)
        {
            if (attacker.HasMovedThisRound && attacker.JumpedLastRound)
            {
                __result = __result + (float)CBTMovement.Settings.ToHitSelfJumped;
            }
        }
    }

    //public string GetAllModifiersDescription(AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot)
    [HarmonyPatch(typeof(ToHit), "GetAllModifiersDescription")]
    public static class ToHit_GetAllModifiersDescription_Patch
    {
        private static void Postfix(ToHit __instance, ref string __result, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot)
        {
            if (attacker.HasMovedThisRound && attacker.JumpedLastRound)
            {
                __result = string.Format("{0}JUMPED {1:+#;-#}; ", __result, CBTMovement.Settings.ToHitSelfJumped);
            }
        }
    }

    //private void UpdateToolTipsSelf()
    [HarmonyPatch(typeof(CombatHUDWeaponSlot), "SetHitChance", new Type[] { typeof(ICombatant) })]
    public static class CombatHUDWeaponSlot_SetHitChance_Patch 
    {
        private static void Postfix(CombatHUDWeaponSlot __instance, ICombatant target)
        {
            AbstractActor actor = __instance.DisplayedWeapon.parent;
            var _this = Traverse.Create(__instance);

            if (actor.HasMovedThisRound && actor.JumpedLastRound)
            {
                _this.Method("AddToolTipDetail", "JUMPED SELF", CBTMovement.Settings.ToHitSelfJumped ).GetValue();
            }
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "InitEffectStats")]
    public static class AbstractActor_InitEffectStats_Patch
    {
        private static void Postfix(AbstractActor __instance)
        {
            __instance.StatCollection.Set("CanShootAfterSprinting", true);
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "ResolveAttackSequence", null)]
    public static class AbstractActor_ResolveAttackSequence_Patch
    {
        private static bool Prefix(AbstractActor __instance)
        {
            return false;
        }

        private static void Postfix(AbstractActor __instance, string sourceID, int sequenceID, int stackItemID, AttackDirection attackDirection)
        {
            AttackDirector.AttackSequence attackSequence = __instance.Combat.AttackDirector.GetAttackSequence(sequenceID);
            if (attackSequence != null && attackSequence.attackDidDamage)
            {
                List<Effect> list = __instance.Combat.EffectManager.GetAllEffectsTargeting(__instance).FindAll((Effect x) => x.EffectData.targetingData.effectTriggerType == EffectTriggerType.OnDamaged);
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].OnEffectTakeDamage(attackSequence.attacker, __instance);
                }
                if (attackSequence.isMelee)
                {
                    int value = attackSequence.attacker.StatCollection.GetValue<int>("MeleeHitPushBackPhases");
                    if (value > 0)
                    {
                        for (int j = 0; j < value; j++)
                        {
                            __instance.ForceUnitOnePhaseDown(sourceID, stackItemID, false);
                        }
                    }
                }
            }
            int evasivePipsCurrent = __instance.EvasivePipsCurrent;
            int evasivePipsCurrent2 = __instance.EvasivePipsCurrent;
            if (evasivePipsCurrent2 < evasivePipsCurrent && !__instance.IsDead && !__instance.IsFlaggedForDeath)
            {
                __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, "-1 EVASION", FloatieMessage.MessageNature.Debuff));
            }
        }
    }

    internal class ModSettings
    {
        [JsonProperty("ToHitSelfJumped")]
        public int ToHitSelfJumped { get; set; }
    }

    public static class CBTMovement
    {
        internal static ModSettings Settings;

        public static void Init(string modDir, string modSettings)
        {
            var harmony = HarmonyInstance.Create("io.github.guetler.CBTMovement");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            try
            {
                Settings = JsonConvert.DeserializeObject<ModSettings>(modSettings);
            }
            catch (Exception)
            {
                Settings = new ModSettings();
            }
        }
    }
}