using System;
using System.Collections.Generic;
using System.Text;

namespace JennessentOps
{
  //'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
  // Copyright ©1996-2005 VBnet, Randy Birch, All Rights Reserved.
  // Some pages may also contain other copyrights by the author.
  // see http://vbnet.mvps.org/index.html?code/sort/qsvariations.htm
  //'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
  // Distribution: You can freely use this code in your own
  //               applications, but you may not reproduce
  //               or publish this code on any web site,
  //               online service, or distribute as source
  //               on any media without express permission.
  //'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
  // MODIFIED JAN. 4, 2006 BY JEFF JENNESS, TO SIMPLIFY IMPLEMENTATION IN ARCGIS
  // MODIFIED MARCH 2021 BY JEFF JENNESS, TO SIMPLIFY IMPLEMENTATION IN ARCGIS PRO
  //-----------------------------------------------------------------------------
  public enum JenVariableTypes
  {
    ENUM_TypeString = 1,
    ENUM_TypeDouble = 2,
    ENUM_TypeLong = 4,
    ENUM_TypeDate = 8,
  }
  public static class JenQuickSort
  {

    /// <summary>
    /// Sorts angles (degrees) into ring order, rotated to start just after the largest angular
    /// gap (or, if dblCentralAngle is supplied, just after the point opposite it). When
    /// booSortClockwise is false the order is reversed. dblMeanBearingInGap returns the bearing
    /// that bisects that gap (the direction pointing into the empty sector), or NaN for an empty
    /// array. Faithful port of the original VB6 AngleSort; the gap bisector replaces the earlier
    /// unfinished mean-bearing heuristic.
    /// </summary>
    public static void AngleSort(double[] dblAngles, bool booSortClockwise, out double dblMeanBearingInGap, double dblCentralAngle = double.NaN)
    {
      dblMeanBearingInGap = double.NaN;
      if (dblAngles == null || dblAngles.Length == 0) { return; }

      int n = dblAngles.Length;

      // 1. Sort the angles ascending (VB6: DoubleAscending).
      SimpleAscending(dblAngles, 0, n - 1);

      // 2. Find the split index.
      long lngSplitIndex = 0;
      if (!Double.IsNaN(dblCentralAngle))
      {
        // Split just after the point opposite the central angle.
        double dblSplitAngle = dblCentralAngle - 180;
        if (dblSplitAngle < 0) { dblSplitAngle += 360; }
        while (lngSplitIndex < n && dblAngles[lngSplitIndex] <= dblSplitAngle) { lngSplitIndex++; }
      }
      else
      {
        // Split at the largest angular gap, including the wrap-around gap.
        double dblLargestGap = dblAngles[0] + 360 - dblAngles[n - 1];
        for (long i = 0; i < n - 1; i++)
        {
          double dblTempGap = dblAngles[i + 1] - dblAngles[i];
          if (dblTempGap > dblLargestGap) { dblLargestGap = dblTempGap; lngSplitIndex = i + 1; }
        }
      }

      // 3. Bearing that bisects the split gap (points into the empty sector).
      double dblGapLo, dblGapHi;
      if (lngSplitIndex <= 0 || lngSplitIndex >= n)
      {
        dblGapLo = dblAngles[n - 1];
        dblGapHi = dblAngles[0] + 360;
      }
      else
      {
        dblGapLo = dblAngles[lngSplitIndex - 1];
        dblGapHi = dblAngles[lngSplitIndex];
      }
      dblMeanBearingInGap = ((dblGapLo + dblGapHi) / 2) % 360;

      // 4. Rotate so the array starts at lngSplitIndex (VB6: pTempArray build).
      double[] dblReturn = new double[n];
      long lngArrayIndex = 0;
      for (long i = lngSplitIndex; i < n; i++) { dblReturn[lngArrayIndex++] = dblAngles[i]; }
      for (long i = 0; i < lngSplitIndex; i++) { dblReturn[lngArrayIndex++] = dblAngles[i]; }

      // 5. Write back clockwise (as-is) or counter-clockwise (reversed).
      if (booSortClockwise)
      {
        for (int i = 0; i < n; i++) { dblAngles[i] = dblReturn[i]; }
      }
      else
      {
        for (int i = 0; i < n; i++) { dblAngles[i] = dblReturn[n - 1 - i]; }
      }
    }

