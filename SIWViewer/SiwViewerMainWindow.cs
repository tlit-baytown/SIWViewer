using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Aga.Controls.Tree.NodeControls;
using SIWViewer.Custom;

namespace SIWViewer
{
    public partial class SiwViewerMainWindow : Form
    {
        private XmlDocument dom;

        public SiwViewerMainWindow()
        {
            InitializeComponent();
            // add the listeners to the tree...
            treeView.BeforeExpand += new TreeViewCancelEventHandler(treeView1_BeforeExpand);
            treeView.AfterSelect += new TreeViewEventHandler(treeView1_AfterSelect);
            // some initializations...
            InitControls();
//            System.Environment.GetCommandLineArgs();
            string[] arguments = Environment.GetCommandLineArgs();
            if (arguments.GetLength(0) == 2)
            {
                ProcessFile(arguments[1]);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutDialog dlg = new AboutDialog();
            dlg.ShowDialog(this);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FilterIndex = 1;
            // dlg.Filter = "SIW Files (*.xml) | *.xml|SIW Files (*.csv) | *.csv";
            dlg.Filter = "SIW Files (*.xml) | *.xml";
            if (DialogResult.OK == dlg.ShowDialog(this))
            {
                string fileName = dlg.FileName;
                ProcessFile(fileName);
            }
        }

        private void ProcessFile(string fileName)
        {
            // implement the open...
            try
            {
                dom = XMLHelper.OpenFile(fileName);
                Text = "SIW Viewer [ " + fileName + "] - \\\\" + XMLHelper.GetAttributeValue(dom.DocumentElement, "computer_name");

                // SECTION 2. Initialize the TreeView control.
                treeView.Nodes.Clear();
                // add the root...
                string name = XMLHelper.GetAttributeValue(dom.DocumentElement, "Title");
                int type = GetNodeType(dom.DocumentElement);
                treeView.Nodes.Add(new CTreeNode(name, type));
                CTreeNode tNode = (CTreeNode)treeView.Nodes[0];
                tNode.Tag = XMLHelper.GetXPath(dom.DocumentElement);

                // SECTION 3. Populate the TreeView with the DOM nodes.
                AddNextLevelNodes(tNode);
                try
                {
                    tNode.Nodes[2].ExpandAll();
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                try
                {
                    tNode.Nodes[1].ExpandAll();
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                try
                {
                    tNode.Nodes[0].ExpandAll();
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                tNode.Expand();

                // SECTION 4: some initializations...
                InitControls();
            }
            catch (XmlException xmlEx)
            {
                MessageBox.Show(xmlEx.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private int GetNodeType(XmlNode node)
        {
            string sType = XMLHelper.GetAttributeValue(node, "__type__");
            int type = sType == null ? 1 : Convert.ToInt32(sType);
            return type;
        }

        private void InitControls()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();
            dataGridView2.Columns.Clear();
            dataGridView2.Rows.Clear();
            splitContainer2.Panel2.Hide();
            splitContainer2.Panel2Collapsed = true;
            splitContainer2.Panel2.Enabled = false;
            splitContainer2.SplitterDistance = splitContainer2.Size.Height;
            treeViewDetails.Hide();
        }

        private void AddNextLevelNodes(CTreeNode tParentNode)
        {
            string xp = tParentNode.Tag.ToString();
            if (xp == null)
            {
                return;
            }

            XmlNode xParentNode = XMLHelper.EvaluateXPath(dom, xp);
            // Add the nodes to the TreeView during the looping process.
            if (xParentNode != null && xParentNode.HasChildNodes)
            {
                if ("page".Equals(xParentNode.Name))
                {
                    return; // don't go deeper that the "page" element...
                }
                XmlNodeList nodeList = xParentNode.ChildNodes;
                int size = nodeList.Count - 1;
                for (int i = 0; i <= size; i++)
                {
                    XmlNode xNode = xParentNode.ChildNodes[i];
                    if (xNode.NodeType == XmlNodeType.Element)
                    {
                        CTreeNode tnode = new CTreeNode(XMLHelper.GetAttributeValue(xNode, "Title"), GetNodeType(xNode));
                        tnode.Tag = XMLHelper.GetXPath(xNode);
                        tParentNode.Nodes.Add(tnode);

                        CTreeNode fakeNode = new CTreeNode("loading...", 1);
                        tnode.Nodes.Add(fakeNode);
                    }
                }
            }
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            CTreeNode tNode = (CTreeNode)e.Node;
            if (tNode.Nodes.Count == 1 && "loading...".Equals(tNode.Nodes[0].Text))
            {
                tNode.Nodes.Clear();
                AddNextLevelNodes(tNode);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            CTreeNode tNode = (CTreeNode)e.Node;
            string xp = tNode.Tag.ToString();
            XmlNode xmlNode = XMLHelper.EvaluateXPath(dom, xp);

            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();

            if (xmlNode != null && "page".Equals(xmlNode.LocalName))
            {
                // hide the second grid, end expand the container
                if (splitContainer2.Panel2.Enabled)
                {
                    splitContainer2.Panel2Collapsed = true;
                    splitContainer2.Panel2.Hide();
                    splitContainer2.SplitterDistance = splitContainer2.Size.Height;
                    splitContainer2.Panel2.Enabled = false;
                }

                if (tNode.TypeNode == 2)
                {
                    dataGridView1.Hide();
                    treeViewDetails.Show();

                    PopulateDetailTree(xmlNode);
                }
                else
                {
                    treeViewDetails.Hide();
                    dataGridView1.Show();

                    PopulateDetailTables(xmlNode);
                }
            }
            else
            {
                InitControls();
            }
            label1.Text = tNode.Text; // +" (type:" + tNode.TypeNode + ")";
            //Console.Out.WriteLine("selected xmlNode = " + (xmlNode != null ? xmlNode.Name : "null") + ", xp = " + xp);
        }

        private void PopulateDetailTree(XmlNode xmlNode)
        {
            treeViewDetails.Columns.Clear();
            treeViewDetails.NodeControls.Clear();

            treeViewDetails.Model = CreateTreeModel(xmlNode);
            treeViewDetails.ExpandAll();
        }

        private DetailsTreeModel CreateTreeModel(XmlNode pageElem)
        {
            // create the columns first
            int cols = 0;
            for (int i = 0; i < pageElem.Attributes.Count; i++)
            {
                XmlNode a = pageElem.Attributes.Item(i);
                if (a.Name.StartsWith("H"))
                {
                    Aga.Controls.Tree.TreeColumn tc = new Aga.Controls.Tree.TreeColumn();
                    tc.Header = a.Value;

                    // compute the width of the max value.
                    string maxValue = XMLHelper.GetMaxValue(pageElem, a.Value, "");
                    SizeF width = treeViewDetails.CreateGraphics().MeasureString(maxValue, treeViewDetails.Font);
                    tc.Width = 20 + width.ToSize().Width + (cols == 0 ? 50 : 0);

                    treeViewDetails.Columns.Add(tc);

                    NodeTextBox columnControl = new NodeTextBox();
                    if (cols > 0)
                    {
                        columnControl.Column = cols;
                    }
                    columnControl.DataPropertyName = a.Name;

                    treeViewDetails.NodeControls.Add(columnControl);

                    cols++;
                }
            }

            // return the model
            return new DetailsTreeModel(new DetailTreeNode(pageElem));
        }

        

        private void PopulateDetailTables(XmlNode xmlNode)
        {
            List<XmlNode> kids = XMLHelper.GetChildElements(xmlNode);
            XmlNode itemEl = null;
            for (int i = kids.Count; --i >= 0; )
            {
                if (itemEl == null)
                {
                    itemEl = kids[i];
                }
                else if (itemEl.Attributes.Count < kids[i].Attributes.Count)
                {
                    itemEl = kids[i];
                }
            }

            // creates the columns
            if (itemEl != null)
            {
                for (int i = 0; i < itemEl.Attributes.Count; i++)
                {
                    string attr_nm = itemEl.Attributes.Item(i).Name;
                    dataGridView1.Columns.Add(attr_nm, XMLHelper.GetAttributeValue(xmlNode, "H" + (i + 1)));
                }
            }
            else
            {
                for (int i = 1; i < 50; i++)
                {
                    string colNm = XMLHelper.GetAttributeValue(xmlNode, "H" + i);
                    if (colNm == null)
                    {
                        break;
                    }
                    else
                    {
                        dataGridView1.Columns.Add("H" + i, colNm);
                    }
                }
            }
            // add the rows...
            if (xmlNode.HasChildNodes)
            {
                XmlNodeList nodeList = xmlNode.ChildNodes;
                int size = nodeList.Count - 1;
                for (int i = 0; i <= size; i++)
                {
                    XmlNode xNode = nodeList.Item(i);
                    if (xNode.NodeType == XmlNodeType.Element)
                    {
                        object[] rowValues = new object[dataGridView1.Columns.Count];
                        for (int j = 0; j < xNode.Attributes.Count; j++)
                        {
                            rowValues[j] = xNode.Attributes.Item(j).Value;
                        }
                        dataGridView1.Rows.Add(rowValues);
                    }
                }
            }
        }

        

        private void AddNode(XmlNode inXmlNode, CTreeNode inTreeNode)
        {
            XmlNode xNode;
            CTreeNode tNode;
            XmlNodeList nodeList;
            int i;
            Console.Out.WriteLine("Date " + System.DateTime.Now + " tree.node = " + inTreeNode);

            // Loop through the XML nodes until the leaf is reached.
            // Add the nodes to the TreeView during the looping process.
            if (inXmlNode.HasChildNodes)
            {
                nodeList = inXmlNode.ChildNodes;
                int size = nodeList.Count - 1;
                for (i = 0; i <= size; i++)
                {
                    xNode = inXmlNode.ChildNodes[i];
                    if (xNode.NodeType == XmlNodeType.Element)
                    {
                        string name = XMLHelper.GetAttributeValue(xNode, "Title");
                        inTreeNode.Nodes.Add(new CTreeNode(name, GetNodeType(xNode)));
                        tNode = (CTreeNode)inTreeNode.Nodes[i];
                        AddNode(xNode, tNode);
                    }
                }
            }
            else
            {
                inTreeNode.Text = XMLHelper.GetAttributeValue(inXmlNode, "Title");
            }
        }

        private void SiwViewerMainWindow_MouseEnter(object sender, EventArgs e)
        {
            this.toolStripStatusLabel1.Text = sender.ToString();
        }

        private void SiwViewerMainWindow_MouseLeave(object sender, EventArgs e)
        {
            this.toolStripStatusLabel1.Text = "";
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            int selRow = -1;
            if (dataGridView1.SelectedRows.Count == 1)
            {
                selRow = dataGridView1.SelectedRows[0].Index;
            }
            if (selRow == -1)
            {
                return;
            }
            string attrs = "";
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                if (dataGridView1.Rows[selRow].Cells[i].Value != null)
                {
                    string val = dataGridView1.Rows[selRow].Cells[i].Value.ToString();
                    if (attrs.Length > 0)
                    {
                        attrs += " and ";
                    }
                    attrs += "@" + dataGridView1.Columns[i].Name + "=\"" + XMLHelper.EncodeXML(val) + "\"";
                }
            }

            string xp = "//item[" + attrs + "]";
            Console.Out.WriteLine("table selection row = " + selRow + " xp = " + xp);

            XmlNode itemXml = XMLHelper.EvaluateXPath(dom, xp);
            List<XmlNode> kids = XMLHelper.GetChildElements(itemXml);
            dataGridView2.Columns.Clear();
            dataGridView2.Rows.Clear();
            if (kids != null && kids.Count == 1 && "detail".Equals(kids[0].LocalName))
            {
                if (!splitContainer2.Panel2.Enabled)
                {
                    splitContainer2.Panel2Collapsed = false;
                    splitContainer2.Panel2.Enabled = true;
                    splitContainer2.SplitterDistance = 2 * splitContainer2.Size.Height / 3;
                    splitContainer2.Panel2.Show();
                }
                // here we have to create the second table ...
                XmlNode xmlNode = kids[0];
                kids = XMLHelper.GetChildElements(xmlNode);
                XmlNode itemEl = null;
                for (int i = kids.Count; --i >= 0; )
                {
                    if (itemEl == null)
                    {
                        itemEl = kids[i];
                    }
                    else if (itemEl.Attributes.Count < kids[i].Attributes.Count)
                    {
                        itemEl = kids[i];
                    }
                }

                // creates the columns
                if (itemEl != null)
                {
                    for (int i = 0; i < itemEl.Attributes.Count; i++)
                    {
                        string attr_nm = itemEl.Attributes.Item(i).Name;
                        dataGridView2.Columns.Add(attr_nm, XMLHelper.GetAttributeValue(xmlNode, "H" + (i + 1)));
                    }
                }
                else
                {
                    for (int i = 1; i < 50; i++)
                    {
                        string colNm = XMLHelper.GetAttributeValue(xmlNode, "H" + i);
                        if (colNm == null)
                        {
                            break;
                        }
                        else
                        {
                            dataGridView2.Columns.Add("H" + i, colNm);
                        }
                    }
                }
                // add the rows...
                if (xmlNode.HasChildNodes)
                {
                    XmlNodeList nodeList = xmlNode.ChildNodes;
                    int size = nodeList.Count - 1;
                    for (int i = 0; i <= size; i++)
                    {
                        XmlNode xNode = nodeList.Item(i);
                        if (xNode.NodeType == XmlNodeType.Element)
                        {
                            object[] rowValues = new object[dataGridView2.Columns.Count];
                            for (int j = 0; j < xNode.Attributes.Count; j++)
                            {
                                rowValues[j] = xNode.Attributes.Item(j).Value;
                            }
                            dataGridView2.Rows.Add(rowValues);
                        }
                    }
                }
            }
            else
            {
                splitContainer2.Panel2Collapsed = true;
                splitContainer2.Panel2.Hide();
                splitContainer2.SplitterDistance = splitContainer2.Size.Height;
                splitContainer2.Panel2.Enabled = false;
            }
        }

        private void onlineHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.gtopala.com");
        }

        private void buyNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.gtopala.com/siw_buy.html");
        }

        private void registerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.gtopala.com");
        }

        private void visitMyHomepageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.gtopala.com");
        }

        private void checkForUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.gtopala.com/siw-download.html");
        }

    }
}
