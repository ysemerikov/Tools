using Tools.Core.Logging;

namespace Tools.Core
{
    public abstract class StarterBase
    {
        protected readonly ILogger logger;
        protected readonly ArgumentReader argumentReader;

        protected StarterBase(ILogger logger, ArgumentReader argumentReader)
        {
            this.logger = logger;
            this.argumentReader = argumentReader;
        }
    }
}