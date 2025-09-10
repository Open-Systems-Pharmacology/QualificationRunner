using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace QualificationRunner.Core.Extensions;

public static class HttpClientExtensions
{
   public static async Task DownloadFileTaskAsync(this HttpClient client, Uri uri, string fileName)
   {
      await using var stream = await client.GetStreamAsync(uri);
      await using var fileStream = new FileStream(fileName, FileMode.CreateNew);
      await stream.CopyToAsync(fileStream);
   }
}