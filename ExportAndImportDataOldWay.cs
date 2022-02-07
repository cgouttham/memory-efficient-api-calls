namespace ConsoleApp1
{
    using Microsoft.ElcArchiveProcessor.Models;
    using Microsoft.ElcArchiveProcessor.Models.SubstrateApiRequestTypes;
    using Microsoft.Exchange.Common.Net;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;

    public static class ExportAndImportDataOldWay
    {
        public static async Task ExportAndImportLargeItemsNotEfficiently()
        {
            // Send Mock HttpRequest that returns a massive Json Payload
            // using Stream httpContentStream = await SendMockExportItemRequestFromFileAsync().ConfigureAwait(false);

            GenericItem genericItem = await ExportGenericItemOldWay().ConfigureAwait(false);
            // DataPlane's implementation of Batch Client uses StringBuilder. For larger items, this causes out of memory exception. For these large items, I will process them not using BatchClient.

            await ImportGenericItemOldWay("destination_folder_id", genericItem.Data, genericItem.ItemId).ConfigureAwait(false);
        }

        private static async Task<GenericItem> ExportGenericItemOldWay()
        {
            GenericItem genericItem = new GenericItem("itemId");

            using Stream httpContentStream = await GetLargeExportedItemOldWayAsync().ConfigureAwait(false);
            using TextReader textReader = new StreamReader(httpContentStream);
            using JsonTextReader jsonReader = new JsonTextReader(textReader);
            while (jsonReader.Read())
            {
                if (jsonReader.TokenType == JsonToken.PropertyName && jsonReader.Value.ToString() == "Data")
                {
                    jsonReader.Read();
                    if (jsonReader.TokenType == JsonToken.String && jsonReader.Value != null)
                    {
                        genericItem.Data = (string)jsonReader.Value;
                    }
                }
            }

            return genericItem;
        }

        private static async Task<Stream> GetLargeExportedItemOldWayAsync()
        {
            Stream mbStream = new MemoryStream();
            using (FileStream file = new FileStream("C:\\Users\\goch\\source\\repos\\ConsoleApp1\\testData\\large_items\\exported_40mb_item.txt", FileMode.Open, FileAccess.Read))
            {
                await file.CopyToAsync(mbStream).ConfigureAwait(false);
            }

            mbStream.Seek(0, SeekOrigin.Begin);
            return mbStream;
        }

        private static async Task ImportGenericItemOldWay(string folderRestId, string data, string itemId)
        {
            ImportItemRequestBodyWithData requestBody = new ImportItemRequestBodyWithData(IdFormat.RestId, folderRestId, data);

            using var httpOutputContent = CreateHttpContentEfficiently(requestBody);

            using (FileStream file = File.Create("C:\\Users\\goch\\source\\repos\\ConsoleApp1\\testData\\large_items\\output_exported_40mb_item_Old_Way.txt"))
            {
                await httpOutputContent.CopyToAsync(file).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Create Http Content From Stream: Resource used: https://johnthiriet.com/efficient-post-calls/
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static HttpContent CreateHttpContentEfficiently(object content)
        {
            HttpContent httpContent = null;

            if (content != null)
            {
                var ms = new MemoryStream();
                SerializeJsonIntoStream(content, ms);
                ms.Seek(0, SeekOrigin.Begin);
                httpContent = new StreamContent(ms);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return httpContent;
        }

        /// <summary>
        /// Serialize C# object into a Json Stream so that HttpContent can interpret it.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="stream"></param>
        private static void SerializeJsonIntoStream(object value, Stream stream)
        {
            using (var sw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
            using (var jtw = new JsonTextWriter(sw) { Formatting = Newtonsoft.Json.Formatting.None })
            {
                var js = new JsonSerializer();
                js.Serialize(jtw, value);
                jtw.Flush();
            }
        }
    }
}
