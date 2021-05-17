using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace ADB.Handlers
{
    public static class Globals
    {
        public static readonly string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + "tagss" + Path.DirectorySeparatorChar;

        public static HttpClient httpClient = new HttpClient();

        public static void Shuffle<T>(this IList<T> list)
        {
            Random random = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}