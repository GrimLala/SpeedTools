using UnityEngine;

namespace SpeedTools
{
    public class TreeLocatorRemover : MonoBehaviour
    {
        void Update()
        {
            if(Locator.GetEyeStateManager().GetState() >= EyeState.ForestIsDark)
            {
                this.gameObject.SetActive(false);
            }
        }
    }
}
