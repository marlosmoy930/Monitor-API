using System;
using System.Collections.Generic;

namespace MonitorQA.Api.Modules.Reports.Models
{
    public abstract class CountableItem<T>
    {
        public int Count { get; set; }

        protected abstract Func<T, bool> TryAddPredicate { get; }

        public bool TryAdd(T value)
        {
            var shouldAdd = TryAddPredicate(value);
            if (shouldAdd)
            {
                Count++;
                return true;
            }

            return false;
        }
    }

    public static class CountableUtil
    {
        public static void TryAdd<TCountable, TValue>(this List<TCountable> items, TValue value)
            where TCountable : CountableItem<TValue>
        {
            foreach (var item in items)
            {
                var isAdded = item.TryAdd(value);
                if (isAdded) break;
            }
        }
    }
}