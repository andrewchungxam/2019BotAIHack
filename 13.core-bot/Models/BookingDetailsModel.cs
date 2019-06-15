using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Models
{
    public class BookingDetailsModel : BaseModel
    {
        public string Destination { get; set; }
        public string Origin { get; set; }
        public string TravelDate { get; set; }
    }
}
