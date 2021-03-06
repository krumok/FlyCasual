﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Abilities;
using System;
using Ship;
using BoardTools;
using RuleSets;

namespace Ship
{
    namespace JumpMaster5000
    {
        public class Dengar : JumpMaster5000, ISecondEditionPilot
        {
            public Dengar() : base()
            {
                PilotName = "Dengar";
                PilotSkill = 9;
                Cost = 33;

                IsUnique = true;

                // Already have Elite icon from JumpMaster5000 class

                PilotAbilities.Add(new DengarPilotAbility());
            }

            public void AdaptPilotToSecondEdition()
            {
                PilotSkill = 6;
                Cost = 64;

                UsesCharges = true;
                MaxCharges = 1;
                RegensCharges = true;

                PilotAbilities.RemoveAll(a => a is DengarPilotAbility);
                PilotAbilities.Add(new Abilities.SecondEdition.DengarPilotAbilitySE());

                SEImageNumber = 214;
            }
        }
    }
}

namespace Abilities
{
    public class DengarPilotAbility : GenericAbility
    {
        private GenericShip shipToPunish;
        private bool isPerformedRegularAttack;

        public override void ActivateAbility()
        {
            HostShip.OnAttackFinishAsDefender += CheckAbility;
            Phases.Events.OnRoundEnd += ClearIsAbilityUsedFlag;
        }

        public override void DeactivateAbility()
        {
            HostShip.OnAttackFinishAsDefender -= CheckAbility;
            Phases.Events.OnRoundEnd -= ClearIsAbilityUsedFlag;
        }

        private void CheckAbility(GenericShip ship)
        {
            if (!CanUseAbility()) return;

            if (HostShip.IsCannotAttackSecondTime) return;

            ShotInfo counterAttackInfo = new ShotInfo(Combat.Defender, Combat.Attacker, Combat.Defender.PrimaryWeapon);
            if (!CanCounterattackUsingShotInfo(counterAttackInfo)) return;

            // Save his attacker, becuase combat data will be cleared
            shipToPunish = Combat.Attacker;

            Combat.Attacker.OnCombatCheckExtraAttack += RegisterAbility;
        }

        protected virtual bool CanUseAbility()
        {
            return !IsAbilityUsed;
        }

        protected virtual bool CanCounterattackUsingShotInfo(ShotInfo counterAttackInfo)
        {
            return counterAttackInfo.InArc;
        }

        protected virtual void MarkAbilityAsUsed()
        {
            IsAbilityUsed = true;
        }

        private void RegisterAbility(GenericShip ship)
        {
            ship.OnCombatCheckExtraAttack -= RegisterAbility;

            RegisterAbilityTrigger(TriggerTypes.OnCombatCheckExtraAttack, DoCounterAttack);
        }

        private void DoCounterAttack(object sender, EventArgs e)
        {
            if (!HostShip.IsCannotAttackSecondTime)
            {
                // Temporary fix
                if (HostShip.IsDestroyed)
                {
                    Triggers.FinishTrigger();
                    return;
                }

                // Save his "is already attacked" flag
                isPerformedRegularAttack = HostShip.IsAttackPerformed;

                // Plan to set IsAbilityUsed only after attack that was successfully started
                HostShip.OnAttackStartAsAttacker += MarkAbilityAsUsed;

                Combat.StartAdditionalAttack(
                    HostShip,
                    FinishExtraAttack,
                    CounterAttackFilter,
                    HostShip.PilotName,
                    "You may perform an additional attack against " + shipToPunish.PilotName + ".",
                    HostShip.ImageUrl
                );
            }
            else
            {
                Messages.ShowErrorToHuman(string.Format("{0} cannot attack one more time", HostShip.PilotName));
                Triggers.FinishTrigger();
            }
        }

        private void FinishExtraAttack()
        {
            // Restore previous value of "is already attacked" flag
            HostShip.IsAttackPerformed = isPerformedRegularAttack;

            // Set IsAbilityUsed only after attack that was successfully started
            HostShip.OnAttackStartAsAttacker -= MarkAbilityAsUsed;

            Triggers.FinishTrigger();
        }

        private bool CounterAttackFilter(GenericShip targetShip, IShipWeapon weapon, bool isSilent)
        {
            bool result = true;

            if (targetShip != shipToPunish)
            {
                if (!isSilent) Messages.ShowErrorToHuman(string.Format("{0} can attack only {1}", HostShip.PilotName, shipToPunish.PilotName));
                result = false;
            }

            return result;
        }

    }
}

namespace Abilities.SecondEdition
{
    public class DengarPilotAbilitySE : DengarPilotAbility
    {
        protected override bool CanCounterattackUsingShotInfo(ShotInfo counterAttackInfo)
        {
            return counterAttackInfo.InArc && HostShip.ArcInfo.GetArc<Arcs.ArcMobile>().Facing == Arcs.ArcFacing.Forward;
        }

        protected override bool CanUseAbility()
        {
            return HostShip.Charges > 0;
        }

        protected override void MarkAbilityAsUsed()
        {
            //Empty delegate is safe here - Sandrem
            HostShip.SpendCharge(delegate { });
        }
    }
}