# ShippedItemInstancePlugin
This plugin reads the 'connectSpec' ShippedItemInstance V4 JSON and map the properties to the ADM classes, and the related ContextItems:
- Product; including the product type and product composition
- PackagedProductInstance == > should this be deprecated, and moved to Product?
- PackagedProduct ==> should this be deprecated and moved to Products

The above questions relate to the newly developed ADAPT Standard which does not include the PackagedProductInstance and PackagedProduct.  Also the PackagedProductInstance should have all the data elements that PackagedProductInstance plus lot identifiers, and quantitative measurements.  The modeling of instances should be reconsidered there could be a ProductInstance that could be packaged.  Is the package itself worthy of being modeled as an instance, as while as an example a seed bag may have a tag sewn to it, it is only describing the seed instance inside the bag.

Key properties from the JSON include:
- Display name (concatenatation of shortened description and Lot Id); this is due to the inability for displays to also show the Brand, Product, LotId and ShipmentId in the product list
- Product Description
- Product TypeCode (equivalent to ADAPT ProductType enum as per https://github.com/ADAPT/Standard/blob/main/adapt-data-type-definitions.json)
- GTIN (Global Trade Item Number, as managed by GS1)
- UID (RFID, barcode, etc); for automatic identification and selection from a pre-populated product list
- Lot Id
- ShipmentID
- Retailer GLN (Global Location NMumber as managed by GS1 and licenced in blocks by AgGateway)
- Carrier SCAC
- Shipment Unit (semi-trailer)
- Quantitative Measurement (e.g., seed size)
- Item Composition is seed treatment and can be mapped to ProductComposition with appropriorate product type

From the ADM, either the ISOXML.zip (TASKDATA.XML and LINKLIST.XML) or the ADM.zip output can be generated.

