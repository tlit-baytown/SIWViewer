using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;

namespace SIWViewer.Custom
{
    public class DetailTreeNode
    {
        private readonly XmlNode xNode;
        private readonly XmlNode pgNode;
        private List<DetailTreeNode> children;

        private string[] headers = new string[19];

        /// <summary>
        /// // The Name is used by the tree to display the first tree column. This is always H1.
        /// </summary>
        public string Name
        {
            get
            {
                return headers[0];
            }
        }

        public string H1 { get { return headers[0]; } }
        public string H2 { get { return headers[1]; } }
        public string H3 { get { return headers[2]; } }
        public string H4 { get { return headers[3]; } }
        public string H5 { get { return headers[4]; } }
        public string H6 { get { return headers[5]; } }
        public string H7 { get { return headers[6]; } }
        public string H8 { get { return headers[7]; } }
        public string H9 { get { return headers[8]; } }
        public string H10 { get { return headers[9]; } }
        public string H11 { get { return headers[10]; } }
        public string H12 { get { return headers[11]; } }
        public string H13 { get { return headers[12]; } }
        public string H14 { get { return headers[13]; } }
        public string H15 { get { return headers[14]; } }
        public string H16 { get { return headers[15]; } }
        public string H17 { get { return headers[16]; } }
        public string H18 { get { return headers[17]; } }
        public string H19 { get { return headers[18]; } }
        public string H20 { get { return headers[19]; } }


        /// <summary>
        /// Get the children nodes associated with this <see cref="DetailTreeNode"/>.
        /// </summary>
        public IEnumerable<DetailTreeNode> ChildNodes
        {
            get
            {
                if (children != null)
                    return children;

                children = new List<DetailTreeNode>();
                xNode.ChildNodes.Cast<XmlNode>().ToList().ForEach(e => {
                    if (e.NodeType == XmlNodeType.Element)
                        children.Add(new DetailTreeNode(e));
                });
                return children;
            }
        }

        /// <summary>
        /// Get a value indicating if this <see cref="DetailTreeNode"/> is considered a leaf node. A leaf node is one who doesn't have
        /// any <see cref="ChildNodes"/>.
        /// </summary>
        public bool IsLeaf
        {
            get
            {
                return xNode.ChildNodes.Count == 0;
            }
        }

        public DetailTreeNode(XmlNode xNode)
        {
            this.xNode = xNode;
            pgNode = GetParentPageElement(this.xNode);

            InitializeHeaders();
        }

        private void InitializeHeaders()
        {
            for (int i = 0; i < headers.Length; i++)
                headers[i] = H(i + 1);
        }

        private string H(int i)
        {
            XmlNode attributeHeader = pgNode.Attributes.GetNamedItem("H" + i);
            if (attributeHeader == null)
                return null;

            string attributeName = attributeHeader.Value.Replace(' ', '_');
            XmlNode attributeNode = xNode.Attributes.GetNamedItem(attributeName);
            if (attributeNode == null)
                return null;

            return attributeNode.Value.Replace('\n', ' ');
        }

        private XmlNode GetParentPageElement(XmlNode xmlNode)
        {
            while (xmlNode != null)
            {
                if (xmlNode.Name.Equals("page"))
                    return xmlNode;
                xmlNode = xmlNode.ParentNode;
            }
            return null;
        }
    }
}
