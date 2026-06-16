using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace HSEGame
{
    public class Game : INotifyPropertyChanged
    {
        private int lifes;
        private volatile int points;
        private int currentQuestionNumber;
        public List<Question> Questions { get; set; }
        private Question currentQuestion;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Свойства, к которым привязывается XAML через Game
        public int Lifes
        {
            get { return lifes; }
            set { lifes = value; OnPropertyChanged(); }
        }

        public int Points
        {
            get { return points; }
            set { points = value; OnPropertyChanged(); }
        }

        public int QuestionNumber
        {
            get { return currentQuestionNumber; }
            set { currentQuestionNumber = value; OnPropertyChanged(); }
        }

        

        public Question CurrentQuestion
        {
            get { return currentQuestion; }
            set
            {
                currentQuestion = value;
                OnPropertyChanged();
                OnPropertyChanged("Text");
                OnPropertyChanged("Options");
            }
        }

        public Game()
        {
            Lifes = 3;
            Points = 0;
            QuestionNumber = 1;
            LoadQuestions();
            LoadQuestionInGame(1);
            SelectQuestion();
        }

        private void LoadQuestions()
        {
            var questionPath = Environment.CurrentDirectory;
            questionPath = Directory.GetParent(questionPath).Parent.FullName;
            questionPath = Path.Combine(questionPath, "resources", "question.json");
            string jsonString = File.ReadAllText(questionPath);
            Questions = JsonSerializer.Deserialize<List<Question>>(jsonString);           
        }

        public void SelectQuestion()
        {
            //if (Questions == null || Questions.Count == 0) return;
            if (QuestionNumber >= 10)
            {
                return;
            }
            CurrentQuestion = Questions[QuestionNumber];
        }

        void LoadQuestionInGame(int difficulty)
        {
            Questions = Questions = Questions
                .Where(x => x.Difficulty == difficulty)
                .OrderBy(x => Guid.NewGuid())
                .ToList();

        }

        public bool Answer(int answerIndex)
        {
            if (CurrentQuestion == null) return false;

            if (CurrentQuestion.CorrectAnswer == answerIndex)
            {
                var temp = CurrentQuestion.Difficulty;
                Points += CurrentQuestion.Points;
            }
            else
            {
                Lifes--;
                QuestionNumber++;
                SelectQuestion();
                return false;
            }

            QuestionNumber++;
            SelectQuestion();
            return true;
        }


        private static string GetSavePath()
        {
            var savePath = Environment.CurrentDirectory;
            savePath = Directory.GetParent(savePath).Parent.FullName;
            return Path.Combine(savePath, "resources", "savegame.json");
        }

        public void SaveGame()
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(GetSavePath(), jsonString);
            }
            catch { }
        }

        public static Game LoadGame()
        {
            string path = GetSavePath();
            if (File.Exists(path))
            {
                try
                {
                    string jsonString = File.ReadAllText(path);
                    Game loadedGame = JsonSerializer.Deserialize<Game>(jsonString);
                    loadedGame.LoadQuestions();
                    return loadedGame;
                }
                catch
                {
                    return new Game();
                }
            }
            return new Game();
        }
    }
}
