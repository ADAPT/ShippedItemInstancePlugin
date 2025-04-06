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
using System.Linq;
using System.Text;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using RepresentationSystem = AgGateway.ADAPT.Representation.RepresentationSystem;
using UnitSystem = AgGateway.ADAPT.Representation.UnitSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using IO.Swagger.Models;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace AgGateway.ADAPT.ShippedItemInstancePlugin
{
    /// <summary>
    /// Maps data from the OAGIS ShippedItemInstance document into the ADAPT ApplicationDataModel
    /// </summary>
    public class Mapper
    {
        #region Constructors
        public Mapper(Catalog catalog)
        {
            Catalog = catalog;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Catalog object that is to recieve the data to be mapped.
        /// </summary>
        private Catalog Catalog { get; set; }
        private IList<IError> Errors { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Takes data from the specified document and adds it to the Catalog
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        ///  
        ///
        public IList<IError> MapDocument(ShippedItemInstanceList shippedProducts)
        {
            Errors = new List<IError>();

            Console.WriteLine("within mapper.MapDocument");

            foreach (ShippedItemInstance shippedItemInstance in shippedProducts)
            {
                Console.WriteLine("within mapper.MapDocument");

                MapShippedItemInstance(shippedItemInstance);
                //
                // the primary goal is to create 
                // Catalog.Products  
                // Catalog.Crops
                // Catalog.Brands
                // Catalog.Manufacturers

            }
            return Errors;
        }

        private void MapShippedItemInstance(ShippedItemInstance shippedItemInstance)
        {

            // capture the displayName and all this item instance context items
            // 
            Console.WriteLine("MapShippedItemInstance -- calling GetProduct");

            Product product = GetProduct(shippedItemInstance);
            //
            Console.WriteLine("MapShippedItemInstance -- calling SetCrop");
            SetCrop(shippedItemInstance, product);
            //
            Console.WriteLine("MapShippedItemInstance -- calling SetManufacturerAndBrand");
            SetManufacturerAndBrand(shippedItemInstance, product);
            //
            Console.WriteLine("MapShippedItemInstance -- calling Set Grower");
            SetGrower(shippedItemInstance);
        }

        private Product GetProduct(ShippedItemInstance shippedItemInstance)
        {
            // Look for product with a description that matches the shipped item instance
            // Primarily need the displayName of the product 
            // quick test
            Product product = Catalog.Products.FirstOrDefault(p => p.Description == shippedItemInstance.DisplayName);

            Console.WriteLine("MapShippedItemInstance -- created product");

            if (shippedItemInstance.TypeCode.ToLower() == "seed")
            {
                product = new CropVarietyProduct();
                Console.WriteLine("MapShippedItemInstance -- CropVarietyProduct");
                product.Description = shippedItemInstance.DisplayName;
                Console.WriteLine("MapShippedItemInstance displayName = " + shippedItemInstance.DisplayName);

                product.ContextItems.AddRange(CreateProductContextItems(shippedItemInstance));

                product.ContextItems.AddRange(CreateProductInstanceSpecificContextItems(shippedItemInstance));
            }
            else
            {
                product = new GenericProduct();
                Console.WriteLine("MapShippedItemInstance -- GenericProduct");
                product.Description = shippedItemInstance.DisplayName;
            }


            Catalog.Products.Add(product);

            return product;
        }

        private ContextItem CreateContextItem(string code, string value)
        {
            Console.WriteLine("code = " + code + " and value = " + value);
            ContextItem contextItem = new ContextItem() { Code = code };

            if (value != null)
            {
                contextItem.Value = value;
            }

            return contextItem;
        }


        private NumericRepresentationValue CreateRepresentationValue(double value, string inputUnitOfMeasure)
        {
            //RepresentationValue
            NumericRepresentationValue returnValue = new NumericRepresentationValue();

            //Use vrSeedLoadQuantity Representation
            RepresentationSystem.Representation representation = RepresentationSystem.RepresentationManager.Instance.Representations.First(r => r.DomainId == "vrSeedLoadQuantity");
            if (representation != null)
            {
                // Convert to ADAPT Numeric Representation object
                returnValue.Representation = ((RepresentationSystem.NumericRepresentation)representation).ToModelRepresentation();
            }

            //Value
            // Map bg to bag
            // 
            string uomCode = inputUnitOfMeasure?.ToLower() == "bg" ? "bag" : inputUnitOfMeasure?.ToLower() ?? string.Empty;
            if (!UnitSystem.InternalUnitSystemManager.Instance.UnitOfMeasures.Contains(uomCode))
            {
                // Didn't find uom so just use unitless
                uomCode = "unitless";

                Errors.Add(new Error(string.Empty, "ShippedItemInstanceMapper.CreateRepresentationValue", $"Unit of Measure {uomCode} not found, using unitless instead.", string.Empty));
            }
            UnitOfMeasure uom = UnitSystem.UnitSystemManager.GetUnitOfMeasure(uomCode);
            returnValue.Value = new ApplicationDataModel.Representations.NumericValue(uom, value);

            return returnValue;
        }

        // do we need this?  Why not reference in GetProduct
        private ProductTypeEnum LookupProductType(string productType)
        {
            ProductTypeEnum productTypeEntry = new ProductTypeEnum();
            // what is the product type enum equivalent to UnitSystem.UnitSystemManager.GetUnitOfMeasure(uomCode)?
            return productTypeEntry;

        }

        private List<ContextItem> CreateProductInstanceSpecificContextItems(ShippedItemInstance shippedItemInstance)
        {
            List<ContextItem> items = new List<ContextItem>();

            Console.WriteLine(" //// CreateProductInstanceSpecificContextItems entered  /////");

            // Lot or Batch Id and type

            // TODO:  Create parent as LotBatchInformation, nested identifier, type code, and optional serial numbers
            // move this to ProductContextItems

            if (shippedItemInstance.Lot?.TypeCode != null && shippedItemInstance.Lot.Id != null)
            {
                items.Add(CreateContextItem(shippedItemInstance.Lot.TypeCode + "Identifier", shippedItemInstance.Lot.Id));



                // the following provides the ability to capture serialize jugs of crop protection related to a specific manufactured batch
                // the serialNumberId array may not be present in the payload as seed will not have serialized instances  
                // 
                //  if(!json.ContainsKey(field.Name))
                //  however json is not in context, what alternative is
                //  from
                //  https://stackoverflow.com/questions/55712367/json-net-detect-an-absence-of-a-property-on-json-which-appears-to-be-a-member
                //
                //
                //
                if (shippedItemInstance.Lot.SerialNumberId != null)
                {
                    Console.WriteLine(" //// serial numbers found  /////");
                    ContextItem lotSerialNumberIdsContextItem = CreateContextItem("SerialNumberIds", null);
                    int SerialNumberIdsIndex = 0;

                    foreach (String serialNumberId in shippedItemInstance.Lot.SerialNumberId)
                    {
                        ContextItem serialNumberIdContextItem = CreateContextItem((++SerialNumberIdsIndex).ToString(), null);
                        if (serialNumberId != null)
                        {
                            serialNumberIdContextItem.NestedItems.Add(CreateContextItem("serialNumber", serialNumberId));
                        }
                        if (serialNumberIdContextItem.NestedItems.Count > 0)
                        {
                            lotSerialNumberIdsContextItem.NestedItems.Add(serialNumberIdContextItem);
                        }
                    }

                }
            }

            // Tender Box e.g., Packaging
            ContextItem contextItem = CreateContextItem("Packaging", null);

            // Add Packaging nested items
            if (shippedItemInstance.Packaging?.TypeCode != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("ShipUnitTypeCode", shippedItemInstance.Packaging.TypeCode));
            }
            if (shippedItemInstance.Packaging?.Id != null && shippedItemInstance.Packaging.TypeCode != null)
            {
                contextItem.NestedItems.Add(CreateContextItem(shippedItemInstance.Packaging.TypeCode + ".Id", shippedItemInstance.Packaging.Id));
            }
            if (shippedItemInstance.Packaging.TypeCode != null && shippedItemInstance.Packaging.Quantity.TypeCode != null
                && shippedItemInstance.Packaging.Quantity.Content != null && shippedItemInstance.Packaging.Quantity.UnitCode != null)
            {
                contextItem.NestedItems.Add(CreateContextItem(shippedItemInstance.Packaging.TypeCode + "." + shippedItemInstance.Packaging.Quantity.TypeCode, shippedItemInstance.Packaging.Quantity.Content.ToString()));
                contextItem.NestedItems.Add(CreateContextItem(shippedItemInstance.Packaging.TypeCode + "." +
                    shippedItemInstance.Packaging.Quantity.TypeCode + ".UOM", shippedItemInstance.Packaging.Quantity.UnitCode));
            }

            // how many bags went into the the seed box
            // what is the weight of each bag
            //
            if (shippedItemInstance.Packaging.TypeCode != null &&
                shippedItemInstance.Packaging.Quantity.Content != null &&
                shippedItemInstance.Packaging.Quantity.UnitCode != null)
            {
                // 
                // need SII model point release change - Quantity.TypeCode added (preferred)
                // or Package.Quantity as an array
                //
                contextItem.NestedItems.Add(CreateContextItem(shippedItemInstance.Packaging.TypeCode +
                    ".PackageQuantity"
                    , shippedItemInstance.Quantity.Content.ToString()));
                contextItem.NestedItems.Add(CreateContextItem(shippedItemInstance.Packaging.TypeCode +
                    ".PackageQuantity.UOM"
                    , shippedItemInstance.Quantity.UnitCode));
            }

            if (contextItem.NestedItems.Count > 0)
            {
                items.Add(contextItem);
            }

            // ShipmentReference
            // flatten this, as we only need the shipmentId.
            contextItem = CreateContextItem("ShipmentReference", null);

            // nested items
            ContextItem nestedContextItem = CreateContextItem("ShipmentReference", null);

            // This one has it's own nested items
            if (shippedItemInstance.ShipmentReference?.Id != null)

                //  Retailer GLN Id
                if (shippedItemInstance.ShipmentReference.ShipFromParty.Location?.Glnid != null)
                {
                    contextItem.NestedItems.Add(CreateContextItem("ShipFromGLN", shippedItemInstance.ShipmentReference.ShipFromParty.Location?.Glnid.ToString()));
                }


            // Retailer generated Shipment Id

            nestedContextItem.NestedItems.Add(CreateContextItem("ShipmentId", shippedItemInstance.ShipmentReference?.Id));

            if (nestedContextItem.NestedItems.Count > 0)
            {
                contextItem.NestedItems.Add(nestedContextItem);
            }
            //  semi-trailer Id
            if (shippedItemInstance.ShipmentReference.ShipUnitReference.Id.Content != null &&
                shippedItemInstance.ShipmentReference.ShipUnitReference.Id.TypeCode != null &&
                shippedItemInstance.ShipmentReference.ShipUnitReference.TypeCode != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("ShippingContainerType",
                    shippedItemInstance.ShipmentReference.ShipUnitReference.TypeCode));
                contextItem.NestedItems.Add(CreateContextItem("ShippingContainerIdentifer." +
                    shippedItemInstance.ShipmentReference.ShipUnitReference.Id.TypeCode,
                    shippedItemInstance.ShipmentReference.ShipUnitReference.Id.Content.ToString()));
            }

            // Carrier SCAC code
            if (shippedItemInstance.ShipmentReference.CarrierParty?.Scacid != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("CarrierSCAC", shippedItemInstance.ShipmentReference.CarrierParty?.Scacid));
            }

            if (contextItem.NestedItems.Count > 0)
            {
                items.Add(contextItem);
            }
            // 
            // id
            contextItem = CreateContextItem("InFieldProductID.API.Path.id", shippedItemInstance.Id);

            items.Add(contextItem);

            // Item.Retailed is the link to AGIIS
            if (shippedItemInstance.Item.RelatedId?.Count > 0)
            {
                contextItem = CreateRelatedIdsContextItem(shippedItemInstance);
                if (contextItem.NestedItems.Count > 0)
                {
                    items.Add(contextItem);
                }
            }

            // Uid is the barcode, RFID tag or whatever is need for identify the product

            contextItem = CreateContextItem("EncodedIdentification", null);

            //  content contains the actual value

            if (shippedItemInstance.Uid?.Content != null && shippedItemInstance.Uid.SchemeId != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("encodingSchemaId", shippedItemInstance.Uid.SchemeId));
                contextItem.NestedItems.Add(CreateContextItem(shippedItemInstance.Uid.SchemeId + "Id", shippedItemInstance.Uid.Content));
            }
            // Scheme Agency is who manages the encoding scheme
            if (shippedItemInstance.Uid?.SchemeAgencyId != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("schemaAgencyId", shippedItemInstance.Uid.SchemeAgencyId));
            }

            if (contextItem.NestedItems.Count > 0)
            {
                items.Add(contextItem);
            }


            //
            if (shippedItemInstance.Results?.Quantitative.Measurement.Count > 0)
            {
                contextItem = CreateQuantitativeResultsContextItem(shippedItemInstance);
                if (contextItem.NestedItems.Count > 0)
                {
                    items.Add(contextItem);
                }
            }

            if (shippedItemInstance.Item.ItemTreatment.Substance.Count() > 0 &&
                shippedItemInstance.Item.ItemTreatment.Name != null)
            // seed treatment is defined, as well is the substances used
            //
            {

                contextItem = CreateItemTreatmentContextItem(shippedItemInstance);
                // 
                if (contextItem.NestedItems.Count > 0)
                {
                    items.Add(contextItem);
                }
            }

            return items;
        }
        private ContextItem CreateItemTreatmentContextItem(ShippedItemInstance shippedItemInstance)
        {
            ItemItemTreatment seedTreatment = shippedItemInstance.Item.ItemTreatment;
            ContextItem seedTreatmentContextItem = CreateContextItem("SeedTreatment", seedTreatment.Name);
            try
            {
                seedTreatmentContextItem.NestedItems.Add(CreateContextItem("seedTreatmentId", seedTreatment.Id));

                foreach (ItemItemTreatmentSubstance substance in shippedItemInstance.Item.ItemTreatment.Substance)
                {
                    if (substance.Name != null && substance.RegistrationStatus != null)
                    {
                        ContextItem seedTreatmentSubstanceContextItem = CreateContextItem("substance.TypeCode", substance.TypeCode);
                        seedTreatmentSubstanceContextItem.NestedItems.Add(CreateContextItem("substance.Name", substance.Name));

                        if (seedTreatmentSubstanceContextItem.NestedItems.Count > 0)
                        {
                            seedTreatmentContextItem.NestedItems.Add(seedTreatmentSubstanceContextItem);
                        }

                        // Create foreach on ItemTreatmentSubstanceRegistrationStatus

                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

            }
            return seedTreatmentContextItem;

        }
        private ContextItem CreateRelatedIdsContextItem(ShippedItemInstance shippedItemInstance)
        {
            ContextItem itemRelatedIdsContextItem = CreateContextItem("RelatedIdentifiers", null);


            foreach (RelatedId relatedId in shippedItemInstance.Item.RelatedId)
            {
                if (relatedId.SourceId != null && relatedId.PartyId != null &&
                    relatedId.Id != null && relatedId.TypeCode != null)
                {
                    ContextItem relatedIdContextItem = CreateContextItem(relatedId.PartyId, relatedId.SourceId);
                    relatedIdContextItem.NestedItems.Add(CreateContextItem(relatedId.TypeCode, relatedId.Id));

                    if (relatedIdContextItem.NestedItems.Count > 0)
                    {
                        itemRelatedIdsContextItem.NestedItems.Add(relatedIdContextItem);
                    }
                }
            }
            return itemRelatedIdsContextItem;
        }

        private ContextItem CreateQuantitativeResultsContextItem(ShippedItemInstance shippedItemInstance)
        {
            ContextItem results = CreateContextItem("QuantitativeMeasurements", null);

            // int quantitateResultIndex = 0;
            foreach (Measurement measurement in shippedItemInstance.Results.Quantitative.Measurement)
            {
                // why isn't this the name instead of a numeric index?
                // old: 
                // ContextItem measurementContextItem = CreateContextItem((++quantitateResultIndex).ToString(), null);

                ContextItem measurementContextItem = CreateContextItem("measurementClassificationTypeCode", measurement.TypeCode);

                // type code only classifies the measurement, and we discussed if there was value to field operations, such as equipment setup
                // if so, it can be brought into the MICS but today this cannot be displayed to the farmer to assist equipment setup
                // in an automated or semi-automated manner
                //
                if (measurement.Name != null && measurement.Measure != null)
                {
                    measurementContextItem.NestedItems.Add(CreateContextItem(measurement.Name, measurement.Measure.ToString()));

                }

                if (measurement.UnitCode != null)
                {
                    measurementContextItem.NestedItems.Add(CreateContextItem("unitOfMeasure", measurement.UnitCode));
                }

                if (measurement.DateTime != null)
                {
                    measurementContextItem.NestedItems.Add(CreateContextItem("measurementTimestamp", measurement.DateTime.ToString()));
                }

                if (measurementContextItem.NestedItems.Count > 0)
                {
                    results.NestedItems.Add(measurementContextItem);
                }
            }

            return results;
        }

        private List<ContextItem> CreateProductContextItems(ShippedItemInstance shippedItemInstance)
        {
            List<ContextItem> contextItems = new List<ContextItem>();

            if (shippedItemInstance.TypeCode != null)
            {
                contextItems.Add(CreateContextItem("Product.Type", shippedItemInstance.TypeCode));
            }
            if (shippedItemInstance.Item.Description != null)
            {
                contextItems.Add(CreateContextItem("Product.Description", shippedItemInstance.Item.Description));
            }
            if (shippedItemInstance.Item.ProductName != null)
            {
                contextItems.Add(CreateContextItem("Product.Name", shippedItemInstance.Item.ProductName));
            }
            if (shippedItemInstance.Item.BrandName != null)
            {
                contextItems.Add(CreateContextItem("Product.BrandName", shippedItemInstance.Item.BrandName));
            }
            if (shippedItemInstance.Item.VarietyName != null)
            {
                contextItems.Add(CreateContextItem("Product.VarietyName", shippedItemInstance.Item.VarietyName));
            }

            // item perPackage quantity, e.g., weight of a bag
            //
            if (shippedItemInstance.Item.Packaging.PerPackageQuantity.Content != null &&
                shippedItemInstance.Item.Packaging.PerPackageQuantity.UnitCode != null)
            {
                contextItems.Add(CreateContextItem("Product.PerPackageQuantity",
                    shippedItemInstance.Item.Packaging.PerPackageQuantity.Content.ToString()));
                contextItems.Add(CreateContextItem("Product.PerPackageQuantity.UOM",
                    shippedItemInstance.Item.Packaging.PerPackageQuantity.UnitCode));
            }
            // Classification
            // this is already in Crop
            //
            ContextItem classificationContextItem = CreateContextItem("Product.Classification", "");

            if (shippedItemInstance.Item?.Classification?.Codes?.Code != null)
            {
                classificationContextItem.NestedItems.Add(CreateContextItem("Type", shippedItemInstance.Item.Classification.TypeCode));
            }
            var count = shippedItemInstance.Item?.Classification?.Codes?.Code?.Count;
            if (count != null && count > 0)
            {
                ContextItem codesContextItem = CreateContextItem("Codes", null);

                int codeIndex = 0;

                foreach (ClassificationCodesCode code in shippedItemInstance.Item.Classification.Codes.Code)
                {
                    ContextItem codeContextItem = CreateContextItem((++codeIndex).ToString(), null);

                    if (code.Content != null && code.TypeCode != null)
                    {
                        codeContextItem.NestedItems.Add(CreateContextItem(code.TypeCode, code.Content));
                    }
                    if (code.ListAgencyId != null)
                    {
                        codeContextItem.NestedItems.Add(CreateContextItem("ListAgencyId", code.ListAgencyId));
                    }

                    if (codeContextItem.NestedItems.Count > 0)
                    {
                        codesContextItem.NestedItems.Add(codeContextItem);
                    }
                }


                if (codesContextItem.NestedItems.Count > 0)
                {
                    classificationContextItem.NestedItems.Add(codesContextItem);
                }
            }

            if (classificationContextItem.NestedItems.Count > 0)
            {
                contextItems.Add(classificationContextItem);
            }


            return contextItems;
        }

        private void SetManufacturerAndBrand(ShippedItemInstance shippedItemInstance, Product product)
        {
            //Set Manufacturer & Brand as available

            if (product != null)
            {
                if (shippedItemInstance.Item.ManufacturingParty?.Name != null)
                {
                    var manufacturer = Catalog.Manufacturers.FirstOrDefault(m => m.Description == shippedItemInstance.Item.ManufacturingParty.Name);
                    if (manufacturer == null)
                    {
                        manufacturer = new Manufacturer()
                        { Description = shippedItemInstance.Item.ManufacturingParty.Name };
                        Catalog.Manufacturers.Add(manufacturer);
                    }
                    product.ManufacturerId = manufacturer.Id.ReferenceId;
                }

                if (shippedItemInstance.Item.BrandName != null)
                {
                    var brandName = Catalog.Brands.FirstOrDefault(b => b.Description == shippedItemInstance.Item.BrandName);

                    if (brandName == null)
                    {
                        brandName = new Brand()
                        { Description = shippedItemInstance.Item.BrandName, ManufacturerId = product.ManufacturerId ?? 0 };
                        Console.WriteLine("Brand Name = " + brandName.Description);


                        Catalog.Brands.Add(brandName);
                    }
                    // it appears that the Add method generates the Id.ReferenceId
                    product.BrandId = brandName.Id.ReferenceId;

                    // map to contentItems
                    //
                    // gtin , add this to unique identifer on product?
                    //
                    var gtin = shippedItemInstance.Item.Gtinid;
                    Console.WriteLine("GTIN = " + gtin);
                    //
                    // Where is gtin used?
                    //
                    // create a colleciton of Product components and add substatnce to it
                    // var productComponents = shippedItemInstance.Item.ItemTreatment.Substance.FirstOrDefault(s => s.Name = )
                    // product.ProductComponents = shippedItemInstance.Item.ItemTreatment.Substance

                }

            }
        }

        private void SetCrop(ShippedItemInstance shippedItemInstance, Product product)
        {
            //Set Crop as available
            // for seed this will be available
            // for crop protection, crop is important, but it is really an associated item
            // the classification of crop protection moves to product type
            //
            // do we need a separate way to manage this?
            //
            if (shippedItemInstance.Item.Classification.TypeCode.ToLower() == "crop")
            {
                // this is where the product is created -- seems a bit overloaded
                //
                //
                if (product is CropVarietyProduct)
                {
                    Console.WriteLine("product is CropVarietyProduct");
                    // this should not use First or Default but filter the array for specific typeCodes
                    // Trait, CropType, AGIIS code for CropType value (content)
                    //
                    // The following will return the array of code entries
                    //
                    var cropInformation = shippedItemInstance.Item.Classification.Codes.Code;

                    if (cropInformation != null)
                    {
                        // CropType 
                        Console.WriteLine("cropInformation is not null");
                        // should implement equivalent to this JSON PATH
                        // $[0].item.classification.codes.code[?@.typeCode=='CropType'].content
                        // 
                        try
                        {
                            string cropName = cropInformation.FirstOrDefault(c => c.TypeCode.ToLower() == "croptype").Content.ToString();
                            Console.WriteLine("cropName = " + cropName);

                            if (cropName != null)
                            {

                                string idAgency = cropInformation.FirstOrDefault(c => c.TypeCode.ToLower() == "croptype").ListAgencyId;
                                Console.WriteLine("idAgency = " + idAgency);
                                //  
                                //
                                string cropID = cropInformation.FirstOrDefault(c => c.TypeCode == cropName && c.ListAgencyId == "AGIIS").Content;
                                Console.WriteLine("cropID = " + cropID);
                                //  
                                //
                                Crop crop = Catalog.Crops.FirstOrDefault(c => c.Name == cropName);

                                // 
                                if (shippedItemInstance.Item.VarietyName != null)
                                {

                                    var varietyName = shippedItemInstance.Item.VarietyName;

                                    Console.WriteLine("varietyName = " + varietyName);

                                    // how does this get mapped to CVT in ISO?
                                    // CVT seems to be coming from product.Description
                                }
                                // 2025-03-22 Was this intended to be to default if no crop found above, or this should this be != null?
                                //
                                if (crop == null)
                                {
                                    // 2025-03-22
                                    //
                                    crop = new Crop() { Name = cropName };

                                    crop.Id.UniqueIds.Add(new UniqueId()
                                    { Source = idAgency, IdType = IdTypeEnum.String, Id = cropID });
                                    Catalog.Crops.Add(crop);
                                }

                                ((CropVarietyProduct)product).CropId = crop.Id.ReferenceId;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);

                        }
                        // need to test to see if this is present first


                    }
                }
            }
        }


        private void SetGrower(ShippedItemInstance shippedItemInstance)
        {
            //  Need to test the typeCode of the both the ShipToParty and ShipFromParty to see which is the grower
            //  Shipments from the Retailer to the Grower is for Seed
            //  Shipments from the Farmer to the elevator or processor are for commodity shipments
            //

            if (shippedItemInstance.ShipmentReference.ShipToParty.TypeCode == "Farmer")
            {
                ShipToParty Farmer = shippedItemInstance.ShipmentReference.ShipToParty;

                Grower grower = Catalog.Growers.FirstOrDefault(c => c.Name == Farmer.Name);
                if (grower == null)
                {
                    grower = new Grower() { Name = Farmer.Name };

                    Console.WriteLine("grower.Name = " + grower.Name);

                    // Previously GLN was used but most farmers lack a GLN, 
                    // so the Retailer's ERP accountId for the farmer is best
                    //
                    if (Farmer.AccountId != null)
                    {
                        UniqueId id = new UniqueId() { Id = Farmer.AccountId, Source = "RetailerERPAccount", IdType = IdTypeEnum.String };
                        grower.Id.UniqueIds.Add(id);
                    }
                    Catalog.Growers.Add(grower);
                }
            }
        }

        #endregion
    }
}
