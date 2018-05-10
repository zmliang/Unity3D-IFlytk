namespace JinkeGroup.Logic
{
    public interface IMessageEventHandler
    {
        bool OnMessageEvent(MessageEvent msgEvent);
    }
}
