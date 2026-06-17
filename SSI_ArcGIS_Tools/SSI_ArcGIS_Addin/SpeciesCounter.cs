using System;
using System.Collections.Generic;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Counts distinct taxa at species/genus/family/order levels for a set of
    /// taxon observations, faithfully reproducing the legacy SummarizeSpeciesArray
    /// (SSI_FeatureServiceFunctions.bas:7145). Counting uses hierarchical
    /// concatenated keys (order → family → genus → species) so that, e.g.,
    /// "Lepidoptera Hesperiidae" is not double-counted when
    /// "Lepidoptera Hesperiidae Zestusa dorus" already exists, while a
    /// higher-level-only observation that is not represented at a lower level
    /// adds to the lower-level counts.
    ///
    /// NOTE: the higher-level prefix (order/family/genus) deliberately carries
    /// forward from the previous observation when a value is null/blank — this
    /// matches the legacy behavior exactly.
    /// </summary>
    internal static class SpeciesCounter
    {
        internal readonly struct Counts
        {
            internal Counts(int species, int genus, int family, int order)
            {
                Species = species;
                Genus = genus;
                Family = family;
                Order = order;
            }

            internal int Species { get; }
            internal int Genus { get; }
            internal int Family { get; }
            internal int Order { get; }
        }

        /// <summary>
        /// Each observation is the taxon record's values; the *Index arguments are
        /// the column positions of the order/family/genus/species fields. Pass
        /// <paramref name="orderIndex"/> = -1 when there is no order level (flora).
        /// </summary>
        internal static Counts Count(
            IReadOnlyList<object[]> observations,
            int orderIndex, int familyIndex, int genusIndex, int speciesIndex)
        {
            var orderKeys = new HashSet<string>();
            var familyKeys = new HashSet<string>();
            var genusKeys = new HashSet<string>();
            var speciesKeys = new HashSet<string>();

            var orderLevel = new List<string>();                                 // orderKey
            var familyLevel = new List<(string family, string order)>();
            var genusLevel = new List<(string genus, string family, string order)>();
            var speciesLevel = new List<(string species, string genus, string family, string order)>();

            // Prefixes persist across observations (legacy does not reset them).
            string strOrder = string.Empty;
            string strFamily = string.Empty;
            string strGenus = string.Empty;

            foreach (object[] obs in observations)
            {
                if (orderIndex != -1)
                {
                    string v = Text(obs, orderIndex);
                    if (v != null)
                    {
                        strOrder = v + " ";
                        if (orderKeys.Add(strOrder))
                        {
                            orderLevel.Add(strOrder);
                        }
                    }
                }

                string fv = Text(obs, familyIndex);
                if (fv != null)
                {
                    strFamily = (strOrder + fv).Trim() + " ";
                    if (familyKeys.Add(strFamily))
                    {
                        familyLevel.Add((strFamily, strOrder));
                    }
                }

                string gv = Text(obs, genusIndex);
                if (gv != null)
                {
                    strGenus = (strFamily + gv).Trim() + " ";
                    if (genusKeys.Add(strGenus))
                    {
                        genusLevel.Add((strGenus, strFamily, strOrder));
                    }
                }

                string sv = Text(obs, speciesIndex);
                if (sv != null)
                {
                    string strSpecies = (strGenus + sv).Trim();
                    if (speciesKeys.Add(strSpecies))
                    {
                        speciesLevel.Add((strSpecies, strGenus, strFamily, strOrder));
                    }
                }
            }

            int family = familyLevel.Count;
            int genus = genusLevel.Count;
            int species = speciesLevel.Count;
            int order = orderIndex != -1 ? orderLevel.Count : 0;

            // Order-level values not represented in any family imply new family/genus/species.
            if (orderIndex != -1)
            {
                foreach (string o in orderLevel)
                {
                    string trimmed = o.Trim();
                    bool found = false;
                    foreach (var f in familyLevel)
                    {
                        if (trimmed == f.order.Trim()) { found = true; break; }
                    }

                    if (!found)
                    {
                        family++;
                        genus++;
                        species++;
                    }
                }
            }

            // Family-level values not represented in any genus imply new genus/species.
            foreach (var f in familyLevel)
            {
                string trimmed = f.family.Trim();
                bool found = false;
                foreach (var g in genusLevel)
                {
                    if (trimmed == g.family.Trim()) { found = true; break; }
                }

                if (!found)
                {
                    genus++;
                    species++;
                }
            }

            // Genus-level values not represented in any species imply a new species.
            foreach (var g in genusLevel)
            {
                string trimmed = g.genus.Trim();
                bool found = false;
                foreach (var s in speciesLevel)
                {
                    if (trimmed == s.genus.Trim()) { found = true; break; }
                }

                if (!found)
                {
                    species++;
                }
            }

            return new Counts(species, genus, family, order);
        }

        /// <summary>Trimmed string value of obs[index], or null if missing/blank.</summary>
        private static string Text(object[] obs, int index)
        {
            if (index < 0 || index >= obs.Length)
            {
                return null;
            }

            object v = obs[index];
            if (v == null || v is DBNull)
            {
                return null;
            }

            string s = v.ToString().Trim();
            return s.Length == 0 ? null : s;
        }
    }
}
