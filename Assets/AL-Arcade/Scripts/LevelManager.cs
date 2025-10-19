using EasyTransition;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] TransitionSettings transitionSettings;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void loadLevel(string levelName)
    {
        TransitionManager.Instance().Transition(levelName , transitionSettings , 0.5f);
    }

    public void loadLevel(int levelIndex)
    {
        TransitionManager.Instance().Transition(levelIndex , transitionSettings , 0.5f);
    }
}
