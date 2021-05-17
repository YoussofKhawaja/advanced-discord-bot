using System.Drawing;
using System.IO;

namespace ADB.Services
{
    public static class HTTPrequest
    {
        public static Image RequestImage(string link)
        {
            System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(link);
            webRequest.AllowWriteStreamBuffering = true;
            webRequest.Timeout = 30000;

            System.Net.WebResponse webResponse = webRequest.GetResponse();

            Stream stream = webResponse.GetResponseStream();
            Image final = Image.FromStream(stream);
            webResponse.Close();
            return final;
        }
    }
}
