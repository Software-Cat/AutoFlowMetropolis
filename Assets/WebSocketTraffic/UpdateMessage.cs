using System;
using System.Collections.Generic;

namespace WebSocketTraffic
{
    [Serializable]
    public class UpdateMessage
    {
        public List<VehicleUpdateMessage> updates;
    }
}
