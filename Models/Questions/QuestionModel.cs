using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAutoTest.Models
{
    public class QuestionModel
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string Description { get; set; }
        public QuestionMedia Media { get; set; }
        public List<QuestionChoices> Choices { get; set; }
    }
}
