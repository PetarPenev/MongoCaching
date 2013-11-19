using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;

namespace _4.CachingUtilities
{
    public class Cache
    {
        private MongoCollection<CacheItem> collection;

        public Cache(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            var database = server.GetDatabase("cachedqueries");
            this.collection = database.GetCollection<CacheItem>("cache");

            var keys = IndexKeys.Ascending("Expires");
            var options = IndexOptions.SetTimeToLive(TimeSpan.FromMinutes(2));
            this.collection.EnsureIndex(keys, options);
        }

        public object this[ICollection<ValuePair> cacheKey]
        {
            get { return Get(cacheKey); }
            set { Add(cacheKey, value, DateTime.Now/*.AddMinutes(2)*/); }
        }

        public object Add(ICollection<ValuePair> key, object entry, DateTime utcExpiry)
        {
            Set(key, entry, utcExpiry);
            return entry;
        }

        public object Get(ICollection<ValuePair> key)
        {            
            //var itemToCompare = new CacheItem();
            //itemToCompare.CacheKey = key;
            //key.Add(new ValuePair() { Key = "az", Value = 3 });

            //var query = Query<CacheItem>.EQ(c => c.CacheKey, itemToCompare.CacheKey);

            List<IMongoQuery> queries = new List<IMongoQuery>();
            foreach (var item in key)
	        {
                switch (item.Direction)
                {
                    case (ComparisonDirection.GreaterInclusive):
                        queries.Add(Query.And(Query.EQ("Key", item.Key), Query.GTE("Value", item.Value), Query.EQ("Direction", item.Direction)));
                        break;
                    case (ComparisonDirection.GreaterExclusive):
                        queries.Add(Query.And(Query.EQ("Key", item.Key), Query.GT("Value", item.Value), Query.EQ("Direction", item.Direction)));
                        break;
                    case (ComparisonDirection.LesserInclusive):
                        queries.Add(Query.And(Query.EQ("Key", item.Key), Query.LTE("Value", item.Value), Query.EQ("Direction", item.Direction)));
                        break;
                    case (ComparisonDirection.LesserExclusive):
                        queries.Add(Query.And(Query.EQ("Key", item.Key), Query.LT("Value", item.Value), Query.EQ("Direction", item.Direction)));
                        break;
                    case (ComparisonDirection.ExactlyEqual):
                        queries.Add(Query.And(Query.EQ("Key", item.Key), Query.EQ("Value", item.Value), Query.EQ("Direction", item.Direction)));
                        break;
                }

                /*if (item.Direction == ComparisonDirection.GreaterInclusive)
                {
                    queries.Add(Query.And(Query.EQ("Key", item.Key), Query.GTE("Value", item.Value), Query.EQ("Direction", item.Direction)));
                }
                else if (item.Direction == ComparisonDirection.Lesser)
                {
                    queries.Add(Query.And(Query.EQ("Key", item.Key), Query.LTE("Value", item.Value), Query.EQ("Direction", item.Direction)));
                }*/
	        }

            /*var queryNew = Query.ElemMatch("CacheKey", Query.And(queries));
            var queryResult = this.collection.Find(queryNew);
            IEnumerable<CacheItem> documentResult = queryResult.ToList();*/

            List<IMongoQuery> arrayQueries = new List<IMongoQuery>();
            foreach (var initialQuery in queries)
            {
                arrayQueries.Add(Query.ElemMatch("CacheKey", initialQuery));
            }

            var queryText = Query.And(arrayQueries);
            string jsonQuery = queryText.ToString();
            var query = this.collection.Find(queryText);

            var entity = query.FirstOrDefault();
            /*foreach (var item in key)
            {
                documentResult = documentResult.Where(c => c.CacheKey.Contains(item, new ValuePairCompararer());
            }*/


            if (entity == null /*|| entity.Expires <= DateTime.Now.ToUniversalTime()*/)
            {
                //Remove(entity);
                return null;
            }

            var f = new BinaryFormatter();
            var ms = new MemoryStream(entity.Item);
            object o = f.Deserialize(ms);
            return o;

            /*CacheItem item = null;
            using (Mongo mongo = Mongo.Create(Helper.ConnectionString()))
            {
                MongoCollection<CacheItem> coll = mongo.GetCollection<CacheItem>();
                item = coll.FindOne(new { _id = key });
            }
            if (item == null || item.Expires <= DateTime.Now.ToUniversalTime())
            {
                Remove(key);
                return null;
            }
            var f = new BinaryFormatter();
            var ms = new MemoryStream(item.Item);
            object o = f.Deserialize(ms);
            return o;*/
        }

        public void Remove(CacheItem entity)
        {

            //var query = Query.EQ(c => c.CacheKey, itemToRemove.CacheKey);
            if (entity != null)
            {
                this.collection.Remove(Query.EQ("_id", entity.Id));
            }
            /*using (Mongo mongo = Mongo.Create(Helper.ConnectionString()))
            {
                MongoCollection<CacheItem> coll = mongo.GetCollection<CacheItem>();
                var q = new CacheItem
                {
                    CacheKey = key
                };
                coll.Delete(q);
            }*/
        }

        public void Set(ICollection<ValuePair> key, object entry, DateTime utcExpiry)
        {
            //JsonSerializer serializer = new JsonSerializer();
            //serializer.NullValueHandling = NullValueHandling.Ignore;

            //var writer = new Json
            //key.Add(new ValuePair() { Key = "az", Value = 3 });

            var f = new BinaryFormatter();
            var ms = new MemoryStream();
            f.Serialize(ms, entry);

            var itemToAdd = new CacheItem();
            itemToAdd.CacheKey = key;
            itemToAdd.Item = ms.ToArray();
            itemToAdd.Expires = utcExpiry;

            this.collection.Insert(itemToAdd);

            /*using (Mongo mongo = Mongo.Create(Helper.ConnectionString()))
            {
                MongoCollection<CacheItem> coll = mongo.GetCollection<CacheItem>();
                var f = new BinaryFormatter();
                var ms = new MemoryStream();
                f.Serialize(ms, entry);
                var q = new CacheItem
                {
                    CacheKey = key,
                    Expires = utcExpiry,
                    Item = ms.ToArray()
                };
                coll.Save(q);
            }*/
        }
    }
}
