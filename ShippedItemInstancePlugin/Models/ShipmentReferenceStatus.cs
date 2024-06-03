/*
 * AgGateway In-Field Product Id
 *
 * Allows the Retailer to send information about the actual product shipped to the Farmer, including shipment identifer, product identifiers, seed lot id, mixture batch id, seed treatment and product composition.  Allows the Farmer (via a mobile application or FMIS), or the Farmer's equipment manufacturer application (aka OEM Platform, e.g., Deere Operations Center, AGCO Fuse, CNH AFS, etc.) to retrieve the Product shipped by a Retailer using GET /setupfiles.   
 *
 * OpenAPI spec version: V4
 * Contact: wg01@aggateway.org
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace IO.Swagger.Models
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public partial class ShipmentReferenceStatus : IEquatable<ShipmentReferenceStatus>
    { 
        /// <summary>
        /// Gets or Sets Code
        /// </summary>

        [DataMember(Name="code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or Sets ReasonCode
        /// </summary>

        [DataMember(Name="reasonCode")]
        public string ReasonCode { get; set; }

        /// <summary>
        /// Gets or Sets Reason
        /// </summary>

        [DataMember(Name="reason")]
        public string Reason { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ShipmentReferenceStatus {\n");
            sb.Append("  Code: ").Append(Code).Append("\n");
            sb.Append("  ReasonCode: ").Append(ReasonCode).Append("\n");
            sb.Append("  Reason: ").Append(Reason).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ShipmentReferenceStatus)obj);
        }

        /// <summary>
        /// Returns true if ShipmentReferenceStatus instances are equal
        /// </summary>
        /// <param name="other">Instance of ShipmentReferenceStatus to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ShipmentReferenceStatus other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return 
                (
                    Code == other.Code ||
                    Code != null &&
                    Code.Equals(other.Code)
                ) && 
                (
                    ReasonCode == other.ReasonCode ||
                    ReasonCode != null &&
                    ReasonCode.Equals(other.ReasonCode)
                ) && 
                (
                    Reason == other.Reason ||
                    Reason != null &&
                    Reason.Equals(other.Reason)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hashCode = 41;
                // Suitable nullity checks etc, of course :)
                    if (Code != null)
                    hashCode = hashCode * 59 + Code.GetHashCode();
                    if (ReasonCode != null)
                    hashCode = hashCode * 59 + ReasonCode.GetHashCode();
                    if (Reason != null)
                    hashCode = hashCode * 59 + Reason.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
        #pragma warning disable 1591

        public static bool operator ==(ShipmentReferenceStatus left, ShipmentReferenceStatus right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ShipmentReferenceStatus left, ShipmentReferenceStatus right)
        {
            return !Equals(left, right);
        }

        #pragma warning restore 1591
        #endregion Operators
    }
}
