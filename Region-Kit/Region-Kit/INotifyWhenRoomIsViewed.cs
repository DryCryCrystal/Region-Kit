using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegionKit
{
    public interface INotifyWhenRoomIsViewed
    {
        void RoomViewed();
        void RoomNoLongerViewed();
    }
}
