using System.Collections.Generic;

namespace Parser.Processors{
    public interface IFlattened<T>
    {
        Dictionary<string, string[]> GetList(T structure);
        
    } 
}