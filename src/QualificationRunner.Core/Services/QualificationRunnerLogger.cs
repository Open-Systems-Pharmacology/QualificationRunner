using Microsoft.Extensions.Logging;
using OSPSuite.Infrastructure.Logging;

namespace QualificationRunner.Core.Services
{
   public class QualificationRunnerLogger : OSPSuiteLogger
   {
      public QualificationRunnerLogger(ILoggerFactory loggerFactory) : base(loggerFactory, Constants.PRODUCT_NAME)
      {
      }
   }
}