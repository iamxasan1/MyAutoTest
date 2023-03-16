
using MyAutoTest.Models.Tickets;

namespace MyAutoTest.Models.Users
{
    class User
    {
        public long ChatId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public EUserStep Step { get; set; }
        public Ticket CurrentTicket { get; set; }
        public List<Ticket> Tickets { get; set;}

        public User() 
        {
            Tickets = new List<Ticket>();
        }
        public string language { get; set; }
    }
}
