using UnityEngine;

public class TVManCutsceneScript : MonoBehaviour
{
    public Animator animator;

    public void PlayAnimation(CutsceneTVManAnimation cutsceneAnimation) 
    {
        if (animator != null) animator.Play(cutsceneAnimation.ToString(), 0);
    }

}

public enum CutsceneTVManAnimation 
{
    Default,
    Talk1,
    Talk2,
    Talk3
}
