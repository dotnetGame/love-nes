using System;
using System.Collections.Generic;
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

        public Clock()
        {
            _clockSinks = new List<IClockSink>();
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
        /// 上电
        /// </summary>
        public void PowerUp()
        {
            _clockSinks.ForEach(o => o.OnPowerUp());

            while (true)
            {
                _clockSinks.ForEach(o => o.OnTick());

                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// 复位
        /// </summary>
        public void Reset()
        {
            _clockSinks.ForEach(o => o.OnReset());
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
