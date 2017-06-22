using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MusicStore.Models
{
    public class Cart
    {
        [Key]
        public int RecordId { get; set; }
        public string CartID { get; set; }
        public int AlbumId { get; set; }
        public int Count { get; set; }
        public DateTime DateCreated { get; set; }

        //navigation
        public virtual Album Album { get; set; }
    } // end class Cart
}