﻿/*******************************************************************************
  * Copyright (C) 2025 AgGateway and ADAPT Contributors
  * All rights reserved. This program and the accompanying materials
  * are made available under the terms of the Eclipse Public License v1.0
  * which accompanies this distribution, and is available at
  * http://www.eclipse.org/legal/epl-v10.html <http://www.eclipse.org/legal/epl-v10.html> 
  *
  *******************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using IO.Swagger.Models;

// add CodeGen namespace
// rename this file to Document.cs, and fix reference in the Mapper.cs

namespace AgGateway.ADAPT.ShippedItemInstancePlugin.Document
{
    //  The class Document would remain and reference the CodeGen model folder
    //
    public class Document
    {
        // the JsonProperty no longer exists in V4 as the root is now suppressed
        //
        // [JsonProperty("ShippedItemInstance")]
        public ShippedItemInstanceList ShippedItemInstances { get; set; }
    }


}
