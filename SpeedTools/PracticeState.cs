using PacificEngine.OW_CommonResources.Game.Player;
using PacificEngine.OW_CommonResources.Game.Resource;
using PacificEngine.OW_CommonResources.Game.State;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Position = PacificEngine.OW_CommonResources.Game.Resource.Position;

namespace SpeedTools
{
    interface IPracticeState
    {
        public float getWaitTime();
        public void setWaitTime(int waitTime);
        public void setSpaceSuit(bool spaceSuit);
        public void setInfiniteFuel(bool inifiniteFuel);
        public void setFreezeSuperNova(bool activeSuperNova);
        public void setEyeCoordinates(bool eyeCoordinates);
        public void setTreeLocator(bool treeLocatorEnabled);
        public void preWait();
        public void postWait();
        public void postSceneLoad();

    }

    /**
     * Abstract parent class that contains all shared behavior for Practice States
     */
    abstract class PracticeState<T>: MonoBehaviour, IPracticeState where T : PracticeState<T>
    {
        private static readonly Lazy<T> Lazy = new(() => (Activator.CreateInstance(typeof(T), true) as T)!);

        public static T Instance => Lazy.Value;

        /*
         * Config values
         */
        protected float waitTime = 0f;
        protected bool spaceSuit = false;
        protected bool infiniteFuel = false;
        protected bool freezeSuperNova = false;
        protected bool eyeCoordinates = false;
        protected bool launchCodes = false;
        protected bool treeLocator = false;

        protected HeavenlyBody teleportBody = null;
        protected Vector3 teleportPosition = Vector3.negativeInfinity;
        protected Quaternion teleportRotation = Quaternion.identity;

        public float getWaitTime()
        {
            return this.waitTime;
        }

        public void setWaitTime(int time)
        {
            this.waitTime = (float)time;
        }


        public void setSpaceSuit(bool spaceSuit)
        {
            this.spaceSuit = spaceSuit;
        }

        public void setInfiniteFuel(bool infiniteFuel)
        {
            this.infiniteFuel= infiniteFuel;
        }

        public void setFreezeSuperNova(bool freezeSuperNova)
        {
            this.freezeSuperNova = freezeSuperNova;
        }

        public void setEyeCoordinates(bool eyeCoordinates)
        {
            this.eyeCoordinates = eyeCoordinates;
        }

        public void setTeleportBody(string bodyName)
        {
            if(bodyName == null)
            {
                this.teleportBody = null;
                return;
            }

            this.teleportBody = HeavenlyBodyHelper.lookupHeavenlyBody(bodyName);
        }

        public void setTeleportPosition(string vector, ref List<string> errorMessageList)
        {
            if(vector == null || String.IsNullOrWhiteSpace(vector))
            {
                this.teleportPosition = Vector3.zero;
                errorMessageList.Add("Position vector cannot be empty");
                return;
            }

            string[] parts = vector.Split(',');

            if(parts.Length != 3 || String.IsNullOrWhiteSpace(parts[0]) || String.IsNullOrWhiteSpace(parts[1]) || String.IsNullOrWhiteSpace(parts[2]))
            {
                this.teleportPosition = Vector3.zero;
                errorMessageList.Add("Incomplete position vector: " + vector);
                return;
            }

            float x = float.Parse(parts[0].Trim());
            float y = float.Parse(parts[1].Trim());
            float z = float.Parse(parts[2].Trim());

            teleportPosition = new Vector3(x, y, z);
        }

        public void setTeleportRotation(string quaternion)
        {
            if (quaternion == null)
            {
                this.teleportRotation = Quaternion.identity;
                return;
            }

            string[] parts = quaternion.Split(',');

            if (parts.Length != 4)
            {
                // Throwing an exception breaks everything. Find a better way to handle this
                //throw new Exception("Unable to parse position from [" + vector + "]. Should be in format [0.0, 0.0, 0.0] ");

                this.teleportRotation = Quaternion.identity;
                return;
            }

            float x = float.Parse(parts[0].Trim());
            float y = float.Parse(parts[1].Trim());
            float z = float.Parse(parts[2].Trim());
            float w = float.Parse(parts[3].Trim());

            teleportRotation = new Quaternion(x, y, z, w);
        }

        /*
         * Only useful for the instrument hunt, but it needs to be visible
         */
        public void setTreeLocator(bool treeLocatorEnabled)
        {
            this.treeLocator = treeLocatorEnabled;
        }

