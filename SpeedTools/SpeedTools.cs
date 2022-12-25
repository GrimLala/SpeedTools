using OWML.Common;
using OWML.ModHelper;
using UnityEngine.InputSystem;
using UnityEngine;
using PacificEngine.OW_CommonResources.Game.State;
using PacificEngine.OW_CommonResources.Game.Config;
using PacificEngine.OW_CommonResources.Game.Player;
using PacificEngine.OW_CommonResources.Game.Resource;
using System;

namespace SpeedTools
{
    enum SpeedToolOptions
    {
        Practice_Statue_Skip,
        Practice_Twin_Flight,
        Practice_Bramble_Entry,
        Practice_Vessel_Activation,
        Practice_Vessel_Clip,
        Practice_Instrument_Hunt,
        Practice_Stranger,
        Practice_Custom_1,
        Practice_Custom_2,
        Practice_Custom_3,
        Toggle_Infinite_Fuel,
        Toggle_Speedup,
        Toggle_Tree_Locator,
        Log_Custom_State_Info,
        Debug
    }

    public class SpeedTools : ModBehaviour
    {
        private string version = "";
        private ScreenPrompt speedtoolsTagger = new ScreenPrompt("");
        private ScreenPrompt speedtoolsTimer = new ScreenPrompt("");
        private ScreenPrompt speedtoolsFuel = new ScreenPrompt("");
        bool speedtoolsEnabled = true;

        void OnGUI()
        {

            /*
             * Display an indicator that the mod is active
             */
            speedtoolsTagger.SetText("SpeedTools v" + version + ": " + (speedtoolsEnabled ? "Enabled" : "Disabled"));
            if (Locator.GetPromptManager()?.GetScreenPromptList(PromptPosition.LowerLeft)?.Contains(speedtoolsTagger) == false)
            {
                Locator.GetPromptManager().AddScreenPrompt(speedtoolsTagger, PromptPosition.LowerLeft, true);
            }

            /*
            * Display an indicator that infinite fuel cheat is active
            */
            if (Player.hasUnlimitedFuel)
            {
                speedtoolsFuel.SetText("Infinite Fuel On");
            }
            else
            {
                speedtoolsFuel.SetText("");
            }

            if (Locator.GetPromptManager()?.GetScreenPromptList(PromptPosition.UpperLeft)?.Contains(speedtoolsFuel) == false)
            {
                Locator.GetPromptManager().AddScreenPrompt(speedtoolsFuel, PromptPosition.UpperLeft, true);
            }

            /*
             * Display a timer similar to the existing OWClock mod
             */
            var elapsed = TimeLoop.GetSecondsElapsed();
            
            // Check to see if a time loop is active; if not, then the rest of OnGUI() will not execute
            if (elapsed < 1f)  // || !TimeLoop.IsTimeLoopEnabled())
                               // This bit makes the loop timer freeze (even though time is still flowing) after warping out of the Ash Twin Project
            {
                return;
            }

            // If a time loop is active, then display the current loop time in minutes and seconds;
            // also in [seconds] so the user will easily know what number to enter for custom sleep times in the config menu
            speedtoolsTimer.SetText("Loop Time: " + ParseTime(elapsed) + " [" + Math.Truncate(elapsed).ToString() + "]");
            if (Locator.GetPromptManager()?.GetScreenPromptList(PromptPosition.LowerLeft)?.Contains(speedtoolsTimer) == false)
            {
                Locator.GetPromptManager().AddScreenPrompt(speedtoolsTimer, PromptPosition.LowerLeft, true);
            }

        }

        /*
         * State tracking variables
         */
        bool loadPracticeState = false;
        float wakeupTime = 0f;
        IPracticeState practiceTarget = null;
        Action postSceneMethod = null;

        // There's probably a better way to do this than having this static, but I've got the brain mush right now
        static GameObject treeLocator = null;

