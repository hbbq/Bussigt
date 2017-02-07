using System;
using System.Threading.Tasks;
using System.Threading;

namespace Bussigt.App.Extensions
{

    internal static class TaskFactoryExtensions
    {

        public static Task StartNewTaskContinuously(this TaskFactory @this, Action action, CancellationToken cancellationToken, TimeSpan timeSpan)
        {
            return @this.StartNew(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    action();

                    await Task.Delay(timeSpan);
                }
            });
        }

    }

}