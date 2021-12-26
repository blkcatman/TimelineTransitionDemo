#nullable enable

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[RequireComponent(typeof(PlayableDirector), typeof(SignalAsset))]
public class TimelineTransition : MonoBehaviour
{
    [SerializeField]
    private PlayableDirector? playableDirector;

    [SerializeField]
    private string startMarkerPrefix = "Start";

    [SerializeField]
    private string stopMarkerPrefix = "End";

    [SerializeField]
    private string sourceTrackName = "TransitionA";

    [SerializeField]
    private string destinationTrackName = "TransitionB";

    private readonly Dictionary<string, double> startMarkerTimes = new Dictionary<string, double>();

    private TrackAsset? sourceTrackAsset;
    private TrackAsset? destinationTrackAsset;

    public IReadOnlyList<string>? StartMarkers => startMarkerTimes.Keys.Any() ? startMarkerTimes.Keys.ToList() : null;

    private GameObject? targetObjectToDisable;

    public void StartTransition(
        Animator? sourceAnimator, 
        Animator? destinationAnimator, 
        string transitionName,
        bool disableSourceAfterTransition = false,
        bool enableDestinationBeforeTransition = false)
    {
        if (playableDirector == null) return;
        if (playableDirector.state == PlayState.Playing) return;
        
        if (sourceTrackAsset != null && sourceAnimator != null)
        {
            targetObjectToDisable = disableSourceAfterTransition ? sourceAnimator.gameObject : null;
            playableDirector.SetGenericBinding(sourceTrackAsset, sourceAnimator);
        }

        if (destinationTrackAsset != null && destinationAnimator != null)
        {
            if (enableDestinationBeforeTransition)
            {
                destinationAnimator.gameObject.SetActive(true);
            }
            playableDirector.SetGenericBinding(destinationTrackAsset, destinationAnimator);
        }

        StartAnimationAtMarker(transitionName);
    }

    private void StartAnimationAtMarker(string markerName)
    {
        if (startMarkerTimes.ContainsKey(markerName))
        {
            if (playableDirector == null) return;
            playableDirector.time = startMarkerTimes[markerName];
            playableDirector.Play();
        }
    }

    #region MonoBehaviour implements

    private void Reset()
    {
        playableDirector = gameObject.GetComponent<PlayableDirector>();
    }

    private void Awake()
    {
        if (playableDirector == null) return;

        var stopTimelineEvent = new UnityEvent();
        stopTimelineEvent.AddListener(() =>
        {
            targetObjectToDisable?.SetActive(false);
            targetObjectToDisable = null;
            playableDirector?.Stop();
        });

        var signalReceiver = gameObject.GetComponent<SignalReceiver>();

        var timelineAsset = playableDirector?.playableAsset as TimelineAsset;
        foreach (var marker in timelineAsset!.markerTrack.GetMarkers())
        {
            var signalEmitter = marker as SignalEmitter;
            if (signalEmitter == null) continue;

            var signalAsset = signalEmitter.asset;
            if (signalAsset.name.Contains(startMarkerPrefix))
            {
                startMarkerTimes.Add(signalAsset.name, marker.time);
            }

            if (signalAsset.name.Contains(stopMarkerPrefix))
            {
                if (!signalReceiver.GetRegisteredSignals().Contains(signalAsset))
                {
                    signalReceiver.AddReaction(signalAsset, stopTimelineEvent);
                }
            }
        }

        foreach (var trackAsset in timelineAsset!.GetRootTracks())
        {
            if (trackAsset as AnimationTrack == null) continue;
            if (trackAsset.name.Contains(sourceTrackName))
            {
                sourceTrackAsset = trackAsset;
            }

            if (trackAsset.name.Contains(destinationTrackName))
            {
                destinationTrackAsset = trackAsset;
            }
        }
    }

    #endregion
}
