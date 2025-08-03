using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;

public class TimelineVCamDisabler : MonoBehaviour
{
    [Header("Target TimeLine")]
    [SerializeField] private PlayableDirector director;

    [Header("Vcam Lists")]
    [SerializeField] private List<GameObject> vcamsToDisable = new();

    private bool hasDisabled = false;

    void Update() {
        if (director == null || hasDisabled)
            return;

        if (director.state != PlayState.Playing) {
            foreach (var vcam in vcamsToDisable) {
                if (vcam != null)
                    vcam.SetActive(false);
            }

            Debug.Log("[VCamDisabler] Timeline ended, VCams deactivated.");
            hasDisabled = true;
        }
    }
}
