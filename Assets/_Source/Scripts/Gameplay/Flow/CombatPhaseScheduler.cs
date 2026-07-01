using System;
using System.Collections;
using UnityEngine;

namespace MagicArcher.Gameplay.Flow
{
    public sealed class CombatPhaseScheduler : MonoBehaviour
    {
        public void RunAfterDelay(float delaySeconds, Action action)
        {
            if (action == null)
                return;

            if (delaySeconds <= 0f)
            {
                action.Invoke();
                return;
            }

            StartCoroutine(RunAfterDelayRoutine(delaySeconds, action));
        }

        static IEnumerator RunAfterDelayRoutine(float delaySeconds, Action action)
        {
            yield return new WaitForSeconds(delaySeconds);
            action?.Invoke();
        }
    }
}
