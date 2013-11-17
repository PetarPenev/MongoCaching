using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4.CachingUtilities
{
    public class ValuePair : IEquatable<ValuePair>
    {
        public string Key { get; set; }

        public int Value { get; set; }

        bool IEquatable<ValuePair>.Equals(ValuePair other)
        {
            return this.Key.Equals(other.Key, StringComparison.InvariantCultureIgnoreCase)
                && this.Value >= other.Value;
        }
    }
}
