using System.Xml;

namespace app
{
    public class ReadTestResult
    {
        private string path = @"C:\Users\user\Desktop\git_test\app\app\bin\Debug\TestResult.xml";
        private XmlAttributeCollection attributes;
        public void TestResults()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(path);

            foreach (XmlNode node in xml.ChildNodes)
                if (node.Name.Equals("test-run"))
                    attributes = node.Attributes;

            GetFailedTest();
        }

        public void GetFailedTest()
        {
            foreach(var attribute in attributes)
            {
                
            }
        }
    }
}
