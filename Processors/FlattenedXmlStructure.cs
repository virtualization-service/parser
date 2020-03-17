using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Parser.Model;
using Parser.Util;

namespace Parser.Processors
{
    public class FlattenedXmlStructure : IFlattened<XElement>
    {
        private Dictionary<string, string[]> _flattened = new Dictionary<string, string[]>();

        private List<ElementStructure> _elementsFound = new List<ElementStructure>();
        
        public Dictionary<string, string[]> GetList(XElement xml)
        {
            foreach (var element in xml.Elements())
            {
                if(element.HasElements) GetList(element);

                else if(!string.IsNullOrEmpty(element.Value))
                {
                    XElement xmlData;
                    if(element.Value.TryParseXml(out xmlData))
                    {
                        var elementSubStructure = new FlattenedXmlStructure().GetList(xmlData);

                        foreach (var subElement in elementSubStructure)
                        {
                            var name = "//" + GetName(element) + subElement.Key.Remove(0,1);
                            if(name.Length > 117) name = name.Substring(name.Length - 117);
                            name = name.Replace(".","_");

                            if(!_flattened.Any(p=> p.Key == name))
                            {
                                _flattened.Add(name,subElement.Value);
                            }
                            else
                            {
                                var finalList =  _flattened.GetValueOrDefault(name).ToList();
                                finalList.AddRange(subElement.Value);
                                _flattened[name] = finalList.ToArray();
                            }
                        }
                    }
                    else
                    {
                            var name = "//" + GetName(element);
                            if(name.Length > 117) name = name.Substring(name.Length - 117);
                            name = name.Replace(".","_");

                            var foundElements = _elementsFound.Count(p=> p.Xpath == name);

                            if(!_flattened.Any(p=> p.Key == name))
                            {
                                _flattened.Add(name,new[] { element.Value});
                            }
                            else
                            {
                                _flattened[name] =  _flattened.GetValueOrDefault(name).Append(element.Value).ToArray();
                            }
                            _elementsFound.Add(new ElementStructure(element.Value, name, foundElements));

                    }
                }

            }
            return _flattened;
        }

        private static string GetName(XElement element)
        {
            if(element.Parent != null)
            {
                return GetName(element.Parent) + "/" + element.Name.LocalName;
            }
            else
            {
                return element.Name.LocalName;
            }
        }
    }
}