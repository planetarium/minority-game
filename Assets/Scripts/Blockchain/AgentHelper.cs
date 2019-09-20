using System.Reflection;
using LibplanetUnity;

namespace Blockchain
{
    public static class AgentHelper
    {
        public static _Agent InnerAgent
        {
            get
            {
                FieldInfo agentField = typeof(Agent).GetField("_agent", BindingFlags.Static | BindingFlags.NonPublic);
                return (_Agent) agentField.GetValue(null);
            }
        }
    }
}
