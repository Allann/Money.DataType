using System.IO;

namespace Hs.Core.Money.Tests.Helpers
{
    public static class StreamExtensions
    {
        public static string ReadToString(this Stream stream)
        {
            stream.Position = 0;
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }

}
