using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4.CachingUtilities
{
    [Serializable]
    public class CacheItem : IEquatable<CacheItem>
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ICollection<ValuePair> CacheKey { get; set; }
        public DateTime Expires { get; set; }
        public byte[] Item { get; set; }

        [BsonConstructor]
        public CacheItem()
        {
            this.CacheKey = new List<ValuePair>();
        }

        public bool Equals(CacheItem other)
        {
            foreach (var item in this.CacheKey)
            {
                bool equalFound = false;

                foreach (var otherItem in other.CacheKey)
                {

                    if (item.Equals(otherItem))
                    {
                        equalFound = true;
                    }
                }

                if (!equalFound)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
