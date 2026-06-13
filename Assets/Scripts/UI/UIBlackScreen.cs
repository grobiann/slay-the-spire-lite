using Cysharp.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace STSLite.UI
{
    public class UIBlackScreen : UIBase
    {
        public static UIBlackScreen Instance { get; private set; }

        public const float DEFAULT_FADE_DURATION = 0.5f;

        [SerializeField] private CanvasGroup _canvasGroup;

        public static async UniTask On(float duration = DEFAULT_FADE_DURATION, CancellationToken cancelToken = default)
        {
            UIBlackScreen transition = Instance ?? UIManager.Instance.Show<UIBlackScreen>();
            await transition.FadeInternal(true, duration, cancelToken);
        }

        public static async UniTask Off(float duration = DEFAULT_FADE_DURATION, CancellationToken cancelToken = default)
        {
            UIBlackScreen transition = Instance ?? UIManager.Instance.Show<UIBlackScreen>();
            await transition.FadeInternal(false, duration, cancelToken);
        }

        private async UniTask FadeInternal(bool onOff, float duration, CancellationToken cancelToken)
        {
            float startAlpha = onOff ? 0f : 1f;
            float endAlpha = onOff ? 1f : 0f;
            _canvasGroup.alpha = startAlpha;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    UIManager.Instance.Close<UIBlackScreen>();
                    return;
                }

                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
                await UniTask.Yield();
            }

            _canvasGroup.alpha = endAlpha;
            if (!onOff)
            {
                UIManager.Instance.Close<UIBlackScreen>();
            }
        }
    }
}