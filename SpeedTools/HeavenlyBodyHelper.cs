using PacificEngine.OW_CommonResources.Game.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedTools
{
    internal class HeavenlyBodyHelper
    {
        public static HeavenlyBody lookupHeavenlyBody(string bodyName)
        {
            switch (bodyName)
            {
                case "None":
                    return HeavenlyBodies.None;
                case "Player":
                    return HeavenlyBodies.Player;
                case "Ship":
                    return HeavenlyBodies.Ship;
                case "Probe":
                    return HeavenlyBodies.Probe;
                case "ModelShip":
                    return HeavenlyBodies.ModelShip;
                case "Sun":
                    return HeavenlyBodies.Sun;
                case "SunStation":
                    return HeavenlyBodies.SunStation;
                case "HourglassTwins":
                    return HeavenlyBodies.HourglassTwins;
                case "AshTwin":
                    return HeavenlyBodies.AshTwin;
                case "EmberTwin":
                    return HeavenlyBodies.EmberTwin;
                case "TimberHearth":
                    return HeavenlyBodies.TimberHearth;
                case "TimberHearthProbe":
                    return HeavenlyBodies.TimberHearthProbe;
                case "Attlerock":
                    return HeavenlyBodies.Attlerock;
                case "BrittleHollow":
                    return HeavenlyBodies.BrittleHollow;
                case "HollowLantern":
                    return HeavenlyBodies.HollowLantern;
                case "GiantsDeep":
                    return HeavenlyBodies.GiantsDeep;
                case "ProbeCannon":
                    return HeavenlyBodies.ProbeCannon;
                case "NomaiProbe":
                    return HeavenlyBodies.NomaiProbe;
                case "NomaiEmberTwinShuttle":
                    return HeavenlyBodies.NomaiEmberTwinShuttle;
                case "NomaiBrittleHollowShuttle":
                    return HeavenlyBodies.NomaiBrittleHollowShuttle;
                case "DarkBramble":
                    return HeavenlyBodies.DarkBramble;
                case "InnerDarkBramble_Hub":
                    return HeavenlyBodies.InnerDarkBramble_Hub;
                case "InnerDarkBramble_EscapePod":
                    return HeavenlyBodies.InnerDarkBramble_EscapePod;
                case "InnerDarkBramble_Nest":
                    return HeavenlyBodies.InnerDarkBramble_Nest;
                case "InnerDarkBramble_Feldspar":
                    return HeavenlyBodies.InnerDarkBramble_Feldspar;
                case "InnerDarkBramble_Gutter":
                    return HeavenlyBodies.InnerDarkBramble_Gutter;
                case "InnerDarkBramble_Vessel":
                    return HeavenlyBodies.InnerDarkBramble_Vessel;
                case "InnerDarkBramble_Maze":
                    return HeavenlyBodies.InnerDarkBramble_Maze;
                case "InnerDarkBramble_SmallNest":
                    return HeavenlyBodies.InnerDarkBramble_SmallNest;
                case "InnerDarkBramble_Secret":
                    return HeavenlyBodies.InnerDarkBramble_Secret;
                case "Interloper":
                    return HeavenlyBodies.Interloper;
                case "WhiteHole":
                    return HeavenlyBodies.WhiteHole;
                case "WhiteHoleStation":
                    return HeavenlyBodies.WhiteHoleStation;
                case "Stranger":
                    return HeavenlyBodies.Stranger;
                case "DreamWorld":
                    return HeavenlyBodies.DreamWorld;
                case "QuantumMoon":
                    return HeavenlyBodies.QuantumMoon;
                case "SatiliteBacker":
                    return HeavenlyBodies.SatiliteBacker;
                case "SatiliteMapping":
                    return HeavenlyBodies.SatiliteMapping;
                case "EyeOfTheUniverse":
                    return HeavenlyBodies.EyeOfTheUniverse;
                case "EyeOfTheUniverse_Vessel":
                    return HeavenlyBodies.EyeOfTheUniverse_Vessel;
                default:
                    return null;
            }
        }

        public static String heavenlyBodyToHumanText(HeavenlyBody body)
        {
            switch (body.name)
            {
                case "OuterWild_Standard_Probe":
                    return "Probe";
                case "OuterWild_Standard_Model_Ship":
                    return "ModelShip";
                case "OuterWild_Standard_Sun":
                    return "Sun";
                case "OuterWild_Standard_Sun_Station":
                    return "SunStation";
                case "OuterWild_Standard_Hourglass_Twins":
                    return "HourglassTwins";
                case "OuterWild_Standard_Ash_Twin":
                    return "AshTwin";
                case "OuterWild_Standard_Ember_Twin":
                    return "EmberTwin";
                case "OuterWild_Standard_Timber_Hearth":
                    return "TimberHearth";
                case "OuterWild_Standard_Timber_Hearth_Probe":
                    return "TimberHearthProbe";
                case "OuterWild_Standard_Attlerock":
                    return "Attlerock";
                case "OuterWild_Standard_Brittle_Hollow":
                    return "BrittleHollow";
                case "OuterWild_Standard_Hollow_Lantern":
                    return "HollowLantern";
                case "OuterWild_Standard_Giants_Deep":
                    return "GiantsDeep";
                case "OuterWild_Standard_Probe_Cannon":
                    return "ProbeCannon";
                case "OuterWild_Standard_Nomai_Probe":
                    return "NomaiProbe";
                case "OuterWild_Standard_Nomai_Ember_Twin_Shuttle":
                    return "NomaiEmberTwinShuttle";
                case "OuterWild_Standard_Nomai_Brittle_Hollow_Shuttle":
                    return "NomaiBrittleHollowShuttle";
                case "OuterWild_Standard_Dark_Bramble":
                    return "DarkBramble";
                case "OuterWild_Standard_Inner_Dark_Bramble_Hub":
                    return "InnerDarkBramble_Hub";
                case "OuterWild_Standard_Inner_Dark_Bramble_Escape_Pod":
                    return "InnerDarkBramble_EscapePod";
                case "OuterWild_Standard_Inner_Dark_Bramble_Nest":
                    return "InnerDarkBramble_Nest";
                case "OuterWild_Standard_Inner_Dark_Bramble_Feldspar":
                    return "InnerDarkBramble_Feldspar";
                case "OuterWild_Standard_Inner_Dark_Bramble_Gutter":
                    return "InnerDarkBramble_Gutter";
                case "OuterWild_Standard_Inner_Dark_Bramble_Vessel":
                    return "InnerDarkBramble_Vessel";
                case "OuterWild_Standard_Inner_Dark_Bramble_Maze":
                    return "InnerDarkBramble_Maze";
                case "OuterWild_Standard_Inner_Dark_Bramble_Small_Nest":
                    return "InnerDarkBramble_SmallNest";
                case "OuterWild_Standard_Inner_Dark_Bramble_Secret":
                    return "InnerDarkBramble_Secret";
                case "OuterWild_Standard_Interloper":
                    return "Interloper";
                case "OuterWild_Standard_White_Hole":
                    return "WhiteHole";
                case "OuterWild_Standard_White_Hole_Station":
                    return "WhiteHoleStation";
                case "OuterWild_Standard_Stranger":
                    return "Stranger";
                case "OuterWild_Standard_Dream_World":
                    return "DreamWorld";
                case "OuterWild_Standard_Quantum_Moon":
                    return "QuantumMoon";
                case "OuterWild_Standard_Satilite_Backer":
                    return "SatiliteBacker";
                case "OuterWild_Standard_Satilite_Mapping":
                    return "SatiliteMapping";
                case "OuterWild_Standard_Eye_Of_The_Universe":
                    return "EyeOfTheUniverse";
                case "OuterWild_Standard_Eye_Of_The_Universe_Vessel":
                    return "EyeOfTheUniverse_Vessel";
                default:
                    return body.name;
            }


        }

    }
}
