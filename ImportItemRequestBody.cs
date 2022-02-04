// ***********************************************************************
// <copyright file="ImportItemRequestBody.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ***********************************************************************

namespace Microsoft.ElcArchiveProcessor.Models.SubstrateApiRequestTypes
{
    using System;
    using System.IO;

    /// <summary>
    /// Substrate API Import Item Request Payload type. More information here: https://docs.substrate.microsoft.net/docs/Substrate-APIs-and-services/Content-APIs/Generic-Item/Generic-Item-API-overview.html?uid=generic-item-api-overview
    /// </summary>
    [Serializable]
    public class ImportItemRequestBody
    {
        /// <summary>
        /// Substrate API Generic Item Import Endpoint's Request Payload Type constructor
        /// </summary>
        /// <param name="idFormat">Id format.</param>
        /// <param name="rawData">Raw Data of Generic Item</param>
        /// <param name="folderId">Folder id of generic item</param>
        public ImportItemRequestBody(IdFormat idFormat, string folderId)
        {
            this.IdFormat = idFormat;
            this.Data = "hello";
            this.FolderId = folderId;
        }

        /// <summary>
        /// Format of Id
        /// </summary>
        public IdFormat IdFormat { get; set; }

        /// <summary>
        /// Folder id 
        /// </summary>
        public string FolderId { get; set; }

        /// <summary>
        /// Raw Data of the item.
        /// </summary>
        public string Data { get; set; }

    }
    public enum IdFormat
    {
        RestId
    }
}
