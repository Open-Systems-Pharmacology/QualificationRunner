using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using OSPSuite.Utility.Exceptions;
using OSPSuite.Utility.Extensions;
using QualificationRunner.Core.Assets;
using QualificationRunner.Core.Services;

namespace QualificationRunner.Core
{
   public class QualificationRunnerException : OSPSuiteException
   {
      public QualificationRunnerException()
      {
      }

      public QualificationRunnerException(string message) : base(message)
      {
      }

      public QualificationRunnerException(string message, Exception innerException) : base(message, innerException)
      {
      }

      protected QualificationRunnerException(SerializationInfo info, StreamingContext context) : base(info, context)
      {
      }

      public QualificationRunnerException(IReadOnlyList<QualificationRunResult> invalidResults) : base(invalidResults.Select(x => Errors.ProjectConfigurationNotValid(x.ProjectId, x.LogFileFullPath)).ToString("\n"))
      {
      }
   }
}