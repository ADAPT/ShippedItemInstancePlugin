# ShippedItemInstancePlugin
This plugin reads the 'connectSpec' ShippedItemInstance V4 JSON and map the properties to the ADM classes, and the related ContextItems:
- Product; including the product type and product composition
- PackagedProductInstance == > should this be deprecated, and moved to Product?
- PackagedProduct ==> should this be deprecated and moved to Products

The above questions relate to the newly developed ADAPT Standard which does not include the PackagedProductInstance and PackagedProduct.  Also the PackagedProductInstance should have all the data elements that PackagedProductInstance plus lot identifiers, and quantitative measurements. In

In the process of recommendations for the ADAPT standard should include need for a Measurement class (density is only one measurement) with name, value, unit code and a first-class UID attribute for selection from the pre-populated product list.  The view is that Product should really be named ProductInstance, as we are always dealing with real instances of a product that appear in a Catalog, and these instances are packaged.  Is the package itself worthy of being modeled as an instance, as while as an example a seed bag may have a tag sewn to it, it is only describing the seed instance inside the bag?

Key properties from the JSON mapped to Product class include:
- Display name (concatenatation of shortened description and Lot Id); this is due to the inability for displays to also show the Brand, Product, LotId and ShipmentId in the product list
- Product Description
- Product TypeCode 
-- equivalent to ADAPT ProductType enum as per https://github.com/ADAPT/Standard/blob/main/adapt-data-type-definitions.json)
-- our initial use case is SEED, but the ShippedItemInstance model has support for substance arrays for fertilizer and crop protection.
- Crop Type based on an agency code list such as USDA (many) or AgGateway; crop type will be of the form related to corn, soybeans, wheat, etc.

The rest of these elements must be mapped to ContextItems which many will be nested:
- GTIN (Global Trade Item Number, as managed by GS1)
- UID (RFID, barcode, etc)
-- for automatic identification and selection from a pre-populated product list
-- The barcode can identify a bag, seed tender box, or a tank
- Lot Id
- ShipmentID
- Retailer GLN (Global Location NMumber as managed by GS1 and licenced in blocks by AgGateway)
- Carrier SCAC
- Shipment Unit (semi-trailer)
- Quantitative Measurement (e.g., seed size)
- Item Composition is seed treatment and can be mapped to ProductComposition with appropriorate product type

From the ADM, either the ISOXML.zip (TASKDATA.XML and LINKLIST.XML) or the ADM.zip output can be generated.