        /*
         * Actions to take before going into wait mode
         */
        public virtual void preWait() { }

        /*
         * Actions to take after finishing wait mode
         */
        public virtual void postWait()
        {
            toggleSpaceSuit(spaceSuit);
            toggleInfiniteFuel(infiniteFuel);
            toggleSuperNova(freezeSuperNova);

            if (teleportBody != null && teleportPosition != Vector3.negativeInfinity) {
                var parent = Position.getBody(teleportBody);
                if (Locator.GetPlayerBody() && parent)
                {
                    Teleportation.teleportPlayerTo(parent, teleportPosition, Vector3.zero, Vector3.zero, Vector3.zero, teleportRotation);
                }
            }
        }

        /*
         * Actions to take after loading a new scene
         */
        public virtual void postSceneLoad() { }

        /*
         * Enable/disable player space suit
         */
        protected void toggleSpaceSuit(bool active)
        {
            Player.spaceSuit = active;
        }

        /*
         * Enable/disable infinite fuel
         */
        protected void toggleInfiniteFuel(bool active)
        {
            Player.hasUnlimitedFuel = active;
        }

        /*
         * Freeze/unfreeze supernova
         * TODO: What actually determines if time is flowing?
         */
        protected void toggleSuperNova(bool frozen)
        {
            SuperNova.freeze = frozen;
        }

        /*
         * Removes Hornfels from in front of the Nomai statue
         */
        protected void deactivateHornfels()
        {
            Destroy(GameObject.Find("Villager_HEA_Hornfels (1)")); //.SetActive(false);
        }

        /*
         * Give the the warp core to the player
         */
        protected void giveWarpCore()
        {
            Possession.pickUpWarpCore(WarpCoreType.Vessel);
        }

        protected void placePlayerInShip(bool seated)
        {
            Teleportation.teleportPlayerToShip();

            HatchController hatchController = GameObject.FindObjectOfType<HatchController>();
            hatchController.CloseHatch();
            ShipTractorBeamSwitch tractorbeamSwitch = GameObject.FindObjectOfType<ShipTractorBeamSwitch>();
            tractorbeamSwitch.DeactivateTractorBeam();

            // TODO: still need to add player to ship gravity volume and maybe trigger some other things. debugging later

            // This adds the player to the gravity volume, but the player doesn't get removed on exiting. Need to re-enter and exit again for that to happen
            ShipDirectionalForceVolume[] gravityVolumes = GameObject.FindObjectsOfType<ShipDirectionalForceVolume>();
            foreach(ShipDirectionalForceVolume volume in gravityVolumes)
            {
                if(volume.name == "ShipGravityVolume")
                {
                    volume.OnEffectVolumeEnter(GameObject.FindGameObjectWithTag("PlayerDetector"));
                    volume.OnEffectVolumeEnter(GameObject.FindGameObjectWithTag("PlayerCameraDetector"));
                    break;
                }
            }

            if(seated) {
                OWTriggerVolume shipOxygenVolume = null;

                ShipCockpitController cockpitController = GameObject.FindObjectOfType<ShipCockpitController>();
                if (cockpitController != null)
                {
                    cockpitController.OnPressInteract();
                }
            }
        }
    }

    /**
     * Practice state for flying to Ash Twin starting from initial wake-up
     */
    class PracticeStateTwinFlight : PracticeState<PracticeStateTwinFlight>
    {
        private PracticeStateTwinFlight()
        {
            this.teleportBody = HeavenlyBodies.TimberHearth;
            this.teleportPosition = new Vector3(19.9f, -44.19f, 185.65f);
        }

        public override void preWait()
        {
            base.preWait();

            // Make sure the supernova loop is on for this state in case it got turned off
            toggleSuperNova(false);
        }
    }

    /**
     * Practice state for skipping the Nomai Statue cutscene
     */
    class PracticeStateStatueSkip : PracticeState<PracticeStateStatueSkip>
    {
        private PracticeStateStatueSkip()
        {
            this.spaceSuit = true;
            //this.infiniteFuel = true;
            //this.freezeSuperNova = true;
            this.launchCodes = true;
            this.teleportBody = HeavenlyBodies.TimberHearth;
            this.teleportPosition = new Vector3(-68f, 5.5f, 215.6f);
            this.teleportRotation = new Quaternion(-0.7f, -0.7f, -0.2f, 0.2f);
            
            // TODO: figure out rotation here
            // fixed rotation values are never gonna work since the relative player--TH rotation is constantly changing
        }

