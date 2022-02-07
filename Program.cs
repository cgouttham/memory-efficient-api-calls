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

    static class Program
    {
         /*
         * Imagine you sent an HttpRequest with a massive Json Payload. 
         * If your json consists of many tiny items in an array, you can use a json streamer no problemo. 
         * What if a single property has a value that is super large? e.g. a base64 encoded message that can be 50MB+. 
         * In that scenario, you'll need to do some Stream Gymnastics to handle the object in a memory efficient way.
         */
        static async Task Main(string[] args)
        {
            for (int i = 0; i < 20; i++)
            {
                // await ExportAndImportLargeItemsNotEfficiently().ConfigureAwait(false);
                await ExportAndImportLargeItemsEfficiently().ConfigureAwait(false);

            }
            Console.WriteLine("Done");
        }

        private static async Task ExportAndImportLargeItemsNotEfficiently()
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

        private static async Task ImportGenericItemOldWay(string folderRestId, string data, string itemId)
        {
            ImportItemRequestBodyWithData requestBody = new ImportItemRequestBodyWithData(IdFormat.RestId, folderRestId, data);

            using var httpOutputContent = CreateHttpContentEfficiently(requestBody);

            using (FileStream file = File.Create("C:\\Users\\goch\\source\\repos\\ConsoleApp1\\testData\\large_items\\output_exported_40mb_item_Old_Way.txt"))
            {
                await httpOutputContent.CopyToAsync(file).ConfigureAwait(false);
            }
        }


        private static async Task ExportAndImportLargeItemsEfficiently()
        {
            // Send Mock HttpRequest that returns a massive Json Payload
            // using Stream httpContentStream = await SendMockExportItemRequestFromFileAsync().ConfigureAwait(false);
            using Stream httpContentStream = await GetLargeExportedItemAsync().ConfigureAwait(false);

            // Get Data Property from Stream and store it in another stream.
            using Stream base64MailDataStream = ReadDataPropertyFromExportResponseAsStream(httpContentStream);

            httpContentStream.Flush();
            httpContentStream.Close();

            // Write Data Property into Import Item Request Body (HttpContent)
            var importItemRequestBody = new ImportItemRequestBody(IdFormat.RestId, "Mock_FolderId");

            await WriteImportItemRequestBodyToHttpRequest(importItemRequestBody, base64MailDataStream).ConfigureAwait(false);

            base64MailDataStream.Flush();
            base64MailDataStream.Close();
        }

        /// <summary>
        /// Write Import Item Request Body Object to a Http Request Body
        /// </summary>
        /// <param name="base64MailDataStream"></param>
        /// <returns></returns>
        private static async Task WriteImportItemRequestBodyToHttpRequest(ImportItemRequestBody importItemRequestBody, Stream base64MailDataStream)
        {
            XmlDictionaryWriter writer = null;
            try
            {
                MultiByteArrayMemoryStream outputHttpContentStream = new MultiByteArrayMemoryStream();

                writer = JsonReaderWriterFactory.CreateJsonWriter(outputHttpContentStream);


                // Write an element (this one is the root).
                writer.WriteStartElement("root");
                writer.WriteAttributeString("type", "object");

                writer.WriteStartElement($"{nameof(ImportItemRequestBody.IdFormat)}");
                writer.WriteAttributeString("type", "string");
                writer.WriteString(importItemRequestBody.IdFormat.ToString());
                writer.WriteEndElement();

                writer.WriteStartElement($"{nameof(ImportItemRequestBody.FolderId)}");
                writer.WriteAttributeString("type", "string");
                writer.WriteString(importItemRequestBody.FolderId);
                writer.WriteEndElement();

                writer.WriteStartElement($"{nameof(ImportItemRequestBody.Data)}");
                writer.WriteAttributeString("type", "string");

                // Write Data Stream to HttpContent Stream
                byte[] buffer = new byte[3000];
                base64MailDataStream.Seek(0, SeekOrigin.Begin);

                int numCharsRead;
                while ((numCharsRead = base64MailDataStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    writer.WriteBase64(buffer, 0, numCharsRead);
                }

                writer.WriteEndElement();
                writer.WriteEndElement();

                // Write the XML to file and close the writer.
                writer.Flush();
                writer.Close();

                HttpContent httpOutputContent = null;
                outputHttpContentStream.Seek(0, SeekOrigin.Begin);


                httpOutputContent = new StreamContent(outputHttpContentStream);
                httpOutputContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                using (FileStream file = File.Create("C:\\Users\\goch\\source\\repos\\ConsoleApp1\\testData\\large_items\\output_exported_40mb_item.txt"))
                {
                    await httpOutputContent.CopyToAsync(file).ConfigureAwait(false);
                }
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        private static async Task<Stream> SendMockExportItemRequestFromFileAsync()
        {
            MemoryStream ms = new MemoryStream();
            using (FileStream file = new FileStream("C:\\Users\\goch\\source\\repos\\ConsoleApp1\\testData\\tiny_item.txt", FileMode.Open, FileAccess.Read))
            {
                await file.CopyToAsync(ms).ConfigureAwait(false);
            }

            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }


        private static async Task<Stream> GetLargeExportedItemAsync()
        {
            Stream mbStream = new MultiByteArrayMemoryStream();
            using (FileStream file = new FileStream("C:\\Users\\goch\\source\\repos\\ConsoleApp1\\testData\\large_items\\exported_40mb_item.txt", FileMode.Open, FileAccess.Read))
            {
                await file.CopyToAsync(mbStream).ConfigureAwait(false);
            }

            mbStream.Seek(0, SeekOrigin.Begin);
            return mbStream;
        }

        private static async Task<Stream> SendMockExportItemRequestAsync()
        {
            string mockHttpResponseBody = "{\"@odata.context\": \"https://exhv-3291.exhv-3291dom.extest.microsoft.com/api/beta/$metadata#Microsoft.OutlookServices.ExportItemResponse\",\"ChangeKey\": \"CQAAABYAAACAeZ7BaJY2TJuMhZqWc4p3AAAAABBr\",\"Data\": \"cmVlQ29kZUNhbXAgaXMgYSBwcm92ZW4gcGF0aCB0byB5b3VyIGZpcnN0IHNvZnR3YXJlIGRldmVsb3BlciBqb2IuCgpNb3JlIHRoYW4gNDAsMDAwIHBlb3BsZSBoYXZlIGdvdHRlbiBkZXZlbG9wZXIgam9icyBhZnRlciBjb21wbGV0aW5nIHRoaXMg4oCTIGluY2x1ZGluZyBhdCBiaWcgY29tcGFuaWVzIGxpa2UgR29vZ2xlIGFuZCBNaWNyb3NvZnQuCgpJZiB5b3UgYXJlIG5ldyB0byBwcm9ncmFtbWluZywgd2UgcmVjb21tZW5kIHlvdSBzdGFydCBhdCB0aGUgYmVnaW5uaW5nIGFuZCBlYXJuIHRoZXNlIGNlcnRpZmljYXRpb25zIGluIG9yZGVyLgoKVG8gZWFybiBlYWNoIGNlcnRpZmljYXRpb24sIGJ1aWxkIGl0cyA1IHJlcXVpcmVkIHByb2plY3RzIGFuZCBnZXQgYWxsIHRoZWlyIHRlc3RzIHRvIHBhc3MuCgpZb3UgY2FuIGFkZCB0aGVzZSBjZXJ0aWZpY2F0aW9ucyB0byB5b3VyIHLDqXN1bcOpIG9yIExpbmtlZEluLiBCdXQgbW9yZSBpbXBvcnRhbnQgdGhhbiB0aGUgY2VydGlmaWNhdGlvbnMgaXMgdGhlIHByYWN0aWNlIHlvdSBnZXQgYWxvbmcgdGhlIHdheS4KCklmIHlvdSBmZWVsIG92ZXJ3aGVsbWVkLCB0aGF0IGlzIG5vcm1hbC4gUHJvZ3JhbW1pbmcgaXMgaGFyZC4KClByYWN0aWNlIGlzIHRoZSBrZXkuIFByYWN0aWNlLCBwcmFjdGljZSwgcHJhY3RpY2UuCgpBbmQgdGhpcyBjdXJyaWN1bHVtIHdpbGwgZ2l2ZSB5b3UgdGhvdXNhbmRzIG9mIGhvdXJzIG9mIGhhbmRzLW9uIHByb2dyYW1taW5nIHByYWN0aWNlLgoKQW5kIGlmIHlvdSB3YW50IHRvIGxlYXJuIG1vcmUgbWF0aCBhbmQgY29tcHV0ZXIgc2NpZW5jZSB0aGVvcnksIHdlIGFsc28gaGF2ZSB0aG91c2FuZHMgb2YgaG91cnMgb2YgdmlkZW8gY291cnNlcyBvbiBmcmVlQ29kZUNhbXAncyBZb3VUdWJlIGNoYW5uZWwuCgpJZiB5b3Ugd2FudCB0byBnZXQgYSBkZXZlbG9wZXIgam9iIG9yIGZyZWVsYW5jZSBjbGllbnRzLCBwcm9ncmFtbWluZyBza2lsbHMgd2lsbCBiZSBqdXN0IHBhcnQgb2YgdGhlIHB1enpsZS4gWW91IGFsc28gbmVlZCB0byBidWlsZCB5b3VyIHBlcnNvbmFsIG5ldHdvcmsgYW5kIHlvdXIgcmVwdXRhdGlvbiBhcyBhIGRldmVsb3Blci4KCllvdSBjYW4gZG8gdGhpcyBvbiBUd2l0dGVyIGFuZCBHaXRIdWIsIGFuZCBhbHNvIG9uIHRoZSBmcmVlQ29kZUNhbXAgZm9ydW0uCgpIYXBweSBjb2Rpbmch\"}";
            HttpContent content = new StringContent(mockHttpResponseBody);
            Stream stream = new MultiByteArrayMemoryStream();
            await content.CopyToAsync(stream).ConfigureAwait(false);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        /// <summary>
        /// Read <see cref="BinaryLargeNodeSdsItem"/> from a stream in parts
        /// 1. All fields other than param binaryDataPropertyName should be deserialized into <see cref="BinaryLargeNodeSdsItem"/>
        /// 2. binaryDataPropertyName field should be stream parsed into outputStream
        /// </summary>
        /// <param name="inputStream">input stream</param>
        /// <returns>parsed binary large node sds item without its binary data (see outputStream description)</returns>
        public static Stream ReadDataPropertyFromExportResponseAsStream(
            Stream inputStream)
        {
            // don't dispose this output stream
            // taken care of during conversion of sdsItemWithoutBinaryData to compressed graph
            Stream outputBase64Stream = new MultiByteArrayMemoryStream();

            using (var reader = JsonReaderWriterFactory.CreateJsonReader(inputStream, XmlDictionaryReaderQuotas.Max))
            {
                if (reader == null)
                {
                    throw new ArgumentNullException();
                }

                int d = reader.NodeType == XmlNodeType.None ? -1 : reader.Depth;

                do
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.LocalName.Equals(nameof(ImportItemRequestBody.Data)))
                        {
                            using (var subReader = reader.ReadSubtree())
                            {
                                subReader.MoveToContent();
                                subReader.CopyBase64ElementContents(outputBase64Stream);
                            }

                            reader.Read();
                        }
                    }

                    reader.Read();
                }
                while (!reader.EOF && (d < reader.Depth || (d == reader.Depth && reader.NodeType == XmlNodeType.EndElement)));
            }

            outputBase64Stream.Seek(0, SeekOrigin.Begin);
            return outputBase64Stream;
        }


        /// <summary>
        /// Copy over Base64 encoded element to a stream
        /// </summary>
        /// <param name="reader">xml reader</param>
        /// <returns>if copy was successful</returns>
        public static bool CopyBase64ElementContents(this XmlReader reader, Stream outputBase64Stream)
        {
            // TODO [dikum] : determine size based on perf metrics, make configurable
            var buffer = new byte[100];

            try
            {
                int readBytes;
                while ((readBytes = reader.ReadElementContentAsBase64(buffer, 0, buffer.Length)) > 0)
                {
                    outputBase64Stream.Write(buffer, 0, readBytes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            return true;
        }

        public static string filenamegenerator(string fileName)
        {
            return $"C:\\Users\\goch\\source\\repos\\ConsoleApp1\\{fileName}";
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

