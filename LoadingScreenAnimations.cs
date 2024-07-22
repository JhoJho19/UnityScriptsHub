using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class LoadingScreenAnimations : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private RectTransform rotatedPart; // Place an object here that will rotate when loading. For example, a round logo.
    private CancellationTokenSource cancellationTokenSource;

    private void Start()
    {
        cancellationTokenSource = new CancellationTokenSource();
        StartLoadingAnimation(cancellationTokenSource.Token).Forget();
        StartRotateAnimation(cancellationTokenSource.Token).Forget();
    }

    private async UniTaskVoid StartLoadingAnimation(CancellationToken cancellationToken)
    {
        string[] loadingStates = { "Loading", "Loading.", "Loading..", "Loading..." };
        int index = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            loadingText.text = loadingStates[index];
            index = (index + 1) % loadingStates.Length;
            try
            {
                await UniTask.Delay(500, cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async UniTaskVoid StartRotateAnimation(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            rotatedPart.Rotate(0, 0, -1);
            try
            {
                await UniTask.Delay(50, cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private void OnDestroy()
    {
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }
}
