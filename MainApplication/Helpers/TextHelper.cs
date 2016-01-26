using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Ultrabox.ChromaSync.Helpers
{
    class TextHelper
    {


        public static string Strip(string text)
        {
            var ns = Regex.Replace(text, "<.*?>", String.Empty);
            return ns;
        }



        private static readonly Regex RE_URL = new Regex(@"(?#Protocol)(?:(?:ht|f)tp(?:s?)\:\/\/|~/|/)?(?#Username:Password)(?:\w+:\w+@)?(?#Subdomains)(?:(?:[-\w]+\.)+(?#TopLevel Domains)(?:com|org|net|gov|mil|biz|info|mobi|name|aero|jobs|museum|travel|[a-z]{2}))(?#Port)(?::[\d]{1,5})?(?#Directories)(?:(?:(?:/(?:[-\w~!$+|.,=]|%[a-f\d]{2})+)+|/)+|\?|#)?(?#Query)(?:(?:\?(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)(?:&(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)*)*(?#Anchor)(?:#(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)?");

        internal static void SetText(TextBlock text_block, string new_text)
        {
            if (text_block == null)
                return;

            text_block.Inlines.Clear();
            if (string.IsNullOrEmpty(new_text))
                return;

            // Find all URLs using a regular expression
            int last_pos = 0;
            foreach (Match match in RE_URL.Matches(new_text))
            {
                // Copy raw string from the last position up to the match
                if (match.Index != last_pos)
                {
                    var raw_text = new_text.Substring(last_pos, match.Index - last_pos);
                    text_block.Inlines.Add(new Run(raw_text));
                }

                // Create a hyperlink for the match
                var link = new Hyperlink(new Run(match.Value))
                {
                    NavigateUri = new Uri(match.Value)
                };
                link.Click += OnUrlClick;

                text_block.Inlines.Add(link);

                // Update the last matched position
                last_pos = match.Index + match.Length;
            }

            // Finally, copy the remainder of the string
            if (last_pos < new_text.Length)
                text_block.Inlines.Add(new Run(new_text.Substring(last_pos)));
        }

        private static void OnUrlClick(object sender, RoutedEventArgs e)
        {
            var link = (Hyperlink)sender;
            // Do something with link.NavigateUri like:
            Process.Start(link.NavigateUri.ToString());
        }
    }
}
