using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4.CachingUtilities
{
    class ValuePairCompararer : IEqualityComparer<ValuePair>
    {
        public bool Equals(ValuePair x, ValuePair y)
        {
            return x.Key.Equals(y.Key) && x.Direction.Equals(y.Direction) /*&& (x.Value >= y.Value)*/;
        }

        public int GetHashCode(ValuePair obj)
        {
            return obj.GetHashCode();
        }
    }
}
