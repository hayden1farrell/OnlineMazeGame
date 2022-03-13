using System.Collections;

namespace OnlineMazeGame;

struct directionMove
{
    public int X;
    public int Y;
}

class Cell
{
    public Dictionary<Char, bool> walls = new Dictionary<Char, bool> { { 'N', true }, { 'E', true }, { 'S', true }, { 'W', true } };
    public bool visited = false;
    public int number = 0;
    public int X = 0;
    public int Y = 0;

    public Dictionary<Char, bool> HasWalls()
    {
        return walls;
    }

    public void RemoveWall(int changeInX, int changeInY)
    {
        if (changeInX == 1) walls['W'] = false;
        else if (changeInX == -1) walls['E'] = false;
        else if (changeInY == 1) walls['N'] = false;
        else if (changeInY == -1) walls['S'] = false;
    }
    public void RemoveOldWall(int changeInX, int changeInY)
    {
        if (changeInX == 1) walls['E'] = false;
        else if (changeInX == -1) walls['W'] = false;
        else if (changeInY == 1) walls['S'] = false;
        else if (changeInY == -1) walls['N'] = false;
    }
}

class  MazeGeneration
{
    public static int[,] CreateMaze()
    {
        //arrays to hold maze
        int mazewidth = 8;
        // maze data a 1 holds  wall
        Cell[,] cells = SetUpCells(mazewidth);
        return MazeGen(cells, mazewidth);
    }
    
    static Cell[,] SetUpCells(int mazex)
    {
        Cell[,] cells = new Cell[mazex, mazex];

        for (int i = 0; i < mazex; i++)
        {
            for (int j = 0; j < mazex; j++)
            {
                cells[i, j] = new Cell();
                cells[i, j].X = i;
                cells[i, j].Y = j;
            }
        }

        return cells;
    }
    
    static int[,] MazeGen(Cell[,] cells, int mazex)
    {
        Random rng = new Random();
        Stack path = new Stack();

        int currentX = 0;
        int currentY = 0;
        cells[currentX, currentY].visited = true;
        cells[currentX, currentY].number = 1;
        path.Push(cells[currentX, currentY]);

        while (unVisited(cells, mazex) == true)
        {
            Cell currentCell = cells[currentX, currentY];

            List<Cell> neighbors = GetNeighbor(currentCell, cells);
            if (neighbors.Count != 0)
            {
                int chosen = rng.Next(neighbors.Count);
                Cell nextCell = neighbors[chosen];
                currentCell.visited = true;
                currentCell.number = 1;
                nextCell.visited = true;
                nextCell.number = 1;

                cells = KnockDownWall(nextCell, currentCell, cells);
                currentX = nextCell.X;
                currentY = nextCell.Y;
                path.Push(nextCell);
            }
            else
            {
                path.Pop();
                currentCell = (Cell)path.Peek();
                currentX = currentCell.X;
                currentY = currentCell.Y;
            }
        }

        return ConverToDisplayArray(cells);
    }
    
    static List<Cell> GetNeighbor(Cell currentCell, Cell[,] cells)
    {
        List<Cell> neighbors = new List<Cell>();
        int currentX = currentCell.X;
        int currentY = currentCell.Y;

        try { if (cells[currentX + 1, currentY].visited == false) neighbors.Add(cells[currentX + 1, currentY]); } catch { };
        try { if (cells[currentX - 1, currentY].visited == false) neighbors.Add(cells[currentX - 1, currentY]); } catch { };
        try { if (cells[currentX, currentY + 1].visited == false) neighbors.Add(cells[currentX, currentY + 1]); } catch { };
        try { if (cells[currentX, currentY - 1].visited == false) neighbors.Add(cells[currentX, currentY - 1]); } catch { };

        return neighbors;
    }
    static bool unVisited(Cell[,] cells, int mazex)
    {
        for (int i = 0; i < mazex; i++)
        {
            for (int j = 0; j < mazex; j++)
            {
                if (cells[i, j].visited == false)
                    return true;
            }
        }
        return false;
    }
    
    private static Cell[,] KnockDownWall(Cell nextCell, Cell currentCell, Cell[,] cells)
    {
        int changeInX = nextCell.X - currentCell.X;
        int changeInY = nextCell.Y - currentCell.Y;

        currentCell.RemoveOldWall(changeInX, changeInY);
        nextCell.RemoveWall(changeInX, changeInY);

        cells[nextCell.X, nextCell.Y] = nextCell;
        cells[currentCell.X, currentCell.Y] = currentCell;

        return cells;
    }
    
    private static int[,] ConverToDisplayArray(Cell[,] cells)
    {
        int[,] Display = new int[cells.GetLength(0) * 3, cells.GetLength(1) * 3];

        for (int i = 1; i <= cells.GetLength(0); i++)
        {
            for (int j = 1; j <= cells.GetLength(1); j++)
            {
                Display[(i * 3) - 2, (j * 3) - 2] = cells[i - 1, j - 1].number;

                Dictionary<Char, bool> walls = cells[i - 1, j - 1].HasWalls();

                foreach (var wall in walls)
                {
                    if (wall.Value == false)
                    {
                        char Open = wall.Key;
                        if (Open == 'N')
                            Display[(j * 3) - 3, (i * 3) - 2] = 1;
                        if (Open == 'S')
                            Display[(j * 3) - 1, (i * 3) - 2] = 1;
                        if (Open == 'E')
                            Display[(j * 3) - 2, (i * 3) - 1] = 1;
                        if (Open == 'W')
                            Display[(j * 3) - 2, (i * 3) - 3] = 1;
                    }
                }
            }
        }

        return Display;
    }
}
public class Game
{
    public int[,]? maze;
    public void NewGame(Server.GameInfo? info, SimpleNet.Server? server)
    {
        maze = MazeGeneration.CreateMaze();
        
    }
}