        /*
         * Create config menu options
         */
        InputMapping<SpeedToolOptions> inputs = new InputMapping<SpeedToolOptions>();
        public override void Configure(IModConfig config)
        {
            inputs.Clear();
            inputs.addInput(config, SpeedToolOptions.Practice_Statue_Skip, "Backslash,Digit2");  // 1 and 4 should be avoided for anything involving the scout launcher as this switches to photo mode
            inputs.addInput(config, SpeedToolOptions.Practice_Twin_Flight, "Backslash,Digit1");
            inputs.addInput(config, SpeedToolOptions.Practice_Bramble_Entry, "Backslash,Digit3");
            inputs.addInput(config, SpeedToolOptions.Practice_Vessel_Activation, "Backslash,Digit4");
            inputs.addInput(config, SpeedToolOptions.Practice_Vessel_Clip, "Backslash,Digit5");
            inputs.addInput(config, SpeedToolOptions.Practice_Instrument_Hunt, "Backslash,Digit6");
            inputs.addInput(config, SpeedToolOptions.Practice_Stranger, "Backslash,7");
            inputs.addInput(config, SpeedToolOptions.Practice_Custom_1, "Backslash,Digit8");
            inputs.addInput(config, SpeedToolOptions.Practice_Custom_2, "Backslash,Digit9");
            inputs.addInput(config, SpeedToolOptions.Practice_Custom_3, "Backslash,Digit0");

            inputs.addInput(config, SpeedToolOptions.Toggle_Infinite_Fuel, "RightBracket,Digit2");
            inputs.addInput(config, SpeedToolOptions.Toggle_Speedup, "RightBracket,Digit0");
            inputs.addInput(config, SpeedToolOptions.Toggle_Tree_Locator, "RightBracket,Digit6");

            inputs.addInput(config, SpeedToolOptions.Log_Custom_State_Info, "Backslash,RightBracket");

            inputs.addInput(config, SpeedToolOptions.Debug, "LeftBracket,RightBracket");

            // Statue Skip
            PracticeStateStatueSkip.Instance.setInfiniteFuel(ConfigHelper.getConfigOrDefault<bool>(config, "Statue Skip Infinite Fuel", true));
            PracticeStateStatueSkip.Instance.setFreezeSuperNova(ConfigHelper.getConfigOrDefault<bool>(config, "Statue Skip Freeze Supernova", true));

            // Twin Flight
            PracticeStateTwinFlight.Instance.setWaitTime(ConfigHelper.getConfigOrDefault<int>(config, "Practice Twin Flight Start Time", 410));
            PracticeStateTwinFlight.Instance.setSpaceSuit(ConfigHelper.getConfigOrDefault<bool>(config, "Twin Flight Spacesuit", false));
            
            // Bramble Entry
            PracticeStateBrambleEntry.Instance.setWaitTime(ConfigHelper.getConfigOrDefault<int>(config, "Practice Bramble Entry Start Time", 488));

            // Vessel Activation
            PracticeStateVesselActivation.Instance.setInfiniteFuel(ConfigHelper.getConfigOrDefault<bool>(config, "Vessel Activation Infinite Fuel", false));
            PracticeStateVesselActivation.Instance.setEyeCoordinates(ConfigHelper.getConfigOrDefault<bool>(config, "Vessel Activation Display Coordinates", true));

            // Vessel Clip
            PracticeStateVesselClip.Instance.setInfiniteFuel(ConfigHelper.getConfigOrDefault<bool>(config, "Vessel Clip Infinite Fuel", false));
            PracticeStateVesselClip.Instance.setTeleportPosition(ConfigHelper.getConfigOrDefault<string>(config, "Vessel Clip Teleport Target Position", "175.44, 13.38, -19.37"));

            // Instrument Hunt
            PracticeStateInstrumentHunt.Instance.setInfiniteFuel(ConfigHelper.getConfigOrDefault<bool>(config, "Instrument Hunt Infinite Fuel", false));
            PracticeStateInstrumentHunt.Instance.setTreeLocator(ConfigHelper.getConfigOrDefault<bool>(config, "Instrument Hunt Tree Locator", false));

            // Stranger
            PracticeStateStranger.Instance.setWaitTime(ConfigHelper.getConfigOrDefault<int>(config, "Stranger Start Time", 0));
            PracticeStateStranger.Instance.setInfiniteFuel(ConfigHelper.getConfigOrDefault<bool>(config, "Stranger Infinite Fuel", false));
            PracticeStateStranger.Instance.setTeleportPosition(ConfigHelper.getConfigOrDefault<string>(config, "Stranger Teleport Target Position", "49.54, -77.79, -293.29"));

            // Custom 1
            PracticeStateCustom1.Instance.setWaitTime(ConfigHelper.getConfigOrDefault<int>(config, "Custom 1 Start Time", 0));
            PracticeStateCustom1.Instance.setSpaceSuit(ConfigHelper.getConfigOrDefault<bool>(config, "Custom 1 Spacesuit", false));
            PracticeStateCustom1.Instance.setInfiniteFuel(ConfigHelper.getConfigOrDefault<bool>(config, "Custom 1 Infinite Fuel", false));
            PracticeStateCustom1.Instance.setFreezeSuperNova(ConfigHelper.getConfigOrDefault<bool>(config, "Custom 1 Freeze Supernova", false));
            PracticeStateCustom1.Instance.setTeleportBody(ConfigHelper.getConfigOrDefault<string>(config, "Custom 1 Teleport Target Body", null));
            PracticeStateCustom1.Instance.setTeleportPosition(ConfigHelper.getConfigOrDefault<string>(config, "Custom 1 Teleport Target Position", null));

            // Custom 2
            PracticeStateCustom2.Instance.setWaitTime(ConfigHelper.getConfigOrDefault<int>(config, "Custom 2 Start Time", 0));
            PracticeStateCustom2.Instance.setSpaceSuit(ConfigHelper.getConfigOrDefault<bool>(config, "Custom 2 Spacesuit", false));
            PracticeStateCustom2.Instance.setInfiniteFuel(ConfigHelper.getConfigOrDefault<bool>(config, "Custom 2 Infinite Fuel", false));
            PracticeStateCustom2.Instance.setFreezeSuperNova(ConfigHelper.getConfigOrDefault<bool>(config, "Custom 2 Freeze Supernova", false));
            PracticeStateCustom2.Instance.setTeleportBody(ConfigHelper.getConfigOrDefault<string>(config, "Custom 2 Teleport Target Body", null));
            PracticeStateCustom2.Instance.setTeleportPosition(ConfigHelper.getConfigOrDefault<string>(config, "Custom 2 Teleport Target Position", null));

            // Custom 3
            PracticeStateCustom3.Instance.setWaitTime(ConfigHelper.getConfigOrDefault<int>(config, "Custom 3 Start Time", 0));
            PracticeStateCustom3.Instance.setSpaceSuit(ConfigHelper.getConfigOrDefault<bool>(config, "Custom 3 Spacesuit", false));
            PracticeStateCustom3.Instance.setInfiniteFuel(ConfigHelper.getConfigOrDefault<bool>(config, "Custom 3 Infinite Fuel", false));
            PracticeStateCustom3.Instance.setFreezeSuperNova(ConfigHelper.getConfigOrDefault<bool>(config, "Custom 3 Freeze Supernova", false));
            PracticeStateCustom3.Instance.setTeleportBody(ConfigHelper.getConfigOrDefault<string>(config, "Custom 3 Teleport Target Body", null));
            PracticeStateCustom3.Instance.setTeleportPosition(ConfigHelper.getConfigOrDefault<string>(config, "Custom 3 Teleport Target Position", null));

        }

        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        private void Start()
        {
            version = ModHelper.Manifest.Version;

            // Example of accessing game code.
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                // Some practice states need to change the scene. This allows them to call more code after the scene is done loading
                if(postSceneMethod != null)
                {
                    // Wait until next update to ensure the game has done all it's shit first
                    ModHelper.Events.Unity.FireOnNextUpdate(postSceneMethod);
                    postSceneMethod = null;
                }

                //if (loadScene != OWScene.SolarSystem) return;
                //ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);

            };
        }

