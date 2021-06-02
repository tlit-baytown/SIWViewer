using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
/**
         * Loading from the XML file ... which has the following structure
         * <report [attrs]>
         *    <xml_version>
         *    <page>
         *      <item [attrs] >
         *          <details title="aaa">
         *      ...or...
         *      <details title="aaa">
         *          <item [attrs]>
         * 
         */
namespace SIWViewer.Custom
{
    /// <summary>
    /// This class provides helper methods for interactions with XML files. Using this class, it is possible to open and traverse one or multiple XML files.
    /// </summary>
    public class XMLHelper
    {
        /// <summary>
        /// Opens a single XML file from the specified path. If the file is not a SIW report file or the XML version is less than 1.2,
        /// an error message will be shown and this method will return <c>null</c>.
        /// </summary>
        /// <param name="path">The path to the XML file to open.</param>
        /// <returns>An <see cref="XmlDocument"/> representing the <paramref name="path"/> or <c>null</c> if an error occured.</returns>
        public static XmlDocument OpenFile(string path)
        {
            XmlDocument dom = new XmlDocument();
            dom.Load(path);

            // Verify the type of file and XML version.
            if (!"report".Equals(dom.DocumentElement.LocalName))
            {
                MessageBox.Show("The file '" + path + "' is not a SIW 'report' file !", "SIW Viewer Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            float ver = Convert.ToSingle(GetAttributeValue(dom.DocumentElement, "xml_version"), CultureInfo.CurrentCulture.NumberFormat);
            if (ver < 1.2f)
            {
                MessageBox.Show($"Only XML versions >= 1.2 are supported. The XML version you tried to open was: {ver}", "SIW Viewer Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return dom;
        }

        /// <summary>
        /// Evaluate an XPath and get the <see cref="XmlNode"/> represented by it.
        /// </summary>
        /// <param name="parent">The parent <see cref="XmlNode"/> of the path.</param>
        /// <param name="xPath">The string representing the XPath.</param>
        /// <returns>An <see cref="XmlNode"/>.</returns>
        public static XmlNode EvaluateXPath(XmlNode parent, string xPath)
        {
            return parent.SelectSingleNode(xPath);
        }

        /// <summary>
        /// Get the XPath for the specified <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="xNode">The <see cref="XmlNode"/> to get the XPath of.</param>
        /// <returns></returns>
        public static string GetXPath(XmlNode xNode)
        {
            if (xNode == null)
            {
                return null;
            }

            if (xNode.NodeType == XmlNodeType.Document)
            {
                return "/";
            }

            string xPath = xNode.LocalName;
            bool first = true;

            xNode.Attributes.Cast<XmlAttribute>().ToList().ForEach(attr =>
            {
                if (first)
                {
                    xPath += "[";
                    first = false;
                }
                else
                {
                    xPath += " and ";
                }
                xPath += $"@{attr.Name}=\"{EncodeXML(attr.Value)}\"";
            });

            if (!first)
            {
                xPath += "]";
            }

            string x = GetXPath(xNode.ParentNode);

            if (x != null)
            {
                xPath = $"{x}/{xPath}";
            }

            return xPath.ToString();
        }

        /// <summary>
        /// Encode the specified string into the correct XML representation. This encode is for the special characters: \, &lt;, &gt;, &amp;
        /// <para>All other characters are simply appeneded to the new encoded string.</para>
        /// </summary>
        /// <param name="str">The string to encode to XML.</param>
        /// <returns>The encoded XML string.</returns>
        public static string EncodeXML(string str)
        {
            StringBuilder encStr = new StringBuilder("");
            str.ToList().ForEach(e =>
            {
                string append =
                    e.Equals('\"') ? "&quot;" :
                    e.Equals('<') ? "&lt;" :
                    e.Equals('>') ? "&gt;" :
                    e.Equals('&') ? "&amp;" :
                    e.ToString();

                encStr.Append(append);
            });
            return encStr.ToString();
        }

        /// <summary>
        /// Get the max value present in the specified <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="xNode">The node to get the max value of.</param>
        /// <param name="attributeName">The attribute name to search.</param>
        /// <param name="maxValue">The current max value.</param>
        /// <returns>The max value present in <paramref name="xNode"/>.</returns>
        public static string GetMaxValue(XmlNode xNode, string attributeName, string maxValue)
        {
            if (xNode is XmlElement)
            {
                attributeName = attributeName.Replace(' ', '_');

                XmlNode attribute = xNode.Attributes.GetNamedItem(attributeName);
                if (attribute != null)
                {
                    string value = attribute.Value.Replace('\n', ' ');
                    if (value.Length > maxValue.Length)
                    {
                        maxValue = value;
                    }
                }

                foreach (XmlNode child in xNode.ChildNodes)
                {
                    string max = GetMaxValue(child, attributeName, maxValue);
                    if (max.Length > maxValue.Length)
                    {
                        maxValue = max;
                    }
                }
            }
            return maxValue;
        }

        /// <summary>
        /// Get the child elements of the specified parent node.
        /// </summary>
        /// <param name="xParentNode">The <see cref="XmlNode"/> to get the children of.</param>
        /// <returns>A list of type <see cref="XmlNode"/> containing the children nodes in the order of appearence.</returns>
        public static List<XmlNode> GetChildElements(XmlNode xParentNode)
        {
            List<XmlNode> children = new List<XmlNode>();
            if (xParentNode != null && xParentNode.HasChildNodes)
            {
                xParentNode.ChildNodes.Cast<XmlNode>().ToList().ForEach(e =>
                {
                    if (e.NodeType == XmlNodeType.Element)
                        children.Insert(0, e);
                });
            }
            else
                Console.WriteLine("No child nodes present or parent node was null!");
            //if (xParentNode != null && xParentNode.HasChildNodes)
            //{
            //    XmlNodeList nodeList = xParentNode.ChildNodes;
            //    for (int i = nodeList.Count; i >= 0; i--)
            //    {
            //        XmlNode xNode = nodeList.Item(i);
            //        if (xNode.NodeType == XmlNodeType.Element)
            //        {
            //            children.Insert(0, xNode);
            //        }
            //    }
            //}
            return children;
        }

        /// <summary>
        /// Get the value of the <paramref name="xmlNode"/>'s attribute <paramref name="attributeName"/>.
        /// </summary>
        /// <param name="xmlNode">The <see cref="XmlNode"/> where the attribute is present.</param>
        /// <param name="attributeName">The name of the attribute to get the value of.</param>
        /// <returns>The attribute value or <c>null</c> if no attribute was found.</returns>
        public static string GetAttributeValue(XmlNode xmlNode, string attributeName)
        {
            string value = null;
            xmlNode.Attributes.Cast<XmlAttribute>().ToList().ForEach(attr => {
                if (attr.Name.Equals(attributeName))
                    value = attr.Value;
            });
            return value;
        }

        /// <summary>
        /// Get the first attribute present in the specified <paramref name="xmlNode"/>.
        /// </summary>
        /// <param name="xmlNode">The <see cref="XmlNode"/> to get the first attribute of.</param>
        /// <returns>The first attribute value for the <paramref name="xmlNode"/> or <c>null</c> if the node as no attributes.</returns>
        public static string GetFirstAttributeValue(XmlNode xmlNode)
        {
            return xmlNode.Attributes.Cast<XmlAttribute>().First().Value;
        }
    }
}
