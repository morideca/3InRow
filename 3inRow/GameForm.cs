using _3inRow;
using System;

using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace _3inRow
{
    public enum Colors
    {
        None,
        Yellow,
        Black,
        Red,
        Green,
        Blue,
        Bomb,
        lineVertical,
        lineHorizontal,
    }

    public enum Type
    {
        none,
        element,
        lineVertical,
        lineHorizontal,
        bomb
    }

    public partial class GameForm : Form
    {
        private const int rows = 8;
        private const int columns = 8;
        private const int cellSize = 50;

        private int score = 0;
        private int topBorder;
        private int rightBorder;
        private int bottomBorder;
        private int leftBorder;
        private int arrowSpeed = 10;
        private int remainingTime = 60;

        private bool isAnimating = false;
        private bool isBomb = false;
        private bool isLine = false;
        private bool afterBonus = false;
        private bool crossMatch = false;
        private bool afterSwap = false;

        private Cell selectedCell;
        private Cell lastClickedCell;
        private Cell tempSelectedCell;
        private Cell tempLastClickedCell;

        List<Cell> matchedCells = new List<Cell>();
        List<Cell> allMatchedCells = new List<Cell>();

        Cell[,] cells = new Cell[rows, columns];

        private Point cellIndexForBomb;
        private Point cellIndexForLine;

        private Label scoreLabel;
        private Label timerLabel;

        private Timer gameTimer;

        private Random rand = new Random();

        public GameForm()
        {
            InitializeComponent();
            InitializeGame();
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;

            scoreLabel = new Label();
            scoreLabel.AutoSize = false;
            scoreLabel.Size = new Size(200, 40);
            scoreLabel.Font = new Font("Arial", 16, FontStyle.Bold);
            scoreLabel.Location = new Point(600, 60);
            this.Controls.Add(scoreLabel);
            UpdateScoreDisplay();

            gameTimer = new Timer();
            gameTimer.Interval = 1000;
            gameTimer.Tick += GameTimer_Tick;

            timerLabel = new Label();
            timerLabel.Text = $"Time left: {remainingTime} seconds";
            timerLabel.AutoSize = false;
            timerLabel.Size = new Size(200, 40);
            timerLabel.Location = new Point(600, 100);
            timerLabel.Font = new Font("Arial", 16, FontStyle.Bold);
            timerLabel.ForeColor = Color.Black;
            UpdateTimerDisplay();
            this.Controls.Add(timerLabel);

            rightBorder = 10;
            bottomBorder = 10;
            leftBorder = 10 + cellSize * columns;
            topBorder = 10 + cellSize * rows;

            gameTimer.Start();
        }

        public void InitializeGame()
        {
            GenerateBoard();

        }

        private void GenerateBoard()
        {
            int i = 1;
            int k = 1;
            for (int x = 0; x < rows; x++)
            {
                k++;
                if (k == 6) k = 3;
                int j = 1;
                for (int y = 0; y < columns; y++)
                {
                    cells[x, y] = new Cell((Colors)k, Type.element);
                    k++;
                    if (k == 6) k = 1;

                    this.Controls.Add(cells[x, y].pictureBox);
                    cells[x, y].SetCellPosition(10 + i * cellSize, 10 + j * cellSize);
                    cells[x, y].pictureBox.Click += CellClick;
                    cells[x, y].FinishedMove += CheckForMatches;
                    cells[x, y].NewIndex(x, y, cells[x, y]);
                    this.ResizeRedraw = true;
                    j++;
                }
                i++;
            }
            UpdateAllCellImage();
        }

        private void UpdateCellImage(Cell cell)
        {
            switch (cell._color)
            {
                case Colors.None:
                    cell.pictureBox.Image = null;
                    break;
                case Colors.Yellow:
                    cell.pictureBox.Image = Properties.Resources.Yellow;
                    break;
                case Colors.Black:
                    cell.pictureBox.Image = Properties.Resources.Black;
                    break;
                case Colors.Red:
                    cell.pictureBox.Image = Properties.Resources.Red;
                    break;
                case Colors.Green:
                    cell.pictureBox.Image = Properties.Resources.Green;
                    break;
                case Colors.Blue:
                    cell.pictureBox.Image = Properties.Resources.Blue;
                    break;
                case Colors.Bomb:
                    cell.pictureBox.Image = Properties.Resources.Bomb;
                    break;
                case Colors.lineHorizontal:
                    cell.pictureBox.Image = Properties.Resources.LineHorizontal;
                    break;
                case Colors.lineVertical:
                    cell.pictureBox.Image = Properties.Resources.LineVertical;
                    break;
            }
        }

        private void UpdateAllCellImage()
        {
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    if (cells[row, col] != null)
                    {
                        switch (cells[row, col]._color)
                        {
                            case Colors.None:
                                cells[row, col].pictureBox.Image = null;
                                break;
                            case Colors.Yellow:
                                cells[row, col].pictureBox.Image = Properties.Resources.Yellow;
                                break;
                            case Colors.Black:
                                cells[row, col].pictureBox.Image = Properties.Resources.Black;
                                break;
                            case Colors.Red:
                                cells[row, col].pictureBox.Image = Properties.Resources.Red;
                                break;
                            case Colors.Green:
                                cells[row, col].pictureBox.Image = Properties.Resources.Green;
                                break;
                            case Colors.Blue:
                                cells[row, col].pictureBox.Image = Properties.Resources.Blue;
                                break;
                            case Colors.Bomb:
                                cells[row, col].pictureBox.Image = Properties.Resources.Bomb;
                                break;
                            case Colors.lineHorizontal:
                                cells[row, col].pictureBox.Image = Properties.Resources.LineHorizontal;
                                break;
                            case Colors.lineVertical:
                                cells[row, col].pictureBox.Image = Properties.Resources.LineVertical;
                                break;
                        }
                    }
                }
            }
        }

        private void CellClick(object sender, EventArgs e)
        {
            if (isAnimating == false)
            {
                PictureBox clickedCell = sender as PictureBox;

                Point indexClickedCell = GetCellIndex(clickedCell);

                if (cells[indexClickedCell.X, indexClickedCell.Y]._color == Colors.Bomb)
                {
                    Boom(cells[indexClickedCell.X, indexClickedCell.Y]);
                }
                else if (cells[indexClickedCell.X, indexClickedCell.Y]._color == Colors.lineVertical)
                {
                    LineBoom(cells[indexClickedCell.X, indexClickedCell.Y], Type.lineVertical);
                }
                else if (cells[indexClickedCell.X, indexClickedCell.Y]._color == Colors.lineHorizontal)
                {
                    LineBoom(cells[indexClickedCell.X, indexClickedCell.Y], Type.lineHorizontal);
                }
                else
                {

                    if (selectedCell == null)
                    {
                        selectedCell = cells[indexClickedCell.X, indexClickedCell.Y];
                        tempSelectedCell = selectedCell;
                        HighlightCell(selectedCell.pictureBox);
                    }
                    else
                    {
                        lastClickedCell = cells[indexClickedCell.X, indexClickedCell.Y];
                        tempLastClickedCell = lastClickedCell;

                        Point indexSelectedCell = GetCellIndex(selectedCell);
                        Point indexLastClickedCell = GetCellIndex(lastClickedCell);

                        cellIndexForBomb = indexLastClickedCell;

                        int cell1X = indexSelectedCell.X;
                        int cell1Y = indexSelectedCell.Y;

                        int cell2Y = indexLastClickedCell.Y;
                        int cell2X = indexLastClickedCell.X;

                        if ((Math.Abs(cell2X - cell1X) == 1 && cell1Y == cell2Y) ||
                            (Math.Abs(cell2Y - cell1Y) == 1 && cell1X == cell2X))
                        {
                            ClearSelection();

                            selectedCell = null;
                            lastClickedCell = null;

                            SwapCells(tempSelectedCell, tempLastClickedCell, false);
                        }
                        else
                        {
                            ClearSelection();

                            selectedCell = null;
                            lastClickedCell = null;
                        }
                    }
                }
            }
        }

        private async void SwapCells(Cell cell2, Cell cell1, bool swapBack)
        {
            int tempCell1X = cell1.IndexX;
            int tempCell1Y = cell1.IndexY;
            int tempCell2X = cell2.IndexX;
            int tempCell2Y = cell2.IndexY;

            cell1.NewIndex(tempCell2X, tempCell2Y, cell1);
            cell2.NewIndex(tempCell1X, tempCell1Y, cell2);

            cells[cell1.IndexX, cell1.IndexY] = cell1;
            cells[cell2.IndexX, cell2.IndexY] = cell2;

            isAnimating = true;

            Task moveCell1 = cell1.moveCell(cell2);
            Task moveCell2 = cell2.moveCell(cell1);

            await Task.WhenAll(moveCell1, moveCell2);

            isAnimating = false;

            if (swapBack == false)
            {
                afterSwap = true;
                CheckForMatches(cell1);
                CheckForMatches(cell2);
                if (allMatchedCells.Count == 0)
                {
                    SwapCells(tempSelectedCell, tempLastClickedCell, true);
                    return;
                }
                DestroyAllMatchedCells();
            }
        }

        private void ClearSelection()
        {
            selectedCell.pictureBox.BorderStyle = BorderStyle.None;
        }


        private void HighlightCell(PictureBox cell)
        {
            cell.BorderStyle = BorderStyle.FixedSingle;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void CheckForMatches(Cell cell)
        {
            int x = GetCellIndex(cell).X;
            int y = GetCellIndex(cell).Y;

            matchedCells.Add(cell);

            Colors currentColor = cell._color;
            //по вертикали
            for (int i = 1; i + y < rows; i++)
            {
                if (cells[x, y + i] != null && cells[x, y + i]._color == currentColor)
                {
                    if (!matchedCells.Contains(cells[x, y + i])) matchedCells.Add(cells[x, y + i]);
                }
                else break;
            }
            for (int i = 1; y - i >= 0; i++)
            {
                if (cells[x, y - i] != null && cells[x, y - i]._color == currentColor)
                {
                    if (!matchedCells.Contains(cells[x, y - i])) matchedCells.Add(cells[x, y - i]);
                }
                else break;
            }
            if (matchedCells.Count >= 3)
            {
                allMatchedCells.AddRange(matchedCells);
                crossMatch = true;
            }
            if (matchedCells.Count == 4)
            {
                isLine = true;
                SetBonuseCellIndex(matchedCells, cell, Type.lineVertical);
            }
            else if (matchedCells.Count >= 5)
            {
                isBomb = true;
                SetBonuseCellIndex(matchedCells, cell, Type.bomb);
            }
            matchedCells.Clear();

            matchedCells.Add(cell);
            //по горизонтали
            for (int i = 1; i + x < columns; i++)
            {
                if (cells[x + i, y] != null && cells[x + i, y]._color == currentColor)
                {

                    if (!matchedCells.Contains(cells[x + i, y])) matchedCells.Add(cells[x + i, y]);
                }
                else break;
            }
            for (int i = 1; x - i >= 0; i++)
            {
                if (cells[x - i, y] != null && cells[x - i, y]._color == currentColor)
                {
                    if (!matchedCells.Contains(cells[x - i, y])) matchedCells.Add(cells[x - i, y]);
                }
                else break;
            }
            if (matchedCells.Count >= 3)
            {
                allMatchedCells.AddRange(matchedCells);
            }
            else crossMatch = false;
            if (matchedCells.Count == 4)
            {
                isLine = true;
                SetBonuseCellIndex(matchedCells, cell, Type.lineVertical);
            }
            else if (matchedCells.Count >= 5)
            {
                isBomb = true;
                SetBonuseCellIndex(matchedCells, cell, Type.bomb);
            }
            if (crossMatch) SetBombIndex(cell);
            matchedCells.Clear();
        }

        private void SetBonuseCellIndex(List<Cell> matchedCells, Cell mainCell, Type type)
        {
            if (afterSwap)
            {
                foreach (var cell in matchedCells)
                {
                    if (mainCell == cell)
                    {
                        if (type == Type.bomb) cellIndexForBomb = GetCellIndex(tempSelectedCell);
                        else if (type == Type.lineVertical) cellIndexForLine = GetCellIndex(tempSelectedCell);
                    }
                }
                afterSwap = false;

            }
            else 
            {
                if (type == Type.bomb) cellIndexForBomb = GetCellIndex(mainCell);
                else if (type == Type.lineVertical) cellIndexForLine = GetCellIndex(mainCell);

            }
        }
        private void SetBombIndex(Cell cell)
        {
            cellIndexForBomb = GetCellIndex(cell);
            isBomb = true;
        }

        public Point GetCellIndex(PictureBox cell)
        {
            Cell _cell = cell.Tag as Cell;
            return new Point(_cell.IndexX, _cell.IndexY);
        }

        public Point GetCellIndex(Cell cell)
        {
            int x = cell.IndexX;
            int y = cell.IndexY;
            return new Point(x, y);
        }

        private void DestroyAllMatchedCells()
        {
            foreach (var cell in allMatchedCells)
            {
                DestroyCell(cell);
            }
            ShiftDown();
            allMatchedCells.Clear();
        }

        private void DestroyCell(Cell cell)
        {
            IncreaseScore(10);
            if (cell._color == Colors.Bomb) Boom(cell);
            else if (cell._color == Colors.lineVertical) LineBoom(cell, Type.lineVertical);
            else if (cell._color == Colors.lineHorizontal) LineBoom(cell, Type.lineHorizontal);
            else
            {
                int indexX = cell.IndexX;
                int indexY = cell.IndexY;

                if (cell != null)
                {
                    this.Controls.Remove(cell.pictureBox);
                    cell.pictureBox.Dispose();
                    cells[indexX, indexY] = null;
                }
            }
        }

        private void DestroyBonus(Cell cell)
        {
            IncreaseScore(10);
            int indexX = cell.IndexX;
            int indexY = cell.IndexY;

            if (cell != null)
            {
                this.Controls.Remove(cell.pictureBox);
                cell.pictureBox.Dispose();
                cells[indexX, indexY] = null;
            }
        }

        private async void ShiftDown()
        {

            bool again = true;

            List<Task> tasks = new List<Task>();

            if (!afterBonus && isBomb) AddBonus(Type.bomb);
            else if (!afterBonus && isLine) AddBonus((Type)rand.Next(2, 4));
            afterBonus = false;

            while (again)
            {
                int emptyCells = 0;
                for (int c = 0; c < columns; c++)
                {
                    for (int r = rows - 2; r >= 0; r--)
                    {
                        int stepsForCellMoveDown = 0;

                        if (cells[c, r] != null)
                        {
                            for (int i = 1; i + r < rows; i++)
                            {
                                if (cells[c, r + i] == null)
                                {
                                    stepsForCellMoveDown++;
                                    emptyCells++;
                                }
                                else break;
                            }
                            if (stepsForCellMoveDown > 0)
                            {
                                isAnimating = true;
                                Task moving = cells[c, r].MoveCell(stepsForCellMoveDown);
                                tasks.Add(moving);
                                cells[c, r + stepsForCellMoveDown] = cells[c, r];
                                cells[c, r] = null;
                                cells[c, r + stepsForCellMoveDown].NewIndex(c, r + stepsForCellMoveDown, cells[c, r + stepsForCellMoveDown]);
                            }
                        }
                    }
                }
                if (emptyCells == 0)
                {
                    again = false;
                }
            }
            await Task.WhenAll(tasks);
            await Task.Delay(100);
            isAnimating = false;

            AddNewCells();
        }

        private void AddBonus(Type type)
        {
            int i = 0;
            int j = 0;
            if (type == Type.bomb)
            {
                i = cellIndexForBomb.X;
                j = cellIndexForBomb.Y;
                cells[i, j] = new Cell(Colors.Bomb, Type.bomb);
                isBomb = false;
            }
            else if (type == Type.lineVertical || type == Type.lineHorizontal)
            {
                i = cellIndexForLine.X;
                j = cellIndexForLine.Y;
                if (type == Type.lineVertical)
                {
                    if (cells[i, j] != null) cells[i, j] = null;
                    cells[i, j] = new Cell(Colors.lineVertical, Type.lineVertical);
                }
                else
                {
                    if (cells[i, j] != null) cells[i, j] = null;
                    cells[i, j] = new Cell(Colors.lineHorizontal, Type.lineVertical);
                }
                isLine = false;
            }

            this.Controls.Add(cells[i, j].pictureBox);
            cells[i, j].pictureBox.BringToFront();
            cells[i, j].SetCellPosition(10 + (i + 1) * cellSize, 10 + (j + 1) * cellSize);
            cells[i, j].NewIndex(i, j, cells[i, j]);
            cells[i, j].pictureBox.Click += CellClick;
            this.ResizeRedraw = true;
            UpdateCellImage(cells[i, j]);

        }

        private async void AddNewCells()
        {
            Random rand = new Random();
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    if (cells[i, j] == null)
                    {
                        cells[i, j] = new Cell((Colors)rand.Next(1, 6), Type.element);
                        this.Controls.Add(cells[i, j].pictureBox);
                        cells[i, j].pictureBox.BringToFront();
                        cells[i, j].SetCellPosition(10 + (i + 1) * cellSize, 10 + (j + 1) * cellSize);
                        cells[i, j].NewIndex(i, j, cells[i, j]);
                        cells[i, j].pictureBox.Click += CellClick;
                        cells[i, j].FinishedMove += CheckForMatches;
                        this.ResizeRedraw = true;
                        UpdateCellImage(cells[i, j]);
                        CheckForMatches(cells[i, j]);
                    }
                }
            }
            await Task.Delay(250);

            if (allMatchedCells.Count > 0)
            {
                DestroyAllMatchedCells();
                allMatchedCells.Clear();
            }
        }

        private async void Boom(Cell cell)
        {
            int x = cell.IndexX;
            int y = cell.IndexY;
            int i = 0;
            int j = 0;
            List<Cell> targetCells = new List<Cell>();

            i = x - 1;
            j = y - 1;
            if (i < columns && i >= 0 && j < columns && j >= 0 && cells[i, j] != null)
            {
                targetCells.Add(cells[i, j]);
            }

            i = x;
            j = y - 1;
            if (i < columns && i >= 0 && j < columns && j >= 0 && cells[i, j] != null)
            {
                targetCells.Add(cells[i, j]);
            }

            i = x + 1;
            j = y - 1;
            if (i < columns && i >= 0 && j < columns && j >= 0 && cells[i, j] != null)
            {
                targetCells.Add(cells[i, j]);
            }

            i = x + 1;
            j = y;
            if (i < columns && i >= 0 && j < columns && j >= 0 && cells[i, j] != null)
            {
                targetCells.Add(cells[i, j]);
            }

            i = x + 1;
            j = y + 1;
            if (i < columns && i >= 0 && j < columns && j >= 0 && cells[i, j] != null)
            {
                targetCells.Add(cells[i, j]);
            }

            i = x;
            j = y + 1;
            if (i < columns && i >= 0 && j < columns && j >= 0 && cells[i, j] != null)
            {
                targetCells.Add(cells[i, j]);
            }

            i = x - 1;
            j = y + 1;
            if (i < columns && i >= 0 && j < columns && j >= 0 && cells[i, j] != null)
            {
                targetCells.Add(cells[i, j]);
            }

            i = x - 1;
            j = y;
            if (i < columns && i >= 0 && j < columns && j >= 0 && cells[i, j] != null)
            {
                targetCells.Add(cells[i, j]);
            }

            DestroyBonus(cell);


            await Task.Delay(250);

            bool hereIsAnotherBonus = false;

            foreach (var targetCell in targetCells)
            {
                if (targetCell.Type == Type.bomb || targetCell.Type == Type.lineVertical ||
                    targetCell.Type == Type.lineHorizontal) hereIsAnotherBonus = true;
                DestroyCell(targetCell);
            }
            if (hereIsAnotherBonus)
            {
                hereIsAnotherBonus = false;
                return;
            }

            afterBonus = true;
            ShiftDown();
        }

        private async void LineBoom(Cell cell, Type type)
        {
            Point tempLocation = cell.pictureBox.Location;

            DestroyBonus(cell);

            if (type == Type.lineVertical)
            {
                PictureBox upArrow = new PictureBox();
                upArrow.Size = new Size(25, 50);
                upArrow.SizeMode = PictureBoxSizeMode.StretchImage;
                upArrow.Image = Properties.Resources.Up;
                this.Controls.Add(upArrow);
                upArrow.BringToFront();
                upArrow.Location = new Point (tempLocation.X + 13, tempLocation.Y);

                Task up = LineArrowMove(upArrow, 0);

                PictureBox downArrow = new PictureBox();
                downArrow.Size = new Size(25, 50);
                downArrow.SizeMode = PictureBoxSizeMode.StretchImage;
                downArrow.Image = Properties.Resources.Down;
                this.Controls.Add(downArrow);
                downArrow.BringToFront();
                downArrow.Location = new Point(tempLocation.X + 13, tempLocation.Y); 
                Task down = LineArrowMove(downArrow, 1);
                isAnimating = true;
                await Task.WhenAll(up, down);
                isAnimating = false;
            }

            if (type == Type.lineHorizontal)
            {
                PictureBox leftArrow = new PictureBox();
                leftArrow.Size = new Size(cellSize, cellSize / 2);
                leftArrow.SizeMode = PictureBoxSizeMode.StretchImage;
                leftArrow.Image = Properties.Resources.Left;
                this.Controls.Add(leftArrow);
                leftArrow.BringToFront();
                leftArrow.Location = new Point(tempLocation.X - 25, tempLocation.Y + 13);
                Task left = LineArrowMove(leftArrow, 2);

                PictureBox rightArrow = new PictureBox();
                rightArrow.Size = new Size(cellSize, cellSize / 2);
                rightArrow.SizeMode = PictureBoxSizeMode.StretchImage;
                rightArrow.Image = Properties.Resources.Right;
                this.Controls.Add(rightArrow);
                rightArrow.BringToFront();
                rightArrow.Location = new Point(tempLocation.X + 25, tempLocation.Y + 13);
                Task right = LineArrowMove(rightArrow, 3);

                isAnimating = true;
                await Task.WhenAll(right, left);
                isAnimating = false;
            }

            if (!isAnimating) ShiftDown();

        }

        private async Task LineArrowMove(PictureBox arrow, int x)
        {
            switch (x)
            {
                case(0):

                    while (arrow.Location.Y > bottomBorder)
                    {
                        Point newPosition = new Point(arrow.Location.X, arrow.Location.Y - arrowSpeed);
                        arrow.Location = newPosition;

                        Cell intersectionCell = IntersectionCell(arrow.Bounds);
                        if (intersectionCell != null) DestroyCell(intersectionCell);

                        await Task.Delay(12);
                    }
                    this.Controls.Remove(arrow);
                    arrow.Dispose();

                    break;
                case(1):

                    while (arrow.Location.Y < topBorder)
                    {
                        Point newPosition = new Point(arrow.Location.X, arrow.Location.Y + arrowSpeed);
                        arrow.Location = newPosition;

                        Cell intersectionCell = IntersectionCell(arrow.Bounds);
                        if (intersectionCell != null) DestroyCell(intersectionCell);

                        await Task.Delay(12);
                    }
                    this.Controls.Remove(arrow);
                    arrow.Dispose();

                    break;
                case(2):

                    while (arrow.Location.X > rightBorder)
                    {
                        Point newPosition = new Point(arrow.Location.X - arrowSpeed, arrow.Location.Y);
                        arrow.Location = newPosition;

                        Cell intersectionCell = IntersectionCell(arrow.Bounds);
                        if (intersectionCell != null) DestroyCell(intersectionCell);

                        await Task.Delay(12);
                    }
                    this.Controls.Remove(arrow);
                    arrow.Dispose();

                    break;
                case(3):
                    while (arrow.Location.X < leftBorder)
                    {
                        Point newPosition = new Point(arrow.Location.X + arrowSpeed, arrow.Location.Y);
                        arrow.Location = newPosition;

                        Cell intersectionCell = IntersectionCell(arrow.Bounds);
                        if (intersectionCell != null) DestroyCell(intersectionCell);

                        await Task.Delay(12);
                    }
                    this.Controls.Remove(arrow);
                    arrow.Dispose();

                    break;
            }
        }

        private Cell IntersectionCell(Rectangle rectArrow)
        {
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    if (cells[i, j] != null)
                    {
                        Rectangle rectCell = cells[i, j].pictureBox.Bounds;
                        if (rectCell.IntersectsWith(rectArrow)) return cells[i, j];
                    }
                }
            }
            return null;
        }
        
        private void IncreaseScore(int points)
        {
            score += points;
            UpdateScoreDisplay();
        }

        private void UpdateScoreDisplay()
        {
            scoreLabel.Text = $"Score: {score}";
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            remainingTime--;

            if (remainingTime <= 0)
            {
                gameTimer.Stop(); 
                EndGame(); 
            }

            UpdateTimerDisplay(); 
        }

        private void UpdateTimerDisplay()
        {
            timerLabel.Text = $"Time: {remainingTime}s";
        }

        private void EndGame()
        {
            MessageBox.Show($"Game Over! Final Score: {score}", "Game Over", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            MainMenuForm menu = new MainMenuForm();
            menu.Show();
            this.Hide();

        }
    }

    public class Cell
    {
        public Colors _color { get; set; }
        public Type Type { get; set; }

        public bool IsMoving { get; set; } = false;

        public int IndexX { get; set; }
        public int IndexY { get; set; }

        private int cellSize = 50;
        private int animationSteps = 10;
        private int animationInterval = 1;

        public delegate void FinishedMoveHandler(Cell cell);
        public event FinishedMoveHandler FinishedMove;

        public PictureBox pictureBox { get; set; }

        public Cell(Colors color, Type type)
        {
            pictureBox = new PictureBox();
            _color = color;
            Type = type;
        }

        public void NewIndex(int x, int y, Cell cell)
        {
            IndexX = x;
            IndexY = y;
            pictureBox.Tag = cell;
        }

        public void SetCellPosition(int x, int y)
        {
            pictureBox.Image = Properties.Resources.Yellow;
            pictureBox.BorderStyle = BorderStyle.None;
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.Margin = new Padding(0);
            pictureBox.Padding = new Padding(0);
            pictureBox.Size = new Size(50, 50);
            pictureBox.Location = new Point(x, y);
        }

        public async Task moveCell(Cell targetPoint)
        {
            Point tempTargetPoint = targetPoint.pictureBox.Location;

            int currentX = pictureBox.Location.X;
            int currentY = pictureBox.Location.Y;

            int targetX = tempTargetPoint.X;
            int targetY = tempTargetPoint.Y;

            for (int i = 0; i < animationSteps; i++)

            {
                Point currentPoint = new Point(currentX + (targetX - currentX) * i / animationSteps,
                    currentY + (targetY - currentY) * i / animationSteps);

                pictureBox.Location = currentPoint;

                await Task.Delay(animationInterval);
            }

            pictureBox.Location = tempTargetPoint;
 
            FinishedMove?.Invoke(this);
        }

        public async Task MoveCell(int steps)
        {
            int cell1Y = pictureBox.Location.Y;
            int cell2Y = pictureBox.Location.Y + cellSize * steps;

            for (int i = 0; i < animationSteps; i++)
            {
                Point currentPoint1 = new Point(pictureBox.Location.X, cell1Y + (cell2Y - cell1Y) * i / animationSteps);

                pictureBox.Location = currentPoint1;

                await Task.Delay(animationInterval);
            }
            pictureBox.Location = new Point(pictureBox.Location.X, cell2Y);
            FinishedMove?.Invoke(this);
        }
    }
}
