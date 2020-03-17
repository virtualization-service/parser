using System.Collections.Generic;

namespace mapper.Processors{
    public interface IFlattened<T>
    {
        Dictionary<string, string[]> GetList(T structure);
        
    } 
}