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
            // Simulating (Exporting and Importing 20 Large Items (50 mb Items) consecutively to analyze memory efficiency and CPU usage.
            // I've attached the "tiny_item.json" file but not the large item. That is in the gitignore because I didn't want to add large items like that 
            // On Git. You can create a large piece of text and base64 encode and replace it with the data field of the tiny_item.json
            // if you'd like to test it yourself.
            for (int i = 0; i < 20; i++)
            {
                /* Comment Out the One You don't want to run. You can use Perfview to do a more detailed analysis of performance of app */ 

                // Old way I parsed ExportedItem from DB1 and imported to DB2
                await ExportAndImportDataOldWay.ExportAndImportLargeItemsNotEfficiently().ConfigureAwait(false);

                // Optimized way to handle the Exported Json object and import it to DB2
                await ExportAndImportDataNewWay.ExportAndImportLargeItemsEfficiently().ConfigureAwait(false);
            }
            Console.WriteLine("Done");
        }
    }
}

