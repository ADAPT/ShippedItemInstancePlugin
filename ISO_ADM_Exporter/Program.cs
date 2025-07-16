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
            AgGateway.ADAPT.StandardPlugin.Plugin standardPlugin = new AgGateway.ADAPT.StandardPlugin.Plugin();

            ;
            int countModel = models.Count;
            Console.WriteLine("ADM Model count = " + countModel.ToString());
            if (countModel > 0)
            {
                for (int i = 0; i < models.Count; i++)
                {
                    Properties p = new Properties();
                    string folder = Path.Combine(outputPath, i.ToString());
                    Directory.CreateDirectory(folder);

                    isoPlugin.Export(models[i], folder, p);
                    Console.WriteLine("Wrote ISO files to " + folder);
                    admPlugin.Export(models[i], folder, p);
                    Console.WriteLine("Wrote ADM files to " + folder);
                    standardPlugin.Export(models[i], folder, p);
                    Console.WriteLine("Wrote ADAPT Std files to " + folder);

                }
            }
            else 
            {
                Console.WriteLine("Model count is zero!  How?");
            }

        }
    }
}