    // Index-sort helper: sorts rows [lo, hi] of a 2-D array by column sortCol, carrying
    // columns 0..maxCol. Comparisons touch only the key column; rows are moved once.
    private static void SortRows2D<T>(T[,] nArray, int lo, int hi, int sortCol, int maxCol, bool descending, IComparer<T> comparer)
    {
      if (hi <= lo) { return; }
      int count = hi - lo + 1;
      T[] keys = new T[count];
      int[] order = new int[count];
      for (int i = 0; i < count; i++) { keys[i] = nArray[lo + i, sortCol]; order[i] = i; }
      Array.Sort(keys, order, comparer);   // comparer == null uses Comparer<T>.Default

      T[,] snapshot = new T[count, maxCol + 1];
      for (int i = 0; i < count; i++)
        for (int c = 0; c <= maxCol; c++)
          snapshot[i, c] = nArray[lo + i, c];

      for (int k = 0; k < count; k++)
      {
        int src = descending ? order[count - 1 - k] : order[k];
        for (int c = 0; c <= maxCol; c++)
          nArray[lo + k, c] = snapshot[src, c];
      }
    }

    /// <summary>
    /// Sorts rows in [inLow, inHi] by lngSortColumn (0-based), reordering columns 0..lngMaxColumnNumber together.
    /// Uses an index sort (sorts the key column, then gathers rows once).
    /// </summary>
    public static void Ascending_2Dimensional(DateTime[,] nArray, long inLow, long inHi, long lngSortColumn, long lngMaxColumnNumber)
    {
      SortRows2D(nArray, (int)inLow, (int)inHi, (int)lngSortColumn, (int)lngMaxColumnNumber, false, null);
    }
    /// <summary>
    /// Sorts rows in [inLow, inHi] by lngSortColumn (0-based), reordering columns 0..lngMaxColumnNumber together.
    /// Uses an index sort (sorts the key column, then gathers rows once).
    /// </summary>
    public static void Ascending_2Dimensional(string[,] nArray, long inLow, long inHi, StringComparison CompareType, long lngSortColumn, long lngMaxColumnNumber)
    {
      SortRows2D(nArray, (int)inLow, (int)inHi, (int)lngSortColumn, (int)lngMaxColumnNumber, false, Comparer<string>.Create((x, y) => String.Compare(x, y, CompareType)));
    }
    /// <summary>
    /// Sorts rows in [inLow, inHi] by lngSortColumn (0-based), reordering columns 0..lngMaxColumnNumber together.
    /// Uses an index sort (sorts the key column, then gathers rows once).
    /// </summary>
    public static void Ascending_2Dimensional(double[,] nArray, long inLow, long inHi, long lngSortColumn, long lngMaxColumnNumber)
    {
      SortRows2D(nArray, (int)inLow, (int)inHi, (int)lngSortColumn, (int)lngMaxColumnNumber, false, null);
    }
    /// <summary>
    /// Sorts rows in [inLow, inHi] by lngSortColumn (0-based), reordering columns 0..lngMaxColumnNumber together.
    /// Uses an index sort (sorts the key column, then gathers rows once).
    /// </summary>
    public static void Ascending_2Dimensional(byte[,] nArray, long inLow, long inHi, long lngSortColumn, long lngMaxColumnNumber)
    {
      SortRows2D(nArray, (int)inLow, (int)inHi, (int)lngSortColumn, (int)lngMaxColumnNumber, false, null);
    }
    /// <summary>
    /// Sorts rows in [inLow, inHi] by lngSortColumn (0-based), reordering columns 0..lngMaxColumnNumber together.
    /// Uses an index sort (sorts the key column, then gathers rows once).
    /// </summary>
    public static void Ascending_2Dimensional(int[,] nArray, long inLow, long inHi, long lngSortColumn, long lngMaxColumnNumber)
    {
      SortRows2D(nArray, (int)inLow, (int)inHi, (int)lngSortColumn, (int)lngMaxColumnNumber, false, null);
    }
    /// <summary>
    /// Sorts rows in [inLow, inHi] by lngSortColumn (0-based), reordering columns 0..lngMaxColumnNumber together.
    /// Uses an index sort (sorts the key column, then gathers rows once).
    /// </summary>
    public static void Ascending_2Dimensional(long[,] nArray, long inLow, long inHi, long lngSortColumn, long lngMaxColumnNumber)
    {
      SortRows2D(nArray, (int)inLow, (int)inHi, (int)lngSortColumn, (int)lngMaxColumnNumber, false, null);
    }
    /// <summary>
    /// Sorts rows in [inLow, inHi] by lngSortColumn (0-based), reordering columns 0..lngMaxColumnNumber together.
    /// Uses an index sort (sorts the key column, then gathers rows once).
    /// </summary>
    public static void Ascending_2Dimensional(float[,] nArray, long inLow, long inHi, long lngSortColumn, long lngMaxColumnNumber)
    {
      SortRows2D(nArray, (int)inLow, (int)inHi, (int)lngSortColumn, (int)lngMaxColumnNumber, false, null);
    }
    /// <summary>
    /// Sorts rows in [inLow, inHi] by lngSortColumn (0-based), reordering columns 0..lngMaxColumnNumber together.
    /// Uses an index sort (sorts the key column, then gathers rows once).
    /// </summary>
    public static void Ascending_2Dimensional(decimal[,] nArray, long inLow, long inHi, long lngSortColumn, long lngMaxColumnNumber)
    {
      SortRows2D(nArray, (int)inLow, (int)inHi, (int)lngSortColumn, (int)lngMaxColumnNumber, false, null);
    }
    /// <summary>
    /// Sorts rows in [inLow, inHi] by lngSortColumn (0-based), reordering columns 0..lngMaxColumnNumber together.
    /// Uses an index sort (sorts the key column, then gathers rows once).
    /// </summary>
    public static void Ascending_2Dimensional(short[,] nArray, long inLow, long inHi, long lngSortColumn, long lngMaxColumnNumber)
    {
      SortRows2D(nArray, (int)inLow, (int)inHi, (int)lngSortColumn, (int)lngMaxColumnNumber, false, null);
    }

