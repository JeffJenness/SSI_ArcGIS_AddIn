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
    /// qualifies only if it is a point feature class whose definition has a
    /// "SiteID", a "SiteName" and an "InventoryLevel" field — the same criteria
    /// used to populate the springs tools' layer lists. Used by the springs
    /// export tools and the Nearest Spring Distances tool.
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
        /// Returns the active map's qualifying springs point layers enriched with
        /// the candidate SiteID and Inventory-Level field names and the layer's
        /// selection / total feature counts — everything the Nearest Spring
        /// Distances dialog needs so it never has to touch the MCT while open.
        /// Runs on the MCT.
        /// </summary>
        internal static List<SpringsDistanceLayerItem> GatherDistanceLayers()
        {
            Map map = MapView.Active?.Map;
            if (map == null)
            {
                return new List<SpringsDistanceLayerItem>();
            }

            var items = new List<SpringsDistanceLayerItem>();
            foreach (FeatureLayer layer in map.GetLayersAsFlattenedList().OfType<FeatureLayer>())
            {
                if (layer.ShapeType != esriGeometryType.esriGeometryPoint)
                {
                    continue;
                }

                try
                {
                    using FeatureClass featureClass = layer.GetFeatureClass();
                    if (featureClass == null)
                    {
                        continue;
                    }

                    FeatureClassDefinition definition = featureClass.GetDefinition();
                    if (definition.FindField("SiteID") < 0 ||
                        definition.FindField("SiteName") < 0 ||
                        definition.FindField("InventoryLevel") < 0)
                    {
                        continue;
                    }

                    // Candidate fields for the two field lists, mirroring the legacy
                    // form: integer/small-integer fields are SiteID candidates and
                    // string fields are Inventory-Level candidates. The geometry
                    // length/area shape fields are excluded.
                    var siteIdFields = new List<string>();
                    var invLevelFields = new List<string>();
                    foreach (Field field in definition.GetFields())
                    {
                        string name = field.Name;
                        if (string.Equals(name, "Shape_Length", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(name, "Shape_Area", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (field.FieldType == FieldType.String)
                        {
                            invLevelFields.Add(name);
                        }
                        else if (field.FieldType == FieldType.Integer ||
                                 field.FieldType == FieldType.SmallInteger ||
                                 field.FieldType == FieldType.BigInteger)
                        {
                            siteIdFields.Add(name);
                        }
                    }

                    long selectionCount = layer.GetSelection()?.GetCount() ?? 0;
                    long totalCount = featureClass.GetCount();

                    items.Add(new SpringsDistanceLayerItem(
                        layer, selectionCount, totalCount, siteIdFields, invLevelFields));
                }
                catch (Exception)
                {
                    // A layer whose source cannot be opened is simply not a candidate.
                }
            }

            return items;
        }

        /// <summary>
        /// True if the layer's feature class has the "SiteID", "SiteName" and
        /// "InventoryLevel" fields (a minimal check that it is a springs feature
        /// class). Any failure to open the source is treated as "not a springs
        /// layer". Runs on the MCT.
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
                return definition.FindField("SiteID") >= 0
                    && definition.FindField("SiteName") >= 0
                    && definition.FindField("InventoryLevel") >= 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
