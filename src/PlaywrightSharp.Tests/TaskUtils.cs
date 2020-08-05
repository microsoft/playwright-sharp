using System.Collections.Generic;
using System.Threading.Tasks;
using PlaywrightSharp.Helpers;

namespace PlaywrightSharp.Tests
{
    internal static class TaskUtils
    {
        public static async Task<TResult[]> WhenAll<TResult>(IEnumerable<Task<TResult>> tasks)
        {
            var task = Task.WhenAll(tasks);
            await task.WithTimeout().ConfigureAwait(false);
            return task.Result;
        }

        public static Task WhenAll(IEnumerable<Task> tasks) => Task.WhenAll(tasks).WithTimeout();

        public static Task WhenAll(params Task[] tasks) => Task.WhenAll(tasks).WithTimeout();

        public static async Task<(T1, T2)> WhenAll<T1, T2>(Task<T1> task1, Task<T2> task2)
        {
            await Task.WhenAll(task1, task2).WithTimeout().ConfigureAwait(false);

            return (task1.Result, task2.Result);
        }

        public static async Task<T> WhenAll<T>(Task<T> task1, Task task2)
        {
            await Task.WhenAll(task1, task2).WithTimeout().ConfigureAwait(false);

            return task1.Result;
        }

        public static async Task<(T1, T2, T3)> WhenAll<T1, T2, T3>(Task<T1> task1, Task<T2> task2, Task<T3> task3)
        {
            await Task.WhenAll(task1, task2, task3).WithTimeout().ConfigureAwait(false);

            return (task1.Result, task2.Result, task3.Result);
        }

        public static async Task<(T1, T2)> WhenAll<T1, T2>(Task<T1> task1, Task<T2> task2, Task task3)
        {
            await Task.WhenAll(task1, task2, task3).WithTimeout().ConfigureAwait(false);

            return (task1.Result, task2.Result);
        }
    }
}
