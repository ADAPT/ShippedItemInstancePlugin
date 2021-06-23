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
using System.Text;
using Newtonsoft.Json;

namespace AgGateway.ADAPT.ShippedItemInstancePlugin.Model
{
    public class Document
    {
        [JsonProperty("ShippedItemInstance")]
        public List<ShippedItemInstance> ShippedItemInstances { get; set; }
    }

    public class ShippedItemInstance
    {
        public string TypeCode { get; set; }
        public Identifier Identifier { get; set; }
        [JsonProperty("ItemIdentifierSet")]
        public List<ItemIdentifierSet> ItemIdentifierSets { get; set; }
        public ItemIdentifier Uid { get; set; }
        public Lot Lot { get; set; }
        public Item Item { get; set; }
        public Quantity Quantity { get; set; }
        public Identifier Description { get; set; }
        public Classification Classification { get; set; }
        public Packaging Packaging { get; set; }
        public DocumentReference DocumentReference { get; set; }
        public Attachment Attachment { get; set; }
        public ManufacturingParty ManufacturingParty { get; set; }
        [JsonProperty("Party")]
        public List<Party> Parties { get; set; }
        public Result Results { get; set; }
    }

    public class Identifier
    {
        public string Content { get; set; }
        public string TypeCode { get; set; }
    }

    public class ItemIdentifier
    {
        public string Content { get; set; }
        public string SchemeIdentifier { get; set; }
        public string SchemeAgencyIdentifier { get; set; }
        public string TypeCode { get; set; }
    }

    public class ItemIdentifierSet
    {
        public string TypeCode { get; set; }
        public string SchemeIdentifier { get; set; }
        public string SchemeVersionIdentifier { get; set; }
        public string SchemeAgencyIdentifier { get; set; }
        [JsonProperty("Identifier")]
        public List<ItemIdentifier> Identifiers { get; set; }
    }

    public class Lot
    {
        public Identifier Identifier { get; set; }
        [JsonProperty("SerialNumberIdentifier")]
        public List<string> SerialNumberIdentifiers { get; set; }
    }

    public class Item
    {
        public ManufacturerItemIdentification ManufacturerItemIdentification { get; set; }
        public string Upcid { get; set; }
        public string Gtinid { get; set; }
        public string Description { get; set; }
        public string ProductName { get; set; }
        public string BrandName { get; set; }
        public string VarietyName { get; set; }
    }

    public class ManufacturerItemIdentification
    {
        public string TypeCode { get; set; }
        public string Identifier { get; set; }
    }

    public class Quantity
    {
        public string Content { get; set; }
        public string UnitCode { get; set; }
    }

    public class Classification
    {
        public string TypeCode { get; set; }
        public CodeList Codes { get; set; }
    }

    public class Code
    {
        public string Content { get; set; }
        public string ListAgencyIdentifier { get; set; }
        public string TypeCode { get; set; }
    }

    public class CodeList
    {
        [JsonProperty("Code")]
        public List<Code> Codes { get; set; }
    }

    public class Packaging
    {
        public string TypeCode { get; set; }
        public string Identifier { get; set; }
    }

    public class DocumentReference
    {
        public string TypeCode { get; set; }
        public Identifier Identifier { get; set; }
        public string DocumentDateTime { get; set; }
    }

    public class Attachment
    {
        public string TypeCode { get; set; }
        public string Uri { get; set; }
    }

    public class ManufacturingParty
    {
        public Identifier Identifier { get; set; }
        public string Name { get; set; }
    }

    public class Party
    {
        public string TypeCode { get; set; }
        [JsonProperty("Identifier")]
        public List<Identifier> Identifiers { get; set; }
        public string Name { get; set; }
        public Location Location { get; set; }
    }

    public class Location
    {
        public string Name { get; set; }
        public string Glnid { get; set; }
    }

    public class Result
    {
        public List<Quantitative> Quantitative { get; set; }
    }

    public class Quantitative
    {
        public string TypeCode { get; set; }
        public string Name { get; set; }
        public UomCode UomCode { get; set; }
        public string SignificantDigitsNumber { get; set; }
        [JsonProperty("Measurement")]
        public Measurement[] Measurements { get; set; }
    }

    public class UomCode
    {
        public string Content { get; set; }
        public string ListIdentifier { get; set; }
        public string ListAgencyIdentifier { get; set; }
    }

    public class Measurement
    {
        public string DateTime { get; set; }
        public string Measure { get; set; }
    }
}
