

namespace ConsoleApp1
{
    using Microsoft.ElcArchiveProcessor.Models.SubstrateApiRequestTypes;
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    static class Program
    {
        private static ArrayPool<byte> _pool = ArrayPool<byte>.Shared;

        static async Task Main(string[] args)
        {
           Main3();
            string mockHttpResponseBody = "{\"@odata.context\": \"https://exhv-3291.exhv-3291dom.extest.microsoft.com/api/beta/$metadata#Microsoft.OutlookServices.ExportItemResponse\",\"ChangeKey\": \"CQAAABYAAACAeZ7BaJY2TJuMhZqWc4p3AAAAABBr\",\"Data\": \"VGhlIHRpdHVsYXIgdGhyZWF0IG9mIFRoZSBCbG9iIGhhcyBhbHdheXMgc3RydWNrIG1lIGFzIHRoZSB1bHRpbWF0ZSBtb3ZpZQptb25zdGVyOiBhbiBpbnNhdGlhYmx5IGh1bmdyeSwgYW1vZWJhLWxpa2UgbWFzcyBhYmxlIHRvIHBlbmV0cmF0ZQp2aXJ0dWFsbHkgYW55IHNhZmVndWFyZCwgY2FwYWJsZSBvZi0tYXMgYSBkb29tZWQgZG9jdG9yIGNoaWxsaW5nbHkKZGVzY3JpYmVzIGl0LS0iYXNzaW1pbGF0aW5nIGZsZXNoIG9uIGNvbnRhY3QuClNuaWRlIGNvbXBhcmlzb25zIHRvIGdlbGF0aW4gYmUgZGFtbmVkLCBpdCdzIGEgY29uY2VwdCB3aXRoIHRoZSBtb3N0CmRldmFzdGF0aW5nIG9mIHBvdGVudGlhbCBjb25zZXF1ZW5jZXMsIG5vdCB1bmxpa2UgdGhlIGdyZXkgZ29vIHNjZW5hcmlvCnByb3Bvc2VkIGJ5IHRlY2hub2xvZ2ljYWwgdGhlb3Jpc3RzIGZlYXJmdWwgb2YKYXJ0aWZpY2lhbCBpbnRlbGxpZ2VuY2UgcnVuIHJhbXBhbnQuClRoZSB0aXR1bGFyIHRocmVhdCBvZiBUaGUgQmxvYiBoYXMgYWx3YXlzIHN0cnVjayBtZSBhcyB0aGUgdWx0aW1hdGUgbW92aWUKbW9uc3RlcjogYW4gaW5zYXRpYWJseSBodW5ncnksIGFtb2ViYS1saWtlIG1hc3MgYWJsZSB0byBwZW5ldHJhdGUKdmlydHVhbGx5IGFueSBzYWZlZ3VhcmQsIGNhcGFibGUgb2YtLWFzIGEgZG9vbWVkIGRvY3RvciBjaGlsbGluZ2x5CmRlc2NyaWJlcyBpdC0tImFzc2ltaWxhdGluZyBmbGVzaCBvbiBjb250YWN0LgpTbmlkZSBjb21wYXJpc29ucyB0byBnZWxhdGluIGJlIGRhbW5lZCwgaXQncyBhIGNvbmNlcHQgd2l0aCB0aGUgbW9zdApkZXZhc3RhdGluZyBvZiBwb3RlbnRpYWwgY29uc2VxdWVuY2VzLCBub3QgdW5saWtlIHRoZSBncmV5IGdvbyBzY2VuYXJpbwpwcm9wb3NlZCBieSB0ZWNobm9sb2dpY2FsIHRoZW9yaXN0cyBmZWFyZnVsIG9mCmFydGlmaWNpYWwgaW50ZWxsaWdlbmNlIHJ1biByYW1wYW50LgpUaGUgdGl0dWxhciB0aHJlYXQgb2YgVGhlIEJsb2IgaGFzIGFsd2F5cyBzdHJ1Y2sgbWUgYXMgdGhlIHVsdGltYXRlIG1vdmllCm1vbnN0ZXI6IGFuIGluc2F0aWFibHkgaHVuZ3J5LCBhbW9lYmEtbGlrZSBtYXNzIGFibGUgdG8gcGVuZXRyYXRlCnZpcnR1YWxseSBhbnkgc2FmZWd1YXJkLCBjYXBhYmxlIG9mLS1hcyBhIGRvb21lZCBkb2N0b3IgY2hpbGxpbmdseQpkZXNjcmliZXMgaXQtLSJhc3NpbWlsYXRpbmcgZmxlc2ggb24gY29udGFjdC4KU25pZGUgY29tcGFyaXNvbnMgdG8gZ2VsYXRpbiBiZSBkYW1uZWQsIGl0J3MgYSBjb25jZXB0IHdpdGggdGhlIG1vc3QKZGV2YXN0YXRpbmcgb2YgcG90ZW50aWFsIGNvbnNlcXVlbmNlcywgbm90IHVubGlrZSB0aGUgZ3JleSBnb28gc2NlbmFyaW8KcHJvcG9zZWQgYnkgdGVjaG5vbG9naWNhbCB0aGVvcmlzdHMgZmVhcmZ1bCBvZgphcnRpZmljaWFsIGludGVsbGlnZW5jZSBydW4gcmFtcGFudC4KVGhlIHRpdHVsYXIgdGhyZWF0IG9mIFRoZSBCbG9iIGhhcyBhbHdheXMgc3RydWNrIG1lIGFzIHRoZSB1bHRpbWF0ZSBtb3ZpZQptb25zdGVyOiBhbiBpbnNhdGlhYmx5IGh1bmdyeSwgYW1vZWJhLWxpa2UgbWFzcyBhYmxlIHRvIHBlbmV0cmF0ZQp2aXJ0dWFsbHkgYW55IHNhZmVndWFyZCwgY2FwYWJsZSBvZi0tYXMgYSBkb29tZWQgZG9jdG9yIGNoaWxsaW5nbHkKZGVzY3JpYmVzIGl0LS0iYXNzaW1pbGF0aW5nIGZsZXNoIG9uIGNvbnRhY3QuClNuaWRlIGNvbXBhcmlzb25zIHRvIGdlbGF0aW4gYmUgZGFtbmVkLCBpdCdzIGEgY29uY2VwdCB3aXRoIHRoZSBtb3N0CmRldmFzdGF0aW5nIG9mIHBvdGVudGlhbCBjb25zZXF1ZW5jZXMsIG5vdCB1bmxpa2UgdGhlIGdyZXkgZ29vIHNjZW5hcmlvCnByb3Bvc2VkIGJ5IHRlY2hub2xvZ2ljYWwgdGhlb3Jpc3RzIGZlYXJmdWwgb2YKYXJ0aWZpY2lhbCBpbnRlbGxpZ2VuY2UgcnVuIHJhbXBhbnQuClRoZSB0aXR1bGFyIHRocmVhdCBvZiBUaGUgQmxvYiBoYXMgYWx3YXlzIHN0cnVjayBtZSBhcyB0aGUgdWx0aW1hdGUgbW92aWUKbW9uc3RlcjogYW4gaW5zYXRpYWJseSBodW5ncnksIGFtb2ViYS1saWtlIG1hc3MgYWJsZSB0byBwZW5ldHJhdGUKdmlydHVhbGx5IGFueSBzYWZlZ3VhcmQsIGNhcGFibGUgb2YtLWFzIGEgZG9vbWVkIGRvY3RvciBjaGlsbGluZ2x5CmRlc2NyaWJlcyBpdC0tImFzc2ltaWxhdGluZyBmbGVzaCBvbiBjb250YWN0LgpTbmlkZSBjb21wYXJpc29ucyB0byBnZWxhdGluIGJlIGRhbW5lZCwgaXQncyBhIGNvbmNlcHQgd2l0aCB0aGUgbW9zdApkZXZhc3RhdGluZyBvZiBwb3RlbnRpYWwgY29uc2VxdWVuY2VzLCBub3QgdW5saWtlIHRoZSBncmV5IGdvbyBzY2VuYXJpbwpwcm9wb3NlZCBieSB0ZWNobm9sb2dpY2FsIHRoZW9yaXN0cyBmZWFyZnVsIG9mCmFydGlmaWNpYWwgaW50ZWxsaWdlbmNlIHJ1biByYW1wYW50Lg==\"}";
            // string mockHttpResponseBody = "aGVsbG8=";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(mockHttpResponseBody)
            };

            using Stream jsonResponseStream = new MemoryStream();
            await response.Content.CopyToAsync(jsonResponseStream).ConfigureAwait(false);
            jsonResponseStream.Seek(0, SeekOrigin.Begin);
            ImportItemRequestBody requestBody = ReadBinaryLargeNodeSdsItem(jsonResponseStream, new MemoryStream());

            Stream httpContentStream = new MemoryStream();
            using (var jsonWriter = JsonReaderWriterFactory.CreateJsonWriter(httpContentStream))
            {
                jsonWriter.WriteStartElement("hi");
            }
        }

