using System;

namespace JennessentOps
{
  public enum JenClockwiseConstants
  {
    ENUM_CounterClockwise = 0,
    ENUM_OnLine = 1,
    ENUM_Clockwise = 2
  }
  public enum JenSphericalMethod
  {
    ENUM_UseTrigonometry = 1,
    ENUM_UseSpherical = 2,
    ENUM_UseSpheroidal = 4
  }
  public enum JenSolarConditions
  {
    ENUM_SunriseAndSunset = 1,
    ENUM_AlwaysNight = 2,
    ENUM_AlwaysDay = 4
  }
  public enum JenSegmentIntersectTypes
  {
    ENUM_NoIntersect = 1,
    ENUM_IntersectEdgeEndpoint = 2,
    ENUM_Crosses = 4,
    ENUM_CollinearSegment = 8
  }
  public static class MyGeometricOps
  {
    public const double dblPi = Math.PI; 

    /// <summary>
    /// Given point and polygon, returns boolean if point is inside polygon.  Uses Crossing method.
    /// </summary>
    /// <param name="dblPointX"></param>
    /// <param name="dblPointY"></param>
    /// <param name="dblPolygon"></param>
    /// <returns></returns>
    public static bool PointInPoly_Crossing(double[] dblPoint, double[][,] dblPolygon) => PointInPoly_Crossing(dblPoint[0], dblPoint[1], dblPolygon);
    /// <summary>
    /// Given point and polygon, returns boolean if point is inside polygon.  Uses Crossing method.
    /// </summary>
    /// <param name="dblPointX"></param>
    /// <param name="dblPointY"></param>
    /// <param name="dblPolygon"></param>
    /// <returns></returns>
    public static bool PointInPoly_Crossing(double dblPointX, double dblPointY, double[][,] dblPolygon)
    {
      // ASSUMES POLYGON IS IN THE FORM OF A VARIANT ARRAY, WHERE EACH OBJECT IN THE ARRAY IS A POLYGON RING.
      // EACH RING IS IN THE FORM OF A DOUBLE-ARRAY OF EACH VERTEX IN THE RING.
      // VERTEX (0) = VERTEX (uBound(RingArray))

      // adapted from http://geomalgorithms.com/a03-_inclusion.html
      //// cn_PnPoly(): crossing number test for a point in a polygon
      ////      Input:   P = a point,
      ////               V[] = vertex points of a polygon V[n+1] with V[n]=V[0]
      ////      Return:  0 = outside, 1 = inside
      //// This code is patterned after [Franklin, 2000]
      //int
      //cn_PnPoly( Point P, Point* V, int n )//{
      //    int    cn = 0;    // the  crossing number counter
      //
      //    // loop through all edges of the polygon
      //    for (int i=0; i<n; i++) {    // edge from V[i]  to V[i+1]
      //       if (((V[i].y <= P.y) && (V[i+1].y > P.y))     // an upward crossing
      //        || ((V[i].y > P.y) && (V[i+1].y <=  P.y))) { // a downward crossing
      //            // compute  the actual edge-ray intersect x-coordinate
      //            float vt = (float)(P.y  - V[i].y) / (V[i+1].y - V[i].y);
      //            if (P.x <  V[i].x + vt * (V[i+1].x - V[i].x)) // P.x < intersect
      //                 ++cn;   // a valid crossing of y=P.y right of P.x
      //        }
      //    }
      //    return (cn&1);    // 0 if even (out), and 1 if  odd (in)
      //
      //}
      ////===================================================================

      double dblX1;
      double dblY1;
      double dblX2;
      double dblY2;
      long lngCrossCounter = 0;

      foreach (double[,] dblRing in dblPolygon)
      {
        for (int i = 0; i < dblRing.GetLength(0) - 1; i++)
        {
          // This "if-then" excludes all cases where the edge segment can//t possibly intersect horizontal line, and
          // also cases where edge segment itself is horizontal
          dblX1 = dblRing[i, 0];
          dblY1 = dblRing[i, 1];
          dblX2 = dblRing[i + 1, 0];
          dblY2 = dblRing[i + 1, 1];

          if ((dblY1 <= dblPointY && dblY2 > dblPointY) || (dblY1 > dblPointY && dblY2 <= dblPointY))
          {
            if (dblPointX < dblX1 + (((dblPointY - dblY1) / (dblY2 - dblY1)) * (dblX2 - dblX1))) { lngCrossCounter++; }
          }
        }
      }
      return lngCrossCounter % 2 == 1;

      //double[][,] dblPolygonRings = new double[4][,]
      //{
      //new double[10,2] {  // Exterior Ring ...
      //  {-111.58015220600000, 35.25729186900000},
      //  {-111.58038476300000, 35.25718009700010},
      //  {-111.58069137100000, 35.25722647800010},
      //  {-111.58076199000000, 35.25741455400000},
      //  {-111.58069007100000, 35.25755546200000},
      //  {-111.58047924100000, 35.25764619500010},
      //  {-111.58030781200000, 35.25763804600010},
      //  {-111.58015798400000, 35.25759091300010},
      //  {-111.58015062800000, 35.25758197600000},
      //  {-111.58015220600000, 35.25729186900000}},
      //new double[6,2] {  // Exterior Ring
      //  {-111.58160260800000, 35.25769463400010},
      //  {-111.58160175300000, 35.25757501000000},
      //  {-111.58182062300000, 35.25758891100000},
      //  {-111.58183220200000, 35.25767857700000},
      //  {-111.58172324800000, 35.25773891500010},
      //  {-111.58160260800000, 35.25769463400010}},
      //new double[9,2] {  // Exterior Ring
      //  {-111.58140122700000, 35.25808738500010},
      //  {-111.58110423000000, 35.25785553600010},
      //  {-111.58107601800000, 35.25747884200010},
      //  {-111.58141924000000, 35.25722654800010},
      //  {-111.58223087200000, 35.25734169900010},
      //  {-111.58230959900000, 35.25764338300010},
      //  {-111.58209312400000, 35.25796443100010},
      //  {-111.58180585500000, 35.25807347800000},
      //  {-111.58140122700000, 35.25808738500010}},
      //new double[8,2] {  // Interior Ring
      //  {-111.58208719000000, 35.25764445300010},
      //  {-111.58188138700000, 35.25741814800010},
      //  {-111.58144770100000, 35.25744714900000},
      //  {-111.58132107200000, 35.25758533100000},
      //  {-111.58144710900000, 35.25787482400010},
      //  {-111.58176210500000, 35.25791675700000},
      //  {-111.58201553100000, 35.25782124900010},
      //  {-111.58208719000000, 35.25764445300010}}
      //};

      //double dblTestPointX;
      //double dblTestPointY;
      //bool booInPolygon;
      //// SHOULD BE OUT
      //dblTestPointX = -111.5835827;
      //dblTestPointY = 35.2576885;
      //booInPolygon = PointInPoly_Crossing(new double[] { dblTestPointX, dblTestPointY }, dblPolygonRings);
      //Console.WriteLine("Should be Out:  Point in Polygon = " + booInPolygon.ToString());
      //// SHOULD BE IN EXTERNAL REGION
      //dblTestPointX = -111.5813091;
      //dblTestPointY = 35.2578188;
      //booInPolygon = PointInPoly_Crossing(new double[] { dblTestPointX, dblTestPointY }, dblPolygonRings);
      //Console.WriteLine("SHOULD BE IN EXTERNAL REGION:  Point in Polygon = " + booInPolygon.ToString());
      //// SHOULD BE IN CENTER ISLAND
      //dblTestPointX = -111.5816889;
      //dblTestPointY = 35.2576428;
      //booInPolygon = PointInPoly_Crossing(new double[] { dblTestPointX, dblTestPointY }, dblPolygonRings);
      //Console.WriteLine("SHOULD BE IN CENTER ISLAND:  Point in Polygon = " + booInPolygon.ToString());
      //// SHOULD BE OUT BETWEEN EXTERNAL REGION AND CENTER ISLAND
      //dblTestPointX = -111.5815094;
      //dblTestPointY = 35.2577677;
      //booInPolygon = PointInPoly_Crossing(new double[] { dblTestPointX, dblTestPointY }, dblPolygonRings);
      //Console.WriteLine("SHOULD BE OUT BETWEEN EXTERNAL REGION AND CENTER ISLAND:  Point in Polygon = " + booInPolygon.ToString());
      ////SHOULD BE IN SEPARATE ISLAND
      //dblTestPointX = -111.5804540;
      //dblTestPointY = 35.2574054;
      //booInPolygon = PointInPoly_Crossing(new double[] { dblTestPointX, dblTestPointY }, dblPolygonRings);
      //Console.WriteLine("SHOULD BE IN SEPARATE ISLAND:  Point in Polygon = " + booInPolygon.ToString());
      //// ONE OF OUTER RING VERTICES:  RETURNS FALSE
      //dblTestPointX = -111.58015220600000;
      //dblTestPointY = 35.25729186900000;
      //booInPolygon = PointInPoly_Crossing(new double[] { dblTestPointX, dblTestPointY }, dblPolygonRings);
      //Console.WriteLine("ONE OF OUTER RING VERTICES:  Point in Polygon = " + booInPolygon.ToString());
      //// ONE OF INNER RING VERTICES:  RETURNS FALSE
      //dblTestPointX = -111.58208719000000;
      //dblTestPointY = 35.25764445300010;
      //booInPolygon = PointInPoly_Crossing(new double[] { dblTestPointX, dblTestPointY }, dblPolygonRings);
      //Console.WriteLine("ONE OF INNER RING VERTICES:  Point in Polygon = " + booInPolygon.ToString());

      ////booInPolygon = PointInPoly_Winding(new double[] { dblTestPointX, dblTestPointY },  dblPolygonRings);
      ////Console.WriteLine("Point in Polygon = " + booInPolygon.ToString());

      //double dblMilliseconds;
      //sw.Start();
      //long lngCounter = 0;
      //for (int i = 0; i < 1000000; i++)
      //{
      //  lngCounter++;
      //  booInPolygon = PointInPoly_Winding(new double[] { dblTestPointX, dblTestPointY }, dblPolygonRings);
      //}
      //sw.Stop();
      //dblMilliseconds = sw.ElapsedMilliseconds;
      //Console.Write("Winding Method (" + lngCounter.ToString("#,##0") + " iterations): " + ((double)sw.ElapsedMilliseconds / 1000).ToString("0.000") + " stopwatch seconds\n");
      //sw.Restart();
      //lngCounter = 0;
      //for (int i = 0; i < 1000000; i++)
      //{
      //  lngCounter++;
      //  booInPolygon = PointInPoly_Crossing(new double[] { dblTestPointX, dblTestPointY }, dblPolygonRings);
      //}
      //sw.Stop();
      //dblMilliseconds = sw.ElapsedMilliseconds;
      //Console.Write("Crossing Method (" + lngCounter.ToString("#,##0") + " iterations): " + ((double)sw.ElapsedMilliseconds / 1000).ToString("0.000") + " stopwatch seconds\n");
    }

    /// <summary>
    /// Given point and polygon, returns boolean if point is inside polygon.  Uses Winding method.
    /// </summary>
    /// <param name="dblPointX"></param>
    /// <param name="dblPointY"></param>
    /// <param name="dblPolygon"></param>
    /// <returns></returns>
    public static bool PointInPoly_Winding(double[] dblPoint, double[][,] dblPolygon) => PointInPoly_Winding(dblPoint[0], dblPoint[1], dblPolygon);
    /// <summary>
    /// Given point and polygon, returns boolean if point is inside polygon.  Uses Winding method.
    /// </summary>
    /// <param name="dblPointX"></param>
    /// <param name="dblPointY"></param>
    /// <param name="dblPolygon"></param>
    /// <returns></returns>
    public static bool PointInPoly_Winding(double dblPointX, double dblPointY, double[][,] dblPolygon)
    {
      // ASSUMES POLYGON IS IN THE FORM OF A VARIANT ARRAY, WHERE EACH OBJECT IN THE ARRAY IS A POLYGON RING.
      // EACH RING IS IN THE FORM OF A DOUBLE-ARRAY OF EACH VERTEX IN THE RING.
      // VERTEX (0) = VERTEX (uBound(RingArray))
      //
      // USES FUNCTION CalcCheckClockwiseNumbers2, BUT HARD-CODED DIRECTLY FOR PERFORMANCE
      //
      // IN TESTS, WINDING FUNCTION GIVES SAME RESULTS AS CROSS-METHOD, BUT RUNS ROUGHLY 9% FASTER
      // IN GENERAL BOTH WINDING AND CROSS METHODS GIVE ACCURATE RESULTS IN MULTPART POLYGONS CONTAINING ISLANDS AND NESTED HOLES

      // adapted from http://geomalgorithms.com/a03-_inclusion.html
      //// isLeft(): tests if a point is Left|On|Right of an infinite line.
      ////    Input:  three points P0, P1, and P2
      ////    Return: >0 for P2 left of the line through P0 and P1
      ////            =0 for P2  on the line
      ////            <0 for P2  right of the line
      ////    See: Algorithm 1 "Area of Triangles and Polygons"
      //inline int
      //isLeft( Point P0, Point P1, Point P2 )
      //{
      //    return ( (P1.x - P0.x) * (P2.y - P0.y)
      //            - (P2.x -  P0.x) * (P1.y - P0.y) );
      //}
      //// wn_PnPoly(): winding number test for a point in a polygon
      ////      Input:   P = a point,
      ////               V[] = vertex points of a polygon V[n+1] with V[n]=V[0]
      ////      Return:  wn = the winding number (=0 only when P is outside)
      //int
      //wn_PnPoly( Point P, Point* V, int n )
      //{
      //    int    wn = 0;    // the  winding number counter
      //
      //    // loop through all edges of the polygon
      //    for (int i=0; i<n; i++) {   // edge from V[i] to  V[i+1]
      //        if (V[i].y <= P.y) {          // start y <= P.y
      //            if (V[i+1].y  > P.y)      // an upward crossing
      //                 if (isLeft( V[i], V[i+1], P) > 0)  // P left of  edge
      //                     ++wn;            // have  a valid up intersect
      //        }
      //        else {                        // start y > P.y (no test needed)
      //            if (V[i+1].y  <= P.y)     // a downward crossing
      //                 if (isLeft( V[i], V[i+1], P) < 0)  // P right of  edge
      //                     --wn;            // have  a valid down intersect
      //        }
      //    }
      //    return wn;
      //}
      ////===================================================================
      double dblX1;
      double dblY1;
      double dblX2;
      double dblY2;
      long lngWindCounter = 0;
      //JenClockwiseConstants jenClockwise;

      //double dblDistance = (dblQX * (dblRY - dblPY)) + (dblQY * (dblPX - dblRX)) - (dblPX * dblRY) + (dblPY * dblRX);
      //booLinearOrCoincident = dblDistance == 0;
      //if (dblDistance < 0) { JenClockwise = JenClockwiseConstants.ENUM_Clockwise; }
      //else if (dblDistance == 0) { JenClockwise = JenClockwiseConstants.ENUM_OnLine; }
      //else { JenClockwise = JenClockwiseConstants.ENUM_CounterClockwise; }
      //dblDistanceToInfiniteLine = Math.Sqrt(dblDistance);
      //return dblDistance < 0;
      foreach (double[,] dblRing in dblPolygon)
      {
        for (int i = 0; i < dblRing.GetLength(0) - 1; i++)
        {
          // This "if-then" excludes all cases where the edge segment can//t possibly intersect horizontal line, and
          // also cases where edge segment itself is horizontal
          dblX1 = dblRing[i, 0];
          dblY1 = dblRing[i, 1];
          dblX2 = dblRing[i + 1, 0];
          dblY2 = dblRing[i + 1, 1];
          if (dblY1 <= dblPointY)
          {
            if (dblY2 > dblPointY)  // Then an upward crossing
            {
              //CalcCheckClockwiseNumbers(dblX1, dblY1, dblX2, dblY2, dblPointX, dblPointY, out _, out jenClockwise, out _);
              //if (jenClockwise == JenClockwiseConstants.ENUM_CounterClockwise) { lngWindCounter++; }
              if ((dblX2 * (dblPointY - dblY1)) + (dblY2 * (dblX1 - dblPointX)) - (dblX1 * dblPointY) + (dblY1 * dblPointX) > 0) { lngWindCounter++; }
            }
          }
          else  // AUTOMATICALLY WE KNOW dblY1 > dblPointY; no test needed
          {
            if (dblY2 <= dblPointY)
            {
              //CalcCheckClockwiseNumbers(dblX1, dblY1, dblX2, dblY2, dblPointX, dblPointY, out _, out jenClockwise, out _);
              //if (jenClockwise == JenClockwiseConstants.ENUM_Clockwise) { lngWindCounter--; }
              if ((dblX2 * (dblPointY - dblY1)) + (dblY2 * (dblX1 - dblPointX)) - (dblX1 * dblPointY) + (dblY1 * dblPointX) < 0) { lngWindCounter--; }
            }
          }
        }
      }
      return lngWindCounter != 0;

      //double[][,] dblPolygonRings = new double[4][,]
      //{
      //new double[10,2] {  // Exterior Ring ...
      //  {-111.58015220600000, 35.25729186900000},
      //  {-111.58038476300000, 35.25718009700010},
      //  {-111.58069137100000, 35.25722647800010},
      //  {-111.58076199000000, 35.25741455400000},
      //  {-111.58069007100000, 35.25755546200000},
      //  {-111.58047924100000, 35.25764619500010},
      //  {-111.58030781200000, 35.25763804600010},
      //  {-111.58015798400000, 35.25759091300010},
      //  {-111.58015062800000, 35.25758197600000},
      //  {-111.58015220600000, 35.25729186900000}},
      //new double[6,2] {  // Exterior Ring
      //  {-111.58160260800000, 35.25769463400010},
      //  {-111.58160175300000, 35.25757501000000},
      //  {-111.58182062300000, 35.25758891100000},
      //  {-111.58183220200000, 35.25767857700000},
      //  {-111.58172324800000, 35.25773891500010},
      //  {-111.58160260800000, 35.25769463400010}},
      //new double[9,2] {  // Exterior Ring
      //  {-111.58140122700000, 35.25808738500010},
      //  {-111.58110423000000, 35.25785553600010},
      //  {-111.58107601800000, 35.25747884200010},
      //  {-111.58141924000000, 35.25722654800010},
      //  {-111.58223087200000, 35.25734169900010},
      //  {-111.58230959900000, 35.25764338300010},
      //  {-111.58209312400000, 35.25796443100010},
      //  {-111.58180585500000, 35.25807347800000},
      //  {-111.58140122700000, 35.25808738500010}},
      //new double[8,2] {  // Interior Ring
      //  {-111.58208719000000, 35.25764445300010},
      //  {-111.58188138700000, 35.25741814800010},
      //  {-111.58144770100000, 35.25744714900000},
      //  {-111.58132107200000, 35.25758533100000},
      //  {-111.58144710900000, 35.25787482400010},
      //  {-111.58176210500000, 35.25791675700000},
      //  {-111.58201553100000, 35.25782124900010},
      //  {-111.58208719000000, 35.25764445300010}}
      //};

      //double dblTestPointX;
      //double dblTestPointY;
      //bool booInPolygon;
      //// SHOULD BE OUT
      //dblTestPointX = -111.5835827;
      //dblTestPointY = 35.2576885;
      //booInPolygon = PointInPoly_Winding(new double[] { dblTestPointX, dblTestPointY }, dblPolygonRings);
      //Console.WriteLine("Should be Out:  Point in Polygon = " + booInPolygon.ToString());
      //// SHOULD BE IN EXTERNAL REGION
      //dblTestPointX = -111.5813091;
      //dblTestPointY = 35.2578188;
      //booInPolygon = PointInPoly_Winding(new double[] { dblTestPointX, dblTestPointY }, dblPolygonRings);
      //Console.WriteLine("SHOULD BE IN EXTERNAL REGION:  Point in Polygon = " + booInPolygon.ToString());
      //// SHOULD BE IN CENTER ISLAND
      //dblTestPointX = -111.5816889;
      //dblTestPointY = 35.2576428;
      //booInPolygon = PointInPoly_Winding(new double[] { dblTestPointX, dblTestPointY }, dblPolygonRings);
      //Console.WriteLine("SHOULD BE IN CENTER ISLAND:  Point in Polygon = " + booInPolygon.ToString());
      //// SHOULD BE OUT BETWEEN EXTERNAL REGION AND CENTER ISLAND
      //dblTestPointX = -111.5815094;
      //dblTestPointY = 35.2577677;
      //booInPolygon = PointInPoly_Winding(new double[] { dblTestPointX, dblTestPointY }, dblPolygonRings);
      //Console.WriteLine("SHOULD BE OUT BETWEEN EXTERNAL REGION AND CENTER ISLAND:  Point in Polygon = " + booInPolygon.ToString());
      ////SHOULD BE IN SEPARATE ISLAND
      //dblTestPointX = -111.5804540;
      //dblTestPointY = 35.2574054;
      //booInPolygon = PointInPoly_Winding(new double[] { dblTestPointX, dblTestPointY }, dblPolygonRings);
      //Console.WriteLine("SHOULD BE IN SEPARATE ISLAND:  Point in Polygon = " + booInPolygon.ToString());
      //// ONE OF OUTER RING VERTICES:  RETURNS FALSE
      //dblTestPointX = -111.58015220600000;
      //dblTestPointY = 35.25729186900000;
      //booInPolygon = PointInPoly_Winding(new double[] { dblTestPointX, dblTestPointY }, dblPolygonRings);
      //Console.WriteLine("ONE OF OUTER RING VERTICES:  Point in Polygon = " + booInPolygon.ToString());
      //// ONE OF INNER RING VERTICES:  RETURNS FALSE
      //dblTestPointX = -111.58208719000000;
      //dblTestPointY = 35.25764445300010;
      //booInPolygon = PointInPoly_Winding(new double[] { dblTestPointX, dblTestPointY }, dblPolygonRings);
      //Console.WriteLine("ONE OF INNER RING VERTICES:  Point in Polygon = " + booInPolygon.ToString());

      ////booInPolygon = PointInPoly_Winding(new double[] { dblTestPointX, dblTestPointY },  dblPolygonRings);
      ////Console.WriteLine("Point in Polygon = " + booInPolygon.ToString());

      //double dblMilliseconds;
      //sw.Start();
      //long lngCounter = 0;
      //for (int i = 0; i < 1000000; i++)
      //{
      //  lngCounter++;
      //  booInPolygon = PointInPoly_Winding(new double[] { dblTestPointX, dblTestPointY }, dblPolygonRings);
      //}
      //sw.Stop();
      //dblMilliseconds = sw.ElapsedMilliseconds;
      //Console.Write("For Loop (" + lngCounter.ToString("#,##0") + " iterations): " + ((double)sw.ElapsedMilliseconds / 1000).ToString("0.000") + " stopwatch seconds\n");
      //Console.Write("For Loop (" + lngCounter.ToString("#,##0") + " iterations): " + dblMilliseconds.ToString("0.000") + " milliseconds\n");
      //Console.Write("For Loop (" + lngCounter.ToString("#,##0") + " iterations): " + (dblMilliseconds / 1000).ToString("0.000") + " seconds\n");
    }

    /// <summary>
    /// Given an envelope, creates a point at the center of the extent.  Also fills X- and Y-coordinate variables.
    /// </summary>
    /// <param name="dblEnvelope"></param>
    /// <param name="dblMidpointX"></param>
    /// <param name="dblMidpointY"></param>
    /// <returns></returns>
    public static double[] ReturnGeometryEnvelopeMidpoint(double[] dblEnvelope, out double dblMidpointX, out double dblMidpointY)
    {
      dblMidpointX = double.NaN;
      dblMidpointY = double.NaN;
      double dblMinX = dblEnvelope[0];
      double dblMinY = dblEnvelope[1];
      double dblMaxX = dblEnvelope[2];
      double dblMaxY = dblEnvelope[3];

      if (double.IsNaN(dblMinX) || double.IsNaN(dblMaxX) || double.IsNaN(dblMinY) || double.IsNaN(dblMaxY)) { return new double[] { double.NaN, double.NaN }; }
      else
      {
        dblMidpointX = dblMinX + ((dblMaxX - dblMinX) / 2);
        dblMidpointY = dblMinY + ((dblMaxY - dblMinY) / 2);
        return new double[] { dblMidpointX, dblMidpointY };
      }
    }
    /// <summary>
    /// Given a multipoint, creates a point at the center of the extent.  Also fills X- and Y-coordinate variables.
    /// </summary>
    /// <param name="dblEnvelope"></param>
    /// <param name="dblMidpointX"></param>
    /// <param name="dblMidpointY"></param>
    /// <returns></returns>
    public static double[] ReturnGeometryEnvelopeMidpoint(double[,] dblMultipoint, out double dblMidpointX, out double dblMidpointY)
    {
      dblMidpointX = double.NaN;
      dblMidpointY = double.NaN;
      _ = ReturnGeometryExtent(dblMultipoint, out double dblMinX, out double dblMinY, out double dblMaxX, out double dblMaxY);

      if (double.IsNaN(dblMinX) || double.IsNaN(dblMaxX) || double.IsNaN(dblMinY) || double.IsNaN(dblMaxY)) { return new double[] { double.NaN, double.NaN }; }
      else
      {
        dblMidpointX = dblMinX + ((dblMaxX - dblMinX) / 2);
        dblMidpointY = dblMinY + ((dblMaxY - dblMinY) / 2);
        return new double[] { dblMidpointX, dblMidpointY };
      }
    }
    /// <summary>
    /// Given a polygon or polyline, creates a point at the center of the extent.  Also fills X- and Y-coordinate variables.
    /// </summary>
    /// <param name="dblEnvelope"></param>
    /// <param name="dblMidpointX"></param>
    /// <param name="dblMidpointY"></param>
    /// <returns></returns>
    public static double[] ReturnGeometryEnvelopeMidpoint(double[][,] dblPolygonOrPolyline, out double dblMidpointX, out double dblMidpointY)
    {
      dblMidpointX = double.NaN;
      dblMidpointY = double.NaN;
      _ = ReturnGeometryExtent(dblPolygonOrPolyline, out double dblMinX, out double dblMinY, out double dblMaxX, out double dblMaxY);

      if (double.IsNaN(dblMinX) || double.IsNaN(dblMaxX) || double.IsNaN(dblMinY) || double.IsNaN(dblMaxY)) { return new double[] { double.NaN, double.NaN }; }
      else
      {
        dblMidpointX = dblMinX + ((dblMaxX - dblMinX) / 2);
        dblMidpointY = dblMinY + ((dblMaxY - dblMinY) / 2);
        return new double[] { dblMidpointX, dblMidpointY };
      }
    }

    /// <summary>
    /// Given a Point double array, returns a 4-element Double envelope array and fills Minimum and Maximum X/Y values
    /// </summary>
    /// <param name="dblPolygonOrPolyline"></param>
    /// <param name="dblMinX"></param>
    /// <param name="dblMinY"></param>
    /// <param name="dblMaxX"></param>
    /// <param name="dblMaxY"></param>
    /// <returns></returns>
    public static double[] ReturnGeometryExtent(double[] dblPoint, out double dblMinX, out double dblMinY, out double dblMaxX, out double dblMaxY)
    {
      dblMinX = dblPoint[0];
      dblMinY = dblPoint[1];
      dblMaxX = dblPoint[0];
      dblMaxY = dblPoint[1];

      return new double[] { dblMinX, dblMinY, dblMaxX, dblMaxY };
    }
    /// <summary>
    /// Given a Multipoint double array, returns a 4-element Double envelope array and fills Minimum and Maximum X/Y values
    /// </summary>
    /// <param name="dblPolygonOrPolyline"></param>
    /// <param name="dblMinX"></param>
    /// <param name="dblMinY"></param>
    /// <param name="dblMaxX"></param>
    /// <param name="dblMaxY"></param>
    /// <returns></returns>
    public static double[] ReturnGeometryExtent(double[,] dblMultipoint,
                                                out double dblMinX,
                                                out double dblMinY,
                                                out double dblMaxX,
                                                out double dblMaxY)
    {
      dblMinX = double.NaN;
      dblMinY = double.NaN;
      dblMaxX = double.NaN;
      dblMaxY = double.NaN;
      double dblTestX;
      double dblTestY;

      for (int i = 0; i < dblMultipoint.GetLength(0); i++)
      {
        dblTestX = dblMultipoint[i, 0];
        dblTestY = dblMultipoint[i, 1];
        dblMinX = (double.IsNaN(dblMinX)) ? dblTestX : Math.Min(dblMinX, dblTestX);
        dblMaxX = (double.IsNaN(dblMaxX)) ? dblTestX : Math.Max(dblMaxX, dblTestX);
        dblMinY = (double.IsNaN(dblMinY)) ? dblTestY : Math.Min(dblMinY, dblTestY);
        dblMaxY = (double.IsNaN(dblMaxY)) ? dblTestY : Math.Max(dblMaxY, dblTestY);
      }

      return new double[] { dblMinX, dblMinY, dblMaxX, dblMaxY };
      //double dblMinX;
      //double dblMinY;
      //double dblMaxX;
      //double dblMaxY;

      //dblEnvelope = ReturnGeometryExtent(dblMultipointCoords, out dblMinX, out dblMinY, out dblMaxX, out dblMaxY);
      //Console.WriteLine("Multipoint Extent = [" + dblMinX.ToString("0.00000") + ", " + dblMinY.ToString("0.00000") + ", " + dblMaxX.ToString("0.00000") + ", " + dblMaxY.ToString("0.00000") + "]");
    }
    /// <summary>
    /// Given a Polygon or Polyline jagged array, returns a 4-element Double envelope array and fills Minimum and Maximum X/Y values
    /// </summary>
    /// <param name="dblPolygonOrPolyline"></param>
    /// <param name="dblMinX"></param>
    /// <param name="dblMinY"></param>
    /// <param name="dblMaxX"></param>
    /// <param name="dblMaxY"></param>
    /// <returns></returns>
    public static double[] ReturnGeometryExtent(double[][,] dblPolygonOrPolyline, out double dblMinX, out double dblMinY, out double dblMaxX, out double dblMaxY)
    {
      dblMinX = double.NaN;
      dblMinY = double.NaN;
      dblMaxX = double.NaN;
      dblMaxY = double.NaN;
      double dblTestX;
      double dblTestY;

      foreach (double[,] dblRing in dblPolygonOrPolyline)
      {
        for (int i = 0; i < dblRing.GetLength(0); i++)
        {
          dblTestX = dblRing[i, 0];
          dblTestY = dblRing[i, 1];
          dblMinX = (double.IsNaN(dblMinX)) ? dblTestX : Math.Min(dblMinX, dblTestX);
          dblMaxX = (double.IsNaN(dblMaxX)) ? dblTestX : Math.Max(dblMaxX, dblTestX);
          dblMinY = (double.IsNaN(dblMinY)) ? dblTestY : Math.Min(dblMinY, dblTestY);
          dblMaxY = (double.IsNaN(dblMaxY)) ? dblTestY : Math.Max(dblMaxY, dblTestY);
        }
      }
      return new double[] { dblMinX, dblMinY, dblMaxX, dblMaxY };

      //double dblMinX;
      //double dblMinY;
      //double dblMaxX;
      //double dblMaxY;

      //double[] dblEnvelope = ReturnGeometryExtent(dblPolygonRings, out dblMinX, out dblMinY, out dblMaxX, out dblMaxY);
      //Console.WriteLine("Polygon Extent = [" + dblMinX.ToString("0.00000") + ", " + dblMinY.ToString("0.00000") + ", " + dblMaxX.ToString("0.00000") + ", " + dblMaxY.ToString("0.00000") + "]");
    }

    /// <summary>
    /// Given a longitude, latitude, date and elevation, returns the magnetic declination and inclination at that point<br>
    /// </br>Currently includes IGRF parameters good from 1990 through 2030.<br>
    /// </br>Fills Double values for magnetic declination and inclination.  <br>
    /// </br>Optionally returns North, East and Vertical component of magnetic force, plus intensity, instead of declination and inclination.<br>
    /// </br><br></br>
    /// Adapted from subroutine igrf13syn at https://www.ngdc.noaa.gov/IAGA/vmod/igrf13.f    /// 
    /// </summary>
    /// <param name="dblLat"></param>
    /// <param name="dblLong"></param>
    /// <param name="dblYear"></param>
    /// <param name="lngISV"></param>
    /// <param name="dblAlt"></param>
    /// <param name="lngType"></param>
    /// <param name="dblGH"></param>
    /// <param name="dblP"></param>
    /// <param name="dblQ"></param>
    /// <param name="dblCL"></param>
    /// <param name="dblSL"></param>
    /// <param name="dblX"></param>
    /// <param name="dblY"></param>
    /// <param name="dblZ"></param>
    /// <param name="dblF"></param>
    /// <param name="dblDec"></param>
    /// <param name="dblInc"></param>
    /// <param name="booSucceeded"></param>
    /// <param name="strFailReason"></param>
    public static void CalcGeomagneticElements(double[] dblLatLongPoint, double dblYear, long lngISV, double dblAlt,
                                        long lngType, double[] dblGH, double[] dblP, double[] dblQ, double[] dblCL,
                                        double[] dblSL, out double dblX, out double dblY, out double dblZ,
                                        out double dblF, out double dblDec, out double dblInc, out bool booSucceeded,
                                        out string strFailReason) => CalcGeomagneticElements(dblLatLongPoint[0], dblLatLongPoint[1], dblYear, lngISV, dblAlt, lngType, dblGH, dblP, dblQ, dblCL, dblSL, out dblX, out dblY, out dblZ, out dblF, out dblDec, out dblInc, out booSucceeded, out strFailReason);
    /// <summary>
    /// Given a longitude, latitude, date and elevation, returns the magnetic declination and inclination at that point<br>
    /// </br>Currently includes IGRF parameters good from 1990 through 2030.<br>
    /// </br>Fills Double values for magnetic declination and inclination.  <br>
    /// </br>Optionally returns North, East and Vertical component of magnetic force, plus intensity, instead of declination and inclination.<br>
    /// </br><br></br>
    /// Adapted from subroutine igrf13syn at https://www.ngdc.noaa.gov/IAGA/vmod/igrf13.f    /// 
    /// </summary>
    /// <param name="dblLat"></param>
    /// <param name="dblLong"></param>
    /// <param name="dblYear"></param>
    /// <param name="lngISV"></param>
    /// <param name="dblAlt"></param>
    /// <param name="lngType"></param>
    /// <param name="dblGH"></param>
    /// <param name="dblP"></param>
    /// <param name="dblQ"></param>
    /// <param name="dblCL"></param>
    /// <param name="dblSL"></param>
    /// <param name="dblX"></param>
    /// <param name="dblY"></param>
    /// <param name="dblZ"></param>
    /// <param name="dblF"></param>
    /// <param name="dblDec"></param>
    /// <param name="dblInc"></param>
    /// <param name="booSucceeded"></param>
    /// <param name="strFailReason"></param>
    public static void CalcGeomagneticElements(double dblLat, double dblLong, double dblYear, long lngISV, double dblAlt,
                                        long lngType, double[] dblGH, double[] dblP, double[] dblQ, double[] dblCL,
                                        double[] dblSL, out double dblX, out double dblY, out double dblZ,
                                        out double dblF, out double dblDec, out double dblInc, out bool booSucceeded,
                                        out string strFailReason)
    {
      //  ADAPTED BY JEFF JENNESS, NOVEMBER 11, 2015
      //  FURTHER ADAPTED BY JEFF JENNESS, February 28, 2021
      //  http://www.ngdc.noaa.gov/IAGA/vmod/igrf.html
      //  see also https://github.com/dpq/finalfrontier/blob/master/igrf11.c
      // =============================================================================
      //     subroutine igrf13syn(isv, Date, itype, Alt, colat, elong, X, Y, Z, F)
      //     https://www.ngdc.noaa.gov/IAGA/vmod/igrf13.f
      //     This is a synthesis routine for the 13th generation IGRF as agreed
      //     in December 2019 by IAGA Working Group V-MOD. It is valid 1900.0 to
      //     2025.0 inclusive. Values for dates from 1945.0 to 2015.0 inclusive are
      //     definitive, otherwise they are non-definitive.
      //   INPUT
      //     isv   = 0 if main-field values are required
      //     isv   = 1 if secular variation values are required
      //     date  = year A.D. Must be greater than or equal to 1900.0 and
      //             less than or equal to 2030.0. Warning message is given
      //             for dates greater than 2025.0. Must be double precision.
      //     itype = 1 if geodetic (spheroid)
      //     itype = 2 if geocentric (sphere)
      //     alt   = height in km above sea level if itype = 1
      //           = distance from centre of Earth in km if itype = 2 (>3485 km)
      // colat = colatitude(0 - 180)
      // elong = east - Longitude(0 - 360)
      //     alt, colat and elong must be double precision.
      // Output
      //     x     = north component (nT) if isv = 0, nT/year if isv = 1
      //     y     = east component (nT) if isv = 0, nT/year if isv = 1
      //     z     = vertical component (nT) if isv = 0, nT/year if isv = 1
      //     f     = total intensity (nT) if isv = 0, rubbish if isv = 1
      //
      //     To get the other geomagnetic elements (D, I, H and secular
      //     variations dD, dH, dI and dF) use routines ptoc and ptocsv.
      //
      //     Adapted from 8th generation version to include new maximum degree for
      //     main-field models for 2000.0 and onwards and use WGS84 spheroid instead
      //     of International Astronomical Union 1966 spheroid as recommended by IAGA
      //     in July 2003. Reference radius remains as 6371.2 km - it is NOT the mean
      //     radius (= 6371.0 km) but 6371.2 km is what is used in determining the
      //     coefficients. Adaptation by Susan Macmillan, August 2003 (for
      //     9th generation), December 2004, December 2009 & December 2014;
      //     by William Brown, December 2019, February 2020.
      //
      //     Coefficients at 1995.0 incorrectly rounded (rounded up instead of
      //     to even) included as these are the coefficients published in Excel
      //     spreadsheet July 2005.
      // ----------------------------------------------------------------------------

      double dblELongitude = (360d + dblLong) % 360d;   // East - Longitude; between 0 and 360
      double dblCoLatitude = 90d - dblLat;              // Between 0 and 180, 0 at North Pole
      booSucceeded = true;
      strFailReason = "";

      // Bail out if year < 1900
      if (dblYear < 1900)
      {
        booSucceeded = false;
        strFailReason = "Date < 1900: Geomagnetic coefficients not valid before that date.";
        dblX = double.NaN;
        dblY = double.NaN;
        dblZ = double.NaN;
        dblF = double.NaN;
        dblDec = double.NaN;
        dblInc = double.NaN;
        return;
      }
      // force dblYear to be <= 2030: update when new coefficients added
      if (dblYear > 2030)
      {
        dblYear = 2030;
        booSucceeded = false;
        strFailReason = "Date > 2030: Values calculated for December 31, 2030.";
      }

      dblX = 0;
      dblY = 0;
      dblZ = 0;
      double dblT;
      double dblTC;
      long lngLL;
      long lngNMX;
      long lngNC;
      long lngKMX;
      double dblONE;

      if (dblYear >= 2020)
      {
        if (lngISV == 1)
        {
          dblT = 1;
          dblTC = 0;
        }
        else
        {
          dblT = dblYear - 2020d;
          dblTC = 1;
        }
        // pointer for last coefficient in pen-ultimate set of MF coefficients...
        lngLL = 3255; // was 3060
        lngNMX = 13;
        lngNC = lngNMX * (lngNMX + 2);
        lngKMX = (lngNMX + 1) * (lngNMX + 2) / 2;
      }
      else
      {
        dblT = 0.2 * (dblYear - 1900d);
        lngLL = (long)dblT;
        dblONE = lngLL;
        dblT -= dblONE;
        // SH models before 1995.0 are only to degree 10
        if (dblYear < 1995)
        {
          lngNMX = 10;
          lngNC = lngNMX * (lngNMX + 2);
          lngLL = lngNC * lngLL;
          lngKMX = (lngNMX + 1) * (lngNMX + 2) / 2;
        }
        else
        {
          lngNMX = 13;
          lngNC = lngNMX * (lngNMX + 2);
          lngLL = (long)(0.2 * (dblYear - 1995d));

          // 19 is the number of SH models that extend to degree 10
          lngLL = 120 * 19 + (lngNC * lngLL);   // REMEMBER THAT C#/VBA ARRAYS ARE 0-BASED, UNLIKE THE ORIGINAL FORTRAN ARRAY
          lngKMX = (lngNMX + 1) * (lngNMX + 2) / 2;
        }
        dblTC = 1d - dblT;
        if (lngISV == 1)
        {
          dblTC = -0.2;
          dblT = 0.2;
        }
      }
      double dblR = dblAlt;
      dblONE = DegToRad(dblCoLatitude);
      double dblCT = Math.Cos(dblONE);
      double dblST = Math.Sin(dblONE);
      dblONE = DegToRad(dblELongitude);
      dblCL[1] = Math.Cos(dblONE);
      dblSL[1] = Math.Sin(dblONE);
      double dblCD = 1;
      double dblSD = 0;
      long lngL = 1;
      long lngM = 1;
      long lngN = 0;
      double dblFN = 0;
      double dblGN = 0;
      double dblFM;
      double dblGMM;
      double dblTwo;
      double dblThree;
      long lngI;
      long lngJ;

      if (lngType == 1)
      {
        // conversion from geodetic to geocentric coordinates (using the WGS84 spheroid)
        double dblA2 = 40680631.6;
        double dblB2 = 40408296d;
        dblONE = dblA2 * dblST * dblST;
        dblTwo = dblB2 * dblCT * dblCT;
        dblThree = dblONE + dblTwo;
        double dblRho = Math.Sqrt(dblThree);
        dblR = Math.Sqrt(dblAlt * (dblAlt + 2d * dblRho) + (dblA2 * dblONE + dblB2 * dblTwo) / dblThree);
        dblCD = (dblAlt + dblRho) / dblR;
        dblSD = (dblA2 - dblB2) / dblRho * dblCT * dblST / dblR;
        dblONE = dblCT;
        dblCT = (dblCT * dblCD) - (dblST * dblSD);
        dblST = (dblST * dblCD) + (dblONE * dblSD);
      }

      double dblRatio = 6371.2 / dblR;
      double dblRR = dblRatio * dblRatio;

      // computation of Schmidt quasi-normal coefficients p and x(=q)

      dblP[1] = 1;       // was Index 1
      dblP[3] = dblST;   // was Index 3
      dblQ[1] = 0;       // was Index 1
      dblQ[3] = dblCT;   // was Index 3

      for (long lngK = 2; lngK <= lngKMX; lngK++)
      {
        if (lngN < lngM)
        {
          lngM = 0;
          lngN++;
          dblRR *= dblRatio;
          dblFN = (double)lngN;
          dblGN = (double)(lngN - 1);
        }
        dblFM = (double)lngM;
        if (lngM != lngN)
        {
          dblGMM = (double)(lngM * lngM);
          dblONE = Math.Sqrt((dblFN * dblFN) - dblGMM);
          dblTwo = Math.Sqrt((dblGN * dblGN) - dblGMM) / dblONE;
          dblThree = (dblFN + dblGN) / dblONE;
          lngI = lngK - lngN;
          lngJ = lngI - lngN + 1;
          dblP[lngK] = (dblThree * dblCT * dblP[lngI]) - (dblTwo * dblP[lngJ]);
          dblQ[lngK] = (dblThree * (dblCT * dblQ[lngI] - dblST * dblP[lngI])) - dblTwo * dblQ[lngJ];
        }
        else
        {
          if (lngK != 3)
          {
            dblONE = Math.Sqrt(1d - (0.5 / dblFM));
            lngJ = lngK - lngN - 1;
            dblP[lngK] = dblONE * dblST * dblP[lngJ];
            dblQ[lngK] = dblONE * (dblST * dblQ[lngJ] + dblCT * dblP[lngJ]);
            dblCL[lngM] = (dblCL[lngM - 1] * dblCL[1]) - (dblSL[lngM - 1] * dblSL[1]);
            dblSL[lngM] = (dblSL[lngM - 1] * dblCL[1]) + (dblCL[lngM - 1] * dblSL[1]);
          }
        }

        // synthesis of x, y and z in geocentric coordinates
        long lngLM = lngLL + lngL;
        dblONE = (dblTC * dblGH[lngLM] + dblT * dblGH[lngLM + lngNC]) * dblRR;

        if (lngM == 0)
        {
          dblX += dblONE * dblQ[lngK];
          dblZ -= (dblFN + 1d) * dblONE * dblP[lngK];
          lngL++;
        }
        else
        {
          dblTwo = ((dblTC * dblGH[lngLM + 1]) + (dblT * dblGH[lngLM + lngNC + 1])) * dblRR;
          dblThree = (dblONE * dblCL[lngM]) + (dblTwo * dblSL[lngM]);
          dblX += dblThree * dblQ[lngK];
          dblZ -= ((dblFN + 1d) * dblThree * dblP[lngK]);
          if (dblST == 0) { dblY += ((dblONE * dblSL[lngM]) - (dblTwo * dblCL[lngM])) * dblQ[lngK] * dblCT; }
          else { dblY += ((dblONE * dblSL[lngM]) - (dblTwo * dblCL[lngM])) * dblFM * dblP[lngK] / dblST; }
          lngL += 2;
        }
        lngM++;
      }
      dblONE = dblX;
      dblX = (dblX * dblCD) + (dblZ * dblSD);
      dblZ = (dblZ * dblCD) - (dblONE * dblSD);
      dblF = Math.Sqrt((dblX * dblX) + (dblY * dblY) + (dblZ * dblZ));

      dblDec = RadToDeg(Math.Atan2(dblY, dblX));
      dblInc = RadToDeg(Math.Atan2(dblZ, Math.Sqrt(Math.Pow(dblX, 2) + Math.Pow(dblY, 2))));

      //Sample Code
      //FillIGRFCoefficients(out double[] dblGH, out double[] dblP, out double[] dblQ, out double[] dblCL, out double[] dblSL);
      //Console.WriteLine(dblGH.GetLength(0).ToString("#,#00") + " elements in dblGH...");

      //double dblLat = 35;                 // Latitude, -90 (South Pole) to 90 (North Pole)
      //double dblLong = -112;              // longitude, -180 to 180 (Greenwich = 0)
      //long lngISV = 0;                    // 0 if main-field values required, 1 if secular variation values required
      //double dblYear = 2021.16164383562;  // Year in decimal format.  Sample is February 28, 2021, 12:05 pm
      //long lngType = 1;                   // 1 if on spheroid, 2 if on sphere
      //double dblAlt = 2;                  //  height in km above MSL if lngType = 1 (spheroid).  Otherwise distance from center of earth
      //                                    // (>3485km) if lngType = 2 (sphere)
      //                                    // Sample uses Spheroid, so "2" = 2km above MSL.

      //// VALUES TO BE RETURNED
      //double dblX;      // north component (nT) if isv = 0, nT/year if isv = 1
      //double dblY;      // east component (nT) if isv = 0, nT/year if isv = 1
      //double dblZ;      // vertical component (nT) if isv = 0, nT/year if isv = 1
      //double dblF;      // total intensity (nT) if isv = 0, rubbish if isv = 1
      //double dblDec;    // Declination in Degrees
      //double dblInc;    // Inclination in Degrees
      //bool booSucceeded;
      //string strFailureReason;

      //sw.Stop();
      ////Console.Write("Vincenty 1,000,000 times (" + dblCounter + "): " + sw.ElapsedMilliseconds + " milliseconds \n");

      //CalcGeomagneticElements(dblLat, dblLong, dblYear, lngISV, dblAlt, lngType, dblGH, dblP, dblQ, dblCL, dblSL, out dblX, out dblY, out dblZ, out dblF, out dblDec, out dblInc, out booSucceeded, out strFailureReason);
      //Console.WriteLine("    --> Succeeded = " + booSucceeded.ToString() + "...Failure Reason = " + strFailureReason);
      //Console.WriteLine("    --> Latitude = " + dblLat.ToString("0.0000"));
      //Console.WriteLine("    --> Longitude = " + dblLong.ToString("0.0000"));
      //Console.WriteLine("    --> Date = " + DateTime.Now.ToString());
      //Console.WriteLine("    --> X = " + dblX.ToString("0.0000"));
      //Console.WriteLine("    --> Y = " + dblY.ToString("0.0000"));
      //Console.WriteLine("    --> Z = " + dblZ.ToString("0.0000"));
      //Console.WriteLine("    --> F = " + dblF.ToString("0.0000"));
      //Console.WriteLine("    --> Declination = " + dblDec.ToString("0.0000"));
      //Console.WriteLine("    --> Inclination = " + dblInc.ToString("0.0000"));
    }
    /// <summary>
    /// Fills IGRF (International Geomagnetic Reference Field) parameters to calculate magnetic declination<br>
    /// </br>https://www.ngdc.noaa.gov/IAGA/vmod/coeffs/igrf13coeffs.txt
    /// </summary>
    /// <param name="dblGH"></param>
    /// <param name="dblP"></param>
    /// <param name="dblQ"></param>
    /// <param name="dblCL"></param>
    /// <param name="dblSL"></param>
    public static void FillIGRFCoefficients(out double[] dblGH, out double[] dblP, out double[] dblQ, out double[] dblCL, out double[] dblSL)
    {
      dblP = new double[106];
      dblQ = new double[106];
      dblCL = new double[14];
      dblSL = new double[14];
      dblGH = new double[] {0.00, -31543.00, -2298.00, 5922.00, -677.00, 2905.00, -1061.00, 924.00, 1121.00, 1022.00, -1469.00, -330.00, 1256.00, 3.00, 572.00,
        523.00, 876.00, 628.00, 195.00, 660.00, -69.00, -361.00, -210.00, 134.00, -75.00, -184.00, 328.00, -210.00, 264.00, 53.00,
        5.00, -33.00, -86.00, -124.00, -16.00, 3.00, 63.00, 61.00, -9.00, -11.00, 83.00, -217.00, 2.00, -58.00, -35.00,
        59.00, 36.00, -90.00, -69.00, 70.00, -55.00, -45.00, 0.00, -13.00, 34.00, -10.00, -41.00, -1.00, -21.00, 28.00,
        18.00, -12.00, 6.00, -22.00, 11.00, 8.00, 8.00, -4.00, -14.00, -9.00, 7.00, 1.00, -13.00, 2.00, 5.00,
        -9.00, 16.00, 5.00, -5.00, 8.00, -18.00, 8.00, 10.00, -20.00, 1.00, 14.00, -11.00, 5.00, 12.00, -3.00,
        1.00, -2.00, -2.00, 8.00, 2.00, 10.00, -1.00, -2.00, -1.00, 2.00, -3.00, -4.00, 2.00, 2.00, 1.00,
        -5.00, 2.00, -2.00, 6.00, 6.00, -4.00, 4.00, 0.00, 0.00, -2.00, 2.00, 4.00, 2.00, 0.00, 0.00,
        -6.00, -31464.00, -2298.00, 5909.00, -728.00, 2928.00, -1086.00, 1041.00, 1065.00, 1037.00, -1494.00, -357.00, 1239.00, 34.00, 635.00,
        480.00, 880.00, 643.00, 203.00, 653.00, -77.00, -380.00, -201.00, 146.00, -65.00, -192.00, 328.00, -193.00, 259.00, 56.00,
        -1.00, -32.00, -93.00, -125.00, -26.00, 11.00, 62.00, 60.00, -7.00, -11.00, 86.00, -221.00, 4.00, -57.00, -32.00,
        57.00, 32.00, -92.00, -67.00, 70.00, -54.00, -46.00, 0.00, -14.00, 33.00, -11.00, -41.00, 0.00, -20.00, 28.00,
        18.00, -12.00, 6.00, -22.00, 11.00, 8.00, 8.00, -4.00, -15.00, -9.00, 7.00, 1.00, -13.00, 2.00, 5.00,
        -8.00, 16.00, 5.00, -5.00, 8.00, -18.00, 8.00, 10.00, -20.00, 1.00, 14.00, -11.00, 5.00, 12.00, -3.00,
        1.00, -2.00, -2.00, 8.00, 2.00, 10.00, 0.00, -2.00, -1.00, 2.00, -3.00, -4.00, 2.00, 2.00, 1.00,
        -5.00, 2.00, -2.00, 6.00, 6.00, -4.00, 4.00, 0.00, 0.00, -2.00, 2.00, 4.00, 2.00, 0.00, 0.00,
        -6.00, -31354.00, -2297.00, 5898.00, -769.00, 2948.00, -1128.00, 1176.00, 1000.00, 1058.00, -1524.00, -389.00, 1223.00, 62.00, 705.00,
        425.00, 884.00, 660.00, 211.00, 644.00, -90.00, -400.00, -189.00, 160.00, -55.00, -201.00, 327.00, -172.00, 253.00, 57.00,
        -9.00, -33.00, -102.00, -126.00, -38.00, 21.00, 62.00, 58.00, -5.00, -11.00, 89.00, -224.00, 5.00, -54.00, -29.00,
        54.00, 28.00, -95.00, -65.00, 71.00, -54.00, -47.00, 1.00, -14.00, 32.00, -12.00, -40.00, 1.00, -19.00, 28.00,
        18.00, -13.00, 6.00, -22.00, 11.00, 8.00, 8.00, -4.00, -15.00, -9.00, 6.00, 1.00, -13.00, 2.00, 5.00,
        -8.00, 16.00, 5.00, -5.00, 8.00, -18.00, 8.00, 10.00, -20.00, 1.00, 14.00, -11.00, 5.00, 12.00, -3.00,
        1.00, -2.00, -2.00, 8.00, 2.00, 10.00, 0.00, -2.00, -1.00, 2.00, -3.00, -4.00, 2.00, 2.00, 1.00,
        -5.00, 2.00, -2.00, 6.00, 6.00, -4.00, 4.00, 0.00, 0.00, -2.00, 2.00, 4.00, 2.00, 0.00, 0.00,
        -6.00, -31212.00, -2306.00, 5875.00, -802.00, 2956.00, -1191.00, 1309.00, 917.00, 1084.00, -1559.00, -421.00, 1212.00, 84.00, 778.00,
        360.00, 887.00, 678.00, 218.00, 631.00, -109.00, -416.00, -173.00, 178.00, -51.00, -211.00, 327.00, -148.00, 245.00, 58.00,
        -16.00, -34.00, -111.00, -126.00, -51.00, 32.00, 61.00, 57.00, -2.00, -10.00, 93.00, -228.00, 8.00, -51.00, -26.00,
        49.00, 23.00, -98.00, -62.00, 72.00, -54.00, -48.00, 2.00, -14.00, 31.00, -12.00, -38.00, 2.00, -18.00, 28.00,
        19.00, -15.00, 6.00, -22.00, 11.00, 8.00, 8.00, -4.00, -15.00, -9.00, 6.00, 2.00, -13.00, 3.00, 5.00,
        -8.00, 16.00, 6.00, -5.00, 8.00, -18.00, 8.00, 10.00, -20.00, 1.00, 14.00, -11.00, 5.00, 12.00, -3.00,
        1.00, -2.00, -2.00, 8.00, 2.00, 10.00, 0.00, -2.00, -1.00, 2.00, -3.00, -4.00, 2.00, 2.00, 1.00,
        -5.00, 2.00, -2.00, 6.00, 6.00, -4.00, 4.00, 0.00, 0.00, -2.00, 1.00, 4.00, 2.00, 0.00, 0.00,
        -6.00, -31060.00, -2317.00, 5845.00, -839.00, 2959.00, -1259.00, 1407.00, 823.00, 1111.00, -1600.00, -445.00, 1205.00, 103.00, 839.00,
        293.00, 889.00, 695.00, 220.00, 616.00, -134.00, -424.00, -153.00, 199.00, -57.00, -221.00, 326.00, -122.00, 236.00, 58.00,
        -23.00, -38.00, -119.00, -125.00, -62.00, 43.00, 61.00, 55.00, 0.00, -10.00, 96.00, -233.00, 11.00, -46.00, -22.00,
        44.00, 18.00, -101.00, -57.00, 73.00, -54.00, -49.00, 2.00, -14.00, 29.00, -13.00, -37.00, 4.00, -16.00, 28.00,
        19.00, -16.00, 6.00, -22.00, 11.00, 7.00, 8.00, -3.00, -15.00, -9.00, 6.00, 2.00, -14.00, 4.00, 5.00,
        -7.00, 17.00, 6.00, -5.00, 8.00, -19.00, 8.00, 10.00, -20.00, 1.00, 14.00, -11.00, 5.00, 12.00, -3.00,
        1.00, -2.00, -2.00, 9.00, 2.00, 10.00, 0.00, -2.00, -1.00, 2.00, -3.00, -4.00, 2.00, 2.00, 1.00,
        -5.00, 2.00, -2.00, 6.00, 6.00, -4.00, 4.00, 0.00, 0.00, -2.00, 1.00, 4.00, 3.00, 0.00, 0.00,
        -6.00, -30926.00, -2318.00, 5817.00, -893.00, 2969.00, -1334.00, 1471.00, 728.00, 1140.00, -1645.00, -462.00, 1202.00, 119.00, 881.00,
        229.00, 891.00, 711.00, 216.00, 601.00, -163.00, -426.00, -130.00, 217.00, -70.00, -230.00, 326.00, -96.00, 226.00, 58.00,
        -28.00, -44.00, -125.00, -122.00, -69.00, 51.00, 61.00, 54.00, 3.00, -9.00, 99.00, -238.00, 14.00, -40.00, -18.00,
        39.00, 13.00, -103.00, -52.00, 73.00, -54.00, -50.00, 3.00, -14.00, 27.00, -14.00, -35.00, 5.00, -14.00, 29.00,
        19.00, -17.00, 6.00, -21.00, 11.00, 7.00, 8.00, -3.00, -15.00, -9.00, 6.00, 2.00, -14.00, 4.00, 5.00,
        -7.00, 17.00, 7.00, -5.00, 8.00, -19.00, 8.00, 10.00, -20.00, 1.00, 14.00, -11.00, 5.00, 12.00, -3.00,
        1.00, -2.00, -2.00, 9.00, 2.00, 10.00, 0.00, -2.00, -1.00, 2.00, -3.00, -4.00, 2.00, 2.00, 1.00,
        -5.00, 2.00, -2.00, 6.00, 6.00, -4.00, 4.00, 0.00, 0.00, -2.00, 1.00, 4.00, 3.00, 0.00, 0.00,
        -6.00, -30805.00, -2316.00, 5808.00, -951.00, 2980.00, -1424.00, 1517.00, 644.00, 1172.00, -1692.00, -480.00, 1205.00, 133.00, 907.00,
        166.00, 896.00, 727.00, 205.00, 584.00, -195.00, -422.00, -109.00, 234.00, -90.00, -237.00, 327.00, -72.00, 218.00, 60.00,
        -32.00, -53.00, -131.00, -118.00, -74.00, 58.00, 60.00, 53.00, 4.00, -9.00, 102.00, -242.00, 19.00, -32.00, -16.00,
        32.00, 8.00, -104.00, -46.00, 74.00, -54.00, -51.00, 4.00, -15.00, 25.00, -14.00, -34.00, 6.00, -12.00, 29.00,
        18.00, -18.00, 6.00, -20.00, 11.00, 7.00, 8.00, -3.00, -15.00, -9.00, 5.00, 2.00, -14.00, 5.00, 5.00,
        -6.00, 18.00, 8.00, -5.00, 8.00, -19.00, 8.00, 10.00, -20.00, 1.00, 14.00, -12.00, 5.00, 12.00, -3.00,
        1.00, -2.00, -2.00, 9.00, 3.00, 10.00, 0.00, -2.00, -2.00, 2.00, -3.00, -4.00, 2.00, 2.00, 1.00,
        -5.00, 2.00, -2.00, 6.00, 6.00, -4.00, 4.00, 0.00, 0.00, -2.00, 1.00, 4.00, 3.00, 0.00, 0.00,
        -6.00, -30715.00, -2306.00, 5812.00, -1018.00, 2984.00, -1520.00, 1550.00, 586.00, 1206.00, -1740.00, -494.00, 1215.00, 146.00, 918.00,
        101.00, 903.00, 744.00, 188.00, 565.00, -226.00, -415.00, -90.00, 249.00, -114.00, -241.00, 329.00, -51.00, 211.00, 64.00,
        -33.00, -64.00, -136.00, -115.00, -76.00, 64.00, 59.00, 53.00, 4.00, -8.00, 104.00, -246.00, 25.00, -25.00, -15.00,
        25.00, 4.00, -106.00, -40.00, 74.00, -53.00, -52.00, 4.00, -17.00, 23.00, -14.00, -33.00, 7.00, -11.00, 29.00,
        18.00, -19.00, 6.00, -19.00, 11.00, 7.00, 8.00, -3.00, -15.00, -9.00, 5.00, 1.00, -15.00, 6.00, 5.00,
        -6.00, 18.00, 8.00, -5.00, 7.00, -19.00, 8.00, 10.00, -20.00, 1.00, 15.00, -12.00, 5.00, 11.00, -3.00,
        1.00, -3.00, -2.00, 9.00, 3.00, 11.00, 0.00, -2.00, -2.00, 2.00, -3.00, -4.00, 2.00, 2.00, 1.00,
        -5.00, 2.00, -2.00, 6.00, 6.00, -4.00, 4.00, 0.00, 0.00, -1.00, 2.00, 4.00, 3.00, 0.00, 0.00,
        -6.00, -30654.00, -2292.00, 5821.00, -1106.00, 2981.00, -1614.00, 1566.00, 528.00, 1240.00, -1790.00, -499.00, 1232.00, 163.00, 916.00,
        43.00, 914.00, 762.00, 169.00, 550.00, -252.00, -405.00, -72.00, 265.00, -141.00, -241.00, 334.00, -33.00, 208.00, 71.00,
        -33.00, -75.00, -141.00, -113.00, -76.00, 69.00, 57.00, 54.00, 4.00, -7.00, 105.00, -249.00, 33.00, -18.00, -15.00,
        18.00, 0.00, -107.00, -33.00, 74.00, -53.00, -52.00, 4.00, -18.00, 20.00, -14.00, -31.00, 7.00, -9.00, 29.00,
        17.00, -20.00, 5.00, -19.00, 11.00, 7.00, 8.00, -3.00, -14.00, -10.00, 5.00, 1.00, -15.00, 6.00, 5.00,
        -5.00, 19.00, 9.00, -5.00, 7.00, -19.00, 8.00, 10.00, -21.00, 1.00, 15.00, -12.00, 5.00, 11.00, -3.00,
        1.00, -3.00, -2.00, 9.00, 3.00, 11.00, 1.00, -2.00, -2.00, 2.00, -3.00, -4.00, 2.00, 2.00, 1.00,
        -5.00, 2.00, -2.00, 6.00, 6.00, -4.00, 4.00, 0.00, 0.00, -1.00, 2.00, 4.00, 3.00, 0.00, 0.00,
        -6.00, -30594.00, -2285.00, 5810.00, -1244.00, 2990.00, -1702.00, 1578.00, 477.00, 1282.00, -1834.00, -499.00, 1255.00, 186.00, 913.00,
        -11.00, 944.00, 776.00, 144.00, 544.00, -276.00, -421.00, -55.00, 304.00, -178.00, -253.00, 346.00, -12.00, 194.00, 95.00,
        -20.00, -67.00, -142.00, -119.00, -82.00, 82.00, 59.00, 57.00, 6.00, 6.00, 100.00, -246.00, 16.00, -25.00, -9.00,
        21.00, -16.00, -104.00, -39.00, 70.00, -40.00, -45.00, 0.00, -18.00, 0.00, 2.00, -29.00, 6.00, -10.00, 28.00,
        15.00, -17.00, 29.00, -22.00, 13.00, 7.00, 12.00, -8.00, -21.00, -5.00, -12.00, 9.00, -7.00, 7.00, 2.00,
        -10.00, 18.00, 7.00, 3.00, 2.00, -11.00, 5.00, -21.00, -27.00, 1.00, 17.00, -11.00, 29.00, 3.00, -9.00,
        16.00, 4.00, -3.00, 9.00, -4.00, 6.00, -3.00, 1.00, -4.00, 8.00, -3.00, 11.00, 5.00, 1.00, 1.00,
        2.00, -20.00, -5.00, -1.00, -1.00, -6.00, 8.00, 6.00, -1.00, -4.00, -3.00, -2.00, 5.00, 0.00, -2.00,
        -2.00, -30554.00, -2250.00, 5815.00, -1341.00, 2998.00, -1810.00, 1576.00, 381.00, 1297.00, -1889.00, -476.00, 1274.00, 206.00, 896.00,
        -46.00, 954.00, 792.00, 136.00, 528.00, -278.00, -408.00, -37.00, 303.00, -210.00, -240.00, 349.00, 3.00, 211.00, 103.00,
        -20.00, -87.00, -147.00, -122.00, -76.00, 80.00, 54.00, 57.00, -1.00, 4.00, 99.00, -247.00, 33.00, -16.00, -12.00,
        12.00, -12.00, -105.00, -30.00, 65.00, -55.00, -35.00, 2.00, -17.00, 1.00, 0.00, -40.00, 10.00, -7.00, 36.00,
        5.00, -18.00, 19.00, -16.00, 22.00, 15.00, 5.00, -4.00, -22.00, -1.00, 0.00, 11.00, -21.00, 15.00, -8.00,
        -13.00, 17.00, 5.00, -4.00, -1.00, -17.00, 3.00, -7.00, -24.00, -1.00, 19.00, -25.00, 12.00, 10.00, 2.00,
        5.00, 2.00, -5.00, 8.00, -2.00, 8.00, 3.00, -11.00, 8.00, -7.00, -8.00, 4.00, 13.00, -1.00, -2.00,
        13.00, -10.00, -4.00, 2.00, 4.00, -3.00, 12.00, 6.00, 3.00, -3.00, 2.00, 6.00, 10.00, 11.00, 3.00,
        8.00, -30500.00, -2215.00, 5820.00, -1440.00, 3003.00, -1898.00, 1581.00, 291.00, 1302.00, -1944.00, -462.00, 1288.00, 216.00, 882.00,
        -83.00, 958.00, 796.00, 133.00, 510.00, -274.00, -397.00, -23.00, 290.00, -230.00, -229.00, 360.00, 15.00, 230.00, 110.00,
        -23.00, -98.00, -152.00, -121.00, -69.00, 78.00, 47.00, 57.00, -9.00, 3.00, 96.00, -247.00, 48.00, -8.00, -16.00,
        7.00, -12.00, -107.00, -24.00, 65.00, -56.00, -50.00, 2.00, -24.00, 10.00, -4.00, -32.00, 8.00, -11.00, 28.00,
        9.00, -20.00, 18.00, -18.00, 11.00, 9.00, 10.00, -6.00, -15.00, -14.00, 5.00, 6.00, -23.00, 10.00, 3.00,
        -7.00, 23.00, 6.00, -4.00, 9.00, -13.00, 4.00, 9.00, -11.00, -4.00, 12.00, -5.00, 7.00, 2.00, 6.00,
        4.00, -2.00, 1.00, 10.00, 2.00, 7.00, 2.00, -6.00, 5.00, 5.00, -3.00, -5.00, -4.00, -1.00, 0.00,
        2.00, -8.00, -3.00, -2.00, 7.00, -4.00, 4.00, 1.00, -2.00, -3.00, 6.00, 7.00, -2.00, -1.00, 0.00,
        -3.00, -30421.00, -2169.00, 5791.00, -1555.00, 3002.00, -1967.00, 1590.00, 206.00, 1302.00, -1992.00, -414.00, 1289.00, 224.00, 878.00,
        -130.00, 957.00, 800.00, 135.00, 504.00, -278.00, -394.00, 3.00, 269.00, -255.00, -222.00, 362.00, 16.00, 242.00, 125.00,
        -26.00, -117.00, -156.00, -114.00, -63.00, 81.00, 46.00, 58.00, -10.00, 1.00, 99.00, -237.00, 60.00, -1.00, -20.00,
        -2.00, -11.00, -113.00, -17.00, 67.00, -56.00, -55.00, 5.00, -28.00, 15.00, -6.00, -32.00, 7.00, -7.00, 23.00,
        17.00, -18.00, 8.00, -17.00, 15.00, 6.00, 11.00, -4.00, -14.00, -11.00, 7.00, 2.00, -18.00, 10.00, 4.00,
        -5.00, 23.00, 10.00, 1.00, 8.00, -20.00, 4.00, 6.00, -18.00, 0.00, 12.00, -9.00, 2.00, 1.00, 0.00,
        4.00, -3.00, -1.00, 9.00, -2.00, 8.00, 3.00, 0.00, -1.00, 5.00, 1.00, -3.00, 4.00, 4.00, 1.00,
        0.00, 0.00, -1.00, 2.00, 4.00, -5.00, 6.00, 1.00, 1.00, -1.00, -1.00, 6.00, 2.00, 0.00, 0.00,
        -7.00, -30334.00, -2119.00, 5776.00, -1662.00, 2997.00, -2016.00, 1594.00, 114.00, 1297.00, -2038.00, -404.00, 1292.00, 240.00, 856.00,
        -165.00, 957.00, 804.00, 148.00, 479.00, -269.00, -390.00, 13.00, 252.00, -269.00, -219.00, 358.00, 19.00, 254.00, 128.00,
        -31.00, -126.00, -157.00, -97.00, -62.00, 81.00, 45.00, 61.00, -11.00, 8.00, 100.00, -228.00, 68.00, 4.00, -32.00,
        1.00, -8.00, -111.00, -7.00, 75.00, -57.00, -61.00, 4.00, -27.00, 13.00, -2.00, -26.00, 6.00, -6.00, 26.00,
        13.00, -23.00, 1.00, -12.00, 13.00, 5.00, 7.00, -4.00, -12.00, -14.00, 9.00, 0.00, -16.00, 8.00, 4.00,
        -1.00, 24.00, 11.00, -3.00, 4.00, -17.00, 8.00, 10.00, -22.00, 2.00, 15.00, -13.00, 7.00, 10.00, -4.00,
        -1.00, -5.00, -1.00, 10.00, 5.00, 10.00, 1.00, -4.00, -2.00, 1.00, -2.00, -3.00, 2.00, 2.00, 1.00,
        -5.00, 2.00, -2.00, 6.00, 4.00, -4.00, 4.00, 0.00, 0.00, -2.00, 2.00, 3.00, 2.00, 0.00, 0.00,
        -6.00, -30220.00, -2068.00, 5737.00, -1781.00, 3000.00, -2047.00, 1611.00, 25.00, 1287.00, -2091.00, -366.00, 1278.00, 251.00, 838.00,
        -196.00, 952.00, 800.00, 167.00, 461.00, -266.00, -395.00, 26.00, 234.00, -279.00, -216.00, 359.00, 26.00, 262.00, 139.00,
        -42.00, -139.00, -160.00, -91.00, -56.00, 83.00, 43.00, 64.00, -12.00, 15.00, 100.00, -212.00, 72.00, 2.00, -37.00,
        3.00, -6.00, -112.00, 1.00, 72.00, -57.00, -70.00, 1.00, -27.00, 14.00, -4.00, -22.00, 8.00, -2.00, 23.00,
        13.00, -23.00, -2.00, -11.00, 14.00, 6.00, 7.00, -2.00, -15.00, -13.00, 6.00, -3.00, -17.00, 5.00, 6.00,
        0.00, 21.00, 11.00, -6.00, 3.00, -16.00, 8.00, 10.00, -21.00, 2.00, 16.00, -12.00, 6.00, 10.00, -4.00,
        -1.00, -5.00, 0.00, 10.00, 3.00, 11.00, 1.00, -2.00, -1.00, 1.00, -3.00, -3.00, 1.00, 2.00, 1.00,
        -5.00, 3.00, -1.00, 4.00, 6.00, -4.00, 4.00, 0.00, 1.00, -1.00, 0.00, 3.00, 3.00, 1.00, -1.00,
        -4.00, -30100.00, -2013.00, 5675.00, -1902.00, 3010.00, -2067.00, 1632.00, -68.00, 1276.00, -2144.00, -333.00, 1260.00, 262.00, 830.00,
        -223.00, 946.00, 791.00, 191.00, 438.00, -265.00, -405.00, 39.00, 216.00, -288.00, -218.00, 356.00, 31.00, 264.00, 148.00,
        -59.00, -152.00, -159.00, -83.00, -49.00, 88.00, 45.00, 66.00, -13.00, 28.00, 99.00, -198.00, 75.00, 1.00, -41.00,
        6.00, -4.00, -111.00, 11.00, 71.00, -56.00, -77.00, 1.00, -26.00, 16.00, -5.00, -14.00, 10.00, 0.00, 22.00,
        12.00, -23.00, -5.00, -12.00, 14.00, 6.00, 6.00, -1.00, -16.00, -12.00, 4.00, -8.00, -19.00, 4.00, 6.00,
        0.00, 18.00, 10.00, -10.00, 1.00, -17.00, 7.00, 10.00, -21.00, 2.00, 16.00, -12.00, 7.00, 10.00, -4.00,
        -1.00, -5.00, -1.00, 10.00, 4.00, 11.00, 1.00, -3.00, -2.00, 1.00, -3.00, -3.00, 1.00, 2.00, 1.00,
        -5.00, 3.00, -2.00, 4.00, 5.00, -4.00, 4.00, -1.00, 1.00, -1.00, 0.00, 3.00, 3.00, 1.00, -1.00,
        -5.00, -29992.00, -1956.00, 5604.00, -1997.00, 3027.00, -2129.00, 1663.00, -200.00, 1281.00, -2180.00, -336.00, 1251.00, 271.00, 833.00,
        -252.00, 938.00, 782.00, 212.00, 398.00, -257.00, -419.00, 53.00, 199.00, -297.00, -218.00, 357.00, 46.00, 261.00, 150.00,
        -74.00, -151.00, -162.00, -78.00, -48.00, 92.00, 48.00, 66.00, -15.00, 42.00, 93.00, -192.00, 71.00, 4.00, -43.00,
        14.00, -2.00, -108.00, 17.00, 72.00, -59.00, -82.00, 2.00, -27.00, 21.00, -5.00, -12.00, 16.00, 1.00, 18.00,
        11.00, -23.00, -2.00, -10.00, 18.00, 6.00, 7.00, 0.00, -18.00, -11.00, 4.00, -7.00, -22.00, 4.00, 9.00,
        3.00, 16.00, 6.00, -13.00, -1.00, -15.00, 5.00, 10.00, -21.00, 1.00, 16.00, -12.00, 9.00, 9.00, -5.00,
        -3.00, -6.00, -1.00, 9.00, 7.00, 10.00, 2.00, -6.00, -5.00, 2.00, -4.00, -4.00, 1.00, 2.00, 0.00,
        -5.00, 3.00, -2.00, 6.00, 5.00, -4.00, 3.00, 0.00, 1.00, -1.00, 2.00, 4.00, 3.00, 0.00, 0.00,
        -6.00, -29873.00, -1905.00, 5500.00, -2072.00, 3044.00, -2197.00, 1687.00, -306.00, 1296.00, -2208.00, -310.00, 1247.00, 284.00, 829.00,
        -297.00, 936.00, 780.00, 232.00, 361.00, -249.00, -424.00, 69.00, 170.00, -297.00, -214.00, 355.00, 47.00, 253.00, 150.00,
        -93.00, -154.00, -164.00, -75.00, -46.00, 95.00, 53.00, 65.00, -16.00, 51.00, 88.00, -185.00, 69.00, 4.00, -48.00,
        16.00, -1.00, -102.00, 21.00, 74.00, -62.00, -83.00, 3.00, -27.00, 24.00, -2.00, -6.00, 20.00, 4.00, 17.00,
        10.00, -23.00, 0.00, -7.00, 21.00, 6.00, 8.00, 0.00, -19.00, -11.00, 5.00, -9.00, -23.00, 4.00, 11.00,
        4.00, 14.00, 4.00, -15.00, -4.00, -11.00, 5.00, 10.00, -21.00, 1.00, 15.00, -12.00, 9.00, 9.00, -6.00,
        -3.00, -6.00, -1.00, 9.00, 7.00, 9.00, 1.00, -7.00, -5.00, 2.00, -4.00, -4.00, 1.00, 3.00, 0.00,
        -5.00, 3.00, -2.00, 6.00, 5.00, -4.00, 3.00, 0.00, 1.00, -1.00, 2.00, 4.00, 3.00, 0.00, 0.00,
        -6.00, -29775.00, -1848.00, 5406.00, -2131.00, 3059.00, -2279.00, 1686.00, -373.00, 1314.00, -2239.00, -284.00, 1248.00, 293.00, 802.00,
        -352.00, 939.00, 780.00, 247.00, 325.00, -240.00, -423.00, 84.00, 141.00, -299.00, -214.00, 353.00, 46.00, 245.00, 154.00,
        -109.00, -153.00, -165.00, -69.00, -36.00, 97.00, 61.00, 65.00, -16.00, 59.00, 82.00, -178.00, 69.00, 3.00, -52.00,
        18.00, 1.00, -96.00, 24.00, 77.00, -64.00, -80.00, 2.00, -26.00, 26.00, 0.00, -1.00, 21.00, 5.00, 17.00,
        9.00, -23.00, 0.00, -4.00, 23.00, 5.00, 10.00, -1.00, -19.00, -10.00, 6.00, -12.00, -22.00, 3.00, 12.00,
        4.00, 12.00, 2.00, -16.00, -6.00, -10.00, 4.00, 9.00, -20.00, 1.00, 15.00, -12.00, 11.00, 9.00, -7.00,
        -4.00, -7.00, -2.00, 9.00, 7.00, 8.00, 1.00, -7.00, -6.00, 2.00, -3.00, -4.00, 2.00, 2.00, 1.00,
        -5.00, 3.00, -2.00, 6.00, 4.00, -4.00, 3.00, 0.00, 1.00, -2.00, 3.00, 3.00, 3.00, -1.00, 0.00,
        -6.00, -29692.00, -1784.00, 5306.00, -2200.00, 3070.00, -2366.00, 1681.00, -413.00, 1335.00, -2267.00, -262.00, 1249.00, 302.00, 759.00,
        -427.00, 940.00, 780.00, 262.00, 290.00, -236.00, -418.00, 97.00, 122.00, -306.00, -214.00, 352.00, 46.00, 235.00, 165.00,
        -118.00, -143.00, -166.00, -55.00, -17.00, 107.00, 68.00, 67.00, -17.00, 68.00, 72.00, -170.00, 67.00, -1.00, -58.00,
        19.00, 1.00, -93.00, 36.00, 77.00, -72.00, -69.00, 1.00, -25.00, 28.00, 4.00, 5.00, 24.00, 4.00, 17.00,
        8.00, -24.00, -2.00, -6.00, 25.00, 6.00, 11.00, -6.00, -21.00, -9.00, 8.00, -14.00, -23.00, 9.00, 15.00,
        6.00, 11.00, -5.00, -16.00, -7.00, -4.00, 4.00, 9.00, -20.00, 3.00, 15.00, -10.00, 12.00, 8.00, -6.00,
        -8.00, -8.00, -1.00, 8.00, 10.00, 5.00, -2.00, -8.00, -8.00, 3.00, -3.00, -6.00, 1.00, 2.00, 0.00,
        -4.00, 4.00, -1.00, 5.00, 4.00, -5.00, 2.00, -1.00, 2.00, -2.00, 5.00, 1.00, 1.00, -2.00, 0.00,
        -7.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00,
        0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00,
        0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00,
        0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00,
        0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00,
        0.00, -29619.40, -1728.20, 5186.10, -2267.70, 3068.40, -2481.60, 1670.90, -458.00, 1339.60, -2288.00, -227.60, 1252.10, 293.40, 714.50,
        -491.10, 932.30, 786.80, 272.60, 250.00, -231.90, -403.00, 119.80, 111.30, -303.80, -218.80, 351.40, 43.80, 222.30, 171.90,
        -130.40, -133.10, -168.60, -39.30, -12.90, 106.30, 72.30, 68.20, -17.40, 74.20, 63.70, -160.90, 65.10, -5.90, -61.20,
        16.90, 0.70, -90.40, 43.80, 79.00, -74.00, -64.60, 0.00, -24.20, 33.30, 6.20, 9.10, 24.00, 6.90, 14.80,
        7.30, -25.40, -1.20, -5.80, 24.40, 6.60, 11.90, -9.20, -21.50, -7.90, 8.50, -16.60, -21.50, 9.10, 15.50,
        7.00, 8.90, -7.90, -14.90, -7.00, -2.10, 5.00, 9.40, -19.70, 3.00, 13.40, -8.40, 12.50, 6.30, -6.20,
        -8.90, -8.40, -1.50, 8.40, 9.30, 3.80, -4.30, -8.20, -8.20, 4.80, -2.60, -6.00, 1.70, 1.70, 0.00,
        -3.10, 4.00, -0.50, 4.90, 3.70, -5.90, 1.00, -1.20, 2.00, -2.90, 4.20, 0.20, 0.30, -2.20, -1.10,
        -7.40, 2.70, -1.70, 0.10, -1.90, 1.30, 1.50, -0.90, -0.10, -2.60, 0.10, 0.90, -0.70, -0.70, 0.70,
        -2.80, 1.70, -0.90, 0.10, -1.20, 1.20, -1.90, 4.00, -0.90, -2.20, -0.30, -0.40, 0.20, 0.30, 0.90,
        2.50, -0.20, -2.60, 0.90, 0.70, -0.50, 0.30, 0.30, 0.00, -0.30, 0.00, -0.40, 0.30, -0.10, -0.90,
        -0.20, -0.40, -0.40, 0.80, -0.20, -0.90, -0.90, 0.30, 0.20, 0.10, 1.80, -0.40, -0.40, 1.30, -1.00,
        -0.40, -0.10, 0.70, 0.70, -0.40, 0.30, 0.30, 0.60, -0.10, 0.30, 0.40, -0.20, 0.00, -0.50, 0.10,
        -0.90, -29554.63, -1669.05, 5077.99, -2337.24, 3047.69, -2594.50, 1657.76, -515.43, 1336.30, -2305.83, -198.86, 1246.39, 269.72, 672.51,
        -524.72, 920.55, 797.96, 282.07, 210.65, -225.23, -379.86, 145.15, 100.00, -305.36, -227.00, 354.41, 42.72, 208.95, 180.25,
        -136.54, -123.45, -168.05, -19.57, -13.55, 103.85, 73.60, 69.56, -20.33, 76.74, 54.75, -151.34, 63.63, -14.58, -63.53,
        14.58, 0.24, -86.36, 50.94, 79.88, -74.46, -61.14, -1.65, -22.57, 38.73, 6.82, 12.30, 25.35, 9.37, 10.93,
        5.42, -26.32, 1.94, -4.64, 24.80, 7.62, 11.20, -11.73, -20.88, -6.88, 9.83, -18.11, -19.71, 10.17, 16.22,
        9.36, 7.61, -11.25, -12.76, -4.87, -0.06, 5.58, 9.76, -20.11, 3.58, 12.69, -6.94, 12.67, 5.01, -6.72,
        -10.76, -8.16, -1.25, 8.10, 8.76, 2.92, -6.66, -7.73, -9.22, 6.01, -2.17, -6.12, 2.19, 1.42, 0.10,
        -2.35, 4.46, -0.15, 4.76, 3.06, -6.58, 0.29, -1.01, 2.06, -3.47, 3.77, -0.86, -0.21, -2.31, -2.09,
        -7.93, 2.95, -1.60, 0.26, -1.88, 1.44, 1.44, -0.77, -0.31, -2.27, 0.29, 0.90, -0.79, -0.58, 0.53,
        -2.69, 1.80, -1.08, 0.16, -1.58, 0.96, -1.90, 3.99, -1.39, -2.15, -0.29, -0.55, 0.21, 0.23, 0.89,
        2.38, -0.38, -2.63, 0.96, 0.61, -0.30, 0.40, 0.46, 0.01, -0.35, 0.02, -0.36, 0.28, 0.08, -0.87,
        -0.49, -0.34, -0.08, 0.88, -0.16, -0.88, -0.76, 0.30, 0.33, 0.28, 1.72, -0.43, -0.54, 1.18, -1.07,
        -0.37, -0.04, 0.75, 0.63, -0.26, 0.21, 0.35, 0.53, -0.05, 0.38, 0.41, -0.22, -0.10, -0.57, -0.18,
        -0.82, -29496.57, -1586.42, 4944.26, -2396.06, 3026.34, -2708.54, 1668.17, -575.73, 1339.85, -2326.54, -160.40, 1232.10, 251.75, 633.73,
        -537.03, 912.66, 808.97, 286.48, 166.58, -211.03, -356.83, 164.46, 89.40, -309.72, -230.87, 357.29, 44.58, 200.26, 189.01,
        -141.05, -118.06, -163.17, -0.01, -8.03, 101.04, 72.78, 68.69, -20.90, 75.92, 44.18, -141.40, 61.54, -22.83, -66.26,
        13.10, 3.02, -78.09, 55.40, 80.44, -75.00, -57.80, -4.55, -21.20, 45.24, 6.54, 14.00, 24.96, 10.46, 7.03,
        1.64, -27.61, 4.92, -3.28, 24.41, 8.21, 10.84, -14.50, -20.03, -5.59, 11.83, -19.34, -17.41, 11.61, 16.71,
        10.85, 6.96, -14.05, -10.74, -3.54, 1.64, 5.50, 9.45, -20.54, 3.45, 11.51, -5.27, 12.75, 3.13, -7.14,
        -12.38, -7.42, -0.76, 7.97, 8.43, 2.14, -8.42, -6.08, -10.08, 7.01, -1.94, -6.24, 2.73, 0.89, -0.10,
        -1.07, 4.71, -0.16, 4.44, 2.45, -7.22, -0.33, -0.96, 2.13, -3.95, 3.09, -1.99, -1.03, -1.97, -2.80,
        -8.31, 3.05, -1.48, 0.13, -2.03, 1.67, 1.65, -0.66, -0.51, -1.76, 0.54, 0.85, -0.79, -0.39, 0.37,
        -2.51, 1.79, -1.27, 0.12, -2.11, 0.75, -1.94, 3.75, -1.86, -2.12, -0.21, -0.87, 0.30, 0.27, 1.04,
        2.13, -0.63, -2.49, 0.95, 0.49, -0.11, 0.59, 0.52, 0.00, -0.39, 0.13, -0.37, 0.27, 0.21, -0.86,
        -0.77, -0.23, 0.04, 0.87, -0.09, -0.89, -0.87, 0.31, 0.30, 0.42, 1.66, -0.45, -0.59, 1.08, -1.14,
        -0.31, -0.07, 0.78, 0.54, -0.18, 0.10, 0.38, 0.49, 0.02, 0.44, 0.42, -0.25, -0.26, -0.53, -0.26,
        -0.79, -29441.46, -1501.77, 4795.99, -2445.88, 3012.20, -2845.41, 1676.35, -642.17, 1350.33, -2352.26, -115.29, 1225.85, 245.04, 581.69,
        -538.70, 907.42, 813.68, 283.54, 120.49, -188.43, -334.85, 180.95, 70.38, -329.23, -232.91, 360.14, 46.98, 192.35, 196.98,
        -140.94, -119.14, -157.40, 15.98, 4.30, 100.12, 69.55, 67.57, -20.61, 72.79, 33.30, -129.85, 58.74, -28.93, -66.64,
        13.14, 7.35, -70.85, 62.41, 81.29, -75.99, -54.27, -6.79, -19.53, 51.82, 5.59, 15.07, 24.45, 9.32, 3.27,
        -2.88, -27.50, 6.61, -2.32, 23.98, 8.89, 10.04, -16.78, -18.26, -3.16, 13.18, -20.56, -14.60, 13.33, 16.16,
        11.76, 5.69, -15.98, -9.10, -2.02, 2.26, 5.33, 8.83, -21.77, 3.02, 10.76, -3.22, 11.74, 0.67, -6.74,
        -13.20, -6.88, -0.10, 7.79, 8.68, 1.04, -9.06, -3.89, -10.54, 8.44, -2.01, -6.26, 3.28, 0.17, -0.40,
        0.55, 4.55, -0.55, 4.40, 1.70, -7.92, -0.67, -0.61, 2.13, -4.16, 2.33, -2.85, -1.80, -1.12, -3.59,
        -8.72, 3.00, -1.40, 0.00, -2.30, 2.11, 2.08, -0.60, -0.79, -1.05, 0.58, 0.76, -0.70, -0.20, 0.14,
        -2.12, 1.70, -1.44, -0.22, -2.57, 0.44, -2.01, 3.49, -2.34, -2.09, -0.16, -1.08, 0.46, 0.37, 1.23,
        1.75, -0.89, -2.19, 0.85, 0.27, 0.10, 0.72, 0.54, -0.09, -0.37, 0.29, -0.43, 0.23, 0.22, -0.89,
        -0.94, -0.16, -0.03, 0.72, -0.02, -0.92, -0.88, 0.42, 0.49, 0.63, 1.56, -0.42, -0.50, 0.96, -1.24,
        -0.19, -0.10, 0.81, 0.42, -0.13, -0.04, 0.38, 0.48, 0.08, 0.48, 0.46, -0.30, -0.35, -0.43, -0.36,
        -0.71, -29404.80, -1450.90, 4652.50, -2499.60, 2982.00, -2991.60, 1677.00, -734.60, 1363.20, -2381.20, -82.10, 1236.20, 241.90, 525.70,
        -543.40, 903.00, 809.50, 281.90, 86.30, -158.40, -309.40, 199.70, 48.00, -349.70, -234.30, 363.20, 47.70, 187.80, 208.30,
        -140.70, -121.20, -151.20, 32.30, 13.50, 98.90, 66.00, 65.50, -19.10, 72.90, 25.10, -121.50, 52.80, -36.20, -64.50,
        13.50, 8.90, -64.70, 68.10, 80.60, -76.70, -51.50, -8.20, -16.90, 56.50, 2.20, 15.80, 23.50, 6.40, -2.20,
        -7.20, -27.20, 9.80, -1.80, 23.70, 9.70, 8.40, -17.60, -15.30, -0.50, 12.80, -21.10, -11.70, 15.30, 14.90,
        13.70, 3.60, -16.50, -6.90, -0.30, 2.80, 5.00, 8.40, -23.40, 2.90, 11.00, -1.50, 9.80, -1.10, -5.10,
        -13.20, -6.30, 1.10, 7.80, 8.80, 0.40, -9.30, -1.40, -11.90, 9.60, -1.90, -6.20, 3.40, -0.10, -0.20,
        1.70, 3.60, -0.90, 4.80, 0.70, -8.60, -0.90, -0.10, 1.90, -4.30, 1.40, -3.40, -2.40, -0.10, -3.80,
        -8.80, 3.00, -1.40, 0.00, -2.50, 2.50, 2.30, -0.60, -0.90, -0.40, 0.30, 0.60, -0.70, -0.20, -0.10,
        -1.70, 1.40, -1.60, -0.60, -3.00, 0.20, -2.00, 3.10, -2.60, -2.00, -0.10, -1.20, 0.50, 0.50, 1.30,
        1.40, -1.20, -1.80, 0.70, 0.10, 0.30, 0.80, 0.50, -0.20, -0.30, 0.60, -0.50, 0.20, 0.10, -0.90,
        -1.10, 0.00, -0.30, 0.50, 0.10, -0.90, -0.90, 0.50, 0.60, 0.70, 1.40, -0.30, -0.40, 0.80, -1.30,
        0.00, -0.10, 0.80, 0.30, 0.00, -0.10, 0.40, 0.50, 0.10, 0.50, 0.50, -0.40, -0.50, -0.40, -0.40,
        -0.60, 5.70, 7.40, -25.90, -11.00, -7.00, -30.20, -2.10, -22.40, 2.20, -5.90, 6.00, 3.10, -1.10, -12.00,
        0.50, -1.20, -1.60, -0.10, -5.90, 6.50, 5.20, 3.60, -5.10, -5.00, -0.30, 0.50, 0.00, -0.60, 2.50,
        0.20, -0.60, 1.30, 3.00, 0.90, 0.30, -0.50, -0.30, 0.00, 0.40, -1.60, 1.30, -1.30, -1.40, 0.80,
        0.00, 0.00, 0.90, 1.00, -0.10, -0.20, 0.60, 0.00, 0.60, 0.70, -0.80, 0.10, -0.20, -0.50, -1.10,
        -0.80, 0.10, 0.80, 0.30, 0.00, 0.10, -0.20, -0.10, 0.60, 0.40, -0.20, -0.10, 0.50, 0.40, -0.30,
        0.30, -0.40, -0.10, 0.50, 0.40, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00,
        0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00,
        0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00,
        0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00,
        0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00,
        0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00,
        0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00,
        0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00,
        0.00};
    }

    /// <summary>
    /// Given a point and a rectangular envelope, returns boolean if point is inside rectangle edge.<br></br><br></br>
    /// Returns booleans<br></br>
    /// </summary>
    public static bool PointInExtent(double[] dblPoint, double[] dblEnvelope) => dblPoint[0] >= dblEnvelope[0] && dblPoint[1] >= dblEnvelope[1] && dblPoint[0] <= dblEnvelope[2] && dblPoint[1] <= dblEnvelope[3];
    /// <summary>
    /// Given a point and a rectangular envelope, returns boolean if point is inside rectangle edge.<br></br><br></br>
    /// Returns booleans<br></br>
    /// </summary>
    public static bool PointInExtent(double[] dblPoint, double dblRectMinX, double dblRectMaxX, double dblRectMinY, double dblRectMaxY) => dblPoint[0] >= dblRectMinX && dblPoint[1] >= dblRectMinY && dblPoint[0] <= dblRectMaxX && dblPoint[1] <= dblRectMaxY;
    /// <summary>
    /// Given a point and a rectangular envelope, returns boolean if point is inside rectangle edge.<br></br><br></br>
    /// Returns booleans<br></br>
    /// </summary>
    public static bool PointInExtent(double dblPointX, double dblPointY, double dblRectMinX, double dblRectMaxX, double dblRectMinY, double dblRectMaxY) => dblPointX >= dblRectMinX && dblPointY >= dblRectMinY && dblPointX <= dblRectMaxX && dblPointY <= dblRectMaxY;

    /// <summary>
    /// Given a segment, or coordinates of the start- and end-points of a segment, and a rectangular envelope, returns boolean if segment intersects or touches rectangle edge.<br></br>
    /// Additionally returns boolean of whether segment is entirely contained in envelope.<br></br><br></br>
    /// Returns booleans<br></br>
    /// </summary>
    public static bool SegmentIntersectsRectangle(double[][,] dblLine1, double[] dblEnvelope, out bool booFullyContained)
    {
      return SegmentIntersectsRectangle(dblLine1[0][0, 0], dblLine1[0][0, 1], dblLine1[0][1, 0], dblLine1[0][1, 1], dblEnvelope[0], dblEnvelope[2], dblEnvelope[1], dblEnvelope[3], out booFullyContained);
    }
    /// <summary>
    /// Given a segment, or coordinates of the start- and end-points of a segment, and a rectangular envelope, returns boolean if segment intersects or touches rectangle edge.<br></br>
    /// Additionally returns boolean of whether segment is entirely contained in envelope.<br></br><br></br>
    /// Returns booleans<br></br>
    /// </summary>
    public static bool SegmentIntersectsRectangle(double[][,] dblLine1, double dblRectMinX, double dblRectMaxX, double dblRectMinY, double dblRectMaxY, out bool booFullyContained)
    {
      return SegmentIntersectsRectangle(dblLine1[0][0, 0], dblLine1[0][0, 1], dblLine1[0][1, 0], dblLine1[0][1, 1], dblRectMinX, dblRectMaxX, dblRectMinY, dblRectMaxY, out booFullyContained);
    }
    /// <summary>
    /// Given a segment, or coordinates of the start- and end-points of a segment, and a rectangular envelope, returns boolean if segment intersects or touches rectangle edge.<br></br>
    /// Additionally returns boolean of whether segment is entirely contained in envelope.<br></br><br></br>
    /// Returns booleans<br></br>
    /// </summary>
    public static bool SegmentIntersectsRectangle(double[] dblLine1Start, double[] dblLine1End, double dblRectMinX, double dblRectMaxX, double dblRectMinY, double dblRectMaxY, out bool booFullyContained)
    {
      return SegmentIntersectsRectangle(dblLine1Start[0], dblLine1Start[1], dblLine1End[0], dblLine1End[1], dblRectMinX, dblRectMaxX, dblRectMinY, dblRectMaxY, out booFullyContained);
    }
    /// <summary>
    /// Given a segment, or coordinates of the start- and end-points of a segment, and a rectangular envelope, returns boolean if segment intersects or touches rectangle edge.<br></br>
    /// Additionally returns boolean of whether segment is entirely contained in envelope.<br></br><br></br>
    /// Returns booleans<br></br>
    /// </summary>
    public static bool SegmentIntersectsRectangle(double dblSegStartX, double dblSegStartY, double dblSegEndX, double dblSegEndY, double dblRectMinX, double dblRectMaxX, double dblRectMinY, double dblRectMaxY, out bool booFullyContained)
    {
      double dblSegMinX = Math.Min(dblSegStartX, dblSegEndX);
      double dblSegMaxX = Math.Max(dblSegStartX, dblSegEndX);
      double dblSegMinY = Math.Min(dblSegStartY, dblSegEndY);
      double dblSegMaxY = Math.Max(dblSegStartY, dblSegEndY);

      // FIX ENVELOPE MIN AND MAX IN CASE THEY WERE SENT INCORRECTLY
      double dblTemp;
      if (dblRectMinX > dblRectMaxX)
      {
        dblTemp = dblRectMinX;
        dblRectMinX = dblRectMaxX;
        dblRectMaxX = dblTemp;
      }
      if (dblRectMinY > dblRectMaxY)
      {
        dblTemp = dblRectMinY;
        dblRectMinY = dblRectMaxY;
        dblRectMaxY = dblTemp;
      }

      // CAN EXCLUDE INITIALLY IF EXTENTS DON'T OVERLAP
      if (dblSegMinX <= dblRectMaxX && dblRectMinX <= dblSegMaxX && dblSegMinY <= dblRectMaxY && dblRectMinY <= dblSegMaxY)
      {
        booFullyContained = dblSegMinX >= dblRectMinX && dblSegMaxX <= dblRectMaxX && dblSegMinY >= dblRectMinY && dblSegMaxY <= dblRectMaxY;
        // First check intersects left side
        if (CalcSegIntersectsEdge(dblSegStartX, dblSegStartY, dblSegEndX, dblSegEndY, dblRectMinX, dblRectMinY, dblRectMinX, dblRectMaxY, out _, out _, out _, out _, out _))
        { return true; }
        // Next check Top
        else if (CalcSegIntersectsEdge(dblSegStartX, dblSegStartY, dblSegEndX, dblSegEndY, dblRectMinX, dblRectMaxY, dblRectMaxX, dblRectMaxY, out _, out _, out _, out _, out _))
        { return true; }
        // Next check Right
        else if (CalcSegIntersectsEdge(dblSegStartX, dblSegStartY, dblSegEndX, dblSegEndY, dblRectMaxX, dblRectMaxY, dblRectMaxX, dblRectMinY, out _, out _, out _, out _, out _))
        { return true; }
        // Next check Bottom
        else if (CalcSegIntersectsEdge(dblSegStartX, dblSegStartY, dblSegEndX, dblSegEndY, dblRectMinX, dblRectMinY, dblRectMaxX, dblRectMinY, out _, out _, out _, out _, out _))
        { return true; }
        // Next check fully contained
        else if (booFullyContained)
        {
          return true;
        }
        else
        { return false; }
      }
      else
      {
        booFullyContained = false;
        return false;
      }

      //double dblSegStartX = 15;
      //double dblSegStartY = 15;
      //double dblSegEndX = 5;
      //double dblSegEndY = 5;
      //double dblRectMinX = 15;
      //double dblRectMaxX = 5;
      //double dblRectMinY = 2;
      //double dblRectMaxY = 6;
      //bool booFullyContained;
      //bool booIntersects;
      //booIntersects = SegmentIntersectsRectangle(dblSegStartX, dblSegStartY, dblSegEndX, dblSegEndY, dblRectMinX, dblRectMaxX, dblRectMinY, dblRectMaxY, out booFullyContained);
      //Console.WriteLine("Intersects = " + booIntersects.ToString() + ", Fully Contained = " + booFullyContained.ToString());
      //booIntersects = SegmentIntersectsRectangle(new double[2] { dblSegStartX, dblSegStartY }, new double[2] { dblSegEndX, dblSegEndY }, dblRectMinX, dblRectMaxX, dblRectMinY, dblRectMaxY, out booFullyContained);
      //Console.WriteLine("Intersects = " + booIntersects.ToString() + ", Fully Contained = " + booFullyContained.ToString());
      //booIntersects = SegmentIntersectsRectangle(new double[][,] { new double[2, 2] { { dblSegStartX, dblSegStartY }, { dblSegEndX, dblSegEndY } } }, dblRectMinX, dblRectMaxX, dblRectMinY, dblRectMaxY, out booFullyContained);
      //Console.WriteLine("Intersects = " + booIntersects.ToString() + ", Fully Contained = " + booFullyContained.ToString());
      //booIntersects = SegmentIntersectsRectangle(new double[][,] { new double[2, 2] { { dblSegStartX, dblSegStartY }, { dblSegEndX, dblSegEndY } } }, new double[4] { dblRectMinX, dblRectMaxX, dblRectMinY, dblRectMaxY }, out booFullyContained);
      //Console.WriteLine("Intersects = " + booIntersects.ToString() + ", Fully Contained = " + booFullyContained.ToString());
    }

    /// <summary>
    /// Given endpoints for two segments, extends both into infinite lines and determines whether they cross each other.  <br>
    /// </br>returns coordinates of intersection point, or double.NaN if lines parallel.<br></br><br></br>
    /// Fills Double values for intersection coordinates
    /// </summary>
    public static void InfiniteLineIntersect(double[][,] dblLine1, double[][,] dblLine2, out JenSegmentIntersectTypes JenIntersectType, out double dblIntersectX, out double dblIntersectY)
    {
      InfiniteLineIntersect(dblLine1[0][0, 0], dblLine1[0][0, 1], dblLine1[0][1, 0], dblLine1[0][1, 1], dblLine2[0][0, 0], dblLine2[0][0, 1], dblLine2[0][1, 0], dblLine2[0][1, 1], out JenIntersectType, out dblIntersectX, out dblIntersectY);
    }
    /// <summary>
    /// Given endpoints for two segments, extends both into infinite lines and determines whether they cross each other.  <br>
    /// </br>returns coordinates of intersection point, or double.NaN if lines parallel.<br></br><br></br>
    /// Fills Double values for intersection coordinates
    /// </summary>
    public static void InfiniteLineIntersect(double[] dblLine1Start, double[] dblLine1End, double[] dblLine2Start, double[] dblLine2End, out JenSegmentIntersectTypes JenIntersectType, out double dblIntersectX, out double dblIntersectY)
    {
      InfiniteLineIntersect(dblLine1Start[0], dblLine1Start[1], dblLine1End[0], dblLine1End[1], dblLine2Start[0], dblLine2Start[1], dblLine2End[0], dblLine2End[1], out JenIntersectType, out dblIntersectX, out dblIntersectY);
    }
    /// <summary>
    /// Given endpoints for two segments, extends both into infinite lines and determines whether they cross each other.  <br>
    /// </br>returns coordinates of intersection point, or double.NaN if lines parallel.<br></br><br></br>
    /// Fills Double values for intersection coordinates
    /// </summary>
    public static void InfiniteLineIntersect(double dblLine1X1, double dblLine1Y1, double dblLine1X2, double dblLine1Y2, double dblLine2X1, double dblLine2Y1, double dblLine2X2, double dblLine2Y2, out JenSegmentIntersectTypes JenIntersectType, out double dblIntersectX, out double dblIntersectY)
    {
      // adapted from http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
      // http://stackoverflow.com/questions/3269434/whats-the-most-efficient-way-to-test-two-integer-ranges-for-overlap
      // and p. 304 of Graphic Gems
      // RETURN lngIntersectionType = 0 FOR NO INTERSECT, 1 for INTERSECT,  2 for COLLINEAR
      // BASED ON START POINTS AND CHANGE TO END POINTS, NOT END POINTS THEMSELVES.
      // THEREFORE dblLine1X2 NEEDS TO BE TREATED AS (dblLine1X2 - dblStartX)
      // If lines collinear, defines intersection points equal to Line 1 Starting Point coordinates

      // GENERAL INTERSECT POINTS ARE THOSE IF THE LINES EXTENDED TO INFINITY
      double dblLine1ExtentX = dblLine1X2 - dblLine1X1;
      double dblLine1ExtentY = dblLine1Y2 - dblLine1Y1;
      double dblLine2ExtentX = dblLine2X2 - dblLine2X1;
      double dblLine2ExtentY = dblLine2Y2 - dblLine2Y1;
      double dblQmPX = dblLine2X1 - dblLine1X1;
      double dblQmPY = dblLine2Y1 - dblLine1Y1;
      double dblLine1ExtentCrossLine2Extent = (dblLine1ExtentX * dblLine2ExtentY) - (dblLine1ExtentY * dblLine2ExtentX);
      double dblUNum = (dblQmPX * dblLine1ExtentY) - (dblQmPY * dblLine1ExtentX);

      if (dblLine1ExtentCrossLine2Extent == 0)     // if lines are parallel
      {
        if (dblUNum == 0) { JenIntersectType = JenSegmentIntersectTypes.ENUM_CollinearSegment; dblIntersectX = dblLine1X1; dblIntersectY = dblLine1Y1; }    // IF LINES ARE COLLINEAR        
        else { JenIntersectType = JenSegmentIntersectTypes.ENUM_NoIntersect; dblIntersectX = double.NaN; dblIntersectY = double.NaN; }
      }
      else
      {
        JenIntersectType = JenSegmentIntersectTypes.ENUM_Crosses;
        double dblLine1ExtentCrossLine2ExtentRecip = 1 / dblLine1ExtentCrossLine2Extent;
        double dblTNum = (dblQmPX * dblLine2ExtentY) - (dblQmPY * dblLine2ExtentX);
        //double dblU = dblUNum * dblLine1ExtentCrossLine2ExtentRecip;
        double dblT = dblTNum * dblLine1ExtentCrossLine2ExtentRecip;
        dblIntersectX = dblLine1X1 + (dblT * dblLine1ExtentX);
        dblIntersectY = dblLine1Y1 + (dblT * dblLine1ExtentY);
      }
      //double dblSegStartX = 5;
      //double dblSegStartY = 5;
      //double dblSegEndX = 15;
      //double dblSegEndY = 15;
      //double dblEdgeStartX = 5;
      //double dblEdgeStartY = 15;
      //double dblEdgeEndX = 6;
      //double dblEdgeEndY = 14;
      ////InfiniteLineIntersect(dblSegStartX, dblSegStartY, dblSegEndX, dblSegEndY, dblEdgeStartX, dblEdgeStartY, dblEdgeEndX, dblEdgeEndY, out JenSegmentIntersectTypes JenIntersect, out double dblIntersectX, out double dblIntersectY);
      ////InfiniteLineIntersect(new double[2] { dblSegStartX, dblSegStartY }, new double[2] { dblSegEndX, dblSegEndY }, new double[2] { dblEdgeStartX, dblEdgeStartY }, new double[2] { dblEdgeEndX, dblEdgeEndY }, out JenSegmentIntersectTypes JenIntersect, out double dblIntersectX, out double dblIntersectY);
      //InfiniteLineIntersect(new double[][,] { new double[2, 2] { { dblSegStartX, dblSegStartY }, { dblSegEndX, dblSegEndY } } }, new double[][,] { new double[2, 2] { { dblEdgeStartX, dblEdgeStartY }, { dblEdgeEndX, dblEdgeEndY } } }, out JenSegmentIntersectTypes JenIntersect, out double dblIntersectX, out double dblIntersectY);
      //Console.WriteLine("lngIntersectionType = " + JenIntersect.ToString());
      //Console.WriteLine("Intersection 1: [" + dblIntersectX.ToString("0.00") + ", " + dblIntersectY.ToString("0.00") + "]");
    }

    /// <summary>
    /// Given two segments, determines whether they cross each other.  <br>
    /// </br>Optionally determines if the crossing is at an edge or along a collinear segment, and the intersection coordinates.<br>
    /// </br>If collinear, optionally returns coordinates of second point of collinear segment.<br></br><br>  </br>
    /// Returns Boolean
    /// </summary>
    /// <returns></returns>
    public static bool CalcSegIntersectsEdge(double[][,] dblSeg, double[][,] dblEdge, out JenSegmentIntersectTypes JenIntersectType, out double dblIntersectX, out double dblIntersectY, out double dblIntersectX2, out double dblIntersectY2, bool booSeekIntersection = false)
    {
      return CalcSegIntersectsEdge(dblSeg[0][0, 0], dblSeg[0][0, 1], dblSeg[0][1, 0], dblSeg[0][1, 1], dblEdge[0][0, 0], dblEdge[0][0, 1], dblEdge[0][1, 0], dblEdge[0][1, 1], out JenIntersectType, out dblIntersectX, out dblIntersectY, out dblIntersectX2, out dblIntersectY2, booSeekIntersection);
    }
    /// <summary>
    /// Given endpoints for two segments, determines whether they cross each other.  <br>
    /// </br>Optionally determines if the crossing is at an edge or along a collinear segment, and the intersection coordinates.<br>
    /// </br>If collinear, optionally returns coordinates of second point of collinear segment.<br></br><br>  </br>
    /// Returns Boolean
    /// </summary>
    /// <param name="dblSegStartX"></param>
    /// <param name="dblSegStartY"></param>
    /// <param name="dblSegEndX"></param>
    /// <param name="dblSegEndY"></param>
    /// <param name="dblEdgeStartX"></param>
    /// <param name="dblEdgeStartY"></param>
    /// <param name="dblEdgeEndX"></param>
    /// <param name="dblEdgeEndY"></param>
    /// <param name="JenIntersectType"></param>
    /// <param name="dblIntersectX"></param>
    /// <param name="dblIntersectY"></param>
    /// <param name="dblIntersectX2"></param>
    /// <param name="dblIntersectY2"></param>
    /// <param name="booSeekIntersection"></param>
    /// <returns></returns>
    public static bool CalcSegIntersectsEdge(double[] dblSegStart, double[] dblSegEnd, double[] dblEdgeStart, double[] dblEdgeEnd, out JenSegmentIntersectTypes JenIntersectType, out double dblIntersectX, out double dblIntersectY, out double dblIntersectX2, out double dblIntersectY2, bool booSeekIntersection = false)
    {
      return CalcSegIntersectsEdge(dblSegStart[0], dblSegStart[1], dblSegEnd[0], dblSegEnd[1], dblEdgeStart[0], dblEdgeStart[1], dblEdgeEnd[0], dblEdgeEnd[1], out JenIntersectType, out dblIntersectX, out dblIntersectY, out dblIntersectX2, out dblIntersectY2, booSeekIntersection);
    }
    /// <summary>
    /// Given endpoints for two segments, determines whether they cross each other.  <br>
    /// </br>Optionally determines if the crossing is at an edge or along a collinear segment, and the intersection coordinates.<br>
    /// </br>If collinear, optionally returns coordinates of second point of collinear segment.<br></br><br>  </br>
    /// Returns Boolean
    /// </summary>
    public static bool CalcSegIntersectsEdge(double dblSegStartX, double dblSegStartY, double dblSegEndX, double dblSegEndY, double dblEdgeStartX, double dblEdgeStartY, double dblEdgeEndX, double dblEdgeEndY, out JenSegmentIntersectTypes JenIntersectType, out double dblIntersectX, out double dblIntersectY, out double dblIntersectX2, out double dblIntersectY2, bool booSeekIntersection = false)
    {
      // adapted from http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
      // http://stackoverflow.com/questions/3269434/whats-the-most-efficient-way-to-test-two-integer-ranges-for-overlap
      // and p. 304 of Graphic Gems
      // RETURN lngIntersectionType = 0 FOR NO INTERSECT, 1 FOR INTERSECT AT EDGE ENDPOINT,
      //          2 FOR CROSSES, 3 for INTERSECT ALONG COLLINEAR SEGMENT
      // BASED ON START POINTS AND CHANGE TO END POINTS, NOT END POINTS THEMSELVES.
      // THEREFORE dblSegEndX NEEDS TO BE TREATED AS (dblSegEndX - dblStartX)

      JenIntersectType = JenSegmentIntersectTypes.ENUM_NoIntersect;  // Default value if not calculating intersection locations
      dblIntersectX = double.NaN;
      dblIntersectY = double.NaN;
      dblIntersectX2 = double.NaN;
      dblIntersectY2 = double.NaN;
      bool booReturn;
      double dblSegExtentDotSegExtent;
      double dblSegExtentDotSegExtentRecip;
      //double dblEdgeExtentDotSegExtent;
      double dblT0;
      double dblT1;

      // CAN EXCLUDE INITIALLY IF EXTENTS DON'T OVERLAP
      if (RangeOverlaps(dblSegStartX, dblSegEndX, dblEdgeStartX, dblEdgeEndX) && RangeOverlaps(dblSegStartY, dblSegEndY, dblEdgeStartY, dblEdgeEndY))
      {
        double dblSegExtentX = dblSegEndX - dblSegStartX;
        double dblSegExtentY = dblSegEndY - dblSegStartY;
        double dblEdgeExtentX = dblEdgeEndX - dblEdgeStartX;
        double dblEdgeExtentY = dblEdgeEndY - dblEdgeStartY;
        double dblQmPX = dblEdgeStartX - dblSegStartX;
        double dblQmPY = dblEdgeStartY - dblSegStartY;
        double dblSegExtentCrossEdgeExtent = (dblSegExtentX * dblEdgeExtentY) - (dblSegExtentY * dblEdgeExtentX);
        double dblUNum = (dblQmPX * dblSegExtentY) - (dblQmPY * dblSegExtentX);

        if (dblSegExtentCrossEdgeExtent == 0)     // if lines are parallel
        {
          if (dblUNum == 0)                       // IF LINES ARE COLLINEAR
          {
            dblSegExtentDotSegExtent = (dblSegExtentX * dblSegExtentX) + (dblSegExtentY * dblSegExtentY);
            dblSegExtentDotSegExtentRecip = 1 / dblSegExtentDotSegExtent;
            double dblEdgeExtentDotSegExtent = (dblEdgeExtentX * dblSegExtentX) + (dblEdgeExtentY * dblSegExtentY);
            dblT0 = Math.Round(((dblQmPX * dblSegExtentX) + (dblQmPY * dblSegExtentY)) * dblSegExtentDotSegExtentRecip, 14);
            dblT1 = Math.Round(dblT0 + (dblEdgeExtentDotSegExtent * dblSegExtentDotSegExtentRecip), 14);

            if (dblEdgeExtentDotSegExtent < 0)        // THEN LINES COLLINEAR, BUT GOING IN OPPOSITE DIRECTIONS
            {
              double dblTemp = dblT0;
              dblT0 = dblT1;
              dblT1 = dblTemp;
            }

            // T0 AND T1 INDICATE RELATIVE POSITION OF EDGE LINE ALONG SEGMENT.  SEGMENT GOES FROM 0 TO 1 (0% TO 100%), SO EDGE MUST
            // LIE BETWEEN 0 AND 1 TO INTERSECT.  T0 INDICATES START, T1 INDICATES END, OF INTERSECTION REGION RELATIVE TO SEGMENT.
            booReturn = (0 <= dblT1) && (dblT0 <= 1);
            if (booSeekIntersection)
            {
              if (booReturn)
              {
                double dblIntersectT0 = Math.Max(0, dblT0);
                double dblIntersectT1 = Math.Min(1, dblT1);
                if (dblT0 == 1)            // I.E. 100% ALONG SEGMENT
                {
                  JenIntersectType = JenSegmentIntersectTypes.ENUM_IntersectEdgeEndpoint;
                  dblIntersectX = dblSegEndX;
                  dblIntersectY = dblSegEndY;
                }
                else if (dblT1 == 0)        // I.E. 0% ALONG SEGMENT
                {
                  JenIntersectType = JenSegmentIntersectTypes.ENUM_IntersectEdgeEndpoint;
                  dblIntersectX = dblSegStartX;
                  dblIntersectY = dblSegStartY;

                }
                else
                {
                  JenIntersectType = JenSegmentIntersectTypes.ENUM_CollinearSegment;
                  dblIntersectX = dblSegStartX + (dblIntersectT0 * dblSegExtentX);
                  dblIntersectY = dblSegStartY + (dblIntersectT0 * dblSegExtentY);
                  dblIntersectX2 = dblSegStartX + (dblIntersectT1 * dblSegExtentX);
                  dblIntersectY2 = dblSegStartY + (dblIntersectT1 * dblSegExtentY);
                }
              }
            }
          }
          else   // LINES ARE ONLY PARALLEL, NOT COLLINEAR; CAN'T TOUCH
          {
            booReturn = false;
            JenIntersectType = JenSegmentIntersectTypes.ENUM_NoIntersect;
          }
        }
        else     // Lines not parallel and extents overlap
        {
          double dblSegExtentCrossEdgeExtentRecip = 1 / dblSegExtentCrossEdgeExtent;
          double dblTNum = (dblQmPX * dblEdgeExtentY) - (dblQmPY * dblEdgeExtentX);
          double dblU = dblUNum * dblSegExtentCrossEdgeExtentRecip;
          double dblT = dblTNum * dblSegExtentCrossEdgeExtentRecip;

          if (dblT >= 0 && dblT <= 1 && dblU >= 0 && dblU <= 1)
          {
            booReturn = true;
            if (booSeekIntersection)
            {
              if (dblU == 0)          //  THEN 0% ALONG EDGE
              {
                JenIntersectType = JenSegmentIntersectTypes.ENUM_IntersectEdgeEndpoint;
                dblIntersectX = dblEdgeStartX;
                dblIntersectY = dblEdgeStartY;
              }
              else if (dblU == 1)      //  THEN 1000% ALONG EDGE
              {
                JenIntersectType = JenSegmentIntersectTypes.ENUM_IntersectEdgeEndpoint;
                dblIntersectX = dblEdgeEndX;
                dblIntersectY = dblEdgeEndY;
              }
              else if (dblT == 0)      //  THEN 0% ALONG  SEGMENT
              {
                JenIntersectType = JenSegmentIntersectTypes.ENUM_IntersectEdgeEndpoint;
                dblIntersectX = dblSegStartX;
                dblIntersectY = dblSegStartY;
              }
              else if (dblT == 1)     //  THEN 100% ALONG  SEGMENT
              {
                JenIntersectType = JenSegmentIntersectTypes.ENUM_IntersectEdgeEndpoint;
                dblIntersectX = dblSegEndX;
                dblIntersectY = dblSegEndY;
              }
              else
              {
                JenIntersectType = JenSegmentIntersectTypes.ENUM_Crosses;
                dblIntersectX = dblSegStartX + (dblT * dblSegExtentX);
                dblIntersectY = dblSegStartY + (dblT * dblSegExtentY);
              }
            }
          }
          else
          {
            booReturn = false;
            JenIntersectType = JenSegmentIntersectTypes.ENUM_NoIntersect;
          }
        }
      }
      else   // If ranges don't overlap; much easier to calculate
      {
        JenIntersectType = JenSegmentIntersectTypes.ENUM_NoIntersect;
        booReturn = false;
      }

      return booReturn;

      //double dblSegStartX = 5;
      //double dblSegStartY = 5;
      //double dblSegEndX = 15;
      //double dblSegEndY = 15;
      //double dblEdgeStartX = 10;
      //double dblEdgeStartY = 10;
      //double dblEdgeEndX = 25;
      //double dblEdgeEndY = 25;
      //bool booSeekIntersection = true;
      //bool booIntersects = CalcSegIntersectsEdge(dblSegStartX, dblSegStartY, dblSegEndX, dblSegEndY, dblEdgeStartX, dblEdgeStartY, dblEdgeEndX, dblEdgeEndY, out JenSegmentIntersectTypes JenIntersect, out double dblIntersectX, out double dblIntersectY, out double dblIntersectX2, out double dblIntersectY2, booSeekIntersection);
      //Console.WriteLine("Intersects = " + booIntersects.ToString());
      //Console.WriteLine("lngIntersectionType = " + JenIntersect.ToString());
      //Console.WriteLine("Intersection 1: [" + dblIntersectX.ToString("0.00") + ", " + dblIntersectY.ToString("0.00") + "]");
      //Console.WriteLine("Intersection 2: [" + dblIntersectX2.ToString("0.00") + ", " + dblIntersectY2.ToString("0.00") + "]");
      //double dblSegStartX = 5;
      //double dblSegStartY = 5;
      //double dblSegEndX = 15;
      //double dblSegEndY = 15;
      //double dblEdgeStartX = 10;
      //double dblEdgeStartY = 5;
      //double dblEdgeEndX = 5;
      //double dblEdgeEndY = 25;
      //bool booSeekIntersection = true;
      ////bool booIntersects = CalcSegIntersectsEdge(dblSegStartX, dblSegStartY, dblSegEndX, dblSegEndY, dblEdgeStartX, dblEdgeStartY, dblEdgeEndX, dblEdgeEndY, out JenSegmentIntersectTypes JenIntersect, out double dblIntersectX, out double dblIntersectY, out double dblIntersectX2, out double dblIntersectY2, booSeekIntersection);
      ////bool booIntersects = CalcSegIntersectsEdge(new double[2] { dblSegStartX, dblSegStartY }, new double[2] { dblSegEndX, dblSegEndY }, new double[2] { dblEdgeStartX, dblEdgeStartY }, new double[2] { dblEdgeEndX, dblEdgeEndY }, out JenSegmentIntersectTypes JenIntersect, out double dblIntersectX, out double dblIntersectY, out double dblIntersectX2, out double dblIntersectY2, booSeekIntersection);
      //bool booIntersects = CalcSegIntersectsEdge(new double[][,] { new double[2, 2] { { dblSegStartX, dblSegStartY }, { dblSegEndX, dblSegEndY } } }, new double[][,] { new double[2, 2] { { dblEdgeStartX, dblEdgeStartY }, { dblEdgeEndX, dblEdgeEndY } } }, out JenSegmentIntersectTypes JenIntersect, out double dblIntersectX, out double dblIntersectY, out double dblIntersectX2, out double dblIntersectY2, booSeekIntersection);
      //Console.WriteLine("Intersects = " + booIntersects.ToString());
      //Console.WriteLine("lngIntersectionType = " + JenIntersect.ToString());
      //Console.WriteLine("Intersection 1: [" + dblIntersectX.ToString("0.00") + ", " + dblIntersectY.ToString("0.00") + "]");
      //Console.WriteLine("Intersection 2: [" + dblIntersectX2.ToString("0.00") + ", " + dblIntersectY2.ToString("0.00") + "]");
    }

    /// <summary>
    /// Given start and end values of two ranges, returns Boolean stating whether ranges overla
    /// </summary>
    /// <param name="dblExtent1Start"></param>
    /// <param name="dblExtent1End"></param>
    /// <param name="dblExtent2Start"></param>
    /// <param name="dblExtent2End"></param>
    /// <returns></returns>
    public static bool RangeOverlaps(double dblExtent1Start, double dblExtent1End, double dblExtent2Start, double dblExtent2End)
    {
      // FIX MIN AND MAX IN CASE THEY WERE SENT INCORRECTLY
      double dblTemp;
      if (dblExtent1Start > dblExtent1End)
      {
        dblTemp = dblExtent1Start;
        dblExtent1Start = dblExtent1End;
        dblExtent1End = dblTemp;
      }
      if (dblExtent2Start > dblExtent2End)
      {
        dblTemp = dblExtent2Start;
        dblExtent2Start = dblExtent2End;
        dblExtent2End = dblTemp;
      }
      return (dblExtent1Start <= dblExtent2End) && (dblExtent2Start <= dblExtent1End);
    }
    public static double Matrix2DCrossProduct(double dblPX, double dblPY, double dblQX, double dblQY) => (dblPX * dblQY) - (dblPY * dblQX);
    public static double Matrix2DDotProduct(double dblPX, double dblPY, double dblQX, double dblQY) => (dblPX * dblQX) + (dblPY * dblQY);

    ///<summary>
    ///Given three consecutive point planar coordinates P:Q:R, returns boolean stating whether point R is counterclockwise from PQ<br/>
    ///Also returns an Int, such that -1 = Counterclockwise, 0 = Coincident, 1 = Clockwise.
    ///and another boolean stating whether the points are in a line or coincident<br/><br/>Returns boolean values
    ///</summary>
    ///
    public static bool CalcCheckClockwiseNumbers(double[] dblP, double[] dblQ, double[] dblR, out bool booLinearOrCoincident, out JenClockwiseConstants JenClockwise, out double dblDistanceToInfiniteLine)
    {
      //double dblX = (dblQX * (dblRY - dblPY)) + (dblQY * (dblPX - dblRX)) - (dblPX * dblRY) + (dblPY * dblRX);
      return CalcCheckClockwiseNumbers(dblP[0], dblP[1], dblQ[0], dblQ[1], dblR[0], dblR[1], out booLinearOrCoincident, out JenClockwise, out dblDistanceToInfiniteLine);
      //dblDistance = (dblQ[0] * (dblR[1] - dblP[1])) + (dblQ[1] * (dblP[0] - dblR[0])) - (dblP[0] * dblR[1]) + (dblP[1] * dblR[0]);
      //booLinearOrCoincident = dblDistance == 0;
      //if (dblDistance < 0) { intNegOne_Zero_One = -1; }
      //else if (dblDistance == 0) { intNegOne_Zero_One = 0; }
      //else { intNegOne_Zero_One = 1; }
      //return dblDistance < 0;
    }
    ///<summary>
    ///Given three consecutive point planar coordinates P:Q:R, returns boolean stating whether point R is counterclockwise from PQ<br/>
    ///Also returns an Int, such that -1 = Counterclockwise, 0 = Coincident, 1 = Clockwise.
    ///and another boolean stating whether the points are in a line or coincident<br/><br/>Returns boolean value
    ///</summary>
    public static bool CalcCheckClockwiseNumbers(double dblPX, double dblPY, double dblQX, double dblQY, double dblRX, double dblRY, out bool booLinearOrCoincident, out JenClockwiseConstants JenClockwise, out double dblDistanceToInfiniteLine)
    {
      // CalcCheckClockwise
      // Jenness Enterprises <www.jennessent.com)>
      // Given 3 consecutive points, this scripts calculates whether the third point lies to the right
      // (clockwise) or to the left (counter-clockwise) of the line connecting the first point to
      // the second point.
      double dblDistance = (dblQX * (dblRY - dblPY)) + (dblQY * (dblPX - dblRX)) - (dblPX * dblRY) + (dblPY * dblRX);
      booLinearOrCoincident = dblDistance == 0;
      if (dblDistance < 0) { JenClockwise = JenClockwiseConstants.ENUM_Clockwise; }
      else if (dblDistance == 0) { JenClockwise = JenClockwiseConstants.ENUM_OnLine; }
      else { JenClockwise = JenClockwiseConstants.ENUM_CounterClockwise; }
      double dblSegmentLength = Math.Sqrt(((dblQX - dblPX) * (dblQX - dblPX)) + ((dblQY - dblPY) * (dblQY - dblPY)));
      dblDistanceToInfiniteLine = (dblSegmentLength == 0) ? 0 : Math.Abs(dblDistance) / dblSegmentLength;
      return dblDistance < 0;
      //bool booLinearOrCoincident;
      //JenClockwiseConstants intNegOne_Zero_One;
      //double dblDistance;

      //Console.WriteLine("Area of Triangle [5.67, 6.78, 12.34] = " + TriangleAreaLegs(5.67, 6.78, 12.34).ToString("0.0000000000"));
      //Console.WriteLine("Area of Triangle Coords [5.67, 123.23, 6.78, 129.33, 12.34, 129.01] = " + TriangleAreaPointsValues(5.67, 123.23, 6.78, 129.33, 12.34, 129.01).ToString("0.0000000000"));
      //Console.WriteLine("Area of Triangle 3D Coords [5.67, 123.23, -12.33, 6.78, 129.33, 0.34, 12.34, 129.01, 5.34] = " + TriangleAreaPoints3DValues(5.67, 123.23, -12.33, 6.78, 129.33, 0.34, 12.34, 129.01, 5.34).ToString("0.0000000000"));
      //Console.WriteLine("Third Point Clockwise [5.67, 123.23, 6.78, 129.33, 12.34, 129.01] = " + CalcCheckClockwiseNumbers(5.67, 123.23, 6.78, 129.33, 12.34, 129.01, out booLinearOrCoincident, out intNegOne_Zero_One, out dblDistance));
      //Console.WriteLine("Linear or Coincident = " + booLinearOrCoincident + ", [-1, 0, 1]? = " + intNegOne_Zero_One.ToString() + ", Distance = " + dblDistance.ToString("0.000"));
      //Console.WriteLine("Third Point Clockwise [5.67, 123.23, 6.78, -129.33, 12.34, 129.01] = " + CalcCheckClockwiseNumbers(5.67, 123.23, 6.78, -129.33, 12.34, 129.01, out booLinearOrCoincident, out intNegOne_Zero_One, out dblDistance));
      //Console.WriteLine("Distance to infinite Line = " + DistancePointToInfiniteLine(5.67, 123.23, 6.78, -129.33, 12.34, 129.01, out _));
      //Console.WriteLine("Linear or Coincident = " + booLinearOrCoincident + ", [-1, 0, 1]? = " + intNegOne_Zero_One.ToString() + ", Distance = " + dblDistance.ToString("0.000"));
      //Console.WriteLine("Third Point Clockwise [0, 0, 5, 5, 10, 10] = " + CalcCheckClockwiseNumbers(0, 0, 5, 5, 10, 10, out booLinearOrCoincident, out intNegOne_Zero_One, out dblDistance));
      //Console.WriteLine("Linear or Coincident = " + booLinearOrCoincident + ", [-1, 0, 1]? = " + intNegOne_Zero_One.ToString() + ", Distance = " + dblDistance.ToString("0.000"));
      //Console.WriteLine("Third Point Clockwise [0, 0, 5, 5, 5, 5] = " + CalcCheckClockwiseNumbers(0, 0, 5, 5, 5, 5, out booLinearOrCoincident, out intNegOne_Zero_One, out dblDistance));
      //Console.WriteLine("Linear or Coincident = " + booLinearOrCoincident + ", [-1, 0, 1]? = " + intNegOne_Zero_One.ToString() + ", Distance = " + dblDistance.ToString("0.000"));
      //Console.WriteLine("Third Point Clockwise [5, 5, 5, 5, 5, 5] = " + CalcCheckClockwiseNumbers(5, 5, 5, 5, 5, 5, out booLinearOrCoincident, out intNegOne_Zero_One, out dblDistance));
      //Console.WriteLine("Linear or Coincident = " + booLinearOrCoincident + ", [-1, 0, 1]? = " + intNegOne_Zero_One.ToString() + ", Distance = " + dblDistance.ToString("0.000"));
    }

    /// <summary>
    ///   ADAPTED FROM "GRAPHIC GEMS" BY ANDREW S. GLASSNER (ACADEMIC PRESS, 1993), P. 61-63 ["NICE NUMBERS FOR GRAPH LABELS"]<br></br>
    ///   dblGraphMinToFill, dblGraphMaxToFill, dblIntervalToFill and all Tic numeric values are in unconverted units<br></br>
    ///   All strTextValuesToFill() values are in converted units
    ///   
    /// </summary>
    /// <param name="dblMinimum"></param>
    /// <param name="dblMaximum"></param>
    /// <param name="lngMinIntervals"></param>
    /// <param name="strTextValuesToFill"></param>
    /// <param name="dblIntervalToFill"></param>
    /// <param name="dblGraphMinToFill"></param>
    /// <param name="dblGraphMaxToFill"></param>
    /// <param name="booSucceeded"></param>
    /// <param name="dblConvertedMinVal"></param>
    /// <param name="strConvertedMinText"></param>
    /// <param name="dblConvertedMaxVal"></param>
    /// <param name="strConvertedMaxText"></param>
    /// <param name="dblConvertedIntervalVal"></param>
    /// <param name="strConvertedIntervalText"></param>
    /// <param name="strFormatStringToFill"></param>
    /// <param name="dblConversionFactor"></param>
    /// <param name="strForceFormatString"></param>
    /// <returns></returns>
    public static double[] ReturnRoundedIntervals2(double dblMinimum, double dblMaximum, long lngMinIntervals, out string[] strTextValuesToFill, out double dblIntervalToFill, out double dblGraphMinToFill, out double dblGraphMaxToFill, out bool booSucceeded, out double dblConvertedMinVal, out string strConvertedMinText, out double dblConvertedMaxVal, out string strConvertedMaxText, out double dblConvertedIntervalVal, out string strConvertedIntervalText, out string strFormatStringToFill, double dblConversionFactor = 1, string strForceFormatString = "")
    {
      double dblConvertMaximum = dblMaximum * dblConversionFactor;
      double dblConvertMinimum = dblMinimum * dblConversionFactor;
      strFormatStringToFill = strForceFormatString;

      if (dblConvertMaximum == dblConvertMinimum)
      {
        booSucceeded = false;
        strTextValuesToFill = new string[0];
        dblIntervalToFill = double.NaN;
        dblGraphMinToFill = double.NaN;
        dblGraphMaxToFill = double.NaN;
        dblConvertedMinVal = double.NaN;
        dblConvertedMaxVal = double.NaN;
        strConvertedMinText = "";
        strConvertedMaxText = "";
        dblConvertedIntervalVal = double.NaN;
        strConvertedIntervalText = "";
        return new double[0];
      }
      else if (dblConvertMaximum < dblConvertMinimum)
      {
        double dblTemp = dblConvertMaximum;
        dblConvertMaximum = dblConvertMinimum;
        dblConvertMinimum = dblTemp;
      }

      double dblRange = NiceNumber(dblConvertMaximum - dblConvertMinimum, false);
      dblIntervalToFill = NiceNumber(dblRange / (double)(lngMinIntervals - 1), true);
      if (dblIntervalToFill <= 0 || double.IsNaN(dblIntervalToFill) || double.IsInfinity(dblIntervalToFill))
      {
        booSucceeded = false;
        strTextValuesToFill = new string[0];
        dblGraphMinToFill = double.NaN;
        dblGraphMaxToFill = double.NaN;
        dblConvertedMinVal = double.NaN;
        dblConvertedMaxVal = double.NaN;
        strConvertedMinText = "";
        strConvertedMaxText = "";
        dblConvertedIntervalVal = double.NaN;
        strConvertedIntervalText = "";
        return new double[0];
      }
      double dblTempGraphMin = Math.Floor(dblConvertMinimum / dblIntervalToFill) * dblIntervalToFill;
      double dblTempGraphMax = Math.Ceiling(dblConvertMaximum / dblIntervalToFill) * dblIntervalToFill;
      int intNFrac = Math.Max((int)-Math.Floor(LogX(10, dblIntervalToFill)), 0);

      dblConvertedMaxVal = dblTempGraphMax;

      string strFormatString = strForceFormatString;
      if (strForceFormatString == "")
      {
        if (intNFrac == 0) { strFormatString = "0"; }
        else { strFormatString = "0." + new string('0', intNFrac); }
      }
      strFormatStringToFill = strFormatString;

      long lngCounter = 0;

      for (double dblInterval = dblTempGraphMin; dblInterval < dblTempGraphMax + (dblIntervalToFill / 2); dblInterval += dblIntervalToFill)
      {
        lngCounter++;
        dblConvertedMaxVal = dblInterval;
      }
      double[] dblReturn = new double[lngCounter];
      strTextValuesToFill = new string[lngCounter];
      lngCounter = -1;
      for (double dblInterval = dblTempGraphMin; dblInterval < dblTempGraphMax + (dblIntervalToFill / 2); dblInterval += dblIntervalToFill)
      {
        lngCounter++;
        dblReturn[lngCounter] = dblInterval / dblConversionFactor;
        strTextValuesToFill[lngCounter] = dblInterval.ToString(strFormatString);
      }

      dblConvertedIntervalVal = dblIntervalToFill;
      dblConvertedMinVal = dblTempGraphMin;
      strConvertedIntervalText = dblConvertedIntervalVal.ToString(strFormatString);
      strConvertedMinText = dblConvertedMinVal.ToString(strFormatString);
      strConvertedMaxText = dblConvertedMaxVal.ToString(strFormatString);

      dblIntervalToFill /= dblConversionFactor;
      dblGraphMinToFill = dblReturn[0];
      dblGraphMaxToFill = dblReturn[dblReturn.GetLength(0) - 1];
      booSucceeded = true;
      return dblReturn;

      //double dblMinimum = 1.23456;
      //double dblMaximum = 3.537698;
      //long lngIntervals = 25;
      //double[] dblReturn = ReturnRoundedIntervals2(dblMinimum, dblMaximum, lngIntervals, out string[] strLabels, out double dblIntervalToFill, out double dblGraphMinToFill, out double dblGraphMaxToFill, out bool booSucceeded, out double dblConvertedMinVal, out string strConvertedMinText, out double dblConvertedMaxVal, out string strConvertedMaxText, out double dblConvertedINtervalVal, out string strConvertedIntervalText, out string strFormatStringToFill, 1, "");
      //Console.WriteLine("Going from " + dblMinimum.ToString(strFormatStringToFill) + " to " + dblMaximum.ToString(strFormatStringToFill) + " with " + lngIntervals.ToString(strFormatStringToFill) + " intervals");
      //Console.WriteLine("Converted Values: " + dblConvertedMinVal.ToString(strFormatStringToFill) + " to " + dblConvertedMaxVal.ToString(strFormatStringToFill) + " with " + dblIntervalToFill.ToString(strFormatStringToFill) + " intervals");
      //for (int i = 0; i < dblReturn.Length; i++)
      //{
      //  Console.WriteLine("..." + i.ToString("0") + "]  " + strLabels[i] + "  (" + dblReturn[i].ToString("0.00000") + ")");
      //}
    }

    /// <summary>
    ///   ADAPTED FROM "GRAPHIC GEMS" BY ANDREW S. GLASSNER (ACADEMIC PRESS, 1993), P. 61-63 ["NICE NUMBERS FOR GRAPH LABELS"]<br></br>
    ///   Returns a "nice" number approximately equal to dblX.  Rounds the number if booRound = True, otherwise takes the ceiling of the number.
    /// </summary>
    /// <param name="dblX"></param>
    /// <param name="booRound"></param>
    /// <returns></returns>
    public static double NiceNumber(double dblX, bool booRound)
    {
      long lngExp = ReturnDecimalMagnitude(dblX);
      double dblFraction = dblX / Math.Pow(10, lngExp);
      double dblRoundFraction;
      if (booRound)
      {
        if (dblFraction < 1.5) { dblRoundFraction = 1; }
        else if (dblFraction < 3) { dblRoundFraction = 2; }
        else if (dblFraction < 7) { dblRoundFraction = 5; }
        else { dblRoundFraction = 10; }
      }
      else
      {
        if (dblFraction <= 1) { dblRoundFraction = 1; }
        else if (dblFraction <= 2) { dblRoundFraction = 2; }
        else if (dblFraction <= 5) { dblRoundFraction = 5; }
        else { dblRoundFraction = 10; }
      }
      return dblRoundFraction * (Math.Pow(10, lngExp));
    }
    public static double LogX(double dblBase, double dblValue) => Math.Log(dblValue) / Math.Log(dblBase);
    public static long ReturnDecimalMagnitude(double dblVal) => (long)Math.Floor(LogX(10d, Math.Abs(dblVal)));

    ///<summary> 
    /// Given two points defining a line, returns the Y-coordinate at a specified X-coordinate on the line.  If vertical line, returns first given Y-coordinate
    ///</summary>
    public static double CalcNewYOnLine(double[] dblPoint1, double[] dblPoint2, double dblGivenX) => (dblPoint2[0] == dblPoint1[0]) ? dblPoint1[1] : (((dblPoint2[1] - dblPoint1[1]) / (dblPoint2[0] - dblPoint1[0])) * (dblGivenX - dblPoint1[0])) + dblPoint1[1];
    ///<summary>
    /// Given X/Y coordinates of two points defining a line, returns the Y-coordinate at a specified X-coordinate on the line.  If vertical line, returns first given Y-coordinate
    ///</summary>
    public static double CalcNewYOnLine(double dblStartX, double dblStartY, double dblEndX, double dblEndY, double dblGivenX) => (dblEndX == dblStartX) ? dblStartY : (((dblEndY - dblStartY) / (dblEndX - dblStartX)) * (dblGivenX - dblStartX)) + dblStartY;

    ///<summary>
    /// Given two points defining a line, returns the X-coordinate at a specified Y-coordinate on the line.
    ///</summary>
    public static double CalcNewXOnLine(double[] dblPoint1, double[] dblPoint2, double dblGivenY) => (dblPoint2[0] == dblPoint1[0]) ? dblPoint1[0] : dblPoint1[0] + ((dblGivenY - dblPoint1[1]) / ((dblPoint2[1] - dblPoint1[1]) / (dblPoint2[0] - dblPoint1[0])));
    ///<summary>
    /// Given X/Y coordinates of two points defining a line, returns the X-coordinate at a specified Y-coordinate on the line.
    ///</summary>
    public static double CalcNewXOnLine(double dblStartX, double dblStartY, double dblEndX, double dblEndY, double dblGivenY) => (dblEndX == dblStartX) ? dblStartX : dblStartX + ((dblGivenY - dblStartY) / ((dblEndY - dblStartY) / (dblEndX - dblStartX)));

    ///<summary>
    /// Given a slope, plus X/Y coordinates of a point on the line, returns the Y-coordinate at a specified X-coordinate on the line.  If vertical line, returns given Y-coordinate
    ///</summary>
    public static double CalcNewYOnLineBySlope(double dblSlope, double[] dblPoint, double dblGivenX) => (double.IsNaN(dblSlope) || double.IsInfinity(dblSlope)) ? dblPoint[1] : (dblSlope * (dblGivenX - dblPoint[0])) + dblPoint[1];
    ///<summary>
    /// Given a slope, plus X/Y coordinates of a point on the line, returns the Y-coordinate at a specified X-coordinate on the line.  If vertical line, returns given Y-coordinate
    ///</summary>
    public static double CalcNewYOnLineBySlope(double dblSlope, double dblStartX, double dblStartY, double dblGivenX) => (double.IsNaN(dblSlope) || double.IsInfinity(dblSlope)) ? dblStartY : (dblSlope * (dblGivenX - dblStartX)) + dblStartY;
    ///<summary>
    /// Given a slope, plus X/Y coordinates of a point on the line, returns the X-coordinate at a specified Y-coordinate on the line.
    ///</summary>
    public static double CalcNewXOnLineBySlope(double dblSlope, double[] dblPoint, double dblGivenY) => (double.IsNaN(dblSlope) || double.IsInfinity(dblSlope)) ? dblPoint[0] : dblPoint[0] + ((dblGivenY - dblPoint[1]) / dblSlope);
    ///<summary>
    /// Given a slope, plus X/Y coordinates of a point on the line, returns the X-coordinate at a specified Y-coordinate on the line.
    ///</summary>
    public static double CalcNewXOnLineBySlope(double dblSlope, double dblStartX, double dblStartY, double dblGivenY) => (double.IsNaN(dblSlope) || double.IsInfinity(dblSlope)) ? dblStartX : dblStartX + ((dblGivenY - dblStartY) / dblSlope);

    public static double CalcDirectionDeviationDegrees(double dblAngle1, double dblAngle2)
    {
      //double dblReturn = ForceAzimuthToCorrectRange(Math.Abs(dblAngle2 - dblAngle1));
      //if (dblReturn > 180) { dblReturn = 360 - dblReturn; }

      //CalcPointLine(0, 0, 1, dblAngle1, out double dblQX, out double dblQY, out _);
      //CalcPointLine(dblQX, dblQY, 1, dblAngle2, out double dblRX, out double dblRY, out _);
      //bool booClockwise = CalcCheckClockwiseNumbers(0, 0, dblQX, dblQY, dblRX, dblRY, out _, out _, out _);
      //if (!booClockwise) { dblReturn = -(Math.Abs(dblReturn)); }

      //return dblReturn;

        // Normalize angles to [0, 360)
        dblAngle1 = ForceAzimuthToCorrectRange(dblAngle1);
        dblAngle2 = ForceAzimuthToCorrectRange(dblAngle2);

        // Calculate the signed difference and wrap to [-180, 180)
        double diff = dblAngle2 - dblAngle1;
        diff = (diff + 180) % 360;
        if (diff < 0)
            diff += 360;
        diff -= 180;

        return diff;

            //Console.WriteLine("-170.738 to 263.076:  " + CalcDirectionDeviationDegrees(-170.7384, 263.0756).ToString("0.000") + "  [73.814)");
            //Console.WriteLine("51.785 to -109.797:  " + CalcDirectionDeviationDegrees(51.7850, -109.7972).ToString("0.000") + "  [-161.582)");
            //Console.WriteLine("376.312 to 308.898:  " + CalcDirectionDeviationDegrees(376.3116, 308.8984).ToString("0.000") + "  [-67.413)");
            //Console.WriteLine("73.399 to 248.116:  " + CalcDirectionDeviationDegrees(73.3987, 248.1156).ToString("0.000") + "  [174.717)");
            //Console.WriteLine("374.312 to 159.684:  " + CalcDirectionDeviationDegrees(374.3115, 159.6836).ToString("0.000") + "  [145.372)");
            //Console.WriteLine("446.687 to 251.316:  " + CalcDirectionDeviationDegrees(446.6875, 251.3155).ToString("0.000") + "  [164.628)");
            //Console.WriteLine("55.512 to 63.678:  " + CalcDirectionDeviationDegrees(55.5125, 63.6784).ToString("0.000") + "  [8.166)");
            //Console.WriteLine("471.910 to 424.026:  " + CalcDirectionDeviationDegrees(471.9102, 424.0263).ToString("0.000") + "  [-47.884)");
            //Console.WriteLine("412.244 to 148.895:  " + CalcDirectionDeviationDegrees(412.2436, 148.8954).ToString("0.000") + "  [96.652)");
            //Console.WriteLine("530.029 to 208.362:  " + CalcDirectionDeviationDegrees(530.0287, 208.3621).ToString("0.000") + "  [38.333)");
            //Console.WriteLine("-76.838 to 164.363:  " + CalcDirectionDeviationDegrees(-76.8381, 164.3629).ToString("0.000") + "  [-118.799)");
            //Console.WriteLine("-21.042 to -44.422:  " + CalcDirectionDeviationDegrees(-21.0419, -44.4222).ToString("0.000") + "  [-23.380)");
            //Console.WriteLine("99.503 to 121.375:  " + CalcDirectionDeviationDegrees(99.5030, 121.3750).ToString("0.000") + "  [21.872)");
            //Console.WriteLine("269.817 to 217.319:  " + CalcDirectionDeviationDegrees(269.8165, 217.3190).ToString("0.000") + "  [-52.498)");
            //Console.WriteLine("411.330 to 394.741:  " + CalcDirectionDeviationDegrees(411.3301, 394.7411).ToString("0.000") + "  [-16.589)");
            //Console.WriteLine("323.175 to 496.872:  " + CalcDirectionDeviationDegrees(323.1752, 496.8718).ToString("0.000") + "  [173.697)");
            //Console.WriteLine("-126.215 to -155.964:  " + CalcDirectionDeviationDegrees(-126.2149, -155.9635).ToString("0.000") + "  [-29.749)");
            //Console.WriteLine("5.309 to 340.413:  " + CalcDirectionDeviationDegrees(5.3087, 340.4129).ToString("0.000") + "  [-24.896)");
            //Console.WriteLine("253.337 to -125.148:  " + CalcDirectionDeviationDegrees(253.3375, -125.1481).ToString("0.000") + "  [-18.486)");
            //Console.WriteLine("184.444 to -111.740:  " + CalcDirectionDeviationDegrees(184.4444, -111.7402).ToString("0.000") + "  [63.815)");
        }

    ///<summary>
    /// Given Percent Slope, returns Slope in Degrees.
    ///<br/><br/>Returns double value
    ///</summary>
    public static double Slope_PercentToDeg(double dblPercent) => Math.Atan(dblPercent) * 180 / Math.PI;
    ///<summary>
    /// Given Slope in Degrees, returns Percent Slope.
    ///<br/><br/>Returns double value
    ///</summary>
    public static double Slope_DegToPercent(double dblDegrees) => Math.Tan(dblDegrees * Math.PI / 180);

    ///<summary>
    /// Given two endpoints and a specified number of vertices, plus option for JenSphericalMethod, returns straight polyline with correct number of vertices.
    ///<br/><br/>Returns polyline jagged double array with single polyline
    ///</summary>
    public static double[][,] PolylineFromEndpoints(double[] dblSegment1Start, double[] dblSegment1End, long lngNumVertices, JenSphericalMethod jenMethod,
       double dblSemiMajorAxis = 6378137.000, double dblSemiMinorAxis = 6356752.31424518, double dblSphereRadius = 6371000.79000915)
    {
      return PolylineFromEndpoints(dblSegment1Start[0], dblSegment1Start[1], dblSegment1End[0], dblSegment1End[1], lngNumVertices, jenMethod, dblSemiMajorAxis, dblSemiMinorAxis, dblSphereRadius);
    }
    ///<summary>
    /// Given two endpoints and a specified number of vertices, plus option for JenSphericalMethod, returns polyline with correct number of vertices.
    ///<br/><br/>Returns polyline jagged double array with single polyline
    ///</summary>
    public static double[][,] PolylineFromEndpoints(double dblPoint1X, double dblPoint1Y, double dblPoint2X, double dblPoint2Y, long lngNumVertices, JenSphericalMethod jenMethod,
       double dblSemiMajorAxis = 6378137.000, double dblSemiMinorAxis = 6356752.31424518, double dblSphereRadius = 6371000.79000915)
    {
      // in the future, write option to go long way around planet?
      if (lngNumVertices < 2) { lngNumVertices = 2; }
      double dblInterval = 1d / ((double)lngNumVertices - 1d);
      double[][,] dblReturn = new double[1][,] { new double[lngNumVertices, 2] };

      // FOR SPHERICAL/SPHEROIDAL
      double[][,] dblSegment = new double[1][,] { new double[2, 2] { { dblPoint1X, dblPoint1Y }, { dblPoint2X, dblPoint2Y } } };

      if ((dblPoint1X == dblPoint2X) && (dblPoint1Y == dblPoint2Y))
      {
        return dblSegment;
      }

      // ADD START AND ENDPOINTS
      dblReturn[0][0, 0] = dblPoint1X;
      dblReturn[0][0, 1] = dblPoint1Y;
      dblReturn[0][lngNumVertices - 1, 0] = dblPoint2X;
      dblReturn[0][lngNumVertices - 1, 1] = dblPoint2Y;

      double dblTempX = 0;
      double dblTempY = 0;
      double dblBearing = 0;
      switch (jenMethod)
      {
        case JenSphericalMethod.ENUM_UseTrigonometry:
          dblInterval *= DistancePythagoreanNumbers(dblPoint1X, dblPoint1Y, dblPoint2X, dblPoint2Y);
          dblBearing = CalcBearingNumbers(dblPoint1X, dblPoint1Y, dblPoint2X, dblPoint2Y);
          break;
        case JenSphericalMethod.ENUM_UseSpherical:
          //dblBearing = AzimuthHaversineNumbers(dblPoint1X, dblPoint1Y, dblPoint2X, dblPoint2Y);
          break;
        case JenSphericalMethod.ENUM_UseSpheroidal:
          //double dblTemp = DistanceHaversineNumbers(dblPoint1X, dblPoint1Y, dblPoint2X, dblPoint2Y, out dblBearing);
          break;
        default:
          break;
      }

      for (int i = 1; i < lngNumVertices - 1; i++)
      {
        switch (jenMethod)
        {
          case JenSphericalMethod.ENUM_UseTrigonometry:
            CalcPointLine(dblPoint1X, dblPoint1Y, (double)i * dblInterval, dblBearing, out dblTempX, out dblTempY, out _);
            break;
          case JenSphericalMethod.ENUM_UseSpherical:
            SpheroidalPolylineMidpointNumbers(dblSegment, (double)i * dblInterval, true, out dblTempX, out dblTempY, dblSphereRadius, dblSphereRadius);
            break;
          case JenSphericalMethod.ENUM_UseSpheroidal:
            SpheroidalPolylineMidpointNumbers(dblSegment, (double)i * dblInterval, true, out dblTempX, out dblTempY, dblSemiMajorAxis, dblSemiMinorAxis);
            break;
          default:
            break;
        }
        dblReturn[0][i, 0] = dblTempX;
        dblReturn[0][i, 1] = dblTempY;
      }

      return dblReturn;

      //double[][,] dblCoords = PolylineFromEndpoints(447149.696, 3901793.447, 447142.836, 3901782.797, 4, JenSphericalMethod.ENUM_UseTrigonometry);
      //double[][,] dblCoords = PolylineFromEndpoints(new double[]{ 447149.696, 3901793.447}, new double[]{ 447142.836, 3901782.797}, 4, JenSphericalMethod.ENUM_UseTrigonometry);
      //Console.WriteLine("...Trigonometry, 2 vertices...");
      //for (int i = 0; i < dblCoords[0].GetLength(0); i++)
      //{
      //  Console.WriteLine(i.ToString("0") + "...[" + dblCoords[0][i, 0].ToString("0.0000000") + ", " + dblCoords[0][i, 1].ToString("0.0000000") + "]");
      //}
      ////...Trigonometry, 4 vertices...
      ////0...[447149.6960000, 3901793.4470000]
      ////1...[447147.4093333, 3901789.8970000]
      ////2...[447145.1226667, 3901786.3470000]
      ////3...[447142.8360000, 3901782.7970000]

      //double[][,] dblCoords = PolylineFromEndpoints(-111, 35, -110, 36, 4, JenSphericalMethod.ENUM_UseSpheroidal);
      //double[][,] dblCoords = PolylineFromEndpoints(new double[] { -111, 35 },new double[] { -110, 36 }, 4, JenSphericalMethod.ENUM_UseSpheroidal);
      //Console.WriteLine("...Spheroidal, 4 vertices...");
      //for (int i = 0; i < dblCoords[0].GetLength(0); i++)
      //{
      //  Console.WriteLine(i.ToString("0") + "...[" + dblCoords[0][i, 0].ToString("0.0000000") + ", " + dblCoords[0][i, 1].ToString("0.0000000") + "]");
      //}
      ////...Spheroidal, 4 vertices...
      ////0...[-111.0000000, 35.0000000]
      ////1...[-110.6694129, 35.3342695]
      ////2...[-110.3360959, 35.6676091]
      ////3...[-110.0000000, 36.0000000]
    }

    ///<summary>
    /// Given endpoint coordinates for two 3D segments, returns distance between them and closest point coordinates on respective segments
    ///<br/><br/>Returns double values
    ///</summary>
    public static double SquaredDistanceBetweenSegments3D(double[][,] dblPolylineSegment1, double[][,] dblPolylineSegment2,
      out double[] dblClosePointOnSeg1, out double[] dblClosePointOnSeg2)
    {
      double dblDistance = SquaredDistanceBetweenSegments3D(dblPolylineSegment1[0][0, 0], dblPolylineSegment1[0][0, 1], dblPolylineSegment1[0][0, 2], dblPolylineSegment1[0][1, 0], dblPolylineSegment1[0][1, 1], dblPolylineSegment1[0][1, 2], dblPolylineSegment2[0][0, 0], dblPolylineSegment2[0][0, 1], dblPolylineSegment2[0][0, 2], dblPolylineSegment2[0][1, 0], dblPolylineSegment2[0][1, 1], dblPolylineSegment2[0][1, 2], out double dblClosePointOnSeg1_X, out double dblClosePointOnSeg1_Y, out double dblClosePointOnSeg1_Z, out double dblClosePointOnSeg2_X, out double dblClosePointOnSeg2_Y, out double dblClosePointOnSeg2_Z);
      dblClosePointOnSeg1 = new double[3] { dblClosePointOnSeg1_X, dblClosePointOnSeg1_Y, dblClosePointOnSeg1_Z };
      dblClosePointOnSeg2 = new double[3] { dblClosePointOnSeg2_X, dblClosePointOnSeg2_Y, dblClosePointOnSeg2_Z };
      return dblDistance;
    }
    ///<summary>
    /// Given endpoint coordinates for two 3D segments, returns distance between them and closest point coordinates on respective segments
    ///<br/><br/>Returns double values
    ///</summary>
    public static double SquaredDistanceBetweenSegments3D(double[] dblSegment1Start, double[] dblSegment1End, double[] dblSegment2Start, double[] dblSegment2End,
      out double[] dblClosePointOnSeg1, out double[] dblClosePointOnSeg2)
    {
      double dblDistance = SquaredDistanceBetweenSegments3D(dblSegment1Start[0], dblSegment1Start[1], dblSegment1Start[2], dblSegment1End[0], dblSegment1End[1], dblSegment1End[2], dblSegment2Start[0], dblSegment2Start[1], dblSegment2Start[2], dblSegment2End[0], dblSegment2End[1], dblSegment2End[2], out double dblClosePointOnSeg1_X, out double dblClosePointOnSeg1_Y, out double dblClosePointOnSeg1_Z, out double dblClosePointOnSeg2_X, out double dblClosePointOnSeg2_Y, out double dblClosePointOnSeg2_Z);
      dblClosePointOnSeg1 = new double[3] { dblClosePointOnSeg1_X, dblClosePointOnSeg1_Y, dblClosePointOnSeg1_Z };
      dblClosePointOnSeg2 = new double[3] { dblClosePointOnSeg2_X, dblClosePointOnSeg2_Y, dblClosePointOnSeg2_Z };
      return dblDistance;
    }
    ///<summary>
    /// Given endpoint coordinates for two 3D segments, returns distance between them and closest point coordinates on respective segments
    ///<br/><br/>Returns double values
    ///</summary>
    public static double SquaredDistanceBetweenSegments3D(double dblSegment1StartX, double dblSegment1StartY, double dblSegment1StartZ, double dblSegment1EndX, double dblSegment1EndY, double dblSegment1EndZ, double dblSegment2StartX, double dblSegment2StartY, double dblSegment2StartZ, double dblSegment2EndX, double dblSegment2EndY, double dblSegment2EndZ, out double dblClosePointOnSeg1_X, out double dblClosePointOnSeg1_Y, out double dblClosePointOnSeg1_Z, out double dblClosePointOnSeg2_X, out double dblClosePointOnSeg2_Y, out double dblClosePointOnSeg2_Z)
    {
      // Adapted from SoftSurfer code at http://softsurfer.com/Archive/algorithm_0106/algorithm_0106.htm#dist3D_Segment_to_Segment%28%29
      //// dist3D_Segment_to_Segment():
      ////    Input:  two 3D line segments S1 and S2
      ////    Return: the shortest distance between S1 and S2
      //Float
      //dist3D_Segment_to_Segment( Segment S1, Segment S2)


      double dblSmallNum = 0.000000000001;

      double[] dblVectorU = new double[3] { dblSegment1EndX - dblSegment1StartX, dblSegment1EndY - dblSegment1StartY, dblSegment1EndZ - dblSegment1StartZ };     // VECTOR OF (SEGMENT 1 END POINT) - (SEGMENT 1 START POINT)
      double[] dblVectorV = new double[3] { dblSegment2EndX - dblSegment2StartX, dblSegment2EndY - dblSegment2StartY, dblSegment2EndZ - dblSegment2StartZ };     // VECTOR OF (SEGMENT 2 END POINT) - (SEGMENT 2 START POINT)
      double[] dblVectorW = new double[3] { dblSegment1StartX - dblSegment2StartX, dblSegment1StartY - dblSegment2StartY, dblSegment1StartZ - dblSegment2StartZ }; // VECTOR OF (SEGMENT 1 START POINT) - (SEGMENT 2 START POINT)

      double dblA = VectorDotProduct(dblVectorU, dblVectorU);      // DOT PRODUCT OF (VectorU * VectorU)
      double dblB = VectorDotProduct(dblVectorU, dblVectorV);      // DOT PRODUCT OF (VectorU * VectorV)
      double dblC = VectorDotProduct(dblVectorV, dblVectorV);      // DOT PRODUCT OF (VectorV * VectorV)
      double dblD = VectorDotProduct(dblVectorU, dblVectorW);      // DOT PRODUCT OF (VectorU * VectorW)
      double dblE = VectorDotProduct(dblVectorV, dblVectorW);      // DOT PRODUCT OF (VectorV * VectorW)
      double dblDenominator = (dblA * dblC) - (dblB * dblB);
      double dblsc;
      double dblsN;
      double dblsD = dblDenominator;
      double dbltc;
      double dbltN;
      double dbltD = dblDenominator;

      if (dblDenominator < dblSmallNum)   // the lines are almost parallel
      {                                   // force using point P0 on segment S1
        dblsN = 0;                        // to prevent possible division by 0.0 later
        dblsD = 1;
        dbltN = dblE;
        dbltD = dblC;
      }
      else
      {
        dblsN = (dblB * dblE) - (dblC * dblD);
        dbltN = (dblA * dblE) - (dblB * dblD);
        if (dblsN < 0)
        {
          dblsN = 0;
          dbltN = dblE;
          dbltD = dblC;
        }
        else if (dblsN > dblsD)
        {
          dblsN = dblsD;
          dbltN = dblE + dblB;
          dbltD = dblC;
        }
      }
      if (dbltN < 0)
      {
        dbltN = 0;
        if (-dblD < 0) { dblsN = 0; }
        else if (-dblD > dblA) { dblsN = dblsD; }
        else { dblsN = -dblD; dblsD = dblA; }
      }
      else if (dbltN > dbltD)
      {
        dbltN = dbltD;
        if (-dblD + dblB < 0) { dblsN = 0; }
        else if (-dblD + dblB > dblA) { dblsN = dblsD; }
        else { dblsN = -dblD + dblB; dblsD = dblA; }
      }

      if (Math.Abs(dblsN) < dblSmallNum) { dblsc = 0; }
      else { dblsc = dblsN / dblsD; }

      if (Math.Abs(dbltN) < dblSmallNum) { dbltc = 0; }
      else { dbltc = dbltN / dbltD; }

      dblClosePointOnSeg1_X = dblSegment1StartX + (dblsc * dblVectorU[0]);
      dblClosePointOnSeg2_X = dblSegment2StartX + (dbltc * dblVectorV[0]);
      dblClosePointOnSeg1_Y = dblSegment1StartY + (dblsc * dblVectorU[1]);
      dblClosePointOnSeg2_Y = dblSegment2StartY + (dbltc * dblVectorV[1]);
      dblClosePointOnSeg1_Z = dblSegment1StartZ + (dblsc * dblVectorU[2]);
      dblClosePointOnSeg2_Z = dblSegment2StartZ + (dbltc * dblVectorV[2]);
      double dblDistance = Math.Pow(dblClosePointOnSeg2_X - dblClosePointOnSeg1_X, 2) + Math.Pow(dblClosePointOnSeg2_Y - dblClosePointOnSeg1_Y, 2) + Math.Pow(dblClosePointOnSeg2_Z - dblClosePointOnSeg1_Z, 2);

      return dblDistance;

      //double dblDistance = SquaredDistanceBetweenSegments3D(447102.647150, 3901831.331690, 100001.402790, 447241.792360, 3901719.289740, 100063.834160, 447142.836190, 3901782.796850, 100018.688190, 447201.486910, 3901673.864700, 100068.474780, out double dblClosePointOnSeg1_X, out double dblClosePointOnSeg1_Y, out double dblClosePointOnSeg1_Z, out double dblClosePointOnSeg2_X, out double dblClosePointOnSeg2_Y, out double dblClosePointOnSeg2_Z);
      //Console.WriteLine("Point on Segment 1: [" + dblClosePointOnSeg1_X.ToString("0.000") + ", " + dblClosePointOnSeg1_Y.ToString("0.000") + ", " + dblClosePointOnSeg1_Z.ToString("0.000") + "]");
      //Console.WriteLine("Point on Segment 2: [" + dblClosePointOnSeg2_X.ToString("0.000") + ", " + dblClosePointOnSeg2_Y.ToString("0.000") + ", " + dblClosePointOnSeg2_Z.ToString("0.000") + "]\nDistance = " + Math.Sqrt(dblDistance).ToString("0.000"));
      //double dblDistance = SquaredDistanceBetweenSegments3D(new double[] { 447102.647150, 3901831.331690, 100001.402790 }, new double[]{ 447241.792360, 3901719.289740, 100063.834160 }, new double[] { 447142.836190, 3901782.796850, 100018.688190 }, new double[] { 447201.486910, 3901673.864700, 100068.474780 }, out double[] dblClosePointOnSeg1, out double []dblClosePointOnSeg2);
      //Console.WriteLine("Point on Segment 1: [" + dblClosePointOnSeg1[0].ToString("0.000") + ", " + dblClosePointOnSeg1[1].ToString("0.000") + ", " + dblClosePointOnSeg1[2].ToString("0.000") + "]");
      //Console.WriteLine("Point on Segment 2: [" + dblClosePointOnSeg2[0].ToString("0.000") + ", " + dblClosePointOnSeg2[1].ToString("0.000") + ", " + dblClosePointOnSeg2[2].ToString("0.000") + "]\nDistance = " + Math.Sqrt(dblDistance).ToString("0.000"));
      //double dblDistance = SquaredDistanceBetweenSegments3D(new double[][,] { new double[,] {{ 447102.647150, 3901831.331690, 100001.402790 }, { 447241.792360, 3901719.289740, 100063.834160 } }}, new double[][,] {new double[,]{{ 447142.836190, 3901782.796850, 100018.688190 }, { 447201.486910, 3901673.864700, 100068.474780 }}}, out double[] dblClosePointOnSeg1, out double[] dblClosePointOnSeg2);
      //Console.WriteLine("Point on Segment 1: [" + dblClosePointOnSeg1[0].ToString("0.000") + ", " + dblClosePointOnSeg1[1].ToString("0.000") + ", " + dblClosePointOnSeg1[2].ToString("0.000") + "]");
      //Console.WriteLine("Point on Segment 2: [" + dblClosePointOnSeg2[0].ToString("0.000") + ", " + dblClosePointOnSeg2[1].ToString("0.000") + ", " + dblClosePointOnSeg2[2].ToString("0.000") + "]\nDistance = " + Math.Sqrt(dblDistance).ToString("0.000"));
      //Point on Segment 1: [447149.696, 3901793.447, 100022.513]
      //Point on Segment 2: [447142.836, 3901782.797, 100018.688]
      //Distance = 13.233
    }
    ///<summary>
    /// Given endpoint coordinates for two 2D segments, returns distance between them and closest point coordinates on respective segments
    ///<br/><br/>Returns double values
    ///</summary>
    public static double SquaredDistanceBetweenSegments(double[][,] dblPolylineSegment1, double[][,] dblPolylineSegment2,
      out double[] dblClosePointOnSeg1, out double[] dblClosePointOnSeg2)
    {
      double dblDistance = SquaredDistanceBetweenSegments(dblPolylineSegment1[0][0, 0], dblPolylineSegment1[0][0, 1], dblPolylineSegment1[0][1, 0], dblPolylineSegment1[0][1, 1], dblPolylineSegment2[0][0, 0], dblPolylineSegment2[0][0, 1], dblPolylineSegment2[0][1, 0], dblPolylineSegment2[0][1, 1], out double dblClosePointOnSeg1_X, out double dblClosePointOnSeg1_Y, out double dblClosePointOnSeg2_X, out double dblClosePointOnSeg2_Y);
      dblClosePointOnSeg1 = new double[2] { dblClosePointOnSeg1_X, dblClosePointOnSeg1_Y };
      dblClosePointOnSeg2 = new double[2] { dblClosePointOnSeg2_X, dblClosePointOnSeg2_Y };
      return dblDistance;
    }
    ///<summary>
    /// Given endpoint coordinates for two 2D segments, returns distance between them and closest point coordinates on respective segments
    ///<br/><br/>Returns double values
    ///</summary>
    public static double SquaredDistanceBetweenSegments(double[] dblSegment1Start, double[] dblSegment1End, double[] dblSegment2Start, double[] dblSegment2End,
      out double[] dblClosePointOnSeg1, out double[] dblClosePointOnSeg2)
    {
      double dblDistance = SquaredDistanceBetweenSegments(dblSegment1Start[0], dblSegment1Start[1], dblSegment1End[0], dblSegment1End[1], dblSegment2Start[0], dblSegment2Start[1], dblSegment2End[0], dblSegment2End[1], out double dblClosePointOnSeg1_X, out double dblClosePointOnSeg1_Y, out double dblClosePointOnSeg2_X, out double dblClosePointOnSeg2_Y);
      dblClosePointOnSeg1 = new double[2] { dblClosePointOnSeg1_X, dblClosePointOnSeg1_Y };
      dblClosePointOnSeg2 = new double[2] { dblClosePointOnSeg2_X, dblClosePointOnSeg2_Y };
      return dblDistance;
    }
    ///<summary>
    /// Given endpoint coordinates for two 2D segments, returns distance between them and closest point coordinates on respective segments
    ///<br/><br/>Returns double values
    ///</summary>
    public static double SquaredDistanceBetweenSegments(double dblSegment1StartX, double dblSegment1StartY, double dblSegment1EndX, double dblSegment1EndY,
      double dblSegment2StartX, double dblSegment2StartY, double dblSegment2EndX, double dblSegment2EndY, out double dblClosePointOnSeg1_X, out double dblClosePointOnSeg1_Y,
      out double dblClosePointOnSeg2_X, out double dblClosePointOnSeg2_Y)
    {
      // Adapted from SoftSurfer code at http://softsurfer.com/Archive/algorithm_0106/algorithm_0106.htm#dist3D_Segment_to_Segment%28%29
      //// dist3D_Segment_to_Segment():
      ////    Input:  two 3D line segments S1 and S2
      ////    Return: the shortest distance between S1 and S2
      //Float
      //dist3D_Segment_to_Segment( Segment S1, Segment S2)


      double dblSmallNum = 0.000000000001;

      double[] dblVectorU = new double[2] { dblSegment1EndX - dblSegment1StartX, dblSegment1EndY - dblSegment1StartY };     // VECTOR OF (SEGMENT 1 END POINT) - (SEGMENT 1 START POINT)
      double[] dblVectorV = new double[2] { dblSegment2EndX - dblSegment2StartX, dblSegment2EndY - dblSegment2StartY };     // VECTOR OF (SEGMENT 2 END POINT) - (SEGMENT 2 START POINT)
      double[] dblVectorW = new double[2] { dblSegment1StartX - dblSegment2StartX, dblSegment1StartY - dblSegment2StartY }; // VECTOR OF (SEGMENT 1 START POINT) - (SEGMENT 2 START POINT)

      double dblA = VectorDotProduct(dblVectorU, dblVectorU);      // DOT PRODUCT OF (VectorU * VectorU)
      double dblB = VectorDotProduct(dblVectorU, dblVectorV);      // DOT PRODUCT OF (VectorU * VectorV)
      double dblC = VectorDotProduct(dblVectorV, dblVectorV);      // DOT PRODUCT OF (VectorV * VectorV)
      double dblD = VectorDotProduct(dblVectorU, dblVectorW);      // DOT PRODUCT OF (VectorU * VectorW)
      double dblE = VectorDotProduct(dblVectorV, dblVectorW);      // DOT PRODUCT OF (VectorV * VectorW)
      double dblDenominator = (dblA * dblC) - (dblB * dblB);
      double dblsc;
      double dblsN;
      double dblsD = dblDenominator;
      double dbltc;
      double dbltN;
      double dbltD = dblDenominator;

      if (dblDenominator < dblSmallNum)   // the lines are almost parallel
      {                                   // force using point P0 on segment S1
        dblsN = 0;                        // to prevent possible division by 0.0 later
        dblsD = 1;
        dbltN = dblE;
        dbltD = dblC;
      }
      else
      {
        dblsN = (dblB * dblE) - (dblC * dblD);
        dbltN = (dblA * dblE) - (dblB * dblD);
        if (dblsN < 0)
        {
          dblsN = 0;
          dbltN = dblE;
          dbltD = dblC;
        }
        else if (dblsN > dblsD)
        {
          dblsN = dblsD;
          dbltN = dblE + dblB;
          dbltD = dblC;
        }
      }
      if (dbltN < 0)
      {
        dbltN = 0;
        if (-dblD < 0) { dblsN = 0; }
        else if (-dblD > dblA) { dblsN = dblsD; }
        else { dblsN = -dblD; dblsD = dblA; }
      }
      else if (dbltN > dbltD)
      {
        dbltN = dbltD;
        if (-dblD + dblB < 0) { dblsN = 0; }
        else if (-dblD + dblB > dblA) { dblsN = dblsD; }
        else { dblsN = -dblD + dblB; dblsD = dblA; }
      }

      if (Math.Abs(dblsN) < dblSmallNum) { dblsc = 0; }
      else { dblsc = dblsN / dblsD; }

      if (Math.Abs(dbltN) < dblSmallNum) { dbltc = 0; }
      else { dbltc = dbltN / dbltD; }

      dblClosePointOnSeg1_X = dblSegment1StartX + (dblsc * dblVectorU[0]);
      dblClosePointOnSeg2_X = dblSegment2StartX + (dbltc * dblVectorV[0]);
      dblClosePointOnSeg1_Y = dblSegment1StartY + (dblsc * dblVectorU[1]);
      dblClosePointOnSeg2_Y = dblSegment2StartY + (dbltc * dblVectorV[1]);
      double dblDistance = Math.Pow(dblClosePointOnSeg2_X - dblClosePointOnSeg1_X, 2) + Math.Pow(dblClosePointOnSeg2_Y - dblClosePointOnSeg1_Y, 2);

      return dblDistance;

      //// does not intersect
      //double dblDistance = SquaredDistanceBetweenSegments(447082.892510, 3901806.679980, 447228.051510, 3901681.760060, 447135.329730, 3901776.999810, 447283.458150, 3901684.828970, out double dblClosePointOnSeg1_X, out double dblClosePointOnSeg1_Y, out double dblClosePointOnSeg2_X, out double dblClosePointOnSeg2_Y);
      //Console.WriteLine("Point on Segment 1: [" + dblClosePointOnSeg1_X.ToString("0.000") + ", " + dblClosePointOnSeg1_Y.ToString("0.000") + "]");
      //Console.WriteLine("Point on Segment 2: [" + dblClosePointOnSeg2_X.ToString("0.000") + ", " + dblClosePointOnSeg2_Y.ToString("0.000") + "]\nDistance = " + Math.Sqrt(dblDistance).ToString("0.000"));
      //double dblDistance = SquaredDistanceBetweenSegments(new double[] { 447082.892510, 3901806.679980 }, new double[] { 447228.051510, 3901681.760060 }, new double[] { 447135.329730, 3901776.999810 }, new double[] { 447283.458150, 3901684.828970 }, out double[] dblClosePointOnSeg1, out double[] dblClosePointOnSeg2);
      //Console.WriteLine("Point on Segment 1: [" + dblClosePointOnSeg1[0].ToString("0.000") + ", " + dblClosePointOnSeg1[1].ToString("0.000") + "]");
      //Console.WriteLine("Point on Segment 2: [" + dblClosePointOnSeg2[0].ToString("0.000") + ", " + dblClosePointOnSeg2[1].ToString("0.000") + "]\nDistance = " + Math.Sqrt(dblDistance).ToString("0.000"));
      //double dblDistance = SquaredDistanceBetweenSegments(new double[][,] { new double[,] { { 447082.892510, 3901806.679980 }, { 447228.051510, 3901681.760060 } } }, new double[][,] { new double[,] { { 447135.329730, 3901776.999810 }, { 447283.458150, 3901684.828970 } } }, out double[] dblClosePointOnSeg1, out double[] dblClosePointOnSeg2);
      //Console.WriteLine("Point on Segment 1: [" + dblClosePointOnSeg1[0].ToString("0.000") + ", " + dblClosePointOnSeg1[1].ToString("0.000") + "]");
      //Console.WriteLine("Point on Segment 2: [" + dblClosePointOnSeg2[0].ToString("0.000") + ", " + dblClosePointOnSeg2[1].ToString("0.000") + "]\nDistance = " + Math.Sqrt(dblDistance).ToString("0.000"));
      ////Point on Segment 1: [447127.693, 3901768.126]
      ////Point on Segment 2: [447135.330, 3901777.000]
      ////Distance = 11.708

      //// does intersect
      //double dblDistance = SquaredDistanceBetweenSegments(447143.376140, 3901808.020320, 447248.964570, 3901692.485150, 447169.114900, 3901774.601070, 447270.133320, 3901735.419260, out double dblClosePointOnSeg1_X, out double dblClosePointOnSeg1_Y, out double dblClosePointOnSeg2_X, out double dblClosePointOnSeg2_Y);
      //Console.WriteLine("Point on Segment 1: [" + dblClosePointOnSeg1_X.ToString("0.000") + ", " + dblClosePointOnSeg1_Y.ToString("0.000") + "]");
      //Console.WriteLine("Point on Segment 2: [" + dblClosePointOnSeg2_X.ToString("0.000") + ", " + dblClosePointOnSeg2_Y.ToString("0.000") + "]\nDistance = " + Math.Sqrt(dblDistance).ToString("0.000"));
      ////Point on Segment 1: [447176.556, 3901771.715]
      ////Point on Segment 2: [447176.556, 3901771.715]
      ////Distance = 0.000
    }
    ///<summary>
    /// Given 2 double arrays of same length, returns the sum of each element from 1st array multiplied with the same element from the 2nd array.
    ///<br/><br/>Returns double value
    ///</summary>
    public static double VectorDotProduct(double[] dblVector1, double[] dblVector2)
    {
      double dblReturn = 0;
      for (int i = 0; i < dblVector1.Length; i++)
      {
        dblReturn += (dblVector1[i] * dblVector2[i]);
      }
      return dblReturn;
    }


    ///<summary>
    /// Given a multipoint and 2 consecutive points defining a line segment, this scripts calculates the distances to the farthest points CW and CCW of the infinite<br/>
    /// line defined by the segment, and returns the coordinates of those farthest points.<br/>
    ///<br/><br/>Returns double values
    ///</summary>
    public static void CalculateLongestPerpendicularsFromSegment(double[,] dblCoordinates, double[] dblSegmentStart, double[] dblSegmentEnd,
      out double dblLengthClockwise, out double dblLengthCounterClockwise, out double dblFarCW_X, out double dblFarCW_Y, out double dblFarCCW_X, out double dblFarCCW_Y)
    {
      CalculateLongestPerpendicularsFromSegment(dblCoordinates, dblSegmentStart[0], dblSegmentStart[1], dblSegmentEnd[0], dblSegmentEnd[1], out dblLengthClockwise, out dblLengthCounterClockwise, out dblFarCW_X, out dblFarCW_Y, out dblFarCCW_X, out dblFarCCW_Y);
    }
    ///<summary>
    /// Given a multipoint and 2 polyline endpoints defining a line segment, this scripts calculates the distances to the farthest points CW and CCW of the infinite<br/>
    /// line defined by the segment, and returns the coordinates of those farthest points.<br/>
    ///<br/><br/>Returns double values
    ///</summary>
    public static void CalculateLongestPerpendicularsFromSegment(double[,] dblCoordinates, double[][,] dblSegmentPolyline,
      out double dblLengthClockwise, out double dblLengthCounterClockwise, out double dblFarCW_X, out double dblFarCW_Y, out double dblFarCCW_X, out double dblFarCCW_Y)
    {
      long lngMaxPolylineIndex = dblSegmentPolyline.GetLength(0) - 1;
      long lngMaxVertexIndex = dblSegmentPolyline[lngMaxPolylineIndex].GetLength(0) - 1;
      CalculateLongestPerpendicularsFromSegment(dblCoordinates, dblSegmentPolyline[0][0, 0], dblSegmentPolyline[0][0, 1], dblSegmentPolyline[lngMaxPolylineIndex][lngMaxVertexIndex, 0], dblSegmentPolyline[lngMaxPolylineIndex][lngMaxVertexIndex, 1], out dblLengthClockwise, out dblLengthCounterClockwise, out dblFarCW_X, out dblFarCW_Y, out dblFarCCW_X, out dblFarCCW_Y);
    }
    ///<summary>
    /// Given a multipoint and 2 consecutive points defining a line segment, this scripts calculates the distances to the farthest points CW and CCW of the infinite<br/>
    /// line defined by the segment, and returns the coordinates of those farthest points.<br/>
    ///<br/><br/>Returns double values
    ///</summary>
    public static void CalculateLongestPerpendicularsFromSegment(double[,] dblCoordinates, double dblSegmentStartX, double dblSegmentStartY, double dblSegmentEndX, double dblSegmentEndY,
      out double dblLengthClockwise, out double dblLengthCounterClockwise, out double dblFarCW_X, out double dblFarCW_Y, out double dblFarCCW_X, out double dblFarCCW_Y)
    {
      double dblFarthestClockwise = 0;
      double dblFarthestCounterClockwise = 0;
      double dblTestDist;
      double dblTestX;
      double dblTestY;

      dblFarCW_X = double.NaN;
      dblFarCW_Y = double.NaN;
      dblFarCCW_X = double.NaN;
      dblFarCCW_Y = double.NaN;

      for (int i = 0; i < dblCoordinates.GetLength(0); i++)
      {
        dblTestX = dblCoordinates[i, 0];
        dblTestY = dblCoordinates[i, 1];
        dblTestDist = DistancePointToInfiniteLine(dblSegmentStartX, dblSegmentStartY, dblSegmentEndX, dblSegmentEndY, dblTestX, dblTestY, out JenClockwiseConstants lngClockwise);
        switch (lngClockwise)
        {
          case JenClockwiseConstants.ENUM_CounterClockwise:
            if (dblTestDist >= dblFarthestCounterClockwise)
            {
              dblFarthestCounterClockwise = dblTestDist;
              dblFarCCW_X = dblTestX;
              dblFarCCW_Y = dblTestY;
            }
            break;
          case JenClockwiseConstants.ENUM_OnLine:
            break;
          case JenClockwiseConstants.ENUM_Clockwise:
            if (dblTestDist >= dblFarthestClockwise)
            {
              dblFarthestClockwise = dblTestDist;
              dblFarCW_X = dblTestX;
              dblFarCW_Y = dblTestY;
            }
            break;
          default:
            break;
        }
      }
      dblLengthClockwise = dblFarthestClockwise;
      dblLengthCounterClockwise = dblFarthestCounterClockwise;

      //      double[,] dblMultipointCoords = new double[,]
      //      {
      //        {447137.2725194690000, 3901774.294874670000},
      //        {447157.1017295120000, 3901835.432029960000},
      //        {447185.9528034930000, 3901778.788986210000},
      //        {447144.1061216590000, 3901792.104619740000},
      //        {447188.9288562540000, 3901824.872567650000},
      //        {447150.3193074460000, 3901791.129313710000},
      //        {447169.7059267760000, 3901784.546961780000},
      //        {447181.0385280850000, 3901774.268196820000},
      //        {447189.7091859580000, 3901807.420933250000},
      //        {447142.8403216600000, 3901801.750150920000},
      //        {447081.8774765730000, 3901779.304418560000},
      //        {447085.9242731330000, 3901799.614604710000},
      //        {447091.7570632700000, 3901778.302276130000},
      //        {447143.9567643400000, 3901814.555283780000},
      //        {447116.0953825710000, 3901778.408036230000},
      //        {447171.4147239920000, 3901796.263960600000},
      //        {447178.9347928760000, 3901790.597651000000},
      //        {447178.4293776750000, 3901817.274204490000},
      //        {447165.2063244580000, 3901811.423244480000},
      //        {447133.1299978490000, 3901820.305131670000},
      //        {447131.8234294650000, 3901789.106862550000}
      //      };
      //      CalculateLongestPerpendicularsFromSegment(dblMultipointCoords, 447050, 3901749, 447215, 3901830, out double dblLengthCW, out double dblLengthCCW, out double dblFarCW_X, out double dblFarCW_Y, out double dblFarCCW_X, out double dblFarCCW_Y);
      //      CalculateLongestPerpendicularsFromSegment(dblMultipointCoords,  new double[]{ 447050, 3901749},  new double[] { 447215, 3901830}, out double dblLengthCW, out double dblLengthCCW, out double dblFarCW_X, out double dblFarCW_Y, out double dblFarCCW_X, out double dblFarCCW_Y);
      //      CalculateLongestPerpendicularsFromSegment(dblMultipointCoords, new double[][,] { new double[,] { { 447050, 3901749 }, { 447215, 3901830 } } }, out double dblLengthCW, out double dblLengthCCW, out double dblFarCW_X, out double dblFarCW_Y, out double dblFarCCW_X, out double dblFarCCW_Y);
      //      Console.WriteLine("Farthest Point CW: [" + dblFarCW_X.ToString("0.000") + ", " + dblFarCW_Y.ToString("0.000") + "]; Distance = " + dblLengthCW.ToString("0.000"));
      //      Console.WriteLine("Farthest Point CCW: [" + dblFarCCW_X.ToString("0.000") + ", " + dblFarCCW_Y.ToString("0.000") + "]; Distance = " + dblLengthCCW.ToString("0.000"));
      //      // Farthest Point CW:[447181.039, 3901774.268]; Distance = 35.063
      //      // Farthest Point CCW:[447157.102, 3901835.432]; Distance = 30.390
    }

    ///<summary>
    /// Given 2 consecutive points defining a line segment, this scripts calculates whether the third point lies to the right<br/>
    /// (clockwise) or to the left (counter-clockwise) of the segment, the distance from the point to the segment, the position of <br/>
    /// that point on the segment, the distance to the infinite line defined by the segment, and whether that point is perpendicular<br/>
    /// to the segment.
    ///<br/><br/>Returns double values and JenClockwiseConstants
    ///</summary>
    public static double DistancePointToSegment(double dblSegmentStartX, double dblSegmentStartY, double dblSegmentEndX, double dblSegmentEndY,
      double dblPointX, double dblPointY, out JenClockwiseConstants lngClockwise, out double dblX_On_Segment, out double dblY_On_Segment,
      out double dblDistToInfiniteLine, out double dblProportionAlongLine, out bool booPointIsPerpendicular)
    {
      // DistancePointToInfiniteLine
      // Jenness Enterprises <www.jennessent.com)>
      // WILL CRASH IF SEGMENT START POINT COORDINATES ARE EQUAL TO SEGMENT END POINT COORDINATES
      // Given 2 consecutive points defining a line segment, this scripts calculates whether the third point lies to the right
      // (clockwise) or to the left (counter-clockwise) of the segment, the distance from the point to the segment, the position of 
      // that point on the segment, the distance to the infinite line defined by the segment, and whether that point is perpendicular
      // to the segment.

      // ASSUMES COORDINATES ARE PROJECTED!!!

      if ((dblSegmentStartX == dblSegmentEndX) && (dblSegmentStartY == dblSegmentEndY))
      {
        lngClockwise = JenClockwiseConstants.ENUM_OnLine;
        dblX_On_Segment = double.NaN;
        dblY_On_Segment = double.NaN;
        dblDistToInfiniteLine = double.NaN;
        dblProportionAlongLine = double.NaN;
        booPointIsPerpendicular = false;
        return Double.NaN;
      }
      else
      {
        double dblNumerator = ((dblPointX - dblSegmentStartX) * (dblSegmentEndX - dblSegmentStartX)) + ((dblPointY - dblSegmentStartY) * (dblSegmentEndY - dblSegmentStartY));
        double dblDenom = ((dblSegmentEndX - dblSegmentStartX) * (dblSegmentEndX - dblSegmentStartX)) + ((dblSegmentEndY - dblSegmentStartY) * (dblSegmentEndY - dblSegmentStartY));
        dblProportionAlongLine = dblNumerator / dblDenom;
        dblX_On_Segment = dblSegmentStartX + (dblProportionAlongLine * (dblSegmentEndX - dblSegmentStartX));
        dblY_On_Segment = dblSegmentStartY + (dblProportionAlongLine * (dblSegmentEndY - dblSegmentStartY));
        double dblS = (((dblSegmentStartY - dblPointY) * (dblSegmentEndX - dblSegmentStartX)) - ((dblSegmentStartX - dblPointX) * (dblSegmentEndY - dblSegmentStartY))) / dblDenom;

        if (dblS < 0) { lngClockwise = JenClockwiseConstants.ENUM_CounterClockwise; }
        else if (dblS == 0) { lngClockwise = JenClockwiseConstants.ENUM_OnLine; }
        else { lngClockwise = JenClockwiseConstants.ENUM_Clockwise; }

        dblDistToInfiniteLine = Math.Abs(dblS) * Math.Sqrt(dblDenom);

        //dblDistToInfiniteLine = (((dblSegmentEndX - dblSegmentStartX) * (dblSegmentStartY - dblPointY)) -
        //           ((dblSegmentStartX - dblPointX) * (dblSegmentEndY - dblSegmentStartY))) /
        //           (Math.Pow(Math.Pow((dblSegmentEndX - dblSegmentStartX), 2) + Math.Pow((dblSegmentEndY - dblSegmentStartY), 2), 0.5));

        double dblDistance;
        if (dblProportionAlongLine >= 0 && dblProportionAlongLine <= 1)
        {
          booPointIsPerpendicular = true;
          dblDistance = dblDistToInfiniteLine;
        }
        else
        {
          booPointIsPerpendicular = false;
          double dblDistToStart = ((dblPointX - dblSegmentStartX) * (dblPointX - dblSegmentStartX)) + ((dblPointY - dblSegmentStartY) * (dblPointY - dblSegmentStartY));
          double dblDistToEnd = ((dblPointX - dblSegmentEndX) * (dblPointX - dblSegmentEndX)) + ((dblPointY - dblSegmentEndY) * (dblPointY - dblSegmentEndY));
          dblDistance = dblDistToStart < dblDistToEnd ? Math.Sqrt(dblDistToStart) : Math.Sqrt(dblDistToEnd);
        }

        return Math.Abs(dblDistance);

        //double dblSegmentStartX = 112;
        //double dblSegmentStartY = 334;
        //double dblSegmentEndX = -234;
        //double dblSegmentEndY = 667;
        //double dblPointX = 30;
        //double dblPointY = 60;
        //double dblX_on_Segment;
        //double dblY_on_Segment;
        //double dblDistToInfiniteLine;
        //double dblProportion;
        //bool booPerpendicular;
        //JenClockwiseConstants lngClockwise;

        //double dblDistance = DistancePointToSegment(dblSegmentStartX, dblSegmentStartY, dblSegmentEndX, dblSegmentEndY, dblPointX, dblPointY, out lngClockwise,
        //  out dblX_on_Segment, out dblY_on_Segment, out dblDistToInfiniteLine, out dblProportion, out booPerpendicular);
        //Console.WriteLine("---  DistancePointToSegment 1  ------------------------");
        //Console.WriteLine(dblDistance.ToString("0.0000000") + ":  [" + lngClockwise + "]");
        //Console.WriteLine("Distance to Infinite Line = " + dblDistToInfiniteLine.ToString("0.0000000"));
        //Console.WriteLine("Proportion on Segment = " + dblProportion.ToString("0.0000000"));
        //Console.WriteLine("Projected Coordinates = [" + dblX_on_Segment.ToString("0.0000000") + ", " + dblY_on_Segment.ToString("0.0000000") + "]");
        //Console.WriteLine("Perpendicular to Segment = " + booPerpendicular.ToString());

        //dblDistance = DistancePointToSegment(new double[] { dblSegmentStartX, dblSegmentStartY }, new double[] { dblSegmentEndX, dblSegmentEndY }, dblPointX, dblPointY, out lngClockwise,
        //  out dblX_on_Segment, out dblY_on_Segment, out dblDistToInfiniteLine, out dblProportion, out booPerpendicular);
        //Console.WriteLine("---  DistancePointToSegment 2  ------------------------");
        //Console.WriteLine(dblDistance.ToString("0.0000000") + ":  [" + lngClockwise + "]");
        //Console.WriteLine("Distance to Infinite Line = " + dblDistToInfiniteLine.ToString("0.0000000"));
        //Console.WriteLine("Proportion on Segment = " + dblProportion.ToString("0.0000000"));
        //Console.WriteLine("Projected Coordinates = [" + dblX_on_Segment.ToString("0.0000000") + ", " + dblY_on_Segment.ToString("0.0000000") + "]");
        //Console.WriteLine("Perpendicular to Segment = " + booPerpendicular.ToString());

        //dblDistance = DistancePointToSegment(new double[][,] { new double[,] { { dblSegmentStartX, dblSegmentStartY }, { dblSegmentEndX, dblSegmentEndY } } }, dblPointX, dblPointY, out lngClockwise,
        //  out dblX_on_Segment, out dblY_on_Segment, out dblDistToInfiniteLine, out dblProportion, out booPerpendicular);
        //Console.WriteLine("---  DistancePointToSegment 3  ------------------------");
        //Console.WriteLine(dblDistance.ToString("0.0000000") + ":  [" + lngClockwise + "]");
        //Console.WriteLine("Distance to Infinite Line = " + dblDistToInfiniteLine.ToString("0.0000000"));
        //Console.WriteLine("Proportion on Segment = " + dblProportion.ToString("0.0000000"));
        //Console.WriteLine("Projected Coordinates = [" + dblX_on_Segment.ToString("0.0000000") + ", " + dblY_on_Segment.ToString("0.0000000") + "]");
        //Console.WriteLine("Perpendicular to Segment = " + booPerpendicular.ToString());

        //dblDistance = DistancePointToSegment(new double[][,] { new double[,] { { dblSegmentStartX, dblSegmentStartY }, { dblSegmentEndX, dblSegmentEndY } } }, new double[] { dblPointX, dblPointY }, out lngClockwise,
        //  out dblX_on_Segment, out dblY_on_Segment, out dblDistToInfiniteLine, out dblProportion, out booPerpendicular);
        //Console.WriteLine("---  DistancePointToSegment 4  ------------------------");
        //Console.WriteLine(dblDistance.ToString("0.0000000") + ":  [" + lngClockwise + "]");
        //Console.WriteLine("Distance to Infinite Line = " + dblDistToInfiniteLine.ToString("0.0000000"));
        //Console.WriteLine("Proportion on Segment = " + dblProportion.ToString("0.0000000"));
        //Console.WriteLine("Projected Coordinates = [" + dblX_on_Segment.ToString("0.0000000") + ", " + dblY_on_Segment.ToString("0.0000000") + "]");
        //Console.WriteLine("Perpendicular to Segment = " + booPerpendicular.ToString());
      }
    }
    ///<summary>
    /// Given 2 consecutive points defining a line segment, this scripts calculates whether the third point lies to the right<br/>
    /// (clockwise) or to the left (counter-clockwise) of the segment, the distance from the point to the segment, the position of <br/>
    /// that point on the segment, the distance to the infinite line defined by the segment, and whether that point is perpendicular<br/>
    /// to the segment.
    ///<br/><br/>Returns double values and JenClockwiseConstants
    ///</summary>
    public static double DistancePointToSegment(double[] dblSegmentStart, double[] dblSegmentEnd,
      double dblPointX, double dblPointY, out JenClockwiseConstants lngClockwise, out double dblX_On_Segment, out double dblY_On_Segment,
      out double dblDistToInfiniteLine, out double dblProportionAlongLine, out bool booPointIsPerpendicular)
    {
      double dblDistance = DistancePointToSegment(dblSegmentStart[0], dblSegmentStart[1], dblSegmentEnd[0], dblSegmentEnd[1], dblPointX, dblPointY, out lngClockwise,
          out dblX_On_Segment, out dblY_On_Segment, out dblDistToInfiniteLine, out dblProportionAlongLine, out booPointIsPerpendicular);

      return Math.Abs(dblDistance);
    }
    ///<summary>
    /// Given 2 consecutive points defining a line segment, this scripts calculates whether the third point lies to the right<br/>
    /// (clockwise) or to the left (counter-clockwise) of the segment, the distance from the point to the segment, the position of <br/>
    /// that point on the segment, the distance to the infinite line defined by the segment, and whether that point is perpendicular<br/>
    /// to the segment.
    ///<br/><br/>Returns double values and JenClockwiseConstants
    ///</summary>
    public static double DistancePointToSegment(double[][,] dblSegmentPolyline,
      double dblPointX, double dblPointY, out JenClockwiseConstants lngClockwise, out double dblX_On_Segment, out double dblY_On_Segment,
      out double dblDistToInfiniteLine, out double dblProportionAlongLine, out bool booPointIsPerpendicular)
    {
      // DistancePointToInfiniteLine
      // Jenness Enterprises <www.jennessent.com)>
      // WILL CRASH IF SEGMENT START POINT COORDINATES ARE EQUAL TO SEGMENT END POINT COORDINATES
      // Given 2 consecutive points defining a line segment, this scripts calculates whether the third point lies to the right
      // (clockwise) or to the left (counter-clockwise) of the segment, the distance from the point to the segment, the position of 
      // that point on the segment, the distance to the infinite line defined by the segment, and whether that point is perpendicular
      // to the segment.

      // ASSUMES COORDINATES ARE PROJECTED!!!
      //double dblSegmentStartX = dblSegmentPolyline[0][0, 0];
      //double dblSegmentStartY = dblSegmentPolyline[0][0, 1];
      //double dblSegmentEndX = dblSegmentPolyline[0][1, 0];
      //double dblSegmentEndY = dblSegmentPolyline[0][1, 1];

      long lngMaxPolylineIndex = dblSegmentPolyline.GetLength(0) - 1;
      long lngMaxVertexIndex = dblSegmentPolyline[lngMaxPolylineIndex].GetLength(0) - 1;
      double dblDistance = DistancePointToSegment(dblSegmentPolyline[0][0, 0], dblSegmentPolyline[0][0, 1], dblSegmentPolyline[lngMaxPolylineIndex][lngMaxVertexIndex, 0], dblSegmentPolyline[lngMaxPolylineIndex][lngMaxVertexIndex, 1], dblPointX, dblPointY, out lngClockwise, out dblX_On_Segment, out dblY_On_Segment, out dblDistToInfiniteLine, out dblProportionAlongLine, out booPointIsPerpendicular);

      return Math.Abs(dblDistance);
    }
    ///<summary>
    /// Using segment connecting starting and ending coordinates of a given polyline, this scripts calculates whether the third point lies to the right<br/>
    /// (clockwise) or to the left (counter-clockwise) of the segment, the distance from the point to the segment, the position of <br/>
    /// that point on the segment, the distance to the infinite line defined by the segment, and whether that point is perpendicular<br/>
    /// to the segment.
    ///<br/><br/>Returns double values and JenClockwiseConstants
    ///</summary>
    public static double DistancePointToSegment(double[][,] dblSegmentPolyline,
      double[] dblPoint, out JenClockwiseConstants lngClockwise, out double dblX_On_Segment, out double dblY_On_Segment,
      out double dblDistToInfiniteLine, out double dblProportionAlongLine, out bool booPointIsPerpendicular)
    {
      // DistancePointToInfiniteLine
      // Jenness Enterprises <www.jennessent.com)>
      // WILL CRASH IF SEGMENT START POINT COORDINATES ARE EQUAL TO SEGMENT END POINT COORDINATES
      // Given 2 consecutive points defining a line segment, this scripts calculates whether the third point lies to the right
      // (clockwise) or to the left (counter-clockwise) of the segment, the distance from the point to the segment, the position of 
      // that point on the segment, the distance to the infinite line defined by the segment, and whether that point is perpendicular
      // to the segment.

      // ASSUMES COORDINATES ARE PROJECTED!!!
      long lngMaxPolylineIndex = dblSegmentPolyline.GetLength(0) - 1;
      long lngMaxVertexIndex = dblSegmentPolyline[lngMaxPolylineIndex].GetLength(0) - 1;
      double dblDistance = DistancePointToSegment(dblSegmentPolyline[0][0, 0], dblSegmentPolyline[0][0, 1], dblSegmentPolyline[lngMaxPolylineIndex][lngMaxVertexIndex, 0], dblSegmentPolyline[lngMaxPolylineIndex][lngMaxVertexIndex, 1], dblPoint[0], dblPoint[1], out lngClockwise, out dblX_On_Segment, out dblY_On_Segment, out dblDistToInfiniteLine, out dblProportionAlongLine, out booPointIsPerpendicular);

      return Math.Abs(dblDistance);
    }

    ///<summary>
    ///Given segment starting and ending coordinates, plus a query point, returns distance from that point to an infinite line defined by segment<br/>
    ///Also returns whether that point was clockwise, counter-clockwise, or on the line of that infinite line.
    ///<br/><br/>Returns double value and JenClockwiseConstants
    ///</summary>
    public static double DistancePointToInfiniteLine(double dblSegmentStartX, double dblSegmentStartY, double dblSegmentEndX, double dblSegmentEndY,
      double dblPointX, double dblPointY, out JenClockwiseConstants lngClockwise)
    {
      // DistancePointToInfiniteLine
      // Jenness Enterprises <www.jennessent.com)>
      // WILL CRASH IF SEGMENT START POINT COORDINATES ARE EQUAL TO SEGMENT END POINT COORDINATES
      // Given 2 consecutive points defining a line with direction, this scripts calculates whether the third point lies to the right
      // (clockwise) or to the left (counter-clockwise) of the line connecting the first point to the second point, and the distance
      // from the point to the line.

      // ASSUMES COORDINATES ARE PROJECTED!!!

      if ((dblSegmentStartX == dblSegmentEndX) && (dblSegmentStartY == dblSegmentEndY))
      {
        lngClockwise = JenClockwiseConstants.ENUM_OnLine;
        return Double.NaN;
      }
      else
      {
        double dblDistance = (((dblSegmentEndX - dblSegmentStartX) * (dblSegmentStartY - dblPointY)) -
                   ((dblSegmentStartX - dblPointX) * (dblSegmentEndY - dblSegmentStartY))) /
                   (Math.Pow(Math.Pow((dblSegmentEndX - dblSegmentStartX), 2) + Math.Pow((dblSegmentEndY - dblSegmentStartY), 2), 0.5));

        if (dblDistance < 0) { lngClockwise = JenClockwiseConstants.ENUM_CounterClockwise; }
        else if (dblDistance == 0) { lngClockwise = JenClockwiseConstants.ENUM_OnLine; }
        else { lngClockwise = JenClockwiseConstants.ENUM_Clockwise; }

        return Math.Abs(dblDistance);

        //double dblSegmentStartX = 112;
        //double dblSegmentStartY = 334;
        //double dblSegmentEndX = -234;
        //double dblSegmentEndY = 667;
        //double dblPointX = 30;
        //double dblPointY = 60;
        //JenClockwiseConstants lngClockwise;

        //double dblDistance = DistancePointToInfiniteLine(dblSegmentStartX, dblSegmentStartY, dblSegmentEndX, dblSegmentEndY, dblPointX, dblPointY, out lngClockwise);
        //Console.WriteLine("---  DistancePointToSegment 1  ------------------------");
        //Console.WriteLine(dblDistance.ToString("0.0000000") + ":  [" + lngClockwise + "]");

        //dblDistance = DistancePointToInfiniteLine(new double[] { dblSegmentStartX, dblSegmentStartY }, new double[] { dblSegmentEndX, dblSegmentEndY }, dblPointX, dblPointY, out lngClockwise);
        //Console.WriteLine("---  DistancePointToSegment 2  ------------------------");
        //Console.WriteLine(dblDistance.ToString("0.0000000") + ":  [" + lngClockwise + "]");

        //dblDistance = DistancePointToInfiniteLine(new double[][,] { new double[,] { { dblSegmentStartX, dblSegmentStartY }, { dblSegmentEndX, dblSegmentEndY } } }, dblPointX, dblPointY, out lngClockwise);
        //Console.WriteLine("---  DistancePointToSegment 3  ------------------------");
        //Console.WriteLine(dblDistance.ToString("0.0000000") + ":  [" + lngClockwise + "]");

        //dblDistance = DistancePointToInfiniteLine(new double[][,] { new double[,] { { dblSegmentStartX, dblSegmentStartY }, { dblSegmentEndX, dblSegmentEndY } } }, new double[] { dblPointX, dblPointY }, out lngClockwise);
        //Console.WriteLine("---  DistancePointToSegment 4  ------------------------");
        //Console.WriteLine(dblDistance.ToString("0.0000000") + ":  [" + lngClockwise + "]");
        ////---DistancePointToSegment 1------------------------
        ////254.2827336:  [ENUM_CounterClockwise]
        ////---DistancePointToSegment 2------------------------
        ////254.2827336:  [ENUM_CounterClockwise]
        ////---DistancePointToSegment 3------------------------
        ////254.2827336:  [ENUM_CounterClockwise]
        ////---DistancePointToSegment 4------------------------
        ////254.2827336:  [ENUM_CounterClockwise]
      }
    }
    ///<summary>
    ///Given segment starting and ending coordinates, plus a query point, returns distance from that point to an infinite line defined by segment<br/>
    ///Also returns whether that point was clockwise, counter-clockwise, or on the line of that infinite line.
    ///<br/><br/>Returns double value and JenClockwiseConstants
    ///</summary>
    public static double DistancePointToInfiniteLine(double[] dblSegmentStart, double[] dblSegmentEnd,
      double dblPointX, double dblPointY, out JenClockwiseConstants lngClockwise)
    {
      double dblDistance = DistancePointToInfiniteLine(dblSegmentStart[0], dblSegmentStart[1], dblSegmentEnd[0], dblSegmentEnd[1], dblPointX, dblPointY, out lngClockwise);

      return Math.Abs(dblDistance);
    }
    ///<summary>
    ///Using segment connecting starting and ending coordinates of a given polyline, plus a given query point, returns distance from that point to an infinite line defined by segment<br/>
    ///Also returns whether that point was clockwise, counter-clockwise, or on the line of that infinite line.
    ///<br/><br/>Returns double value and JenClockwiseConstants
    ///</summary>
    public static double DistancePointToInfiniteLine(double[][,] dblSegmentPolyline,
      double dblPointX, double dblPointY, out JenClockwiseConstants lngClockwise)
    {
      // ASSUMES COORDINATES ARE PROJECTED!!!
      long lngMaxPolylineIndex = dblSegmentPolyline.GetLength(0) - 1;
      long lngMaxVertexIndex = dblSegmentPolyline[lngMaxPolylineIndex].GetLength(0) - 1;
      double dblDistance = DistancePointToInfiniteLine(dblSegmentPolyline[0][0, 0], dblSegmentPolyline[0][0, 1], dblSegmentPolyline[lngMaxPolylineIndex][lngMaxVertexIndex, 0], dblSegmentPolyline[lngMaxPolylineIndex][lngMaxVertexIndex, 1], dblPointX, dblPointY, out lngClockwise);

      return Math.Abs(dblDistance);
    }
    ///<summary>
    ///Given segment starting and ending coordinates, plus a query point, returns distance from that point to an infinite line defined by segment<br/>
    ///Also returns whether that point was clockwise, counter-clockwise, or on the line of that infinite line.
    ///<br/><br/>Returns double value and JenClockwiseConstants
    ///</summary>
    public static double DistancePointToInfiniteLine(double[][,] dblSegmentPolyline,
      double[] dblPoint, out JenClockwiseConstants lngClockwise)
    {
      // DistancePointToInfiniteLine
      // Jenness Enterprises <www.jennessent.com)>
      // WILL CRASH IF SEGMENT START POINT COORDINATES ARE EQUAL TO SEGMENT END POINT COORDINATES
      // Given 2 consecutive points defining a line with direction, this scripts calculates whether the third point lies to the right
      // (clockwise) or to the left (counter-clockwise) of the line connecting the first point to the second point, and the distance
      // from the point to the line.

      // ASSUMES COORDINATES ARE PROJECTED!!!
      long lngMaxPolylineIndex = dblSegmentPolyline.GetLength(0) - 1;
      long lngMaxVertexIndex = dblSegmentPolyline[lngMaxPolylineIndex].GetLength(0) - 1;
      double dblDistance = DistancePointToInfiniteLine(dblSegmentPolyline[0][0, 0], dblSegmentPolyline[0][0, 1], dblSegmentPolyline[lngMaxPolylineIndex][lngMaxVertexIndex, 0], dblSegmentPolyline[lngMaxPolylineIndex][lngMaxVertexIndex, 1], dblPoint[0], dblPoint[1], out lngClockwise);

      return Math.Abs(dblDistance);
    }

    ///<summary>
    ///Given a value in DMS, returns value for DD<br/>
    ///<br/><br/>Returns double value for DD
    ///</summary>
    public static double ConvertDMStoDD(long lngDegrees, long lngMinutes, double dblSeconds)
    {
      if (lngDegrees < 0) { return (double)lngDegrees - ((double)lngMinutes / 60d) - (dblSeconds / 3600d); }
      else { return (double)lngDegrees + ((double)lngMinutes / 60d) + (dblSeconds / 3600d); }
      //long lngMinutes;
      //long lngDegrees;
      //double dblSeconds;
      //double dblDD = -123.456789;
      //Random r = new Random();
      //dblDD = (r.NextDouble() * 360d) - 180d;
      //ConvertDDtoDMS(dblDD, out lngDegrees, out lngMinutes, out dblSeconds);
      //Console.WriteLine(dblDD.ToString("0.0000000") + " Degrees:  [" + lngDegrees.ToString("0.000") + " degrees, " + lngMinutes.ToString("0.000") + " minutes, " +
      //  dblSeconds.ToString("0.0000000") + " seconds]");

      //dblDD = ConvertDMStoDD(lngDegrees, lngMinutes, dblSeconds);
      //Console.WriteLine("[" + lngDegrees.ToString("0.000") + " degrees, " + lngMinutes.ToString("0.000") + " minutes, " +
      //  dblSeconds.ToString("0.0000000") + " seconds] --> " + dblDD.ToString("0.0000000") + " Degrees");
    }
    ///<summary>
    ///Given a value in decimal degrees, returns values for degrees, minutes, seconds<br/>
    ///<br/><br/>Returns long values for degrees and minutes, and double value for seconds
    ///</summary>
    public static void ConvertDDtoDMS(double dblDD, out long lngDegrees, out long lngMinutes, out double dblSeconds)
    {
      lngDegrees = (long)dblDD;
      lngMinutes = (long)((Math.Abs(dblDD - (double)lngDegrees)) * 60L);
      dblSeconds = ((Math.Abs(dblDD - (double)lngDegrees) * 60d) - (double)lngMinutes) * 60d;
      //long lngMinutes;
      //long lngDegrees;
      //double dblSeconds;
      //double dblDD = -123.456789;
      //Random r = new Random();
      //dblDD = (r.NextDouble() * 360d) - 180d;
      //ConvertDDtoDMS(dblDD, out lngDegrees, out lngMinutes, out dblSeconds);
      //Console.WriteLine(dblDD.ToString("0.0000000") + " Degrees:  [" + lngDegrees.ToString("0.000") + " degrees, " + lngMinutes.ToString("0.000") + " minutes, " +
      //  dblSeconds.ToString("0.0000000") + " seconds]");

      //dblDD = ConvertDMStoDD(lngDegrees, lngMinutes, dblSeconds);
      //Console.WriteLine("[" + lngDegrees.ToString("0.000") + " degrees, " + lngMinutes.ToString("0.000") + " minutes, " +
      //  dblSeconds.ToString("0.0000000") + " seconds] --> " + dblDD.ToString("0.0000000") + " Degrees");
    }

    ///<summary>
    ///Given a value from a circular variable, plus the minimum and maximum values, returns a value forcing the given between the specified minimum and maximum.<br/>
    ///<br/><br/>Returns double value.
    ///</summary>
    public static double ForceValueToCorrectRange(double dblValue, double dblMin = 0, double dblMax = 360, bool booMakeMaximumEqualMinimum = true)
    {
      double dblReturn = dblValue - dblMin;
      dblReturn %= (dblMax - dblMin);
      dblReturn += dblMin;
      dblReturn = dblReturn < dblMin ? dblReturn + dblMax - dblMin : dblReturn;
      
      if (booMakeMaximumEqualMinimum && dblReturn == dblMax) { dblReturn = dblMin; }
      
      return dblReturn;

      //Console.WriteLine("12345678 --> " + ForceValueToCorrectRange(12345678, 360, 720));
      //Console.WriteLine("1000 --> " + ForceValueToCorrectRange(1000, 360, 720));
      //Console.WriteLine("500 --> " + ForceValueToCorrectRange(500, 360, 720));
      //Console.WriteLine("400 --> " + ForceValueToCorrectRange(400, 360, 720));
      //Console.WriteLine("300 --> " + ForceValueToCorrectRange(300, 360, 720));
      //Console.WriteLine("100 --> " + ForceValueToCorrectRange(100, 360, 720));
      //Console.WriteLine("-1000 --> " + ForceValueToCorrectRange(-1000, 360, 720));
      //Console.WriteLine("-500 --> " + ForceValueToCorrectRange(-500, 360, 720));
      //Console.WriteLine("-400 --> " + ForceValueToCorrectRange(-400, 360, 720));
      //Console.WriteLine("-300 --> " + ForceValueToCorrectRange(-300, 360, 720));
      //Console.WriteLine("-100 --> " + ForceValueToCorrectRange(-100, 360, 720));
      //Console.WriteLine("-12345678.90123 --> " + ForceValueToCorrectRange(-12345678.90123, 360, 720));
    }
    ///<summary>
    ///Given a compass bearing, returns a value forcing the bearing between 0 and 360.<br/>
    ///<br/><br/>Returns double value.
    ///</summary>
    public static double ForceAzimuthToCorrectRange(double dblAzimuth)
    {
      dblAzimuth %= 360;
      return dblAzimuth < 0 ? dblAzimuth + 360 : dblAzimuth;

      //Console.WriteLine("1000 --> " + ForceAzimuthToCorrectRange(1000));
      //Console.WriteLine("500 --> " + ForceAzimuthToCorrectRange(500));
      //Console.WriteLine("400 --> " + ForceAzimuthToCorrectRange(400));
      //Console.WriteLine("300 --> " + ForceAzimuthToCorrectRange(300));
      //Console.WriteLine("100 --> " + ForceAzimuthToCorrectRange(100));
      //Console.WriteLine("-1000 --> " + ForceAzimuthToCorrectRange(-1000));
      //Console.WriteLine("-500 --> " + ForceAzimuthToCorrectRange(-500));
      //Console.WriteLine("-400 --> " + ForceAzimuthToCorrectRange(-400));
      //Console.WriteLine("-300 --> " + ForceAzimuthToCorrectRange(-300));
      //Console.WriteLine("-100 --> " + ForceAzimuthToCorrectRange(-100));
    }

    ///<summary>
    ///Given a 2D double array of compass bearings and weights, returns the mean direction as well as several statistics describing central tendency and dispersion<br/>
    ///<br/><br/>Returns doubles for all outputs.
    ///</summary>
    public static double ReturnWeightedMeanDir(double[,] dblCompassDirsAndWeights, out double dblResultantLength, out double dblMeanResultantLength, out double dblCircularVariance,
      out double dblAngularVariance, out double dblCircularStandDev, out double dblAngularDeviation, out double dblKappa)
    {
      double dblSumC = 0;
      double dblSumS = 0;
      double dblRadians;
      double dblWeight;
      double dblSumWeights = 0;

      for (int i = 0; i < dblCompassDirsAndWeights.GetLength(0); i++)
      {
        dblWeight = dblCompassDirsAndWeights[i, 1];
        dblSumWeights += dblWeight;
        dblRadians = DegToRad(dblCompassDirsAndWeights[i, 0]);
        dblSumC += Math.Cos(dblRadians) * dblWeight;
        dblSumS += Math.Sin(dblRadians) * dblWeight;
      }
      dblResultantLength = Math.Sqrt(Math.Pow(dblSumC, 2) + Math.Pow(dblSumS, 2));
      dblMeanResultantLength = dblResultantLength / dblSumWeights;

      dblCircularVariance = 1 - dblMeanResultantLength;
      dblAngularVariance = 2 * dblCircularVariance;
      dblCircularStandDev = Math.Sqrt(-2 * (Math.Log(Math.Min(1d, dblMeanResultantLength))));
      dblAngularDeviation = Math.Sqrt(dblAngularVariance);
      dblKappa = ReturnVonMisesKappa(dblMeanResultantLength, dblCompassDirsAndWeights.GetLength(0), true);
      double dblMeanDir = ForceAzimuthToCorrectRange(RadToDeg(Math.Atan2(dblSumS, dblSumC)));
      return dblMeanDir;
      //double[,] dblBearingsAndWeights = new double[26, 2] { { 90.39133, 21.95923 }, { -2.23126, 19.46409 }, { 7.24918, 19.26859 }, { -10.57344, 24.17666 }, { 23.66208, 17.60308 }, { 63.63428, 20.22093 }, { -23.26101, 19.12086 }, { 80.95798, 20.73607 }, { 52.27849, 18.58719 }, { -6.37194, 22.76364 }, { 51.64170, 19.34874 }, { 63.32852, 22.71133 }, { 11.20408, 19.38491 }, { 87.92821, 15.97493 }, { 41.60472, 18.19774 }, { -39.24497, 19.67032 }, { -43.03334, 17.84234 }, { 1.27654, 23.67170 }, { 7.26949, 24.60617 }, { -51.91664, 20.17076 }, { 14.15638, 16.57905 }, { 71.77311, 24.57026 }, { 57.26414, 22.74042 }, { -9.65850, 19.26306 }, { 13.62971, 24.47668 }, { 54.85725, 23.55706 } };

      //Console.WriteLine("Mean Direction = " + ReturnWeightedMeanDir(dblBearingsAndWeights, out double dblResultantLength, out double dblMRL, out double dblCircVar,
      //  out double dblAngVar, out double dblCircStDev, out double dblAngStDev, out double dblKappa).ToString("0.00000"));
      //Console.WriteLine("Resultant Length = " + dblResultantLength.ToString("0.00000"));
      //Console.WriteLine("Mean Resultant Length = " + dblMRL.ToString("0.00000"));
      //Console.WriteLine("Circular Variance = " + dblCircVar.ToString("0.00000"));
      //Console.WriteLine("Angular Variance = " + dblAngVar.ToString("0.00000"));
      //Console.WriteLine("Circular Standard Deviation = " + dblCircStDev.ToString("0.00000"));
      //Console.WriteLine("Angular Standard Deviation = " + dblAngStDev.ToString("0.00000"));
      //Console.WriteLine("Kappa = " + dblKappa.ToString("0.00000"));
    }
    ///<summary>
    ///Given a double array of compass bearings, and another double array of weights, returns the mean direction as well as several statistics describing central tendency and dispersion<br/>
    ///<br/><br/>Returns doubles for all outputs.
    ///</summary>
    public static double ReturnWeightedMeanDir(double[] dblCompassDirs, double[] dblWeights, out double dblResultantLength, out double dblMeanResultantLength, out double dblCircularVariance,
      out double dblAngularVariance, out double dblCircularStandDev, out double dblAngularDeviation, out double dblKappa)
    {
      double dblSumC = 0;
      double dblSumS = 0;
      double dblRadians;
      double dblWeight;
      double dblSumWeights = 0;

      for (int i = 0; i < dblCompassDirs.Length; i++)
      {
        dblWeight = dblWeights[i];
        dblSumWeights += dblWeight;
        dblRadians = DegToRad(dblCompassDirs[i]);
        dblSumC += Math.Cos(dblRadians) * dblWeight;
        dblSumS += Math.Sin(dblRadians) * dblWeight;
      }
      dblResultantLength = Math.Sqrt(Math.Pow(dblSumC, 2) + Math.Pow(dblSumS, 2));
      dblMeanResultantLength = dblResultantLength / dblSumWeights;

      dblCircularVariance = 1 - dblMeanResultantLength;
      dblAngularVariance = 2 * dblCircularVariance;
      dblCircularStandDev = Math.Sqrt(-2 * (Math.Log(Math.Min(1d, dblMeanResultantLength))));
      dblAngularDeviation = Math.Sqrt(dblAngularVariance);
      dblKappa = ReturnVonMisesKappa(dblMeanResultantLength, dblCompassDirs.Length, true);
      double dblMeanDir = ForceAzimuthToCorrectRange(RadToDeg(Math.Atan2(dblSumS, dblSumC)));
      return dblMeanDir;

      //double[] dblBearings = new double[16] { 26.57352, -75.83531, 52.28528, 46.16131, 90.52630, -8.96472, -30.99702, 1.15769, -12.97658, 47.71324, 85.60875, 44.40084, 57.03049, -28.52118, -25.35959, 24.51502 };
      //double[] dblWeights = new double[16] { 17.80874, 24.00656, 22.27271, 18.31452, 19.15711, 16.43667, 20.52567, 17.35336, 24.99498, 23.78050, 16.95890, 16.30265, 22.81245, 20.31852, 22.46614, 20.19755 };

      //Console.WriteLine("Mean Direction = " + ReturnWeightedMeanDir(dblBearings, dblWeights, out double dblResultantLength, out double dblMRL, out double dblCircVar,
      //  out double dblAngVar, out double dblCircStDev, out double dblAngStDev, out double dblKappa).ToString("0.00000"));
      //Console.WriteLine("Resultant Length = " + dblResultantLength.ToString("0.00000"));
      //Console.WriteLine("Mean Resultant Length = " + dblMRL.ToString("0.00000"));
      //Console.WriteLine("Circular Variance = " + dblCircVar.ToString("0.00000"));
      //Console.WriteLine("Angular Variance = " + dblAngVar.ToString("0.00000"));
      //Console.WriteLine("Circular Standard Deviation = " + dblCircStDev.ToString("0.00000"));
      //Console.WriteLine("Angular Standard Deviation = " + dblAngStDev.ToString("0.00000"));
      //Console.WriteLine("Kappa = " + dblKappa.ToString("0.00000"));
    }
    ///<summary>
    ///Given a double array of compass bearings, returns the mean direction as well as several statistics describing central tendency and dispersion<br/>
    ///<br/><br/>Returns doubles for all outputs.
    ///</summary>
    public static double ReturnMeanDir(double[] dblCompassDirs, out double dblResultantLength, out double dblMeanResultantLength, out double dblCircularVariance,
      out double dblAngularVariance, out double dblCircularStandDev, out double dblAngularDeviation, out double dblKappa)
    {
      double dblSumC = 0;
      double dblSumS = 0;
      double dblRadians;

      for (int i = 0; i < dblCompassDirs.Length; i++)
      {
        dblRadians = DegToRad(dblCompassDirs[i]);
        dblSumC += Math.Cos(dblRadians);
        dblSumS += Math.Sin(dblRadians);
      }
      dblResultantLength = Math.Sqrt(Math.Pow(dblSumC, 2) + Math.Pow(dblSumS, 2));
      dblMeanResultantLength = dblResultantLength / dblCompassDirs.Length;

      dblCircularVariance = 1 - dblMeanResultantLength;
      dblAngularVariance = 2 * dblCircularVariance;
      dblCircularStandDev = Math.Sqrt(-2 * (Math.Log(Math.Min(1d, dblMeanResultantLength))));
      dblAngularDeviation = Math.Sqrt(dblAngularVariance);
      dblKappa = ReturnVonMisesKappa(dblMeanResultantLength, dblCompassDirs.Length, true);
      double dblMeanDir = RadToDeg(Math.Atan2(dblSumS, dblSumC));
      if (dblMeanDir < 0) { dblMeanDir += 360; }
      return dblMeanDir;
    }
    ///<summary>
    ///Given a Mean Resultant Length and optionally sample size and whether to correct for small sample sizes, returns the Kata statistic calculated on the Von Mises distirbution<br/>
    ///<br/><br/>Returns double
    ///</summary>
    public static double ReturnVonMisesKappa(double dblMeanResultantLength, long lngPointCount, bool booCorrectIfSmallSample)
    {
      //  VON MISES DISPERSION:  KAPPA
      //  FROM FISHER, P. 88
      double dblKappa;
      if (dblMeanResultantLength < 0.53)
      {
        dblKappa = (2 * dblMeanResultantLength) + Math.Pow(dblMeanResultantLength, 3) + (5 * Math.Pow(dblMeanResultantLength, 5) / 6);
      }
      else if (dblMeanResultantLength < 0.85)
      {
        dblKappa = -0.4 + (1.39 * dblMeanResultantLength) + (0.43 / (1 - dblMeanResultantLength));
      }
      else
      {
        if ((Math.Pow(dblMeanResultantLength, 3) - (4 * Math.Pow(dblMeanResultantLength, 2)) + (3 * dblMeanResultantLength)) == 0) { dblKappa = 1 / 0.000000001; }
        else { dblKappa = 1 / (Math.Pow(dblMeanResultantLength, 3) - (4 * Math.Pow(dblMeanResultantLength, 2)) + (3 * dblMeanResultantLength)); }
      }

      // ADJUST KAPPA FOR SMALL SAMPLE SIZES
      if (lngPointCount <= 15 && booCorrectIfSmallSample)
      {
        if (dblKappa < 2)
        {
          double dblTemp = dblKappa - (2 / (lngPointCount * dblKappa));
          dblKappa = dblTemp < 0 ? 0 : dblTemp;
        }
        else { dblKappa = Math.Pow((lngPointCount - 1), 3) * dblKappa / (Math.Pow(lngPointCount, 3) + lngPointCount); }
      }
      return dblKappa;
    }

    ///<summary>
    ///Given a center point and two radii, with options for spherical, spheroidal and trigonometric, returns a jagged double array polyline with 4 segments extending from center<br/>
    ///If method is trigonmetric, then radii are in coordinate system units.  If spherical or spheroidal, then radii are in meters.
    ///<br/><br/>Returns jagged double array
    ///</summary>
    public static double[][,] CreateCrossAroundPoint(double[] dblPoint, double dblVerticalHalfLength, double dblHorizontalHalfLength,
      JenSphericalMethod jenMethod, double dblSemiMajorAxis = 6378137.000, double dblSemiMinorAxis = 6356752.31424518, double dblSphereRadius = 6371000.79000915)
    {
      double dblOriginX = dblPoint[0];
      double dblOriginY = dblPoint[1];
      return CreateCrossAroundPoint(dblOriginX, dblOriginY, dblVerticalHalfLength, dblHorizontalHalfLength, jenMethod, dblSemiMajorAxis, dblSemiMinorAxis, dblSphereRadius);

    }
    ///<summary>
    ///Given a center point and two radii, with options for spherical, spheroidal and trigonometric, returns a jagged double array polyline with 4 segments extending from center<br/>
    ///If method is trigonmetric, then radii are in coordinate system units.  If spherical or spheroidal, then radii are in meters.
    ///<br/><br/>Returns jagged double array
    ///</summary>
    public static double[][,] CreateCrossAroundPoint(double dblOriginX, double dblOriginY, double dblVerticalHalfLength, double dblHorizontalHalfLength,
      JenSphericalMethod jenMethod, double dblSemiMajorAxis = 6378137.000, double dblSemiMinorAxis = 6356752.31424518, double dblSphereRadius = 6371000.79000915)
    {
      double dblDestX;
      double dblDestY;
      double[][,] dblCross = new double[4][,];
      switch (jenMethod)
      {
        case JenSphericalMethod.ENUM_UseSpherical:
          PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, dblVerticalHalfLength, 0, out dblDestX, out dblDestY, out _, dblSphereRadius, dblSphereRadius);
          dblCross[0] = new double[,] { { dblOriginX, dblOriginY }, { dblDestX, dblDestY } };
          PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, dblHorizontalHalfLength, 90, out dblDestX, out dblDestY, out _, dblSphereRadius, dblSphereRadius);
          dblCross[1] = new double[,] { { dblOriginX, dblOriginY }, { dblDestX, dblDestY } };
          PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, dblVerticalHalfLength, 180, out dblDestX, out dblDestY, out _, dblSphereRadius, dblSphereRadius);
          dblCross[2] = new double[,] { { dblOriginX, dblOriginY }, { dblDestX, dblDestY } };
          PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, dblHorizontalHalfLength, 270, out dblDestX, out dblDestY, out _, dblSphereRadius, dblSphereRadius);
          dblCross[3] = new double[,] { { dblOriginX, dblOriginY }, { dblDestX, dblDestY } };
          break;
        case JenSphericalMethod.ENUM_UseSpheroidal:
          PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, dblVerticalHalfLength, 0, out dblDestX, out dblDestY, out _, dblSemiMajorAxis, dblSemiMinorAxis);
          dblCross[0] = new double[,] { { dblOriginX, dblOriginY }, { dblDestX, dblDestY } };
          PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, dblHorizontalHalfLength, 90, out dblDestX, out dblDestY, out _, dblSemiMajorAxis, dblSemiMinorAxis);
          dblCross[1] = new double[,] { { dblOriginX, dblOriginY }, { dblDestX, dblDestY } };
          PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, dblVerticalHalfLength, 180, out dblDestX, out dblDestY, out _, dblSemiMajorAxis, dblSemiMinorAxis);
          dblCross[2] = new double[,] { { dblOriginX, dblOriginY }, { dblDestX, dblDestY } };
          PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, dblHorizontalHalfLength, 270, out dblDestX, out dblDestY, out _, dblSemiMajorAxis, dblSemiMinorAxis);
          dblCross[3] = new double[,] { { dblOriginX, dblOriginY }, { dblDestX, dblDestY } };
          break;
        default:
          dblCross[0] = new double[,] { { dblOriginX, dblOriginY }, { dblOriginX, dblOriginY + dblVerticalHalfLength } };
          dblCross[1] = new double[,] { { dblOriginX, dblOriginY }, { dblOriginX + dblHorizontalHalfLength, dblOriginY } };
          dblCross[2] = new double[,] { { dblOriginX, dblOriginY }, { dblOriginX, dblOriginY - dblVerticalHalfLength } };
          dblCross[3] = new double[,] { { dblOriginX, dblOriginY }, { dblOriginX - dblHorizontalHalfLength, dblOriginY } };
          break;
      }

      return dblCross;

      //dblOriginX = -111.58108;
      //dblOriginY = 35.2580864;
      //double[][,] dblCoords = CreateCrossAroundPoint(dblOriginX, dblOriginY, 20, 30, JenSphericalMethod.ENUM_UseSpheroidal);
      //Console.WriteLine("  ' Spheroidal");

      //for (int i = 0; i < dblCoords.GetLength(0); i++)
      //{
      //  Console.WriteLine("  set pPolyline = New Polyline");
      //  Console.WriteLine("  set pPolyline.SpatialReference = pWGS84");
      //  Console.WriteLine("  set pPtColl = pPolyline");
      //  Console.WriteLine("  set pPoint = new EsriGeometry.Point");
      //  Console.WriteLine("  pPoint.PutCoords " + dblCoords[i][0, 0].ToString("0.0000000") + ", " + dblCoords[i][0, 1].ToString("0.0000000"));
      //  Console.WriteLine("  pPtColl.AddPoint pPoint ");
      //  Console.WriteLine("  set pPoint = new EsriGeometry.Point");
      //  Console.WriteLine("  pPoint.PutCoords " + dblCoords[i][1, 0].ToString("0.0000000") + ", " + dblCoords[i][1, 1].ToString("0.0000000"));
      //  Console.WriteLine("  pPtColl.AddPoint pPoint ");
      //  //Console.WriteLine(i.ToString("0") + "...[" + dblCoords[0][i, 0].ToString("0.0000000") + ", " + dblCoords[0][i, 1].ToString("0.0000000") + "]");
      //  Console.WriteLine("  MyGeneralOperations.Graphic_MakeFromGeometry pMxDoc, pPolyline, \"Delete_Me\"");
      //}

      //dblOriginX = 447141;
      //dblOriginY = 3901800;
      //double[][,] dblCoords = CreateCrossAroundPoint(dblOriginX, dblOriginY, 20, 30, JenSphericalMethod.ENUM_UseTrigonometry);
      //Console.WriteLine("  ' Spheroidal");

      //for (int i = 0; i < dblCoords.GetLength(0); i++)
      //{
      //  Console.WriteLine("  set pPolyline = New Polyline");
      //  Console.WriteLine("  set pPolyline.SpatialReference = pUTMZone12");
      //  Console.WriteLine("  set pPtColl = pPolyline");
      //  Console.WriteLine("  set pPoint = new EsriGeometry.Point");
      //  Console.WriteLine("  pPoint.PutCoords " + dblCoords[i][0, 0].ToString("0.0000000") + ", " + dblCoords[i][0, 1].ToString("0.0000000"));
      //  Console.WriteLine("  pPtColl.AddPoint pPoint ");
      //  Console.WriteLine("  set pPoint = new EsriGeometry.Point");
      //  Console.WriteLine("  pPoint.PutCoords " + dblCoords[i][1, 0].ToString("0.0000000") + ", " + dblCoords[i][1, 1].ToString("0.0000000"));
      //  Console.WriteLine("  pPtColl.AddPoint pPoint ");
      //  //Console.WriteLine(i.ToString("0") + "...[" + dblCoords[0][i, 0].ToString("0.0000000") + ", " + dblCoords[0][i, 1].ToString("0.0000000") + "]");
      //  Console.WriteLine("  MyGeneralOperations.Graphic_MakeFromGeometry pMxDoc, pPolyline, \"Delete_Me\"");
      //}
    }
    ///<summary>
    ///Given a center point, two radii, and rotation, with options for spherical, spheroidal and trigonometric, returns a jagged double array ellipse<br/>
    ///If method is trigonmetric, then radii are in coordinate system units.  If spherical or spheroidal, then radii are in meters.
    ///<br/><br/>Returns jagged double array
    ///</summary>
    public static double[][,] CreateEllipseAroundPoint(double[] dblPoint, double dblEllipseSemiMajorAxis, double dblEllipseSemiMinorAxis,
      JenSphericalMethod jenMethod, double dblFlatOrientationCCWFromHorizontal = 0, long lngPointCount = 360,
      double dblSemiMajorAxis = 6378137.000, double dblSemiMinorAxis = 6356752.31424518, double dblSphereRadius = 6371000.79000915)
    {
      double dblOriginX = dblPoint[0];
      double dblOriginY = dblPoint[1];
      return CreateEllipseAroundPoint(dblOriginX, dblOriginY, dblEllipseSemiMajorAxis, dblEllipseSemiMinorAxis, jenMethod, dblFlatOrientationCCWFromHorizontal,
        lngPointCount, dblSemiMajorAxis, dblSemiMinorAxis, dblSphereRadius);
    }
    ///<summary>
    ///Given a center point, two radii, and rotation, with options for spherical, spheroidal and trigonometric, returns a jagged double array ellipse<br/>
    ///If method is trigonmetric, then radii are in coordinate system units.  If spherical or spheroidal, then radii are in meters.
    ///<br/><br/>Returns jagged double array
    ///</summary>
    public static double[][,] CreateEllipseAroundPoint(double dblOriginX, double dblOriginY, double dblEllipseSemiMajorAxis, double dblEllipseSemiMinorAxis,
      JenSphericalMethod jenMethod, double dblFlatOrientationCCWFromHorizontal = 0, long lngPointCount = 360,
      double dblSemiMajorAxis = 6378137.000, double dblSemiMinorAxis = 6356752.31424518, double dblSphereRadius = 6371000.79000915)
    {
      double dblInterval = 360d / Convert.ToDouble(lngPointCount);
      double[][,] dblEllipse = new double[1][,];
      dblEllipse[0] = new double[lngPointCount + 1, 2];
      double dblTempX;
      double dblTempY;
      double dblDestX;
      double dblDestY;
      double dblRadiansFromNorth = DegToRad(dblFlatOrientationCCWFromHorizontal);
      double dblRadians;
      double dblBearing;
      for (int i = 0; i <= lngPointCount; i++)
      {
        dblRadians = DegToRad(Convert.ToDouble(i) * dblInterval);
        //Console.WriteLine("Step " + i + "] Adding Bearing '" + (Convert.ToDouble(i) * dblInterval).ToString("0.000") + " degrees");

        switch (jenMethod)
        {
          case JenSphericalMethod.ENUM_UseTrigonometry:
            dblDestX = dblOriginX + (dblEllipseSemiMajorAxis * Math.Cos(dblRadians) * Math.Cos(dblRadiansFromNorth)) -
                (dblEllipseSemiMinorAxis * Math.Sin(dblRadians) * Math.Sin(dblRadiansFromNorth));
            dblDestY = dblOriginY + (dblEllipseSemiMajorAxis * Math.Cos(dblRadians) * Math.Sin(dblRadiansFromNorth)) +
                (dblEllipseSemiMinorAxis * Math.Sin(dblRadians) * Math.Cos(dblRadiansFromNorth));
            break;
          case JenSphericalMethod.ENUM_UseSpheroidal:
            dblTempX = 0 + (dblEllipseSemiMajorAxis * Math.Cos(dblRadians) * Math.Cos(dblRadiansFromNorth)) -
                (dblEllipseSemiMinorAxis * Math.Sin(dblRadians) * Math.Sin(dblRadiansFromNorth));
            dblTempY = 0 + (dblEllipseSemiMajorAxis * Math.Cos(dblRadians) * Math.Sin(dblRadiansFromNorth)) +
                (dblEllipseSemiMinorAxis * Math.Sin(dblRadians) * Math.Cos(dblRadiansFromNorth));
            dblBearing = CalcBearingNumbers(0, 0, dblTempX, dblTempY);
            PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, Math.Pow(Math.Pow(dblTempX, 2) + Math.Pow(dblTempY, 2), 0.5), dblBearing, out dblDestX, out dblDestY, out _, dblSemiMajorAxis, dblSemiMinorAxis);
            break;
          case JenSphericalMethod.ENUM_UseSpherical:
            dblTempX = 0 + (dblEllipseSemiMajorAxis * Math.Cos(dblRadians) * Math.Cos(dblRadiansFromNorth)) -
                (dblEllipseSemiMinorAxis * Math.Sin(dblRadians) * Math.Sin(dblRadiansFromNorth));
            dblTempY = 0 + (dblEllipseSemiMajorAxis * Math.Cos(dblRadians) * Math.Sin(dblRadiansFromNorth)) +
                (dblEllipseSemiMinorAxis * Math.Sin(dblRadians) * Math.Cos(dblRadiansFromNorth));
            dblBearing = CalcBearingNumbers(0, 0, dblTempX, dblTempY);
            PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, Math.Pow(Math.Pow(dblTempX, 2) + Math.Pow(dblTempY, 2), 0.5), dblBearing, out dblDestX, out dblDestY, out _, dblSphereRadius, dblSphereRadius);
            break;
          default:
            dblDestX = double.NaN;
            dblDestY = double.NaN;
            break;
        }
        dblEllipse[0][i, 0] = dblDestX;
        dblEllipse[0][i, 1] = dblDestY;
      }

      return dblEllipse;

      //double dblOriginX = 447141;
      //double dblOriginY = 3901800;

      //double[][,] dblCoords = CreateEllipseAroundPoint(dblOriginX, dblOriginY, 30 ,20, JenSphericalMethod.ENUM_UseTrigonometry,10,10);
      //Console.WriteLine("  ' Trigonometric");
      //Console.WriteLine("  set pPolygon = new polygon");
      //Console.WriteLine("  set pPolygon.SpatialReference = pUTMZone12");
      //Console.WriteLine("  set pPtColl = pPolygon");
      ////Console.WriteLine("...[" + dblOriginX.ToString("0.0000000") + ", " + dblOriginY.ToString("0.0000000") + "], Radius = " + dblRadius.ToString("0.0000000") + "]");
      //for (int i = 0; i < dblCoords[0].GetLength(0); i++)
      //{
      //  Console.WriteLine("  set pPoint = new EsriGeometry.Point");
      //  Console.WriteLine("  pPoint.PutCoords " + dblCoords[0][i, 0].ToString("0.0000000") + ", " + dblCoords[0][i, 1].ToString("0.0000000"));
      //  //Console.WriteLine(i.ToString("0") + "...[" + dblCoords[0][i, 0].ToString("0.0000000") + ", " + dblCoords[0][i, 1].ToString("0.0000000") + "]");
      //  Console.WriteLine("  pPtColl.AddPoint pPoint ");
      //}
      //Console.WriteLine("  MyGeneralOperations.Graphic_MakeFromGeometry pMxDoc, pPolygon, \"Delete_Me\"");

      //dblOriginX = -111.58108;
      //dblOriginY = 35.2580864;
      //double[][,] dblCoords = CreateEllipseAroundPoint(dblOriginX, dblOriginY, 30, 20, JenSphericalMethod.ENUM_UseSpheroidal,  -10, 30);
      //Console.WriteLine("  ' Spheroidal");
      //Console.WriteLine("  set pPolygon = new polygon");
      //Console.WriteLine("  set pPolygon.SpatialReference = pWGS84");
      //Console.WriteLine("  set pPtColl = pPolygon");
      ////Console.WriteLine("...[" + dblOriginX.ToString("0.0000000") + ", " + dblOriginY.ToString("0.0000000") + "], Radius = " + dblRadius.ToString("0.0000000") + "]");
      //for (int i = 0; i < dblCoords[0].GetLength(0); i++)
      //{
      //  Console.WriteLine("  set pPoint = new EsriGeometry.Point");
      //  Console.WriteLine("  pPoint.PutCoords " + dblCoords[0][i, 0].ToString("0.0000000") + ", " + dblCoords[0][i, 1].ToString("0.0000000"));
      //  Console.WriteLine("  pPtColl.AddPoint pPoint ");
      //  //Console.WriteLine(i.ToString("0") + "...[" + dblCoords[0][i, 0].ToString("0.0000000") + ", " + dblCoords[0][i, 1].ToString("0.0000000") + "]");
      //}
      //Console.WriteLine("  MyGeneralOperations.Graphic_MakeFromGeometry pMxDoc, pPolygon, \"Delete_Me\"");

      //dblOriginX = -111.58108;
      //dblOriginY = 35.2580864;
      //double[][,] dblCoords = CreateEllipseAroundPoint(dblOriginX, dblOriginY, 30, 20, JenSphericalMethod.ENUM_UseSpherical, -10, 30);
      //Console.WriteLine("  ' Spheroidal");
      //Console.WriteLine("  set pPolygon = new polygon");
      //Console.WriteLine("  set pPolygon.SpatialReference = pWGS84");
      //Console.WriteLine("  set pPtColl = pPolygon");
      ////Console.WriteLine("...[" + dblOriginX.ToString("0.0000000") + ", " + dblOriginY.ToString("0.0000000") + "], Radius = " + dblRadius.ToString("0.0000000") + "]");
      //for (int i = 0; i < dblCoords[0].GetLength(0); i++)
      //{
      //  Console.WriteLine("  set pPoint = new EsriGeometry.Point");
      //  Console.WriteLine("  pPoint.PutCoords " + dblCoords[0][i, 0].ToString("0.0000000") + ", " + dblCoords[0][i, 1].ToString("0.0000000"));
      //  Console.WriteLine("  pPtColl.AddPoint pPoint ");
      //  //Console.WriteLine(i.ToString("0") + "...[" + dblCoords[0][i, 0].ToString("0.0000000") + ", " + dblCoords[0][i, 1].ToString("0.0000000") + "]");
      //}
      //Console.WriteLine("  MyGeneralOperations.Graphic_MakeFromGeometry pMxDoc, pPolygon, \"Delete_Me\"");
    }
    ///<summary>
    ///Given a center point, distance and bearing, calculates the number of "degrees", such that 66 miles would be roughly 1 degree.<br/>
    ///Only intended to give a quick estimate of conversion, because these "degrees" don't translate to exact changes in latitude or longtitude.
    ///<br/><br/>Returns double
    ///</summary>
    public static double EstimateDistanceOnSpheroid(double dblOriginX, double dblOriginY, double dblMeters, double dblAZ = 45)
    {
      PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, dblMeters, dblAZ, out double dblNewX, out double dblNewY, out _);
      return DistancePythagoreanNumbers(dblOriginX, dblOriginY, dblNewX, dblNewY);
    }
    ///<summary>
    ///Given a center point, distance and bearing, calculates the number of "degrees", such that 66 miles would be roughly 1 degree.<br/>
    ///Only intended to give a quick estimate of conversion, because these "degrees" don't translate to exact changes in latitude or longtitude.
    ///<br/><br/>Returns double
    ///</summary>
    public static double EstimateDistanceOnSpheroid(double[] dblPoint, double dblMeters, double dblAZ = 45)
    {
      return EstimateDistanceOnSpheroid(dblPoint[0], dblPoint[1], dblMeters, dblAZ);
    }

    ///<summary>
    ///Given a center point and X/Y Distances, options for spherical, spheroidal and trigonometric, returns a jagged double array rectangular polygon<br/>
    ///If method is trigonmetric, then radius is in coordinate system units.  If spherical or spheroidal, then X/Y Distances are in meters.
    ///<br/><br/>Returns jagged double array
    ///</summary>
    public static double[][,] CreateBoxAroundPoint(double[] dblPoint, double dblXDistanceFromOrigin, double dblYDistanceFromOrigin, JenSphericalMethod jenMethod,
      double dblSemiMajorAxis = 6378137.000, double dblSemiMinorAxis = 6356752.31424518, double dblSphereRadius = 6371000.79000915)
    {
      double dblOriginX = dblPoint[0];
      double dblOriginY = dblPoint[1];
      return CreateBoxAroundPoint(dblOriginX, dblOriginY, dblXDistanceFromOrigin, dblYDistanceFromOrigin, jenMethod, dblSemiMajorAxis, dblSemiMinorAxis, dblSphereRadius);
    }
    ///<summary>
    ///Given a center point and X/Y Distances, options for spherical, spheroidal and trigonometric, returns a jagged double array rectangular polygon<br/>
    ///If method is trigonmetric, then radius is in coordinate system units.  If spherical or spheroidal, then X/Y Distances are in meters.
    ///<br/><br/>Returns jagged double array
    ///</summary>
    public static double[][,] CreateBoxAroundPoint(double dblOriginX, double dblOriginY, double dblXDistanceFromOrigin, double dblYDistanceFromOrigin, JenSphericalMethod jenMethod,
      double dblSemiMajorAxis = 6378137.000, double dblSemiMinorAxis = 6356752.31424518, double dblSphereRadius = 6371000.79000915)
    {
      double[][,] dblBox = new double[1][,];
      dblBox[0] = new double[5, 2];
      double dblTempX;
      double dblTempY;
      double dblDestX1;
      double dblDestY1;
      double dblDestX2;
      double dblDestY2;
      double dblDestX3;
      double dblDestY3;
      double dblDestX4;
      double dblDestY4;

      //Console.WriteLine("Step " + i + "] Adding Bearing '" + (Convert.ToDouble(i) * dblInterval).ToString("0.000") + " degrees");
      switch (jenMethod)
      {
        case JenSphericalMethod.ENUM_UseTrigonometry:
          dblDestX1 = dblOriginX - dblXDistanceFromOrigin;
          dblDestY1 = dblOriginY + dblYDistanceFromOrigin;
          dblDestX2 = dblOriginX + dblXDistanceFromOrigin;
          dblDestY2 = dblOriginY + dblYDistanceFromOrigin;
          dblDestX3 = dblOriginX + dblXDistanceFromOrigin;
          dblDestY3 = dblOriginY - dblYDistanceFromOrigin;
          dblDestX4 = dblOriginX - dblXDistanceFromOrigin;
          dblDestY4 = dblOriginY - dblYDistanceFromOrigin;
          break;
        case JenSphericalMethod.ENUM_UseSpheroidal:
          PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, dblXDistanceFromOrigin, 270, out dblTempX, out dblTempY, out _, dblSemiMajorAxis, dblSemiMinorAxis);
          PointLineVincentyPerPointNumbers(dblTempX, dblTempY, dblYDistanceFromOrigin, 0, out dblDestX1, out dblDestY1, out _, dblSemiMajorAxis, dblSemiMinorAxis);
          PointLineVincentyPerPointNumbers(dblTempX, dblTempY, dblYDistanceFromOrigin, 180, out dblDestX4, out dblDestY4, out _, dblSemiMajorAxis, dblSemiMinorAxis);
          PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, dblXDistanceFromOrigin, 90, out dblTempX, out dblTempY, out _, dblSemiMajorAxis, dblSemiMinorAxis);
          PointLineVincentyPerPointNumbers(dblTempX, dblTempY, dblYDistanceFromOrigin, 0, out dblDestX2, out dblDestY2, out _, dblSemiMajorAxis, dblSemiMinorAxis);
          PointLineVincentyPerPointNumbers(dblTempX, dblTempY, dblYDistanceFromOrigin, 180, out dblDestX3, out dblDestY3, out _, dblSemiMajorAxis, dblSemiMinorAxis);
          break;
        case JenSphericalMethod.ENUM_UseSpherical:
          PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, dblXDistanceFromOrigin, 270, out dblTempX, out dblTempY, out _, dblSphereRadius, dblSphereRadius);
          PointLineVincentyPerPointNumbers(dblTempX, dblTempY, dblYDistanceFromOrigin, 0, out dblDestX1, out dblDestY1, out _, dblSphereRadius, dblSphereRadius);
          PointLineVincentyPerPointNumbers(dblTempX, dblTempY, dblYDistanceFromOrigin, 180, out dblDestX4, out dblDestY4, out _, dblSphereRadius, dblSphereRadius);
          PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, dblXDistanceFromOrigin, 90, out dblTempX, out dblTempY, out _, dblSphereRadius, dblSphereRadius);
          PointLineVincentyPerPointNumbers(dblTempX, dblTempY, dblYDistanceFromOrigin, 0, out dblDestX2, out dblDestY2, out _, dblSphereRadius, dblSphereRadius);
          PointLineVincentyPerPointNumbers(dblTempX, dblTempY, dblYDistanceFromOrigin, 180, out dblDestX3, out dblDestY3, out _, dblSphereRadius, dblSphereRadius);
          break;
        default:
          dblDestX1 = double.NaN;
          dblDestY1 = double.NaN;
          dblDestX2 = double.NaN;
          dblDestY2 = double.NaN;
          dblDestX3 = double.NaN;
          dblDestY3 = double.NaN;
          dblDestX4 = double.NaN;
          dblDestY4 = double.NaN;
          break;
      }
      dblBox[0] = new double[5, 2] { { dblDestX1, dblDestY1 }, { dblDestX2, dblDestY2 }, { dblDestX3, dblDestY3 }, { dblDestX4, dblDestY4 }, { dblDestX1, dblDestY1 } };

      return dblBox;
    }

    ///<summary>
    ///Given a center point and radius, options for spherical, spheroidal and trigonometric, returns a jagged double array circle<br/>
    ///If method is trigonmetric, then radius is in coordinate system units.  If spherical or spheroidal, then radius is in meters.
    ///<br/><br/>Returns jagged double array
    ///</summary>
    public static double[][,] CreateCircleAroundPoint(double[] dblPoint, double dblRadius, long lngPointCount, JenSphericalMethod jenMethod,
      double dblSemiMajorAxis = 6378137.000, double dblSemiMinorAxis = 6356752.31424518, double dblSphereRadius = 6371000.79000915)
    {
      double dblOriginX = dblPoint[0];
      double dblOriginY = dblPoint[1];
      return CreateCircleAroundPoint(dblOriginX, dblOriginY, dblRadius, lngPointCount, jenMethod, dblSemiMajorAxis, dblSemiMinorAxis, dblSphereRadius);
    }
    ///<summary>
    ///Given a center point and radius, options for spherical, spheroidal and trigonometric, returns a jagged double array circle<br/>
    ///If method is trigonmetric, then radius is in coordinate system units.  If spherical or spheroidal, then radius is in meters.
    ///<br/><br/>Returns jagged double array
    ///</summary>
    public static double[][,] CreateCircleAroundPoint(double dblOriginX, double dblOriginY, double dblRadius, long lngPointCount, JenSphericalMethod jenMethod,
      double dblSemiMajorAxis = 6378137.000, double dblSemiMinorAxis = 6356752.31424518, double dblSphereRadius = 6371000.79000915)
    {
      double dblInterval = 360d / Convert.ToDouble(lngPointCount);
      double[][,] dblCircle = new double[1][,];
      dblCircle[0] = new double[lngPointCount + 1, 2];
      double dblDestX;
      double dblDestY;
      for (int i = 0; i <= lngPointCount; i++)
      {
        //Console.WriteLine("Step " + i + "] Adding Bearing '" + (Convert.ToDouble(i) * dblInterval).ToString("0.000") + " degrees");
        switch (jenMethod)
        {
          case JenSphericalMethod.ENUM_UseTrigonometry:
            CalcPointLine(dblOriginX, dblOriginY, dblRadius, Convert.ToDouble(i) * dblInterval, out dblDestX, out dblDestY, out _);
            break;
          case JenSphericalMethod.ENUM_UseSpheroidal:
            PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, dblRadius, Convert.ToDouble(i) * dblInterval, out dblDestX, out dblDestY, out _, dblSemiMajorAxis, dblSemiMinorAxis);
            break;
          case JenSphericalMethod.ENUM_UseSpherical:
            PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, dblRadius, Convert.ToDouble(i) * dblInterval, out dblDestX, out dblDestY, out _, dblSphereRadius, dblSphereRadius);
            break;
          default:
            dblDestX = double.NaN;
            dblDestY = double.NaN;
            break;
        }
        dblCircle[0][i, 0] = dblDestX;
        dblCircle[0][i, 1] = dblDestY;
      }
      return dblCircle;
      //double dblOriginX = 25;
      //double dblOriginY = 25;
      //double dblRadius = 10;

      //double[][,] dblCoords = CreateCircleAroundPoint(dblOriginX, dblOriginY, dblRadius, 4, JenSphericalMethod.ENUM_UseTrigonometry);
      //Console.WriteLine("Trigonometric");
      //Console.WriteLine("...[" + dblOriginX.ToString("0.0000000") + ", " + dblOriginY.ToString("0.0000000") + "], Radius = " + dblRadius.ToString("0.0000000") + "]");
      //for (int i = 0; i < dblCoords[0].GetLength(0); i++)
      //{
      //  Console.WriteLine(i.ToString("0") + "...[" + dblCoords[0][i, 0].ToString("0.0000000") + ", " + dblCoords[0][i, 1].ToString("0.0000000") + "]");
      //}
      //dblCoords = CreateCircleAroundPoint(dblOriginX, dblOriginY, dblRadius, 4, JenSphericalMethod.ENUM_UseSpheroidal);
      //Console.WriteLine("Spheroidal");
      //Console.WriteLine("...[" + dblOriginX.ToString("0.0000000") + ", " + dblOriginY.ToString("0.0000000") + "], Radius = " + dblRadius.ToString("0.0000000") + "]");
      //for (int i = 0; i < dblCoords[0].GetLength(0); i++)
      //{
      //  Console.WriteLine(i.ToString("0") + "...[" + dblCoords[0][i, 0].ToString("0.0000000") + ", " + dblCoords[0][i, 1].ToString("0.0000000") + "]");
      //}
      //dblCoords = CreateCircleAroundPoint(dblOriginX, dblOriginY, dblRadius, 4, JenSphericalMethod.ENUM_UseSpherical);
      //Console.WriteLine("Spherical");
      //Console.WriteLine("...[" + dblOriginX.ToString("0.0000000") + ", " + dblOriginY.ToString("0.0000000") + "], Radius = " + dblRadius.ToString("0.0000000") + "]");
      //for (int i = 0; i < dblCoords[0].GetLength(0); i++)
      //{
      //  Console.WriteLine(i.ToString("0") + "...[" + dblCoords[0][i, 0].ToString("0.0000000") + ", " + dblCoords[0][i, 1].ToString("0.0000000") + "]");
      //}
    }

    ///<summary>
    ///Given three consecutive point planar coordinates P:Q:R, returns the internal angle defined by PQ -> QR, and also the angle of deviation<br/>
    ///<br/><br/>Returns double value
    ///</summary>
    public static double CalcInternalAngle(double dblPX, double dblPY, double dblQX, double dblQY, double dblRX, double dblRY, out double dblAngleDev)
    {
      double dblLenPQ = Math.Pow(Math.Pow(dblPX - dblQX, 2) + Math.Pow(dblPY - dblQY, 2), 0.5);
      double dblLenQR = Math.Pow(Math.Pow(dblQX - dblRX, 2) + Math.Pow(dblQY - dblRY, 2), 0.5);
      double dblLenRP = Math.Pow(Math.Pow(dblRX - dblPX, 2) + Math.Pow(dblRY - dblPY, 2), 0.5);
      double dblReturn = RadToDeg(Math.Acos(Math.Clamp((Math.Pow(dblLenPQ, 2) + Math.Pow(dblLenQR, 2) - Math.Pow(dblLenRP, 2)) / (2 * dblLenPQ * dblLenQR), -1d, 1d)));
      dblAngleDev = 180 - dblReturn;
      return dblReturn;
    }

    ///<summary>
    ///Given a Point in geocentric [Planetocentric] coordinates, returns Point in geographic [Planetographic] coordinates.<br/>
    ///If spheroid not specified, assumes WGS 1984.
    ///<br/><br/>Returns doubles for Latitude and Longitude.
    ///</summary>
    public static double[] FeatureLongitudeShift(double[] dblSourceGeometry, double dblLongitudeShift)
    {
      double[] dblReturn = new double[dblSourceGeometry.Length];
      dblReturn[0] = dblSourceGeometry[0] + dblLongitudeShift;
      dblReturn[1] = dblSourceGeometry[1];

      return dblReturn;
    }
    ///<summary>
    ///Given a Multipoint in geocentric [Planetocentric] coordinates, returns feature in geographic [Planetographic] coordinates.<br/>
    ///If spheroid not specified, assumes WGS 1984.
    ///<br/><br/>Returns doubles for Latitude and Longitude.
    ///</summary>
    public static double[,] FeatureLongitudeShift(double[,] dblSourceGeometry, double dblLongitudeShift)
    {
      double[,] dblReturn = new double[dblSourceGeometry.GetLength(0), dblSourceGeometry.GetLength(1)];

      for (int i = 0; i < dblSourceGeometry.GetLength(0); i++)
      {
        dblReturn[i, 0] = dblSourceGeometry[i, 0] + dblLongitudeShift;
        dblReturn[i, 1] = dblSourceGeometry[i, 1];
      }

      return dblReturn;
    }
    ///<summary>
    ///Given a polygon or polyline in geocentric [Planetocentric] coordinates, returns feature in geographic [Planetographic] coordinates.<br/>
    ///If spheroid not specified, assumes WGS 1984.
    ///<br/><br/>Returns doubles for Latitude and Longitude.
    ///</summary>
    public static double[][,] FeatureLongitudeShift(double[][,] dblSourceGeometry, double dblLongitudeShift)
    {
      double[][,] dblReturn = new double[dblSourceGeometry.Length][,];
      //      foreach (double[,] dblPart in dblReturn)
      for (int j = 0; j < dblSourceGeometry.Length; j++)
      {
        double[,] dblPart = dblSourceGeometry[j];
        double[,] dblNewPart = new double[dblPart.GetLength(0), dblPart.GetLength(1)];
        for (int i = 0; i < dblPart.GetLength(0); i++)
        {
          dblNewPart[i, 0] = dblPart[i, 0] + dblLongitudeShift;
          dblNewPart[i, 1] = dblPart[i, 1];
        }
        dblReturn[j] = dblNewPart;
      }
      return dblReturn;
    }

    ///<summary>
    ///Given a Point in geocentric [Planetocentric] coordinates, returns Point in geographic [Planetographic] coordinates.<br/>
    ///If spheroid not specified, assumes WGS 1984.
    ///<br/><br/>Returns doubles for Latitude and Longitude.
    ///</summary>
    public static double[] FeaturePlanetOCentricToPlanetOGraphic(double[] dblSourceGeometry, double dblLongitudeShift,
      double dblSemiMajorAxis = 6378137.000, double dblSemiMinorAxis = 6356752.31424518)
    {
      double[] dblReturn = new double[dblSourceGeometry.Length];

      XYOCentricToOGraphic(dblSourceGeometry[0], dblSourceGeometry[1], dblLongitudeShift, out double dblNewLong, out double dblNewLat, dblSemiMajorAxis, dblSemiMinorAxis);
      dblReturn[0] = dblNewLong;
      dblReturn[1] = dblNewLat;

      return dblReturn;
    }
    ///<summary>
    ///Given a Multipoint in geocentric [Planetocentric] coordinates, returns feature in geographic [Planetographic] coordinates.<br/>
    ///If spheroid not specified, assumes WGS 1984.
    ///<br/><br/>Returns doubles for Latitude and Longitude.
    ///</summary>
    public static double[,] FeaturePlanetOCentricToPlanetOGraphic(double[,] dblSourceGeometry, double dblLongitudeShift,
      double dblSemiMajorAxis = 6378137.000, double dblSemiMinorAxis = 6356752.31424518)
    {
      double[,] dblReturn = new double[dblSourceGeometry.GetLength(0), dblSourceGeometry.GetLength(1)];

      for (int i = 0; i < dblSourceGeometry.GetLength(0); i++)
      {
        XYOCentricToOGraphic(dblSourceGeometry[i, 0], dblSourceGeometry[i, 1], dblLongitudeShift, out double dblNewLong, out double dblNewLat, dblSemiMajorAxis, dblSemiMinorAxis);
        dblReturn[i, 0] = dblNewLong;
        dblReturn[i, 1] = dblNewLat;
      }

      return dblReturn;
    }
    ///<summary>
    ///Given a polygon or polyline in geocentric [Planetocentric] coordinates, returns feature in geographic [Planetographic] coordinates.<br/>
    ///If spheroid not specified, assumes WGS 1984.
    ///<br/><br/>Returns doubles for Latitude and Longitude.
    ///</summary>
    public static double[][,] FeaturePlanetOCentricToPlanetOGraphic(double[][,] dblSourceGeometry, double dblLongitudeShift,
      double dblSemiMajorAxis = 6378137.000, double dblSemiMinorAxis = 6356752.31424518)
    {
      double[][,] dblReturn = new double[dblSourceGeometry.Length][,];

      //      foreach (double[,] dblPart in dblReturn)
      for (int j = 0; j < dblSourceGeometry.Length; j++)
      {
        double[,] dblPart = dblSourceGeometry[j];
        double[,] dblNewPart = new double[dblPart.GetLength(0), dblPart.GetLength(1)];
        for (int i = 0; i < dblPart.GetLength(0); i++)
        {
          XYOCentricToOGraphic(dblPart[i, 0], dblPart[i, 1], dblLongitudeShift, out double dblNewLong, out double dblNewLat, dblSemiMajorAxis, dblSemiMinorAxis);
          dblNewPart[i, 0] = dblNewLong;
          dblNewPart[i, 1] = dblNewLat;
        }
        dblReturn[j] = dblNewPart;
      }
      return dblReturn;
    }

    ///<summary>
    ///Given a Point in geographic [Planetographic] coordinates, returns Point in geocentric [Planetocentric] coordinates.<br/>
    ///If spheroid not specified, assumes WGS 1984.
    ///<br/><br/>Returns doubles for Latitude and Longitude.
    ///</summary>
    public static double[] FeaturePlanetOGraphicToPlanetOCentric(double[] dblSourceGeometry, double dblLongitudeShift,
      double dblSemiMajorAxis = 6378137.000, double dblSemiMinorAxis = 6356752.31424518)
    {
      double[] dblReturn = new double[dblSourceGeometry.Length];

      XYOGraphicToOCentric(dblSourceGeometry[0], dblSourceGeometry[1], dblLongitudeShift, out double dblNewLong, out double dblNewLat, dblSemiMajorAxis, dblSemiMinorAxis);
      dblReturn[0] = dblNewLong;
      dblReturn[1] = dblNewLat;

      return dblReturn;
    }
    ///<summary>
    ///Given a Multipoint in geographic [Planetographic] coordinates, returns feature in geocentric [Planetocentric] coordinates.<br/>
    ///If spheroid not specified, assumes WGS 1984.
    ///<br/><br/>Returns doubles for Latitude and Longitude.
    ///</summary>
    public static double[,] FeaturePlanetOGraphicToPlanetOCentric(double[,] dblSourceGeometry, double dblLongitudeShift,
      double dblSemiMajorAxis = 6378137.000, double dblSemiMinorAxis = 6356752.31424518)
    {
      double[,] dblReturn = new double[dblSourceGeometry.GetLength(0), dblSourceGeometry.GetLength(1)];

      for (int i = 0; i < dblSourceGeometry.GetLength(0); i++)
      {
        XYOGraphicToOCentric(dblSourceGeometry[i, 0], dblSourceGeometry[i, 1], dblLongitudeShift, out double dblNewLong, out double dblNewLat, dblSemiMajorAxis, dblSemiMinorAxis);
        dblReturn[i, 0] = dblNewLong;
        dblReturn[i, 1] = dblNewLat;
      }

      return dblReturn;
    }
    ///<summary>
    ///Given a polygon or polyline in geographic [Planetographic] coordinates, returns feature in geocentric [Planetocentric] coordinates.<br/>
    ///If spheroid not specified, assumes WGS 1984.
    ///<br/><br/>Returns doubles for Latitude and Longitude.
    ///</summary>
    public static double[][,] FeaturePlanetOGraphicToPlanetOCentric(double[][,] dblSourceGeometry, double dblLongitudeShift,
      double dblSemiMajorAxis = 6378137.000, double dblSemiMinorAxis = 6356752.31424518)
    {
      double[][,] dblReturn = new double[dblSourceGeometry.Length][,];

      //      foreach (double[,] dblPart in dblReturn)
      for (int j = 0; j < dblSourceGeometry.Length; j++)
      {
        double[,] dblPart = dblSourceGeometry[j];
        double[,] dblNewPart = new double[dblPart.GetLength(0), dblPart.GetLength(1)];
        for (int i = 0; i < dblPart.GetLength(0); i++)
        {
          XYOGraphicToOCentric(dblPart[i, 0], dblPart[i, 1], dblLongitudeShift, out double dblNewLong, out double dblNewLat, dblSemiMajorAxis, dblSemiMinorAxis);
          dblNewPart[i, 0] = dblNewLong;
          dblNewPart[i, 1] = dblNewLat;
        }
        dblReturn[j] = dblNewPart;
      }
      return dblReturn;
    }

    ///<summary>
    ///Given geographic [Planetographic] coordinates, returns geocentric [Planetocentric] coordinates.<br/>
    ///If spheroid not specified, assumes WGS 1984.
    ///<br/><br/>Returns doubles for Latitude and Longitude.
    ///</summary>
    public static void XYOGraphicToOCentric(double dblLongitude, double dblLatitude, double dblLongitudeShift, out double dblNewLongitude,
      out double dblNewLatitude, double dblSemiMajorAxis = 6378137.000, double dblSemiMinorAxis = 6356752.31424518)
    {
      dblNewLongitude = dblLongitude + dblLongitudeShift;
      dblNewLatitude = RadToDeg(Math.Atan(Math.Tan(DegToRad(dblLatitude)) / Math.Pow((dblSemiMajorAxis / dblSemiMinorAxis), 2)));
    }
    ///<summary>
    ///Given geocentric [Planetocentric] coordinates, returns geographic [Planetographic] coordinates.<br/>
    ///If spheroid not specified, assumes WGS 1984.
    ///<br/><br/>Returns doubles for Latitude and Longitude.
    ///</summary>
    public static void XYOCentricToOGraphic(double dblLongitude, double dblLatitude, double dblLongitudeShift, out double dblNewLongitude,
      out double dblNewLatitude, double dblSemiMajorAxis = 6378137.000, double dblSemiMinorAxis = 6356752.31424518)
    {
      dblNewLongitude = dblLongitude + dblLongitudeShift;
      dblNewLatitude = RadToDeg(Math.Atan(Math.Pow((dblSemiMajorAxis / dblSemiMinorAxis), 2) * (Math.Tan(DegToRad(dblLatitude)))));
    }

    ///<summary>
    ///Given projected coordinates, a bearing and a distance, returns destination coordinates, <br/>
    ///destination bearing and a jagged double array of polyline connector.  If spheroid not specified, assumes WGS 1984.
    ///<br/><br/>Returns doubles for Latitude, Longitude, bearing and coordinate array.
    ///</summary>
    public static void CalcPointLine(double dblOriginX, double dblOriginY, double dblLength, double dblAzimuth,
      out double dblDestX, out double dblDestY, out double[][,] dblPolyline)
    {
      dblAzimuth %= 360;
      if (dblAzimuth < 0) { dblAzimuth += 360; }
      double dblNorthSouth = 0;
      double dblEastWest = 0;
      double dblNorthSouthDistance = 0;
      double dblEastWestDistance = 0;

      switch (dblAzimuth)
      {
        case 0:
          dblNorthSouthDistance = dblLength;
          dblNorthSouth = 1;
          dblEastWestDistance = 0;
          dblEastWest = 1;
          break;
        case 90:
          dblNorthSouthDistance = 0;
          dblNorthSouth = 1;
          dblEastWestDistance = dblLength;
          dblEastWest = 1;
          break;
        case 180:
          dblNorthSouthDistance = dblLength;
          dblNorthSouth = -1;
          dblEastWestDistance = 0;
          dblEastWest = 1;
          break;
        case 270:
          dblNorthSouthDistance = 0;
          dblNorthSouth = 1;
          dblEastWestDistance = dblLength;
          dblEastWest = -1;
          break;
        default:
          if (dblAzimuth > 0 && dblAzimuth < 90)
          {
            dblNorthSouthDistance = Math.Cos(DegToRad(dblAzimuth)) * dblLength;
            dblNorthSouth = 1;
            dblEastWestDistance = Math.Sin(DegToRad(dblAzimuth)) * dblLength;
            dblEastWest = 1;
          }
          else if (dblAzimuth > 90 && dblAzimuth < 180)
          {
            dblNorthSouthDistance = Math.Sin(DegToRad(dblAzimuth - 90)) * dblLength;
            dblNorthSouth = -1;
            dblEastWestDistance = Math.Cos(DegToRad(dblAzimuth - 90)) * dblLength;
            dblEastWest = 1;
          }
          else if (dblAzimuth > 180 && dblAzimuth < 270)
          {
            dblNorthSouthDistance = (Math.Cos(DegToRad(dblAzimuth - 180))) * dblLength;
            dblNorthSouth = -1;
            dblEastWestDistance = Math.Sin(DegToRad(dblAzimuth - 180)) * dblLength;
            dblEastWest = -1;
          }
          else if (dblAzimuth > 270 && dblAzimuth < 360)
          {
            dblNorthSouthDistance = (Math.Sin(DegToRad(dblAzimuth - 270))) * dblLength;
            dblNorthSouth = 1;
            dblEastWestDistance = Math.Cos(DegToRad(dblAzimuth - 270)) * dblLength;
            dblEastWest = -1;
          }
          break;
      }
      double dblMovementNorth = dblNorthSouthDistance * dblNorthSouth;
      double dblMovementWest = dblEastWestDistance * dblEastWest;
      dblDestX = dblOriginX + dblMovementWest;
      dblDestY = dblOriginY + dblMovementNorth;

      dblPolyline = new double[1][,];
      double[,] dblVertices = new double[,] { { dblOriginX, dblOriginY }, { dblDestX, dblDestY } };
      dblPolyline[0] = dblVertices;
    }

    ///<summary>
    ///Given three 3D plane triangle corner coordinates, returns area of triangle<br/><br/>Returns double value
    ///</summary>
    public static double TriangleAreaPoints3DValues(double dblPX, double dblPY, double dblPZ,
      double dblQX, double dblQY, double dblQZ, double dblRX, double dblRY, double dblRZ)
    {
      double dblI = Math.Pow(((dblQY - dblPY) * (dblRZ - dblPZ)) - ((dblRY - dblPY) * (dblQZ - dblPZ)), 2);
      double dblJ = Math.Pow(((dblQX - dblPX) * (dblRZ - dblPZ)) - ((dblRX - dblPX) * (dblQZ - dblPZ)), 2);
      double dblK = Math.Pow(((dblQX - dblPX) * (dblRY - dblPY)) - ((dblRX - dblPX) * (dblQY - dblPY)), 2);

      return Math.Sqrt(dblI + dblJ + dblK) / 2;
    }
    ///<summary>
    ///Given three plane triangle corner coordinates, returns area of triangle<br/><br/>Returns double value
    ///</summary>
    public static double TriangleAreaPointsValues(double dbl1X, double dbl1Y, double dbl2X, double dbl2Y, double dbl3X, double dbl3Y)
    {
      return Math.Abs((((dbl2X - dbl1X) * (dbl3Y - dbl1Y)) - ((dbl3X - dbl1X) * (dbl2Y - dbl1Y))) / 2);
    }
    ///<summary>
    ///Given three plane triangle edge lengths, returns area of triangle<br/><br/>Returns double value
    ///</summary>
    public static double TriangleAreaLegs(double dblLeg1, double dblLeg2, double dblLeg3)
    {
      double dblS = (dblLeg1 + dblLeg2 + dblLeg3) / 2;
      return Math.Sqrt(dblS * (dblS - dblLeg1) * (dblS - dblLeg2) * (dblS - dblLeg3));
    }

    ///<summary>
    ///Given a multipoint double array, returns distance from farthest-separated vertices<br/>
    ///Optionally calculates using spherical, spheroidal or plane trigonometric methods<br/>
    ///If no spheroid specified, assumes WGS 84.<br/><br/>Returns double value
    ///</summary>
    public static void CalcFarthestPointsNumbers(double[,] dblMultipoint, JenSphericalMethod jenMethod, out double dblPoint1X,
      out double dblPoint1Y, out double dblPoint2X, out double dblPoint2Y, out double dblDistance, out double dblAz1,
      out double dblAz2, out double dblReverseAz1, out double dblReverseAz2,
      double dblEquatorialRadius = 6378137.000, double dblPolarRadius = 6356752.31424518)
    {
      double dblTestX1;
      double dblTestY1;
      double dblTestX2;
      double dblTestY2;
      double dblTestAz1 = double.NaN;
      double dblTestAz2 = double.NaN;
      dblAz1 = double.NaN;
      dblAz2 = double.NaN;
      dblReverseAz1 = double.NaN;
      dblReverseAz2 = double.NaN;
      double dblSphereRadius = Math.Pow((Math.Pow(dblEquatorialRadius, 2d) * dblPolarRadius), (1d / 3d));    // SPHERE OF SAME VOLUME RADIUS; PROPER 3-AXIS GEOMETRIC MEAN; (a^2 * b) ^ (1/3)

      double dblMaxDistance = -999;
      double dblTestDistance;
      long lngPointCount = dblMultipoint.GetLength(0);

      // Stop if no points
      if (lngPointCount == 0)
      {
        dblPoint1X = double.NaN;
        dblPoint1Y = double.NaN;
        dblPoint2X = double.NaN;
        dblPoint2Y = double.NaN;
        dblDistance = double.NaN;
        return;
      }

      dblPoint1X = dblMultipoint[0, 0];
      dblPoint1Y = dblMultipoint[0, 1];
      dblPoint2X = dblMultipoint[0, 0];
      dblPoint2Y = dblMultipoint[0, 1];

      // Stop if only one point
      if (lngPointCount == 1)
      {
        dblDistance = 0;
        return;
      }

      // Check all possible pairs of points
      for (int i = 0; i < lngPointCount - 1; i++)
      {
        dblTestX1 = dblMultipoint[i, 0];
        dblTestY1 = dblMultipoint[i, 1];
        //Console.WriteLine(i.ToString("0") + ":...[" + dblTestX1.ToString("0.0000000000") + ", " + dblTestY1.ToString("0.0000000000") + "]");

        for (int j = i + 1; j < lngPointCount; j++)
        {
          dblTestX2 = dblMultipoint[j, 0];
          dblTestY2 = dblMultipoint[j, 1];

          if (jenMethod == JenSphericalMethod.ENUM_UseSpherical)
          {
            dblTestDistance = DistanceHaversineNumbers(dblTestY1, dblTestX1, dblTestY2, dblTestX2, out dblTestAz1, dblSphereRadius);
          }
          else if (jenMethod == JenSphericalMethod.ENUM_UseSpheroidal)
          {
            dblTestDistance = DistanceVincentyNumbers(dblTestX1, dblTestY1, dblTestX2, dblTestY2, out dblTestAz1, out dblTestAz2, dblEquatorialRadius, dblPolarRadius);
          }
          else
          {
            dblTestDistance = DistancePythagoreanNumbers(dblTestX1, dblTestY1, dblTestX2, dblTestY2);
          }
          if (dblTestDistance > dblMaxDistance)
          {
            dblMaxDistance = dblTestDistance;
            dblPoint1X = dblTestX1;
            dblPoint1Y = dblTestY1;
            dblPoint2X = dblTestX2;
            dblPoint2Y = dblTestY2;

            if (jenMethod == JenSphericalMethod.ENUM_UseSpherical)
            {
              dblAz1 = dblTestAz1;
              if (dblAz1 > 360) { dblAz1 -= 360; }
              if (dblAz1 < 0) { dblAz1 += 360; }
              dblAz2 = dblAz1;
            }
            else if (jenMethod == JenSphericalMethod.ENUM_UseSpheroidal)
            {
              dblAz1 = dblTestAz1;
              dblAz2 = dblTestAz2;
            }
            else
            {
              dblAz1 = CalcBearingNumbers(dblTestX1, dblTestY1, dblTestX2, dblTestY2);
              if (dblAz1 > 360) { dblAz1 -= 360; }
              if (dblAz1 < 0) { dblAz1 += 360; }
              dblAz2 = dblAz1;
            }
          }

        } // end j
      }   // end i

      //Console.WriteLine((lngPointCount - 1).ToString("0") + ":...[" + dblCoords[lngPointCount-1,0].ToString("0.0000000000") + ", " + dblCoords[lngPointCount - 1, 1].ToString("0.0000000000") + "]");
      dblDistance = dblMaxDistance;

      if (dblMaxDistance <= 0)
      {
        dblAz1 = double.NaN;
        dblAz2 = double.NaN;
        dblReverseAz1 = double.NaN;
        dblReverseAz2 = double.NaN;
      }
      else
      {
        dblReverseAz1 = dblAz1 - 180;
        if (dblReverseAz1 < 0) { dblReverseAz1 += 360; }
        dblReverseAz2 = dblAz2 - 180;
        if (dblReverseAz2 < 0) { dblReverseAz2 += 360; }
      }

    }
    ///<summary>
    ///Given a polygon or polyline double jagged array, returns distance from farthest-separated vertices<br/>
    ///Optionally calculates using spherical, spheroidal or plane trigonometric methods<br/>
    ///If no spheroid specified, assumes WGS 84.<br/><br/>Returns double value
    ///</summary>
    public static void CalcFarthestPointsNumbers(double[][,] dblPolylinesOrRings, JenSphericalMethod jenMethod, out double dblPoint1X,
      out double dblPoint1Y, out double dblPoint2X, out double dblPoint2Y, out double dblDistance, out double dblAz1,
      out double dblAz2, out double dblReverseAz1, out double dblReverseAz2,
      double dblEquatorialRadius = 6378137.000, double dblPolarRadius = 6356752.31424518)
    {
      dblAz1 = double.NaN;
      dblAz2 = double.NaN;
      dblReverseAz1 = double.NaN;
      dblReverseAz2 = double.NaN;
      int lngCounter = -1;

      long lngPointCount = 0;

      // Get total number of points
      foreach (double[,] dblPart in dblPolylinesOrRings)
      {
        lngPointCount += (dblPart.GetLength(0));
      }

      // Stop if no points
      if (lngPointCount == 0)
      {
        dblPoint1X = double.NaN;
        dblPoint1Y = double.NaN;
        dblPoint2X = double.NaN;
        dblPoint2Y = double.NaN;
        dblDistance = double.NaN;
        return;
      }

      // Put points into single 2D array
      double[,] dblCoords = new double[lngPointCount, 2];
      foreach (double[,] dblRing in dblPolylinesOrRings)
      {
        for (int i = 0; i < dblRing.GetLength(0); i++)
        {
          lngCounter++;
          dblCoords[lngCounter, 0] = dblRing[i, 0];
          dblCoords[lngCounter, 1] = dblRing[i, 1];
        }
      }

      dblPoint1X = dblCoords[0, 0];
      dblPoint1Y = dblCoords[0, 1];
      dblPoint2X = dblCoords[0, 0];
      dblPoint2Y = dblCoords[0, 1];

      // Stop if only one point
      if (lngPointCount == 1)
      {
        dblDistance = 0;
        return;
      }

      // Run through version of function designed for multipoint
      CalcFarthestPointsNumbers(dblCoords, jenMethod, out dblPoint1X, out dblPoint1Y, out dblPoint2X, out dblPoint2Y, out dblDistance,
        out dblAz1, out dblAz2, out dblReverseAz1, out dblReverseAz2,dblEquatorialRadius, dblPolarRadius);

      //double[][,] dblPolygonRings = new double[4][,]
      //{
      //new double[10,2] {  // Exterior Ring
      //  {-111.58015220600000, 35.25729186900000},
      //  {-111.58038476300000, 35.25718009700010},
      //  {-111.58069137100000, 35.25722647800010},
      //  {-111.58076199000000, 35.25741455400000},
      //  {-111.58069007100000, 35.25755546200000},
      //  {-111.58047924100000, 35.25764619500010},
      //  {-111.58030781200000, 35.25763804600010},
      //  {-111.58015798400000, 35.25759091300010},
      //  {-111.58015062800000, 35.25758197600000},
      //  {-111.58015220600000, 35.25729186900000}},
      //new double[6,2] {  // Exterior Ring
      //  {-111.58160260800000, 35.25769463400010},
      //  {-111.58160175300000, 35.25757501000000},
      //  {-111.58182062300000, 35.25758891100000},
      //  {-111.58183220200000, 35.25767857700000},
      //  {-111.58172324800000, 35.25773891500010},
      //  {-111.58160260800000, 35.25769463400010}},
      //new double[9,2] {  // Exterior Ring
      //  {-111.58140122700000, 35.25808738500010},
      //  {-111.58110423000000, 35.25785553600010},
      //  {-111.58107601800000, 35.25747884200010},
      //  {-111.58141924000000, 35.25722654800010},
      //  {-111.58223087200000, 35.25734169900010},
      //  {-111.58230959900000, 35.25764338300010},
      //  {-111.58209312400000, 35.25796443100010},
      //  {-111.58180585500000, 35.25807347800000},
      //  {-111.58140122700000, 35.25808738500010}},
      //new double[8,2] {  // Interior Ring
      //  {-111.58208719000000, 35.25764445300010},
      //  {-111.58188138700000, 35.25741814800010},
      //  {-111.58144770100000, 35.25744714900000},
      //  {-111.58132107200000, 35.25758533100000},
      //  {-111.58144710900000, 35.25787482400010},
      //  {-111.58176210500000, 35.25791675700000},
      //  {-111.58201553100000, 35.25782124900010},
      //  {-111.58208719000000, 35.25764445300010}}
      //};

      //double dblPoint1X;
      //double dblPoint1Y;
      //double dblPoint2X;
      //double dblPoint2Y;
      //double dblDistance;
      //double dblAz1;
      //double dblAz2;
      //double dblReverseAz1;
      //double dblReverseAz2;

      //CalcFarthestPointsNumbers(dblPolygonRings, JenSphericalMethod.ENUM_UseSpherical, out dblPoint1X, out dblPoint1Y, out dblPoint2X, out dblPoint2Y, out dblDistance,
      //  out dblAz1, out dblAz2, out dblReverseAz1, out dblReverseAz2);
      //Console.WriteLine("Spherical");
      //Console.WriteLine("Distance from [" + dblPoint1X.ToString("0.00000") + ", " + dblPoint1Y.ToString("0.00000") + "] to ["
      //  + dblPoint2X.ToString("0.00000") + ", " + dblPoint2Y.ToString("0.00000") + "] = " + dblDistance.ToString("0.0000000000"));
      //Console.WriteLine("Azimuth 1 = " + dblAz1.ToString("0.0000000000") + ", Azimuth 2 = " + dblAz2.ToString("0.0000000000"));
      //Console.WriteLine("Reverse Azimuth 1 = " + dblReverseAz1.ToString("0.0000000000") + ", Reverse Azimuth 2 = " + dblReverseAz2.ToString("0.0000000000"));
      //CalcFarthestPointsNumbers(dblPolygonRings, JenSphericalMethod.ENUM_UseSpheroidal, out dblPoint1X, out dblPoint1Y, out dblPoint2X, out dblPoint2Y, out dblDistance,
      //  out dblAz1, out dblAz2, out dblReverseAz1, out dblReverseAz2);
      //Console.WriteLine("\nSpheroidal");
      //Console.WriteLine("Distance from [" + dblPoint1X.ToString("0.00000") + ", " + dblPoint1Y.ToString("0.00000") + "] to ["
      //  + dblPoint2X.ToString("0.00000") + ", " + dblPoint2Y.ToString("0.00000") + "] = " + dblDistance.ToString("0.0000000000"));
      //Console.WriteLine("Azimuth 1 = " + dblAz1.ToString("0.0000000000") + ", Azimuth 2 = " + dblAz2.ToString("0.0000000000"));
      //Console.WriteLine("Reverse Azimuth 1 = " + dblReverseAz1.ToString("0.0000000000") + ", Reverse Azimuth 2 = " + dblReverseAz2.ToString("0.0000000000"));
      //CalcFarthestPointsNumbers(dblPolygonRings, JenSphericalMethod.ENUM_UseTrigonometry, out dblPoint1X, out dblPoint1Y, out dblPoint2X, out dblPoint2Y, out dblDistance,
      //  out dblAz1, out dblAz2, out dblReverseAz1, out dblReverseAz2);
      //Console.WriteLine("\nTrigonometric");
      //Console.WriteLine("Distance from [" + dblPoint1X.ToString("0.00000") + ", " + dblPoint1Y.ToString("0.00000") + "] to ["
      //  + dblPoint2X.ToString("0.00000") + ", " + dblPoint2Y.ToString("0.00000") + "] = " + dblDistance.ToString("0.0000000000"));
      //Console.WriteLine("Azimuth 1 = " + dblAz1.ToString("0.0000000000") + ", Azimuth 2 = " + dblAz2.ToString("0.0000000000"));
      //Console.WriteLine("Reverse Azimuth 1 = " + dblReverseAz1.ToString("0.0000000000") + ", Reverse Azimuth 2 = " + dblReverseAz2.ToString("0.0000000000"));
    }

    ///<summary>
    ///Given two sets of projected coordinates, returns distance from first point to second in coordinate system units<br/><br/>Returns double value
    ///</summary>
    public static double DistancePythagoreanNumbers(double dblX1, double dblY1, double dblX2, double dblY2)
    {
      return Math.Sqrt(Math.Pow(dblX2 - dblX1, 2) + Math.Pow(dblY2 - dblY1, 2));
    }

    ///<summary>
    ///Given Latitude/Longitude coordinates, a bearing and a distance, and number of vertices, returns destination coordinates<br/>
    ///and destination bearing.  If spheroid not specified, assumes WGS 1984.
    ///<br/><br/>Returns doubles for Latitude, Longitude, bearing and coordinate array.
    ///</summary>
    public static void PointLineVincentyNumbers(double dblOriginX, double dblOriginY, double dblLength, double dblBeginningAzimuth,
      out double dblDestX, out double dblDestY,
      double dblEquatorialRadius = 6378137.000, double dblPolarRadius = 6356752.31424518)
    {
      PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, dblLength, dblBeginningAzimuth, out dblDestX, out dblDestY, out _,
          dblEquatorialRadius, dblPolarRadius);

      //double dblOriginX = -111.58208719;
      //double dblOriginY = 35.2576444530001;
      //double dblDestX;
      //double dblDestY;
      //double dblDestBearing;
      //double[,] dblCoords;
      //PointLineVincentyNumbers(dblOriginX, dblOriginY, 123456, 123.45, out dblDestX, out dblDestY, out dblDestBearing, out dblCoords, 7);
      //Console.WriteLine("...[" + dblDestX.ToString("0.0000000") + ", " + dblDestY.ToString("0.0000000") + "], New Bearing = " + dblDestBearing.ToString("0.0000000") + "]");

      //for (int i = 0; i < dblCoords.GetLength(0); i++)
      //{
      //  Console.WriteLine(i.ToString("0") + "...[" + dblCoords[i, 0].ToString("0.0000000") + ", " + dblCoords[i, 1].ToString("0.0000000") + "]");
      //}
    }
    ///<summary>
    ///Given Latitude/Longitude coordinates, a bearing and a distance, returns destination coordinates<br/>
    ///and destination bearing.  If spheroid not specified, assumes WGS 1984.
    ///<br/><br/>Returns doubles for Latitude, Longitude, and bearing.
    ///</summary>
    public static void PointLineVincentyNumbers(double dblOriginX, double dblOriginY, double dblLength, double dblBeginningAzimuth,
      out double dblDestX, out double dblDestY, out double dblDestAZ,
      double dblEquatorialRadius = 6378137.000, double dblPolarRadius = 6356752.31424518)
    {
      PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, dblLength, dblBeginningAzimuth, out dblDestX, out dblDestY, out dblDestAZ,
          dblEquatorialRadius, dblPolarRadius);

      //double dblOriginX = -111.58208719;
      //double dblOriginY = 35.2576444530001;
      //double dblDestX;
      //double dblDestY;
      //double dblDestBearing;
      //double[,] dblCoords;
      //PointLineVincentyNumbers(dblOriginX, dblOriginY, 123456, 123.45, out dblDestX, out dblDestY, out dblDestBearing, out dblCoords, 7);
      //Console.WriteLine("...[" + dblDestX.ToString("0.0000000") + ", " + dblDestY.ToString("0.0000000") + "], New Bearing = " + dblDestBearing.ToString("0.0000000") + "]");

      //for (int i = 0; i < dblCoords.GetLength(0); i++)
      //{
      //  Console.WriteLine(i.ToString("0") + "...[" + dblCoords[i, 0].ToString("0.0000000") + ", " + dblCoords[i, 1].ToString("0.0000000") + "]");
      //}
    }
    ///<summary>
    ///Given Latitude/Longitude coordinates, a bearing and a distance, and number of vertices, returns destination coordinates, <br/>
    ///destination bearing and an array of intermediate coordinates.  If spheroid not specified, assumes WGS 1984.
    ///<br/><br/>Returns doubles for Latitude, Longitude, bearing and coordinate array.
    ///</summary>
    public static void PointLineVincentyNumbers(double dblOriginX, double dblOriginY, double dblLength, double dblBeginningAzimuth,
      out double dblDestX, out double dblDestY, out double dblDestAZ, out double[][,] dblVertices, long lngNumVertices = 1,
      double dblEquatorialRadius = 6378137.000, double dblPolarRadius = 6356752.31424518)
    {
      double dblShort;
      double[,] dblTemp;

      if (lngNumVertices > 1)
      {
        dblShort = dblLength / Convert.ToDouble(lngNumVertices - 1);
        dblTemp = new double[lngNumVertices, 2];
        dblTemp[0, 0] = dblOriginX;
        dblTemp[0, 1] = dblOriginY;
        for (int i = 1; i < (lngNumVertices - 1); i++)
        {
          PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, i * dblShort, dblBeginningAzimuth, out double dblLongitude, out double dblLatitude, out _,
            dblEquatorialRadius, dblPolarRadius);
          dblTemp[i, 0] = dblLongitude;
          dblTemp[i, 1] = dblLatitude;
        }
        PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, dblLength, dblBeginningAzimuth, out dblDestX, out dblDestY, out dblDestAZ,
            dblEquatorialRadius, dblPolarRadius);
        dblTemp[lngNumVertices - 1, 0] = dblDestX;
        dblTemp[lngNumVertices - 1, 1] = dblDestY;
      }
      else
      {
        dblTemp = new double[1, 2];
        PointLineVincentyPerPointNumbers(dblOriginX, dblOriginY, dblLength, dblBeginningAzimuth, out dblDestX, out dblDestY, out dblDestAZ,
            dblEquatorialRadius, dblPolarRadius);
        dblTemp[0, 0] = dblDestX;
        dblTemp[0, 1] = dblDestY;
      }
      //Output Polyline should be in Polyline form (jagged array)
      dblVertices = new double[1][,];
      dblVertices[0] = dblTemp;

      //double dblOriginX = -111.58208719;
      //double dblOriginY = 35.2576444530001;
      //double dblDestX;
      //double dblDestY;
      //double dblDestBearing;
      //double[,] dblCoords;
      //PointLineVincentyNumbers(dblOriginX, dblOriginY, 123456, 123.45, out dblDestX, out dblDestY, out dblDestBearing, out dblCoords, 7);
      //Console.WriteLine("...[" + dblDestX.ToString("0.0000000") + ", " + dblDestY.ToString("0.0000000") + "], New Bearing = " + dblDestBearing.ToString("0.0000000") + "]");

      //for (int i = 0; i < dblCoords.GetLength(0); i++)
      //{
      //  Console.WriteLine(i.ToString("0") + "...[" + dblCoords[i, 0].ToString("0.0000000") + ", " + dblCoords[i, 1].ToString("0.0000000") + "]");
      //}
    }

    ///<summary>
    ///Given double array containing projected coordinates, returns the centroid coordinates.
    ///<br/><br/>Returns doubles for X and Y.
    ///</summary>
    public static void MultipointCentroid(double[,] dblCoordinates, out double dblCentroidX, out double dblCentroidY)
    {
      double dblRunningX = 0;
      double dblRunningY = 0;
      double dblCoordCount = dblCoordinates.GetLength(0);
      if (dblCoordCount == 0) { dblCentroidX = double.NaN; dblCentroidY = double.NaN; return; }
      for (int i = 0; i < dblCoordCount; i++)
      {
        dblRunningX += dblCoordinates[i, 0];
        dblRunningY += dblCoordinates[i, 1];
      }
      dblCentroidX = dblRunningX / dblCoordCount;
      dblCentroidY = dblRunningY / dblCoordCount;
    }
    ///<summary>
    ///Given double array containing Latitude/Longitude coordinates, returns the spherical centroid coordinates.
    ///<br/><br/>Returns doubles for Latitude and Longitude.
    ///</summary>
    public static void MultipointCentroidSphere(double[,] dblCoordinates, out double dblCentroidX, out double dblCentroidY)
    {
      double dblRunningX = 0;
      double dblRunningY = 0;
      double dblRunningZ = 0;
      double dblCoordCount = dblCoordinates.GetLength(0);
      if (dblCoordCount == 0) { dblCentroidX = double.NaN; dblCentroidY = double.NaN; return; }
      for (int i = 0; i < dblCoordCount; i++)
      {
        SphericalLatLongToCart(dblCoordinates[i, 0], dblCoordinates[i, 1], out double dblX, out double dblY, out double dblZ);
        dblRunningX += dblX;
        dblRunningY += dblY;
        dblRunningZ += dblZ;
      }
      SphericalCartToLatLong(out dblCentroidX, out dblCentroidY, dblRunningX /= dblCoordCount, dblRunningY /= dblCoordCount, dblRunningZ /= dblCoordCount);
    }
    ///<summary>
    ///Given double array containing Latitude/Longitude coordinates, returns the spheroidal centroid coordinates.
    ///If spheroid is not set, then will assume WGS 1984.<br/><br/>Returns doubles for Latitude, Longitude and Area.
    ///</summary>
    public static void MultipointCentroidSpheroid(double[,] dblCoordinates, out double dblCentroidX, out double dblCentroidY,
      double dblEquatorialRadius = 6378137.000, double dblPolarRadius = 6356752.31424518)
    {
      double dblRunningX = 0;
      double dblRunningY = 0;
      double dblRunningZ = 0;
      double dblCoordCount = dblCoordinates.GetLength(0);
      if (dblCoordCount == 0) { dblCentroidX = double.NaN; dblCentroidY = double.NaN; return; }
      for (int i = 0; i < dblCoordCount; i++)
      {
        SpheroidalLatLongToCart(dblCoordinates[i, 0], dblCoordinates[i, 1], out double dblX, out double dblY, out double dblZ, dblEquatorialRadius, dblPolarRadius);
        dblRunningX += dblX;
        dblRunningY += dblY;
        dblRunningZ += dblZ;
      }
      SpheroidalCartToLatLong(out dblCentroidX, out dblCentroidY, dblRunningX /= dblCoordCount, dblRunningY /= dblCoordCount, dblRunningZ /= dblCoordCount,
         dblEquatorialRadius, dblPolarRadius);
    }
    ///<summary>
    ///Given jagged double array containing polygon interior and exterior ring<br/>
    ///Latitude/Longitude coordinates, returns the polygon area in square meters.<br/>
    ///If spheroid is not set, then will assume WGS 1984.<br/><br/>Returns double for Area.
    ///</summary>
    public static double SphericalPolygonAreaNumbers(double[][,] dblPolygonRings,
      double dblEquatorialRadius = 6378137.000, double dblPolarRadius = 6356752.31424518)
    {
      double dblArea = 0;
      double dblTriangleArea;
      double dblMultiplier = 0;
      double dblLatA;
      double dblLongA;
      double dblLatB;
      double dblLongB;

      foreach (double[,] dblRing in dblPolygonRings)
      {
        MultipointCentroidSphere(dblRing, out double dblCentroidLongitude, out double dblCentroidLatitude);
        for (int i = 0; i < (dblRing.GetLength(0) - 1); i++)
        {
          dblLongA = dblRing[i, 0];
          dblLatA = dblRing[i, 1];
          dblLongB = dblRing[i + 1, 0];
          dblLatB = dblRing[i + 1, 1];
          dblTriangleArea = SphericalTriangleArea(dblLongA, dblLatA, dblLongB, dblLatB, dblCentroidLongitude, dblCentroidLatitude,
            ref dblMultiplier, dblEquatorialRadius, dblPolarRadius);

          dblArea += dblTriangleArea;
        }
      }
      return dblArea;
    }

    ///<summary>
    ///Given jagged double array containing polygon interior and exterior ring<br/>
    ///Latitude/Longitude coordinates, returns the centroid coordinates and the<br/>
    ///polygon area in square meters.<br/>
    ///If spheroid is not set, then will assume WGS 1984.<br/><br/>Returns doubles for Latitude and Longitude.
    ///</summary>
    public static double SphericalPolygonAreaNumbers(double[][,] dblPolygonRings, out double dblCentroidX, out double dblCentroidY,
      double dblEquatorialRadius = 6378137.000, double dblPolarRadius = 6356752.31424518)
    {
      double dblArea = 0;
      double dblTriangleArea;
      double dblMultiplier = 0;
      double dblLatA;
      double dblLongA;
      double dblLatB;
      double dblLongB;
      double dblRunningX = 0;
      double dblRunningY = 0;
      double dblRunningZ = 0;

      foreach (double[,] dblRing in dblPolygonRings)
      {
        MultipointCentroidSphere(dblRing, out double dblCentroidLongitude, out double dblCentroidLatitude);
        for (int i = 0; i < (dblRing.GetLength(0) - 1); i++)
        {
          dblLongA = dblRing[i, 0];
          dblLatA = dblRing[i, 1];
          dblLongB = dblRing[i + 1, 0];
          dblLatB = dblRing[i + 1, 1];
          dblTriangleArea = SphericalTriangleArea(dblLongA, dblLatA, dblLongB, dblLatB, dblCentroidLongitude, dblCentroidLatitude,
            ref dblMultiplier, dblEquatorialRadius, dblPolarRadius);

          dblArea += dblTriangleArea;

          // FOR POLYGON CENTROID
          SphericalLatLongToCart(dblLongA, dblLatA, out double dbl1X, out double dbl1Y, out double dbl1Z);
          SphericalLatLongToCart(dblLongB, dblLatB, out double dbl2X, out double dbl2Y, out double dbl2Z);
          SphericalLatLongToCart(dblCentroidLongitude, dblCentroidLatitude, out double dbl3X, out double dbl3Y, out double dbl3Z);
          TriangleCentroid3D(dbl1X, dbl1Y, dbl1Z, dbl2X, dbl2Y, dbl2Z, dbl3X, dbl3Y, dbl3Z,
                  out double dblTempCentX, out double dblTempCentY, out double dblTempCentZ);

          // NORMALIZE VECTOR
          double dblVectLength = Math.Sqrt(Math.Pow(dblTempCentX, 2) + Math.Pow(dblTempCentY, 2) + Math.Pow(dblTempCentZ, 2));
          dblTempCentX /= dblVectLength;
          dblTempCentY /= dblVectLength;
          dblTempCentZ /= dblVectLength;

          dblRunningX += (dblTempCentX * dblTriangleArea);
          dblRunningY += (dblTempCentY * dblTriangleArea);
          dblRunningZ += (dblTempCentZ * dblTriangleArea);
        }
      }

      if (dblArea > 0)
      {
        dblRunningX /= dblArea;
        dblRunningY /= dblArea;
        dblRunningZ /= dblArea;
        SphericalCartToLatLong(out dblCentroidX, out dblCentroidY, dblRunningX, dblRunningY, dblRunningZ);
      }
      else
      {
        dblCentroidX = double.NaN;
        dblCentroidY = double.NaN;
      }

      return dblArea;

      //double[][,] dblPolygonRings = new double[4][,]
      //{
      //new double[10,2] {  // Exterior Ring
      //  {-111.58015220600000, 35.25729186900000},
      //  {-111.58038476300000, 35.25718009700010},
      //  {-111.58069137100000, 35.25722647800010},
      //  {-111.58076199000000, 35.25741455400000},
      //  {-111.58069007100000, 35.25755546200000},
      //  {-111.58047924100000, 35.25764619500010},
      //  {-111.58030781200000, 35.25763804600010},
      //  {-111.58015798400000, 35.25759091300010},
      //  {-111.58015062800000, 35.25758197600000},
      //  {-111.58015220600000, 35.25729186900000}},
      //new double[6,2] {  // Exterior Ring
      //  {-111.58160260800000, 35.25769463400010},
      //  {-111.58160175300000, 35.25757501000000},
      //  {-111.58182062300000, 35.25758891100000},
      //  {-111.58183220200000, 35.25767857700000},
      //  {-111.58172324800000, 35.25773891500010},
      //  {-111.58160260800000, 35.25769463400010}},
      //new double[9,2] {  // Exterior Ring
      //  {-111.58140122700000, 35.25808738500010},
      //  {-111.58110423000000, 35.25785553600010},
      //  {-111.58107601800000, 35.25747884200010},
      //  {-111.58141924000000, 35.25722654800010},
      //  {-111.58223087200000, 35.25734169900010},
      //  {-111.58230959900000, 35.25764338300010},
      //  {-111.58209312400000, 35.25796443100010},
      //  {-111.58180585500000, 35.25807347800000},
      //  {-111.58140122700000, 35.25808738500010}},
      //new double[8,2] {  // Interior Ring
      //  {-111.58208719000000, 35.25764445300010},
      //  {-111.58188138700000, 35.25741814800010},
      //  {-111.58144770100000, 35.25744714900000},
      //  {-111.58132107200000, 35.25758533100000},
      //  {-111.58144710900000, 35.25787482400010},
      //  {-111.58176210500000, 35.25791675700000},
      //  {-111.58201553100000, 35.25782124900010},
      //  {-111.58208719000000, 35.25764445300010}}
      //};
      //double dblCentroidX;
      //double dblCentroidY;
      //double dblArea = SphericalPolygonAreaNumbers(dblPolygonRings, out dblCentroidX, out dblCentroidY);
      //Console.WriteLine("...[" + dblCentroidX.ToString("0.0000000000") + ", " + dblCentroidY.ToString("0.0000000000") + "], Area = " + dblArea.ToString("0.0000000000") + "]");
    }

    ///<summary>
    ///Given jagged double array containing polyline or polygon ring Latitude/Longitude coordinates, and a specified distance<br/>
    ///returns the coordinates of the point that distance along the polygon.<br/>
    ///If spheroid is not set, then will assume WGS 1984.<br/><br/>Returns doubles for Latitude and Longitude.
    ///</summary>
    public static double SpheroidalPolylineLength(double[][,] dblPolylineCoords,
      double dblEquatorialRadius = 6378137, double dblPolarRadius = 6356752.31424518)
    {

      //double[][,] dblPolylineCoords = new double[][,]
      //{
      //    new double[,] {{-111.5792848779150,35.2564468282811},
      //        {-111.5795802546250,35.2564726359222},
      //        {-111.5797645083180,35.2566812069286},
      //        {-111.5802474065620,35.2567891329665},
      //        {-111.5807100345440,35.2570696026123},
      //        {-111.5807769525830,35.2571101731051},
      //        {-111.5810839820180,35.2572172265369},
      //        {-111.5813069619190,35.2572199336351},
      //        {-111.5813803577360,35.2572208253539},
      //        {-111.5814081724890,35.2572211605914},
      //        {-111.5820575462550,35.2571080325312},
      //        {-111.5821408024270,35.2570842872676},
      //        {-111.5835951174160,35.2566695377509},
      //        {-111.5835991326170,35.2566683912734},
      //        {-111.5836766722550,35.2566462775342},
      //        {-111.5836804179320,35.2566504703598},
      //        {-111.5848734771490,35.2579840168962}}
      //};
      //Console.WriteLine(dblPolylineCoords[0][3, 1]);
      //Console.WriteLine(dblPolylineCoords.Length);
      //Console.WriteLine(dblPolylineCoords.GetLength(0));
      //Console.WriteLine(dblPolylineCoords[0].GetLength(0));
      //Console.WriteLine(dblPolylineCoords[0].GetLength(1));

      //double dblLength;
      //double dblOutX;
      //double dblOutY;
      //SpheroidalPolylineMidpointNumbers(dblPolylineCoords, 300, false, out dblOutX, out dblOutY, out dblLength);
      //Console.WriteLine("...[" + dblOutX.ToString("0.000000") + ", " + dblOutY.ToString("0.000000") + "], Length = " + dblLength.ToString("0.000000") + "]");


      double dblStartLat;
      double dblStartLong;
      double dblEndLat;
      double dblEndLong;
      double dblLength;
      double dblCumulativeLength = 0;

      foreach (double[,] dblPolylinePart in dblPolylineCoords)
      {
        dblStartLong = dblPolylinePart[0, 0];
        dblStartLat = dblPolylinePart[0, 1];
        for (int i = 1; i < dblPolylinePart.GetLength(0); i++)
        {
          dblEndLong = dblPolylinePart[i, 0];
          dblEndLat = dblPolylinePart[i, 1];
          dblLength = DistanceVincentyNumbers(dblStartLong, dblStartLat, dblEndLong, dblEndLat, dblEquatorialRadius, dblPolarRadius);
          dblCumulativeLength += dblLength;
          dblStartLong = dblEndLong;
          dblStartLat = dblEndLat;
        }
      }
      return dblCumulativeLength;

    }
    public static void PointLineVincentyPerPointNumbers(double dblOriginLong, double dblOriginLat, double dblLength,
      double dblAzimuth, out double dblDestLong, out double dblDestLat, out double dblDestAz,
      double dblEquatorialRadius = 6378137.000, double dblPolarRadius = 6356752.31424518)
    {

      // ASSUMES pPoint IS GEOGRAPHIC
      // ADAPTED FROM Vincenty, T. (1975). “Direct and inverse solutions of geodesics on the
      //                                    ellipsoid with application of nested equations.” Surv. Rev., XXII(176),
      //                                    88–93.
      // ADAPTED FROM CHRIS VENESS; http://www.movable-type.co.uk/scripts/latlong-vincenty.html


      // POINT 1 = dblOriginLong, dblOriginLat
      // POINT 2 = dblQX, dblQY
      //Dim dblOriginLong As Double
      //dblOriginLong = pPoint.X
      //Dim dblOriginLat As Double
      //dblOriginLat = pPoint.Y


      if (dblLength == 0)  // SAME POINT
      {
        dblDestLat = dblOriginLat;
        dblDestLong = dblOriginLong;
        dblDestAz = dblAzimuth;
        return;
      }

      double A = dblEquatorialRadius;   // SPHEROID; EQUATORIAL RADIUS
      double B = dblPolarRadius;        // SPHEROID; POLAR RADIUS
      double dblf = (A - B) / A;           // FLATTENING

      double dblTanU1 = (1 - dblf) * (Math.Tan(DegToRad(dblOriginLat)));
      double U1 = Math.Atan(dblTanU1);
      double dblCosU1 = Math.Cos(U1);
      double dblSinU1 = Math.Sin(U1);

      double dblS = dblLength;

      double cosAlpha1 = Math.Cos(DegToRad(dblAzimuth));
      double sinAlpha1 = Math.Sin(DegToRad(dblAzimuth));
      //double tanSigma1 = dblTanU1 / cosAlpha1;                                                                        // [1]
      double Sigma1 = Math.Atan2(dblTanU1, cosAlpha1);
      double sinAlpha = dblCosU1 * sinAlpha1;                                                                         // [2]
      double cosSqAlpha = 1 - Math.Pow(sinAlpha, 2);                                                                 // TRIG IDENTITY
      //double cosAlpha = Math.Sqrt(cosSqAlpha);

      double uSq = (cosSqAlpha * (Math.Pow(A, 2) - Math.Pow(B, 2))) / Math.Pow(B, 2);
      double dblA1 = (uSq * (-768 + (uSq * (320 - (175 * uSq)))));
      double dblA = 1 + ((uSq / 16384) * (4096 + dblA1));                                                             // [3]
      double dblB1 = (uSq * (-128 + (uSq * (74 - (uSq * 47)))));
      double dblB = (uSq / 1024) * (256 + dblB1);                                                                     // [4]

      //Dim Sigma As Double
      double sinSigma;
      double cosSigma;
      double DeltaSigma;
      double DeltaSigma1;
      double DeltaSigma2;
      double DeltaSigma3;
      double cos2SigmaM;

      //Dim lngIterations As Long
      long lngIterations = 40;

      //Dim SigmaCompare As Double
      double SigmaCompare = 2 * Math.PI;
      double dblSigma = dblS / (B * dblA);                  // FIRST ESTIMATION

      while ((Math.Abs(dblSigma - SigmaCompare) > 0.000000000001) && (lngIterations > 0))
      {
        cos2SigmaM = Math.Cos(2 * Sigma1 + dblSigma);                                                                 // [5]
        sinSigma = Math.Sin(dblSigma);
        cosSigma = Math.Cos(dblSigma);
        DeltaSigma1 = ((dblB / 6) * cos2SigmaM * (-3 + 4 * Math.Pow(sinSigma, 2)) * (-3 + 4 * Math.Pow(cos2SigmaM, 2)));
        DeltaSigma2 = ((dblB / 4) * (cosSigma * (-1 + 2 * Math.Pow(cos2SigmaM, 2)) - DeltaSigma1));
        DeltaSigma3 = cos2SigmaM + DeltaSigma2;
        DeltaSigma = dblB * sinSigma * DeltaSigma3;                                                                   // [6]
        SigmaCompare = dblSigma;
        dblSigma = (dblS / (B * dblA)) + DeltaSigma;                                                                  // [7]

        lngIterations--;
      }

      if (Math.Abs(dblSigma - SigmaCompare) > 0.000000000001)   // failed to converge within the iteration limit
      {
        dblDestLat = double.NaN;
        dblDestLong = double.NaN;
        dblDestAz = double.NaN;
        return;
      }

      cos2SigmaM = Math.Cos(2 * Sigma1 + dblSigma);
      sinSigma = Math.Sin(dblSigma);
      cosSigma = Math.Cos(dblSigma);
      //Dim dblLat2Denom As Double
      //Dim dblLat2Temp As Double
      double dblLat2Temp = dblSinU1 * sinSigma - dblCosU1 * cosSigma * cosAlpha1;
      double dblLat2Denom = (1 - dblf) * (Math.Sqrt(Math.Pow(sinAlpha, 2) + Math.Pow(dblLat2Temp, 2)));

      // CALCULATE LATITUDE FOR NEW POINT
      //Dim dblLat2 As Double
      double dblLat2 = Math.Atan2(dblSinU1 * cosSigma + dblCosU1 * sinSigma * cosAlpha1, dblLat2Denom);               // [8]

      // CALCULATE LONGITUDE FOR NEW POINT
      //Dim dblLambda As Double
      //Dim dblLambda1 As Double
      //Dim dblLambda1a As Double
      double dblLambda = Math.Atan2(sinSigma * sinAlpha1, dblCosU1 * cosSigma - dblSinU1 * sinSigma * cosAlpha1);     // [9]
      double C = (dblf / 16) * cosSqAlpha * (4 + (dblf * (4 - (3 * cosSqAlpha))));                                    // [10]
      double dblLambda1 = cos2SigmaM + C * cosSigma * (-1 + 2 * Math.Pow(cos2SigmaM, 2));
      double dblLambda1a = C * sinSigma * dblLambda1;
      //Dim dblLambda2 As Double
      double dblLambda2 = dblSigma + dblLambda1a;
      double dblFinalLongRadians = dblLambda - ((1 - C) * dblf * sinAlpha * dblLambda2);                              // [11]

      dblDestLong = dblOriginLong + RadToDeg(dblFinalLongRadians);
      dblDestLat = RadToDeg(dblLat2);

      dblDestAz = RadToDeg(Math.Atan2(sinAlpha, -dblLat2Temp));
      if (dblDestAz < 0) { dblDestAz = 360 + dblDestAz; }
    }

    ///<summary>
    ///Given jagged double array containing polyline Latitude/Longitude coordinates, and a specified distance<br/>
    ///returns the coordinates of the point that distance along the polygon.<br/>
    ///If spheroid is not set, then will assume WGS 1984.<br/><br/>Returns doubles for Latitude and Longitude.
    ///</summary>
    public static void SpheroidalPolylineMidpointNumbers(double[][,] dblPolylineCoords, double dblDistance, bool booIsRatio,
      out double dblPointX, out double dblPointY,
      double dblEquatorialRadius = 6378137, double dblPolarRadius = 6356752.31424518)
    {
            //double[][,] dblPolylineCoords = new double[][,]
            //{
            //    new double[,] {{-111.5792848779150,35.2564468282811},
            //        {-111.5795802546250,35.2564726359222},
            //        {-111.5797645083180,35.2566812069286},
            //        {-111.5802474065620,35.2567891329665},
            //        {-111.5807100345440,35.2570696026123},
            //        {-111.5807769525830,35.2571101731051},
            //        {-111.5810839820180,35.2572172265369},
            //        {-111.5813069619190,35.2572199336351},
            //        {-111.5813803577360,35.2572208253539},
            //        {-111.5814081724890,35.2572211605914},
            //        {-111.5820575462550,35.2571080325312},
            //        {-111.5821408024270,35.2570842872676},
            //        {-111.5835951174160,35.2566695377509},
            //        {-111.5835991326170,35.2566683912734},
            //        {-111.5836766722550,35.2566462775342},
            //        {-111.5836804179320,35.2566504703598},
            //        {-111.5848734771490,35.2579840168962}}
            //};
            //Console.WriteLine(dblPolylineCoords[0][3, 1]);
            //Console.WriteLine(dblPolylineCoords.Length);
            //Console.WriteLine(dblPolylineCoords.GetLength(0));
            //Console.WriteLine(dblPolylineCoords[0].GetLength(0));
            //Console.WriteLine(dblPolylineCoords[0].GetLength(1));

            //double dblLength;
            //double dblOutX;
            //double dblOutY;
            //SpheroidalPolylineMidpointNumbers(dblPolylineCoords, 300, false, out dblOutX, out dblOutY, out dblLength);
            //Console.WriteLine("...[" + dblOutX.ToString("0.000000") + ", " + dblOutY.ToString("0.000000") + "], Length = " + dblLength.ToString("0.000000") + "]");

            //            1.Use double.IsNaN for NaN Checks
            //Instead of dblLength != double.NaN, use!double.IsNaN(dblLength) for correct NaN comparison.
            //2.Reduce Redundant Assignments
            //You do not need to set dblLength, dblAz1, dblStartLong, and dblStartLat to double.NaN before the search loop. They will be set if a segment is found.
            //3.Use Early Return for Edge Cases
            //If the polyline is empty or has no segments, return early with NaN outputs.
            //4.Improve Variable Naming and Comments
            //Clarify variable names and add concise comments for maintainability.
            //5.Avoid Magic Numbers
            //Use named constants for array indices in dblSegStats for readability.
            //6.Consider Using a Struct or Class for Segment Stats



      double dblStartLat;
      double dblStartLong;
      double dblEndLat;
      double dblEndLong;
      double dblLength;
      double dblCumulativeLength = 0;
      long lngSegCount = 0;
      double dblAz1;
      double dblPolylineLength;

      // Get total number of segments
      foreach (double[,] dblPolylinePart in dblPolylineCoords)
      {
        lngSegCount += (dblPolylinePart.GetLength(0) - 1);
      }

      double[,] dblSegStats = new double[5, lngSegCount];
      long lngSegIndex = -1;
      foreach (double[,] dblPolylinePart in dblPolylineCoords)
      {
        dblStartLong = dblPolylinePart[0, 0];
        dblStartLat = dblPolylinePart[0, 1];
        for (int i = 1; i < dblPolylinePart.GetLength(0); i++)
        {
          dblEndLong = dblPolylinePart[i, 0];
          dblEndLat = dblPolylinePart[i, 1];
          dblLength = DistanceVincentyNumbers(dblStartLong, dblStartLat, dblEndLong, dblEndLat, out dblAz1, out double dblAz2, dblEquatorialRadius, dblPolarRadius);
          dblCumulativeLength += dblLength;
          lngSegIndex++;
          dblSegStats[0, lngSegIndex] = dblLength;
          dblSegStats[1, lngSegIndex] = dblCumulativeLength;
          dblSegStats[2, lngSegIndex] = dblAz1;
          dblSegStats[3, lngSegIndex] = dblStartLong;
          dblSegStats[4, lngSegIndex] = dblStartLat;
          //Debug.Print ("...Segment " + lngSegIndex + ": Segment Length = " + dblLength + ", Cumulative Length = " + dblCumulativeLength);
          dblStartLong = dblEndLong;
          dblStartLat = dblEndLat;
        }
      }
      dblPolylineLength = dblCumulativeLength;

      double dblHalfLength;
      if (booIsRatio)  // Then dblDistance should be a ratio between 0 and 1
      {
        dblDistance = dblDistance > 1 ? 1 : dblDistance;
        dblDistance = dblDistance < 0 ? 0 : dblDistance;
        dblHalfLength = dblPolylineLength * dblDistance;
      }
      else { dblHalfLength = dblDistance; }
      // Now dblHalfLength is actual distance, regardless of whether we specify ratio or not.

      dblLength = double.NaN;
      dblAz1 = double.NaN;
      dblStartLong = double.NaN;
      dblStartLat = double.NaN;

      for (int i = 0; i < lngSegCount; i++)
      {
        dblCumulativeLength = dblSegStats[1, i];
        if (dblCumulativeLength > dblHalfLength)
        {
          dblLength = dblSegStats[0, i];
          dblAz1 = dblSegStats[2, i];
          dblStartLong = dblSegStats[3, i];
          dblStartLat = dblSegStats[4, i];
          break;
        }
      }

      if (!double.IsNaN(dblLength))
      {
        double dblProperDistance = dblLength - (dblCumulativeLength - dblHalfLength);
        PointLineVincentyPerPointNumbers(dblStartLong, dblStartLat, dblProperDistance, dblAz1, out dblPointX, out dblPointY, out _, dblEquatorialRadius, dblPolarRadius);
      }
      else
      {
        dblPointX = double.NaN;
        dblPointY = double.NaN;
      }
      //Debug.Print("...dblPointX " + dblPointX + ", dblPointY = " + dblPointY + ":   dblAz1 = " + dblAz1);
    }
    ///<summary>
    ///Given jagged double array containing polyline Latitude/Longitude coordinates, and a specified distance<br/>
    ///returns the coordinates of the point that distance along the polygon.<br/>
    ///If spheroid is not set, then will assume WGS 1984.<br/><br/>Returns doubles for Latitude and Longitude, and the total length of the polyline.
    ///</summary>
    public static void SpheroidalPolylineMidpointNumbers(double[][,] dblPolylineCoords, double dblDistance, bool booIsRatio,
      out double dblPointX, out double dblPointY, out double dblPolylineLength,
      double dblEquatorialRadius = 6378137, double dblPolarRadius = 6356752.31424518)
    {
      //double[][,] dblPolylineCoords = new double[][,]
      //{
      //    new double[,] {{-111.5792848779150,35.2564468282811},
      //        {-111.5795802546250,35.2564726359222},
      //        {-111.5797645083180,35.2566812069286},
      //        {-111.5802474065620,35.2567891329665},
      //        {-111.5807100345440,35.2570696026123},
      //        {-111.5807769525830,35.2571101731051},
      //        {-111.5810839820180,35.2572172265369},
      //        {-111.5813069619190,35.2572199336351},
      //        {-111.5813803577360,35.2572208253539},
      //        {-111.5814081724890,35.2572211605914},
      //        {-111.5820575462550,35.2571080325312},
      //        {-111.5821408024270,35.2570842872676},
      //        {-111.5835951174160,35.2566695377509},
      //        {-111.5835991326170,35.2566683912734},
      //        {-111.5836766722550,35.2566462775342},
      //        {-111.5836804179320,35.2566504703598},
      //        {-111.5848734771490,35.2579840168962}}
      //};
      //Console.WriteLine(dblPolylineCoords[0][3, 1]);
      //Console.WriteLine(dblPolylineCoords.Length);
      //Console.WriteLine(dblPolylineCoords.GetLength(0));
      //Console.WriteLine(dblPolylineCoords[0].GetLength(0));
      //Console.WriteLine(dblPolylineCoords[0].GetLength(1));

      //double dblLength;
      //double dblOutX;
      //double dblOutY;
      //SpheroidalPolylineMidpointNumbers(dblPolylineCoords, 300, false, out dblOutX, out dblOutY, out dblLength);
      //Console.WriteLine("...[" + dblOutX.ToString("0.000000") + ", " + dblOutY.ToString("0.000000") + "], Length = " + dblLength.ToString("0.000000") + "]");
      double dblStartLat;
      double dblStartLong;
      double dblEndLat;
      double dblEndLong;
      double dblLength;
      double dblCumulativeLength = 0;
      long lngSegCount = 0;
      double dblAz1;

      // Get total number of segments
      foreach (double[,] dblPolylinePart in dblPolylineCoords)
      {
        lngSegCount += (dblPolylinePart.GetLength(0) - 1);
      }

      double[,] dblSegStats = new double[5, lngSegCount];
      long lngSegIndex = -1;
      foreach (double[,] dblPolylinePart in dblPolylineCoords)
      {
        dblStartLong = dblPolylinePart[0, 0];
        dblStartLat = dblPolylinePart[0, 1];
        for (int i = 1; i < dblPolylinePart.GetLength(0); i++)
        {
          dblEndLong = dblPolylinePart[i, 0];
          dblEndLat = dblPolylinePart[i, 1];
          dblLength = DistanceVincentyNumbers(dblStartLong, dblStartLat, dblEndLong, dblEndLat, out dblAz1, out double dblAz2, dblEquatorialRadius, dblPolarRadius);
          dblCumulativeLength += dblLength;
          lngSegIndex++;
          dblSegStats[0, lngSegIndex] = dblLength;
          dblSegStats[1, lngSegIndex] = dblCumulativeLength;
          dblSegStats[2, lngSegIndex] = dblAz1;
          dblSegStats[3, lngSegIndex] = dblStartLong;
          dblSegStats[4, lngSegIndex] = dblStartLat;
          //Debug.Print ("...Segment " + lngSegIndex + ": Segment Length = " + dblLength + ", Cumulative Length = " + dblCumulativeLength);
          dblStartLong = dblEndLong;
          dblStartLat = dblEndLat;
        }
      }
      dblPolylineLength = dblCumulativeLength;

      double dblHalfLength;
      if (booIsRatio)  // Then dblDistance should be a ratio between 0 and 1
      {
        dblDistance = dblDistance > 1 ? 1 : dblDistance;
        dblDistance = dblDistance < 0 ? 0 : dblDistance;
        dblHalfLength = dblPolylineLength * dblDistance;
      }
      else { dblHalfLength = dblDistance; }
      // Now dblHalfLength is actual distance, regardless of whether we specify ratio or not.

      dblLength = double.NaN;
      dblAz1 = double.NaN;
      dblStartLong = double.NaN;
      dblStartLat = double.NaN;

      for (int i = 0; i < lngSegCount; i++)
      {
        dblCumulativeLength = dblSegStats[1, i];
        if (dblCumulativeLength > dblHalfLength)
        {
          dblLength = dblSegStats[0, i];
          dblAz1 = dblSegStats[2, i];
          dblStartLong = dblSegStats[3, i];
          dblStartLat = dblSegStats[4, i];
          break;
        }
      }

      if (!double.IsNaN(dblLength))
      {
        double dblProperDistance = dblLength - (dblCumulativeLength - dblHalfLength);
        PointLineVincentyPerPointNumbers(dblStartLong, dblStartLat, dblProperDistance, dblAz1, out dblPointX, out dblPointY, out _, dblEquatorialRadius, dblPolarRadius);
      }
      else
      {
        dblPointX = double.NaN;
        dblPointY = double.NaN;
      }
      //Debug.Print("...dblPointX " + dblPointX + ", dblPointY = " + dblPointY + ":   dblAz1 = " + dblAz1);
    }

    ///<summary>
    ///Given Latitude and Longitude, and optionally a height above the ellipsoid, converts to Cartesian Coordinate System and returns X, Y and Z coordinates.<br/>
    ///If spheroid is not set, then will assume WGS 1984.<br/><br/>Returns doubles for Latitude and Longitude.
    ///</summary>
    public static void SpheroidalLatLongToCart(double dblLongitude, double dblLatitude, out double X, out double Y, out double Z,
      double dblEquatorialRadius = 6378137, double dblPolarRadius = 6356752.31424518, double dblHeightAboveEllipsoid = 0)
    {
      // IF SPHEROID PARAMETERS NOT INCLUDED, DEFAULTS TO WGS84
      // Phi is angle from north pole down to Latitude
      // Theta is angle from Greenwich

      // MODIFIED FROM J.C. ILIFFE, CHAPTER 2
      // NOTE:  ILIFFE USES PHI FOR LATITUDE DIRECTLY, RATHER THAN AS DISTANCE FROM POLES

      //double dblX;
      //double dblY;
      //double dblZ;
      //dblLongitude = -112.237;
      //dblLatitude = 35.123;
      //Console.WriteLine("\nSpheroidal:");
      //SpheroidalLatLongToCart(dblLongitude, dblLatitude, out dblX, out dblY, out dblZ);
      //Console.WriteLine("...[" + dblLongitude.ToString("0.000") + ", " + dblLatitude.ToString("0.000") +
      //    "] --> [" + dblX.ToString("0.00") + ", " + dblY.ToString("0.00") + ", " + dblZ.ToString("0.00") + "]");
      //SpheroidalCartToLatLong(out dblLongitude, out dblLatitude, dblX, dblY, dblZ);
      //Console.WriteLine("...[" + dblX.ToString("0.00") + ", " + dblY.ToString("0.00") + ", " + dblZ.ToString("0.00") + "]" +
      //  " --> [" + dblLongitude.ToString("0.000") + ", " + dblLatitude.ToString("0.000") + "]");

      double dblPhi = DegToRad(90 - dblLatitude);
      double dblTheta = DegToRad(dblLongitude);

      double dblEccentSquared = (Math.Pow(dblEquatorialRadius, 2) - Math.Pow(dblPolarRadius, 2)) / Math.Pow(dblEquatorialRadius, 2);
      double dblNu = dblEquatorialRadius / Math.Sqrt(1 - (dblEccentSquared * Math.Pow(Math.Cos(dblPhi), 2)));

      X = (dblNu + dblHeightAboveEllipsoid) * Math.Sin(dblPhi) * Math.Cos(dblTheta);
      Y = (dblNu + dblHeightAboveEllipsoid) * Math.Sin(dblPhi) * Math.Sin(dblTheta);
      Z = (((1 - dblEccentSquared) * dblNu) + dblHeightAboveEllipsoid) * Math.Cos(dblPhi);
    }
    ///<summary>
    ///Given three 3D coordinates, presumed to be on surface of spheroid, returns Latitude and Longitude.<br/>
    ///If no ellipsoid parameters specified, defaults to WGS 1984.<br/><br/>Returns doubles for Latitude and Longitude.
    ///</summary>
    public static void SpheroidalCartToLatLong(out double dblLongitude, out double dblLatitude, double X, double Y, double Z,
      double dblEquatorialRadius = 6378137, double dblPolarRadius = 6356752.31424518)
    {
      // IF SPHEROID PARAMETERS NOT INCLUDED, DEFAULTS TO WGS84
      // Phi is angle from north pole down to Latitude
      // Theta is angle from Greenwich

      // MODIFIED FROM J.C. ILIFFE, CHAPTER 2
      // NOTE:  ILIFFE USES PHI FOR LATITUDE DIRECTLY, RATHER THAN AS DISTANCE FROM POLES

      //double dblX;
      //double dblY;
      //double dblZ;
      //dblLongitude = -112.237;
      //dblLatitude = 35.123;
      //Console.WriteLine("\nSpheroidal:");
      //SpheroidalLatLongToCart(dblLongitude, dblLatitude, out dblX, out dblY, out dblZ);
      //Console.WriteLine("...[" + dblLongitude.ToString("0.000") + ", " + dblLatitude.ToString("0.000") +
      //    "] --> [" + dblX.ToString("0.00") + ", " + dblY.ToString("0.00") + ", " + dblZ.ToString("0.00") + "]");
      //SpheroidalCartToLatLong(out dblLongitude, out dblLatitude, dblX, dblY, dblZ);
      //Console.WriteLine("...[" + dblX.ToString("0.00") + ", " + dblY.ToString("0.00") + ", " + dblZ.ToString("0.00") + "]" +
      //  " --> [" + dblLongitude.ToString("0.000") + ", " + dblLatitude.ToString("0.000") + "]");

      double dblP = Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));
      double dblU = Math.Atan2((Z * dblEquatorialRadius), (dblP * dblPolarRadius));
      double dblEccentSquared = (Math.Pow(dblEquatorialRadius, 2) - Math.Pow(dblPolarRadius, 2)) / Math.Pow(dblEquatorialRadius, 2);
      double dblEpsilon = dblEccentSquared / (1 - dblEccentSquared);

      double dblPhi = Math.Atan2(dblP - (dblEccentSquared * dblEquatorialRadius * (Math.Pow(Math.Cos(dblU), 3))),
                  Z + (dblEpsilon * dblPolarRadius * Math.Pow(Math.Sin(dblU), 3)));
      double dblTheta = Math.Atan2(Y, X);

      dblLongitude = RadToDeg(dblTheta);
      dblLatitude = 90 - RadToDeg(dblPhi);
    }
    ///<summary>
    ///Given Latitude and Longitude, converts to Cartesian Coordinate System and returns X, Y and Z coordinates.<br/>
    ///If radius is not set, then will assume a unit sphere.<br/><br/>Returns doubles for Latitude and Longitude.
    ///</summary>
    public static void SphericalLatLongToCart(double dblLongitude, double dblLatitude, out double X, out double Y, out double Z, double dblRadius = 1)
    {
      // Phi is angle from north pole down to Latitude
      // Theta is angle from Greenwich

      //double dblX;
      //double dblY;
      //double dblZ;
      //dblLongitude = -112.237;
      //dblLatitude = 35.123;
      //Console.WriteLine("\nSpherical:");
      //SphericalLatLongToCart(dblLongitude, dblLatitude, out dblX, out dblY, out dblZ);
      //Console.WriteLine("...[" + dblLongitude.ToString("0.000") + ", " + dblLatitude.ToString("0.000") +
      //    "] --> [" + dblX.ToString("0.00") + ", " + dblY.ToString("0.00") + ", " + dblZ.ToString("0.00") + "]");
      //SphericalCartToLatLong(out dblLongitude, out dblLatitude, dblX, dblY, dblZ);
      //Console.WriteLine("...[" + dblX.ToString("0.00") + ", " + dblY.ToString("0.00") + ", " + dblZ.ToString("0.00") + "]" +
      //  " --> [" + dblLongitude.ToString("0.000") + ", " + dblLatitude.ToString("0.000"));

      double dblPhi = DegToRad(90 - dblLatitude);
      double dblTheta = DegToRad(dblLongitude);
      X = dblRadius * Math.Sin(dblPhi) * Math.Cos(dblTheta);
      Y = dblRadius * Math.Sin(dblPhi) * Math.Sin(dblTheta);
      Z = dblRadius * Math.Cos(dblPhi);
    }
    ///<summary>
    ///Given three 3D coordinates, presumed to be on surface of sphere, returns Latitude and Longitude.<br/><br/>Returns doubles for Latitude and Longitude.
    ///</summary>
    public static void SphericalCartToLatLong(out double dblLongitude, out double dblLatitude, double X, double Y, double Z)
    {
      // Phi is angle from north pole down to Latitude
      // Theta is angle from Greenwich

      //double dblX;
      //double dblY;
      //double dblZ;
      //dblLongitude = -112.237;
      //dblLatitude = 35.123;
      //Console.WriteLine("\nSpherical:");
      //SphericalLatLongToCart(dblLongitude, dblLatitude, out dblX, out dblY, out dblZ);
      //Console.WriteLine("...[" + dblLongitude.ToString("0.000") + ", " + dblLatitude.ToString("0.000") +
      //    "] --> [" + dblX.ToString("0.00") + ", " + dblY.ToString("0.00") + ", " + dblZ.ToString("0.00") + "]");
      //SphericalCartToLatLong(out dblLongitude, out dblLatitude, dblX, dblY, dblZ);
      //Console.WriteLine("...[" + dblX.ToString("0.00") + ", " + dblY.ToString("0.00") + ", " + dblZ.ToString("0.00") + "]" +
      //  " --> [" + dblLongitude.ToString("0.000") + ", " + dblLatitude.ToString("0.000"));

      double dblPhi = Math.Atan2(Math.Sqrt((Math.Pow(X, 2) + Math.Pow(Y, 2))), Z);
      double dblTheta = Math.Atan2(Y, X);

      dblLongitude = RadToDeg(dblTheta);
      dblLatitude = 90 - RadToDeg(dblPhi);
    }
    ///<summary>
    ///Given three 2D coordinates, returns 2D centroid.<br/><br/>Returns doubles for Centroid X, Y and Z coordinate.
    ///</summary>
    public static void TriangleCentroidPLane(double dblPX, double dblPY, double dblQX, double dblQY,
       double dblRX, double dblRY, out double dblCentX, out double dblCentY)
    {
      dblCentX = (dblPX + dblQX + dblRX) / 3d;
      dblCentY = (dblPY + dblQY + dblRY) / 3d;
    }
    ///<summary>
    ///Given three 3D coordinates, returns 3D centroid.<br/><br/>Returns doubles for Centroid X, Y and Z coordinate.
    ///</summary>
    public static void TriangleCentroid3D(double dblPX, double dblPY, double dblPZ, double dblQX, double dblQY, double dblQZ,
       double dblRX, double dblRY, double dblRZ, out double dblCentX, out double dblCentY, out double dblCentZ)
    {
      //double dblCentX;
      //double dblCentY;
      //double dblCentZ;
      //TriangleCentroid3D(12, 23, 34, 14, 27, 39, 10, 26, 33, out dblCentX, out dblCentY, out dblCentZ);

      dblCentX = (dblPX + dblQX + dblRX) / 3d;
      dblCentY = (dblPY + dblQY + dblRY) / 3d;
      dblCentZ = (dblPZ + dblQZ + dblRZ) / 3d;
    }
    ///<summary>
    ///Given a Lat/Long location, time of day and hours from Greenwich,<br/>
    ///calculates the sun position in the sky.<br/><br/>Returns doubles for Compass Direction and Angle Up.
    ///</summary>
    public static void SolarFunctions(double dblLatitude, double dblLongitude, DateTime datDateWithTime, double dblHoursFromGreenwich,
       out double dblSunDirection, out double dblSunAngleUp)
    {
      // ADAPTED FROM http://www.esrl.noaa.gov/gmd/grad/solcalc/
      // SAMPLE EXCEL FILE http://www.esrl.noaa.gov/gmd/grad/solcalc/NOAA_Solar_Calculations_day.xls
      // GLOSSARY OF TERMS AT http://www.esrl.noaa.gov/gmd/grad/solcalc/glossary.html
      // Sample Code at Bottom

      // VARIABLES BELOW ARE NAMED ACCORDING TO DESCRIPTION AND EXCEL COLUMN
      //Dim dblA As Double
      //Dim dblB As Double
      //Dim boo_W_Crashed As Boolean
      //Dim boo_Y_Crashed As Boolean
      //Dim boo_Z_Crashed As Boolean
      //Dim boo_AA_Crashed As Boolean

      //Dim dbl_E_Time_PastLocalMidnight As Double
      //Dim dbl_F_JulianDay As Double
      //Dim dbl_G_Julian_Century As Double
      //Dim dbl_I_Geom_Mean_Long_Sun_Deg As Double
      //Dim dbl_J_GeomMean_Anom_Sun_Deg As Double
      //Dim dbl_K_Eccent_Earth_Orbit As Double
      //Dim dbl_L_Sun_Eq_of_Ctr As Double
      //Dim dbl_M_Sun_True_Long_Deg As Double
      //Dim dbl_N_Sun_True_Anom_Deg As Double
      //Dim dbl_O_Sun_Rad_vector_AUs As Double
      //Dim dbl_P_Sun_App_Long_Deg As Double
      //Dim dbl_Q_Mean_Obliq_Ecliptic_Deg As Double
      //Dim dbl_R_Obliq_Corr_Deg As Double
      //Dim dbl_S_Sun_Rt_Ascen_Deg As Double
      //Dim dbl_T_Sun_Declin_Deg As Double
      //Dim dbl_U_Var_Y As Double
      //Dim dbl_V_EqOfTime_Minutes As Double
      //Dim dbl_W_AH_Sunrise_Deg As Double
      //Dim dbl_X_Solar_Noon_LST As Double
      //Dim dbl_Y_Sunrise_Time_LST As Double
      //Dim dbl_Z_Sunset_Time_LST As Double
      //Dim dbl_AA_Sunlight_Duration_Min As Double
      //Dim dbl_AB_True_Solar_Time_Min As Double
      //Dim dbl_AC_Hour_Angle_Deg As Double
      //Dim dbl_AD_Solar_Zenith_Angle_Deg As Double
      //Dim dbl_AE_Solar_Elevation_Angle_Deg As Double
      //Dim dbl_AF_Approx_Atmospheric_Refraction_Deg As Double
      //Dim dbl_AG_Solar_Elev_Corrected_for_Refract_Deg As Double
      //Dim dbl_AH_Solar_Azimuth_Angle_Deg_CW_From_N As Double


      // ALL REFERENCE EQUATIONS BELOW ARE COPIED DIRECTLY FROM EXCEL.
      // ALL REFERENCE VARIABLES HAVE "2" IN THE NAME BECAUSE THEY WERE COPIED FROM ROW 2.
      // BE CAREFUL OF EXCEL "ATAN2" FUNCTION BECAUSE IT USES NON-TRADITIONAL PARAMETER ORDER.
      // BE CAREFUL OF EXCEL "MOD" FUNCTION BECAUSE IT RETURNS DOUBLE VALUES, NOT INTEGER VALUES LIKE VB MOD.


      // $B$3 = Latitude
      // $B$4 = Longitude
      // $B$5 = hours difference from Greenwich
      // $B$7 = Date


      // SOME FUNCTIONS FILL FAIL IF NO SUNRISE OR SUNSET ON A PARTICULAR DAY.
      //  dbl_W_AH_Sunrise_Deg
      //  dbl_Y_Sunrise_Time_LST
      //  dbl_Z_Sunset_Time_LST
      //  dbl_AA_Sunlight_Duration_Min
      // SHOULD BE ABLE TO CATCH THESE AND SAY WHETHER IT IS CONSTANT DAYLIGHT OR NIGHT BASED ON
      //   SOLAR ELEVATION AT SOLAR NOON.  NEGATIVE VALUE MEANS NIGHT.

      //E2 = 0.1/24, E3 = E2+0.1/24, E4 = E3+0.1/24, etc. to increase in 6-minute increments
      // BASICALLY THE NUMBER OF DAYS PAST MIDNIGHT, SO WILL ALWAYS BE < 1.
      double dblDateWithTime = datDateWithTime.ToOADate();
      double dbl_E_Time_PastLocalMidnight = dblDateWithTime - Math.Truncate(dblDateWithTime);
      ////   Debug.Print "dbl_E_Time_PastLocalMidnight = " & Format(dbl_E_Time_PastLocalMidnight, "0.000000000000")

      //  //F2 = D2+2415018.5+E2-$B$5/24
      double dbl_F_JulianDay = dblDateWithTime + 2415018.5 - (dblHoursFromGreenwich / 24);
      //   Debug.Print "dbl_F_JulianDay = " & CStr(dbl_F_JulianDay)

      //G2 =(F2-2451545)/36525
      double dbl_G_Julian_Century = (dbl_F_JulianDay - 2451545) / 36525;
      //   Debug.Print "dbl_G_Julian_Century = " & CStr(dbl_G_Julian_Century)

      //I2 =MOD(280.46646+G2*(36000.76983 + G2*0.0003032),360)
      double dbl_I_Geom_Mean_Long_Sun_Deg = ((280.46646 + dbl_G_Julian_Century * (36000.76983 + dbl_G_Julian_Century * 0.0003032)) % 360d);
      //   Debug.Print "dbl_I_Geom_Mean_Long_Sun_Deg = " & Format(dbl_I_Geom_Mean_Long_Sun_Deg, "0.000000000000")

      //J2 =357.52911+G2*(35999.05029 - 0.0001537*G2)
      double dbl_J_GeomMean_Anom_Sun_Deg = 357.52911 + dbl_G_Julian_Century * (35999.05029 - 0.0001537 * dbl_G_Julian_Century);
      //   Debug.Print "dbl_J_GeomMean_Anom_Sun_Deg = " & Format(dbl_J_GeomMean_Anom_Sun_Deg, "0.000000000000")

      //K2 =0.016708634-G2*(0.000042037+0.0000001267*G2)
      double dbl_K_Eccent_Earth_Orbit = 0.016708634 - dbl_G_Julian_Century * (0.000042037 + 0.0000001267 * dbl_G_Julian_Century);
      //   Debug.Print "dbl_K_Eccent_Earth_Orbit = " & Format(dbl_K_Eccent_Earth_Orbit, "0.000000000000")

      //L2 =SIN(RADIANS(J2))*(1.914602-G2*(0.004817+0.000014*G2))+SIN(RADIANS(2*J2))*(0.019993-0.000101*G2)+SIN(RADIANS(3*J2))*0.000289
      double dbl_L_Sun_Eq_of_Ctr = Math.Sin(DegToRad(dbl_J_GeomMean_Anom_Sun_Deg)) * (1.914602 - dbl_G_Julian_Century *
          (0.004817 + 0.000014 * dbl_G_Julian_Century)) + Math.Sin(DegToRad(2 * dbl_J_GeomMean_Anom_Sun_Deg)) *
          (0.019993 - 0.000101 * dbl_G_Julian_Century) + Math.Sin(DegToRad(3 * dbl_J_GeomMean_Anom_Sun_Deg)) * 0.000289;
      //   Debug.Print "dbl_L_Sun_Eq_of_Ctr = " & Format(dbl_L_Sun_Eq_of_Ctr, "0.000000000000")

      //M2 =I2+L2
      double dbl_M_Sun_True_Long_Deg = dbl_I_Geom_Mean_Long_Sun_Deg + dbl_L_Sun_Eq_of_Ctr;
      //   Debug.Print "dbl_M_Sun_True_Long_Deg = " & Format(dbl_M_Sun_True_Long_Deg, "0.000000000000")

      //N2 =J2+L2
      //double dbl_N_Sun_True_Anom_Deg = dbl_J_GeomMean_Anom_Sun_Deg + dbl_L_Sun_Eq_of_Ctr;
      //   Debug.Print "dbl_N_Sun_True_Anom_Deg = " & Format(dbl_N_Sun_True_Anom_Deg, "0.000000000000")

      ////O2 =(1.000001018*(1-K2*K2))/(1+K2*COS(RADIANS(N2)))
      //double dbl_O_Sun_Rad_vector_AUs = (1.000001018 * (1 - dbl_K_Eccent_Earth_Orbit * dbl_K_Eccent_Earth_Orbit)) /
      //      (1 + dbl_K_Eccent_Earth_Orbit * Math.Cos(DegToRad(dbl_N_Sun_True_Anom_Deg)));
      ////   Debug.Print "dbl_O_Sun_Rad_vector_AUs = " & Format(dbl_O_Sun_Rad_vector_AUs, "0.000000000000")

      //P2 =M2-0.00569-0.00478*SIN(RADIANS(125.04-1934.136*G2))
      double dbl_P_Sun_App_Long_Deg = dbl_M_Sun_True_Long_Deg - 0.00569 - 0.00478 *
            Math.Sin(DegToRad(125.04 - 1934.136 * dbl_G_Julian_Century));
      //   Debug.Print "dbl_P_Sun_App_Long_Deg = " & Format(dbl_P_Sun_App_Long_Deg, "0.000000000000")

      //Q2 =23+(26+((21.448-G2*(46.815+G2*(0.00059-G2*0.001813))))/60)/60
      double dbl_Q_Mean_Obliq_Ecliptic_Deg = 0.00059 - (dbl_G_Julian_Century * 0.001813);
      dbl_Q_Mean_Obliq_Ecliptic_Deg = 46.815 + (dbl_G_Julian_Century * dbl_Q_Mean_Obliq_Ecliptic_Deg);
      dbl_Q_Mean_Obliq_Ecliptic_Deg = 21.448 - (dbl_G_Julian_Century * dbl_Q_Mean_Obliq_Ecliptic_Deg);
      dbl_Q_Mean_Obliq_Ecliptic_Deg = 23 + ((26 + (dbl_Q_Mean_Obliq_Ecliptic_Deg / 60)) / 60);
      //   Debug.Print "dbl_Q_Mean_Obliq_Ecliptic_Deg = " & CStr(dbl_Q_Mean_Obliq_Ecliptic_Deg)

      //R2 =Q2+0.00256*COS(RADIANS(125.04-1934.136*G2))
      double dbl_R_Obliq_Corr_Deg = dbl_Q_Mean_Obliq_Ecliptic_Deg + 0.00256 *
            Math.Cos(DegToRad(125.04 - 1934.136 * dbl_G_Julian_Century));
      //   Debug.Print "dbl_R_Obliq_Corr_Deg = " & Format(dbl_R_Obliq_Corr_Deg, "0.000000000000")

      ////S2 =DEGREES(ATAN2(COS(RADIANS(P2)),COS(RADIANS(R2))*SIN(RADIANS(P2))))
      //// NOTE:  EXCEL USES UNUSUAL ATAN2 DEFINITION.  I SWITCHED PARAMETERS IN MY FUNCTION
      //double dbl_S_Sun_Rt_Ascen_Deg = RadToDeg(Math.Atan2
      //    (Math.Cos(DegToRad(dbl_R_Obliq_Corr_Deg)) * Math.Sin(DegToRad(dbl_P_Sun_App_Long_Deg)),
      //    Math.Cos(DegToRad(dbl_P_Sun_App_Long_Deg))));
      ////   Debug.Print "dbl_S_Sun_Rt_Ascen_Deg = " & Format(dbl_S_Sun_Rt_Ascen_Deg, "0.000000000000")

      //T2 =DEGREES(ASIN(SIN(RADIANS(R2))*SIN(RADIANS(P2))))
      double dbl_T_Sun_Declin_Deg = RadToDeg(Math.Asin(Math.Sin(DegToRad(dbl_R_Obliq_Corr_Deg)) *
            Math.Sin(DegToRad(dbl_P_Sun_App_Long_Deg))));
      //   Debug.Print "dbl_T_Sun_Declin_Deg = " & Format(dbl_T_Sun_Declin_Deg, "0.000000000000")

      //U2 =TAN(RADIANS(R2/2))*TAN(RADIANS(R2/2))
      double dbl_U_Var_Y = Math.Tan(DegToRad(dbl_R_Obliq_Corr_Deg / 2)) * Math.Tan(DegToRad(dbl_R_Obliq_Corr_Deg / 2));
      //   Debug.Print "dbl_U_Var_Y = " & Format(dbl_U_Var_Y, "0.000000000000")

      //V2 =4*DEGREES(U2*SIN(2*RADIANS(I2))-2*K2*SIN(RADIANS(J2))+4*K2*U2*SIN(RADIANS(J2))*COS(2*RADIANS(I2))-0.5*U2*U2*SIN(4*RADIANS(I2))-1.25*K2*K2*SIN(2*RADIANS(J2)))
      double dbl_V_EqOfTime_Minutes = dbl_U_Var_Y * Math.Sin(2d * DegToRad(dbl_I_Geom_Mean_Long_Sun_Deg));
      dbl_V_EqOfTime_Minutes -= (2d * dbl_K_Eccent_Earth_Orbit * Math.Sin(DegToRad(dbl_J_GeomMean_Anom_Sun_Deg)));
      dbl_V_EqOfTime_Minutes += 4d * dbl_K_Eccent_Earth_Orbit * dbl_U_Var_Y * Math.Sin(DegToRad(dbl_J_GeomMean_Anom_Sun_Deg)) *
          Math.Cos(2 * DegToRad(dbl_I_Geom_Mean_Long_Sun_Deg));
      dbl_V_EqOfTime_Minutes -= 0.5 * dbl_U_Var_Y * dbl_U_Var_Y * Math.Sin(4d * DegToRad(dbl_I_Geom_Mean_Long_Sun_Deg));
      dbl_V_EqOfTime_Minutes -= 1.25 * dbl_K_Eccent_Earth_Orbit * dbl_K_Eccent_Earth_Orbit * Math.Sin(2d * DegToRad(dbl_J_GeomMean_Anom_Sun_Deg));
      dbl_V_EqOfTime_Minutes = 4d * RadToDeg(dbl_V_EqOfTime_Minutes);
      //   Debug.Print "dbl_V_EqOfTime_Minutes = " & Format(dbl_V_EqOfTime_Minutes, "0.000000000000")

      //W2 =DEGREES(ACOS(COS(RADIANS(90.833))/(COS(RADIANS($B$3))*COS(RADIANS(T2)))-TAN(RADIANS($B$3))*TAN(RADIANS(T2))))
      // NOTE:  THIS VALUE COULD CRASH IF NO SUNRISE OR SUNSET; PAST ARCTIC OR ANTARCTIC CIRCLE AND AT THE RIGHT TIME OF YEAR
      //  dbl_W_AH_Sunrise_Deg = Cos(DegToRad(90.833)) / _
      //      (Cos(DegToRad(dblLatitude)) * Cos(DegToRad(dbl_T_Sun_Declin_Deg)))
      //////   Debug.Print "dbl_W_AH_Sunrise_Deg: A = " & Format(dbl_W_AH_Sunrise_Deg, "0.000000000000")
      //  dbl_W_AH_Sunrise_Deg = dbl_W_AH_Sunrise_Deg - (Tan(DegToRad(dblLatitude)) * Tan(DegToRad(dbl_T_Sun_Declin_Deg)))
      //////   Debug.Print "dbl_W_AH_Sunrise_Deg: B = " & Format(dbl_W_AH_Sunrise_Deg, "0.000000000000")
      //  dbl_W_AH_Sunrise_Deg = RadToDeg(Math.ACos(dbl_W_AH_Sunrise_Deg))
      ////  dbl_W_AH_Sunrise_Deg = RadToDeg(Math.ACos(Cos(DegToRad(90.833)) / _
      ////(Cos(DegToRad(dblLatitude)) * Cos(DegToRad(dbl_T_Sun_Declin_Deg))) - _
      //      Tan(DegToRad(dblLatitude)) * Tan(DegToRad(dbl_T_Sun_Declin_Deg))))

      //bool boo_W_Crashed = false;
      //double dbl_W_AH_Sunrise_Deg = Return_W_AH_Sunrise_Deg(dblLatitude, dbl_T_Sun_Declin_Deg, out boo_W_Crashed);
      ////  Debug.Print "dbl_W_AH_Sunrise_Deg = " & Format(dbl_W_AH_Sunrise_Deg, "0.000000000000")

      ////X2 =(720-4*$B$4-V2+$B$5*60)/1440
      //double dbl_X_Solar_Noon_LST = (720d - 4d * dblLongitude - dbl_V_EqOfTime_Minutes + dblHoursFromGreenwich * 60d) / 1440d;
      ////   Debug.Print "dbl_X_Solar_Noon_LST = " & Format(dbl_X_Solar_Noon_LST, "Hh:Nn:Ss")

      //double dbl_Y_Sunrise_Time_LST;
      //double dbl_Z_Sunset_Time_LST;
      //double dbl_AA_Sunlight_Duration_Min;
      //if (boo_W_Crashed)   // Sunrise, Sunset and Sun Duration will also crash
      //{
      //  dbl_Y_Sunrise_Time_LST = double.NaN;
      //  dbl_Z_Sunset_Time_LST = double.NaN;
      //  dbl_AA_Sunlight_Duration_Min = double.NaN;
      //}
      //else
      //{  //Y2 =X2-W2*4/1440
      //  dbl_Y_Sunrise_Time_LST = dbl_X_Solar_Noon_LST - dbl_W_AH_Sunrise_Deg * 4d / 1440d;
      //  //   Debug.Print "dbl_Y_Sunrise_Time_LST = " & Format(dbl_Y_Sunrise_Time_LST, "Hh:Nn:Ss")

      //  //Z2 =X2+W2*4/1440
      //  dbl_Z_Sunset_Time_LST = dbl_X_Solar_Noon_LST + dbl_W_AH_Sunrise_Deg * 4d / 1440d;
      //  //   Debug.Print "dbl_Z_Sunset_Time_LST = " & Format(dbl_Z_Sunset_Time_LST, "Hh:Nn:Ss")

      //  //    //AA2 =8*W2
      //  dbl_AA_Sunlight_Duration_Min = 8d * dbl_W_AH_Sunrise_Deg;
      //  //   Debug.Print "dbl_AA_Sunlight_Duration_Min = " & Format(dbl_AA_Sunlight_Duration_Min, "0.000000000000")
      //}

      //AB2 =MOD(E2*1440+V2+4*$B$4-60*$B$5,1440)
      double dbl_AB_True_Solar_Time_Min = ((dbl_E_Time_PastLocalMidnight * 1440d + dbl_V_EqOfTime_Minutes +
          4d * dblLongitude - 60d * dblHoursFromGreenwich) % 1440d + 1440d) % 1440d;
      //   Debug.Print "dbl_AB_True_Solar_Time_Min = " & Format(dbl_AB_True_Solar_Time_Min, "0.000000000000")

      //AC2 =IF(AB2/4<0,AB2/4+180,AB2/4-180)
      double dbl_AC_Hour_Angle_Deg = (dbl_AB_True_Solar_Time_Min / 4d < 0) ? (dbl_AB_True_Solar_Time_Min / 4d) + 180d :  dbl_AB_True_Solar_Time_Min / 4d - 180d;
      //   Debug.Print "dbl_AC_Hour_Angle_Deg = " & Format(dbl_AC_Hour_Angle_Deg, "0.000000000000")

      //AD2 =DEGREES(ACOS(SIN(RADIANS($B$3))*SIN(RADIANS(T2))+COS(RADIANS($B$3))*COS(RADIANS(T2))*COS(RADIANS(AC2))))
      // ZENITH ANGLE IS MEASURED DOWN FROM STRAIGHT UP
      double dbl_AD_Solar_Zenith_Angle_Deg = RadToDeg(Math.Acos(Math.Clamp(Math.Sin(DegToRad(dblLatitude)) * Math.Sin(DegToRad(dbl_T_Sun_Declin_Deg)) +
            Math.Cos(DegToRad(dblLatitude)) * Math.Cos(DegToRad(dbl_T_Sun_Declin_Deg)) * Math.Cos(DegToRad(dbl_AC_Hour_Angle_Deg)), -1d, 1d)));
      //   Debug.Print "dbl_AD_Solar_Zenith_Angle_Deg = " & Format(dbl_AD_Solar_Zenith_Angle_Deg, "0.000000000000")

      //AE2 =90-AD2
      // THIS IS THE TRUE SOLAR ELEVATION; REGARDLESS OF WHERE WE SEE IT
      double dbl_AE_Solar_Elevation_Angle_Deg = 90d - dbl_AD_Solar_Zenith_Angle_Deg;
      //   Debug.Print "dbl_AE_Solar_Elevation_Angle_Deg = " & Format(dbl_AE_Solar_Elevation_Angle_Deg, "0.000000000000")

      //AF2 =IF(AE2>85,0,IF(AE2>5,58.1/TAN(RADIANS(AE2))-0.07/POWER(TAN(RADIANS(AE2)),3)+0.000086/POWER(TAN(RADIANS(AE2)),5),IF(AE2>-0.575,1735+AE2*(-518.2+AE2*(103.4+AE2*(-12.79+AE2*0.711))),-20.772/TAN(RADIANS(AE2)))))/3600
      double dbl_AF_Approx_Atmospheric_Refraction_Deg;
      if (dbl_AE_Solar_Elevation_Angle_Deg > 85)
      {
        dbl_AF_Approx_Atmospheric_Refraction_Deg = 0;
      }
      else
      {
        if (dbl_AE_Solar_Elevation_Angle_Deg > 5)
        {
          // IF(AE2>5,58.1/TAN(RADIANS(AE2))-0.07/POWER(TAN(RADIANS(AE2)),3)+0.000086/POWER(TAN(RADIANS(AE2)),5)
          dbl_AF_Approx_Atmospheric_Refraction_Deg = 58.1 / Math.Tan(DegToRad(dbl_AE_Solar_Elevation_Angle_Deg)) - 0.07 /
                Math.Pow((Math.Tan(DegToRad(dbl_AE_Solar_Elevation_Angle_Deg))), 3) +
                0.000086 / Math.Pow((Math.Tan(DegToRad(dbl_AE_Solar_Elevation_Angle_Deg))), 5);
        }
        else
        {
          // IF(AE2>-0.575,1735+AE2*(-518.2+AE2*(103.4+AE2*(-12.79+AE2*0.711))),-20.772/TAN(RADIANS(AE2)))))/3600
          if (dbl_AE_Solar_Elevation_Angle_Deg > -0.575)
          {
            dbl_AF_Approx_Atmospheric_Refraction_Deg = (-518.2 + dbl_AE_Solar_Elevation_Angle_Deg *
                (103.4 + dbl_AE_Solar_Elevation_Angle_Deg * (-12.79 + dbl_AE_Solar_Elevation_Angle_Deg * 0.711)));
            dbl_AF_Approx_Atmospheric_Refraction_Deg = 1735 + dbl_AE_Solar_Elevation_Angle_Deg *
                dbl_AF_Approx_Atmospheric_Refraction_Deg;
          }
          else
          {
            dbl_AF_Approx_Atmospheric_Refraction_Deg = -20.772 / Math.Tan(DegToRad(dbl_AE_Solar_Elevation_Angle_Deg));
          }
        }
      }
      dbl_AF_Approx_Atmospheric_Refraction_Deg /= 3600d;
      //   Debug.Print "dbl_AF_Approx_Atmospheric_Refraction_Deg = " & Format(dbl_AF_Approx_Atmospheric_Refraction_Deg, "0.000000000000")

      //AG2 =AE2+AF2
      // THIS IS WHERE WE SEE THE SUN; WE SEE IT BEFORE IT HAS ACTUALLY COME UP OVER THE HORIZON.
      double dbl_AG_Solar_Elev_Corrected_for_Refract_Deg = dbl_AE_Solar_Elevation_Angle_Deg + dbl_AF_Approx_Atmospheric_Refraction_Deg;
      //   Debug.Print "dbl_AG_Solar_Elev_Corrected_for_Refract_Deg = " & Format(dbl_AG_Solar_Elev_Corrected_for_Refract_Deg, "0.000000000000")

      //  //AH2 = IF(AC2 > 0, MOD(DEGREES(ACOS(((SIN(RADIANS($B$3)) * COS(RADIANS(AD2))) - SIN(RADIANS(T2))) / (COS(RADIANS($B$3)) * SIN(RADIANS(AD2))))) + 180, 360), MOD(540 - DEGREES(ACOS(((SIN(RADIANS($B$3)) * COS(RADIANS(AD2))) - SIN(RADIANS(T2))) / (COS(RADIANS($B$3)) * SIN(RADIANS(AD2))))), 360))
      double dblA;
      double dblB;
      double dbl_AH_Solar_Azimuth_Angle_Deg_CW_From_N;
      if (dbl_AC_Hour_Angle_Deg > 0)
      {
        // MOD(DEGREES(ACOS(((SIN(RADIANS($B$3))*COS(RADIANS(AD2)))-SIN(RADIANS(T2)))/(COS(RADIANS($B$3))*SIN(RADIANS(AD2)))))+180,360)
        dblA = Math.Sin(DegToRad(dblLatitude)) * Math.Cos(DegToRad(dbl_AD_Solar_Zenith_Angle_Deg)) -
              Math.Sin(DegToRad(dbl_T_Sun_Declin_Deg));
        dblB = Math.Cos(DegToRad(dblLatitude)) * Math.Sin(DegToRad(dbl_AD_Solar_Zenith_Angle_Deg));
        dbl_AH_Solar_Azimuth_Angle_Deg_CW_From_N = (RadToDeg(Math.Acos(Math.Clamp(dblA / dblB, -1d, 1d))) + 180) % 360d;
      }
      else
      {
        // MOD(540-DEGREES(ACOS(((SIN(RADIANS($B$3))*COS(RADIANS(AD2)))-SIN(RADIANS(T2)))/(COS(RADIANS($B$3))*SIN(RADIANS(AD2))))),360))
        dblA = (Math.Sin(DegToRad(dblLatitude)) * Math.Cos(DegToRad(dbl_AD_Solar_Zenith_Angle_Deg))) - Math.Sin(DegToRad(dbl_T_Sun_Declin_Deg));
        dblB = Math.Cos(DegToRad(dblLatitude)) * Math.Sin(DegToRad(dbl_AD_Solar_Zenith_Angle_Deg));
        dbl_AH_Solar_Azimuth_Angle_Deg_CW_From_N = (540 - RadToDeg(Math.Acos(Math.Clamp(dblA / dblB, -1d, 1d)))) % 360d;
        //   Debug.Print "dbl_AH_Solar_Azimuth_Angle_Deg_CW_From_N = " & Format(dbl_AH_Solar_Azimuth_Angle_Deg_CW_From_N, "0.000000000000")
      }
      //if (boo_W_Crashed)
      //{
      //  lngSunriseExists = (dbl_AG_Solar_Elev_Corrected_for_Refract_Deg > 0) ? JenSolarConditions.ENUM_AlwaysDay : JenSolarConditions.ENUM_AlwaysNight;
      //}
      //else { lngSunriseExists = JenSolarConditions.ENUM_SunriseAndSunset; }
      //dblSunrise = dbl_Y_Sunrise_Time_LST;
      //dblSunset = dbl_Z_Sunset_Time_LST;
      dblSunDirection = dbl_AH_Solar_Azimuth_Angle_Deg_CW_From_N;
      dblSunAngleUp = dbl_AG_Solar_Elev_Corrected_for_Refract_Deg;

      //  // SAMPLE CODE
      //  //  Debug.Print "--------------------------------------"
      //  //
      //  //  Dim dblLatitude As Double
      //  //  Dim dblLongitude As Double
      //  //  Dim datDateWithTime As Date
      //  //  Dim dblHoursFromGreenwich As Double
      //  //
      //  //
      //  //  Dim dblSunrise As Double
      //  //  Dim dblSunset As Double
      //  //  Dim dblSunDirection As Double
      //  //  Dim dblSunAngleUp As Double
      //  //  Dim lngSolarOption As JenSolarConditions
      //  //  Dim dblTimePastMidnight As Double
      //  //  Dim dblSunDirectionAtSunrise As Double
      //  //  Dim dblSunDirectionAtSunset As Double
      //  //
      //  //  dblLatitude = 34.98
      //  //  dblLongitude = -111.60592
      //  //  datDateWithTime = CDate("6/21/2010 20:00:00")
      //  ////  datDateWithTime = DateAdd("h", 7, Now)
      //  //  dblHoursFromGreenwich = -7
      //  ////  dblHoursFromGreenwich = 0
      //  //
      //  //  Debug.Print "Date as Double = " & dblDateWithTime
      //  //  Debug.Print Format(datDateWithTime, "Long Date")
      //  //  Debug.Print Format(datDateWithTime, "Long Time")
      //  //  Debug.Print "Longitude = " & CStr(dblLongitude)
      //  //  Debug.Print "Latitude = " & CStr(dblLatitude)
      //  //
      //  //  SolarFunctions dblLatitude, dblLongitude, datDateWithTime, dblHoursFromGreenwich, _
      //  //     lngSolarOption, dblSunrise, dblSunset, dblSunDirection, dblSunAngleUp, dblSunDirectionAtSunrise, _
      //  //     dblSunDirectionAtSunset
      //  //
      //  //  dblTimePastMidnight = dblDateWithTime - Fix(datDateWithTime)
      //  //
      //  //  Debug.Print "---"
      //  //  If lngSolarOption = ENUM_SunriseAndSunset Then
      //  //    Debug.Print "Sunrise = " & Format(dblSunrise, "Hh:Nn:Ss")
      //  //    Debug.Print "Sunset = " & Format(dblSunset, "Hh:Nn:Ss")
      //  //    Debug.Print "Observed Time >= Sunrise = " & Format(dblTimePastMidnight >= dblSunrise, ">")
      //  //    Debug.Print "Observed Time <= Sunset = " & Format(dblTimePastMidnight <= dblSunset, ">")
      //  //    Debug.Print "Sun Visible at Time = " & Format((dblTimePastMidnight >= dblSunrise) And _
      //  //          (dblTimePastMidnight <= dblSunset), ">")
      //  //  Else
      //  //    If lngSolarOption = ENUM_AlwaysDay Then
      //  //      Debug.Print "No sunrise or sunset; Always day..."
      //  //    Else
      //  //      Debug.Print "No sunrise or sunset; Always Night..."
      //  //    End If
      //  //  End If
      //  //
      //  //  Debug.Print "Sun Direction = " & CStr(dblSunDirection) & " degrees"
      //  //  Debug.Print "Sun Angle = " & CStr(dblSunAngleUp) & " degrees up"
      //  //  Debug.Print "Sun Direction at Sunrise = " & CStr(dblSunDirectionAtSunrise) & " degrees"
      //  //  Debug.Print "Sun Direction at Sunset = " & CStr(dblSunDirectionAtSunset) & " degrees"
      //  //  Debug.Print "---"
      //  //  Debug.Print "Done..."


      //  dblSunrise = dbl_Y_Sunrise_Time_LST
      //  dblSunset = dbl_Z_Sunset_Time_LST
      //  dblSunDirection = dbl_AH_Solar_Azimuth_Angle_Deg_CW_From_N
      //  dblSunAngleUp = dbl_AG_Solar_Elev_Corrected_for_Refract_Deg
    }
    ///<summary>
    ///Given a Lat/Long location, time of day and hours from Greenwich,<br/>
    ///calculates the sun position in the sky, time of sunset and sunrise, <br/>
    ///sun direction at sunrise and sunset, and enumeration delineating whether sunrise and sunset exist.<br/>
    ///Can convert times to DateTime format using "DateTime.FromOADate(dblTimeOfDay)"
    ///<br/><br/>Returns doubles for times of day, compass directions and angle up.
    ///</summary>
    public static void SolarFunctions(double dblLatitude, double dblLongitude, DateTime datDateWithTime, double dblHoursFromGreenwich,
       out JenSolarConditions lngSunriseExists, out double dblSunrise, out double dblSunset, out double dblSunDirection,
       out double dblSunAngleUp, out double dblSunDirectionAtSunrise, out double dblSunDirectionAtSunset, out double dblMinutesOfSunlight)
    {
      // ADAPTED FROM http://www.esrl.noaa.gov/gmd/grad/solcalc/
      // SAMPLE EXCEL FILE http://www.esrl.noaa.gov/gmd/grad/solcalc/NOAA_Solar_Calculations_day.xls
      // GLOSSARY OF TERMS AT http://www.esrl.noaa.gov/gmd/grad/solcalc/glossary.html
      // Sample Code at Bottom

      // VARIABLES BELOW ARE NAMED ACCORDING TO DESCRIPTION AND EXCEL COLUMN


      // ALL REFERENCE EQUATIONS BELOW ARE COPIED DIRECTLY FROM EXCEL.
      // ALL REFERENCE VARIABLES HAVE "2" IN THE NAME BECAUSE THEY WERE COPIED FROM ROW 2.
      // BE CAREFUL OF EXCEL "ATAN2" FUNCTION BECAUSE IT USES NON-TRADITIONAL PARAMETER ORDER.
      // BE CAREFUL OF EXCEL "MOD" FUNCTION BECAUSE IT RETURNS DOUBLE VALUES, NOT INTEGER VALUES LIKE VB MOD.


      // $B$3 = Latitude
      // $B$4 = Longitude
      // $B$5 = hours difference from Greenwich
      // $B$7 = Date


      // SOME FUNCTIONS FILL FAIL IF NO SUNRISE OR SUNSET ON A PARTICULAR DAY.
      //  dbl_W_AH_Sunrise_Deg
      //  dbl_Y_Sunrise_Time_LST
      //  dbl_Z_Sunset_Time_LST
      //  dbl_AA_Sunlight_Duration_Min
      // SHOULD BE ABLE TO CATCH THESE AND SAY WHETHER IT IS CONSTANT DAYLIGHT OR NIGHT BASED ON
      //   SOLAR ELEVATION AT SOLAR NOON.  NEGATIVE VALUE MEANS NIGHT.

      //E2 = 0.1/24, E3 = E2+0.1/24, E4 = E3+0.1/24, etc. to increase in 6-minute increments
      // BASICALLY THE NUMBER OF DAYS PAST MIDNIGHT, SO WILL ALWAYS BE < 1.
      double dblDateWithTime = datDateWithTime.ToOADate();
      double dbl_E_Time_PastLocalMidnight = dblDateWithTime - Math.Truncate(dblDateWithTime);
      ////   Debug.Print "dbl_E_Time_PastLocalMidnight = " & Format(dbl_E_Time_PastLocalMidnight, "0.000000000000")

      //  //F2 = D2+2415018.5+E2-$B$5/24
      double dbl_F_JulianDay = dblDateWithTime + 2415018.5 - (dblHoursFromGreenwich / 24);
      //   Debug.Print "dbl_F_JulianDay = " & CStr(dbl_F_JulianDay)

      //G2 =(F2-2451545)/36525
      double dbl_G_Julian_Century = (dbl_F_JulianDay - 2451545) / 36525;
      //   Debug.Print "dbl_G_Julian_Century = " & CStr(dbl_G_Julian_Century)

      //I2 =MOD(280.46646+G2*(36000.76983 + G2*0.0003032),360)
      double dbl_I_Geom_Mean_Long_Sun_Deg = ((280.46646 + dbl_G_Julian_Century * (36000.76983 + dbl_G_Julian_Century * 0.0003032)) % 360d);
      //   Debug.Print "dbl_I_Geom_Mean_Long_Sun_Deg = " & Format(dbl_I_Geom_Mean_Long_Sun_Deg, "0.000000000000")

      //J2 =357.52911+G2*(35999.05029 - 0.0001537*G2)
      double dbl_J_GeomMean_Anom_Sun_Deg = 357.52911 + dbl_G_Julian_Century * (35999.05029 - 0.0001537 * dbl_G_Julian_Century);
      //   Debug.Print "dbl_J_GeomMean_Anom_Sun_Deg = " & Format(dbl_J_GeomMean_Anom_Sun_Deg, "0.000000000000")

      //K2 =0.016708634-G2*(0.000042037+0.0000001267*G2)
      double dbl_K_Eccent_Earth_Orbit = 0.016708634 - dbl_G_Julian_Century * (0.000042037 + 0.0000001267 * dbl_G_Julian_Century);
      //   Debug.Print "dbl_K_Eccent_Earth_Orbit = " & Format(dbl_K_Eccent_Earth_Orbit, "0.000000000000")

      //L2 =SIN(RADIANS(J2))*(1.914602-G2*(0.004817+0.000014*G2))+SIN(RADIANS(2*J2))*(0.019993-0.000101*G2)+SIN(RADIANS(3*J2))*0.000289
      double dbl_L_Sun_Eq_of_Ctr = Math.Sin(DegToRad(dbl_J_GeomMean_Anom_Sun_Deg)) * (1.914602 - dbl_G_Julian_Century *
          (0.004817 + 0.000014 * dbl_G_Julian_Century)) + Math.Sin(DegToRad(2 * dbl_J_GeomMean_Anom_Sun_Deg)) *
          (0.019993 - 0.000101 * dbl_G_Julian_Century) + Math.Sin(DegToRad(3 * dbl_J_GeomMean_Anom_Sun_Deg)) * 0.000289;
      //   Debug.Print "dbl_L_Sun_Eq_of_Ctr = " & Format(dbl_L_Sun_Eq_of_Ctr, "0.000000000000")

      //M2 =I2+L2
      double dbl_M_Sun_True_Long_Deg = dbl_I_Geom_Mean_Long_Sun_Deg + dbl_L_Sun_Eq_of_Ctr;
      //   Debug.Print "dbl_M_Sun_True_Long_Deg = " & Format(dbl_M_Sun_True_Long_Deg, "0.000000000000")

      //N2 =J2+L2
      //double dbl_N_Sun_True_Anom_Deg = dbl_J_GeomMean_Anom_Sun_Deg + dbl_L_Sun_Eq_of_Ctr;
      //   Debug.Print "dbl_N_Sun_True_Anom_Deg = " & Format(dbl_N_Sun_True_Anom_Deg, "0.000000000000")

      ////O2 =(1.000001018*(1-K2*K2))/(1+K2*COS(RADIANS(N2)))
      //double dbl_O_Sun_Rad_vector_AUs = (1.000001018 * (1 - dbl_K_Eccent_Earth_Orbit * dbl_K_Eccent_Earth_Orbit)) /
      //      (1 + dbl_K_Eccent_Earth_Orbit * Math.Cos(DegToRad(dbl_N_Sun_True_Anom_Deg)));
      ////   Debug.Print "dbl_O_Sun_Rad_vector_AUs = " & Format(dbl_O_Sun_Rad_vector_AUs, "0.000000000000")

      //P2 =M2-0.00569-0.00478*SIN(RADIANS(125.04-1934.136*G2))
      double dbl_P_Sun_App_Long_Deg = dbl_M_Sun_True_Long_Deg - 0.00569 - 0.00478 *
            Math.Sin(DegToRad(125.04 - 1934.136 * dbl_G_Julian_Century));
      //   Debug.Print "dbl_P_Sun_App_Long_Deg = " & Format(dbl_P_Sun_App_Long_Deg, "0.000000000000")

      //Q2 =23+(26+((21.448-G2*(46.815+G2*(0.00059-G2*0.001813))))/60)/60
      double dbl_Q_Mean_Obliq_Ecliptic_Deg = 0.00059 - (dbl_G_Julian_Century * 0.001813);
      dbl_Q_Mean_Obliq_Ecliptic_Deg = 46.815 + (dbl_G_Julian_Century * dbl_Q_Mean_Obliq_Ecliptic_Deg);
      dbl_Q_Mean_Obliq_Ecliptic_Deg = 21.448 - (dbl_G_Julian_Century * dbl_Q_Mean_Obliq_Ecliptic_Deg);
      dbl_Q_Mean_Obliq_Ecliptic_Deg = 23 + ((26 + (dbl_Q_Mean_Obliq_Ecliptic_Deg / 60)) / 60);
      //   Debug.Print "dbl_Q_Mean_Obliq_Ecliptic_Deg = " & CStr(dbl_Q_Mean_Obliq_Ecliptic_Deg)

      //R2 =Q2+0.00256*COS(RADIANS(125.04-1934.136*G2))
      double dbl_R_Obliq_Corr_Deg = dbl_Q_Mean_Obliq_Ecliptic_Deg + 0.00256 *
            Math.Cos(DegToRad(125.04 - 1934.136 * dbl_G_Julian_Century));
      //   Debug.Print "dbl_R_Obliq_Corr_Deg = " & Format(dbl_R_Obliq_Corr_Deg, "0.000000000000")

      ////S2 =DEGREES(ATAN2(COS(RADIANS(P2)),COS(RADIANS(R2))*SIN(RADIANS(P2))))
      //// NOTE:  EXCEL USES UNUSUAL ATAN2 DEFINITION.  I SWITCHED PARAMETERS IN MY FUNCTION
      //double dbl_S_Sun_Rt_Ascen_Deg = RadToDeg(Math.Atan2
      //    (Math.Cos(DegToRad(dbl_R_Obliq_Corr_Deg)) * Math.Sin(DegToRad(dbl_P_Sun_App_Long_Deg)),
      //    Math.Cos(DegToRad(dbl_P_Sun_App_Long_Deg))));
      ////   Debug.Print "dbl_S_Sun_Rt_Ascen_Deg = " & Format(dbl_S_Sun_Rt_Ascen_Deg, "0.000000000000")

      //T2 =DEGREES(ASIN(SIN(RADIANS(R2))*SIN(RADIANS(P2))))
      double dbl_T_Sun_Declin_Deg = RadToDeg(Math.Asin(Math.Sin(DegToRad(dbl_R_Obliq_Corr_Deg)) *
            Math.Sin(DegToRad(dbl_P_Sun_App_Long_Deg))));
      //   Debug.Print "dbl_T_Sun_Declin_Deg = " & Format(dbl_T_Sun_Declin_Deg, "0.000000000000")

      //U2 =TAN(RADIANS(R2/2))*TAN(RADIANS(R2/2))
      double dbl_U_Var_Y = Math.Tan(DegToRad(dbl_R_Obliq_Corr_Deg / 2)) * Math.Tan(DegToRad(dbl_R_Obliq_Corr_Deg / 2));
      //   Debug.Print "dbl_U_Var_Y = " & Format(dbl_U_Var_Y, "0.000000000000")

      //V2 =4*DEGREES(U2*SIN(2*RADIANS(I2))-2*K2*SIN(RADIANS(J2))+4*K2*U2*SIN(RADIANS(J2))*COS(2*RADIANS(I2))-0.5*U2*U2*SIN(4*RADIANS(I2))-1.25*K2*K2*SIN(2*RADIANS(J2)))
      double dbl_V_EqOfTime_Minutes = dbl_U_Var_Y * Math.Sin(2d * DegToRad(dbl_I_Geom_Mean_Long_Sun_Deg));
      dbl_V_EqOfTime_Minutes -= (2d * dbl_K_Eccent_Earth_Orbit * Math.Sin(DegToRad(dbl_J_GeomMean_Anom_Sun_Deg)));
      dbl_V_EqOfTime_Minutes += 4d * dbl_K_Eccent_Earth_Orbit * dbl_U_Var_Y * Math.Sin(DegToRad(dbl_J_GeomMean_Anom_Sun_Deg)) *
          Math.Cos(2 * DegToRad(dbl_I_Geom_Mean_Long_Sun_Deg));
      dbl_V_EqOfTime_Minutes -= 0.5 * dbl_U_Var_Y * dbl_U_Var_Y * Math.Sin(4d * DegToRad(dbl_I_Geom_Mean_Long_Sun_Deg));
      dbl_V_EqOfTime_Minutes -= 1.25 * dbl_K_Eccent_Earth_Orbit * dbl_K_Eccent_Earth_Orbit * Math.Sin(2d * DegToRad(dbl_J_GeomMean_Anom_Sun_Deg));
      dbl_V_EqOfTime_Minutes = 4d * RadToDeg(dbl_V_EqOfTime_Minutes);
      //   Debug.Print "dbl_V_EqOfTime_Minutes = " & Format(dbl_V_EqOfTime_Minutes, "0.000000000000")

      //W2 =DEGREES(ACOS(COS(RADIANS(90.833))/(COS(RADIANS($B$3))*COS(RADIANS(T2)))-TAN(RADIANS($B$3))*TAN(RADIANS(T2))))
      // NOTE:  THIS VALUE COULD CRASH IF NO SUNRISE OR SUNSET; PAST ARCTIC OR ANTARCTIC CIRCLE AND AT THE RIGHT TIME OF YEAR
      //  dbl_W_AH_Sunrise_Deg = Cos(DegToRad(90.833)) / _
      //      (Cos(DegToRad(dblLatitude)) * Cos(DegToRad(dbl_T_Sun_Declin_Deg)))
      //////   Debug.Print "dbl_W_AH_Sunrise_Deg: A = " & Format(dbl_W_AH_Sunrise_Deg, "0.000000000000")
      //  dbl_W_AH_Sunrise_Deg = dbl_W_AH_Sunrise_Deg - (Tan(DegToRad(dblLatitude)) * Tan(DegToRad(dbl_T_Sun_Declin_Deg)))
      //////   Debug.Print "dbl_W_AH_Sunrise_Deg: B = " & Format(dbl_W_AH_Sunrise_Deg, "0.000000000000")
      //  dbl_W_AH_Sunrise_Deg = RadToDeg(Math.ACos(dbl_W_AH_Sunrise_Deg))
      ////  dbl_W_AH_Sunrise_Deg = RadToDeg(Math.ACos(Cos(DegToRad(90.833)) / _
      ////(Cos(DegToRad(dblLatitude)) * Cos(DegToRad(dbl_T_Sun_Declin_Deg))) - _
      //      Tan(DegToRad(dblLatitude)) * Tan(DegToRad(dbl_T_Sun_Declin_Deg))))

      double dbl_W_AH_Sunrise_Deg = Return_W_AH_Sunrise_Deg(dblLatitude, dbl_T_Sun_Declin_Deg, out bool boo_W_Crashed);
      //  Debug.Print "dbl_W_AH_Sunrise_Deg = " & Format(dbl_W_AH_Sunrise_Deg, "0.000000000000")

      //X2 =(720-4*$B$4-V2+$B$5*60)/1440
      double dbl_X_Solar_Noon_LST = (720d - 4d * dblLongitude - dbl_V_EqOfTime_Minutes + dblHoursFromGreenwich * 60d) / 1440d;
      //   Debug.Print "dbl_X_Solar_Noon_LST = " & Format(dbl_X_Solar_Noon_LST, "Hh:Nn:Ss")

      double dbl_Y_Sunrise_Time_LST;
      double dbl_Z_Sunset_Time_LST;
      double dbl_AA_Sunlight_Duration_Min;
      if (boo_W_Crashed)   // Sunrise, Sunset and Sun Duration will also crash
      {
        dbl_Y_Sunrise_Time_LST = double.NaN;
        dbl_Z_Sunset_Time_LST = double.NaN;
        dbl_AA_Sunlight_Duration_Min = double.NaN;
      }
      else
      {  //Y2 =X2-W2*4/1440
        dbl_Y_Sunrise_Time_LST = dbl_X_Solar_Noon_LST - dbl_W_AH_Sunrise_Deg * 4d / 1440d;
        //   Debug.Print "dbl_Y_Sunrise_Time_LST = " & Format(dbl_Y_Sunrise_Time_LST, "Hh:Nn:Ss")

        //Z2 =X2+W2*4/1440
        dbl_Z_Sunset_Time_LST = dbl_X_Solar_Noon_LST + dbl_W_AH_Sunrise_Deg * 4d / 1440d;
        //   Debug.Print "dbl_Z_Sunset_Time_LST = " & Format(dbl_Z_Sunset_Time_LST, "Hh:Nn:Ss")

        //    //AA2 =8*W2
        dbl_AA_Sunlight_Duration_Min = 8d * dbl_W_AH_Sunrise_Deg;
        //   Debug.Print "dbl_AA_Sunlight_Duration_Min = " & Format(dbl_AA_Sunlight_Duration_Min, "0.000000000000")
      }

      //AB2 =MOD(E2*1440+V2+4*$B$4-60*$B$5,1440)
      double dbl_AB_True_Solar_Time_Min = ((dbl_E_Time_PastLocalMidnight * 1440d + dbl_V_EqOfTime_Minutes +
          4d * dblLongitude - 60d * dblHoursFromGreenwich) % 1440d + 1440d) % 1440d;
      //   Debug.Print "dbl_AB_True_Solar_Time_Min = " & Format(dbl_AB_True_Solar_Time_Min, "0.000000000000")

      //AC2 =IF(AB2/4<0,AB2/4+180,AB2/4-180)
      double dbl_AC_Hour_Angle_Deg = (dbl_AB_True_Solar_Time_Min / 4d < 0) ? (dbl_AB_True_Solar_Time_Min / 4d) + 180d : dbl_AB_True_Solar_Time_Min / 4d - 180d;
      //   Debug.Print "dbl_AC_Hour_Angle_Deg = " & Format(dbl_AC_Hour_Angle_Deg, "0.000000000000")

      //AD2 =DEGREES(ACOS(SIN(RADIANS($B$3))*SIN(RADIANS(T2))+COS(RADIANS($B$3))*COS(RADIANS(T2))*COS(RADIANS(AC2))))
      // ZENITH ANGLE IS MEASURED DOWN FROM STRAIGHT UP
      double dbl_AD_Solar_Zenith_Angle_Deg = RadToDeg(Math.Acos(Math.Clamp(Math.Sin(DegToRad(dblLatitude)) * Math.Sin(DegToRad(dbl_T_Sun_Declin_Deg)) +
            Math.Cos(DegToRad(dblLatitude)) * Math.Cos(DegToRad(dbl_T_Sun_Declin_Deg)) * Math.Cos(DegToRad(dbl_AC_Hour_Angle_Deg)), -1d, 1d)));
      //   Debug.Print "dbl_AD_Solar_Zenith_Angle_Deg = " & Format(dbl_AD_Solar_Zenith_Angle_Deg, "0.000000000000")

      //AE2 =90-AD2
      // THIS IS THE TRUE SOLAR ELEVATION; REGARDLESS OF WHERE WE SEE IT
      double dbl_AE_Solar_Elevation_Angle_Deg = 90d - dbl_AD_Solar_Zenith_Angle_Deg;
      //   Debug.Print "dbl_AE_Solar_Elevation_Angle_Deg = " & Format(dbl_AE_Solar_Elevation_Angle_Deg, "0.000000000000")

      //AF2 =IF(AE2>85,0,IF(AE2>5,58.1/TAN(RADIANS(AE2))-0.07/POWER(TAN(RADIANS(AE2)),3)+0.000086/POWER(TAN(RADIANS(AE2)),5),IF(AE2>-0.575,1735+AE2*(-518.2+AE2*(103.4+AE2*(-12.79+AE2*0.711))),-20.772/TAN(RADIANS(AE2)))))/3600
      double dbl_AF_Approx_Atmospheric_Refraction_Deg;
      if (dbl_AE_Solar_Elevation_Angle_Deg > 85)
      {
        dbl_AF_Approx_Atmospheric_Refraction_Deg = 0;
      }
      else
      {
        if (dbl_AE_Solar_Elevation_Angle_Deg > 5)
        {
          // IF(AE2>5,58.1/TAN(RADIANS(AE2))-0.07/POWER(TAN(RADIANS(AE2)),3)+0.000086/POWER(TAN(RADIANS(AE2)),5)
          dbl_AF_Approx_Atmospheric_Refraction_Deg = 58.1 / Math.Tan(DegToRad(dbl_AE_Solar_Elevation_Angle_Deg)) - 0.07 /
                Math.Pow((Math.Tan(DegToRad(dbl_AE_Solar_Elevation_Angle_Deg))), 3) +
                0.000086 / Math.Pow((Math.Tan(DegToRad(dbl_AE_Solar_Elevation_Angle_Deg))), 5);
        }
        else
        {
          // IF(AE2>-0.575,1735+AE2*(-518.2+AE2*(103.4+AE2*(-12.79+AE2*0.711))),-20.772/TAN(RADIANS(AE2)))))/3600
          if (dbl_AE_Solar_Elevation_Angle_Deg > -0.575)
          {
            dbl_AF_Approx_Atmospheric_Refraction_Deg = (-518.2 + dbl_AE_Solar_Elevation_Angle_Deg *
                (103.4 + dbl_AE_Solar_Elevation_Angle_Deg * (-12.79 + dbl_AE_Solar_Elevation_Angle_Deg * 0.711)));
            dbl_AF_Approx_Atmospheric_Refraction_Deg = 1735 + dbl_AE_Solar_Elevation_Angle_Deg *
                dbl_AF_Approx_Atmospheric_Refraction_Deg;
          }
          else
          {
            dbl_AF_Approx_Atmospheric_Refraction_Deg = -20.772 / Math.Tan(DegToRad(dbl_AE_Solar_Elevation_Angle_Deg));
          }
        }
      }
      dbl_AF_Approx_Atmospheric_Refraction_Deg /= 3600d;
      //   Debug.Print "dbl_AF_Approx_Atmospheric_Refraction_Deg = " & Format(dbl_AF_Approx_Atmospheric_Refraction_Deg, "0.000000000000")

      //AG2 =AE2+AF2
      // THIS IS WHERE WE SEE THE SUN; WE SEE IT BEFORE IT HAS ACTUALLY COME UP OVER THE HORIZON.
      double dbl_AG_Solar_Elev_Corrected_for_Refract_Deg = dbl_AE_Solar_Elevation_Angle_Deg + dbl_AF_Approx_Atmospheric_Refraction_Deg;
      //   Debug.Print "dbl_AG_Solar_Elev_Corrected_for_Refract_Deg = " & Format(dbl_AG_Solar_Elev_Corrected_for_Refract_Deg, "0.000000000000")

      //  //AH2 = IF(AC2 > 0, MOD(DEGREES(ACOS(((SIN(RADIANS($B$3)) * COS(RADIANS(AD2))) - SIN(RADIANS(T2))) / (COS(RADIANS($B$3)) * SIN(RADIANS(AD2))))) + 180, 360), MOD(540 - DEGREES(ACOS(((SIN(RADIANS($B$3)) * COS(RADIANS(AD2))) - SIN(RADIANS(T2))) / (COS(RADIANS($B$3)) * SIN(RADIANS(AD2))))), 360))
      double dblA;
      double dblB;
      double dbl_AH_Solar_Azimuth_Angle_Deg_CW_From_N;
      if (dbl_AC_Hour_Angle_Deg > 0)
      {
        // MOD(DEGREES(ACOS(((SIN(RADIANS($B$3))*COS(RADIANS(AD2)))-SIN(RADIANS(T2)))/(COS(RADIANS($B$3))*SIN(RADIANS(AD2)))))+180,360)
        dblA = Math.Sin(DegToRad(dblLatitude)) * Math.Cos(DegToRad(dbl_AD_Solar_Zenith_Angle_Deg)) -
              Math.Sin(DegToRad(dbl_T_Sun_Declin_Deg));
        dblB = Math.Cos(DegToRad(dblLatitude)) * Math.Sin(DegToRad(dbl_AD_Solar_Zenith_Angle_Deg));
        dbl_AH_Solar_Azimuth_Angle_Deg_CW_From_N = (RadToDeg(Math.Acos(Math.Clamp(dblA / dblB, -1d, 1d))) + 180) % 360d;
      }
      else
      {
        // MOD(540-DEGREES(ACOS(((SIN(RADIANS($B$3))*COS(RADIANS(AD2)))-SIN(RADIANS(T2)))/(COS(RADIANS($B$3))*SIN(RADIANS(AD2))))),360))
        dblA = (Math.Sin(DegToRad(dblLatitude)) * Math.Cos(DegToRad(dbl_AD_Solar_Zenith_Angle_Deg))) - Math.Sin(DegToRad(dbl_T_Sun_Declin_Deg));
        dblB = Math.Cos(DegToRad(dblLatitude)) * Math.Sin(DegToRad(dbl_AD_Solar_Zenith_Angle_Deg));
        dbl_AH_Solar_Azimuth_Angle_Deg_CW_From_N = (540 - RadToDeg(Math.Acos(Math.Clamp(dblA / dblB, -1d, 1d)))) % 360d;
        //   Debug.Print "dbl_AH_Solar_Azimuth_Angle_Deg_CW_From_N = " & Format(dbl_AH_Solar_Azimuth_Angle_Deg_CW_From_N, "0.000000000000")
      }
      if (boo_W_Crashed)
      {
        lngSunriseExists = (dbl_AG_Solar_Elev_Corrected_for_Refract_Deg > 0) ? JenSolarConditions.ENUM_AlwaysDay : JenSolarConditions.ENUM_AlwaysNight;
      }
      else { lngSunriseExists = JenSolarConditions.ENUM_SunriseAndSunset; }

      dblSunrise = dbl_Y_Sunrise_Time_LST;
      dblSunset = dbl_Z_Sunset_Time_LST;
      dblSunDirection = dbl_AH_Solar_Azimuth_Angle_Deg_CW_From_N;
      dblSunAngleUp = dbl_AG_Solar_Elev_Corrected_for_Refract_Deg;
      dblMinutesOfSunlight = dbl_AA_Sunlight_Duration_Min;

      DateTime datFullSunriseDate;
      DateTime datFullSunsetDate;
      //double dblTempSunAngle;

      if (boo_W_Crashed || lngSunriseExists != JenSolarConditions.ENUM_SunriseAndSunset)
      {
        dblSunDirectionAtSunrise = double.NaN;
        dblSunDirectionAtSunset = double.NaN;
      }
      else
      {
        datFullSunriseDate = (dblDateWithTime < 0) ? DateTime.FromOADate((Math.Truncate(dblDateWithTime)) - dblSunrise) : DateTime.FromOADate((Math.Truncate(dblDateWithTime)) + dblSunrise);
        SolarFunctions(dblLatitude, dblLongitude, datFullSunriseDate, dblHoursFromGreenwich, out double dblTempSunDirection, out _);
        dblSunDirectionAtSunrise = dblTempSunDirection;
        datFullSunsetDate = (dblDateWithTime < 0) ? DateTime.FromOADate((Math.Truncate(dblDateWithTime)) - dblSunset) : DateTime.FromOADate((Math.Truncate(dblDateWithTime)) + dblSunset);
        SolarFunctions(dblLatitude, dblLongitude, datFullSunsetDate, dblHoursFromGreenwich, out dblTempSunDirection, out _);
        dblSunDirectionAtSunset = dblTempSunDirection;
      }

      //  // SAMPLE CODE      double dblLatitude;
      //double dblLongitude;
      //DateTime datDateWithTime = DateTime.Now;
      //double dblHoursFromGreenwich;
      //double dblSunDirection;
      //double dblSunAngleUp;
      //JenSolarConditions jenSolar;
      //double dblSunset;
      //double dblSunrise;
      //double dblSunDirectionAtSunrise;
      //double dblSunDirectionAtSunset;

      //SolarFunctions(35d, -111d, datDateWithTime, -7, out jenSolar, out dblSunrise, out dblSunset, out dblSunDirection, out dblSunAngleUp,
      //  out dblSunDirectionAtSunrise, out dblSunDirectionAtSunset);
      //Console.Write("Sun Direction in Flagstaff, at " + datDateWithTime.ToShortDateString() + "; " + datDateWithTime.ToShortTimeString() +
      //  ":  Bearing = " + dblSunDirection.ToString("0") + ", Angle = " + dblSunAngleUp.ToString("0"));
      //Console.Write("\nSunrise = " + DateTime.FromOADate(dblSunrise).ToLongTimeString());
      //Console.Write("\nSunrise = " + DateTime.FromOADate(dblSunset).ToLongTimeString());
      //Console.Write("\nSun Direction at Sunrise = " + dblSunDirectionAtSunrise.ToString("0.00") + " degrees");
      //Console.Write("\nSun Direction at Sunset = " + dblSunDirectionAtSunset.ToString("0.00") + " degrees");
    }
    public static double Return_W_AH_Sunrise_Deg(double dblLatitude, double dbl_T_Sun_Declin_Deg, out bool booCrashed)
    {
      booCrashed = false;
      try
      {
        // W2 =DEGREES(ACOS(COS(RADIANS(90.833))/(COS(RADIANS($B$3))*COS(RADIANS(T2)))-TAN(RADIANS($B$3))*TAN(RADIANS(T2))))
        // NOTE:  THIS VALUE COULD CRASH IF NO SUNRISE OR SUNSET; PAST ARCTIC OR ANTARCTIC CIRCLE AND AT THE RIGHT TIME OF YEAR
        double dbl_W_AH_Sunrise_Deg = Math.Cos(DegToRad(90.833)) / (Math.Cos(DegToRad(dblLatitude)) * Math.Cos(DegToRad(dbl_T_Sun_Declin_Deg)));
        dbl_W_AH_Sunrise_Deg -=  (Math.Tan(DegToRad(dblLatitude)) * Math.Tan(DegToRad(dbl_T_Sun_Declin_Deg)));
        dbl_W_AH_Sunrise_Deg = RadToDeg(Math.Acos(dbl_W_AH_Sunrise_Deg));
        return dbl_W_AH_Sunrise_Deg;
      }
      catch (Exception)
      {
        booCrashed = true;
        return double.NaN;
      }
    }
    ///<summary>
    ///Given two sets of projected coordinates, returns compass bearing from first point to second<br/><br/>Returns double value
    ///</summary>
    public static double CalcBearingNumbers(double dblX1, double dblY1, double dblX2, double dblY2)
    {
      //Console.WriteLine("Bearing (-111, 35, -110, 36)= " + CalcBearingNumbers(-111, 35, -110, 36) + " degrees");
      //Console.WriteLine("Bearing (-111, 35, -111, 36)= " + CalcBearingNumbers(-111, 35, -111, 36) + " degrees");
      //Console.WriteLine("Bearing (-111, 35, -112, 36) = " + CalcBearingNumbers(-111, 35, -112, 36) + " degrees");
      //Console.WriteLine("Bearing (-111, 35, -112, 35)= " + CalcBearingNumbers(-111, 35, -112, 35) + " degrees");
      //Console.WriteLine("Bearing (111, 35, 110, 34)= " + CalcBearingNumbers(111, 35, 110, 34) + " degrees");
      //Console.WriteLine("Bearing (111, 35, 111, 34)= " + CalcBearingNumbers(111, 35, 111, 34) + " degrees");
      //Console.WriteLine("Bearing (111, 35, 112, 34)= " + CalcBearingNumbers(111, 35, 112, 34) + " degrees");
      //Console.WriteLine("Bearing (111, 35, 112, 35)= " + CalcBearingNumbers(111, 35, 112, 35 ) + " degrees");
      //Console.WriteLine("Bearing (111, 35, 112, 35)= " + CalcBearingNumbers(111, 35, 111, 35) + " degrees");

      double dblXDist = dblX2 - dblX1;
      double dblYDist = dblY2 - dblY1;
      double dblReturn;
      double dblXYTandDeg;

      if ((dblXDist == 0 && dblYDist == 0))
      {
        dblXYTandDeg = Double.NaN;
      }
      else
      {
        dblXYTandDeg = RadToDeg(Math.Atan2(dblYDist, dblXDist));
      }

      dblReturn = CompassToPolar(dblXYTandDeg);
      return dblReturn;
    }
    ///<summary>
    ///Given polar direction (0 = East, increases CCW), optionally in Degrees or Radians,<br/>returns a double representing compass bearing
    ///</summary>
    public static double PolarToCompass(double dblPolar, bool booPolarInRadians = false)
    {
      if (booPolarInRadians) { dblPolar = RadToDeg(dblPolar); }
      if (90 - dblPolar < 0) { return 450 - dblPolar; }
      else { return 90 - dblPolar; }
    }
    ///<summary>
    ///Given compass direction (0 = North, increases CW), returns a double representing polar direction (0 = East, increases CCW) <br/> optionally in Degrees or Radians
    ///</summary>
    public static double CompassToPolar(double dblDegrees, bool booPolarInRadians = false)
    {
      //Console.WriteLine("CompassToPolar: 45 --> " + CompassToPolar(45) + " degrees, " + CompassToPolar(45,true) + " radians --> " +
      //  +PolarToCompass (CompassToPolar(45, true),true ) + " degrees");
      //Console.WriteLine("CompassToPolar: 15 --> " + CompassToPolar(15) + " degrees, " + CompassToPolar(15, true) + " radians --> " +
      //  +PolarToCompass(CompassToPolar(15, true), true) + " degrees");
      //Console.WriteLine("CompassToPolar: 210 --> " + CompassToPolar(210) + " degrees, " + CompassToPolar(210, true) + " radians --> " +
      //  +PolarToCompass(CompassToPolar(210, true), true) + " degrees");
      //Console.WriteLine("CompassToPolar: 270 --> " + CompassToPolar(270) + " degrees, " + CompassToPolar(270, true) + " radians --> " +
      //  +PolarToCompass(CompassToPolar(270, true), true) + " degrees");
      //Console.WriteLine("CompassToPolar: 350 --> " + CompassToPolar(350) + " degrees, " + CompassToPolar(350, true) + " radians --> " +
      //  +PolarToCompass(CompassToPolar(350, true), true) + " degrees");
      //Console.WriteLine("CompassToPolar: 0 --> " + CompassToPolar(0) + " degrees, " + CompassToPolar(0, true) + " radians --> " +
      //  +PolarToCompass(CompassToPolar(0, true), true) + " degrees");
      double dblReturn;
      dblReturn = 90 - dblDegrees;
      if (dblReturn < 0 || dblReturn > 360)
      {
        dblReturn %= 360;
      }
      if (dblReturn < 0)
      {
        dblReturn += 360;
      }
      if (booPolarInRadians) { dblReturn = DegToRad(dblReturn); }
      return dblReturn;
    }
    ///<summary>
    ///Given two sets of unprojected coordinates, returns distance in sphere radius units (often meters) <br/>
    ///from first point to second<br/><br/>Estimates distance on sphere using Haversine method<br></br>Returns double value
    ///</summary>
    public static double DistanceHaversineNumbers(double dblLat1, double dblLong1, double dblLat2, double dblLong2,
            double dblRadius = 6371000.79000915)
    {
      double dblLat;
      double dblLong;
      double dblTemp;
      double dblReturn;

      dblLat1 = DegToRad(dblLat1);
      dblLat2 = DegToRad(dblLat2);
      dblLat = dblLat1 - dblLat2;
      dblLong = DegToRad(dblLong1 - dblLong2);
      dblTemp = Math.Pow(Math.Sin(dblLat / 2), 2) + Math.Cos(dblLat1) * Math.Cos(dblLat2) * Math.Pow(Math.Sin(dblLong / 2), 2);
      dblReturn = (2 * Math.Atan2(Math.Pow(dblTemp, 0.5), Math.Pow(Math.Max(0d, 1 - dblTemp), 0.5))) * dblRadius;

      return dblReturn;
    }
    ///<summary>
    ///Given two sets of unprojected coordinates, returns distance in spheroid units (often meters) <br/> 
    ///and compass bearing (in degrees) from first point to second<br/><br/>Returns double values
    ///</summary>
    public static double DistanceHaversineNumbers(double dblLat1, double dblLong1, double dblLat2, double dblLong2,
           out double dblAzimuth, double dblRadius = 6371000.79000915)
    {
      double dblLat;
      double dblLong;
      double dblTemp;
      double dblReturn;

      dblLat1 = DegToRad(dblLat1);
      dblLat2 = DegToRad(dblLat2);
      dblLat = dblLat1 - dblLat2;
      dblLong = DegToRad(dblLong1 - dblLong2);
      dblTemp = Math.Pow(Math.Sin(dblLat / 2), 2) + Math.Cos(dblLat1) * Math.Cos(dblLat2) * Math.Pow(Math.Sin(dblLong / 2), 2);
      dblReturn = (2 * Math.Atan2(Math.Pow(dblTemp, 0.5), Math.Pow(Math.Max(0d, 1 - dblTemp), 0.5))) * dblRadius;

      double PX = DegToRad(dblLong1);
      double QX = DegToRad(dblLong2);

      double dblTheta;
      double DeltaLong;
      DeltaLong = QX - PX;

      dblTheta = Math.Atan2(Math.Sin(DeltaLong) * Math.Cos(dblLat2), Math.Cos(dblLat1) * Math.Sin(dblLat2) - Math.Sin(dblLat1) * Math.Cos(dblLat2) * Math.Cos(DeltaLong));
      dblAzimuth = RadToDeg(dblTheta);
      if (dblAzimuth < 0)
      {
        dblAzimuth += 360;
      }
      return dblReturn;
    }
    ///<summary>
    ///Given angle in degrees, returns angle in radians<br/><br/>Returns double value
    ///</summary>
    public static double DegToRad(double dblDegrees)
    {
      //return dblDegrees * 3.14159265358979 / 180;
      return dblDegrees * dblPi / 180;
    }
    ///<summary>
    ///Given angle in radians, returns angle in degrees<br/><br/>Returns double value
    ///</summary>
    public static double RadToDeg(double dblRadians)
    {
      //return dblRadians * 180 / 3.14159265358979;
      return dblRadians * 180 / dblPi;
    }
    ///<summary>
    ///Given two sets of unprojected coordinates, returns distance in spheroidal units (often meters) <br/> 
    ///and beginning and ending compass bearing (in degrees) from first point to second<br/><br/>
    ///
    /// ADAPTED FROM Vincenty, T. (1975). “Direct and inverse solutions of geodesics on the ellipsoid <br/>
    ///                                    with application of nested equations.” Surv. Rev., XXII(176), 88–93. <br/><br/>
    /// Returns double values
    ///</summary>
    public static double DistanceVincentyNumbers(double dblPX, double dblPY, double dblQX, double dblQY, out double dblStartBearing, out double dblEndBearing,
      double dblEquatorialRadius = 6378137, double dblPolarRadius = 6356752.31424518)
    {
      //' MODIFICATION OF DistanceVincentyNumbers TO ALLOW FOR ANY ELLIPSOID
      //' ADAPTED FROM Vincenty, T. (1975). “Direct and inverse solutions of geodesics on the ellipsoid
      //'                                    with application of nested equations.” Surv. Rev., XXII(176), 88–93.                                    
      //' ADAPTED FROM CHRIS VENESS; http://www.movable-type.co.uk/scripts/latlong-vincenty-direct.html

      //Console.WriteLine("Distance (-111, 35, -110, 36)= " + DistanceVincentyNumbers(-111, 35, -110, 36,out dblStartBearing, out dblEndBearing).ToString("#,##0.000") + " meters");
      //Console.WriteLine("\tStart Bearing = " + dblStartBearing.ToString("#0.000") + "\tEnd Bearing = " + dblEndBearing.ToString("#0.000"));
      //Console.WriteLine("Distance (-111, 35, -111, 36)= " + DistanceVincentyNumbers(-111, 35, -111, 36, out dblStartBearing, out dblEndBearing).ToString("#,##0.000") + " meters");
      //Console.WriteLine("\tStart Bearing = " + dblStartBearing.ToString("#0.000") + "\tEnd Bearing = " + dblEndBearing.ToString("#0.000"));
      //Console.WriteLine("Distance (-111, 35, -112, 36) = " + DistanceVincentyNumbers(-111, 35, -112, 36, out dblStartBearing, out dblEndBearing).ToString("#,##0.000") + " meters");
      //Console.WriteLine("\tStart Bearing = " + dblStartBearing.ToString("#0.000") + "\tEnd Bearing = " + dblEndBearing.ToString("#0.000"));
      //Console.WriteLine("Distance (-111, 35, -112, 35)= " + DistanceVincentyNumbers(-111, 35, -112, 35, out dblStartBearing, out dblEndBearing).ToString("#,##0.000") + " meters");
      //Console.WriteLine("\tStart Bearing = " + dblStartBearing.ToString("#0.000") + "\tEnd Bearing = " + dblEndBearing.ToString("#0.000"));
      //Console.WriteLine("Distance (111, 35, 110, 34)= " + DistanceVincentyNumbers(111, 35, 110, 34, out dblStartBearing, out dblEndBearing).ToString("#,##0.000") + " meters");
      //Console.WriteLine("\tStart Bearing = " + dblStartBearing.ToString("#0.000") + "\tEnd Bearing = " + dblEndBearing.ToString("#0.000"));
      //Console.WriteLine("Distance (111, 35, 111, 34)= " + DistanceVincentyNumbers(111, 35, 111, 34, out dblStartBearing, out dblEndBearing).ToString("#,##0.000") + " meters");
      //Console.WriteLine("\tStart Bearing = " + dblStartBearing.ToString("#0.000") + "\tEnd Bearing = " + dblEndBearing.ToString("#0.000"));
      //Console.WriteLine("Distance (111, 35, 112, 34)= " + DistanceVincentyNumbers(111, 35, 112, 34, out dblStartBearing, out dblEndBearing).ToString("#,##0.000") + " meters");
      //Console.WriteLine("\tStart Bearing = " + dblStartBearing.ToString("#0.000") + "\tEnd Bearing = " + dblEndBearing.ToString("#0.000"));
      //Console.WriteLine("Distance (111, 35, 112, 35)= " + DistanceVincentyNumbers(111, 35, 112, 35, out dblStartBearing, out dblEndBearing).ToString("#,##0.000") + " meters");
      //Console.WriteLine("\tStart Bearing = " + dblStartBearing.ToString("#0.000") + "\tEnd Bearing = " + dblEndBearing.ToString("#0.000"));
      //Console.WriteLine("Distance (111, 35, 112, 35)= " + DistanceVincentyNumbers(111, 35, 111, 35, out dblStartBearing, out dblEndBearing).ToString("#,##0.000") + " meters");
      //Console.WriteLine("\tStart Bearing = " + dblStartBearing.ToString("#0.000") + "\tEnd Bearing = " + dblEndBearing.ToString("#0.000"));

      if (dblPX == dblQX && dblPY == dblQY)
      {
        dblStartBearing = Double.NaN;
        dblEndBearing = Double.NaN;
        return 0;
      }

      double A = dblEquatorialRadius;  // SPHEROID; EQUATORIAL RADIUS
      double B = dblPolarRadius;       // SPHEROID; POLAR RADIUS

      double f = (A - B) / A;  // FLATTENING

      double dblL = DegToRad(dblQX - dblPX);
      double U1 = Math.Atan((1 - f) * (Math.Tan(DegToRad(dblPY))));    // REDUCED LATITUDE FOR POINT 1;  dblPY
      double U2 = Math.Atan((1 - f) * (Math.Tan(DegToRad(dblQY))));    // REDUCED LATITUDE FOR POINT 2;  dblQY

      double dblSinU1 = Math.Sin(U1);
      double dblCosU1 = Math.Cos(U1);
      double dblSinU2 = Math.Sin(U2);
      double dblCosU2 = Math.Cos(U2);

      double dblLambda = dblL;   // FIRST ESTIMATE OF LAMBDA
      double dblLambdaComp = 2 * dblPi;
      long lngIterations = 40;

      double sinLambda = 0;
      double cosLambda = 0;
      double sinSigma = 0;
      double cosSigma = 0;
      double Sigma = 0;
      double sinAlpha;
      double cosSqAlpha = 0;
      double cos2SigmaM = 0;
      double C;

      double dblLambda1;
      double dblLambda1a;

      while ((Math.Abs(dblLambda - dblLambdaComp) > 0.000000000001) && (lngIterations > 0))         //  VINCENTY EQUATION NUMBERS
      {
        sinLambda = Math.Sin(dblLambda);                                                           //   |
        cosLambda = Math.Cos(dblLambda);                                                           //   |
        sinSigma = Math.Pow((Math.Pow(dblCosU2 * sinLambda, 2) + Math.Pow((dblCosU1 * dblSinU2) -
                (dblSinU1 * dblCosU2 * cosLambda), 2)), 0.5);                                      //  [14]
        cosSigma = (dblSinU1 * dblSinU2) + (dblCosU1 * dblCosU2 * cosLambda);                      //  [15]
        Sigma = Math.Atan2(sinSigma, cosSigma);                                                    //  [16]
        sinAlpha = (dblCosU1 * dblCosU2 * sinLambda) / sinSigma;                                   //  [17]
        cosSqAlpha = 1 - Math.Pow(sinAlpha, 2);                                                    //  TRIG IDENTITY
        if (cosSqAlpha == 0) { cos2SigmaM = 0; }                                                  //  ADAPTED FROM VENESS
        else { cos2SigmaM = cosSigma - ((2 * dblSinU1 * dblSinU2) / cosSqAlpha); }                 //  [18]
                    
        C = (f / 16) * cosSqAlpha * (4 + (f * (4 - (3 * cosSqAlpha))));                            //  [10]
        dblLambdaComp = dblLambda;
        dblLambda1 = cos2SigmaM + C * cosSigma * (-1 + (2 * cos2SigmaM * cos2SigmaM));
        dblLambda1a = C * sinSigma * dblLambda1;

        // VINCENTY WRITES EQUATION AS "L = dblLambda - ((1 - C)...
        dblLambda = dblL + ((1 - C) * f * sinAlpha * (Sigma + dblLambda1a));                          //  [11]

        lngIterations--;
      }

      if (Math.Abs(dblLambda - dblLambdaComp) > 0.000000000001)   // failed to converge within the iteration limit
      {
        dblStartBearing = Double.NaN;
        dblEndBearing = Double.NaN;
        return 0;
      }

      double uSq = (cosSqAlpha * (Math.Pow(A, 2) - Math.Pow(B, 2))) / Math.Pow(B, 2);
      double dblA1 = (uSq * (-768 + (uSq * (320 - (175 * uSq)))));
      double dblA = 1 + ((uSq / 16384) * (4096 + dblA1));                                        // [3]
      double dblB1 = (uSq * (-128 + (uSq * (74 - (uSq * 47)))));
      double dblB = (uSq / 1024) * (256 + dblB1);                                                // [4]

      double DeltaSigma1 = ((dblB / 6) * cos2SigmaM * (-3 + 4 * Math.Pow(sinSigma, 2)) * (-3 + 4 * Math.Pow(cos2SigmaM, 2)));
      double DeltaSigma2 = ((dblB / 4) * (cosSigma * (-1 + 2 * Math.Pow(cos2SigmaM, 2)) - DeltaSigma1));
      double DeltaSigma3 = cos2SigmaM + DeltaSigma2;
      double DeltaSigma = dblB * sinSigma * DeltaSigma3;                                         // [6]
      double s = B * dblA * (Sigma - DeltaSigma);

      dblStartBearing = RadToDeg(Math.Atan2(dblCosU2 * sinLambda, (dblCosU1 * dblSinU2) - (dblSinU1 * dblCosU2 * cosLambda)));
      dblEndBearing = RadToDeg(Math.Atan2(dblCosU1 * sinLambda, -(dblSinU1 * dblCosU2) + (dblCosU1 * dblSinU2 * cosLambda)));


      if (dblStartBearing < 0) { dblStartBearing = 360 + dblStartBearing; }
      if (dblEndBearing < 0) { dblEndBearing = 360 + dblEndBearing; }

      return s;

    }
    ///<summary>
    ///Given two sets of unprojected coordinates, returns distance in spheroidal units (often meters) <br/> 
    ///from first point to second<br/><br/>
    ///
    /// ADAPTED FROM Vincenty, T. (1975). “Direct and inverse solutions of geodesics on the ellipsoid <br/>
    ///                                    with application of nested equations.” Surv. Rev., XXII(176), 88–93. <br/><br/>
    /// Returns double values
    ///</summary>
    public static double DistanceVincentyNumbers(double dblPX, double dblPY, double dblQX, double dblQY,
      double dblEquatorialRadius = 6378137, double dblPolarRadius = 6356752.31424518)
    {
      //' MODIFICATION OF DistanceVincentyNumbers TO ALLOW FOR ANY ELLIPSOID
      //' ADAPTED FROM Vincenty, T. (1975). “Direct and inverse solutions of geodesics on the ellipsoid
      //'                                    with application of nested equations.” Surv. Rev., XXII(176), 88–93.                                    
      //' ADAPTED FROM CHRIS VENESS; http://www.movable-type.co.uk/scripts/latlong-vincenty-direct.html

      if (dblPX == dblQX && dblPY == dblQY)
      {
        return 0;
      }

      double A = dblEquatorialRadius;  // SPHEROID; EQUATORIAL RADIUS
      double B = dblPolarRadius;       // SPHEROID; POLAR RADIUS

      double f = (A - B) / A;  // FLATTENING

      double l = DegToRad(dblQX - dblPX);
      double U1 = Math.Atan((1 - f) * (Math.Tan(DegToRad(dblPY))));    // REDUCED LATITUDE FOR POINT 1;  dblPY
      double U2 = Math.Atan((1 - f) * (Math.Tan(DegToRad(dblQY))));    // REDUCED LATITUDE FOR POINT 2;  dblQY

      double dblSinU1 = Math.Sin(U1);
      double dblCosU1 = Math.Cos(U1);
      double dblSinU2 = Math.Sin(U2);
      double dblCosU2 = Math.Cos(U2);

      double dblLambda = l;   // FIRST ESTIMATE OF LAMBDA
      double dblLambdaComp = 2 * dblPi;
      long lngIterations = 40;

      double sinLambda;
      double cosLambda;
      double sinSigma = 0;
      double cosSigma = 0;
      double Sigma = 0;
      double sinAlpha;
      double cosSqAlpha = 0;
      double cos2SigmaM = 0;
      double C;

      double dblLambda1;
      double dblLambda1a;

      while ((Math.Abs(dblLambda - dblLambdaComp) > 0.000000000001) && (lngIterations > 0))        //  VINCENTY EQUATION NUMBERS
      {
        sinLambda = Math.Sin(dblLambda);                                                           //   |
        cosLambda = Math.Cos(dblLambda);                                                           //   |
        sinSigma = Math.Pow((Math.Pow(dblCosU2 * sinLambda, 2) + Math.Pow((dblCosU1 * dblSinU2) -
                (dblSinU1 * dblCosU2 * cosLambda), 2)), 0.5);                                      //  [14]
        cosSigma = (dblSinU1 * dblSinU2) + (dblCosU1 * dblCosU2 * cosLambda);                      //  [15]
        Sigma = Math.Atan2(sinSigma, cosSigma);                                                    //  [16]
        sinAlpha = (dblCosU1 * dblCosU2 * sinLambda) / sinSigma;                                   //  [17]
        cosSqAlpha = 1 - Math.Pow(sinAlpha, 2);                                                    //  TRIG IDENTITY
        if (cosSqAlpha == 0) { cos2SigmaM = 0; }                                                   //  ADAPTED FROM VENESS
        else { cos2SigmaM = cosSigma - ((2 * dblSinU1 * dblSinU2) / cosSqAlpha); }                 //  [18]

        C = (f / 16) * cosSqAlpha * (4 + (f * (4 - (3 * cosSqAlpha))));                            //  [10]
        dblLambdaComp = dblLambda;
        dblLambda1 = cos2SigmaM + C * cosSigma * (-1 + (2 * cos2SigmaM * cos2SigmaM));
        dblLambda1a = C * sinSigma * dblLambda1;

        // VINCENTY WRITES EQUATION AS "L = dblLambda - ((1 - C)...
        dblLambda = l + ((1 - C) * f * sinAlpha * (Sigma + dblLambda1a));                          //  [11]

        lngIterations--;
      }

      if (Math.Abs(dblLambda - dblLambdaComp) > 0.000000000001)   // failed to converge within the iteration limit
      {
        return 0;
      }

      double uSq = (cosSqAlpha * (Math.Pow(A, 2) - Math.Pow(B, 2))) / Math.Pow(B, 2);
      double dblA1 = (uSq * (-768 + (uSq * (320 - (175 * uSq)))));
      double dblA = 1 + ((uSq / 16384) * (4096 + dblA1));                                         // [3]
      double dblB1 = (uSq * (-128 + (uSq * (74 - (uSq * 47)))));
      double dblB = (uSq / 1024) * (256 + dblB1);                                                 // [4]

      double DeltaSigma1 = ((dblB / 6) * cos2SigmaM * (-3 + 4 * Math.Pow(sinSigma, 2)) * (-3 + 4 * Math.Pow(cos2SigmaM, 2)));
      double DeltaSigma2 = ((dblB / 4) * (cosSigma * (-1 + 2 * Math.Pow(cos2SigmaM, 2)) - DeltaSigma1));
      double DeltaSigma3 = cos2SigmaM + DeltaSigma2;
      double DeltaSigma = dblB * sinSigma * DeltaSigma3;                                          // [6]
      double s = B * dblA * (Sigma - DeltaSigma);

      return s;

    }
    ///<summary>
    ///Given two sets of unprojected coordinates, returns the starting compass bearing <br/>
    ///from first point to second<br/><br/>Estimates bearing on sphere using Haversine method<br></br>Returns double value
    ///</summary>
    public static double AzimuthHaversineNumbers(double dblLong1, double dblLat1, double dblLong2, double dblLat2)
    {
      //Console.WriteLine("Haversine Bearing (-111, 35, -110, 36)= " + AzimuthHaversineNumbers(-111, 35, -110, 36).ToString("#,##0.000") + " degrees");
      //Console.WriteLine("Haversine Bearing (-111, 35, -111, 36)= " + AzimuthHaversineNumbers(-111, 35, -111, 36).ToString("#,##0.000") + " degrees");
      //Console.WriteLine("Haversine Bearing (-111, 35, -112, 36) = " + AzimuthHaversineNumbers(-111, 35, -112, 36).ToString("#,##0.000") + " degrees");
      //Console.WriteLine("Haversine Bearing (-111, 35, -112, 35)= " + AzimuthHaversineNumbers(-111, 35, -112, 35).ToString("#,##0.000") + " degrees");
      //Console.WriteLine("Haversine Bearing (111, 35, 110, 34)= " + AzimuthHaversineNumbers(111, 35, 110, 34).ToString("#,##0.000") + " degrees");
      //Console.WriteLine("Haversine Bearing (111, 35, 111, 34)= " + AzimuthHaversineNumbers(111, 35, 111, 34).ToString("#,##0.000") + " degrees");
      //Console.WriteLine("Haversine Bearing (111, 35, 112, 34)= " + AzimuthHaversineNumbers(111, 35, 112, 34).ToString("#,##0.000") + " degrees");
      //Console.WriteLine("Haversine Bearing (111, 35, 112, 35)= " + AzimuthHaversineNumbers(111, 35, 112, 35).ToString("#,##0.000") + " degrees");
      //Console.WriteLine("Haversine Bearing (111, 35, 112, 35)= " + AzimuthHaversineNumbers(111, 35, 111, 35).ToString("#,##0.000") + " degrees");
      if (dblLong1 == dblLong2 && dblLat1 == dblLat2)
      {
        return Double.NaN;
      }
      double PX = DegToRad(dblLong1);
      double PY = DegToRad(dblLat1);
      double QX = DegToRad(dblLong2);
      double QY = DegToRad(dblLat2);
      double DeltaLong = QX - PX;
      //Dim dblTheta As Double
      double dblTheta = Math.Atan2(Math.Sin(DeltaLong) * Math.Cos(QY), Math.Cos(PY) * Math.Sin(QY) - Math.Sin(PY) * Math.Cos(QY) * Math.Cos(DeltaLong));
      double dblAzimuthHaversine = RadToDeg(dblTheta);
      dblAzimuthHaversine %= 360;
      if (dblAzimuthHaversine < 0) { dblAzimuthHaversine += 360; }

      return dblAzimuthHaversine;

    }
    ///<summary>
    ///Given three sets of unprojected coordinates, returns the area of the enclosed triangle <br/>
    ///Estimates area on sphere using Haversine method<br/>Returns negative value if points are counterclockwise<br/><br/>Returns double value
    ///</summary>
    public static double SphericalTriangleArea(double dblPointAX, double dblPointAY, double dblPointBX,
      double dblPointBY, double dblPointCX, double dblPointCY, ref double dblMult,
      double dblEquatorialRadius = 6378137, double dblPolarRadius = 6356752.31424518)
    {
      // BASED ON GIRARD// S FORMULA:  Area = R ^ 2 * (A + B + C - Pi)
      //                          Where A = Angle 1
      //                                B = Angle 2
      //                                C = Angle 3
      //                   A + B + C - Pi = Spherical Excess
      //                                R = Sphere Radius
      // Trick is to get Angles A, B and C from points.
      //
      // ANOTHER FORMULATION, BASED ON DISTANCES:
      //                       Tan(E / 4) = sqrt(Tan(S / 2) * Tan((S - A) / 2) * Tan((S - B) / 2) * Tan((S - C) / 2))
      //                 Spherical Excess = E
      //                   where  a, b, c = sides of spherical triangle
      //                                S = (A + B + C) / 2
      // INITAL AZIMUTH = atn( sin (Lo2 - Lo1) / (cos (Lo2 - Lo1) sin L1 - cos L1 tan L2)
      //         http://fer3.com/arc/m2.aspx?i=1688&y=200111

      //Console.WriteLine("Spherical Triangle Area ([-111.000, 35.000], [-110.000, 36.000], [-109.000, 34.000])= " +
      //  (SphericalTriangleArea(-111.000, 35.000, -110.000, 36.000, -109.000, 34.000, ref dblMult) / 1000000).ToString("#,##0.000") + " sq. km.");// 15213.158 sq. km.
      //Console.WriteLine("Spherical Triangle Area ([-151.601, 80.696], [-129.654, -29.375], [-15.868, -87.869])= " +
      //  (SphericalTriangleArea(-151.601, 80.696, -129.654, -29.375, -15.868, -87.869, ref dblMult) / 1000000).ToString("#,##0.000") + " sq. km.");// 12867402.479 sq. km.
      //Console.WriteLine("Spherical Triangle Area ([-125.029, -23.994], [-118.886, -14.329], [175.376, 21.571])= " +
      //  (SphericalTriangleArea(-125.029, -23.994, -118.886, -14.329, 175.376, 21.571, ref dblMult) / 1000000).ToString("#,##0.000") + " sq. km.");// -5980956.616 sq. km.
      //Console.WriteLine("Spherical Triangle Area ([-134.133, 18.767], [80.822, 72.520], [-85.939, -6.878])= " +
      //  (SphericalTriangleArea(-134.133, 18.767, 80.822, 72.520, -85.939, -6.878, ref dblMult) / 1000000).ToString("#,##0.000") + " sq. km.");// 39784916.051 sq. km.
      //Console.WriteLine("Spherical Triangle Area ([-141.022, -31.115], [-130.761, -39.945], [-59.437, 14.352])= " +
      //  (SphericalTriangleArea(-141.022, -31.115, -130.761, -39.945, -59.437, 14.352, ref dblMult) / 1000000).ToString("#,##0.000") + " sq. km.");// -7589136.043 sq. km.
      //Console.WriteLine("Spherical Triangle Area ([-36.419, 6.259], [-105.532, -26.580], [62.761, -64.702])= " +
      //  (SphericalTriangleArea(-36.419, 6.259, -105.532, -26.580, 62.761, -64.702, ref dblMult) / 1000000).ToString("#,##0.000") + " sq. km.");// -56547737.589 sq. km.

      if (Math.Abs(dblPointAX - dblPointBX) < 0.000000000001 && Math.Abs(dblPointAX - dblPointCX) < 0.000000000001 ||
        Math.Abs(dblPointAX - dblPointBX) < 0.000000000001 && Math.Abs(dblPointAY - dblPointBY) < 0.000000000001 ||
        Math.Abs(dblPointAX - dblPointCX) < 0.000000000001 && Math.Abs(dblPointAY - dblPointCY) < 0.000000000001 ||
        Math.Abs(dblPointBX - dblPointCX) < 0.000000000001 && Math.Abs(dblPointBY - dblPointCY) < 0.000000000001) { return 0; }

      // SPECIAL CASE IF TWO POINTS AT POLE
      long lngPoleCounter = 0;
      if (Math.Abs(Math.Abs(dblPointAY) - 90) < 0.000000000001) { lngPoleCounter++; }
      if (Math.Abs(Math.Abs(dblPointBY) - 90) < 0.000000000001) { lngPoleCounter++; }
      if (Math.Abs(Math.Abs(dblPointCY) - 90) < 0.000000000001) { lngPoleCounter++; }
      if (lngPoleCounter > 1) { return 0; }

      double dblR = Math.Pow((Math.Pow(dblEquatorialRadius, 2) * dblPolarRadius), (1d / 3d));    // SPHERE OF SAME VOLUME RADIUS; PROPER 3-AXIS GEOMETRIC MEAN; (a^2 * b) ^ (1/3)

      //  CALCULATE LENGTHS AND BEARINGS OF GEOCURVES USING HAVERSINE FORMULA
      double dblLat1 = DegToRad(dblPointAY);
      double dblLat2 = DegToRad(dblPointBY);
      double dblLat = dblLat1 - dblLat2;
      double dblLong = DegToRad(dblPointAX - dblPointBX);
      //double dblLong2 = -dblLong;
      double dblTemp = Math.Pow((Math.Sin(dblLat / 2)), 2) + Math.Cos(dblLat1) * Math.Cos(dblLat2) * Math.Pow((Math.Sin(dblLong / 2)), 2);
      double dblAB = 2 * Math.Atan2(Math.Sqrt(dblTemp), Math.Sqrt(Math.Max(0d, 1 - dblTemp)));
      double dblAzimuthAB = Math.Atan2(Math.Sin(-dblLong) * Math.Cos(dblLat2),
            Math.Cos(dblLat1) * Math.Sin(dblLat2) - Math.Sin(dblLat1) * Math.Cos(dblLat2) * Math.Cos(-dblLong));

      dblLat1 = DegToRad(dblPointBY);
      dblLat2 = DegToRad(dblPointCY);
      dblLat = dblLat1 - dblLat2;
      dblLong = DegToRad(dblPointBX - dblPointCX);
      dblTemp = Math.Pow((Math.Sin(dblLat / 2)), 2) + Math.Cos(dblLat1) * Math.Cos(dblLat2) * Math.Pow((Math.Sin(dblLong / 2)), 2);
      double dblBC = 2 * Math.Atan2(Math.Sqrt(dblTemp), Math.Sqrt(Math.Max(0d, 1 - dblTemp)));

      dblLat1 = DegToRad(dblPointCY);
      dblLat2 = DegToRad(dblPointAY);
      dblLat = dblLat1 - dblLat2;
      dblLong = DegToRad(dblPointCX - dblPointAX);
      dblTemp = Math.Pow((Math.Sin(dblLat / 2)), 2) + Math.Cos(dblLat1) * Math.Cos(dblLat2) * Math.Pow((Math.Sin(dblLong / 2)), 2);
      double dblCA = 2 * Math.Atan2(Math.Sqrt(dblTemp), Math.Sqrt(Math.Max(0d, 1 - dblTemp)));
      double dblAzimuthAC = Math.Atan2(Math.Sin(dblLong) * Math.Cos(dblLat1),
            Math.Cos(dblLat2) * Math.Sin(dblLat1) - Math.Sin(dblLat2) * Math.Cos(dblLat1) * Math.Cos(dblLong));

      //double dblAB = DistanceHaversineNumbers(dblPointAY, dblPointAX, dblPointBY, dblPointBX, dblR);
      //double dblAzimuthAB = AzimuthHaversineNumbers(dblPointAY, dblPointAX, dblPointBY, dblPointBX);
      //double dblBC = DistanceHaversineNumbers(dblPointBY, dblPointBX, dblPointCY, dblPointCX, dblR);
      //double dblAzimuthBC = AzimuthHaversineNumbers(dblPointBY, dblPointBX, dblPointCY, dblPointCX);
      //double dblCA = DistanceHaversineNumbers(dblPointCY, dblPointCX, dblPointAY, dblPointAX, dblR);
      //double dblAzimuthAC = AzimuthHaversineNumbers(dblPointAY, dblPointAX, dblPointCY, dblPointCX);      

      if (dblAzimuthAB < 0) { dblAzimuthAB += 2 * dblPi; }
      if (dblAzimuthAC < 0) { dblAzimuthAC += 2 * dblPi; }

      double dblDiff = dblAzimuthAC - dblAzimuthAB;
      double dblMultiplier;

      if (dblDiff > 0)              //  EITHER AC > AB or AC IS TO THE LEFT OF NORTH
      {
        if (dblDiff > dblPi) { dblMultiplier = -1; }         //  THEN AC IS TO THE LEFT OF NORTH, Multiplier goes Counterclockwise
        else { dblMultiplier = 1; }                          //  THEN AC > AB, Multiplier goes Clockwise
      }
      else                             //  EITHER AC < AB or AB IS TO THE LEFT OF NORTH
      {
        if (Math.Abs(dblDiff) > dblPi) { dblMultiplier = 1; }   //  THEN AB IS TO THE LEFT OF NORTH, Multiplier goes Clockwise
        else { dblMultiplier = -1; }                         //  THEN AC < AB, Multiplier goes Counterclockwise       
      }

      double dblS = (dblAB + dblBC + dblCA) / 2;
      double dblTanEOver4 = Math.Sqrt(Math.Tan(dblS / 2) * Math.Tan((dblS - dblAB) / 2) * Math.Tan((dblS - dblBC) / 2) * Math.Tan((dblS - dblCA) / 2));
      double dblE = Math.Atan(dblTanEOver4) * 4;

      dblMult = dblMultiplier;
      return Math.Pow(dblR, 2) * dblE * dblMultiplier;
    }
  }
}
