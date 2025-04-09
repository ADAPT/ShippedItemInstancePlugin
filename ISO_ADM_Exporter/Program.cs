using AgGateway.ADAPT.ApplicationDataModel.ADM;
using System;
using System.Collections.Generic;
using System.IO;

namespace ISO_ADM_Exporter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: {inputPath} {outputPath}");
            }
            string inputPath = args[0];
            string outputPath = args[1];

            AgGateway.ADAPT.ShippedItemInstancePlugin.Plugin sPlugin = new AgGateway.ADAPT.ShippedItemInstancePlugin.Plugin();
            IList<ApplicationDataModel> models = sPlugin.Import(inputPath);

            AgGateway.ADAPT.ISOv4Plugin.Plugin isoPlugin = new AgGateway.ADAPT.ISOv4Plugin.Plugin();
            AgGateway.ADAPT.ADMPlugin.Plugin admPlugin = new AgGateway.ADAPT.ADMPlugin.Plugin();
            for (int i = 0; i < models.Count; i++)
            {
                Properties p = new Properties();
                string folder = Path.Combine(outputPath, i.ToString());
                Directory.CreateDirectory(folder);

                isoPlugin.Export(models[i], folder, p);
                Console.WriteLine("Wrote ISO file to " + folder);
                admPlugin.Export(models[i], folder, p);
                Console.WriteLine("Wrote ADM file to " + folder);
            
            }

        }
    }
}
