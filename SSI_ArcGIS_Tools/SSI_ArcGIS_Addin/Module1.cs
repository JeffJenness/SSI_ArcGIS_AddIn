using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SSI_ArcGIS_Addin
{
    internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("SSI_ArcGIS_Addin_Module");

        private const string LastOutputFolderKey = "ExportSubset_LastOutputFolder";
        private const string SelectedOnlyKey = "ExportSubset_SelectedFeaturesOnly";
        private const string TrimStringsKey = "ExportSubset_TrimStrings";
        private const string CreateSummaryKey = "ExportSubset_CreateSummary";
        private const string CreateGpxKey = "ExportSubset_CreateGpx";
        private const string WriteMetadataKey = "ExportSubset_WriteMetadata";

        // Nearest Spring Distances dialog choices.
        private const string DistFolderKey = "SpringDistance_LastFolder";
        private const string DistNamesKey = "SpringDistance_IncludeNames";
        private const string DistElevationsKey = "SpringDistance_IncludeElevations";
        private const string DistInfoSourceKey = "SpringDistance_IncludeInfoSource";
        private const string DistDateKey = "SpringDistance_IncludeDate";
        private const string DistInvLevelKey = "SpringDistance_IncludeInvLevel";

        /// <summary>
        /// Last-used Export Subset of Springs dialog choices. Held for the session
        /// and persisted with the project so the dialog can default to them.
        /// </summary>
        public static string LastOutputFolder { get; set; }

        public static bool LastSelectedFeaturesOnly { get; set; } = true;
        public static bool LastTrimStrings { get; set; }
        public static bool LastCreateSummary { get; set; }
        public static bool LastCreateGpx { get; set; }
        public static bool LastWriteMetadata { get; set; }

        /// <summary>
        /// Last-used Nearest Spring Distances dialog choices, persisted with the
        /// project. The five "include" options default to true (the first time the
        /// dialog is opened in a project all are checked); the login option is not
        /// persisted because it is disabled for now.
        /// </summary>
        public static string LastDistanceFolder { get; set; }

        public static bool LastDistIncludeNames { get; set; } = true;
        public static bool LastDistIncludeElevations { get; set; } = true;
        public static bool LastDistIncludeInfoSource { get; set; } = true;
        public static bool LastDistIncludeDate { get; set; } = true;
        public static bool LastDistIncludeInvLevel { get; set; } = true;

        #region Overrides

        protected override Task OnReadSettingsAsync(ModuleSettingsReader settings)
        {
            if (settings?.Get(LastOutputFolderKey) is string folder && !string.IsNullOrWhiteSpace(folder))
            {
                LastOutputFolder = folder;
            }

            LastSelectedFeaturesOnly = ReadBool(settings, SelectedOnlyKey, LastSelectedFeaturesOnly);
            LastTrimStrings = ReadBool(settings, TrimStringsKey, LastTrimStrings);
            LastCreateSummary = ReadBool(settings, CreateSummaryKey, LastCreateSummary);
            LastCreateGpx = ReadBool(settings, CreateGpxKey, LastCreateGpx);
            LastWriteMetadata = ReadBool(settings, WriteMetadataKey, LastWriteMetadata);

            if (settings?.Get(DistFolderKey) is string distFolder && !string.IsNullOrWhiteSpace(distFolder))
            {
                LastDistanceFolder = distFolder;
            }

            LastDistIncludeNames = ReadBool(settings, DistNamesKey, LastDistIncludeNames);
            LastDistIncludeElevations = ReadBool(settings, DistElevationsKey, LastDistIncludeElevations);
            LastDistIncludeInfoSource = ReadBool(settings, DistInfoSourceKey, LastDistIncludeInfoSource);
            LastDistIncludeDate = ReadBool(settings, DistDateKey, LastDistIncludeDate);
            LastDistIncludeInvLevel = ReadBool(settings, DistInvLevelKey, LastDistIncludeInvLevel);
            return Task.FromResult(0);
        }

        protected override Task OnWriteSettingsAsync(ModuleSettingsWriter settings)
        {
            if (!string.IsNullOrWhiteSpace(LastOutputFolder))
            {
                settings.Add(LastOutputFolderKey, LastOutputFolder);
            }

            settings.Add(SelectedOnlyKey, LastSelectedFeaturesOnly.ToString());
            settings.Add(TrimStringsKey, LastTrimStrings.ToString());
            settings.Add(CreateSummaryKey, LastCreateSummary.ToString());
            settings.Add(CreateGpxKey, LastCreateGpx.ToString());
            settings.Add(WriteMetadataKey, LastWriteMetadata.ToString());

            if (!string.IsNullOrWhiteSpace(LastDistanceFolder))
            {
                settings.Add(DistFolderKey, LastDistanceFolder);
            }

            settings.Add(DistNamesKey, LastDistIncludeNames.ToString());
            settings.Add(DistElevationsKey, LastDistIncludeElevations.ToString());
            settings.Add(DistInfoSourceKey, LastDistIncludeInfoSource.ToString());
            settings.Add(DistDateKey, LastDistIncludeDate.ToString());
            settings.Add(DistInvLevelKey, LastDistIncludeInvLevel.ToString());
            return Task.FromResult(0);
        }

        private static bool ReadBool(ModuleSettingsReader settings, string key, bool fallback)
        {
            object value = settings?.Get(key);
            if (value is bool b)
            {
                return b;
            }

            return value is string s && bool.TryParse(s, out bool parsed) ? parsed : fallback;
        }

        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}
