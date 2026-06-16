using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace HSEGame
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private TranslateTransform3D firstModelMove;
        private TranslateTransform3D secondModelMove;
        public Game ActiveGame { get; set; }
        bool isActiveGame;
        List<Question> questions;
        public MainWindow()
        {
            
            InitializeComponent();
            isActiveGame = false;
            ActiveGame = Game.LoadGame();
            MyViewport.Visibility = Visibility.Hidden;
            GamePanel.DataContext = this;
            this.DataContext = this;


            if (ActiveGame.QuestionNumber > 1 && ActiveGame.Lifes > 0)
            {
                isActiveGame = true;
                EntryView.Visibility = Visibility.Hidden;
                GamePanel.Visibility = Visibility.Visible;
            }
            else
            {
                EntryView.Visibility = Visibility.Visible;
                GamePanel.Visibility = Visibility.Hidden;
            }

            InitializeComponent();
            InitializeScene();

            // Вызываем загрузку при старте приложения
        }

        public string CurrentPoints
        {
            get
            {
                return ActiveGame.Points.ToString();
            }
        }

        private void InitializeScene()
        {
            try
            {
                firstModelMove = new TranslateTransform3D(0, 0, 0);
                LoadAndAddObject("Models/road.obj", firstModelMove);

                secondModelMove = new TranslateTransform3D(-0.5, 0, -12);
                LoadAndAddObject("Models/car.obj", secondModelMove);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сборки сцены: {ex.Message}");
            }
        }

        private void StartSynchronizedMovement()
        {
            // 1. Убедимся, что объекты и камера физически существуют на сцене
            if (secondModelMove == null || TheCamera == null) return;

            Duration duration = new Duration(TimeSpan.FromSeconds(5));

            // 2. НАСТРАИВАЕМ АНИМАЦИЮ ОБЪЕКТА
            DoubleAnimation modelAnim = new DoubleAnimation();
            modelAnim.From = -12;
            modelAnim.To = 12;
            modelAnim.Duration = duration;


            // 3. НАСТРАИВАЕМ АНИМАЦИЮ КАМЕРЫ
            Point3DAnimation cameraAnim = new Point3DAnimation();
            cameraAnim.From = new Point3D(0, 16, -19);
            cameraAnim.To = new Point3D(0, 16, 5);
            cameraAnim.Duration = duration;


            // 4. ПРИНУДИТЕЛЬНО ВКЛЮЧАЕМ СЦЕНУ В ПАМЯТИ
            MyViewport.UpdateLayout();

            // 5. ГЛАВНЫЙ СЕКРЕТ: Запускаем анимации напрямую в обход Storyboard
            // Это заставит WPF жестко перезаписывать свойства каждый кадр
            secondModelMove.BeginAnimation(TranslateTransform3D.OffsetZProperty, modelAnim);
            TheCamera.BeginAnimation(PerspectiveCamera.PositionProperty, cameraAnim);
        }
        private void LoadAndAddObject(string filePath, TranslateTransform3D transform)
        {
            ObjReader reader = new ObjReader();
            Model3DGroup importedModel = reader.Read(filePath);

            ModelVisual3D uniqueVisual = new ModelVisual3D();
            uniqueVisual.Content = importedModel;

            uniqueVisual.Transform = transform;

            MyViewport.Children.Add(uniqueVisual);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            if (isActiveGame == false)
            {
                ActiveGame = new Game();
                OnPropertyChanged("ActiveGame");
                GuideInfo.Visibility = Visibility.Visible;
            }
            else
            {
                ActiveGame.Lifes = 3;
                ActiveGame.Points = 0;
                ActiveGame.QuestionNumber = 1;
                ActiveGame.SelectQuestion();
            }
            this.DataContext = this;
            EntryView.Visibility = Visibility.Hidden;
        }

        private void SetupCamera()
        {
            // 1. Берем текущую камеру из нашего Viewport3D
            var camera = MyViewport.Camera as PerspectiveCamera;

            if (camera != null)
            {
                // 2. Где физически находится камера (X, Y, Z)
                camera.Position = new Point3D(0, 16, 4); // Приподнята вверх на 5 и отодвинута назад на 10

                // 3. Вектор направления взгляда камеры. 
                // Если объект в центре (0,0,0), то смотрим из точки камеры в центр:
                camera.LookDirection = new Vector3D(0, -8, -3);

                // 4. Задаем, какая ось направлена вверх (обычно это ось Y)
                camera.UpDirection = new Vector3D(0, 1, 0);
            }
        }

        private void StartGameAfterGuide_Click(object sender, RoutedEventArgs e)
        {
            GuideInfo.Visibility = Visibility.Hidden;
            GamePanel.Visibility = Visibility.Visible;
            MyViewport.Visibility = Visibility.Visible;
            isActiveGame = true;
            this.DataContext = this;
        }

        void importData()
        {
            var questionPath = Environment.CurrentDirectory;
            questionPath = Directory.GetParent(questionPath).Parent.FullName;
            questionPath = System.IO.Path.Combine(questionPath, "resources", "question.json");

            string jsonString = File.ReadAllText(questionPath);
            questions = JsonSerializer.Deserialize<List<Question>>(jsonString);
        }


        void AnswerMethod(int answer)
        {
            if (ActiveGame == null || ActiveGame.Lifes == 0) return;

            if (ActiveGame.Answer(answer))
            {
                StartSynchronizedMovement();
                OnPropertyChanged(CurrentPoints);
            }
                

            if (ActiveGame.Lifes == 0 || ActiveGame.QuestionNumber >= 10)
            {
                MessageBox.Show($"Игра окончена! Вы набрали очков: {ActiveGame.Points}");
                isActiveGame = false;
                GamePanel.Visibility = Visibility.Hidden;
                EntryView.Visibility = Visibility.Visible;
                MyViewport.Visibility = Visibility.Hidden;
            }
        }
        private void ABtn_Click(object sender, RoutedEventArgs e)
        {
            AnswerMethod(0);
        }

        private void CBtn_Click(object sender, RoutedEventArgs e)
        {
            AnswerMethod(2);
        }

        private void BBtn_Click(object sender, RoutedEventArgs e)
        {
            AnswerMethod(1);
        }

        private void DBtn_Click(object sender, RoutedEventArgs e)
        {
            AnswerMethod(3);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

        }
    }
}
