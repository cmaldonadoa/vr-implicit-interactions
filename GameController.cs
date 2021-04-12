using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameController : MonoBehaviour
{
    GameStep currentStep;
    static SceneController1 instance;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {        
        if (currentStep.completed)
        {
            currentStep = currentStep.NextStep();
            currentStep.Init();
        }

        if (currentStep.disabled) return;
        currentStep.Update();
    }

    /// <summary>
    /// Helper method to run coroutines inside GameSteps.
    /// </summary>
    static void RunCoroutine(IEnumerator coroutine)
    {
        instance.StartCoroutine(coroutine);
    }
}
