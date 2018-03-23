using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace LoveNes
{
    /// <summary>
    /// 时钟
    /// </summary>
    public class Clock
    {
        private readonly List<IClockSink> _clockSinks;
        private readonly List<IClockSink> _clock3Sinks;

        private readonly Stopwatch _stopwatch;
        private long _actualAge;
        private readonly uint _frequency;

        public Clock(uint frequency)
        {
            _clockSinks = new List<IClockSink>();
            _clock3Sinks = new List<IClockSink>();
            _frequency = frequency;
            _stopwatch = new Stopwatch();
        }

        /// <summary>
        /// 添加时钟终端
        /// </summary>
        /// <param name="sink">终端</param>
        public void AddSink(IClockSink sink)
        {
            _clockSinks.Add(sink);
        }

        /// <summary>
        /// 添加 3 倍时钟终端
        /// </summary>
        /// <param name="sink">终端</param>
        public void Add3TimesSink(IClockSink sink)
        {
            _clock3Sinks.Add(sink);
        }

        /// <summary>
        /// 上电
        /// </summary>
        public void PowerUp()
        {
            _clockSinks.ForEach(o => o.OnPowerUp());
            _clock3Sinks.ForEach(o => o.OnPowerUp());

            _actualAge = 0;
            _stopwatch.Restart();

            while (true)
            {
                var expectedAge = _stopwatch.ElapsedMilliseconds * _frequency / 1000;
                var updateTimes = expectedAge - _actualAge;
                if (updateTimes <= 0)
                {
                    Thread.Sleep(1);
                }

                while (updateTimes > 0)
                {
                    _clockSinks.ForEach(o => o.OnTick());

                    for (int i = 0; i < 3; i++)
                        _clock3Sinks.ForEach(o => o.OnTick());

                    _actualAge++;
                    updateTimes--;
                }
            }
        }

        /// <summary>
        /// 复位
        /// </summary>
        public void Reset()
        {
            _clockSinks.ForEach(o => o.OnReset());
            _clock3Sinks.ForEach(o => o.OnReset());
        }
    }

    /// <summary>
    /// 时钟终端
    /// </summary>
    public interface IClockSink
    {
        /// <summary>
        /// Tick
        /// </summary>
        void OnTick();

        /// <summary>
        /// 上电
        /// </summary>
        void OnPowerUp();

        /// <summary>
        /// 复位
        /// </summary>
        void OnReset();
    }
}
