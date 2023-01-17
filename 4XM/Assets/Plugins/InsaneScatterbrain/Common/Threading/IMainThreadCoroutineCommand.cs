using System.Collections;

namespace InsaneScatterbrain.Threading
{
    public interface IMainThreadCoroutineCommand : IMainThreadCommand
    {
        IEnumerator ExecuteCoroutine();
    }
}