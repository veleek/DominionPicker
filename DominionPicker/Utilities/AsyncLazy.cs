using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Ben.Dominion.Utilities
{
    public class AsyncLazy<T> : Lazy<ConfiguredTaskAwaitable<T>>
    {
        public AsyncLazy(Func<T> valueFactory) :
            base(() => Task.Factory.StartNew(valueFactory).ConfigureAwait(false), LazyThreadSafetyMode.ExecutionAndPublication)
        {
        }

        public AsyncLazy(Func<Task<T>> taskFactory) :
            base(() => Task.Factory.StartNew(taskFactory).Unwrap().ConfigureAwait(false), LazyThreadSafetyMode.ExecutionAndPublication)
        {
        }

        public ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter GetAwaiter()
        {
            return this.Value.GetAwaiter();
        }
    }
}