        public void Update()
        {
            inputs.Update();
            var currentFrame = inputs.getPressedThisFrame();
            currentFrame = currentFrame.FindAll(x => x.Item2.keyMatchCount() == currentFrame[0].Item2.keyMatchCount());

            foreach (var input in currentFrame)
            {
                switch (input.Item1)
                {
                    case SpeedToolOptions.Practice_Statue_Skip:
                        practice_StatueSkip();
                        break;
                    case SpeedToolOptions.Practice_Twin_Flight:
                        practice_TwinFlight();
                        break;
                    case SpeedToolOptions.Practice_Bramble_Entry:
                        practice_BrambleEntry();
                        break;
                    case SpeedToolOptions.Practice_Vessel_Activation:
                        practice_VesselActivation();
                        break;
                    case SpeedToolOptions.Practice_Vessel_Clip:
                        practice_VesselClip();
                        break;
                    case SpeedToolOptions.Practice_Instrument_Hunt:
                        practice_InstrumentHunt();
                        break;
                    case SpeedToolOptions.Practice_Stranger:
                        practice_Stranger();
                        break;
                    case SpeedToolOptions.Practice_Custom_1:
                        practice_Custom1();
                        break;
                    case SpeedToolOptions.Practice_Custom_2:
                        practice_Custom2();
                        break;
                    case SpeedToolOptions.Practice_Custom_3:
                        practice_Custom3();
                        break;
                    case SpeedToolOptions.Toggle_Infinite_Fuel:
                        toggleInfiniteFuel();
                        break;
                    case SpeedToolOptions.Toggle_Speedup:
                        toggleSpeedup();
                        break;
                    case SpeedToolOptions.Toggle_Tree_Locator:
                        toggleTreeLocator();
                        break;
                    case SpeedToolOptions.Log_Custom_State_Info:
                        logCurrentState();
                        break;
                    case SpeedToolOptions.Debug:
                        doDebug();
                        break;
                    default:
                        break;
                }
            }

            /*
             * Logic for loading a practice state
             * If there is a practice state to load, accelerate time until the wakeup time and
             * then run the postWait method on the practice state
             * Currently assuming that the preWait has already been called and wakeupTime has been set
             */
            if (loadPracticeState)
            {
                float elapsedSecs = TimeLoop.GetSecondsElapsed();
                if (wakeupTime <= elapsedSecs)
                {
                    // Set time back to normal
                    Time.timeScale = 1f;

                    // Run postWait
                    practiceTarget.postWait();

                    // TODO: Write message to screen that waiting is over

                    // clear the practice target and load flag
                    practiceTarget = null;
                    loadPracticeState = false;
                }
                // Don't want to overshoot the wakeup time, so decrease the timescale as it gets closer
                else if ((wakeupTime - elapsedSecs < Time.timeScale) && Time.timeScale > 2)
                {
                    Time.timeScale /= 2;
                }
            }
        }

