using System;

namespace QualificationRunner.Core.Domain
{
   [Flags]
   public enum ExitCodes
   {
      Success = 0,
      Error = 1 << 0,
   }
}