    /// <summary>
    /// Sorts rows in [inLow, inHi] by lngSortColumn (0-based), reordering columns 0..lngMaxColumnNumber together.
    /// Uses an index sort (sorts the key column, then gathers rows once).
    /// </summary>
    public static void Descending_2Dimensional(DateTime[,] nArray, long inLow, long inHi, long lngSortColumn, long lngMaxColumnNumber)
    {
      SortRows2D(nArray, (int)inLow, (int)inHi, (int)lngSortColumn, (int)lngMaxColumnNumber, true, null);
    }
    /// <summary>
    /// Sorts rows in [inLow, inHi] by lngSortColumn (0-based), reordering columns 0..lngMaxColumnNumber together.
    /// Uses an index sort (sorts the key column, then gathers rows once).
    /// </summary>
    public static void Descending_2Dimensional(string[,] nArray, long inLow, long inHi, StringComparison CompareType, long lngSortColumn, long lngMaxColumnNumber)
    {
      SortRows2D(nArray, (int)inLow, (int)inHi, (int)lngSortColumn, (int)lngMaxColumnNumber, true, Comparer<string>.Create((x, y) => String.Compare(x, y, CompareType)));
    }
    /// <summary>
    /// Sorts rows in [inLow, inHi] by lngSortColumn (0-based), reordering columns 0..lngMaxColumnNumber together.
    /// Uses an index sort (sorts the key column, then gathers rows once).
    /// </summary>
    public static void Descending_2Dimensional(double[,] nArray, long inLow, long inHi, long lngSortColumn, long lngMaxColumnNumber)
    {
      SortRows2D(nArray, (int)inLow, (int)inHi, (int)lngSortColumn, (int)lngMaxColumnNumber, true, null);
    }
    /// <summary>
    /// Sorts rows in [inLow, inHi] by lngSortColumn (0-based), reordering columns 0..lngMaxColumnNumber together.
    /// Uses an index sort (sorts the key column, then gathers rows once).
    /// </summary>
    public static void Descending_2Dimensional(byte[,] nArray, long inLow, long inHi, long lngSortColumn, long lngMaxColumnNumber)
    {
      SortRows2D(nArray, (int)inLow, (int)inHi, (int)lngSortColumn, (int)lngMaxColumnNumber, true, null);
    }
    /// <summary>
    /// Sorts rows in [inLow, inHi] by lngSortColumn (0-based), reordering columns 0..lngMaxColumnNumber together.
    /// Uses an index sort (sorts the key column, then gathers rows once).
    /// </summary>
    public static void Descending_2Dimensional(int[,] nArray, long inLow, long inHi, long lngSortColumn, long lngMaxColumnNumber)
    {
      SortRows2D(nArray, (int)inLow, (int)inHi, (int)lngSortColumn, (int)lngMaxColumnNumber, true, null);
    }
    /// <summary>
    /// Sorts rows in [inLow, inHi] by lngSortColumn (0-based), reordering columns 0..lngMaxColumnNumber together.
    /// Uses an index sort (sorts the key column, then gathers rows once).
    /// </summary>
    public static void Descending_2Dimensional(long[,] nArray, long inLow, long inHi, long lngSortColumn, long lngMaxColumnNumber)
    {
      SortRows2D(nArray, (int)inLow, (int)inHi, (int)lngSortColumn, (int)lngMaxColumnNumber, true, null);
    }
    /// <summary>
    /// Sorts rows in [inLow, inHi] by lngSortColumn (0-based), reordering columns 0..lngMaxColumnNumber together.
    /// Uses an index sort (sorts the key column, then gathers rows once).
    /// </summary>
    public static void Descending_2Dimensional(float[,] nArray, long inLow, long inHi, long lngSortColumn, long lngMaxColumnNumber)
    {
      SortRows2D(nArray, (int)inLow, (int)inHi, (int)lngSortColumn, (int)lngMaxColumnNumber, true, null);
    }
    /// <summary>
    /// Sorts rows in [inLow, inHi] by lngSortColumn (0-based), reordering columns 0..lngMaxColumnNumber together.
    /// Uses an index sort (sorts the key column, then gathers rows once).
    /// </summary>
    public static void Descending_2Dimensional(decimal[,] nArray, long inLow, long inHi, long lngSortColumn, long lngMaxColumnNumber)
    {
      SortRows2D(nArray, (int)inLow, (int)inHi, (int)lngSortColumn, (int)lngMaxColumnNumber, true, null);
    }
    /// <summary>
    /// Sorts rows in [inLow, inHi] by lngSortColumn (0-based), reordering columns 0..lngMaxColumnNumber together.
    /// Uses an index sort (sorts the key column, then gathers rows once).
    /// </summary>
    public static void Descending_2Dimensional(short[,] nArray, long inLow, long inHi, long lngSortColumn, long lngMaxColumnNumber)
    {
      SortRows2D(nArray, (int)inLow, (int)inHi, (int)lngSortColumn, (int)lngMaxColumnNumber, true, null);
    }

