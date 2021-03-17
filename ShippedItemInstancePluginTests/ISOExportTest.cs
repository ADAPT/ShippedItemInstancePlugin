using AgGateway.ADAPT.ApplicationDataModel.ADM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;

namespace ShippedItemInstancePluginTests
{
    public class ISOExportTest
    {
        //TODO:  This test is not functional until the version of the ISO Plugin supporting ContextItem/LinkList changes is released
        [Fact (Skip= "Not yet functional")]
        public void ISOImportAndExport()
        {
            //Read the sample data into ADAPT
            AgGateway.ADAPT.ShippedItemInstancePlugin.Plugin shippedItemInstancePlugin = new AgGateway.ADAPT.ShippedItemInstancePlugin.Plugin();
            IList<ApplicationDataModel> models = shippedItemInstancePlugin.Import(@"..\..\..\..\SampleData");

            //Export with the ISO Plugin and test that the data is unaltered upon re-import
            string workingPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestOutput");
            Directory.CreateDirectory(workingPath);
            AgGateway.ADAPT.ISOv4Plugin.Plugin isoPlugin = new AgGateway.ADAPT.ISOv4Plugin.Plugin();
            for (int i = 0; i < models.Count; i++)
            {
                Properties p = new Properties();
                string folder1 = $@"{workingPath}\ISOExport1\{i}";
                Directory.CreateDirectory(folder1);
                isoPlugin.Export(models[i], folder1, p);

                var isoPlugin2 = new AgGateway.ADAPT.ISOv4Plugin.Plugin();
                IList<ApplicationDataModel> reImport = isoPlugin2.Import(folder1);
                string folder2 = $@"{workingPath}\ISOExport2\{i}";
                Directory.CreateDirectory(folder2);
                isoPlugin.Export(reImport[0], folder2, p);

                string export1 = File.ReadAllText(Path.Combine(folder1, "TASKDATA", "LinkList.xml"));
                string export2 = File.ReadAllText(Path.Combine(folder2, "TASKDATA", "LinkList.xml"));
                Assert.Equal(export1, export2);
            }
        }
    }
}
