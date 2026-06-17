using System;
using System.Collections.Generic;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Basic statistics and linear regression used by the summary export,
    /// matching the legacy BasicStatsFromArraySimpleFast2 and CalcRegression
    /// (MyGeneralOperations.bas). Standard deviation is the SAMPLE stdev (÷ N-1).
    /// </summary>
    internal static class SummaryStats
    {
        internal readonly struct MeanResult
        {
            internal MeanResult(int count, double mean, double sampleStdDev)
            {
                Count = count;
                Mean = mean;
                SampleStdDev = sampleStdDev;
            }

            internal int Count { get; }
            internal double Mean { get; }
            internal double SampleStdDev { get; }
        }

        /// <summary>
        /// Mean and sample standard deviation of the values. Count &lt;= 1 yields a
        /// standard deviation of 0; an empty list yields count 0.
        /// </summary>
        internal static MeanResult MeanAndStdDev(IReadOnlyList<double> values)
        {
            int n = values.Count;
            if (n == 0)
            {
                return new MeanResult(0, 0, 0);
            }

            double sum = 0;
            for (int i = 0; i < n; i++)
            {
                sum += values[i];
            }

            double mean = sum / n;
            if (n == 1)
            {
                return new MeanResult(1, mean, 0);
            }

            double sumSqDev = 0;
            for (int i = 0; i < n; i++)
            {
                double dev = values[i] - mean;
                sumSqDev += dev * dev;
            }

            double stdev = Math.Sqrt(sumSqDev / (n - 1));
            return new MeanResult(n, mean, stdev);
        }

        internal readonly struct RegressionResult
        {
            internal RegressionResult(double? slope, double rSquared, double? adjustedRSquared)
            {
                Slope = slope;
                RSquared = rSquared;
                AdjustedRSquared = adjustedRSquared;
            }

            internal double? Slope { get; }
            internal double RSquared { get; }
            internal double? AdjustedRSquared { get; }
        }

        /// <summary>
        /// Ordinary-least-squares regression of y on x. Slope is null when all x
        /// are identical; adjusted R² is null when fewer than 3 points. Matches
        /// the legacy CalcRegression. Requires at least 2 points.
        /// </summary>
        internal static RegressionResult Regression(IReadOnlyList<double> x, IReadOnlyList<double> y)
        {
            int n = x.Count;
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0, sumY2 = 0;
            for (int i = 0; i < n; i++)
            {
                sumX += x[i];
                sumY += y[i];
                sumXY += x[i] * y[i];
                sumX2 += x[i] * x[i];
                sumY2 += y[i] * y[i];
            }

            double denomX = n * sumX2 - sumX * sumX;
            double? slope = denomX == 0 ? (double?)null : (n * sumXY - sumX * sumY) / denomX;

            double denomY = n * sumY2 - sumY * sumY;
            double r = (denomX == 0 || denomY == 0)
                ? 0
                : (n * sumXY - sumX * sumY) / Math.Sqrt(denomX * denomY);
            double r2 = r * r;

            double? adjR2 = n > 2 ? 1 - ((1 - r2) * (n - 1) / (n - 2)) : (double?)null;

            return new RegressionResult(slope, r2, adjR2);
        }
    }
}
