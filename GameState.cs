using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    public class GameState
    {
        public int Rows { get; }
        public int Cols { get; }
        public GridValue[,] Grid { get; }
        public Direction Dir { get; private set; }
        public int Score { get; private set; }
        public bool GameOver { get; private set; }


        private readonly LinkedList<Direction> dirChanges = new LinkedList<Direction>();
        private readonly LinkedList<Position> snakePositions = new LinkedList<Position>();
        private readonly Random random = new Random();

        public GameState(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Grid = new GridValue[rows, cols];
            Dir = Direction.Right;

            AddSnake();
            AddFood();

        }

        private void AddSnake()//добавление змеи в сетку
        {
            int r = Rows / 2;

            for (int c = 0; c <= 3; c++)
            {
                Grid[r, c] = GridValue.Snake;
                snakePositions.AddFirst(new Position(r, c));
            }
        }

        private IEnumerable<Position> EmptyPositions()//возвращает все пустые позиции
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (Grid[r, c] == GridValue.Empty)
                    {
                        yield return new Position(r, c);// возвращает элемент коллекции в итераторе и перемещает текущую позицию на следующий элемент.
                    }
                }
            }
        }

        private void AddFood()//добавление еды
        {
            List<Position> empty = new List<Position>(EmptyPositions());

            if (empty.Count == 0)//если нет пустых позиций возвращаем 
            {
                return;
            }

            Position pos = empty[random.Next(0, empty.Count)];
            Grid[pos.Row, pos.Col] = GridValue.Food;
        }
        public Position HeadPosition()//возвращает позицию головы
        {
            return snakePositions.First.Value;
        }
        public Position TailPosition()//возвращает позицию хвоста
        {
            return snakePositions.Last.Value;
        }
        public IEnumerable<Position> SnakePositions()//возвращает позицию змеи
        {
            return snakePositions;
        }

        private void AddHead(Position pos)//Добавляет новую позицию к передней части змеи делая ее головой
        {
            snakePositions.AddFirst(pos);
            Grid[pos.Row, pos.Col] = GridValue.Snake;//добавляем эту запись в массив
        }

        private void RemoveTail()
        {
            Position tail = snakePositions.Last.Value;//получаю позицию хвоста
            Grid[tail.Row, tail.Col] = GridValue.Empty;//делаю позицию пустой в массиве
            snakePositions.RemoveLast();
        }

        private Direction GetLastDirection()
        {
            if (dirChanges.Count == 0)
            {
                return Dir;
            }

            return dirChanges.Last.Value;
        }

        private bool CanChangeDirection(Direction newDir)
        {
            if(dirChanges.Count == 2)
            {
                return false;
            }

            Direction lastDir= GetLastDirection();
            return newDir != lastDir && newDir != lastDir.Opposite();
        }

        public void ChangeDirection(Direction dir)//меняет направление змеи
        {
            //если может менять направление
            if (CanChangeDirection(dir))
            {
                dirChanges.AddLast(dir);
            }
            
        }
        private bool OutsideGird(Position pos)//находится ли за сеткой
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col >= Cols;
        }

        private GridValue WillHit(Position newHeadPos)//куда ударится
        {
            if (OutsideGird(newHeadPos))//если за пределами сетки
            {
                return GridValue.Outside;
            }

            if (newHeadPos == TailPosition())//совпадают ли позиции хвоста и головы
            {
                return GridValue.Empty;
            }

            return Grid[newHeadPos.Row, newHeadPos.Col];
        }

        public void Move()
        {
            if (dirChanges.Count > 0)
            {
                Dir = dirChanges.First.Value;
                dirChanges.RemoveFirst();
            }

            Position newHeadPos = HeadPosition().Translate(Dir);//получ новое положение головы
            GridValue hit = WillHit(newHeadPos);
            if (hit == GridValue.Outside || hit == GridValue.Snake) 
            {
                GameOver = true;
            }
            else if (hit == GridValue.Empty) 
            {
                RemoveTail();
                AddHead(newHeadPos);
            }
            else if(hit == GridValue.Food)
            {
                AddHead(newHeadPos);
                Score++;
                AddFood();
            }
        }
    }
}
