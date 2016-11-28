using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Ben.Utilities
{

    public class AsyncLazy<T>
       : Lazy<ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter>
    {
        public AsyncLazy(Func<T> valueFactory)
        : base(() => Task.Run(valueFactory).ConfigureAwait(false).GetAwaiter(), LazyThreadSafetyMode.ExecutionAndPublication)
        {
        }

        public AsyncLazy(Func<Task<T>> taskFactory)
        : base(() => Task.Run(taskFactory).ConfigureAwait(false).GetAwaiter(), LazyThreadSafetyMode.ExecutionAndPublication)
        {
        }

        public ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter GetAwaiter()
        {
            return this.Value;
        }
    }
}