using Serilog.Core;
using Serilog.Events;

namespace Blockchain
{
    internal class UnityDebugSink : ILogEventSink
    {
        public void Emit(LogEvent logEvent)
        {
            if (logEvent.Exception is null)
            {
                UnityEngine.Debug.Log($"{logEvent.Timestamp}: {logEvent.RenderMessage()}");
            }
            else
            {
                UnityEngine.Debug.Log($"{logEvent.Timestamp}: {logEvent.RenderMessage()}\n{logEvent.Exception}");
            }
        }
    }
}
