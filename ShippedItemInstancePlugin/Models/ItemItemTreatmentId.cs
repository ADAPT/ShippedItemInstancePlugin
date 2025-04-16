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
    public partial class ItemItemTreatmentId : IEquatable<ItemItemTreatmentId>
    { 
        /// <summary>
        /// Gets or Sets Content
        /// </summary>
        [Required]

        [DataMember(Name="content")]
        public string Content { get; set; }

        /// <summary>
        /// Gets or Sets SchemeAgencyId
        /// </summary>

        [DataMember(Name="schemeAgencyId")]
        public string SchemeAgencyId { get; set; }

        /// <summary>
        /// Gets or Sets TypeCode
        /// </summary>

        [DataMember(Name="typeCode")]
        public string TypeCode { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ItemItemTreatmentId {\n");
            sb.Append("  Content: ").Append(Content).Append("\n");
            sb.Append("  SchemeAgencyId: ").Append(SchemeAgencyId).Append("\n");
            sb.Append("  TypeCode: ").Append(TypeCode).Append("\n");
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
            return obj.GetType() == GetType() && Equals((ItemItemTreatmentId)obj);
        }

        /// <summary>
        /// Returns true if ItemItemTreatmentId instances are equal
        /// </summary>
        /// <param name="other">Instance of ItemItemTreatmentId to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ItemItemTreatmentId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return 
                (
                    Content == other.Content ||
                    Content != null &&
                    Content.Equals(other.Content)
                ) && 
                (
                    SchemeAgencyId == other.SchemeAgencyId ||
                    SchemeAgencyId != null &&
                    SchemeAgencyId.Equals(other.SchemeAgencyId)
                ) && 
                (
                    TypeCode == other.TypeCode ||
                    TypeCode != null &&
                    TypeCode.Equals(other.TypeCode)
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
                    if (Content != null)
                    hashCode = hashCode * 59 + Content.GetHashCode();
                    if (SchemeAgencyId != null)
                    hashCode = hashCode * 59 + SchemeAgencyId.GetHashCode();
                    if (TypeCode != null)
                    hashCode = hashCode * 59 + TypeCode.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
        #pragma warning disable 1591

        public static bool operator ==(ItemItemTreatmentId left, ItemItemTreatmentId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ItemItemTreatmentId left, ItemItemTreatmentId right)
        {
            return !Equals(left, right);
        }

        #pragma warning restore 1591
        #endregion Operators
    }
}