    public static void SimpleAscending(DateTime[] nArray, long inLow, long inHi)
    {
      if (inHi <= inLow) { return; }
      int intIndex = (int)inLow;
      int intLength = (int)(inHi - inLow + 1);
      Array.Sort(nArray, intIndex, intLength);
    }
    public static void SimpleAscending(string[] nArray, long inLow, long inHi, StringComparison CompareType)
    {
      if (inHi <= inLow) { return; }
      int intIndex = (int)inLow;
      int intLength = (int)(inHi - inLow + 1);
      Array.Sort(nArray, intIndex, intLength, Comparer<string>.Create((x, y) => String.Compare(x, y, CompareType)));
    }
    public static void SimpleAscending(double[] nArray, long inLow, long inHi)
    {
      if (inHi <= inLow) { return; }
      int intIndex = (int)inLow;
      int intLength = (int)(inHi - inLow + 1);
      Array.Sort(nArray, intIndex, intLength);
    }
    public static void SimpleAscending(byte[] nArray, long inLow, long inHi)
    {
      if (inHi <= inLow) { return; }
      int intIndex = (int)inLow;
      int intLength = (int)(inHi - inLow + 1);
      Array.Sort(nArray, intIndex, intLength);
    }
    public static void SimpleAscending(int[] nArray, long inLow, long inHi)
    {
      if (inHi <= inLow) { return; }
      int intIndex = (int)inLow;
      int intLength = (int)(inHi - inLow + 1);
      Array.Sort(nArray, intIndex, intLength);
    }
    public static void SimpleAscending(long[] nArray, long inLow, long inHi)
    {
      if (inHi <= inLow) { return; }
      int intIndex = (int)inLow;
      int intLength = (int)(inHi - inLow + 1);
      Array.Sort(nArray, intIndex, intLength);
    }
    public static void SimpleAscending(float[] nArray, long inLow, long inHi)
    {
      if (inHi <= inLow) { return; }
      int intIndex = (int)inLow;
      int intLength = (int)(inHi - inLow + 1);
      Array.Sort(nArray, intIndex, intLength);
    }
    public static void SimpleAscending(decimal[] nArray, long inLow, long inHi)
    {
      if (inHi <= inLow) { return; }
      int intIndex = (int)inLow;
      int intLength = (int)(inHi - inLow + 1);
      Array.Sort(nArray, intIndex, intLength);
    }
    public static void SimpleAscending(short[] nArray, long inLow, long inHi)
    {
      if (inHi <= inLow) { return; }
      int intIndex = (int)inLow;
      int intLength = (int)(inHi - inLow + 1);
      Array.Sort(nArray, intIndex, intLength);
    }

