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

        /// <summary>
        /// Last-used Export Subset of Springs dialog choices. Held for the session
        /// and persisted with the project so the dialog can default to them.
        /// </summary>
        public static string LastOutputFolder { get; set; }

        public static bool LastSelectedFeaturesOnly { get; set; } = true;
        public static bool LastTrimStrings { get; set; }
        public static bool LastCreateSummary { get; set; }
        public static bool LastCreateGpx { get; set; }

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
