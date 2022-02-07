// ***********************************************************************
// <copyright file="GenericItem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ***********************************************************************

namespace Microsoft.ElcArchiveProcessor.Models
{
    using System.Collections.Generic;
    using Microsoft.ElcArchiveProcessor.Models.SubstrateApiRequestTypes;

    /// <summary>
    /// Substrate Generic Item Data Type. More information here: https://docs.substrate.microsoft.net/docs/Substrate-APIs-and-services/Content-APIs/Generic-Item/Generic-Item-API-overview.html?uid=generic-item-api-overview
    /// </summary>
    public class GenericItem
    {
        /// <summary>
        /// Substrate API Generic Item Import Endpoint's constructor with just itemId
        /// </summary>
        /// <param name="itemId">Item Id.</param>
        public GenericItem(string itemId)
        {
            this.ItemId = itemId;
            this.IdFormat = IdFormat.RestId;
            this.Data = null;
            this.FolderId = null;
        }

        /// <summary>
        /// Substrate API Generic Item Import Endpoint's constructor without folderId
        /// </summary>
        /// <param name="itemId">Item's current id</param>
        /// <param name="idFormat">Id format.</param>
        /// <param name="rawData">Current Raw Data stored in the Generic Item</param>
        /// <param name="folderId">Current Location of the item.</param>
        /// <param name="previousItemId">Item's previous location before it moved or got deleted</param>
        public GenericItem(string itemId, IdFormat idFormat, string rawData = null, string folderId = null, string previousItemId = null)
        {
            this.ItemId = itemId;
            this.IdFormat = idFormat;
            this.Data = rawData;
            this.FolderId = folderId;
            this.PreviousItemId = previousItemId;
        }

        /// <summary>
        /// Item's current id
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// Raw Data of the item.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Format of Id
        /// </summary>
        public IdFormat IdFormat { get; set; }

        /// <summary>
        /// Folder id 
        /// </summary>
        public string FolderId { get; set; }

        /// <summary>
        /// Previous item id (Used when exporting or deleting items to store predeleted/premoved itemid)
        /// </summary>
        public string PreviousItemId { get; set; }
    }
}
