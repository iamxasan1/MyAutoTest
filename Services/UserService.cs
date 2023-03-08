using MyAutoTest.Models.Tickets;
using MyAutoTest.Models.Users;
using Newtonsoft.Json;
using User = MyAutoTest.Models.Users.User;
using File = System.IO.File;

namespace MyAutoTest.Services
{
    class UserServices
    {
        private readonly QuestionService _questionService;
        public List<User> _users;

        public UserServices(QuestionService questionService)
        {
            _questionService = questionService;
            ReadUserJson();
        }

        public User AddUser(long chatId, string name)
        {
            if(_users.Any(u => u.ChatId == chatId))
            {
                return _users.First(u => u.ChatId == chatId);
            }
            else
            {
                var user = new User()
                {
                    Name = name,
                    ChatId = chatId,
                    Tickets = new List<Ticket>()
                    
                };

                for (int i = 0; i < _questionService.TicketCount; i++)
                {
                    user.Tickets.Add(new Ticket(i, QuestionService.TicketQuestionCount));       
                }

                _users.Add(user);
                SaveUserjson();
                return user;
            }

        }

        public void UpdateUserStep(User user ,EUserStep step)
        {
            user.Step = step;
            SaveUserjson();
        }


        void ReadUserJson()
        {
            if (File.Exists("users.json"))
            {
                var json = File.ReadAllText("users.json");
                _users = JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();

            }
            _users = new List<User>();
        }

        public void SaveUserjson()
        {
            var json = JsonConvert.SerializeObject(_users);
            File.WriteAllText("users.json", json);
        }

        
    }
}