using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SIWViewer.Custom
{
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

            float ver = Convert.ToSingle(GetAttribute(dom.DocumentElement, "xml_version"), CultureInfo.CurrentCulture.NumberFormat);
            if (ver < 1.2f)
            {
                MessageBox.Show($"Only XML versions >= 1.2 are supported. The XML version you tried to open was: {ver}", "SIW Viewer Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return dom;
        }

        public static XmlNode EvaluateXPath(XmlNode parent, string xPath)
        {
            return parent.SelectSingleNode(xPath);
        }

        public string GetXPath(XmlNode xnode)
        {
            if (xnode == null)
            {
                return null;
            }
            if (xnode.NodeType == XmlNodeType.Document)
            {
                return "/";
            }

            string xp = xnode.LocalName;
            bool first = true;
            foreach (XmlAttribute a in xnode.Attributes)
            {
                if (first)
                {
                    xp += "[";
                    first = false;
                }
                else
                {
                    xp += " and ";
                }
                xp += "@" + a.Name + "=\"" + EncodeXML(a.Value) + "\"";
            }
            if (!first)
            {
                xp += "]";
            }
            string x = GetXPath(xnode.ParentNode);
            if (x != null)
            {
                xp = x + "/" + xp;
            }
            return xp;
        }

        public static string EncodeXML(string str)
        {
            StringBuilder encStr = new StringBuilder("");
            char[] array = str.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                switch (array[i])
                {
                    case '\"':
                        encStr.Append("&quot;");
                        break;
                    case '<':
                        encStr.Append("&lt;");
                        break;
                    case '>':
                        encStr.Append("&gt;");
                        break;
                    case '&':
                        encStr.Append("&amp;");
                        break;
                    default:
                        encStr.Append(array[i]);
                        break;
                }
            }
            return encStr.ToString();
        }

        public static string GetMaxValue(XmlNode xNode, string attrNm, string maxValue)
        {
            if (xNode is XmlElement)
            {
                attrNm = attrNm.Replace(' ', '_');

                XmlNode attr = xNode.Attributes.GetNamedItem(attrNm);
                if (attr != null)
                {
                    string value = attr.Value.Replace('\n', ' ');
                    if (value.Length > maxValue.Length)
                    {
                        maxValue = value;
                    }
                }
                foreach (XmlNode kid in xNode.ChildNodes)
                {
                    string max = GetMaxValue(kid, attrNm, maxValue);
                    if (max.Length > maxValue.Length)
                    {
                        maxValue = max;
                    }
                }
            }
            return maxValue;
        }

        public static List<XmlNode> GetChildElements(XmlNode xParentNode)
        {
            List<XmlNode> kids = new List<XmlNode>();
            if (xParentNode != null && xParentNode.HasChildNodes)
            {
                XmlNodeList nodeList = xParentNode.ChildNodes;
                for (int i = nodeList.Count; --i >= 0;)
                {
                    XmlNode xNode = nodeList.Item(i);
                    if (xNode.NodeType == XmlNodeType.Element)
                    {
                        kids.Insert(0, xNode);
                    }
                }
            }
            return kids;
        }

        public static string GetAttribute(XmlNode xmlNode, string attrName)
        {
            for (int i = xmlNode.Attributes.Count; --i >= 0;)
            {
                if (xmlNode.Attributes.Item(i).Name.Equals(attrName))
                {
                    return xmlNode.Attributes.Item(i).Value;
                }
            }
            return null;
        }

        public static string GetFirstAttribute(XmlNode xmlNode)
        {
            if (xmlNode.Attributes.Count > 0)
            {
                return xmlNode.Attributes.Item(0).Value;
            }
            return null;
        }
    }
}
