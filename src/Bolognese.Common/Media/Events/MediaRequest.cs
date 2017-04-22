namespace Bolognese.Common.Media
{
    public class MediaRequest
    {
        public MediaRequestType RequestType { get; private set; }

        public MediaRequest(MediaRequestType type)
        {
            RequestType = type;
        }
    }
}