        private void practice_StatueSkip()
        {
            loadPractice(PracticeStateStatueSkip.Instance);
        }

        private void practice_VesselActivation()
        {
            loadPractice(PracticeStateVesselActivation.Instance);
        }

        private void practice_VesselClip()
        {
            loadPractice(PracticeStateVesselClip.Instance);
        }

        private void practice_TwinFlight()
        {
            loadPractice(PracticeStateTwinFlight.Instance);
        }

        private void practice_BrambleEntry()
        {
            loadPractice(PracticeStateBrambleEntry.Instance);
        }

        private void practice_InstrumentHunt()
        {
            loadPractice(PracticeStateInstrumentHunt.Instance);
        }

        private void practice_Stranger()
        {
            loadPractice(PracticeStateStranger.Instance);
        }

        private void practice_Custom1()
        {
            loadPractice(PracticeStateCustom1.Instance);
        }

        private void practice_Custom2()
        {
            loadPractice(PracticeStateCustom2.Instance);
        }

        private void practice_Custom3()
        {
            loadPractice(PracticeStateCustom3.Instance);
        }

        /*
         * Output information necessary for creating a custom state out to the log
         */
        private void logCurrentState()
        {
            var playerAbsoluteState = PositionState.fromCurrentState(Locator.GetPlayerBody());

            // getClosestInfluence returns a list sorted by distance, we want the closest so we grab the first item from the list
            var parent = Position.getClosetInfluence(playerAbsoluteState.position, Position.getAstros(), new HeavenlyBody[0])[0].Item1;

            var playerRelativeState = RelativeState.getSurfaceMovement(parent, Locator.GetPlayerBody());

            string output = "STATE INFO \n"
                + "Loop time: " + Math.Truncate(TimeLoop.GetSecondsElapsed()) + "\n"
                + "Parent body: " + HeavenlyBodyHelper.heavenlyBodyToHumanText(parent) + "\n"
                + "Position: " + formatVector(playerRelativeState?.position ?? Vector3.zero);

            ModHelper.Console.WriteLine(output, MessageType.Success);
        }

        /*
         * Debug method for outputting whatever the hell info is needed at the time
         */
        private void doDebug()
        {
            // This provides a reliable, reproducable location and rotation for placing the ship for practicing bramble entry
            // Current state has the ship flat on the bridge but if a different orientation is neede this will be helpful
            var bridge = GameObject.Find("Structure_HT_TT_Bridge").transform;
            var ship = Locator.GetShipBody().transform;
            var shipRelPos = bridge.InverseTransformPoint(ship.position);
            string output =
                "Position:" + shipRelPos + "\n"
                + "Rotation:" + (Quaternion.Inverse(bridge.rotation) * ship.rotation);

            ModHelper.Console.WriteLine(output, MessageType.Success);
        }

