using System;
using System.Collections.Generic;
using System.Dynamic;

namespace QualificationRunner.Core.Services
{
   public static class ExpandoObjectHelpers
   {
      public static bool HasProperty(ExpandoObject obj, string propertyName)
      {
         return ((IDictionary<String, object>) obj).ContainsKey(propertyName);
      }
   }
}