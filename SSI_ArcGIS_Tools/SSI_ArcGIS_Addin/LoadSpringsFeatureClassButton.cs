using ArcGIS.Desktop.Framework.Contracts;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Loads the Springs feature class (World_Springs) into the active map with the standard single-symbol symbology.
    /// Same behavior as the Load Springs tool; only the symbology differs.
    /// </summary>
    internal class LoadSpringsFeatureClassButton : SpringsLayerLoaderButton
    {
        protected override string RendererJson => SpringsRendererJson;

        // Hard-coded CIM renderer JSON exported from World_Springs.lyrx.
        private const string SpringsRendererJson = @"{""type"":""CIMSimpleRenderer"",""sampleSize"":10000,""patch"":""Default"",""symbol"":{""type"":""CIMSymbolReference"",""symbol"":{""type"":""CIMPointSymbol"",""symbolLayers"":[{""type"":""CIMVectorMarker"",""enable"":true,""anchorPoint"":{""x"":0,""y"":0,""z"":0},""anchorPointUnits"":""Relative"",""dominantSizeAxis3D"":""Y"",""size"":6,""billboardMode3D"":""FaceNearPlane"",""frame"":{""xmin"":-5,""ymin"":-5,""xmax"":5,""ymax"":5},""markerGraphics"":[{""type"":""CIMMarkerGraphic"",""geometry"":{""curveRings"":[[[0,5],{""a"":[[0,5],[1.3804943107526663e-15,0],0,1]}]]},""symbol"":{""type"":""CIMPolygonSymbol"",""symbolLayers"":[{""type"":""CIMSolidStroke"",""enable"":true,""capStyle"":""Round"",""joinStyle"":""Round"",""lineStyle3D"":""Strip"",""miterLimit"":10,""width"":0.5,""height3D"":1,""anchor3D"":""Center"",""color"":{""type"":""CIMRGBColor"",""colorSpace"":{""type"":""CIMICCColorSpace"",""url"":""sRGB IEC61966-2.1""},""values"":[190,232,255,100]}},{""type"":""CIMSolidFill"",""enable"":true,""color"":{""type"":""CIMRGBColor"",""colorSpace"":{""type"":""CIMICCColorSpace"",""url"":""sRGB IEC61966-2.1""},""values"":[0,77,168,100]}}],""angleAlignment"":""Map""}}],""scaleSymbolsProportionally"":true,""respectFrame"":true}],""haloSize"":1,""scaleX"":1,""angleAlignment"":""Display""}}}";
    }
}
