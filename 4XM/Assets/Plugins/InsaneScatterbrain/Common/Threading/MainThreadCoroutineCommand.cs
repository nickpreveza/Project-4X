using System;
using System.Collections;
using System.Threading;

namespace InsaneScatterbrain.Threading
{
    public class MainThreadCoroutineCommand : IMainThreadCoroutineCommand
    {
        private readonly IEnumerator action;
        private readonly ManualResetEvent ready = new ManualResetEvent(false);
        
        public bool Done { get; private set; }

        public MainThreadCoroutineCommand(IEnumerator action)
        {
            this.action = action;
        }

        /// <inheritdoc cref="IMainThreadCommand.Execute"/>
        public IEnumerator ExecuteCoroutine()
        {
            while (action.MoveNext())
            {
                yield return action.Current;
            }

            ready.Set();
            Done = true;
        }

        public void Execute()
        {
            // Not being executed as a coroutine, so do all the work in one go.
            while (action.MoveNext()) { }
            
            ready.Set();
            Done = true;
        }

        /// <summary>
        /// Makes the calling thread wait for the completion of this command.
        /// </summary>
        /// <exception cref="Exception">
        /// If an exception occurred while executing the command, it will be rethrown here to the calling thread.
        /// </exception>
        public void WaitForCompletion()
        {
            ready.Reset();
            ready.WaitOne();
        }
    }
}
