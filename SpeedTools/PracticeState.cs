using PacificEngine.OW_CommonResources.Game.Player;
using PacificEngine.OW_CommonResources.Game.Resource;
using PacificEngine.OW_CommonResources.Game.State;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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

        public void setTeleportPosition(string vector)
        {
            if(vector == null)
            {
                this.teleportPosition = Vector3.negativeInfinity;
                return;
            }

            string[] parts = vector.Split(',');

            if(parts.Length != 3)
            {
                // Throwing an exception breaks everything. Find a better way to handle this
                //throw new Exception("Unable to parse position from [" + vector + "]. Should be in format [0.0, 0.0, 0.0] ");

                this.teleportPosition = Vector3.negativeInfinity;
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
    }

    /**
     * Practice state for flying to Ash Twin starting from initial wake-up
     */
    class PracticeStateTwinFlight : PracticeState<PracticeStateTwinFlight>
    {
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
                Teleportation.teleportObjectTo(Locator.GetShipBody(), parent, new Vector3(-9.268302f, -1.562688f, -128.2221f), Vector3.zero, Vector3.zero, Vector3.zero, Quaternion.identity);
            }
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
     * Custom Practice states. No override logic. All settings controled via menu
     */
    class PracticeStateCustom1 : PracticeState<PracticeStateCustom1> { }
    class PracticeStateCustom2 : PracticeState<PracticeStateCustom2> { }
    class PracticeStateCustom3 : PracticeState<PracticeStateCustom3> { }
}
