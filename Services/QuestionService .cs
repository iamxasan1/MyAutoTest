using MyAutoTest.Models;
using Newtonsoft.Json;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot;
using MyAutoTest.Models.Tickets;


namespace MyAutoTest.Services
{
    class QuestionService
    {
        private List<QuestionModel> _questions;
        public string[] choiceName = {"A", "B", "C", "D", "E", "F"};  
        private readonly ITelegramBotClient _bot;

        public const int TicketQuestionCount = 5;  
        public int QuestionsCount 
        { 
            get 
            {
                return _questions.Count; 
            } 
        }

        public int TicketCount { get { return QuestionsCount / TicketQuestionCount; } }

        public QuestionService(ITelegramBotClient bot) 
        {
            _bot = bot;
            var json = File.ReadAllText("uz.json");
            _questions = JsonConvert.DeserializeObject<List<QuestionModel>>(json)!;
        }

        public void ReadJson(string lang)
        {
            if (lang == null || lang == "uzbek 🇺🇿")
            {
                var json = File.ReadAllText("uz.json");
                _questions = JsonConvert.DeserializeObject<List<QuestionModel>>(json)!;
            }
            if (lang == "russian 🇷🇺")
            {
                var json = File.ReadAllText("rus.json");
                _questions = JsonConvert.DeserializeObject<List<QuestionModel>>(json)!;
            }
        }

        public bool QuestionAnswer(int questionIndex, int choiceIndex)
        {
            return _questions[questionIndex].Choices[choiceIndex].Answer;
        }
        InlineKeyboardMarkup CreateChoiceButtons(int index, int? choiceIndex = null, bool? answer = null)
        {
            var choices = new List<List<InlineKeyboardButton>>();
            for (int i = 0; i < _questions[index].Choices.Count; i++)
            {
                //var choiceText = answer == null ? _questions[index].Choices[i].Text : _questions[index].Choices[(int)i].Text + _questions[index].Choices[i].Answer;
                var choiceText = choiceName[i];
                var choiceButton = new List<InlineKeyboardButton>()
        {
            InlineKeyboardButton.WithCallbackData(choiceText, $"{index},{i}")
        };
                choices.Add(choiceButton);
            }
            return new InlineKeyboardMarkup(choices);
        }


        public void SendQuestionByIndex(long chatId, int index)
        {
            var question = _questions[index];
            var message = $"{question.Id}. {question.Question}\n";
            for(int i = 0; i < _questions[index].Choices.Count; i++)
            {
                message += $"{choiceName[i]})  {_questions[index].Choices[i].Text}\n";
            }


            if (question.Media.Exist)
            {
                FileStream? file = default;
                try
                {
                    var fileBytes = File.ReadAllBytes($"Autotest/{question.Media.Name}.png");
                    var ms = new MemoryStream(fileBytes);
                    _bot.SendPhotoAsync(
                        chatId: chatId,
                        photo: new InputOnlineFile(ms),
                        caption: message,
                        replyMarkup: CreateChoiceButtons(index));
                }
                catch (Exception e)
                {

                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                _bot.SendTextMessageAsync(chatId, message, replyMarkup: CreateChoiceButtons(index));
            }
        }

        public Ticket CreateTicket()
        {
            var random = new Random();
            var ticket = random.Next(0, TicketCount);
            var startQuestionIndex = ticket * TicketQuestionCount;
            var finishQuestiionIndex = (ticket + 1) * TicketQuestionCount;

            return new Ticket()
            { 
                TicketIndex = ticket, 
                QuestionsCount = TicketQuestionCount,
                StartIndex = startQuestionIndex,
                CurrentQuestionIndex = startQuestionIndex,
                CorrectCount = 0
            };
        }
    }
}

