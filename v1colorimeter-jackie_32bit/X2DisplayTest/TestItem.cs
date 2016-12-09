using System;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;

namespace X2DisplayTest
{
    public class TestNode
    {
        public string NodeName { get; set; }
        public double Upper { get; set; }
        public double Lower { get; set; }
        public string Unit { get; set; }
        public double Value { get; set; }
        public string Error { get; set; }
        public bool IsNeedTest { get; set; }
        public bool Result { get; private set; }

        public bool Run()
        {
            return (this.Result = (this.Value <= this.Upper && this.Value >= this.Lower));
        }
    }

    public class TestItem
    {
        public string TestName { get; set; }
        public System.Drawing.Color RGB { get; set; }
        public double Exposure { get; set; }
        public bool IsNeedTest { get; set; }
        public bool Result { get; set; }
        
        public List<TestNode> TestNodes { get; set; }

        public TestItem()
        { 
            TestNodes = new List<TestNode>();
        }

        public bool Run()
        {
            this.Result = true;

            foreach (TestNode node in TestNodes)
            {
                if (node.IsNeedTest) {
                    this.Result &= node.Run();
                }
            }

            return this.Result;
        }

        public bool RunCa310()
        {
            this.Result = true;

            for (int i = 3; i < TestNodes.Count; i++)
            {
                TestNode node = TestNodes[i];

                if (node.IsNeedTest)
                {
                    this.Result &= node.Run();
                }
            }

            return this.Result;
        }

        public bool RunUnifAndMura()
        {
            this.Result = true;

            for (int i = 1; i < 2; i++)
            {
                TestNode node = TestNodes[i];

                if (node.IsNeedTest)
                {
                    this.Result &= node.Run();
                }
            }

            return this.Result;
        }
    }

    public class Xml
    {
        public Xml(string scriptName)
        {
            this.scriptName = scriptName;
            xml = new XmlDocument();
            xml.Load(scriptName);
            this.LoadScript();
        }

        private string scriptName;
        private XmlDocument xml;
        private List<TestItem> items;

        public List<TestItem> Items
        {
            get {
                return items;
            }
        }

        public double PixelDistRatio 
        {
            get {
                double ratio = 0;
                XmlNode node = xml.SelectSingleNode("X2/X2Params/Calibration");

                if (node != null)
                {
                    XmlElement element = (XmlElement)node;

                    if (!double.TryParse(element.GetAttribute("Value"), out ratio)) {
                        ratio = 0;
                    }
                }
                return ratio;
            }
            set {
                XmlNode node = xml.SelectSingleNode("X2/X2Params/Calibration");

                if (node != null)
                {
                    XmlElement element = (XmlElement)node;
                    element.SetAttribute("Value", value.ToString());
                }
                xml.Save(this.scriptName);
            }
        }

        private TestNode LoadTestNode(XmlNode node)
        {
            TestNode testNode = new TestNode();
            XmlElement element = (XmlElement)node;

            if (element != null)
            {
                testNode.NodeName = element.GetAttribute("TestName");
                testNode.Upper = Convert.ToDouble(element.GetAttribute("Upper"));
                testNode.Lower = Convert.ToDouble(element.GetAttribute("Lower"));
                testNode.Unit = element.GetAttribute("Unit");
                testNode.IsNeedTest = Convert.ToBoolean(element.GetAttribute("IsNeedTest"));
            }
            
            return testNode;
        }

        private void SaveTestNode(XmlNode node, TestNode testNode)
        {
            if (node != null) {
                XmlElement element = xml.CreateElement("TestNode");
                element.SetAttribute("TestName", testNode.NodeName);
                element.SetAttribute("Upper", testNode.Upper.ToString());
                element.SetAttribute("Lower", testNode.Lower.ToString());
                element.SetAttribute("Unit", testNode.Unit);
                element.SetAttribute("IsNeedTest", testNode.IsNeedTest.ToString());
                node.AppendChild(element);
            }
        }

        private TestItem LoadTestItem(XmlNode node)
        {
            TestItem item = new TestItem();
            XmlElement element = (XmlElement)node.SelectSingleNode("Array");

            foreach (XmlNode n in element.ChildNodes)
            {
                item.TestNodes.Add(this.LoadTestNode(n));
            }

            element = (XmlElement)node.SelectSingleNode("Exposure");
            item.Exposure = Convert.ToDouble(element.GetAttribute("Time"));
            element = (XmlElement)node.SelectSingleNode("Info");
            item.TestName = element.GetAttribute("ItemName");
            item.IsNeedTest = bool.Parse(element.GetAttribute("IsNeedTest"));

            MatchCollection matches = Regex.Matches(element.GetAttribute("RGB"), @"\d+");

            if (matches.Count == 3) {
                item.RGB = System.Drawing.Color.FromArgb(int.Parse(matches[0].Value),
                    int.Parse(matches[1].Value), int.Parse(matches[2].Value));
            }

            return item;
        }

        private void SaveTestItem(XmlNode node, TestItem item)
        {
            XmlElement element = (XmlElement)node.SelectSingleNode("Array");
            element.RemoveAll();

            foreach (TestNode testNode in item.TestNodes)
            {
                this.SaveTestNode(element, testNode);
            }

            element = (XmlElement)node.SelectSingleNode("Exposure");
            element.SetAttribute("Time", item.Exposure.ToString());
            element = (XmlElement)node.SelectSingleNode("Info");
            element.SetAttribute("ItemName", item.TestName);
            element.SetAttribute("IsNeedTest", item.IsNeedTest.ToString());
            element.SetAttribute("RGB", string.Format("({0},{1},{2})", item.RGB.R, item.RGB.G, item.RGB.B));
        }

        public void LoadScript()
        {
            items = new List<TestItem>();

            XmlNodeList node = xml.SelectNodes("X2/Item");

            foreach (XmlNode n in node)
            {
                items.Add(this.LoadTestItem(n));
            }
        }

        public void SaveScript()
        {
            XmlNodeList node = xml.SelectNodes("X2/Item");

            for (int i = 0; i < items.Count; i++)
            {
                this.SaveTestItem(node[i], items[i]);
            }
            xml.Save(this.scriptName);
        }
    }
}
