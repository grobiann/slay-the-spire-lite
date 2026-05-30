using Cysharp.Threading.Tasks;
using STSLite.Core.Models;

namespace STSLite.Core
{
    public static class OneTimeInitialization
    {
        public static void ExecuteEssential()
        {
            DefinitionDB.Init();
        }
    }


    public class PreloadManager
    {

        public static async UniTask LoadRunAssets()
        {
        }

        public static async UniTask LoadActAssets(ActDefinition act)
        {
        }
    }
}