        /*
         * Load a practice state
         */
        private void loadPractice(IPracticeState practiceState)
        {
            loadPracticeState = true;
            practiceTarget = practiceState;
            postSceneMethod = practiceTarget.postSceneLoad;

            practiceTarget.preWait();

            waitUntil(practiceTarget.getWaitTime());
        }

        private void waitUntil(float secondsIntoLoop)
        {
            wakeupTime = secondsIntoLoop;

            // No wait needed
            if (wakeupTime == 0)
            {
                return;
            }
            //Make sure target time hasn't already passed
            else if (wakeupTime < TimeLoop.GetSecondsElapsed())
            {
                ModHelper.Console.WriteLine("Wait time already passed: " + secondsIntoLoop, MessageType.Success);
                return;
            }

            else if (wakeupTime > 0 && (!TimeLoop._isTimeFlowing || SuperNova.freeze))
            {
                // This doesn't account for everything: Vessel clip stops time but still will allow acceleration
                ModHelper.Console.WriteLine("Time is no longer flowing, cannot accelerate", MessageType.Success);
                return;
            }

            Time.timeScale = 50f;
            ModHelper.Console.WriteLine("Waiting until: " + wakeupTime, MessageType.Success);
        }

        /*
         * Convert a timestamp to a clock format string
         */
        static string ParseTime(float timestamp)
        {
            var minutes = Mathf.Floor(timestamp / 60f).ToString().PadLeft(2, '0');
            var seconds = Math.Truncate(timestamp % 60f).ToString().PadLeft(2, '0');
            var clock = $"{minutes}:{seconds}";
            return clock;
        }

        /*
         * Convert a vector to a more human readable format with rounding. We don't need down to the 5th decimal place.
         */
        static string formatVector(Vector3 v)
        {
            return "(" + Math.Round(v.x, 2) + ", " + Math.Round(v.y, 2) + ", " + Math.Round(v.z, 2) + ")";
        }

        /*
         * Turn infinite fuel on/off for the player's space suit
         */
        static public void toggleInfiniteFuel()
        {
            Player.hasUnlimitedFuel = !Player.hasUnlimitedFuel;
        }

        /*
         * Turn time acceleration on/off
         * Accelerated time scale is set to 50, but most systems probably won't be able to reach that speed as the effective
         * time scale is limited by how fast the CPU can perform all the calculations necessary in this game
         */
        static public void toggleSpeedup()
        {
            if (Time.timeScale == 1f)
            {
                Time.timeScale = 50f;
            } else
            {
                Time.timeScale = 1f;
            }
        }

        /*
         * Turn quantum tree indicator on/off
         */
        static public void toggleTreeLocator()
        {
            if(treeLocator == null || !treeLocator.activeSelf)
            {
                toggleTreeLocator(true);
            } else
            {
                toggleTreeLocator(false);
            }
        }

        /*
         * Turn quantum tree indicator on/off
         */
        static public void toggleTreeLocator(bool state)
        {
            // If we haven't created the locator yet, do that now
            if (treeLocator == null)
            {
                var fog = GameObject.Find("Sector_ForestOfGalaxies").GetComponent<Transform>();

                var locatorObject = Resources.FindObjectsOfTypeAll<DistantSunController>()[0].gameObject;

                if (locatorObject)
                {
                    treeLocator = (GameObject)UnityEngine.Object.Instantiate(locatorObject, fog, false);

                    // The controller has code that deactivates the object. Kill it dead
                    Destroy(treeLocator.GetComponent<DistantSunController>());

                    treeLocator.name = "TreeTriangleLocator";
                    treeLocator.transform.localPosition = new Vector3(-55f, 0f, 0f);
                    treeLocator.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                    
                }
            }

            treeLocator.SetActive(state);
        }

        private bool GetKey(Key keyCode)
        {
            return Keyboard.current[keyCode].IsPressed();
        }

        private bool GetKeyDown(Key keyCode)
        {
            return Keyboard.current[keyCode].wasPressedThisFrame;
        }

        private bool GetKeyUp(Key keyCode)
        {
            return Keyboard.current[keyCode].wasReleasedThisFrame;
        }
    }
}