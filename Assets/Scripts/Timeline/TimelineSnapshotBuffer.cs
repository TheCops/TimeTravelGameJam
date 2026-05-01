using System;

namespace TimeTravelBanana.Timeline
{
    public class TimelineSnapshotBuffer<T>
    {
        private readonly float[] _times;
        private readonly T[] _states;
        private readonly int _capacity;
        private int _head;
        private int _count;

        public int Count => _count;
        public int Capacity => _capacity;
        public float OldestTime => _count > 0 ? _times[OldestIndex()] : 0f;
        public float NewestTime => _count > 0 ? _times[NewestIndex()] : 0f;

        public TimelineSnapshotBuffer(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _capacity = capacity;
            _times = new float[capacity];
            _states = new T[capacity];
        }

        public void Append(float time, T state)
        {
            _times[_head] = time;
            _states[_head] = state;
            _head = (_head + 1) % _capacity;
            if (_count < _capacity) _count++;
        }

        public void Clear()
        {
            _head = 0;
            _count = 0;
        }

        public void Truncate(float time)
        {
            while (_count > 0 && _times[NewestIndex()] > time)
            {
                _head = (_head - 1 + _capacity) % _capacity;
                _count--;
            }
        }

        public struct Sample
        {
            public T Before;
            public T After;
            public float Alpha;
            public bool ClampedLow;
            public bool ClampedHigh;
        }

        public bool TrySample(float time, out Sample sample)
        {
            sample = default;
            if (_count == 0) return false;

            int oldest = OldestIndex();
            int newest = NewestIndex();

            if (time <= _times[oldest])
            {
                sample.Before = _states[oldest];
                sample.After = _states[oldest];
                sample.Alpha = 0f;
                sample.ClampedLow = true;
                return true;
            }
            if (time >= _times[newest])
            {
                sample.Before = _states[newest];
                sample.After = _states[newest];
                sample.Alpha = 0f;
                sample.ClampedHigh = true;
                return true;
            }

            int lo = 0;
            int hi = _count - 1;
            while (lo + 1 < hi)
            {
                int mid = (lo + hi) >> 1;
                int physicalMid = (oldest + mid) % _capacity;
                if (_times[physicalMid] <= time) lo = mid;
                else hi = mid;
            }

            int beforeIdx = (oldest + lo) % _capacity;
            int afterIdx = (oldest + hi) % _capacity;
            float t0 = _times[beforeIdx];
            float t1 = _times[afterIdx];
            sample.Before = _states[beforeIdx];
            sample.After = _states[afterIdx];
            sample.Alpha = t1 > t0 ? (time - t0) / (t1 - t0) : 0f;
            return true;
        }

        private int OldestIndex() => (_head - _count + _capacity) % _capacity;
        private int NewestIndex() => (_head - 1 + _capacity) % _capacity;
    }
}
