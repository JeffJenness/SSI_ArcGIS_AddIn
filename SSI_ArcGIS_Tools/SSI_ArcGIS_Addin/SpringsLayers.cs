using System;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Shared discovery of springs feature layers in the active map. A layer
    /// qualifies only if it is a point feature class whose definition has both a
    /// "SiteID" and a "SiteName" field — the same criteria used to populate the
    /// Export Geodatabase tool's layer list. Used by the springs export tools.
    /// </summary>
    internal static class SpringsLayers
    {
        /// <summary>
        /// Returns the active map's point feature layers that qualify as springs
        /// layers, each with its current selection count. Runs on the MCT.
        /// </summary>
        internal static List<SpringsLayerItem> GatherPointSpringsLayers()
        {
            Map map = MapView.Active?.Map;
            if (map == null)
            {
                return new List<SpringsLayerItem>();
            }

            var items = new List<SpringsLayerItem>();
            foreach (FeatureLayer layer in map.GetLayersAsFlattenedList().OfType<FeatureLayer>())
            {
                if (layer.ShapeType != esriGeometryType.esriGeometryPoint)
                {
                    continue;
                }

                if (!HasSpringsKeyFields(layer))
                {
                    continue;
                }

                long count = layer.GetSelection()?.GetCount() ?? 0;
                items.Add(new SpringsLayerItem(layer, count));
            }

            return items;
        }

        /// <summary>
        /// True if the layer's feature class has both the "SiteID" and "SiteName"
        /// fields (a minimal check that it is a springs feature class). Any failure
        /// to open the source is treated as "not a springs layer". Runs on the MCT.
        /// </summary>
        private static bool HasSpringsKeyFields(FeatureLayer layer)
        {
            try
            {
                using FeatureClass featureClass = layer.GetFeatureClass();
                if (featureClass == null)
                {
                    return false;
                }

                FeatureClassDefinition definition = featureClass.GetDefinition();
                return definition.FindField("SiteID") >= 0 && definition.FindField("SiteName") >= 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
