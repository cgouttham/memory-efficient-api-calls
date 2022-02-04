namespace ConsoleApp1
{
    using Microsoft.ElcArchiveProcessor.Models.SubstrateApiRequestTypes;
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
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
            // Send Mock HttpRequest that returns a massive Json Payload
            HttpContent httpContent = SendMockExportItemRequest();

            // Read HttpContent as a stream.
            using Stream jsonResponseStream = await httpContent.ReadAsStreamAsync().ConfigureAwait(false);
            jsonResponseStream.Seek(0, SeekOrigin.Begin);

            Stream base64MailDataStream = ReadDataPropertyFromExportResponseAsStream(jsonResponseStream);

            XmlDictionaryWriter writer = null;
            try
            {
                MemoryStream ms = new MemoryStream();

                writer = JsonReaderWriterFactory.CreateJsonWriter(ms);

                var importItemRequestBody = new ImportItemRequestBody(IdFormat.RestId, "Mock_FolderId");

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

                // Write to Stream
                byte[] buffer = new byte[3000];
                base64MailDataStream.Seek(0, SeekOrigin.Begin);
                var base64StreamReader = new XmlTextReader(base64MailDataStream);

                int numCharsRead;
                base64MailDataStream.Seek(0, SeekOrigin.Begin);
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
                ms.Seek(0, SeekOrigin.Begin);
                httpOutputContent = new StreamContent(ms);
                httpOutputContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                string s = await httpOutputContent.ReadAsStringAsync().ConfigureAwait(false);
            }
             finally
             {
                 if (writer != null)
                     writer.Close();
             }
        }

        private static HttpContent SendMockExportItemRequest()
        {
            string mockHttpResponseBody = "{\"@odata.context\": \"https://exhv-3291.exhv-3291dom.extest.microsoft.com/api/beta/$metadata#Microsoft.OutlookServices.ExportItemResponse\",\"ChangeKey\": \"CQAAABYAAACAeZ7BaJY2TJuMhZqWc4p3AAAAABBr\",\"Data\": \"cmVlQ29kZUNhbXAgaXMgYSBwcm92ZW4gcGF0aCB0byB5b3VyIGZpcnN0IHNvZnR3YXJlIGRldmVsb3BlciBqb2IuCgpNb3JlIHRoYW4gNDAsMDAwIHBlb3BsZSBoYXZlIGdvdHRlbiBkZXZlbG9wZXIgam9icyBhZnRlciBjb21wbGV0aW5nIHRoaXMg4oCTIGluY2x1ZGluZyBhdCBiaWcgY29tcGFuaWVzIGxpa2UgR29vZ2xlIGFuZCBNaWNyb3NvZnQuCgpJZiB5b3UgYXJlIG5ldyB0byBwcm9ncmFtbWluZywgd2UgcmVjb21tZW5kIHlvdSBzdGFydCBhdCB0aGUgYmVnaW5uaW5nIGFuZCBlYXJuIHRoZXNlIGNlcnRpZmljYXRpb25zIGluIG9yZGVyLgoKVG8gZWFybiBlYWNoIGNlcnRpZmljYXRpb24sIGJ1aWxkIGl0cyA1IHJlcXVpcmVkIHByb2plY3RzIGFuZCBnZXQgYWxsIHRoZWlyIHRlc3RzIHRvIHBhc3MuCgpZb3UgY2FuIGFkZCB0aGVzZSBjZXJ0aWZpY2F0aW9ucyB0byB5b3VyIHLDqXN1bcOpIG9yIExpbmtlZEluLiBCdXQgbW9yZSBpbXBvcnRhbnQgdGhhbiB0aGUgY2VydGlmaWNhdGlvbnMgaXMgdGhlIHByYWN0aWNlIHlvdSBnZXQgYWxvbmcgdGhlIHdheS4KCklmIHlvdSBmZWVsIG92ZXJ3aGVsbWVkLCB0aGF0IGlzIG5vcm1hbC4gUHJvZ3JhbW1pbmcgaXMgaGFyZC4KClByYWN0aWNlIGlzIHRoZSBrZXkuIFByYWN0aWNlLCBwcmFjdGljZSwgcHJhY3RpY2UuCgpBbmQgdGhpcyBjdXJyaWN1bHVtIHdpbGwgZ2l2ZSB5b3UgdGhvdXNhbmRzIG9mIGhvdXJzIG9mIGhhbmRzLW9uIHByb2dyYW1taW5nIHByYWN0aWNlLgoKQW5kIGlmIHlvdSB3YW50IHRvIGxlYXJuIG1vcmUgbWF0aCBhbmQgY29tcHV0ZXIgc2NpZW5jZSB0aGVvcnksIHdlIGFsc28gaGF2ZSB0aG91c2FuZHMgb2YgaG91cnMgb2YgdmlkZW8gY291cnNlcyBvbiBmcmVlQ29kZUNhbXAncyBZb3VUdWJlIGNoYW5uZWwuCgpJZiB5b3Ugd2FudCB0byBnZXQgYSBkZXZlbG9wZXIgam9iIG9yIGZyZWVsYW5jZSBjbGllbnRzLCBwcm9ncmFtbWluZyBza2lsbHMgd2lsbCBiZSBqdXN0IHBhcnQgb2YgdGhlIHB1enpsZS4gWW91IGFsc28gbmVlZCB0byBidWlsZCB5b3VyIHBlcnNvbmFsIG5ldHdvcmsgYW5kIHlvdXIgcmVwdXRhdGlvbiBhcyBhIGRldmVsb3Blci4KCllvdSBjYW4gZG8gdGhpcyBvbiBUd2l0dGVyIGFuZCBHaXRIdWIsIGFuZCBhbHNvIG9uIHRoZSBmcmVlQ29kZUNhbXAgZm9ydW0uCgpIYXBweSBjb2Rpbmch\"}";
            return new StringContent(mockHttpResponseBody);
        }

        public static Stream ConvertToBase64(this Stream stream)
        {
            byte[] bytes;
            stream.Seek(0, SeekOrigin.Begin);
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }

            string base64 = Convert.ToBase64String(bytes);
            return new MemoryStream(Encoding.UTF8.GetBytes(base64));
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
            MemoryStream outputBase64Stream = new MemoryStream();

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

               outputBase64Stream.Seek(0, SeekOrigin.Begin);
            }

            return outputBase64Stream;
        }


        /// <summary>
        /// Copy over Base64 encoded element to a stream
        /// </summary>
        /// <param name="reader">xml reader</param>
        /// <returns>if copy was successful</returns>
        public static bool CopyBase64ElementContents(this XmlReader reader, MemoryStream outputBase64Stream)
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

    }
}

