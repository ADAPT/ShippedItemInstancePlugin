/*******************************************************************************
  * Copyright (C) 2021 AgGateway and ADAPT Contributors
  * All rights reserved. This program and the accompanying materials
  * are made available under the terms of the Eclipse Public License v1.0
  * which accompanies this distribution, and is available at
  * http://www.eclipse.org/legal/epl-v10.html <http://www.eclipse.org/legal/epl-v10.html> 
  *
  * Contributors:
  *    Rob Cederberg, Kelly Nelson - initial implementation
  *    Scott Nieman, V4 uplift
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
        public IList<IError> MapDocument(Document.Document document)
        {
            Errors = new List<IError>();

       
            foreach (ShippedItemInstance shippedItemInstance in document.ShippedItemInstances)
            {
                MapShippedItemInstance(shippedItemInstance);
            }
            return Errors;
        }

        private void MapShippedItemInstance(ShippedItemInstance shippedItemInstance)
        {
            //-----------------------
            //PackagedProductInstance
            //-----------------------
            // The PackagedProductInstance represents a single, unique product shipment line and maps 1:1 to the ShippedItemInstance
            // The shipment line is typically a single seed lot or crop protection batch
            // 
            //
            PackagedProductInstance packagedProductInstance = new PackagedProductInstance();

            //Description and quantity are set on the related class properties

            packagedProductInstance.GrossWeight = CreateRepresentationValue((double)shippedItemInstance.Packaging.Quantity.Content,shippedItemInstance.Packaging.Quantity.UnitCode);
            packagedProductInstance.Description = shippedItemInstance.Item.Description;
            

            var quantity = (double)shippedItemInstance.Packaging.Quantity?.Content;

            packagedProductInstance.ProductQuantity = CreateRepresentationValue(quantity, shippedItemInstance.Quantity.UnitCode);
         


            //The remaining data is somewhat specific to the ShippedItemInstance and is persisted as ContextItems
            //The ContextItem data generally is intended to be passed out of the ApplicationDataModel and passed back in unaltered,  
            //in order that the data may return, e.g., on a logged planting operation and reconcile that planting operation 
            //back to this ShippedItemInstance.
         
            packagedProductInstance.ContextItems.AddRange(CreatePackagedProductInstanceContextItems(shippedItemInstance));

            //-----------------------
            // PackagedProduct
            //-----------------------
            // Packaged product is defined a referenced product within a specific packaging or container
            // 
            // Multiple ShippedItemInstances may map to the same PackagedProduct -- this is not true
            //
         
            PackagedProduct packagedProduct = GetPackagedProduct(shippedItemInstance);
            
            //
            if (packagedProduct != null)
            {
                packagedProductInstance.PackagedProductId = packagedProduct.Id.ReferenceId;
            }
            else
            {
                Errors.Add(new Error(null, "Mapper.MapShippedItemInstance", $"Couldn't create PackagedProduct for PackageProductInstance {packagedProductInstance.Id.ReferenceId}", null));
            }

            //Add the PackagedProductInstance to the Catalog.   The PackagedProduct is added in the subroutine above.
            Catalog.PackagedProductInstances.Add(packagedProductInstance);

            //Set other contextual information from the ShippedItemInstance into relevant ADAPT classes
            SetManufacturerAndBrand(shippedItemInstance);
            SetCrop(shippedItemInstance);
            SetGrower(shippedItemInstance);
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

        private List<ContextItem> CreatePackagedProductInstanceContextItems(ShippedItemInstance shippedItemInstance)
        {
            List<ContextItem> items = new List<ContextItem>();

            // Lot or Batch Id and type
            if (shippedItemInstance.Lot?.Id != null)
            {
                // TODO:
 
                items.Add(CreateContextItem("LotBatchIdentifier", shippedItemInstance.Lot?.Id));

                if (shippedItemInstance.Lot?.SerialNumberId.Count > 0)
                {
                    ContextItem lotSerialNumberIdsContextItem = CreateContextItem("SerialNumberIds", null);
                    int SerialNumberIdsIndex = 0;

                    foreach (String serialNumberId in shippedItemInstance.Lot.SerialNumberId)
                    {
                        ContextItem serialNumberIdContextItem = CreateContextItem((++SerialNumberIdsIndex).ToString(), null);
                        if (serialNumberId != null)
                        {
                            serialNumberIdContextItem.NestedItems.Add(CreateContextItem("serialNumber" , serialNumberId));
                        }
                        if (serialNumberIdContextItem.NestedItems.Count > 0)
                        {
                                lotSerialNumberIdsContextItem.NestedItems.Add(serialNumberIdContextItem);
                        }
                    }

                }
            }

            if (shippedItemInstance.Lot?.TypeCode != null)
            {
                items.Add(CreateContextItem("LotBatchIdentifierTypeCode", shippedItemInstance.Lot.TypeCode));
            }
            // Batch Serial identifiers, e.g., serialized jugs of crop protection
            //


            // Packaging
            ContextItem contextItem = CreateContextItem("Packaging", null);

            // Add Packaging nested items
            if (shippedItemInstance.Packaging?.TypeCode != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("typeCode", shippedItemInstance.Packaging.TypeCode));
            }
            if (shippedItemInstance.Packaging?.Id != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("identifier", shippedItemInstance.Packaging.Id));
            }
            if (contextItem.NestedItems.Count > 0)
            {
                items.Add(contextItem);
            }

            // ShipmentReference
            contextItem = CreateContextItem("ShipmentReference", null);

            // nested items
            ContextItem nestedContextItem = CreateContextItem("ShipmentReference", null);

            // This one has it's own nested items
            if (shippedItemInstance.ShipmentReference?.Id != null)
            {
                nestedContextItem.NestedItems.Add(CreateContextItem("ShipmentId", shippedItemInstance.ShipmentReference?.Id));
            }
            if (nestedContextItem.NestedItems.Count > 0)
            {
                contextItem.NestedItems.Add(nestedContextItem);
            }

            if (shippedItemInstance.ShipmentReference?.ActualShipDateTime != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("ActualShipDateTime", shippedItemInstance.ShipmentReference.ActualShipDateTime.ToString()));
            }

            if (contextItem.NestedItems.Count > 0)
            {
                items.Add(contextItem);
            }

            // Id
            contextItem = CreateContextItem("Id", null);

            // nested items
            if (shippedItemInstance.TypeCode != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("typeCode", shippedItemInstance.TypeCode));
            }
            if (shippedItemInstance.Id != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("content", shippedItemInstance.Id));
            }
            if (contextItem.NestedItems.Count > 0)
            {
                items.Add(contextItem);
            }

            // ItemIdSet
            if (shippedItemInstance.Item.RelatedId?.Count > 0)
            {
                contextItem = CreateRelatedIdsContextItem(shippedItemInstance);
                if (contextItem.NestedItems.Count > 0)
                {
                    items.Add(contextItem);
                }
            }

            // Uid
            contextItem = CreateContextItem("uid", null);
            if (shippedItemInstance.Uid?.Content != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("content", shippedItemInstance.Uid.Content));
            }
            if (shippedItemInstance.Uid?.SchemeId != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("schemaId", shippedItemInstance.Uid.SchemeId));
            }
            if (shippedItemInstance.Uid?.SchemeAgencyId != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("schemaAgencyId", shippedItemInstance.Uid.SchemeAgencyId));
            }
            if (shippedItemInstance.Uid?.TypeCode != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("typeCode", shippedItemInstance.Uid.TypeCode));
            }

            if (contextItem.NestedItems.Count > 0)
            {
                items.Add(contextItem);
            }

            // Quantitative Results
            if (shippedItemInstance.Results?.Quantitative.Measurement.Count > 0)
            {
                contextItem = CreateQuantitativeResultsContextItem(shippedItemInstance);
                if (contextItem.NestedItems.Count > 0)
                {
                    items.Add(contextItem);
                }
            }

            return items;
        }

        private ContextItem CreateContextItem(string code, string value)
        {
            ContextItem item = new ContextItem() { Code = code };

            if (value != null)
            {
                item.Value = value;
            }

            return item;
        }

        private ContextItem CreateRelatedIdsContextItem(ShippedItemInstance shippedItemInstance)
        {
            ContextItem itemRelatedIdsContextItem = CreateContextItem("ItemRelatedIds", null);

            int relateIdIndex = 0;

            foreach (ItemRelatedId relatedId in shippedItemInstance.Item.RelatedId)
            {
                ContextItem relatedIdContextItem = CreateContextItem((++relateIdIndex).ToString(), null);
                if (relatedId.Id != null)
                {
                    relatedIdContextItem.NestedItems.Add(CreateContextItem("id", relatedId.Id));
                }
                if (relatedId.TypeCode != null)
                {
                    relatedIdContextItem.NestedItems.Add(CreateContextItem("typeCode", relatedId.TypeCode));
                }
                if (relatedId.SourceId != null)
                {
                    relatedIdContextItem.NestedItems.Add(CreateContextItem("source", relatedId.SourceId));
                }
                if (relatedId.TypeCode != null)
                {
                    relatedIdContextItem.NestedItems.Add(CreateContextItem("partyId", relatedId.PartyId));
                }

                if (relatedIdContextItem.NestedItems.Count > 0)
                {
                    itemRelatedIdsContextItem.NestedItems.Add(relatedIdContextItem);
                }
            }
            return itemRelatedIdsContextItem;
        }

        private ContextItem CreateQuantitativeResultsContextItem(ShippedItemInstance shippedItemInstance)
        {
            ContextItem results = CreateContextItem("QuantitativeMeasurements", null);

            int quantitateResultIndex = 0;
            foreach (Measurement measurement in shippedItemInstance.Results.Quantitative.Measurement)
            {
                ContextItem measurementContextItem = CreateContextItem((++quantitateResultIndex).ToString(), null);

                if (measurement.TypeCode != null)
                {
                    measurementContextItem.NestedItems.Add(CreateContextItem("typeCode", measurement.TypeCode));
                }
                if (measurement.Name != null)
                {
                    measurementContextItem.NestedItems.Add(CreateContextItem("name", measurement.Name));
                }

                 if (measurement.Measure != null)
                {
                    measurementContextItem.NestedItems.Add(CreateContextItem("measure", measurement.Measure.ToString()));
                }
                if (measurement.UnitCode != null)
                {
                    measurementContextItem.NestedItems.Add(CreateContextItem("measure", measurement.UnitCode));
                }
                // date time of measure
                if (measurement.DateTime != null)
                {
                    measurementContextItem.NestedItems.Add(CreateContextItem("measurement timestamp", measurement.DateTime.ToString()));
                }

                if (measurementContextItem.NestedItems.Count > 0)
                {
                    results.NestedItems.Add(measurementContextItem);
                }
            }          

            return results;
        }

        private PackagedProduct GetPackagedProduct(ShippedItemInstance shippedItemInstance)
        {
            PackagedProduct packagedProduct = null;
            Item item = shippedItemInstance.Item;
            if (item?.ManufacturerItemIdentification?.Id == null && item?.Gtinid == null && item?.Upcid == null)
            {
                // No ids specified so use the descriptionn to find a PackageProduct that matches
                packagedProduct = Catalog.PackagedProducts.FirstOrDefault(pp => pp.Description == item?.Description);
            }
            else
            {
                // Try to find a matching PackagedProduct based on the ManufacturerItemIdentifier, UPC Id or GTIN Id
                if (!string.IsNullOrEmpty(item?.ManufacturerItemIdentification?.TypeCode) 
                    && !string.IsNullOrEmpty(item?.ManufacturerItemIdentification?.Id))
                {
                    packagedProduct = Catalog.PackagedProducts.FirstOrDefault(pp => pp.ContextItems.Any(i => (i.Code == item?.ManufacturerItemIdentification?.TypeCode && 
                        i.Value == item?.ManufacturerItemIdentification?.Id)));
                }
                else if (!string.IsNullOrEmpty(item?.Gtinid) && !string.IsNullOrEmpty(item?.Upcid))
                {
                    packagedProduct = Catalog.PackagedProducts.FirstOrDefault(pp => pp.ContextItems.Any(i => (i.Code == "UPC" && i.Value == item?.Upcid) 
                        || (i.Code == "GTIN" && i.Value == item?.Gtinid)));
                }
            }

            if (packagedProduct == null && item?.Description != null)
            {
                // Didn't find a match so create a new object
                packagedProduct = new PackagedProduct();

                packagedProduct.Description = item?.Description;

                // Set context items
                
                //Set description so that it can in theory persist as data for models (e.g., ISO) that do not have the PackagedProduct object.
                if (item?.Description != null)
                {
                    packagedProduct.ContextItems.Add(
                                new ContextItem()
                                {
                                    Code = "Description",
                                    Value = item?.Description
                                });

                }
                if (item?.Packaging?.PerPackageQuantity?.Content != null)
                {
                    packagedProduct.ContextItems.Add(
                                new ContextItem()
                                {
                                    Code = "PerPackageQuantity",
                                    Value = item.Packaging.PerPackageQuantity.Content.ToString()
                                });

                }
                if (item?.Packaging?.PerPackageQuantity?.UnitCode != null)
                {
                    packagedProduct.ContextItems.Add(
                                new ContextItem()
                                {
                                    Code = "PerPackageQuantityUnitCode",
                                    Value = item.Packaging.PerPackageQuantity.UnitCode
                                });

                }
                //The below identifiers are set as ContextItems vs. UniqueIDs so that they can import/export hierarchically
                //based on the logic in the ISO plugin to handle hierarchical PackagedProducts & PackagedProductInstances
                if (item?.ManufacturerItemIdentification?.Id != null)
                {
                    packagedProduct.ContextItems.Add(
                                new ContextItem()
                                {
                                    Code = item?.ManufacturerItemIdentification?.TypeCode,
                                    Value = item?.ManufacturerItemIdentification?.Id
                                });

                }

                if (item?.Upcid != null)
                {
                    packagedProduct.ContextItems.Add(
                                new ContextItem()
                                {
                                    Code = "UPC",
                                    Value = item?.Upcid
                                });
                }

                if (item?.Gtinid != null)
                {
                    packagedProduct.ContextItems.Add(
                                new ContextItem()
                                {
                                    Code = "GTIN",
                                    Value = item?.Gtinid
                                });
                }

                Catalog.PackagedProducts.Add(packagedProduct);

                // Tie to a Product object
                Product product = GetProduct(shippedItemInstance);
                if (product != null)
                {
                    packagedProduct.ProductId = product.Id.ReferenceId;
                }
                else
                {
                    Errors.Add(new Error(null, "Mapper.GetPackagedProduct", $"Unable to create Product for Packaged Product {packagedProduct.Id.ReferenceId}", null));
                }
            }

            return packagedProduct;
        }

        private Product GetProduct(ShippedItemInstance shippedItemInstance)
        {
            // Look for product with a description that matches the shipped item instance
            Product product = Catalog.Products.FirstOrDefault(p => p.Description == shippedItemInstance.DisplayName);

            if (product == null && shippedItemInstance?.DisplayName != null)
            {
                if (shippedItemInstance.TypeCode == null || shippedItemInstance.TypeCode.ToLower() == "seed") 
                {
                    product = new CropVarietyProduct();
                }
                else
                {
                    product = new GenericProduct();
                }

                product.Description = shippedItemInstance.DisplayName;
                product.ContextItems.AddRange(CreateProductContextItems(shippedItemInstance));

                Catalog.Products.Add(product);
            }

            return product;
        }

        private List<ContextItem> CreateProductContextItems(ShippedItemInstance shippedItemInstance)
        {
            List<ContextItem> contextItems = new List<ContextItem>();

            if (shippedItemInstance.TypeCode != null)
            {
                contextItems.Add(CreateContextItem("TypeCode", shippedItemInstance.TypeCode));
            }
            if (shippedItemInstance.Item?.Description != null)
            {
                contextItems.Add(CreateContextItem("ItemDescription", shippedItemInstance.Item.Description));
            }
            if (shippedItemInstance.Item?.ProductName != null)
            {
                contextItems.Add(CreateContextItem("ItemProductName", shippedItemInstance.Item.ProductName));
            }
            if (shippedItemInstance.Item?.BrandName != null)
            {
                contextItems.Add(CreateContextItem("ItemBrandName", shippedItemInstance.Item.BrandName));
            }
            if (shippedItemInstance.Item?.VarietyName != null)
            {
                contextItems.Add(CreateContextItem("ItemVarietyName", shippedItemInstance.Item.VarietyName));
            }

            // Classification
            ContextItem classificationContextItem = CreateContextItem("Item.Classification", null);
            if (shippedItemInstance.Item.Classification.Codes.Code != null)
            {
                classificationContextItem.NestedItems.Add(CreateContextItem("typeCode", shippedItemInstance.Item.Classification.TypeCode));
            }
            if (shippedItemInstance.Item.Classification.Codes?.Code?.Count > 0)
            {
                ContextItem codesContextItem = CreateContextItem("codes", null);
                int codeIndex = 0;
                foreach (ClassificationCodesCode code in shippedItemInstance.Item.Classification.Codes.Code)
                {
                    ContextItem codeContextItem = CreateContextItem((++codeIndex).ToString(), null);

                    if (code.Content != null)
                    {
                        codeContextItem.NestedItems.Add(CreateContextItem("content", code.Content));
                    }
                    if (code.ListAgencyId != null)
                    {
                        codeContextItem.NestedItems.Add(CreateContextItem("listAgencyIdentifier", code.ListAgencyId));
                    }
                    if (code.TypeCode != null)
                    {
                        codeContextItem.NestedItems.Add(CreateContextItem("typeCode", code.TypeCode));
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

            // ManufacturingParty
            ContextItem manufacturingPartyContextItem = CreateContextItem("manufacturingParty", null);
            if (shippedItemInstance.Item.ManufacturingParty?.Name != null)
            {
                manufacturingPartyContextItem.NestedItems.Add(CreateContextItem("ManufacturerName", shippedItemInstance.Item.ManufacturingParty.Name));
            }
            ContextItem identifierContextItem = CreateContextItem("ManufacturerId", null);
            if (shippedItemInstance.Item.ManufacturingParty?.Id != null)
            {
                identifierContextItem.NestedItems.Add(CreateContextItem("Id", shippedItemInstance.Item.ManufacturingParty.Id));
            }
            
            if (identifierContextItem.NestedItems.Count > 0)
            {
                manufacturingPartyContextItem.NestedItems.Add(identifierContextItem);
            }
            if (manufacturingPartyContextItem.NestedItems.Count > 0)
            {
               contextItems.Add(manufacturingPartyContextItem);
            }

            return contextItems;
        }

        private void SetManufacturerAndBrand(ShippedItemInstance shippedItemInstance)
        {
            //Set Manufacturer & Brand as available
            var product = GetProduct(shippedItemInstance);
            if (product != null)
            {
                if (shippedItemInstance.Item.ManufacturingParty?.Name != null)
                {
                    var manufacturer = Catalog.Manufacturers.FirstOrDefault(m => m.Description == shippedItemInstance.Item.ManufacturingParty.Name);
                    if (manufacturer == null)
                    {
                        manufacturer = new Manufacturer() { Description = shippedItemInstance.Item.ManufacturingParty.Name };
                        Catalog.Manufacturers.Add(manufacturer);
                    }
                    product.ManufacturerId = manufacturer.Id.ReferenceId;
                }

                if (shippedItemInstance.Item?.BrandName != null)
                {
                    var brand = Catalog.Brands.FirstOrDefault(b => b.Description == shippedItemInstance.Item.BrandName);
                    if (brand == null)
                    {
                        brand = new Brand() { Description = shippedItemInstance.Item.BrandName, ManufacturerId = product.ManufacturerId ?? 0};
                        Catalog.Brands.Add(brand);
                    }
                    product.BrandId = brand.Id.ReferenceId;
                }
            }
        }

        private void SetCrop(ShippedItemInstance shippedItemInstance)
        {
            //Set Crop as available
            if (shippedItemInstance.Item.Classification?.TypeCode != null &&
                shippedItemInstance.Item.Classification?.TypeCode.ToLower() == "crop")
            {
                var product = GetProduct(shippedItemInstance);
                if (product != null && product is CropVarietyProduct)
                {
                    var cropInformation = shippedItemInstance.Item.Classification?.Codes?.Code?.FirstOrDefault();
                    if (cropInformation != null)
                    {
                        string cropName = cropInformation.TypeCode;
                        string cropID = cropInformation.Content;
                        string idAgency = cropInformation.ListAgencyId;
                        Crop crop = Catalog.Crops.FirstOrDefault(c => c.Name == cropName);
                        if (crop == null)
                        {
                            crop = new Crop() { Name = cropName };
                            crop.Id.UniqueIds.Add(new UniqueId() { Source = idAgency, IdType = IdTypeEnum.String, Id = cropID });
                            Catalog.Crops.Add(crop);
                        }
                        ((CropVarietyProduct)product).CropId = crop.Id.ReferenceId;
                    }
                }
            }
        }


        private void SetGrower(ShippedItemInstance shippedItemInstance)
        {
            //Set Grower as available
            ShipToParty modelGrower = shippedItemInstance.ShipmentReference.ShipToParty;
            if (modelGrower != null)
            {
                Grower grower = Catalog.Growers.FirstOrDefault(c => c.Name == modelGrower.Name);
                if (grower == null)
                {
                    grower = new Grower() { Name = modelGrower.Name };
                    if (modelGrower.Location?.Glnid != null)
                    {
                        UniqueId id = new UniqueId() { Id = modelGrower.Location.Glnid, Source = "GLN", IdType = IdTypeEnum.String };
                        grower.Id.UniqueIds.Add(id);
                    }
                    Catalog.Growers.Add(grower);
                }
            }
        }

        #endregion
    }
}