        /// <summary>
        /// Read <see cref="BinaryLargeNodeSdsItem"/> from a stream in parts
        /// 1. All fields other than param binaryDataPropertyName should be deserialized into <see cref="BinaryLargeNodeSdsItem"/>
        /// 2. binaryDataPropertyName field should be stream parsed into outputStream
        /// </summary>
        /// <param name="inputStream">input stream</param>
        /// <param name="intermediateStream">intermediate stream</param>
        /// <returns>parsed binary large node sds item without its binary data (see outputStream description)</returns>
        public static ImportItemRequestBody ReadBinaryLargeNodeSdsItem(
            Stream inputStream,
            MemoryStream intermediateStream)
        {
            // don't dispose this output stream
            // taken care of during conversion of sdsItemWithoutBinaryData to compressed graph
            MemoryStream outputBase64Stream = new MemoryStream();

            using (var jsonReader = JsonReaderWriterFactory.CreateJsonReader(inputStream, XmlDictionaryReaderQuotas.Max))
            {
                using (var jsonWriter = JsonReaderWriterFactory.CreateJsonWriter(intermediateStream))
                {
                    var capturedProperties = new Dictionary<string, object>();

                    jsonWriter.WriteTransformedNode(
                        jsonReader,
                        r => r.LocalName.Equals(nameof(ImportItemRequestBody.Data)),
                        (r, w) =>
                        {
                            r.MoveToContent();
                            r.CopyBase64ElementContents(r.LocalName, outputBase64Stream);
                        },
                        r => false,
                        new Dictionary<string, Type>(),
                        capturedProperties);

                    outputBase64Stream.Seek(0, SeekOrigin.Begin);
                }
            }

            return new ImportItemRequestBody(IdFormat.RestId, "hi", outputBase64Stream);
        }


