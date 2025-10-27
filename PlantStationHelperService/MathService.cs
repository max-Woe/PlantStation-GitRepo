using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantStationHelperService
{
    /// <summary>
    /// Provides static utility methods for mathematical calculations, such as data smoothing and statistical analysis.
    /// This is a static class and cannot be instantiated.
    /// </summary>
    public static class MathService
    {
        /// <summary>
        /// Calculates the moving average of the given data with the specified window size.
        /// This implementation uses a central moving average, where the window is centered around the current data point (i).
        /// </summary>
        /// <param name="data">The numerical data points (as a <see cref="T:System.Double"/> array) over which the average should be calculated.</param>
        /// <param name="windowSize">The number of data points to be used for each average value. Must be a positive integer.</param>
        /// <returns>A <see cref="T:System.Double"/> array containing the smoothed data values. The output array has the same length as the input array.</returns>
        /// <remarks>
        /// The method handles boundary conditions by restricting the window to the available data points at the start and end of the array.
        /// If the window size is an even number, the division by 2 will be truncated, effectively making the window slightly off-center.
        /// </remarks>
        static public double[] CalculateMovingAverage(double[] data, int windowSize)
        {
            if (windowSize <= 0)
            {
                return (double[])data.Clone();
            }
            if (data == null || data.Length == 0)
            {
                return new double[0];
            }

            double[] smoothed = new double[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                int start = Math.Max(0, i - windowSize / 2);

                int end = Math.Min(data.Length - 1, i + windowSize / 2);

                double sum = 0;
                int count = 0;

                for (int j = start; j <= end; j++)
                {
                    sum += data[j];
                    count++;
                }

                smoothed[i] = sum / count;
            }
            return smoothed;
        }
    }
}