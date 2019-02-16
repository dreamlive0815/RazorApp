
using System;
using System.Collections.Generic;

using HtmlAgilityPack;

namespace Extensions
{
    public static class StringXPathExtension
    {
        public static string XPathSingleAttr(this string s, string xpath, string attribute)
        {
            return XPathSingle(s, xpath)?.GetAttributeValue(attribute, null);
        }

        public static string XPathSingleHtml(this string s, string xpath)
        {
            return XPathSingle(s, xpath)?.InnerHtml;
        }

        public static HtmlNode XPathSingle(this string s, string xpath)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(s);
            return doc.DocumentNode.SelectSingleNode(xpath);
        }

        public static List<string> XPathHtml(this string s, string xpath)
        {
            var nodes = XPath(s, xpath);
            var list = new List<string>();
            if (nodes == null) return list;
            foreach (var node in nodes) {
                list.Add(node.InnerHtml);
            }
            return list;
        }

        public static HtmlNodeCollection XPath(this string s, string xpath)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(s);
            return doc.DocumentNode.SelectNodes(xpath);
        }
    }

    public static class HtmlNodeExtension
    {
        public static string SelectSingleNodeAttr(this HtmlNode node, string xpath, string attribute)
        {
            return node?.SelectSingleNode(xpath)?.GetAttributeValue(attribute, null);
        }

        public static string SelectSingleNodeHtml(this HtmlNode node, string xpath)
        {
            return node?.SelectSingleNode(xpath)?.InnerHtml;
        }

        public static List<string> SelectNodesHtml(this HtmlNode node, string xpath)
        {
            var nodes = node?.SelectNodes(xpath);
            var list = new List<string>();
            if (nodes == null) return list;
            foreach (var n in nodes) {
                list.Add(n.InnerHtml);
            }
            return list;
        }
    }
}