using _02.SQLDataClasses;
using _03.SqlDataContext;
using _4.CachingUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace _01.WebServices.Controllers
{
    public class BlogController : ApiController
    {
        private const string mongoConnectionString = "mongodb://localhost/";

        private Cache cacheHelper;

        public BlogController()
            : base()
        {
            this.cacheHelper = new Cache(mongoConnectionString);
        }
        public IEnumerable<Post> GetPosts([FromUri]int postNumber)
        {
            var searchKey = new List<ValuePair>();
            searchKey.Add(new ValuePair() { Key = "postcount", Value = postNumber });

            var cachedResult = this.cacheHelper[searchKey];
            if (cachedResult != null)
            {
                var result = (List<Post>)cachedResult;
                return result.Take(postNumber);
            }

            var context = new BlogContext();
            var posts = context.Posts.Include("Comments").Take(postNumber).ToList();
            this.cacheHelper[searchKey] = posts;
            return posts;
        }

        public Post GetPost([FromUri]int id)
        {
            return null;
        }

        public IEnumerable<Comment> GetComments([FromUri]int postId, [FromUri]int commentCount)
        {
            return null;
        }
    }
}
