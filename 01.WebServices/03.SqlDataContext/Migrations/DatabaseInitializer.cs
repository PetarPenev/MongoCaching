namespace _03.SqlDataContext.Migrations
{
    using _02.SQLDataClasses;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    public sealed class DatabaseInitializer : DbMigrationsConfiguration<_03.SqlDataContext.BlogContext>
    {
        public DatabaseInitializer()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(BlogContext context)
        {
            if (context.Posts.Count() == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    CreatePostWithComments(context, i);
                }
            }
        }

        private void CreatePostWithComments(BlogContext context, int postNumber)
        {
            var post = new Post();
            post.PostText = String.Format("{0} post text.", postNumber);
            for (int i = 0; i < 20; i++)
            {
                post.Comments.Add(new Comment()
                {
                    CommentText = String.Format("This is comment {0} for post {1}.",
                        i, postNumber)
                });
            }

            context.Posts.Add(post);
            context.SaveChanges();
        }
    }
}
