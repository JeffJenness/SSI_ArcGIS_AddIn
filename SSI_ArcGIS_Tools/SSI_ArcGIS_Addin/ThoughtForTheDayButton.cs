using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ArcGIS.Desktop.Framework.Contracts;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Ribbon button for the "Thought for the Day" toy: picks a random inspirational
    /// quote from the deployed Inspirational_Quotes.json and shows it in the
    /// non-modal <see cref="QuoteWindow"/> card. Runs entirely on the UI thread.
    /// </summary>
    internal sealed class ThoughtForTheDayButton : Button
    {
        private const string QuotesFileName = "Inspirational_Quotes.json";

        private static readonly Random Rng = new();
        private static List<QuoteEntry> _quotes;

        protected override void OnClick()
        {
            try
            {
                List<QuoteEntry> quotes = LoadQuotes();
                if (quotes == null || quotes.Count == 0)
                {
                    MessageBox.Show("No inspirational quotes are available.", "Thought for the Day");
                    return;
                }

                QuoteEntry pick = quotes[Rng.Next(quotes.Count)];
                QuoteWindow.ShowQuote(pick.Quote, pick.Author);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not show a thought for the day:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Thought for the Day");
            }
        }

        /// <summary>
        /// Loads (and caches) the quotes from the JSON file deployed in the add-in's
        /// Metadata folder.
        /// </summary>
        private static List<QuoteEntry> LoadQuotes()
        {
            if (_quotes != null)
            {
                return _quotes;
            }

            string installDir = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(installDir ?? string.Empty, "Metadata", QuotesFileName);
            if (!File.Exists(path))
            {
                return _quotes = new List<QuoteEntry>();
            }

            string json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _quotes = JsonSerializer.Deserialize<List<QuoteEntry>>(json, options) ?? new List<QuoteEntry>();
            return _quotes;
        }

        /// <summary>One entry from Inspirational_Quotes.json.</summary>
        private sealed class QuoteEntry
        {
            public string Quote { get; set; }
            public string Author { get; set; }
        }
    }
}
