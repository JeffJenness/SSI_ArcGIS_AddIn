using System;
using System.Collections.Generic;
using System.Globalization;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Holds the in-memory sets of key values that are built while copying one
    /// dataset and consulted when filtering later datasets, plus the two
    /// exclusion sets derived from the dialog's optional SQL clauses. Mirrors the
    /// pSaveSites / pSaveSurveys / pSaveHUC8s / pSaveHUC12s / pGedFloraTIDs /
    /// pExclude* collections of the legacy CopyAndDeleteNonSelectedSprings2.
    ///
    /// All keys are canonical strings produced by <see cref="KeyString"/> so that
    /// numeric ids compare consistently regardless of their storage type.
    /// </summary>
    internal sealed class KeepSetStore
    {
        private readonly Dictionary<KeepSet, HashSet<string>> _sets;

        internal KeepSetStore()
        {
            _sets = new Dictionary<KeepSet, HashSet<string>>();
            foreach (KeepSet set in Enum.GetValues(typeof(KeepSet)))
            {
                _sets[set] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            ExcludeSiteIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ExcludeSurveyIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>SiteIDs to drop (from the "exclude surveys for certain springs" SQL clause).</summary>
        internal HashSet<string> ExcludeSiteIds { get; }

        /// <summary>SurveyIDs to drop (from the "exclude certain surveys" SQL clause).</summary>
        internal HashSet<string> ExcludeSurveyIds { get; }

        internal HashSet<string> Get(KeepSet set) => _sets[set];

        internal void Add(KeepSet set, string key) => _sets[set].Add(key);

        internal bool Contains(KeepSet set, string key) => _sets[set].Contains(key);

        internal int Count(KeepSet set) => _sets[set].Count;

        /// <summary>
        /// Canonical string form of a key value: null/DBNull → null; integral
        /// numeric values → plain integer text (so 12.0 and 12 match, and a long
        /// stored as text matches the same long stored as a number); everything
        /// else → trimmed invariant string.
        /// </summary>
        internal static string KeyString(object value)
        {
            if (value == null || value is DBNull)
            {
                return null;
            }

            switch (value)
            {
                case long l:
                    return l.ToString(CultureInfo.InvariantCulture);
                case int i:
                    return i.ToString(CultureInfo.InvariantCulture);
                case short s:
                    return s.ToString(CultureInfo.InvariantCulture);
                case byte b:
                    return b.ToString(CultureInfo.InvariantCulture);
                case double d:
                    return IsIntegral(d) ? ((long)d).ToString(CultureInfo.InvariantCulture)
                                         : d.ToString(CultureInfo.InvariantCulture);
                case float f:
                    return IsIntegral(f) ? ((long)f).ToString(CultureInfo.InvariantCulture)
                                         : f.ToString(CultureInfo.InvariantCulture);
                case decimal m:
                    return m == Math.Floor(m) ? ((long)m).ToString(CultureInfo.InvariantCulture)
                                              : m.ToString(CultureInfo.InvariantCulture);
                default:
                    return value.ToString().Trim();
            }
        }

        private static bool IsIntegral(double d) =>
            !double.IsNaN(d) && !double.IsInfinity(d) && d == Math.Floor(d)
            && d >= long.MinValue && d <= long.MaxValue;
    }
}
