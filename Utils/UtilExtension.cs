using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace mapper.Util
{
    public static class UtilExtensions
    {
        public static bool TryParseXml( this string xmlData, out XElement parsedData)
        {
            try 
            {
                parsedData = XElement.Parse(xmlData);
                return true;
            }
            catch
            {
                parsedData = null;
                return false;
            }

        }

        public static bool TryParseJson( this string jsonData, out JToken parsedData)
        {
            try 
            {
                parsedData = JToken.Parse(jsonData);
                return true;
            }
            catch
            {
                parsedData = null;
                return false;
            }

        }
    }
}