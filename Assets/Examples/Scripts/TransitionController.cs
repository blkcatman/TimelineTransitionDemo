#nullable enable

using UnityEngine;

public class TransitionController : MonoBehaviour
{
    [SerializeField]
    private Animator? destinationAnimator;

    [SerializeField]
    private string transitionName = "StartTransition";
    
    private Animator? sourceAnimator;
    
    private TimelineTransition? timelineTransition;

    public void OnButtonClicked()
    {
        if (timelineTransition != null && sourceAnimator != null && destinationAnimator != null)
        {
            if (sourceAnimator == destinationAnimator) return;
            
            StartTransition(sourceAnimator, destinationAnimator);
        }
    }
    
    private void StartTransition(in Animator source, in Animator destination)
    {
        timelineTransition?.StartTransition(source, destination, transitionName);
    }
    
    #region MonoBehaviour implements

    private void Start()
    {
        timelineTransition = FindObjectOfType<TimelineTransition>();

        var currentTransform = transform;
        while (currentTransform.parent != null)
        {
            var parent = currentTransform.parent.gameObject;
            var animator = parent.GetComponent<Animator>();
            if (animator != null)
            {
                sourceAnimator = animator;
                break;
            }

            currentTransform = currentTransform.parent;
        }
    }
    
    #endregion
}
