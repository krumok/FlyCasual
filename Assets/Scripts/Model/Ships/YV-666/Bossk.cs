﻿using RuleSets;
using Ship;
using SubPhases;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ship
{
    namespace YV666
    {
        public class Bossk : YV666, ISecondEditionPilot
        {
            public Bossk() : base()
            {
                PilotName = "Bossk";
                PilotSkill = 7;
                Cost = 35;

                IsUnique = true;

                PrintedUpgradeIcons.Add(Upgrade.UpgradeType.Elite);

                PilotAbilities.Add(new Abilities.BosskAbility());
            }

            public void AdaptPilotToSecondEdition()
            {
                PilotSkill = 4;
                Cost = 70;

                PilotAbilities.RemoveAll(a => a is Abilities.BosskAbility);
                PilotAbilities.Add(new Abilities.SecondEdition.BosskPilotAbilitySE());

                SEImageNumber = 210;
            }
        }
    }
}

namespace Abilities
{
    //When you perform an attack that hits, before dealing damage. you may cancel 1 critical result to add 2 success results.
    public class BosskAbility : GenericAbility
    {
        public override void ActivateAbility()
        {
            HostShip.OnShotHitAsAttacker += RegisterBosskPilotAbility;
        }

        public override void DeactivateAbility()
        {
            HostShip.OnShotHitAsAttacker -= RegisterBosskPilotAbility;
        }

        protected virtual void RegisterBosskPilotAbility()
        {
            if (Combat.DiceRollAttack.CriticalSuccesses > 0)
            {
                Phases.CurrentSubPhase.Pause();
                RegisterAbilityTrigger(TriggerTypes.OnShotHit, PromptToChangeCritSuccess);
            }
        }

        private void PromptToChangeCritSuccess(object sender, EventArgs e)
        {
            DecisionSubPhase decisionSubPhase = (DecisionSubPhase)Phases.StartTemporarySubPhaseNew(
                Name,
                typeof(DecisionSubPhase),
                Triggers.FinishTrigger);

            decisionSubPhase.InfoText = "Bossk: Would you like to cancel 1 critical result to add 2 success results?";

            decisionSubPhase.AddDecision("Yes", ConvertCriticalsToSuccesses);
            decisionSubPhase.AddDecision("No",  delegate { DecisionSubPhase.ConfirmDecision(); }
            );

            decisionSubPhase.RequiredPlayer = HostShip.Owner.PlayerNo;
            decisionSubPhase.ShowSkipButton = true;

            decisionSubPhase.DefaultDecisionName = "No";

            decisionSubPhase.Start();
        }

        private void ConvertCriticalsToSuccesses(object sender, EventArgs e)
        {
            Combat.DiceRollAttack.AddDice(DieSide.Success);
            Combat.DiceRollAttack.AddDice(DieSide.Success);

            Combat.DiceRollAttack.DiceList.Remove(
                Combat.DiceRollAttack.DiceList.First(die => die.Side == DieSide.Crit));

            Messages.ShowInfoToHuman("Bossk: Changed one critical result into two success results.");
            DecisionSubPhase.ConfirmDecision();
            Phases.CurrentSubPhase.Resume();
        }
    }
}

namespace Abilities.SecondEdition
{
    public class BosskPilotAbilitySE : BosskAbility
    {
        protected override void RegisterBosskPilotAbility()
        {
            if (Combat.ChosenWeapon == HostShip.PrimaryWeapon)
            {
                base.RegisterBosskPilotAbility();
            }
        }
    }
}
