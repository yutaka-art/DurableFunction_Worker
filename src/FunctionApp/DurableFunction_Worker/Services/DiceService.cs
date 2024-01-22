using System;
using System.Threading.Tasks;

namespace DurableFunction_Worker.Services
{
    #region IDiceService
    /// <summary>
    /// IDiceService インタフェース
    /// </summary>
    public interface IDiceService : IDisposable
    {
        /// <summary>
        /// サイコロ
        /// </summary>
        /// <param name="judgeValue">判定値</param>
        /// <returns></returns>
        Task<double> RollDiceUntilAsync(int judgeValue);
    }
    #endregion

    #region DiceService
    /// <summary>
    /// DiceService
    /// </summary>
    public class DiceService : IDiceService
    {
        #region Constructor
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DiceService() : base() { }
        #endregion

        #region Method
        /// <summary>
        /// サイコロ
        /// </summary>
        /// <param name="judgeValue">判定値</param>
        /// <returns></returns>
        public async Task<double> RollDiceUntilAsync(int judgeValue)
        {
            var random = new Random();
            int total = 0;

            var calcDatetime = DateTime.Now;

            do
            {
                int dice1 = random.Next(1, 7); // 1から6までの乱数
                int dice2 = random.Next(1, 7);
                int dice3 = random.Next(1, 7);
                int dice4 = random.Next(1, 7);
                int dice5 = random.Next(1, 7);
                total = dice1 + dice2 + dice3 + dice4 + dice5;

                // ここで1秒待機
                await Task.Delay(1000);

            } while (total < judgeValue);

            var processingTime = DateTime.Now.Subtract(calcDatetime).TotalMilliseconds;

            return processingTime;
        }
        #endregion

        #region Dispose
        private bool IsDisposed = false;

        /// <summary>
        /// リソースの開放
        /// </summary>
        public void Dispose()
        {
            if (this.IsDisposed) return;

            this.IsDisposed = true;
        }
        #endregion
    }
    #endregion
}
