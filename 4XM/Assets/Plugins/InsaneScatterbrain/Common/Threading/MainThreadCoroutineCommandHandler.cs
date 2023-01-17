using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;

namespace InsaneScatterbrain.Threading
{
    /// <summary>
    /// Basic handler of main thread commands. It queues the commands and executes any commands in the queue on update.
    /// </summary>
    public class MainThreadCoroutineCommandHandler : IMainThreadCoroutineCommandHandler
    {
        private readonly ConcurrentQueue<IMainThreadCoroutineCommand> queue = new ConcurrentQueue<IMainThreadCoroutineCommand>();
        private readonly WaitForNewCommand waitForNewCommand;

        public MainThreadCoroutineCommandHandler()
        {
            waitForNewCommand = new WaitForNewCommand(queue);
        }

        public void Queue(IMainThreadCoroutineCommand command, bool waitForCompletion = true)
        {
            queue.Enqueue(command);

            if (!waitForCompletion) return;
            
            command.WaitForCompletion();
        }

        /// <inheritdoc cref="IMainThreadCommandHandler.Update"/>
        public IEnumerator UpdateCoroutine()
        {
            while (true)
            {
                while (queue.Count == 0)
                {
                    yield return waitForNewCommand;  
                }

                IMainThreadCoroutineCommand command;
                while (!queue.TryDequeue(out command))
                {
                    yield return waitForNewCommand;  
                }
                
                var commandEnumerator = command.ExecuteCoroutine();
                if (commandEnumerator == null)  // This command doesn't support being executed in a coroutine, so we just execute it and move on.
                    continue;

                while (commandEnumerator.MoveNext())
                {
                    yield return commandEnumerator.Current;
                }
                
                yield return waitForNewCommand;
            }
        }
        
        private class WaitForNewCommand : CustomYieldInstruction
        {
            private readonly ConcurrentQueue<IMainThreadCoroutineCommand> queue;

            public WaitForNewCommand(ConcurrentQueue<IMainThreadCoroutineCommand> queue)
            {
                this.queue = queue;
            }

            public override bool keepWaiting => queue.Count == 0;
        }
    }
}
