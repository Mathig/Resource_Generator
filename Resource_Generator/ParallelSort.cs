using System;
using System.Threading.Tasks;

namespace ParallelSort
{
    public static class Sort
    {
        private static void ParallelQuickSort<T>(T[] array, int from, int to, int depthRemaining) where T : IComparable
        {
            if (to - from > 1)
            {
                if (depthRemaining > 0)
                {
                    int pivot = from + (to - from) / 2; // could be anything, use middle
                    pivot = Partition<T>(array, from, to, pivot);
                    Parallel.Invoke(
                        () => ParallelQuickSort(array, from, pivot, depthRemaining - 1),
                        () => ParallelQuickSort(array, pivot + 1, to, depthRemaining - 1));
                }
                else
                {
                    T[] tempArray = new T[to - from];
                    for (int i = 0; i < to - from; i++)
                    {
                        tempArray[i] = array[from + i];
                    }
                    Array.Sort(tempArray);
                    for (int i = 0; i < to - from; i++)
                    {
                        array[from + i] = tempArray[i];
                    }
                }
            }
        }

        private static int Partition<T>(T[] array, int from, int to, int pivot) where T : IComparable
        {
            var arrayPivot = array[pivot];  // pivot value
            Swap(array, pivot, to - 1); // move pivot value to end for now, after this pivot not used
            var newPivot = from; // new pivot
            for (int i = from; i < to - 1; i++) // be careful to leave pivot value at the end
            {
                if (array[i].CompareTo(arrayPivot) != -1)
                {
                    Swap(array, newPivot, i);  // move value smaller than arrayPivot down to newpivot
                    newPivot++;
                }
            }
            Swap(array, newPivot, to - 1); // move pivot value to its final place
            return newPivot; // new pivot
        }

        /// <summary>
        /// Swaps two variables.
        /// </summary>
        /// <typeparam name="T">Type of variable to swap.</typeparam>
        /// <param name="array">Array containing variables.</param>
        /// <param name="i">First index to swap.</param>
        /// <param name="j">Second index to swap.</param>
        private static void Swap<T>(T[] array, int i, int j) where T : IComparable
        {
            var temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }

        public static void ParallelQuickSort<T>(T[] array) where T : IComparable
        {
            ParallelQuickSort(array, 0, array.Length,
                 (int)Math.Log(Environment.ProcessorCount, 2) + 4);
        }
    }
}