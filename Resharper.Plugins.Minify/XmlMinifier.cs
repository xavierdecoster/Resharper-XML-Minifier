using System;
using System.Text.RegularExpressions;

namespace Resharper.Plugins.Minify
{
    public class XmlMinifier : IMinifier
    {
        public string Minify(string input)
        {
            // Remove carriage returns, tabs and whitespaces
            string output = Regex.Replace(input, @"\n|\t", " ");
            output = Regex.Replace(output, @">\s+<", "><").Trim();
            output = Regex.Replace(output, @"\s{2,}", " ");

            // Remove XML comments 
            output = Regex.Replace(output, "<!--.*?-->", String.Empty, RegexOptions.Singleline);

            return output;
        }
    }
}