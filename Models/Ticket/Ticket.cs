
namespace MyAutoTest.Models.Tickets
{
    public class Ticket
    {
        public int TicketIndex { get; set; }
        public int CorrectCount { get; set; }
        public int QuestionsCount { get; set; }
        public int StartIndex { get; set; }
        public int CurrentQuestionIndex { get; set; }
        public bool IsCompleted
        {
            get
            {
                return CurrentQuestionIndex - StartIndex >= QuestionsCount;
            }
        }

        public Ticket() { }
        public Ticket(int ticketIndex, int questionsCount)
        {
            TicketIndex = ticketIndex;
            CorrectCount = 0;
            QuestionsCount = questionsCount;
            StartIndex = ticketIndex * questionsCount;
            CurrentQuestionIndex = StartIndex;
        }

        public void SetDefault()
        {
            CorrectCount = 0;
            CurrentQuestionIndex =  StartIndex;
        }
    }
}
