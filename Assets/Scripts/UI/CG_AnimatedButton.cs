using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.UI.Button;

[RequireComponent(typeof(Animator))]
public class CG_AnimatedButton : UIBehaviour, IEventSystemHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Animator animator;

    [SerializeField]
    private ButtonClickedEvent m_OnClick = new ButtonClickedEvent();
    public void OnPointerDown(PointerEventData eventData)
    {
        if (animator != null) animator.Play("Default", 0);
        m_OnClick?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (animator != null) animator.Play("Animated", 0); 
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (animator != null) animator.Play("Default", 0); 
    }
}