    public static void SimpleDescending(DateTime[] nArray, long inLow, long inHi)
    {
      if (inHi <= inLow) { return; }
      int intIndex = (int)inLow;
      int intLength = (int)(inHi - inLow + 1);
      Array.Sort(nArray, intIndex, intLength);
      Array.Reverse(nArray, intIndex, intLength);
    }
    public static void SimpleDescending(string[] nArray, long inLow, long inHi, StringComparison CompareType)
    {
      if (inHi <= inLow) { return; }
      int intIndex = (int)inLow;
      int intLength = (int)(inHi - inLow + 1);
      Array.Sort(nArray, intIndex, intLength, Comparer<string>.Create((x, y) => String.Compare(x, y, CompareType)));
      Array.Reverse(nArray, intIndex, intLength);
    }
    public static void SimpleDescending(double[] nArray, long inLow, long inHi)
    {
      if (inHi <= inLow) { return; }
      int intIndex = (int)inLow;
      int intLength = (int)(inHi - inLow + 1);
      Array.Sort(nArray, intIndex, intLength);
      Array.Reverse(nArray, intIndex, intLength);
    }
    public static void SimpleDescending(byte[] nArray, long inLow, long inHi)
    {
      if (inHi <= inLow) { return; }
      int intIndex = (int)inLow;
      int intLength = (int)(inHi - inLow + 1);
      Array.Sort(nArray, intIndex, intLength);
      Array.Reverse(nArray, intIndex, intLength);
    }
    public static void SimpleDescending(int[] nArray, long inLow, long inHi)
    {
      if (inHi <= inLow) { return; }
      int intIndex = (int)inLow;
      int intLength = (int)(inHi - inLow + 1);
      Array.Sort(nArray, intIndex, intLength);
      Array.Reverse(nArray, intIndex, intLength);
    }
    public static void SimpleDescending(long[] nArray, long inLow, long inHi)
    {
      if (inHi <= inLow) { return; }
      int intIndex = (int)inLow;
      int intLength = (int)(inHi - inLow + 1);
      Array.Sort(nArray, intIndex, intLength);
      Array.Reverse(nArray, intIndex, intLength);
    }
    public static void SimpleDescending(float[] nArray, long inLow, long inHi)
    {
      if (inHi <= inLow) { return; }
      int intIndex = (int)inLow;
      int intLength = (int)(inHi - inLow + 1);
      Array.Sort(nArray, intIndex, intLength);
      Array.Reverse(nArray, intIndex, intLength);
    }
    public static void SimpleDescending(decimal[] nArray, long inLow, long inHi)
    {
      if (inHi <= inLow) { return; }
      int intIndex = (int)inLow;
      int intLength = (int)(inHi - inLow + 1);
      Array.Sort(nArray, intIndex, intLength);
      Array.Reverse(nArray, intIndex, intLength);
    }
    public static void SimpleDescending(short[] nArray, long inLow, long inHi)
    {
      if (inHi <= inLow) { return; }
      int intIndex = (int)inLow;
      int intLength = (int)(inHi - inLow + 1);
      Array.Sort(nArray, intIndex, intLength);
      Array.Reverse(nArray, intIndex, intLength);
    }
  }
}
