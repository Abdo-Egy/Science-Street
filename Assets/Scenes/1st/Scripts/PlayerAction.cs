using AL_Arcade.DialogueSystem.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddPlayerAction : MonoBehaviour
{
    public void SetCurrentObjective(string action)
    {
        GameContextBuilder.Instance.SetCurrentObjective(action);
    }
    public void SetPlayerAction(string action)
    {
        GameContextBuilder.Instance.AddPlayerAction(action);
    }
}
