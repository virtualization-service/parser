using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Parser.Model;

namespace Parser.Processors
{
    public class FlattenedJsonStructure : IFlattened<JToken>
    {
        private Dictionary<string, string[]> _flattened = new Dictionary<string, string[]>();

        private List<ElementStructure> _elementsFound = new List<ElementStructure>();

        public Dictionary<string, string[]> GetList(JToken json)
        {
            foreach (var jsonValue in json.Children())
            {
                if(jsonValue.HasValues)
                    GetList(jsonValue);
                else{
                    var jValue = jsonValue.Value<string>();
                    var path = jsonValue.Path;
                    var foundElements = _elementsFound.Count(p=> p.Xpath == path);
                    _elementsFound.Add(new ElementStructure(jValue, path, foundElements));

                    _flattened.Add(path, new[] { jValue});
                }

            }
            return _flattened;
        }
    }
}
        