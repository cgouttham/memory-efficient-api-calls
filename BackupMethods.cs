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

    class BackupMethods
    {
        /// <summary>
        /// Read <see cref="BinaryLargeNodeSdsItem"/> from a stream in parts
        /// 1. All fields other than param binaryDataPropertyName should be deserialized into <see cref="BinaryLargeNodeSdsItem"/>
        /// 2. binaryDataPropertyName field should be stream parsed into outputStream
        /// </summary>
        /// <param name="inputStream">input stream</param>
        /// <param name="intermediateStream">intermediate stream</param>
        /// <returns>parsed binary large node sds item without its binary data (see outputStream description)</returns>
        public static async Task<Stream> StepThroughJsonUsingReader()
        {
            string mockHttpResponseBody = "{\"@odata.context\": \"https://exhv-3291.exhv-3291dom.extest.microsoft.com/api/beta/$metadata#Microsoft.OutlookServices.ExportItemResponse\",\"ChangeKey\": \"CQAAABYAAACAeZ7BaJY2TJuMhZqWc4p3AAAAABBr\",\"Data\": \"VGhlIHRpdHVsYXIgdGhyZWF0IG9mIFRoZSBCbG9iIGhhcyBhbHdheXMgc3RydWNrIG1lIGFzIHRoZSB1bHRpbWF0ZSBtb3ZpZQptb25zdGVyOiBhbiBpbnNhdGlhYmx5IGh1bmdyeSwgYW1vZWJhLWxpa2UgbWFzcyBhYmxlIHRvIHBlbmV0cmF0ZQp2aXJ0dWFsbHkgYW55IHNhZmVndWFyZCwgY2FwYWJsZSBvZi0tYXMgYSBkb29tZWQgZG9jdG9yIGNoaWxsaW5nbHkKZGVzY3JpYmVzIGl0LS0iYXNzaW1pbGF0aW5nIGZsZXNoIG9uIGNvbnRhY3QuClNuaWRlIGNvbXBhcmlzb25zIHRvIGdlbGF0aW4gYmUgZGFtbmVkLCBpdCdzIGEgY29uY2VwdCB3aXRoIHRoZSBtb3N0CmRldmFzdGF0aW5nIG9mIHBvdGVudGlhbCBjb25zZXF1ZW5jZXMsIG5vdCB1bmxpa2UgdGhlIGdyZXkgZ29vIHNjZW5hcmlvCnByb3Bvc2VkIGJ5IHRlY2hub2xvZ2ljYWwgdGhlb3Jpc3RzIGZlYXJmdWwgb2YKYXJ0aWZpY2lhbCBpbnRlbGxpZ2VuY2UgcnVuIHJhbXBhbnQuClRoZSB0aXR1bGFyIHRocmVhdCBvZiBUaGUgQmxvYiBoYXMgYWx3YXlzIHN0cnVjayBtZSBhcyB0aGUgdWx0aW1hdGUgbW92aWUKbW9uc3RlcjogYW4gaW5zYXRpYWJseSBodW5ncnksIGFtb2ViYS1saWtlIG1hc3MgYWJsZSB0byBwZW5ldHJhdGUKdmlydHVhbGx5IGFueSBzYWZlZ3VhcmQsIGNhcGFibGUgb2YtLWFzIGEgZG9vbWVkIGRvY3RvciBjaGlsbGluZ2x5CmRlc2NyaWJlcyBpdC0tImFzc2ltaWxhdGluZyBmbGVzaCBvbiBjb250YWN0LgpTbmlkZSBjb21wYXJpc29ucyB0byBnZWxhdGluIGJlIGRhbW5lZCwgaXQncyBhIGNvbmNlcHQgd2l0aCB0aGUgbW9zdApkZXZhc3RhdGluZyBvZiBwb3RlbnRpYWwgY29uc2VxdWVuY2VzLCBub3QgdW5saWtlIHRoZSBncmV5IGdvbyBzY2VuYXJpbwpwcm9wb3NlZCBieSB0ZWNobm9sb2dpY2FsIHRoZW9yaXN0cyBmZWFyZnVsIG9mCmFydGlmaWNpYWwgaW50ZWxsaWdlbmNlIHJ1biByYW1wYW50LgpUaGUgdGl0dWxhciB0aHJlYXQgb2YgVGhlIEJsb2IgaGFzIGFsd2F5cyBzdHJ1Y2sgbWUgYXMgdGhlIHVsdGltYXRlIG1vdmllCm1vbnN0ZXI6IGFuIGluc2F0aWFibHkgaHVuZ3J5LCBhbW9lYmEtbGlrZSBtYXNzIGFibGUgdG8gcGVuZXRyYXRlCnZpcnR1YWxseSBhbnkgc2FmZWd1YXJkLCBjYXBhYmxlIG9mLS1hcyBhIGRvb21lZCBkb2N0b3IgY2hpbGxpbmdseQpkZXNjcmliZXMgaXQtLSJhc3NpbWlsYXRpbmcgZmxlc2ggb24gY29udGFjdC4KU25pZGUgY29tcGFyaXNvbnMgdG8gZ2VsYXRpbiBiZSBkYW1uZWQsIGl0J3MgYSBjb25jZXB0IHdpdGggdGhlIG1vc3QKZGV2YXN0YXRpbmcgb2YgcG90ZW50aWFsIGNvbnNlcXVlbmNlcywgbm90IHVubGlrZSB0aGUgZ3JleSBnb28gc2NlbmFyaW8KcHJvcG9zZWQgYnkgdGVjaG5vbG9naWNhbCB0aGVvcmlzdHMgZmVhcmZ1bCBvZgphcnRpZmljaWFsIGludGVsbGlnZW5jZSBydW4gcmFtcGFudC4KVGhlIHRpdHVsYXIgdGhyZWF0IG9mIFRoZSBCbG9iIGhhcyBhbHdheXMgc3RydWNrIG1lIGFzIHRoZSB1bHRpbWF0ZSBtb3ZpZQptb25zdGVyOiBhbiBpbnNhdGlhYmx5IGh1bmdyeSwgYW1vZWJhLWxpa2UgbWFzcyBhYmxlIHRvIHBlbmV0cmF0ZQp2aXJ0dWFsbHkgYW55IHNhZmVndWFyZCwgY2FwYWJsZSBvZi0tYXMgYSBkb29tZWQgZG9jdG9yIGNoaWxsaW5nbHkKZGVzY3JpYmVzIGl0LS0iYXNzaW1pbGF0aW5nIGZsZXNoIG9uIGNvbnRhY3QuClNuaWRlIGNvbXBhcmlzb25zIHRvIGdlbGF0aW4gYmUgZGFtbmVkLCBpdCdzIGEgY29uY2VwdCB3aXRoIHRoZSBtb3N0CmRldmFzdGF0aW5nIG9mIHBvdGVudGlhbCBjb25zZXF1ZW5jZXMsIG5vdCB1bmxpa2UgdGhlIGdyZXkgZ29vIHNjZW5hcmlvCnByb3Bvc2VkIGJ5IHRlY2hub2xvZ2ljYWwgdGhlb3Jpc3RzIGZlYXJmdWwgb2YKYXJ0aWZpY2lhbCBpbnRlbGxpZ2VuY2UgcnVuIHJhbXBhbnQuClRoZSB0aXR1bGFyIHRocmVhdCBvZiBUaGUgQmxvYiBoYXMgYWx3YXlzIHN0cnVjayBtZSBhcyB0aGUgdWx0aW1hdGUgbW92aWUKbW9uc3RlcjogYW4gaW5zYXRpYWJseSBodW5ncnksIGFtb2ViYS1saWtlIG1hc3MgYWJsZSB0byBwZW5ldHJhdGUKdmlydHVhbGx5IGFueSBzYWZlZ3VhcmQsIGNhcGFibGUgb2YtLWFzIGEgZG9vbWVkIGRvY3RvciBjaGlsbGluZ2x5CmRlc2NyaWJlcyBpdC0tImFzc2ltaWxhdGluZyBmbGVzaCBvbiBjb250YWN0LgpTbmlkZSBjb21wYXJpc29ucyB0byBnZWxhdGluIGJlIGRhbW5lZCwgaXQncyBhIGNvbmNlcHQgd2l0aCB0aGUgbW9zdApkZXZhc3RhdGluZyBvZiBwb3RlbnRpYWwgY29uc2VxdWVuY2VzLCBub3QgdW5saWtlIHRoZSBncmV5IGdvbyBzY2VuYXJpbwpwcm9wb3NlZCBieSB0ZWNobm9sb2dpY2FsIHRoZW9yaXN0cyBmZWFyZnVsIG9mCmFydGlmaWNpYWwgaW50ZWxsaWdlbmNlIHJ1biByYW1wYW50Lg==\"}";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(mockHttpResponseBody)
            };

            using Stream jsonResponseStream = new MemoryStream();
            await response.Content.CopyToAsync(jsonResponseStream).ConfigureAwait(false);
            jsonResponseStream.Seek(0, SeekOrigin.Begin);

            // don't dispose this output stream
            // taken care of during conversion of sdsItemWithoutBinaryData to compressed graph
            MemoryStream outputBase64Stream = new MemoryStream();


            using (var reader = JsonReaderWriterFactory.CreateJsonReader(jsonResponseStream, XmlDictionaryReaderQuotas.Max))
            {
                reader.Read(); // "Element, Name=\"root\""
                reader.MoveToAttribute(0); // "Attribute, Name=\"type\", Value=\"object\""
                reader.Read(); // "Element, Name=\"a:item\""
                reader.MoveToAttribute(0); // "Attribute, Name=\"xmlns:a\", Value=\"item\""
                reader.MoveToAttribute(1); // "Attribute, Name=\"item\", Value=\"@odata.context\""
                reader.MoveToAttribute(2); // "Attribute, Name=\"type\", Value=\"string\""
                reader.Read(); // "Text, Value=\"https://exhv-3291.exhv-3291dom.extest.microsoft.com/api/beta/$metadata#Microsoft.OutlookServices.ExportItemResponse\""
                reader.Read(); // "EndElement, Name=\"a:item\""
                reader.Read(); // "Element, Name=\"ChangeKey\""
                reader.MoveToAttribute(0); // "Attribute, Name=\"type\", Value=\"string\""
                reader.Read(); // "Text, Value=\"CQAAABYAAACAeZ7BaJY2TJuMhZqWc4p3AAAAABBr\""
                reader.Read(); // "EndElement, Name=\"ChangeKey\""
                reader.Read(); // "Element, Name=\"Data\""
                reader.MoveToAttribute(0); // "Attribute, Name=\"type\", Value=\"string\""
                reader.Read(); // "Text, Value=\"VGhlIHRpdHVsYXIgdGhyZWF0IG9mIFRoZSBCbG9iIGhhcyBhbHdheXMgc3RydWNrIG1lIGFzIHRoZSB1bHRpbWF0ZSBtb3ZpZQptb25zdGVyOiBhbiBpbnNhdGlhYmx5IGh1bmdyeSwgYW1vZWJhLWxpa2UgbWFzcyBhYmxlIHRvIHBlbmV0cmF0ZQp2aXJ0dWFsbHkgYW55IHNhZmVndWFyZCwgY2FwYWJsZSBvZi0tYXMgYSBkb29tZWQgZG9jdG9yIGNoaWxsaW5nbHkKZGVzY3JpYmVzIGl0LS0iYXNzaW1pbGF0aW5nIGZsZXNoIG9uIGNvbnRhY3QuClNuaWRlIGNvbXBhcmlzb25zIHRvIGdlbGF0aW4gYmUgZGFtbmVkLCBpdCdzIGEgY29uY2VwdCB3aXRoIHRoZSBtb3N0CmRldmFzdGF0aW5nIG9mIHBvdGVudGlhbCBjb25zZXF1ZW5jZXMsIG5vdCB1bmxpa2UgdGhlIGdyZXkgZ29vIHNjZW5hcmlvCnByb3Bvc2VkIGJ5IHRlY2hub2xvZ2ljYWwgdGhlb3Jpc3RzIGZlYXJmdWwgb2YKYXJ0aWZpY2lhbCBpbnRlbGxpZ2VuY2UgcnVuIHJhbXBhbnQuClRoZSB0aXR1bGFyIHRocmVhdCBvZiBUaGUgQmxvYiBoYXMgYWx3YXlzIHN0cnVjayBtZSBhcyB0aGUgdWx0aW1hdGUgbW92aWUKbW9uc3RlcjogYW4gaW5zYXRpYWJseSBodW5ncnksIGFtb2ViYS1saWtlIG1hc3MgYWJsZSB0byBwZW5ldHJhdGUKdmlydHVhbGx5IGFueSBzYWZlZ3VhcmQsIGNhcGFibGUgb2YtLWFzIGEgZG9vbWVkIGRvY3RvciBjaGlsbGluZ2x5CmRlc2NyaWJlcyBpdC0tImFzc2ltaWxhdGluZyBmbGVzaCBvbiBjb250YWN0LgpTbmlkZSBjb21wYXJpc29ucyB0byBnZWxhdGluIGJlIGRhbW5lZCwgaXQncyBhIGNvbmNlcHQgd2l0aCB0aGUgbW9zdApkZXZhc3RhdGluZyBvZiBwb3RlbnRpYWwgY29uc2VxdWVuY2VzLCBub3QgdW5saWtlIHRoZSBncmV5IGdvbyBzY2VuYXJpbwpwcm9wb3NlZCBieSB0ZWNobm9sb2dpY2FsIHRoZW9yaXN0cyBmZWFyZnVsIG9mCmFydGlmaWNpYWwgaW50ZWxsaWdlbmNlIHJ1biByYW1wYW50LgpUaGUgdGl0dWxhciB0aHJlYXQgb2YgVGhlIEJsb2IgaGFzIGFsd2F5cyBzdHJ1Y2sgbWUgYXMgdGhlIHVsdGltYXRlIG1vdmllCm1vbnN0ZXI6IGFuIGluc2F0aWFibHkgaHVuZ3J5LCBhbW9lYmEtbGlrZSBtYXNzIGFibGUgdG8gcGVuZXRyYXRlCnZpcnR1YWxseSBhbnkgc2FmZWd1YXJkLCBjYXBhYmxlIG9mLS1hcyBhIGRvb21lZCBkb2N0b3IgY2hpbGxpbmdseQpkZXNjcmliZXMgaXQtLSJhc3NpbWlsYXRpbmcgZmxlc2ggb24gY29udGFjdC4KU25pZGUgY29tcGFyaXNvbnMgdG8gZ2VsYXRpbiBiZSBkYW1uZWQsIGl0J3MgYSBjb25jZXB0IHdpdGggdGhlIG1vc3QKZGV2YXN0YXRpbmcgb2YgcG90ZW50aWFsIGNvbnNlcXVlbmNlcywgbm90IHVubGlrZSB0aGUgZ3JleSBnb28gc2NlbmFyaW8KcHJvcG9zZWQgYnkgdGVjaG5vbG9naWNhbCB0aGVvcmlzdHMgZmVhcmZ1bCBvZgphcnRpZmljaWFsIGludGVsbGlnZW5jZSBydW4gcmFtcGFudC4KVGhlIHRpdHVsYXIgdGhyZWF0IG9mIFRoZSBCbG9iIGhhcyBhbHdheXMgc3RydWNrIG1lIGFzIHRoZSB1bHRpbWF0ZSBtb3ZpZQptb25zdGVyOiBhbiBpbnNhdGlhYmx5IGh1bmdy\""
                reader.Read(); // "Text, Value=\"eSwgYW1vZWJhLWxpa2UgbWFzcyBhYmxlIHRvIHBlbmV0cmF0ZQp2aXJ0dWFsbHkgYW55IHNhZmVndWFyZCwgY2FwYWJsZSBvZi0tYXMgYSBkb29tZWQgZG9jdG9yIGNoaWxsaW5nbHkKZGVzY3JpYmVzIGl0LS0iYXNzaW1pbGF0aW5nIGZsZXNoIG9uIGNvbnRhY3QuClNuaWRlIGNvbXBhcmlzb25zIHRvIGdlbGF0aW4gYmUgZGFtbmVkLCBpdCdzIGEgY29uY2VwdCB3aXRoIHRoZSBtb3N0CmRldmFzdGF0aW5nIG9mIHBvdGVudGlhbCBjb25zZXF1ZW5jZXMsIG5vdCB1bmxpa2UgdGhlIGdyZXkgZ29vIHNjZW5hcmlvCnByb3Bvc2VkIGJ5IHRlY2hub2xvZ2ljYWwgdGhlb3Jpc3RzIGZlYXJmdWwgb2YKYXJ0aWZpY2lhbCBpbnRlbGxpZ2VuY2UgcnVuIHJhbXBhbnQuClRoZSB0aXR1bGFyIHRocmVhdCBvZiBUaGUgQmxvYiBoYXMgYWx3YXlzIHN0cnVjayBtZSBhcyB0aGUgdWx0aW1hdGUgbW92aWUKbW9uc3RlcjogYW4gaW5zYXRpYWJseSBodW5ncnksIGFtb2ViYS1saWtlIG1hc3MgYWJsZSB0byBwZW5ldHJhdGUKdmlydHVhbGx5IGFueSBzYWZlZ3VhcmQsIGNhcGFibGUgb2YtLWFzIGEgZG9vbWVkIGRvY3RvciBjaGlsbGluZ2x5CmRlc2NyaWJlcyBpdC0tImFzc2ltaWxhdGluZyBmbGVzaCBvbiBjb250YWN0LgpTbmlkZSBjb21wYXJpc29ucyB0byBnZWxhdGluIGJlIGRhbW5lZCwgaXQncyBhIGNvbmNlcHQgd2l0aCB0aGUgbW9zdApkZXZhc3RhdGluZyBvZiBwb3RlbnRpYWwgY29uc2VxdWVuY2VzLCBub3QgdW5saWtlIHRoZSBncmV5IGdvbyBzY2VuYXJpbwpwcm9wb3NlZCBieSB0ZWNobm9sb2dpY2FsIHRoZW9yaXN0cyBmZWFyZnVsIG9mCmFydGlmaWNpYWwgaW50ZWxsaWdlbmNlIHJ1biByYW1wYW50Lg==\""
                reader.Read(); // "EndElement, Name=\"Data\""
                reader.Read(); // "EndElement, Name=\"root\""
            }

            return outputBase64Stream;
        }


        /*
        reader.Read(); // "Element, Name=\"root\""
        reader.MoveToAttribute(0); // "Attribute, Name=\"type\", Value=\"object\""
        reader.Read(); // "Element, Name=\"a:item\""
        reader.MoveToAttribute(0); // "Attribute, Name=\"xmlns:a\", Value=\"item\""
        reader.MoveToAttribute(1); // "Attribute, Name=\"item\", Value=\"@odata.context\""
        reader.MoveToAttribute(2); // "Attribute, Name=\"type\", Value=\"string\""
        reader.Read(); // "Text, Value=\"https://exhv-3291.exhv-3291dom.extest.microsoft.com/api/beta/$metadata#Microsoft.OutlookServices.ExportItemResponse\""
        reader.Read(); // "EndElement, Name=\"a:item\""
        reader.Read(); // "Element, Name=\"ChangeKey\""
        reader.MoveToAttribute(0); // "Attribute, Name=\"type\", Value=\"string\""
        reader.Read(); // "Text, Value=\"CQAAABYAAACAeZ7BaJY2TJuMhZqWc4p3AAAAABBr\""
        reader.Read(); // "EndElement, Name=\"ChangeKey\""
        reader.Read(); // "Element, Name=\"Data\""
        reader.MoveToAttribute(0); // "Attribute, Name=\"type\", Value=\"string\""
        reader.Read(); // "Text, Value=\"VGhlIHRpdHVsYXIgdGhyZWF0IG9mIFRoZSBCbG9iIGhhcyBhbHdheXMgc3RydWNrIG1lIGFzIHRoZSB1bHRpbWF0ZSBtb3ZpZQptb25zdGVyOiBhbiBpbnNhdGlhYmx5IGh1bmdyeSwgYW1vZWJhLWxpa2UgbWFzcyBhYmxlIHRvIHBlbmV0cmF0ZQp2aXJ0dWFsbHkgYW55IHNhZmVndWFyZCwgY2FwYWJsZSBvZi0tYXMgYSBkb29tZWQgZG9jdG9yIGNoaWxsaW5nbHkKZGVzY3JpYmVzIGl0LS0iYXNzaW1pbGF0aW5nIGZsZXNoIG9uIGNvbnRhY3QuClNuaWRlIGNvbXBhcmlzb25zIHRvIGdlbGF0aW4gYmUgZGFtbmVkLCBpdCdzIGEgY29uY2VwdCB3aXRoIHRoZSBtb3N0CmRldmFzdGF0aW5nIG9mIHBvdGVudGlhbCBjb25zZXF1ZW5jZXMsIG5vdCB1bmxpa2UgdGhlIGdyZXkgZ29vIHNjZW5hcmlvCnByb3Bvc2VkIGJ5IHRlY2hub2xvZ2ljYWwgdGhlb3Jpc3RzIGZlYXJmdWwgb2YKYXJ0aWZpY2lhbCBpbnRlbGxpZ2VuY2UgcnVuIHJhbXBhbnQuClRoZSB0aXR1bGFyIHRocmVhdCBvZiBUaGUgQmxvYiBoYXMgYWx3YXlzIHN0cnVjayBtZSBhcyB0aGUgdWx0aW1hdGUgbW92aWUKbW9uc3RlcjogYW4gaW5zYXRpYWJseSBodW5ncnksIGFtb2ViYS1saWtlIG1hc3MgYWJsZSB0byBwZW5ldHJhdGUKdmlydHVhbGx5IGFueSBzYWZlZ3VhcmQsIGNhcGFibGUgb2YtLWFzIGEgZG9vbWVkIGRvY3RvciBjaGlsbGluZ2x5CmRlc2NyaWJlcyBpdC0tImFzc2ltaWxhdGluZyBmbGVzaCBvbiBjb250YWN0LgpTbmlkZSBjb21wYXJpc29ucyB0byBnZWxhdGluIGJlIGRhbW5lZCwgaXQncyBhIGNvbmNlcHQgd2l0aCB0aGUgbW9zdApkZXZhc3RhdGluZyBvZiBwb3RlbnRpYWwgY29uc2VxdWVuY2VzLCBub3QgdW5saWtlIHRoZSBncmV5IGdvbyBzY2VuYXJpbwpwcm9wb3NlZCBieSB0ZWNobm9sb2dpY2FsIHRoZW9yaXN0cyBmZWFyZnVsIG9mCmFydGlmaWNpYWwgaW50ZWxsaWdlbmNlIHJ1biByYW1wYW50LgpUaGUgdGl0dWxhciB0aHJlYXQgb2YgVGhlIEJsb2IgaGFzIGFsd2F5cyBzdHJ1Y2sgbWUgYXMgdGhlIHVsdGltYXRlIG1vdmllCm1vbnN0ZXI6IGFuIGluc2F0aWFibHkgaHVuZ3J5LCBhbW9lYmEtbGlrZSBtYXNzIGFibGUgdG8gcGVuZXRyYXRlCnZpcnR1YWxseSBhbnkgc2FmZWd1YXJkLCBjYXBhYmxlIG9mLS1hcyBhIGRvb21lZCBkb2N0b3IgY2hpbGxpbmdseQpkZXNjcmliZXMgaXQtLSJhc3NpbWlsYXRpbmcgZmxlc2ggb24gY29udGFjdC4KU25pZGUgY29tcGFyaXNvbnMgdG8gZ2VsYXRpbiBiZSBkYW1uZWQsIGl0J3MgYSBjb25jZXB0IHdpdGggdGhlIG1vc3QKZGV2YXN0YXRpbmcgb2YgcG90ZW50aWFsIGNvbnNlcXVlbmNlcywgbm90IHVubGlrZSB0aGUgZ3JleSBnb28gc2NlbmFyaW8KcHJvcG9zZWQgYnkgdGVjaG5vbG9naWNhbCB0aGVvcmlzdHMgZmVhcmZ1bCBvZgphcnRpZmljaWFsIGludGVsbGlnZW5jZSBydW4gcmFtcGFudC4KVGhlIHRpdHVsYXIgdGhyZWF0IG9mIFRoZSBCbG9iIGhhcyBhbHdheXMgc3RydWNrIG1lIGFzIHRoZSB1bHRpbWF0ZSBtb3ZpZQptb25zdGVyOiBhbiBpbnNhdGlhYmx5IGh1bmdy\""
        reader.Read(); // "Text, Value=\"eSwgYW1vZWJhLWxpa2UgbWFzcyBhYmxlIHRvIHBlbmV0cmF0ZQp2aXJ0dWFsbHkgYW55IHNhZmVndWFyZCwgY2FwYWJsZSBvZi0tYXMgYSBkb29tZWQgZG9jdG9yIGNoaWxsaW5nbHkKZGVzY3JpYmVzIGl0LS0iYXNzaW1pbGF0aW5nIGZsZXNoIG9uIGNvbnRhY3QuClNuaWRlIGNvbXBhcmlzb25zIHRvIGdlbGF0aW4gYmUgZGFtbmVkLCBpdCdzIGEgY29uY2VwdCB3aXRoIHRoZSBtb3N0CmRldmFzdGF0aW5nIG9mIHBvdGVudGlhbCBjb25zZXF1ZW5jZXMsIG5vdCB1bmxpa2UgdGhlIGdyZXkgZ29vIHNjZW5hcmlvCnByb3Bvc2VkIGJ5IHRlY2hub2xvZ2ljYWwgdGhlb3Jpc3RzIGZlYXJmdWwgb2YKYXJ0aWZpY2lhbCBpbnRlbGxpZ2VuY2UgcnVuIHJhbXBhbnQuClRoZSB0aXR1bGFyIHRocmVhdCBvZiBUaGUgQmxvYiBoYXMgYWx3YXlzIHN0cnVjayBtZSBhcyB0aGUgdWx0aW1hdGUgbW92aWUKbW9uc3RlcjogYW4gaW5zYXRpYWJseSBodW5ncnksIGFtb2ViYS1saWtlIG1hc3MgYWJsZSB0byBwZW5ldHJhdGUKdmlydHVhbGx5IGFueSBzYWZlZ3VhcmQsIGNhcGFibGUgb2YtLWFzIGEgZG9vbWVkIGRvY3RvciBjaGlsbGluZ2x5CmRlc2NyaWJlcyBpdC0tImFzc2ltaWxhdGluZyBmbGVzaCBvbiBjb250YWN0LgpTbmlkZSBjb21wYXJpc29ucyB0byBnZWxhdGluIGJlIGRhbW5lZCwgaXQncyBhIGNvbmNlcHQgd2l0aCB0aGUgbW9zdApkZXZhc3RhdGluZyBvZiBwb3RlbnRpYWwgY29uc2VxdWVuY2VzLCBub3QgdW5saWtlIHRoZSBncmV5IGdvbyBzY2VuYXJpbwpwcm9wb3NlZCBieSB0ZWNobm9sb2dpY2FsIHRoZW9yaXN0cyBmZWFyZnVsIG9mCmFydGlmaWNpYWwgaW50ZWxsaWdlbmNlIHJ1biByYW1wYW50Lg==\""
        reader.Read(); // "EndElement, Name=\"Data\""
        reader.Read(); // "EndElement, Name=\"root\""
        */
    }
}