        /// <summary>
        /// Copy over Base64 encoded element to a stream
        /// </summary>
        /// <param name="reader">xml reader</param>
        /// <param name="chunkName">chunk Name</param>
        /// <param name="chunkNameBinaryStreamMap">chunkName BinaryStreamMap</param>
        /// <returns>if copy was successful</returns>
        public static bool CopyBase64ElementContents(this XmlReader reader, string chunkName, MemoryStream outputBase64Stream)
        {
            Main3();
            // TODO [dikum] : determine size based on perf metrics, make configurable
            byte[] buffer = new byte[1000];
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


        /// <summary>
        /// Make a DEEP copy of the current xmlreader node to xmlwriter, allowing the caller to transform selected elements.
        /// </summary>
        /// <param name="writer">xml writer</param>
        /// <param name="reader">xml reader</param>
        /// <param name="shouldTransform">should transform the node before writing</param>
        /// <param name="transform">transform action</param>
        /// <param name="shouldCapture">should capture the node value as is</param>
        /// <param name="propertyNameToTypeMap">property name to type map, to help capture typed value</param>
        /// <param name="capturedProperties">captured properties with their typed values</param>
        public static void WriteTransformedNode(
            this XmlWriter writer,
            XmlReader reader,
            Predicate<XmlReader> shouldTransform,
            Action<XmlReader, XmlWriter> transform,
            Predicate<XmlReader> shouldCapture,
            Dictionary<string, Type> propertyNameToTypeMap,
            Dictionary<string, object> capturedProperties)
        {
            if (reader == null || writer == null || shouldTransform == null || transform == null || shouldCapture == null)
            {
                throw new ArgumentNullException();
            }

            int d = reader.NodeType == XmlNodeType.None ? -1 : reader.Depth;

            var captureNode = false;
            var capturedNodeName = string.Empty;
            var capturedNodeValue = string.Empty;
            var capturedNodeType = typeof(string);

            do
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (shouldTransform(reader))
                    {
                        using (var subReader = reader.ReadSubtree())
                        {
                            transform(subReader, writer);
                        }

                        reader.Read();
                    }
                    else if (captureNode || shouldCapture(reader))
                    {
                        capturedNodeName = reader.LocalName;

                        if (propertyNameToTypeMap.TryGetValue(capturedNodeName, out capturedNodeType))
                        {
                            captureNode = true;
                        }

                        writer.WriteShallowNode(reader, ref capturedNodeValue);
                    }
                    else
                    {
                        writer.WriteShallowNode(reader, ref capturedNodeValue);
                    }
                }
                else if (captureNode && reader.NodeType == XmlNodeType.Text)
                {
                    writer.WriteShallowNode(reader, ref capturedNodeValue, captureNode);
                    captureNode = false;

                    if (capturedProperties == null)
                    {
                        capturedProperties = new Dictionary<string, object>();
                    }

                    // TODO [dikum] : add support for more types, if needed
                    if (capturedNodeType == typeof(int))
                    {
                        int.TryParse(capturedNodeValue, out var parsedValue);
                        capturedProperties[capturedNodeName] = parsedValue;
                    }
                    else if (capturedNodeType == typeof(string))
                    {
                        capturedProperties[capturedNodeName] = capturedNodeValue;
                    }
                    else if (capturedNodeType == typeof(DateTimeOffset?))
                    {
                        DateTimeOffset.TryParse(capturedNodeValue, out var parsedValue);
                        capturedProperties[capturedNodeName] = parsedValue;
                    }
                }
                else
                {
                    writer.WriteShallowNode(reader, ref capturedNodeValue);
                }
            }
            while (!reader.EOF && (d < reader.Depth || (d == reader.Depth && reader.NodeType == XmlNodeType.EndElement)));
        }

        private const string m_Document = "sampledata.xml";

        public static void Main3()
        {

            XmlWriter writer = null;

            try
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                writer = XmlWriter.Create(m_Document, settings);

                writer.WriteComment("sample XML fragment");

                // Write an element (this one is the root).
                writer.WriteStartElement("book");

                // Write the namespace declaration.
                writer.WriteAttributeString("xmlns", "bk", null, "urn:samples");

                // Write the genre attribute.
                writer.WriteAttributeString("genre", "novel");

                // Write the title.
                writer.WriteStartElement("title");
                writer.WriteString("The Handmaid's Tale");
                writer.WriteEndElement();

                // Write the price.
                writer.WriteElementString("price", "19.95");

                // Lookup the prefix and write the ISBN element.
                string prefix = writer.LookupPrefix("urn:samples");
                writer.WriteStartElement(prefix, "ISBN", "urn:samples");
                writer.WriteString("1-861003-78");
                writer.WriteEndElement();

                // Write the style element (shows a different way to handle prefixes).
                writer.WriteElementString("style", "urn:samples", "hardcover");

                // Write the close tag for the root element.
                writer.WriteEndElement();

                // Write the XML to file and close the writer.
                writer.Flush();
                writer.Close();
            }

            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

            /// <summary>
            /// Make a SHALLOW copy of the current xmlreader node to xmlwriter, and advance the XML reader past the current node.
            /// </summary>
            /// <param name="writer">xml writer</param>
            /// <param name="reader">xml reader</param>
            /// <param name="capturedValue">captured value</param>
            /// <param name="capture">whether to capture a node's value</param>
            public static void WriteShallowNode(
            this XmlWriter writer,
            XmlReader reader,
            ref string capturedValue,
            bool capture = false)
        {
            // Adapted from https://docs.microsoft.com/en-us/archive/blogs/mfussell/combining-the-xmlreader-and-xmlwriter-classes-for-simple-streaming-transformations
            // By Mark Fussell https://docs.microsoft.com/en-us/archive/blogs/mfussell/
            // and rewritten to avoid using reader.Value, which fully materializes the text value of a node.
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            switch (reader.NodeType)
            {
                case XmlNodeType.None:
                    // This is returned by the System.Xml.XmlReader if a Read method has not been called.
                    reader.Read();
                    break;

                case XmlNodeType.Element:
                    writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                    writer.WriteAttributes(reader, true);

                    if (reader.IsEmptyElement)
                    {
                        writer.WriteEndElement();
                    }

                    reader.Read();
                    break;

                case XmlNodeType.Text:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                case XmlNodeType.CDATA:
                case XmlNodeType.XmlDeclaration:
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.EntityReference:
                case XmlNodeType.DocumentType:
                case XmlNodeType.Comment:

                    // Use reader.Value if node is marked 'capture'
                    if (capture)
                    {
                        capturedValue = reader.Value;
                    }

                    // Avoid using reader.Value as this will fully materialize the string value of the node.  Use WriteNode instead,
                    // it copies text values in chunks.  See: https://referencesource.microsoft.com/#system.xml/System/Xml/Core/XmlWriter.cs,368
                    writer.WriteNode(reader, true);
                    break;

                case XmlNodeType.EndElement:
                    writer.WriteFullEndElement();
                    reader.Read();
                    break;

                default:
                    throw new XmlException(string.Format("Unknown NodeType {0}", reader.NodeType));
            }
        }
    }
}
