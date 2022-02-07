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
                await  ExportAndImportDataOldWay.ExportAndImportLargeItemsNotEfficiently().ConfigureAwait(false);
                await ExportAndImportDataNewWay.ExportAndImportLargeItemsEfficiently().ConfigureAwait(false);

            }
            Console.WriteLine("Done");
        }
    }
}

