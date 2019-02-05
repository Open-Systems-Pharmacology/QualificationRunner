using System.Threading.Tasks;

namespace QualificationRunner.Core.Services
{
   public interface IBatchRunner<TBatchOptions>
   {
      Task RunBatchAsync(TBatchOptions runOptions);
   }
}