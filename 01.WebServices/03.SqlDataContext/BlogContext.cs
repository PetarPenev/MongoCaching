using _02.SQLDataClasses;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _03.SqlDataContext
{
    public class BlogContext : DbContext
    {
        public BlogContext() :
            base("BlogDb")
        {
        }

        public DbSet<Post> Posts { get; set; }

        public DbSet<Comment> Comments { get; set; }
    }
}
