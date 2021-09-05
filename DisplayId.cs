using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace avaness.CameraLCD
{
    public struct DisplayId : IEquatable<DisplayId>
    {
        public long EntityId;
        public int Area;

        public DisplayId(long entityId, int area)
        {
            EntityId = entityId;
            Area = area;
        }

        public override bool Equals(object obj)
        {
            return obj is DisplayId id && Equals(id);
        }

        public bool Equals(DisplayId other)
        {
            return EntityId == other.EntityId &&
                   Area == other.Area;
        }

        public override int GetHashCode()
        {
            int hashCode = -1120748461;
            hashCode = hashCode * -1521134295 + EntityId.GetHashCode();
            hashCode = hashCode * -1521134295 + Area.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(DisplayId left, DisplayId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DisplayId left, DisplayId right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return "{EntityId: " + EntityId + ", Area: " + Area + "}";
        }
    }
}
