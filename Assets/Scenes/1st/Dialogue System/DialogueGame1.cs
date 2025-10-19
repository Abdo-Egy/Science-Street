using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AL_Arcade.DialogueSystem.Scripts;

public class DialogueGame1 : MonoBehaviour
{
    [SerializeField] string GameName;
    [SerializeField] string GameDescription;
    [SerializeField] string GameMechanics;
    void Start()
    {
        GameContextBuilder.Instance.InitializeGame(GameName, GameDescription, GameMechanics);
    }
}