        public override void postWait()
        {
            // Hornfels will be blocking the entrance after your first death.
            // Delete him so the skip movement can be practiced any time
            deactivateHornfels();
            
            // Give launch codes in case Hornfels hasn't been spoken to
            Data.launchCodes = this.launchCodes;
     
            base.postWait();
        }
    }

    /**
     * Practice state for Vessel activation (placing warp core and inputting coordinates) and then into Vessel clip
     */
    class PracticeStateVesselActivation : PracticeState<PracticeStateVesselActivation>
    {
        private PracticeStateVesselActivation()
        {
            this.spaceSuit = true;
            //this.eyeCoordinates = true;
            this.teleportBody = HeavenlyBodies.InnerDarkBramble_Vessel;
            this.teleportPosition = new Vector3(82.4f, 4.4f, -11.3f);
            this.teleportRotation = new Quaternion(-2.0f, 20.0f, -2.0f, 15.0f);  // no idea why these numbers work for the correct rotation, but they do
        }

        public override void postWait()
        {
            if (SceneManager.GetActiveScene().name != OWScene.SolarSystem.ToString())
            {
                AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(OWScene.SolarSystem.ToString());
            } else
            {
                giveWarpCore();

                base.postWait();

                // Give eye coordinates if this option is toggled on
                Data.eyeCoordinates = this.eyeCoordinates;
            }
        }

        public override void postSceneLoad()
        {
            base.postWait();
        }
    }

    /**
     * Practice state for Vessel clip. Warp core and coordinates are already in place.
     * There appears to be some race conditions here as triggering the warp too quickly can cause the player to get stuck in the black void
     */
    class PracticeStateVesselClip : PracticeState<PracticeStateVesselClip>
    {
        private PracticeStateVesselClip()
        {
            this.spaceSuit = true;
            this.teleportBody = HeavenlyBodies.InnerDarkBramble_Vessel;
            this.teleportPosition = new Vector3(175.44f, 13.38f, -19.37f);
            this.teleportRotation = new Quaternion(-2.0f, 20.0f, -2.0f, 15.0f);  // no idea why these numbers work for the correct rotation, but they do
        }

        public override void postWait()
        {
            base.postWait();

            // Add the player and camera to volume so gravity can take effect
            var volume = GameObject.Find("GravityOxygenVolume_VesselBridge").GetComponent<OWTriggerVolume>();

            volume.AddObjectToVolume(GameObject.Find("PlayerDetector"));
            volume.AddObjectToVolume(GameObject.Find("CameraDetector"));

            // Insert the warp core
            var warpController = GameObject.Find("WarpController").GetComponent<VesselWarpController>();

            warpController.OnWarpCorePlaced(Items.createWarpCore(WarpCoreType.Vessel));

            // Set the coordinates of the pillar
            var x = GameObject.Find("Interface_X").GetComponent<NomaiNodeController>();
            var y = GameObject.Find("Interface_Y").GetComponent<NomaiNodeController>();
            var z = GameObject.Find("Interface_Z").GetComponent<NomaiNodeController>();

            var xlist = new List<int>() { 4, 5, 1 };
            var ylist = new List<int>() { 4, 1, 0, 3 };
            var zlist = new List<int>() { 4, 5, 0, 3, 2, 1 };

            x._activeNodes.AddRange(xlist);
            y._activeNodes.AddRange(ylist);
            z._activeNodes.AddRange(zlist);
        }

        public override void postSceneLoad()
        {
            
        }
    }

    /**
     * Practice state for entering Dark Bramble after acquiring the warp core and leaving Ash Twin Project
     */
    class PracticeStateBrambleEntry : PracticeState<PracticeStateBrambleEntry>
    {
        private PracticeStateBrambleEntry()
        {
            this.spaceSuit = true;
            this.teleportBody = HeavenlyBodies.AshTwin;
            this.teleportPosition = new Vector3(0f, 13.9f, -123.8f);
        }

        public override void preWait()
        {
            base.preWait();

            // Make sure the supernova loop is on for this state in case it got turned off
            toggleSuperNova(false);
        }

