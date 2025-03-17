/*******************************************************************************
  * Copyright (C) 2025 AgGateway and ADAPT Contributors
  * All rights reserved. This program and the accompanying materials
  * are made available under the terms of the Eclipse Public License v1.0
  * which accompanies this distribution, and is available at
  * http://www.eclipse.org/legal/epl-v10.html <http://www.eclipse.org/legal/epl-v10.html> 
  *
  *******************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using IO.Swagger.Models;
using Newtonsoft.Json;

namespace AgGateway.ADAPT.ShippedItemInstancePlugin
{
    public class Plugin : IPlugin
    {
        #region IPlugin implementation
        public string Name => "Shipped_Item_Instance-Plugin";

        public string Version => "4.0";

        public string Owner => "AgGateway";

        public IList<IError> Errors { get; set; }

        public void Export(ApplicationDataModel.ADM.ApplicationDataModel dataModel, string exportPath, Properties properties = null)
        {
            throw new NotImplementedException();
        }

        public Properties GetProperties(string dataPath)
        {
            throw new NotImplementedException();
        }

        public IList<ApplicationDataModel.ADM.ApplicationDataModel> Import(string dataPath, Properties properties = null)
        {
            IList<ApplicationDataModel.ADM.ApplicationDataModel> models = new List<ApplicationDataModel.ADM.ApplicationDataModel>();

            List<IError> errors = new List<IError>();

            List<string> fileNames = GetInputFiles(dataPath);

            fileNames.Sort(); // required to ensure OS file system sorting differences are handled

            foreach (string fileName in fileNames)
            {

                try
                {
                    string jsonText = File.ReadAllText(fileName);
                    
                    Console.WriteLine("Read JSON fileName =" + fileName);                    
                    Console.WriteLine(jsonText);

                   ShippedItemInstanceList items = JsonConvert.DeserializeObject<ShippedItemInstanceList>(jsonText);
                    if (items != null)
                    {
                        //Each document will import as individual ApplicationDataModel
                        ApplicationDataModel.ADM.ApplicationDataModel adm = new ApplicationDataModel.ADM.ApplicationDataModel();
                        adm.Catalog = new Catalog() { Description = fileName };

                        //Map the document data into the Catalog
                        Mapper mapper = new Mapper(adm.Catalog);
                        errors.AddRange(mapper.MapDocument(items));

                        models.Add(adm);
                    }
                    else
                    {
                        errors.Add(new Error(null, $"Importing {fileName}", "Couldn't parse ShippedItemInstances", null));
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(new Error(null, $"Exception Importing {fileName}", ex.Message, ex.StackTrace));
                }
            }

            //Read the Errors property after import to inspect any diagnostic messages.
            Errors = errors;

            return models;
        }

        public void Initialize(string args = null)
        {
        }

        public bool IsDataCardSupported(string dataPath, Properties properties = null)
        {
            List<string> fileNames = GetInputFiles(dataPath);

            foreach (string fileName in fileNames)
            {
                string jsonText = File.ReadAllText(fileName);

                //  This node will not be present, why did this exist?  
                //  could be replaced with item or other properties
                //
                if (jsonText.Contains("shippedItemInstance"))

                {
                    return true;
                }
            }

            return false;

        }

        public IList<IError> ValidateDataOnCard(string dataPath, Properties properties = null)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Methods

        private List<string> GetInputFiles(string dataPath)
        {
            try
            {
                if (Directory.Exists(dataPath))
                {
                    return Directory.GetFiles(dataPath, "*.json", SearchOption.TopDirectoryOnly).ToList<string>();
                }
                else
                {
                    return new List<string>();
                }
            }
            catch(Exception ex)
            {
                Errors.Add(new Error(null, "Plugin.GetInputFiles", "Unable to find data files", ex.StackTrace));
                return new List<string>();
            }
        }

        #endregion
    }
}
