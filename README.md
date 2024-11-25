# ShippedItemInstancePlugin
This plugin reads the 'connectSpec' ShippedItemInstance V4 JSON and map the properties to the ADM classes, and the related ContextItems:
- Product
- PackagedProductInstance == > should this be deprecated, and moved to Product?
- PackagedProduct ==> should this be deprecated and moved to Products

Key properties from the JSON include:
- Display name (concatenatation of shortened description and Lot Id); this is due to the inability for displays to also show the Brand, Product, LotId and ShipmentId in the product list
- Product Description
- GTIN
- UID (RFID, barcode, etc); for automatic identification and selection from a pre-populated product list
- Lot Id
- ShipmentID
- Retailer GLN
- Carrier SCAC
- Shipment Unit (semi-trailer)
- Quantitative Measurement (e.g., seed size)
- Item Composition is seed treatment and can be mapped to ProductComposition with appropriorate product type

From the ADM, either the ISOXML.zip (TASKDATA.XML and LINKLIST.XML) or the ADM.zip output can be generated.

