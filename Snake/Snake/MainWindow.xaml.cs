using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Snake
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Definitions
        private SolidColorBrush SnakeColor = Brushes.Black,
                                BackColor = Brushes.LightBlue,
                                BorderColor = Brushes.Gray;

        private const int INTERVAL_LENGTH = 55;
        private const bool PERMEABLE_WALLS = true;

        private SolidColorBrush[] foodColors = new SolidColorBrush[]
            { Brushes.Red, Brushes.Blue, Brushes.Yellow, Brushes.Green, Brushes.Orange };

        private enum directions { Left, Right, Up, Down };
        private static directions currentDirection { get; set; }
        #endregion Definitions

        #region properties
        private static Coords movement;

        private static Random rand = new Random();

        private Label[,] labels { get; set; }
        private Border[,] borders { get; set; }
        private List<Coords> snake;

        private Coords food;
        private SolidColorBrush currentFoodColor;

        private static DispatcherTimer Timer { get; set; }
        #endregion properties

        private void Draw()
        {
            // draw all cells black
            for (int i = 0; i < Grid.Rows; i++)
            {
                for (int j = 0; j < Grid.Columns; j++)
                {
                    labels[i, j].Background = BackColor;
                    borders[i, j].Background = BackColor;
                    borders[i, j].BorderThickness = new Thickness(0);
                    borders[i, j].BorderBrush = BackColor;
                }
            }

            // draw snake cells white
            for (int i = 0; i < snake.Count; i++)
            {
                borders[snake[i].x, snake[i].y].Background = SnakeColor;
                labels[snake[i].x, snake[i].y].Background = SnakeColor;
                borders[snake[i].x, snake[i].y].BorderThickness = new Thickness(2);
                borders[snake[i].x, snake[i].y].CornerRadius = new CornerRadius(1);
                borders[snake[i].x, snake[i].y].BorderBrush = BorderColor;
            }

            // draw food in random color
            labels[food.x, food.y].Background = currentFoodColor;
        }

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if (currentDirection != directions.Down)
                    {
                        movement = new Coords(-1, 0);
                        currentDirection = directions.Up;
                    }
                    break;
                case Key.Down:
                    if (currentDirection != directions.Up)
                    {
                        movement = new Coords(1, 0);
                        currentDirection = directions.Down;
                    }
                    break;
                case Key.Left:
                    if (currentDirection != directions.Right)
                    {
                        movement = new Coords(0, -1);
                        currentDirection = directions.Left;
                    }
                    break;
                case Key.Right:
                    if (currentDirection != directions.Left)
                    {
                        movement = new Coords(0, 1);
                        currentDirection = directions.Right;
                    }
                    break;
                case Key.Space:
                    // TODO: Pause
                    break;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            Init();
        }

        private void Init()
        {
            // initialize snake
            snake = new List<Coords>();
            snake.Insert(0, new Coords(15, 20));
            snake.Insert(0, new Coords(16, 21));
            snake.Insert(0, new Coords(17, 22));

            // initialize direction
            movement = new Coords(0, 1);
            currentDirection = directions.Right;

            // initialize food
            GenerateNewFood();

            // inizalize grid
            labels = new Label[Grid.Rows, Grid.Columns];
            borders = new Border[Grid.Rows, Grid.Columns];

            for (int i = 0; i < Grid.Rows; i++)
            {
                for (int j = 0; j < Grid.Columns; j++)
                {
                    Label label = new Label();
                    label.Background = Brushes.Black;

                    Border border = new Border();
                    border.Child = label;

                    Grid.Children.Add(border);

                    borders[i, j] = border;
                    labels[i, j] = label;
                }
            }

            // Timer
            Timer = new DispatcherTimer();
            Timer.Tick += new EventHandler(TimeStep);
            Timer.Interval = new TimeSpan(0, 0, 0, 0, INTERVAL_LENGTH);
            Timer.Start();

            // key handler
            this.PreviewKeyDown += OnKeyDown;

            Draw();
        }

        private void TimeStep(object sender, EventArgs e)
        {
            UpdateSnake();
            Draw();
        }

        private bool UpdateSnake()
        {
            // check if updated snaked is inside the grid
            Coords newHead = snake[0] + movement;
            if (CoordIsValid(newHead) == false)
            {
                return false;
            }

            // border permeability
            if (PERMEABLE_WALLS == true)
            {
                if (newHead.x < 0)
                {
                    newHead.x = Grid.Rows - 1;
                }
                else if (newHead.x >= Grid.Rows)
                {
                    newHead.x = 0;
                }
                if (newHead.y < 0)
                {
                    newHead.y = Grid.Columns - 1;
                }
                else if (newHead.y >= Grid.Columns)
                {
                    newHead.y = 0;
                }
            }

            // update the snake
            snake.Insert(0, newHead);
            if (newHead.x == food.x && newHead.y == food.y)
            {
                GenerateNewFood();
            }
            else
            {
                snake.RemoveAt(snake.Count - 1);
            }

            return true;
        }

        //private void printSnake()
        //{
        //    for (int i = 0; i < snake.Count; i++)
        //    {
        //        Console.Write(snake[i].y + ", ");
        //    }
        //    Console.WriteLine();
        //}

        private void GenerateNewFood()
        {
            // create list of all valid positions for the food inside the grid
            List<Coords> positions = new List<Coords>();
            for (int i = 0; i < Grid.Rows; i++)
            {
                for (int j = 0; j < Grid.Columns; j++)
                {
                    bool valid = true;
                    Coords curCoord = new Coords(i, j);
                    for (int k = 0; k < snake.Count; k++)
                    {
                        if (snake[k].Equals(curCoord))
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (valid == true)
                    {
                        positions.Add(curCoord);
                    }
                }
            }

            // pick random element from the list and assign it to the food
            food = positions[rand.Next(0, positions.Count)];

            // Give the food a random color
            currentFoodColor = foodColors[rand.Next(0, foodColors.Length)];
        }

        private bool CoordIsValid(Coords c)
        {
            // check for border collision
            if (PERMEABLE_WALLS == false &&
               (c.x < 0 || c.x >= Grid.Rows ||
                c.y < 0 || c.y >= Grid.Columns))
            {
                return false;
            }

            // check if the snake bites itself   -- inspired by kerstin
            for (int i = 0; i < snake.Count; i++)
            {
                if (snake[i].x == c.x && snake[i].y == c.y)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
