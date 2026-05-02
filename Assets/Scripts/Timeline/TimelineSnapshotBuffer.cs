using System;

namespace TimeTravelBanana.Timeline
{
    public class TimelineSnapshotBuffer<T>
    {
        private readonly float[] timeSnapshots;
        private readonly T[] stateSnapshots;
        private readonly int maxCapacity;
        private int snapshotCount;

        public int SnapshotCount => snapshotCount;
        public int MaxCapacity => maxCapacity;
        public bool IsFull => snapshotCount == maxCapacity;

        public TimelineSnapshotBuffer(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            maxCapacity = capacity;
            timeSnapshots = new float[capacity];
            stateSnapshots = new T[capacity];
        }

        public bool TryAppend(float time, T state)
        {
            if (snapshotCount == maxCapacity) 
                return false;
            timeSnapshots[snapshotCount] = time;
            stateSnapshots[snapshotCount] = state;
            snapshotCount++;

            return true;
        }


        public void Truncate(float time)
        {
            while (snapshotCount > 0 && timeSnapshots[snapshotCount - 1] > time) 
                snapshotCount--;
        }

        public struct Sample
        {
            public T Before;
            public T After;
            public float Alpha;
        }

        public bool TrySample(float time, out Sample sample)
        {
            sample = default;
            if (snapshotCount == 0) 
                return false;

            if (time <= timeSnapshots[0])
            {
                sample.Before = stateSnapshots[0];
                sample.After = stateSnapshots[0];
                sample.Alpha = 0f;
                return true;
            }
            else if (time >= timeSnapshots[snapshotCount - 1])
            {
                sample.Before = stateSnapshots[snapshotCount - 1];
                sample.After = stateSnapshots[snapshotCount - 1];
                sample.Alpha = 0f;
                return true;
            }

            int lo = 0;
            int hi = snapshotCount - 1;
            while (lo + 1 < hi)
            {
                int mid = (lo + hi) >> 1;
                if (timeSnapshots[mid] <= time) lo = mid;
                else hi = mid;
            }

            float t0 = timeSnapshots[lo];
            float t1 = timeSnapshots[hi];
            sample.Before = stateSnapshots[lo];
            sample.After = stateSnapshots[hi];
            sample.Alpha = t1 > t0 ? (time - t0) / (t1 - t0) : 0f;
            return true;
        }
    }
}
