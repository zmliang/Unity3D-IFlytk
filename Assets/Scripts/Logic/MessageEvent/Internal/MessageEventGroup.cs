using JinkeGroup.Logic.Internal;
using JinkeGroup.Util;
namespace JinkeGroup.Logic.MessageEventInternal
{
    public class MessageEventGroup
    {

        public BinarySortList<IMessageEventHandler> MessageEventHandlers = new BinarySortList<IMessageEventHandler>(32);

        public bool OnMessageEvent(MessageEvent messageEvent)
        {
            for (int i = 0; i < MessageEventHandlers.Count; ++i)
            {
                IMessageEventHandler handler = MessageEventHandlers[i];
                if (handler.OnMessageEvent(messageEvent))
                {
#if DEVEL_BUILD || PROD_BUILD || UNITY_EDITOR
                    string desc = MessageEventManager.Instance.GetMessageEventDesc(messageEvent);
                    JinkeGroup.Util.Logger.DebugT("MessageEventGroup", "EventMessage processed by {0}\n{1}", handler.ToString(), desc);
#endif
                    return true;
                }
            }
            return false;
        }
    }
}