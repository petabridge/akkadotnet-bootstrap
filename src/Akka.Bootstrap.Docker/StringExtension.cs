using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Akka.Bootstrap.Docker
{
    public static class StringExtension
    {
        public const string NotInUnquotedText = "$\"{}[]:=,#`^?!@*&\\";
        public const string NewLine = "\n\r";

        public static bool NeedQuotes(this string s)
        {
            return s.Any(c => NotInUnquotedText.Contains(c));
        }

        public static bool NeedTripleQuotes(this string s)
        {
            return s.NeedQuotes() && s.Any(c => NewLine.Contains(c));
        }

        public static string AddQuotes(this string s)
        {
            if (s.Contains('"'))
                return "\"" + s.Replace("\"", "\\\"") + "\"";

            return "\"" + s + "\"";
        }

        public static string AddTripleQuotes(this string s)
        {
            return "\"\"" + s.AddQuotes() + "\"\"";
        }
    }
}