        public override void postWait()
        {
            giveWarpCore();

            base.postWait();

            // Teleport player's ship to the bridge outside
            var parent = Position.getBody(HeavenlyBodies.AshTwin);
            if (Locator.GetShipBody() && parent && !PlayerState.IsInsideShip())
            {
                var bridge = GameObject.Find("Structure_HT_TT_Bridge").transform;
                var shipAbsPos = bridge.TransformPoint(new Vector3(-6.96f, -0.2f, -126.73f));
                Teleportation.teleportObjectTo(Locator.GetShipBody(), shipAbsPos, parent.GetPointVelocity(shipAbsPos), Vector3.zero, parent.GetAcceleration(), bridge.rotation * new Quaternion(-0.5f, 0.5f, -0.5f, 0.5f));
            }
        }
    }

    class PracticeStateBrambleNavigation : PracticeState<PracticeStateBrambleNavigation>
    {
        private PracticeStateBrambleNavigation()
        {
            this.spaceSuit = true;
            //this.teleportBody = HeavenlyBodies.AshTwin;
            //this.teleportPosition = new Vector3(0f, 13.9f, -123.8f);
        }

        public override void preWait()
        {
            base.preWait();

            // Make sure the supernova loop is on for this state in case it got turned off
            toggleSuperNova(false);
        }

        public override void postWait()
        {
            base.postWait();

            giveWarpCore();

            // Teleport player's ship to the bridge outside
            /*
            var parent = Position.getBody(HeavenlyBodies.AshTwin);
            if (Locator.GetShipBody() && parent && !PlayerState.IsInsideShip())
            {
                var bridge = GameObject.Find("Structure_HT_TT_Bridge").transform;
                var shipAbsPos = bridge.TransformPoint(new Vector3(-6.96f, -0.2f, -126.73f));
                Teleportation.teleportObjectTo(Locator.GetShipBody(), shipAbsPos, parent.GetPointVelocity(shipAbsPos), Vector3.zero, parent.GetAcceleration(), bridge.rotation * new Quaternion(-0.5f, 0.5f, -0.5f, 0.5f));
            }*/

            // Teleport player's ship to inside Bramble hub, speeding into the nest seed
            var parent = Position.getBody(HeavenlyBodies.InnerDarkBramble_Hub);
            if (Locator.GetShipBody() && parent)
            {
                var hub = GameObject.Find("SpawnPoint_Ship_HubDimension").transform;
                var shipAbsPos = hub.TransformPoint(new Vector3(-17.0f, 103.8f, 356.5f));
                Teleportation.teleportObjectTo(Locator.GetShipBody(), shipAbsPos, new Vector3(10f, -250.0f, -50.0f), Vector3.zero, parent.GetAcceleration(), hub.rotation * new Quaternion(-0.2f, 0.5f, -0.8f, 0.1f));
            }
            // TODO: maybe fix rotation based on how people typically enter the node
            // TODO: reset ship status (fully repaired) on state reset
            // TODO: reset all fishies to their original status (position, speed, rotation, + sleepin) on state reset

            // also TODO: add section to config for position, velocity, etc.
            //              this is not very user friendly right now; I had to use the position debug menu
            //              from cheats mod to figure out a roughly appropriate velocity vector here...

            // also also TODO: add a console output for position and rotation to make this mildly more user friendly?
            //              currently I'm using a random object inside bramble ("SpawnPoint_Ship_HubDimension") to
            //              find and set fixed coordinates and rotation inside. not sure if we need to do this since bramble
            //              itself is a unique space with no velocity or acceleration. could be useful if someone wants to start
            //              the ship just outside bramble

            placePlayerInShip(true);
        }
    }


    /**
     * Practice state for final concert. Starts right as the player drops into the Observatory
     */
    class PracticeStateInstrumentHunt : PracticeState<PracticeStateInstrumentHunt>
    {
        private PracticeStateInstrumentHunt()
        {
            this.spaceSuit = true;
        }


        public override void postWait()
        {
            base.postWait();

            //OWScene.EyeOfTheUniverse
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("EyeOfTheUniverse");
        }

