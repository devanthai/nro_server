using System;
using System.Collections.Generic;
using System.Linq;

namespace NRO_Server.Application.Helper
{
    public static class ExtensionList
    {
        public static List<T> Copy<T>(this List<T> listToClone) where T: ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
        
        public static T RemoveAndGetItem<T>(this IList<T> list, int iIndexToRemove)
        {
            var item = list[iIndexToRemove];
            list.RemoveAt(iIndexToRemove);
            return item;
        } 
    }
}