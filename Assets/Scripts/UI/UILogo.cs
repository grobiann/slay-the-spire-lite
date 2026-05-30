using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace STSLite.UI
{
    public class UILogo : UIBase
    {
        private const float LOGO_ANIMATION_LENGTH = 2.0f;

        public static async UniTask ShowLogo()
        {
            var uiLogo = UIManager.Instance.Show<UILogo>();
            await uiLogo.PlayLogoAnimation();
            UIManager.Instance.Close<UILogo>();
        }

        public async UniTask PlayLogoAnimation()
        {
            CancellationToken cancelToken = gameObject.GetCancellationTokenOnDestroy();
            await UIBlackScreen.Off(UIBlackScreen.DEFAULT_FADE_DURATION, cancelToken);
            await UniTask.Delay(TimeSpan.FromSeconds(LOGO_ANIMATION_LENGTH), cancellationToken: cancelToken);
            await UIBlackScreen.On(UIBlackScreen.DEFAULT_FADE_DURATION, cancelToken);
        }
    }
}
