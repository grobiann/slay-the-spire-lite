using STSLite.Core.Models;

namespace STSLite.UI
{
    public class UIMapScreen : UIBase
    {
        public static UIMapScreen Instance { get; private set; }
        //[SerializeField] private CanvasGroup _canvasGroup;
        //public async UniTask ShowMapScreen(CancellationToken cancelToken = default)
        //{
        //    _canvasGroup.alpha = 1f;
        //    _canvasGroup.interactable = true;
        //    _canvasGroup.blocksRaycasts = true;
        //    await UniTask.Yield();
        //}
        //public async UniTask HideMapScreen(CancellationToken cancelToken = default)
        //{
        //    _canvasGroup.alpha = 0f;
        //    _canvasGroup.interactable = false;
        //    _canvasGroup.blocksRaycasts = false;
        //    await UniTask.Yield();
        //}

        public void SetMap(ActMap map, string seed)
        {

        }

        public void ClearDrawings()
        {

        }
    }
}
