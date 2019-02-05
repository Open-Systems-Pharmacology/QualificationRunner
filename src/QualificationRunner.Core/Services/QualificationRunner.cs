using System.Threading.Tasks;
using QualificationRunner.Core.RunOptions;

namespace QualificationRunner.Core.Services
{
   public class QualificationRunner : IBatchRunner<QualificationRunOptions>
   {
      public Task RunBatchAsync(QualificationRunOptions runOptions)
      {
         return Task.CompletedTask;
      }
   }
}