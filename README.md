# ShippedItemInstancePlugin
The V4 SII plugin is used in the scope of In-Field Product ID, provide the middleware to transform a 'ship notice'  contain product shipped to the farmer and load the resulting product list into the MICS (tractor display) either via the ISO XML or ADM files.  
It provides the V4 implementation of the GET /setup-files endpoint as discussed here:
https://github.com/AgGateway/In-FieldProductID


## Stepping stone to ADAPT Standard
The plug-in reads the AgGateway defined ShippedItemInstance V4 JSON and maps the properties to the following ADM classes, and related ContextItems:

- Catalog.Products; including the product type and product composition
- Catalog.Brands
- Catalog.Manufacturers
- Catalog.Crops

## Sample V4 ShippedItemInstance payloads
Sample payloads are located in SampleData/v4 folders in this repo.  They are ProductType = SEED use cases, including support for seed (item) treatment.  

## Use Cases
The following other use cases are supported but currently now their substance ingredients (mix):
- FERTILIZER_CHEMICAL
- FERTILIZER_ORGANIC
- PLANT (e.g., rice and other seedlings)
- HERBICIDE
- INSECTICIDE
- FUNGICIDE
- RODENTICIDE
- NEMATICIDE
- SPRAY_ADJUVANT
- NITROGEN_STABILIZER
- GROWTH_REGULATOR
- DEFOLIANT

## Major changes in this release compared to original pilot release:

*V4.0.1 Patch Release*
1. This patch releases now references ```List<ShippedItemInstance>``` and not ```<ShippedItemInstanceList>``` model types.
2. ILogger is introduces to allowing logs to appear in Azure Application Insights, searchable in Transaction Logs.
3. Most importantly, adds the public method TransformSIIToADM to allow in-memory model to model transformation instead of creating a temporary folder and dropping payloads to file.  On Azure this is optimal.  The method accepts ```List<ShippedItemInstance>``` and returns a single ```<ApplicationDataModel>```.

*V4.0.0 Release*
1. the ShippedItemInstance JSON RESTFul API was approved to v4 in Standards and Guidelines, therefore the JSON path references have changed and the Mapper had to be updated;
2. the Model is auto-generated off the RESTful Web API yaml using Swagger CodeGen, which the yaml itself was generated from connectCenter (Score) using the connectCenter tool built by NIST;
3. the ADAPT Standard work has removed classes that were used in v1 of the plug-in a) PackagedProduct and b) PackageProductInstance.  The data elements mapped to those classes in the mapper where moved to product context items;
4. GetProduct() was being called twice, and this is reduced to one call, with the returned product being passed into the subsequent calls as necessary;
5. the approach used in this plug-in for Context Items has changed significantly to account for the generic identification and code structures used in CCTS (and more specifically connectSpec).  For example, the typeCode is mapped to code and content is mapped to value to reduce the number of contextItems and provide more realistic name/value pair approach;
6. The NuGet package version will bump to v4 to align with the version of the S&G approved model;
7. The Readme.io file has significant changes related to the intent of the plug-in.

## Known Issues:
- ```LookupProductType(string productType)``` was never historically implemented, likely to look up the data definition enumerations for ProductType.  CropVarietyProduct() appears to be defaulting that.  
- VarietyName could not be explicitly mapped to its own class.attribute placeholder in the Application Data Model C# class library.  Therefore CVT  (crop variety) is equivalent to PDT product.description.  See #17.
- The ShippedItemInstance V4 JSON repeats ShipmentReference in each array instance, which was a conscience decision in WG01 whose members agreed to have JSON that was similar to a flat file / CSV.  There is value for GET /shipped-item-instances queries that return data from multiple shipments, however, the intent of a QR printed on a shipping document that can be scanned is specific to one shipment.  The change in connectCenter is very easy if there is a desire to make change.  LINKLIST.XML will show repeating shipment information as  a result.
- PackageQuantity is hard-code, to a miss in enabling typeCode in shippedItemInstance.quantity. See #18.
