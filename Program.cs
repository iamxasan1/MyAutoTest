﻿using JFA.Telegram.Console;
using MyAutoTest.Models.Users;
using MyAutoTest.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using User = MyAutoTest.Models.Users.User;

var botManager = new TelegramBotManager();
var bot = botManager.Create("6263742812:AAFoMdqXOo-Hl_LsZ4xWQB_nrmEJnBY4ibE");

var questionService = new QuestionService(bot);
var userService = new UserServices(questionService);
userService.UzMenuList.Add("start test", "Testni Boshlash");
userService.UzMenuList.Add("tickets", "Biletlar");
userService.UzMenuList.Add("show result", "Natijalarni ko`rish");
userService.UzMenuList.Add("choose language", "tilni tanlash");
userService.RuMenuList.Add("start test", "Начать тест");
userService.RuMenuList.Add("tickets", "Билеты");
userService.RuMenuList.Add("show result", "Показать результаты");
userService.RuMenuList.Add("choose language", "Выберите язык");

botManager.Start(OnUpdate);

void OnUpdate(Update update)
{
    var (chatId, message, name, isSucces) = GetMessage(update);
    if(!isSucces)
        return;

    var user = userService.AddUser(chatId, name);

    questionService.ReadJson(user.language);

    switch (user.Step)
    {
        case EUserStep.Default: SendLanguageCode(user); break;
        case EUserStep.ChooseLanguageSendMenu: SaveLanguageSendMenu(user, message); break;
        case EUserStep.InMenu: ChooseMenu(user, message); break;
        case EUserStep.InTest: CheckAnswer(user, message); break;
         
    }
    if(update.Type == UpdateType.CallbackQuery)
    {
        if (message.StartsWith("page"))
        {
            bot.DeleteMessageAsync(user.ChatId, update.CallbackQuery.Message.MessageId);
             var page = Convert.ToInt32(message.Replace("page", ""));
            SHowTickets(user, page);
        }
    }
}

Tuple<long, string, string, bool>GetMessage(Update update)
{
    if(update.Type == UpdateType.Message)
    {
        return new(update.Message.From.Id, update.Message.Text, update.Message.From.FirstName, true);
    }
    if(update.Type == UpdateType.CallbackQuery) 
    {
        return new(update.CallbackQuery.From.Id, update.CallbackQuery.Data, update.CallbackQuery.From.FirstName, true);
    }
    return new(default, default, default, false);
}


void SendLanguageCode(User user)
{
    var buttons = new List<List<KeyboardButton>>()
    {
        new List<KeyboardButton>()
        {
            new KeyboardButton("Uzbek 🇺🇿")
        },
         new List<KeyboardButton>()
        {
            new KeyboardButton("Russian 🇷🇺")
        },
    };
    bot.SendTextMessageAsync(user.ChatId, "choose-language",replyMarkup: new ReplyKeyboardMarkup(buttons));
    userService.UpdateUserStep(user, EUserStep.ChooseLanguageSendMenu);
}
void SendMenu(User user, ITelegramBotClient bot)
{
    if(user.language == "uzbek 🇺🇿")
    {
        userService.ShowMenuUz(user, bot);
    }
    if(user.language == "russian 🇷🇺")
    {
        userService.ShowMenuRu(user, bot);
    }
    
    userService.UpdateUserStep(user, EUserStep.InMenu);
}

void ChooseMenu(User user, string message)
{
    switch (message)
    {
        case "Testni Boshlash": StartTest(user); break; 
        case "Начать тест": StartTest(user); break; 
        case "Biletlar": SHowTickets(user); break; 
        case "Билеты": SHowTickets(user); break; 
        case "Natijalarni ko`rish": ShowResult(user); break;
        case "Показать результаты": ShowResult(user); break;
        case "tilni tanlash": SendLanguageCode(user); break;
        case "Выберите язык": SendLanguageCode(user); break;
        case "Start":
            {
                userService.UpdateUserStep(user, EUserStep.InTest);
                SendTicketQuestion(user);
            }
            break;

    }
    if (message.StartsWith("start-ticket"))
    {
        var ticketIndex = Convert.ToInt32(message.Replace("start-ticket", ""));
        StartTicket(user, ticketIndex);
    }
}

