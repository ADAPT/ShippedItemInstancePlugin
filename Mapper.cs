/*******************************************************************************
  * Copyright (C) 2021 AgGateway and ADAPT Contributors
  * All rights reserved. This program and the accompanying materials
  * are made available under the terms of the Eclipse Public License v1.0
  * which accompanies this distribution, and is available at
  * http://www.eclipse.org/legal/epl-v10.html <http://www.eclipse.org/legal/epl-v10.html> 
  *
  * Contributors:
  *    Rob Cederberg, Kelly Nelson - initial implementation
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

namespace AgGateway.ADAPT.ShippedItemInstancePlugin
{
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
        public IList<IError> MapDocument(Model.Document document)
        {
            Errors = new List<IError>();

            foreach (Model.ShippedItemInstance shippedItemInstance in document.ShippedItemInstances)
            {
                MapShippedItemInstance(shippedItemInstance);
            }
            return Errors;
        }

        private void MapShippedItemInstance(Model.ShippedItemInstance shippedItemInstance)
        {
            PackagedProductInstance packagedProductInstance = new PackagedProductInstance();

            packagedProductInstance.Description = string.Format("Shipment {0}", shippedItemInstance.Identifier?.Content);

            double quantity;
            if (double.TryParse(shippedItemInstance.Quantity?.Content, out quantity))
            {
                packagedProductInstance.ProductQuantity = CreateRepresentationValue(quantity, shippedItemInstance.Quantity.UnitCode);
            }
            else
            {
                Errors.Add(new Error(string.Empty, "ShippedItemInstanceMapper.MapShippedItemInstance", $"Quantity {shippedItemInstance.Quantity?.Content} is invalid.", string.Empty));
            }

            packagedProductInstance.ContextItems.AddRange(CreatePackagedProductInstanceContextItems(shippedItemInstance));

            PackagedProduct packagedProduct = GetPackagedProduct(shippedItemInstance);

            if (packagedProduct != null)
            {
                packagedProductInstance.PackagedProductId = packagedProduct.Id.ReferenceId;
            }
            else
            {
                Errors.Add(new Error(null, "Mapper.MapShippedItemInstance", $"Couldn't create PackagedProduct for PackageProductInstance {packagedProductInstance.Id.ReferenceId}", null));
            }

            Catalog.PackagedProductInstances.Add(packagedProductInstance);

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

        private List<ContextItem> CreatePackagedProductInstanceContextItems(Model.ShippedItemInstance shippedItemInstance)
        {
            List<ContextItem> items = new List<ContextItem>();

            // Lot
            if (shippedItemInstance.Lot?.Identifier?.Content != null)
            {
                items.Add(CreateContextItem("Lot", shippedItemInstance.Lot?.Identifier?.Content));
            }

            // Packaging
            ContextItem contextItem = CreateContextItem("Packaging", null);

            // Add Packaging nested items
            if (shippedItemInstance.Packaging?.TypeCode != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("typeCode", shippedItemInstance.Packaging.TypeCode));
            }
            if (shippedItemInstance.Packaging?.Identifier != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("identifier", shippedItemInstance.Packaging.Identifier));
            }
            if (contextItem.NestedItems.Count > 0)
            {
                items.Add(contextItem);
            }

            // DocumentReference
            contextItem = CreateContextItem("DocumentReference", null);

            // nested items
            ContextItem nestedContextItem = CreateContextItem("Identifier", null);

            // This one has it's own nested items
            if (shippedItemInstance.DocumentReference?.Identifier?.Content != null)
            {
                nestedContextItem.NestedItems.Add(CreateContextItem("content", shippedItemInstance.DocumentReference?.Identifier?.Content));
            }
            if (shippedItemInstance.DocumentReference?.Identifier?.TypeCode != null)
            {
                nestedContextItem.NestedItems.Add(CreateContextItem("typeCode", shippedItemInstance.DocumentReference.Identifier.TypeCode));
            }
            if (nestedContextItem.NestedItems.Count > 0)
            {
                contextItem.NestedItems.Add(nestedContextItem);
            }

            if (shippedItemInstance.DocumentReference?.TypeCode != null)
            {
                 contextItem.NestedItems.Add(CreateContextItem("typeCode", shippedItemInstance.DocumentReference.TypeCode));
            }

            if (shippedItemInstance.DocumentReference?.DocumentDateTime != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("documentDateTime", shippedItemInstance.DocumentReference.DocumentDateTime.ToString()));
            }
            if (contextItem.NestedItems.Count > 0)
            {
                items.Add(contextItem);
            }

            // Identifier
            contextItem = CreateContextItem("Identifier", null);

            // nested items
            if (shippedItemInstance.Identifier?.TypeCode != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("typeCode", shippedItemInstance.Identifier.TypeCode));
            }
            if (shippedItemInstance.Identifier?.Content != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("content", shippedItemInstance.Identifier.Content));
            }
            if (contextItem.NestedItems.Count > 0)
            {
                items.Add(contextItem);
            }

            // ItemIdentifierSet
            if (shippedItemInstance.ItemIdentifierSets?.Count > 0)
            {
                contextItem = CreateItemIdentifierSetsContextItem(shippedItemInstance);
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
            if (shippedItemInstance.Uid?.SchemeIdentifier != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("schemaIdentifier", shippedItemInstance.Uid.SchemeIdentifier));
            }
            if (shippedItemInstance.Uid?.SchemeAgencyIdentifier != null)
            {
                contextItem.NestedItems.Add(CreateContextItem("schemaAgencyIdentifier", shippedItemInstance.Uid.SchemeAgencyIdentifier));
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
            if (shippedItemInstance.Results?.Quantitative.Count > 0)
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

        private ContextItem CreateItemIdentifierSetsContextItem(Model.ShippedItemInstance shippedItemInstance)
        {
            ContextItem itemIdentifierSetsContextItem = CreateContextItem("ItemIdentifierSets", null);

            int identifierSetIndex = 0;
            foreach (Model.ItemIdentifierSet itemIdentifierSet in shippedItemInstance.ItemIdentifierSets)
            {
                ContextItem itemIdentifierSetContextItem = CreateContextItem((++identifierSetIndex).ToString(), null);
                if (itemIdentifierSet.SchemeIdentifier != null)
                {
                    itemIdentifierSetContextItem.NestedItems.Add(CreateContextItem("schemeIdentifier", itemIdentifierSet.SchemeIdentifier));
                }
                if (itemIdentifierSet.SchemeVersionIdentifier != null)
                {
                    itemIdentifierSetContextItem.NestedItems.Add(CreateContextItem("schemaVersionIdentifier", itemIdentifierSet.SchemeVersionIdentifier));
                }
                if (itemIdentifierSet.SchemeAgencyIdentifier != null)
                {
                    itemIdentifierSetContextItem.NestedItems.Add(CreateContextItem("schemaAgencyIdentifier", itemIdentifierSet.SchemeAgencyIdentifier));
                }
                if (itemIdentifierSet.TypeCode != null)
                {
                    itemIdentifierSetContextItem.NestedItems.Add(CreateContextItem("typeCode", itemIdentifierSet.TypeCode));
                }

                ContextItem identifiersContextItem = CreateContextItem("identifiers", null);
                int identifierIndex = 0;
                foreach (Model.ItemIdentifier identifier in itemIdentifierSet.Identifiers)
                {
                    ContextItem identifierContextItem = CreateContextItem((++identifierIndex).ToString(), null);
                    if (identifier.Content != null)
                    {
                        identifierContextItem.NestedItems.Add(CreateContextItem("content", identifier.Content));
                    }
                    if (identifier.SchemeIdentifier != null)
                    {
                        identifierContextItem.NestedItems.Add(CreateContextItem("schemaIdentifier", identifier.SchemeIdentifier));
                    }
                    if (identifier.SchemeAgencyIdentifier != null)
                    {
                        identifierContextItem.NestedItems.Add(CreateContextItem("schemaAgencyIdentifier", identifier.SchemeAgencyIdentifier));
                    }
                    if (identifier.TypeCode != null)
                    {
                        identifierContextItem.NestedItems.Add(CreateContextItem("typeCode", identifier.TypeCode));
                    }
                    if (identifierContextItem.NestedItems.Count > 0)
                    {
                        identifiersContextItem.NestedItems.Add(identifierContextItem);
                    }
                }

                if (identifiersContextItem.NestedItems.Count > 0)
                {
                    itemIdentifierSetContextItem.NestedItems.Add(identifiersContextItem);
                }

                if (itemIdentifierSetContextItem.NestedItems.Count > 0)
                {
                    itemIdentifierSetsContextItem.NestedItems.Add(itemIdentifierSetContextItem);
                }
            }

            return itemIdentifierSetsContextItem;
        }

        private ContextItem CreateQuantitativeResultsContextItem(Model.ShippedItemInstance shippedItemInstance)
        {
            ContextItem results = CreateContextItem("QuantitativeResults", null);

            int quantitateResultIndex = 0;
            foreach (Model.Quantitative quantitativeResult in shippedItemInstance.Results.Quantitative)
            {
                ContextItem quantitativeResultContextItem = CreateContextItem((++quantitateResultIndex).ToString(), null);

                if (quantitativeResult.TypeCode != null)
                {
                    quantitativeResultContextItem.NestedItems.Add(CreateContextItem("typeCode", quantitativeResult.TypeCode));
                }
                if (quantitativeResult.Name != null)
                {
                    quantitativeResultContextItem.NestedItems.Add(CreateContextItem("name", quantitativeResult.Name));
                }

                // Unit of Measure
                ContextItem uomCodeContextItem = CreateContextItem("uomCode", null);
                if (quantitativeResult.UomCode?.Content != null)
                {
                    uomCodeContextItem.NestedItems.Add(CreateContextItem("content", quantitativeResult.UomCode.Content));
                }
                if (quantitativeResult.UomCode?.ListIdentifier != null)
                {
                    uomCodeContextItem.NestedItems.Add(CreateContextItem("listIdentifier", quantitativeResult.UomCode.ListIdentifier));
                }
                if (quantitativeResult.UomCode?.ListAgencyIdentifier != null)
                {
                    uomCodeContextItem.NestedItems.Add(CreateContextItem("listAgencyIdentifier", quantitativeResult.UomCode.ListAgencyIdentifier));
                }
                if (uomCodeContextItem.NestedItems.Count > 0)
                {
                    quantitativeResultContextItem.NestedItems.Add(uomCodeContextItem);
                }

                //Significant Digits
                if (quantitativeResult.SignificantDigitsNumber != null)
                {
                    quantitativeResultContextItem.NestedItems.Add(CreateContextItem("significantDigitsNumber", quantitativeResult.SignificantDigitsNumber));
                }

                // Measurement
                ContextItem measurementsContextItem = CreateContextItem("measurements", null);
                int measurementIndex = 0;
                foreach (Model.Measurement measurement in quantitativeResult.Measurements)
                {
                    ContextItem measurementContextItem = CreateContextItem((++measurementIndex).ToString(), null);
                    if (measurement.DateTime != null)
                    {
                        measurementContextItem.NestedItems.Add(CreateContextItem("dateTime", measurement.DateTime.ToString()));
                    }
                    if (measurement.Measure != null)
                    {
                        measurementContextItem.NestedItems.Add(CreateContextItem("measure", measurement.Measure));
                    }

                    if (measurementContextItem.NestedItems.Count > 0)
                    {
                        measurementsContextItem.NestedItems.Add(measurementContextItem);
                    }
                }

                if (measurementsContextItem.NestedItems.Count > 0)
                {
                    quantitativeResultContextItem.NestedItems.Add(measurementsContextItem);
                }

                // Add to results if any nested items were added
                if (quantitativeResultContextItem.NestedItems.Count > 0)
                {
                    results.NestedItems.Add(quantitativeResultContextItem);
                }
            }          

            return results;
        }

        private PackagedProduct GetPackagedProduct(Model.ShippedItemInstance shippedItemInstance)
        {
            PackagedProduct packagedProduct = null;
            Model.Item item = shippedItemInstance.Item;
            if (item?.ManufacturerItemIdentification?.Identifier == null && item?.Gtinid == null && item?.Upcid == null)
            {
                // No ids specified so use the descriptionn to find a PackageProduct that matches
                packagedProduct = Catalog.PackagedProducts.FirstOrDefault(pp => pp.Description == item?.Description);
            }
            else
            {
                // Try to find a matching PackagedProduct based on the ManufacturerItemIdentifier, UPC Id or GTIN Id
                packagedProduct = Catalog.PackagedProducts.FirstOrDefault(pp => pp.ContextItems.Any(i => (i.Code == item?.ManufacturerItemIdentification?.TypeCode && i.Value == item?.ManufacturerItemIdentification?.Identifier) ||
                                                                                                         (i.Code == "UPC" && i.Value == item?.Upcid) || (i.Code == "GTIN" && i.Value == item?.Gtinid )));
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

                if (item?.ManufacturerItemIdentification?.Identifier != null)
                {
                    packagedProduct.ContextItems.Add(
                                new ContextItem()
                                {
                                    Code = item?.ManufacturerItemIdentification?.TypeCode,
                                    Value = item?.ManufacturerItemIdentification?.Identifier
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

        private Product GetProduct(Model.ShippedItemInstance shippedItemInstance)
        {
            // Look for product with a description that matches the shipped item instance
            Product product = Catalog.Products.FirstOrDefault(p => p.Description == shippedItemInstance.Description?.Content);

            if (product == null && shippedItemInstance.Description?.Content != null)
            {
                if (shippedItemInstance.TypeCode == "seed")
                {
                    product = new CropVarietyProduct();
                }
                else
                {
                    product = new GenericProduct();
                }

                product.Description = shippedItemInstance.Description?.Content;
                product.ContextItems.AddRange(CreateProductContextItems(shippedItemInstance));

                Catalog.Products.Add(product);
            }

            return product;
        }

        private List<ContextItem> CreateProductContextItems(Model.ShippedItemInstance shippedItemInstance)
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
            ContextItem classificationContextItem = CreateContextItem("Classification", null);
            if (shippedItemInstance.Classification?.TypeCode != null)
            {
                classificationContextItem.NestedItems.Add(CreateContextItem("typeCode", shippedItemInstance.Classification.TypeCode));
            }
            if (shippedItemInstance.Classification?.Codes?.Codes?.Count > 0)
            {
                ContextItem codesContextItem = CreateContextItem("codes", null);
                int codeIndex = 0;
                foreach (Model.Code code in shippedItemInstance.Classification.Codes.Codes)
                {
                    ContextItem codeContextItem = CreateContextItem((++codeIndex).ToString(), null);

                    if (code.Content != null)
                    {
                        codeContextItem.NestedItems.Add(CreateContextItem("content", code.Content));
                    }
                    if (code.ListAgencyIdentifier != null)
                    {
                        codeContextItem.NestedItems.Add(CreateContextItem("listAgencyIdentifier", code.ListAgencyIdentifier));
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
            if (shippedItemInstance.ManufacturingParty?.Name != null)
            {
                manufacturingPartyContextItem.NestedItems.Add(CreateContextItem("name", shippedItemInstance.ManufacturingParty.Name));
            }
            ContextItem identifierContextItem = CreateContextItem("identifier", null);
            if (shippedItemInstance.ManufacturingParty?.Identifier?.Content != null)
            {
                identifierContextItem.NestedItems.Add(CreateContextItem("content", shippedItemInstance.ManufacturingParty.Identifier.Content));
            }
            if (shippedItemInstance.ManufacturingParty?.Identifier?.TypeCode != null)
            {
                identifierContextItem.NestedItems.Add(CreateContextItem("typeCode", shippedItemInstance.ManufacturingParty.Identifier.TypeCode));
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

        private void SetManufacturerAndBrand(Model.ShippedItemInstance shippedItemInstance)
        {
            //Set Manufacturer & Brand as available
            if (shippedItemInstance.ManufacturingParty?.Name != null)
            {
                var product = GetProduct(shippedItemInstance);
                if (product != null)
                {
                    var manufacturer = Catalog.Manufacturers.FirstOrDefault(m => m.Description == shippedItemInstance.ManufacturingParty.Name);
                    if (manufacturer == null)
                    {
                        manufacturer = new Manufacturer() { Description = shippedItemInstance.ManufacturingParty.Name };
                        Catalog.Manufacturers.Add(manufacturer);
                    }
                    product.ManufacturerId = manufacturer.Id.ReferenceId;

                    if (shippedItemInstance.Item?.BrandName != null)
                    {
                        var brand = Catalog.Brands.FirstOrDefault(b => b.Description == shippedItemInstance.Item.BrandName);
                        if (brand == null)
                        {
                            brand = new Brand() { Description = shippedItemInstance.Item.BrandName, ManufacturerId = manufacturer.Id.ReferenceId };
                            Catalog.Brands.Add(brand);
                        }
                        product.BrandId = brand.Id.ReferenceId;
                    }
                }
            }
        }

        private void SetCrop(Model.ShippedItemInstance shippedItemInstance)
        {
            //Set Crop as available
            if (shippedItemInstance.Classification?.TypeCode == "Crop")
            {
                var product = GetProduct(shippedItemInstance);
                if (product != null && product is CropVarietyProduct)
                {
                    var cropInformation = shippedItemInstance.Classification?.Codes?.Codes?.FirstOrDefault();
                    if (cropInformation != null)
                    {
                        string cropName = cropInformation.TypeCode;
                        string cropID = cropInformation.Content;
                        string idAgency = cropInformation.ListAgencyIdentifier;
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


        private void SetGrower(Model.ShippedItemInstance shippedItemInstance)
        {
            //Set Grower as available
            Model.Party modelGrower = shippedItemInstance.Parties.FirstOrDefault(p => p.TypeCode == "grower");
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
