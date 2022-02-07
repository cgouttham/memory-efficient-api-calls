﻿namespace ConsoleApp1
{
    using System;
    using System.IO;

    /// <summary>
    /// Substrate API Import Item Request Payload type. More information here: https://docs.substrate.microsoft.net/docs/Substrate-APIs-and-services/Content-APIs/Generic-Item/Generic-Item-API-overview.html?uid=generic-item-api-overview
    /// </summary>
    [Serializable]
    public class ImportItemRequestBodyWithData
    {
        /// <summary>
        /// Substrate API Generic Item Import Endpoint's Request Payload Type constructor
        /// </summary>
        /// <param name="idFormat">Id format.</param>
        /// <param name="rawData">Raw Data of Generic Item</param>
        /// <param name="folderId">Folder id of generic item</param>
        public ImportItemRequestBodyWithData(IdFormat idFormat, string folderId, string data)
        {
            this.IdFormat = idFormat;
            this.Data = data;
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
}
