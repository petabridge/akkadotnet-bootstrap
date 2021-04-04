using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Configuration;

namespace Akka.Bootstrap.Docker
{
    public static class StringExtension
    {
        public const string NotInUnquotedText = "$\"{}[]:=,#`^?!@*&\\";

        public static bool NeedQuotes(this string s)
            => s.NotQuoted() && s.Any(c => NotInUnquotedText.Contains(c));

        public static string AddQuotes(this string s)
            => "\"" + s + "\"";

        public static bool NotQuoted(this string s)
            => s.First() != '"' && s.Last() != '"';

        public static bool Quoted(this string s)
            => s.First() == '"' && s.Last() == '"';

        public static string AddQuotesIfNeeded(this string s)
            => s.NeedQuotes() ? s.AddQuotes() : s;

        public static string UnQuoteIfNeeded(this string s)
        {
            return s.NotQuoted() 
                ? s 
                : new string(s.Skip(1).Take(s.Length - 2).ToArray());
        }
    }
}
