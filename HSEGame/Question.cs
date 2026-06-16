using System.Text.Json.Serialization;

namespace HSEGame
{
    public class Question : QuestionCategory
    {
        public string Text 
        { 
            get; 
            set;
        }
        public string[] Options 
        { 
            get; set; 
        }
        public int CorrectAnswer 
        { 
            get; set;
        }
        public int Points
        {
            get; set;
        }

        public byte Difficulty
        {
            get; set;
        }


        [JsonConstructor]
        public Question() : base("")
        {
        }

        [JsonIgnore]
        public int QuestionNumber
        {
            get;
            set;
        }

        public Question(string categoryName, string text, string[] options, int correctAnswer, byte Difficulty) : base(categoryName)
        {
            Text = text;
            Options = options;
            CorrectAnswer = correctAnswer;
            this.Difficulty = Difficulty;
        }


        [JsonIgnore]        

        public string GetStringDifficulty
        {
            get
            {
                if (Difficulty == 1)
                {
                    Points = 100;
                    return "Легко";
                }
                else if (Difficulty == 2)
                {
                    Points = 200;
                    return "Средне";
                }
                else
                {
                    Points = 300;
                    return "Сложно";
                }
            }
        }

    }
}
