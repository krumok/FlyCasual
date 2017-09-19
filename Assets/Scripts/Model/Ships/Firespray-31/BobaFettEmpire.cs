﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ship
{
    namespace Firespray31
    {
        public class BobaFettEmpire : Firespray31
        {
            public BobaFettEmpire() : base()
            {
                PilotName = "Boba Fett";
                ImageUrl = "https://raw.githubusercontent.com/guidokessels/xwing-data/master/images/pilots/Galactic%20Empire/Firespray-31/boba-fett.png";
                PilotSkill = 8;
                Cost = 39;

                SkinName = "Boba Fett";

                PrintedUpgradeIcons.Add(Upgrade.UpgradeType.Elite);

                faction = Faction.Empire;
            }

            public override void InitializePilot()
            {
                base.InitializePilot();

                OnManeuverIsRevealed += RegisterAskChangeManeuver;
            }

            private void RegisterAskChangeManeuver(GenericShip ship)
            {
                Triggers.RegisterTrigger(new Trigger() {
                    Name = "Boba Fett's ability",
                    TriggerType = TriggerTypes.OnManeuverIsRevealed,
                    TriggerOwner = ship.Owner.PlayerNo,
                    EventHandler = AskChangeManeuver
                });
            }

            private void AskChangeManeuver(object sender, System.EventArgs e)
            {
                GameManagerScript Game = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
                Game.UI.ShowDirectionMenu();
            }
        }
    }
}