        public override void postSceneLoad()
        {
            Locator.GetEyeStateManager().SetState(EyeState.WarpedToSurface);

            var parent = Position.getBody(HeavenlyBodies.EyeOfTheUniverse);
            if (Locator.GetPlayerBody() && parent)
            {
                // This takes you to the ledge before the jump
                //Teleportation.teleportPlayerTo(parent, new Vector3(9.567f, -219.316f, 0.556f), Vector3.zero, Vector3.zero, Vector3.zero, Quaternion.identity);

                // This drops you in right before you warp to the observatory
                Teleportation.teleportPlayerTo(parent, new Vector3(-80.616f, -3905.84f, 180.686f), Vector3.zero, Vector3.zero, Vector3.zero, Quaternion.identity);
            }

            // Reapply space suit after sceneload and warping. Scene load is sketchy
            toggleSpaceSuit(spaceSuit);

            SpeedTools.toggleTreeLocator(treeLocator);
        }
    }

    /**
     * Practice state for Stranger (experimental)
     */
    class PracticeStateStranger : PracticeState<PracticeStateStranger>
    {
        private PracticeStateStranger()
        {
            this.spaceSuit = true;
            this.teleportBody = HeavenlyBodies.Stranger;
            this.teleportPosition = new Vector3(49.54f, -77.79f, -293.29f);
        }

        public override void postWait()
        {
            base.postWait();

            /**
             * For now, this teleport works okay.
             * 
             * You can dream etc. and most things seem to be working normally.
             * 
             * One issue: the "static" ringworld remains cloaked - everything technically exists and can be interacted with but it's hard because invisible.
             * "Visible Stranger" mod fixes the outside being invisible. I don't know if we want to decloak it as well or just ignore it.
             */

            // I was testing if teleporting to the front dock first would uncloak the Stranger, but this didn't work.
            // Maybe not enough loading time but probably not an elegant solution anyway.
            /*
            var parent = Position.getBody(HeavenlyBodies.Stranger);
            if (Locator.GetPlayerBody() && parent)
            {
                // First teleport to front dock so the front area loads properly
                //Teleportation.teleportPlayerTo(parent, new Vector3(45.5f, -169f, -290f), Vector3.zero, Vector3.zero, Vector3.zero, Quaternion.identity);

                // Now teleport to desired area within the Stranger
                Teleportation.teleportPlayerTo(parent, teleportPosition, Vector3.zero, Vector3.zero, Vector3.zero, Quaternion.identity);
            }
            */

            //Locator.GetRingWorldController()._playerInsideRingWorld = true;

            // We don't want this one, this is for outside
            //var volume1 = GameObject.Find("SectorTrigger_Ringworld").GetComponent<OWTriggerVolume>();

            // This volume triggers ring interior to load
            var volumeLoad = GameObject.Find("RingInteriorSectorTriggerVolume").GetComponent<OWTriggerVolume>();

            // This volume triggers oxygen to load
            var volumeO2 = GameObject.Find("FluidOxygenVolume").GetComponent<OWTriggerVolume>();

            // TODO: ForceVolume seems to be the artificial gravity (centrifugal force) volume for the Stranger interior.
            // For some reason the player remains in the rotational frame when you teleport to another location --
            // Turns out this is an issue any time we add the player to volumes by hand. Gravity will remain whatever direction/strength it was.
            // We should resolve this at some point, for now it's just quirky I guess
            //var volume4 = GameObject.Find("ForceVolume").GetComponent<OWTriggerVolume>();

            // TODO: There's gotta be a better way than just adding ourselves to volumes, right?
            // There are multiple EntrywayTriggers that basically do this for us:
            // e.g. GameObject.Find("LightSide_InnerAirlock").GetComponent<EntrywayTrigger>();
            // but I don't know how they're used, unfortunately. Maybe we could experiment

            // Add player to trigger volume to load area
            volumeLoad.AddObjectToVolume(GameObject.Find("PlayerDetector"));
            //volumeLoad.AddObjectToVolume(GameObject.Find("CameraDetector"));

            // Add player to oxygen volume
            volumeO2.AddObjectToVolume(GameObject.Find("PlayerDetector"));
            //volumeO2.AddObjectToVolume(GameObject.Find("CameraDetector"));
        }

        public override void postSceneLoad()
        {

        }
    }

    /**
     * Custom Practice states. No override logic. All settings controled via menu
     */
    class PracticeStateCustom1 : PracticeState<PracticeStateCustom1> { }
    class PracticeStateCustom2 : PracticeState<PracticeStateCustom2> { }
    class PracticeStateCustom3 : PracticeState<PracticeStateCustom3> { }
}
