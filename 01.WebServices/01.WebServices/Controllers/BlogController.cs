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

        public HttpResponseMessage GetPostsWithMaxLength([FromUri]int postNumber, [FromUri]int maximalPostLength)
        {
            if ((postNumber < 0) || (maximalPostLength < 0))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Both request parameters need to be non-negative.");
            }

            var searchKey = new List<ValuePair>();
            searchKey.Add(new ValuePair() { Key = "postcount", Value = postNumber, Direction = ComparisonDirection.GreaterInclusive });
            searchKey.Add(new ValuePair() { Key = "minlength", Value = maximalPostLength, Direction = ComparisonDirection.GreaterInclusive });

            var cachedResult = this.cacheHelper[searchKey];
            if (cachedResult != null)
            {
                var result = (IEnumerable<Post>)cachedResult;
                result = result.Where(r => r.PostText.Length <= maximalPostLength);
                if (result.Count() >= postNumber)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, result.Take(postNumber));
                }
            }

            var context = new BlogContext();
            var posts = context.Posts.Include("Comments").Where(p => p.PostText.Length <=maximalPostLength).Take(postNumber).ToList();
            this.cacheHelper[searchKey] = posts;
            return Request.CreateResponse(HttpStatusCode.OK, posts);
        }

        public HttpResponseMessage GetPostsWithMinLength([FromUri]int postNumber, [FromUri]int minimalPostLength)
        {
            if ((postNumber < 0) || (minimalPostLength < 0))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Both request parameters need to be non-negative.");
            }

            var searchKey = new List<ValuePair>();
            searchKey.Add(new ValuePair() { Key = "postcount", Value = postNumber, Direction = ComparisonDirection.GreaterInclusive });
            searchKey.Add(new ValuePair() { Key = "minlength", Value = minimalPostLength, Direction = ComparisonDirection.LesserInclusive });

            var cachedResult = this.cacheHelper[searchKey];
            if (cachedResult != null)
            {
                var result = (IEnumerable<Post>)cachedResult;
                result = result.Where(r => r.PostText.Length >= minimalPostLength);
                if (result.Count() >= postNumber)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, result.Take(postNumber));
                }
            }

            var context = new BlogContext();
            var posts = context.Posts.Include("Comments").Where(p => p.PostText.Length >= minimalPostLength).Take(postNumber).ToList();
            this.cacheHelper[searchKey] = posts;
            return Request.CreateResponse(HttpStatusCode.OK, posts);
        }

        public HttpResponseMessage GetPost([FromUri]int id)
        {
            var searchKey = new List<ValuePair>();
            searchKey.Add(new ValuePair() { Key = "postid", Value = id, Direction = ComparisonDirection.ExactlyEqual });

            var cachedResult = this.cacheHelper[searchKey];
            if (cachedResult != null)
            {
                var result = (Post)cachedResult;
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }

            var context = new BlogContext();
            var searchedPost = context.Posts.Include("Comments").Where(p => p.Id == id).SingleOrDefault();
            if (searchedPost == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Post not found");
            }

            this.cacheHelper[searchKey] = searchedPost;
            return Request.CreateResponse(HttpStatusCode.OK, searchedPost);
        }

        public HttpResponseMessage GetComments([FromUri]int postId, [FromUri]int commentCount)
        {
            if (commentCount < 0)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Comment count needs to be non-negative.");
            }

            var searchKey = new List<ValuePair>();
            searchKey.Add(new ValuePair() { Key = "postid", Value = postId, Direction = ComparisonDirection.ExactlyEqual });
            searchKey.Add(new ValuePair() { Key = "commentcount", Value = commentCount, Direction = ComparisonDirection.GreaterInclusive });

            var cachedResult = this.cacheHelper[searchKey];
            if (cachedResult != null)
            {
                var result = (IEnumerable<Comment>)cachedResult;
                return Request.CreateResponse(HttpStatusCode.OK, result.Take(commentCount));
            }

            var context = new BlogContext();
            var searchedPost = context.Posts.Include("Comments").Where(p => p.Id == postId).SingleOrDefault();
            if (searchedPost == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Post not found");
            }

            var comments = searchedPost.Comments.Take(commentCount).ToList();
            this.cacheHelper[searchKey] = comments;
            return Request.CreateResponse(HttpStatusCode.OK, comments);
        }
    }
}
