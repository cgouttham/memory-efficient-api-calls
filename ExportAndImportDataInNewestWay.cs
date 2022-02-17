
namespace ConsoleApp1
{
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

    public static class ExportAndImportDataNewestWay
    {
        public static async Task ExportAndImportLargeItemsEfficiently()
        {
            // Send Mock HttpRequest that returns a massive Json Payload
            // using Stream httpContentStream = await SendMockExportItemRequestFromFileAsync().ConfigureAwait(false);
            using Stream httpContentStream = GetLargeExportedItemAsync();

            // Get Data Property from Stream and store it in another stream.
            await ReadDataPropertyFromExportResponseAndWriteToJsonStream(httpContentStream).ConfigureAwait(false);
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


        private static Stream GetLargeExportedItemAsync()
        {
            FileStream file = new FileStream("C:\\Users\\goch\\source\\repos\\ConsoleApp1\\testData\\large_items\\exported_40mb_item.txt", FileMode.Open, FileAccess.Read);
            return file;
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
        private static async Task ReadDataPropertyFromExportResponseAndWriteToJsonStream(Stream inputStream)
        {
            // don't dispose this output stream
            // taken care of during conversion of sdsItemWithoutBinaryData to compressed graph

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
                                await subReader.CopyBase64ElementContents().ConfigureAwait(false);
                            }

                            reader.Read();
                        }
                    }

                    reader.Read();
                }
                while (!reader.EOF && (d < reader.Depth || (d == reader.Depth && reader.NodeType == XmlNodeType.EndElement)));
            }
        }


        /// <summary>
        /// Copy over Base64 encoded element to a stream
        /// </summary>
        /// <param name="reader">xml reader</param>
        /// <returns>if copy was successful</returns>
        private static async Task CopyBase64ElementContents(this XmlReader reader)
        {
            Stream outputBase64Stream = new MultiByteArrayMemoryStream();

            XmlDictionaryWriter writer = null;

            // Write Data Property into Import Item Request Body (HttpContent)
            var importItemRequestBody = new ImportItemRequestBody(IdFormat.RestId, "Mock_FolderId");

            writer = JsonReaderWriterFactory.CreateJsonWriter(outputBase64Stream);

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

            writer.WriteNode(reader, false);

            writer.WriteEndElement();

            // Write the XML to file and close the writer.
            writer.Flush();
            writer.Close();

            HttpContent httpOutputContent = null;
            outputBase64Stream.Seek(0, SeekOrigin.Begin);


            httpOutputContent = new StreamContent(outputBase64Stream);
            httpOutputContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using (FileStream file = File.Create("C:\\Users\\goch\\source\\repos\\ConsoleApp1\\testData\\large_items\\newest_output_exported_40mb_item_2.txt"))
            {
                await httpOutputContent.CopyToAsync(file).ConfigureAwait(false);
            }

            outputBase64Stream.Flush();
            outputBase64Stream.Close();
        }

        public static string filenamegenerator(string fileName)
        {
            return $"C:\\Users\\goch\\source\\repos\\ConsoleApp1\\{fileName}";
        }
    }
}
