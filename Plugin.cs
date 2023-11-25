using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Base_Mod;
using HarmonyLib;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace Dispenser_Shows_Loaded_And_Total_Counts {
    [UsedImplicitly]
    public class Plugin : BaseGameMod {
        protected override bool UseHarmony => true;
    }

    [HarmonyPatch]
    [UsedImplicitly]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class SendLoadedAmountWithSetItem {
        [HarmonyTargetMethod]
        [UsedImplicitly]
        public static MethodBase TargetMethod() {
            return typeof(CargoDisplayPanelUi).GetMethod(nameof(CargoDisplayPanelUi.AssignItems), BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [UsedImplicitly]
        [HarmonyPostfix]
        public static bool Prefix(ref CargoDisplayPanelUi        __instance,
                                  ref CargoDisplayPanelUi.Slot[] ___m_slots,
                                  ref ItemDisplayPanel           ___m_panel,
                                  ref TrainProduction            ___m_production,
                                  ref int[]                      ___m_amount,
                                  ref bool                       ___m_isOnline,
                                  ref Color                      ___m_offlineColor) {
            if (__instance.gameObject.TryGetComponentInParent<Dispenser>(out var dispenser)) {
                foreach (var slot in ___m_slots) {
                    var item         = ___m_panel.Items[slot.ItemIndex];
                    var queued       = item != null && ___m_production.IsQueued(item);
                    var loadedAmount = dispenser.IsPickable(slot.ItemIndex) ? dispenser.Slots[slot.ItemIndex].Item.Amount : 0;
                    slot.SlotUi.SetItem(item != null ? item.Icon : null, ___m_amount[slot.ItemIndex], loadedAmount, ___m_isOnline ? Color.white : ___m_offlineColor, queued);
                }
                return false;
            } else {
                return true;
            }
        }
    }

    [HarmonyPatch]
    [UsedImplicitly]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class MakeSetTextShowLoadedAmount {
        [HarmonyTargetMethod]
        [UsedImplicitly]
        public static MethodBase TargetMethod() {
            return typeof(ItemDisplayWorldUi).GetMethod(nameof(ItemDisplayWorldUi.UpdateText), BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [UsedImplicitly]
        [HarmonyPostfix]
        public static bool Prefix(ref ItemDisplayWorldUi __instance, ref TextMeshPro ___m_text, ref int ___m_inventoryCount) {
            if (__instance.gameObject.TryGetComponentInParent<Dispenser>(out var dispenser)) {
                var slotIndex    = GetSlotIndex(__instance.name);
                var slot         = dispenser.Slots[slotIndex];
                var loadedAmount = dispenser.IsPickable(slotIndex) ? slot.Item.Amount : 0;
                ___m_text.SetTextFormat("{0} [{1}]", Format.Multiples(loadedAmount), Format.Multiples(___m_inventoryCount));
                return false;
            }
            return true;
        }

        private static int GetSlotIndex(string name) {
            switch (name) {
                case "SlotUi_0a": return 0;
                case "SlotUi_1a": return 1;
                case "SlotUi_2a": return 2;
                default: return -1;
            }
        }
    }
}