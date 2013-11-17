using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _02.SQLDataClasses
{
    [Serializable]
    public class Post
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50), MinLength(5)]
        public string PostText { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public Post()
        {
            this.Comments = new List<Comment>();
        }
    }
}