void StartTest(User user)
{
    user.CurrentTicket = questionService.CreateTicket();

    bot.SendTextMessageAsync(user.ChatId, $"{user.CurrentTicket.TicketIndex} ticket\n{user.CurrentTicket.QuestionsCount} questions",
    replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Start")));
}
void ShowResult(User user)
{
    var message = "ticket results:\n";

    message += $"Tickets: {user.Tickets.Count(t=> t.IsCompleted)}\n";
    message += $"Questions: {user.Tickets.Sum(t=>t.CorrectCount)}\n";

    bot.SendTextMessageAsync(user.ChatId, message);
}

void SendTicketQuestion(User user)
{
    questionService.SendQuestionByIndex(user.ChatId, user.CurrentTicket.CurrentQuestionIndex);
   
}
void CheckAnswer(User user, string message)
{

    try
    {
        int[] data = message.Split(',').Select(int.Parse).ToArray();

        var answer = questionService.QuestionAnswer(data[0], data[1]);

        if (answer)
            user.CurrentTicket.CorrectCount++;
        user.CurrentTicket.CurrentQuestionIndex++;

        if (user.CurrentTicket.IsCompleted)
        {
            bot.SendTextMessageAsync(user.ChatId, $"Result: {user.CurrentTicket.CorrectCount}/{user.CurrentTicket.QuestionsCount}");
            //user.Tickets.Add(user.CurrentTicket);
            userService.UpdateUserStep(user, EUserStep.InMenu);
        }
        else
        {
            SendTicketQuestion(user);
        }

    }
    catch (Exception e)
    {

        Console.WriteLine(e.Message);
    }   
}
void SHowTickets(User user, int page = 1)
{
    var pagesCount = questionService.TicketCount / 5;
    var message = $"Tickets\n{page}/{pagesCount}";
    var buttons = new List<List<InlineKeyboardButton>>();
    for (int i = page * 5 - 5; i < page * 5; i++)
    {
        var ticket = user.Tickets[i]; 
        var ticketInfo = $"Ticket {ticket.TicketIndex + 1} ";

        if(ticket.StartIndex != ticket.CurrentQuestionIndex)
        {
            if (ticket.CorrectCount == ticket.QuestionsCount)
            {
                ticketInfo += $"✅";
            }
            else
                ticketInfo += $"{ticket.CorrectCount}/{ticket.QuestionsCount}";
        }

        buttons.Add(new List<InlineKeyboardButton>()
        {
            InlineKeyboardButton.WithCallbackData(ticketInfo, $"start-ticket{ticket.TicketIndex}")
        });
    }
    buttons.Add(CreatePaginationButtons(pagesCount ,page));
    bot.SendTextMessageAsync(user.ChatId, message, replyMarkup: new InlineKeyboardMarkup(buttons));
}

List<InlineKeyboardButton> CreatePaginationButtons(int pagesCount, int page)
{
    var buttons = new List<InlineKeyboardButton>();

    if(page > 1)
        buttons.Add(InlineKeyboardButton.WithCallbackData($"<", $"page{page-1}"));

    if(pagesCount > page)
        buttons.Add(InlineKeyboardButton.WithCallbackData($"{page}", $"page{page}"));
    if(pagesCount - 1 > page)
        buttons.Add(InlineKeyboardButton.WithCallbackData($"{page + 1}", $"page{page+1}"));
    if(pagesCount - 2 > page)
        buttons.Add(InlineKeyboardButton.WithCallbackData($"{page + 2}", $"page{page+2}")); 
    if(pagesCount - 3 > page)
        buttons.Add(InlineKeyboardButton.WithCallbackData($"{page + 3}", $"page{page+3}"));
    if(pagesCount - 4 > page)
        buttons.Add(InlineKeyboardButton.WithCallbackData($"{page + 4}", $"page{page+4}"));
    if(pagesCount - 5 > page)
        buttons.Add(InlineKeyboardButton.WithCallbackData($"{page + 5}", $"page{page+5}"));

    
    if (pagesCount-1 > page) 
        buttons.Add(InlineKeyboardButton.WithCallbackData($">", $"page{page+1}"));
        

    return buttons;
}


void StartTicket(User user, int ticketIndex)
{
    user.CurrentTicket = user.Tickets[ticketIndex];
    user.CurrentTicket.SetDefault();

    bot.SendTextMessageAsync(
        user.ChatId,
        $"{user.CurrentTicket.TicketIndex + 1} ticket\n{user.CurrentTicket.QuestionsCount} questions",
        replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Start")));
}


void SaveLanguageSendMenu(User user, string LangCode)
{
    userService.SetUsetLanguage(user, LangCode);
    userService.SaveUserjson();
    SendMenu(user, bot);
}

