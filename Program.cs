using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

// Code that generates rooms and connects them under rules with eachother. Area has entry and exit on bottom and top line and you can get from any room to exit or entreance.
// First matrix shows IDs of rooms, second is graphical showcase where "|" and "_" are walls and "/" and ",," are doors

namespace Room_Doors_generator
{
    internal class Program
    {

        class CQB_Cell
        {
            public int[] Walls = { 1, 1, 1, 1 }; // 0 - no wall , 1 - wall (default because of border walls), 2 - wall with door... {L, U, R, D}
            public int RoomID;
            public int X;
            public int Y;
            public List<char> NextRoomDir = new List<char>();
            public List<int> NextRoomID = new List<int>();
            public List<int> AlreadyNextId = new List<int>();
        }
        class CQB_Room
        {
            public int ID;
            public List<CQB_Cell> CQB_RoomCells = new List<CQB_Cell>();
            public List<CQB_Room> CQB_NextRoom = new List<CQB_Room>();
            public List<CQB_Cell> CQB_DoorCell = new List<CQB_Cell>();
            public List<int> ConnectedRoomIDs = new List<int>();
            public List<int> NextRoomIDs = new List<int>();
            public int CountConnectedRoom = 0;
            public List<int> TempNextID = new List<int>();
        }
        class CQB_Grid
        {
            public List<CQB_Room> CQB_Rooms = new List<CQB_Room>();

        }
        static void Init()
        {
            List<List<CQB_Cell>> FinalField = new List<List<CQB_Cell>>();
            CQB_Grid Grid = new CQB_Grid();
            int[,] Field = new int[5, 5];

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Field[i, j] = 0;
                }
            }

            RandomiseRoomsProto(Field);     //Works only with 2D array - Code Base
            FixField(Field);
            for (int i = 0; i < 5; i++)
            {
                for (int y = 0; y < 5; y++)
                {
                    Console.Write(Field[i, y] + " ");

                }
                Console.WriteLine();
            }
            bool check = false;
            bool check2 = true;
            while (check == false)
            {
                Idea(Field, Grid, check2);
                if (check2 == false)
                {
                    Grid = null;
                    CQB_Grid TempGrid = new CQB_Grid();
                    Grid = TempGrid;
                    continue;
                    check = true;
                }
                check = IsItOk(Field, Grid);
                if (check == false)
                {
                    Grid = null;
                    CQB_Grid TempGrid = new CQB_Grid();
                    Grid = TempGrid;
                }
            }

            FieldTo2DList(Field, Grid, FinalField);

            FixWalls(FinalField);

            Console.WriteLine();
            for (int i = 0; i < 5; i++)
            {
                if (FinalField[0][i].Walls[1] == 2)
                {
                    Console.Write(" ,,");
                }
                else
                    Console.Write(" __");


            }
            Console.WriteLine();
            for (int i = 0; i < 5; i++)
            {

                for (int j = 0; j < 5; j++)
                {
                    if (FinalField[i][j].Walls[0] == 1)
                        Console.Write('|');
                    else if (FinalField[i][j].Walls[0] == 2)
                        Console.Write('/');
                    else
                        Console.Write(' ');
                    if (FinalField[i][j].Walls[3] == 1)
                        Console.Write("__");
                    else if (FinalField[i][j].Walls[3] == 2)
                        Console.Write(",,");
                    else
                        Console.Write("  ");
                }
                Console.WriteLine('|');
            }


        }

        private static void FixWalls(List<List<CQB_Cell>> finalField)
        {
            for (int i = 0; i < 5; i++)
            {
                for (int y = 0; y < 5; y++)
                {
                    if (i != 4)
                    {
                        if (finalField[i][y].Walls[3] == 2)
                            finalField[i + 1][y].Walls[1] = 2;
                        if (finalField[i][y].RoomID == finalField[i + 1][y].RoomID)
                        {
                            finalField[i][y].Walls[3] = 0;
                            finalField[i + 1][y].Walls[1] = 0;
                        }
                    }
                    if (i != 0)
                    {
                        if (finalField[i][y].Walls[1] == 2)
                            finalField[i - 1][y].Walls[3] = 2;
                        if (finalField[i][y].RoomID == finalField[i - 1][y].RoomID)
                        {
                            finalField[i][y].Walls[1] = 0;
                            finalField[i - 1][y].Walls[3] = 0;
                        }
                    }
                    if (y != 4)
                    {
                        if (finalField[i][y].Walls[2] == 2)
                            finalField[i][y + 1].Walls[0] = 2;
                        if (finalField[i][y].RoomID == finalField[i][y + 1].RoomID)
                        {
                            finalField[i][y].Walls[2] = 0;
                            finalField[i][y + 1].Walls[0] = 0;
                        }
                    }
                    if (y != 0)
                    {
                        if (finalField[i][y].Walls[0] == 2)
                            finalField[i][y - 1].Walls[2] = 2;
                        if (finalField[i][y].RoomID == finalField[i][y - 1].RoomID)
                        {
                            finalField[i][y].Walls[0] = 0;
                            finalField[i][y - 1].Walls[2] = 0;
                        }
                    }


                }
            }
            Random rnd = new Random();
            finalField[4][rnd.Next(finalField[0].Count())].Walls[3] = 2;
            finalField[0][rnd.Next(finalField[0].Count())].Walls[1] = 2;
        }
        private static void FieldTo2DList(int[,] field, CQB_Grid grid, List<List<CQB_Cell>> finalField)
        {
            for (int i = 0; i < 5; i++)
            {
                List<CQB_Cell> tempo = new List<CQB_Cell>();
                for (int y = 0; y < 5; y++)
                {
                    CQB_Cell temp = new CQB_Cell();
                    tempo.Add(temp);
                }
                finalField.Add(tempo);
            }
            for (int i = 0; i < grid.CQB_Rooms.Count(); i++)
            {
                foreach (CQB_Cell cell in grid.CQB_Rooms[i].CQB_RoomCells)
                {
                    finalField[cell.X][cell.Y] = cell;
                }
            }
        }
        private static void FixField(int[,] field)
        {
            List<int> temp = new List<int>();
            int count = 0;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (temp.Contains(field[i, j]) == false)
                    {
                        temp.Add(field[i, j]);
                        count++;
                    }
                }
            }
            int max = temp.Max();
            temp.Clear();
            int space = 0;
            int tempint = 0;
            for (int x = 0; x < max; x++)
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int y = 0; y < 5; y++)
                    {
                        if (field[i, y] != x)
                        {
                            tempint++;
                        }
                    }
                }
                if (tempint == 25)
                {
                    for (int k = 0; k < 5; k++)
                    {
                        for (int l = 0; l < 5; l++)
                        {
                            if (field[k, l] > x)
                            {
                                field[k, l] -= 1;
                            }
                        }
                    }
                }
                tempint = 0;
            }

        }
        private static bool IsItOk(int[,] field, CQB_Grid grid)
        {
            bool check = true;
            for (int i = 0; i < grid.CQB_Rooms.Count(); i++)
            {
                if (grid.CQB_Rooms[i].ConnectedRoomIDs.Contains(grid.CQB_Rooms[i].ID) == true)
                {
                    check = false;
                }
            }
            foreach (CQB_Room room in grid.CQB_Rooms)
            {
                if (check == false)
                {
                    break;
                }
                List<int> tpm = new List<int>();
                for (int w = 0; w < room.ConnectedRoomIDs.Count(); w++)
                {

                    if (tpm.Contains(room.ConnectedRoomIDs[w]) == false)
                    {
                        tpm.Add(room.ConnectedRoomIDs[w]);
                    }
                    else
                    {
                        check = false;
                        break;
                    }
                }
                tpm.Clear();
                List<int> temp = new List<int>();
                tpm = temp;

            }
            if (check == false)
            {
                return false;
            }



            else
            {


                List<int> Current = new List<int>();
                List<int> Next = new List<int>();
                List<int> Final = new List<int>();
                Final.Add(0);
                for (int u = 0; u < grid.CQB_Rooms[0].ConnectedRoomIDs.Count(); u++)
                {
                    Current.Add(grid.CQB_Rooms[0].ConnectedRoomIDs[u]);
                }
                while (Current.Count() > 0)
                {
                    for (int i = 0; i < Current.Count(); i++)
                    {
                        if (Final.Contains(Current[i]) == false)
                        {
                            Final.Add(Current[i]);
                        }
                        foreach (int list in grid.CQB_Rooms[Current[i]].ConnectedRoomIDs)
                        {
                            if (Final.Contains(list) == false && Next.Contains(list) == false)
                            {
                                Next.Add(list);
                            }
                        }

                    }
                    Current.Clear();
                    Current.AddRange(Next);
                    Next.Clear();
                }
                if (Final.Count() == grid.CQB_Rooms.Count() && check == true)
                {
                    return true;
                }
                else { return false; }
            }
        }
        private static void Idea(int[,] field, CQB_Grid grid, bool check)
        {
            int well = 0;
            Random random = new Random();
            int temp = 0;
            foreach (int x in field)
            {
                if (x > temp)
                    temp = x;
            }
            for (int i = 0; i <= temp; i++)
            {
                CQB_Room tempR = new CQB_Room();
                tempR.ID = i;
                grid.CQB_Rooms.Add(tempR);
            }
            for (int i = 0; i < 5; i++)
            {
                for (int y = 0; y < 5; y++)
                {
                    CQB_Cell cell = new CQB_Cell();
                    cell.X = i;
                    cell.Y = y;
                    cell.RoomID = field[i, y];
                    if (i != 0 && field[i - 1, y] != field[i, y])
                    {
                        cell.NextRoomID.Add(field[i - 1, y]);
                    }
                    if (i != 4 && field[i + 1, y] != field[i, y])
                    {
                        cell.NextRoomID.Add(field[i + 1, y]);
                    }
                    if (y != 0 && field[i, y - 1] != field[i, y])
                    {
                        cell.NextRoomID.Add(field[i, y - 1]);
                    }
                    if (y != 4 && field[i, y + 1] != field[i, y])
                    {
                        cell.NextRoomID.Add(field[i, y + 1]);
                    }
                    grid.CQB_Rooms[field[i, y]].CQB_RoomCells.Add(cell);
                }
            }
            foreach (CQB_Room room in grid.CQB_Rooms)
            {
                for (int z = 0; z < room.CQB_RoomCells.Count(); z++)
                {
                    foreach (CQB_Cell cell in room.CQB_RoomCells)
                    {
                        for (int h = 0; h < cell.NextRoomID.Count(); h++)
                        {
                            if (room.NextRoomIDs.Contains(cell.NextRoomID[h]) == false)
                            {
                                room.NextRoomIDs.Add(cell.NextRoomID[h]);
                            }
                        }
                    }
                }
            }
            int DoorCount = 0;
            int sx = random.Next(5);
            int sy = random.Next(5);
            for (int ID = 0; ID < grid.CQB_Rooms.Count(); ID++)
            {
                if (well > 4000)
                {
                    check = false;
                    break;
                }
                grid.CQB_Rooms[ID].TempNextID = grid.CQB_Rooms[ID].NextRoomIDs;
                int rndCellID = random.Next(0, grid.CQB_Rooms[ID].CQB_RoomCells.Count());
                while (grid.CQB_Rooms[ID].ConnectedRoomIDs.Count() < 2)
                {
                    well++;
                    if (well > 4000)
                    {
                        check = false;
                        break;
                    }

                    if (grid.CQB_Rooms[ID].ConnectedRoomIDs.Count() >= 2)
                        break;
                    List<int> Idk = new List<int>();
                    int tr = 0;
                    for (int e = 0; e < grid.CQB_Rooms[ID].NextRoomIDs.Count(); e++)
                    {
                        Idk.Add(grid.CQB_Rooms[grid.CQB_Rooms[ID].NextRoomIDs[e]].ConnectedRoomIDs.Count());
                    }
                    if (Idk.Count() == 0 || Idk.Min() >= 2)
                    {
                        if (Idk.Count() == 0)
                        {
                            break;
                        }
                        int minindex = 0;
                        for (int w = 0; w < Idk.Count(); w++)
                        {
                            if (Idk[0] == Idk.Min())
                            {
                                minindex = w; break;
                            }
                        }
                        if (grid.CQB_Rooms[ID].ConnectedRoomIDs.Contains(grid.CQB_Rooms[minindex].ID) == true)
                        {
                            break;
                        }
                        grid.CQB_Rooms[ID].ConnectedRoomIDs.Add(grid.CQB_Rooms[ID].NextRoomIDs[minindex]);
                        grid.CQB_Rooms[grid.CQB_Rooms[ID].NextRoomIDs[minindex]].ConnectedRoomIDs.Add(ID);
                        List<char> finNei = new List<char>();
                        foreach (CQB_Cell cell in grid.CQB_Rooms[ID].CQB_RoomCells)
                        {
                            if (cell.X != 0 && field[cell.X - 1, cell.Y] == grid.CQB_Rooms[ID].NextRoomIDs[minindex])
                            {
                                cell.Walls[1] = 2;
                                foreach (CQB_Cell cell2 in grid.CQB_Rooms[grid.CQB_Rooms[ID].NextRoomIDs[minindex]].CQB_RoomCells)
                                {
                                    if (cell2.X == cell.X - 1 && cell2.Y == cell.Y)
                                    {
                                        cell2.Walls[3] = 2;
                                        break;
                                    }
                                }
                                break;
                            }
                            if (cell.X != 4 && field[cell.X + 1, cell.Y] == grid.CQB_Rooms[ID].NextRoomIDs[minindex])
                            {
                                cell.Walls[3] = 2;
                                foreach (CQB_Cell cell2 in grid.CQB_Rooms[grid.CQB_Rooms[ID].NextRoomIDs[minindex]].CQB_RoomCells)
                                {
                                    if (cell2.X == cell.X + 1 && cell2.Y == cell.Y)
                                    {
                                        cell2.Walls[1] = 2;
                                        break;
                                    }
                                }
                                break;
                            }
                            if (cell.Y != 0 && field[cell.X, cell.Y - 1] == grid.CQB_Rooms[ID].NextRoomIDs[minindex])
                            {
                                cell.Walls[0] = 2;
                                foreach (CQB_Cell cell2 in grid.CQB_Rooms[grid.CQB_Rooms[ID].NextRoomIDs[minindex]].CQB_RoomCells)
                                {
                                    if (cell2.X == cell.X && cell2.Y == cell.Y - 1)
                                    {
                                        cell2.Walls[2] = 2;
                                        break;
                                    }
                                }
                                break;
                            }
                            if (cell.Y != 4 && field[cell.X, cell.Y + 1] == grid.CQB_Rooms[ID].NextRoomIDs[minindex])
                            {
                                cell.Walls[2] = 2;
                                foreach (CQB_Cell cell2 in grid.CQB_Rooms[grid.CQB_Rooms[ID].NextRoomIDs[minindex]].CQB_RoomCells)
                                {
                                    if (cell2.X == cell.X && cell2.Y == cell.Y + 1)
                                    {
                                        cell2.Walls[0] = 2;
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                        break;
                    }
                    List<char> Dir = new List<char>();
                    int X = grid.CQB_Rooms[ID].CQB_RoomCells[random.Next(grid.CQB_Rooms[ID].CQB_RoomCells.Count())].X;
                    int Y = grid.CQB_Rooms[ID].CQB_RoomCells[random.Next(grid.CQB_Rooms[ID].CQB_RoomCells.Count())].Y;
                    if (X != 0 && field[X - 1, Y] != field[X, Y] && grid.CQB_Rooms[field[X - 1, Y]].ConnectedRoomIDs.Count() < 2)
                    {
                        Dir.Add('u');
                    }
                    if (X != 4 && field[X + 1, Y] != field[X, Y] && grid.CQB_Rooms[field[X + 1, Y]].ConnectedRoomIDs.Count() < 2)
                    {
                        Dir.Add('d');
                    }
                    if (Y != 0 && field[X, Y - 1] != field[X, Y] && grid.CQB_Rooms[field[X, Y - 1]].ConnectedRoomIDs.Count() < 2)
                    {
                        Dir.Add('l');
                    }
                    if (Y != 4 && field[X, Y + 1] != field[X, Y] && grid.CQB_Rooms[field[X, Y + 1]].ConnectedRoomIDs.Count() < 2)
                    {
                        Dir.Add('r');
                    }
                    if (Dir.Count() != 0)
                    {
                        char DefDir = Dir[random.Next(Dir.Count())];
                        if (DefDir == 'u')
                        {
                            if (grid.CQB_Rooms[field[X - 1, Y]].ConnectedRoomIDs.Contains(ID) == false)
                            {
                                grid.CQB_Rooms[ID].ConnectedRoomIDs.Add(field[X - 1, Y]);
                                grid.CQB_Rooms[field[X - 1, Y]].ConnectedRoomIDs.Add(ID);
                                foreach (CQB_Cell cell in grid.CQB_Rooms[ID].CQB_RoomCells)
                                {
                                    if (cell.X == X && cell.Y == Y)
                                    {
                                        cell.Walls[1] = 2;
                                    }
                                }
                                foreach (CQB_Cell cell in grid.CQB_Rooms[field[X - 1, Y]].CQB_RoomCells)
                                {
                                    if (cell.X == X - 1 && cell.Y == Y)
                                    {
                                        cell.Walls[3] = 2;
                                    }
                                }
                            }
                        }
                        if (DefDir == 'd')
                        {
                            if (grid.CQB_Rooms[field[X + 1, Y]].ConnectedRoomIDs.Contains(ID) == false)
                            {
                                grid.CQB_Rooms[ID].ConnectedRoomIDs.Add(field[X + 1, Y]);
                                grid.CQB_Rooms[field[X + 1, Y]].ConnectedRoomIDs.Add(ID);
                                foreach (CQB_Cell cell in grid.CQB_Rooms[ID].CQB_RoomCells)
                                {
                                    if (cell.X == X && cell.Y == Y)
                                    {
                                        cell.Walls[3] = 2;
                                    }
                                }
                                foreach (CQB_Cell cell in grid.CQB_Rooms[field[X + 1, Y]].CQB_RoomCells)
                                {
                                    if (cell.X == X + 1 && cell.Y == Y)
                                    {
                                        cell.Walls[1] = 2;
                                    }
                                }
                            }
                        }
                        if (DefDir == 'l')
                        {
                            if (grid.CQB_Rooms[field[X, Y - 1]].ConnectedRoomIDs.Contains(ID) == false)
                            {
                                grid.CQB_Rooms[ID].ConnectedRoomIDs.Add(field[X, Y - 1]);
                                grid.CQB_Rooms[field[X, Y - 1]].ConnectedRoomIDs.Add(ID);
                                foreach (CQB_Cell cell in grid.CQB_Rooms[ID].CQB_RoomCells)
                                {
                                    if (cell.X == X && cell.Y == Y)
                                    {
                                        cell.Walls[0] = 2;
                                    }
                                }
                                foreach (CQB_Cell cell in grid.CQB_Rooms[field[X, Y - 1]].CQB_RoomCells)
                                {
                                    if (cell.X == X && cell.Y == Y - 1)
                                    {
                                        cell.Walls[2] = 2;
                                    }
                                }
                            }
                        }
                        if (DefDir == 'r')
                        {
                            if (grid.CQB_Rooms[field[X, Y + 1]].ConnectedRoomIDs.Contains(ID) == false)
                            {
                                grid.CQB_Rooms[ID].ConnectedRoomIDs.Add(field[X, Y + 1]);
                                grid.CQB_Rooms[field[X, Y + 1]].ConnectedRoomIDs.Add(ID);
                                foreach (CQB_Cell cell in grid.CQB_Rooms[ID].CQB_RoomCells)
                                {
                                    if (cell.X == X && cell.Y == Y)
                                    {
                                        cell.Walls[2] = 2;
                                    }
                                }
                                foreach (CQB_Cell cell in grid.CQB_Rooms[field[X, Y + 1]].CQB_RoomCells)
                                {
                                    if (cell.X == X && cell.Y == Y + 1)
                                    {
                                        cell.Walls[0] = 2;
                                    }
                                }
                            }
                        }

                    }


                }
            }
            /*while(DoorCount < grid.CQB_Rooms.Count * 2 - 1)
            {
                char[] Way = { 'l', 'u', 'r', 'd' };
                char direction = Way[random.Next(Way.Count())];
                if(sx != 0 && direction == 'u')
                {
                    if (field[sx, sy] != field[sx - 1, sy] && grid.CQB_Rooms[field[sx - 1, sy]].ConnectedRoomIDs.Contains(field[sx, sy]) == false)
                    {
                        foreach(CQB_Cell cell in grid.CQB_Rooms[field[sx, sy]].CQB_RoomCells)
                        {
                            if(cell.X == sx && cell.Y == sy)
                            {
                                cell.Walls[1] = 2;
                                grid.CQB_Rooms[field[sx, sy]].ConnectedRoomIDs.Add(field[sx - 1, sy]);
                            }
                        }
                        foreach (CQB_Cell cell in grid.CQB_Rooms[field[sx - 1, sy]].CQB_RoomCells)
                        {
                            if (cell.X == sx - 1 && cell.Y == sy)
                            {
                                cell.Walls[3] = 2;
                                grid.CQB_Rooms[field[sx - 1, sy]].ConnectedRoomIDs.Add(field[sx, sy]);
                            }
                        }
                        DoorCount++;
                    }
                    else
                    {
                        sx--;
                    }
                }
                else if(sy != 0 && direction == 'l')
                {
                    if (field[sx, sy] != field[sx, sy - 1] && grid.CQB_Rooms[field[sx, sy - 1]].ConnectedRoomIDs.Contains(field[sx, sy]) == false)
                    {
                        foreach (CQB_Cell cell in grid.CQB_Rooms[field[sx, sy]].CQB_RoomCells)
                        {
                            if (cell.X == sx && cell.Y == sy)
                            {
                                cell.Walls[0] = 2;
                                grid.CQB_Rooms[field[sx, sy]].ConnectedRoomIDs.Add(field[sx, sy - 1]);
                            }
                        }
                        foreach (CQB_Cell cell in grid.CQB_Rooms[field[sx, sy - 1]].CQB_RoomCells)
                        {
                            if (cell.X == sx && cell.Y == sy - 1)
                            {
                                cell.Walls[2] = 2;
                                grid.CQB_Rooms[field[sx, sy - 1]].ConnectedRoomIDs.Add(field[sx , sy]);
                            }
                        }
                        DoorCount++;
                    }
                    else
                    {
                        sy--;
                    }
                }
                else if(sx != 4 && direction == 'd')
                {
                    if (field[sx, sy] != field[sx + 1, sy] && grid.CQB_Rooms[field[sx + 1, sy]].ConnectedRoomIDs.Contains(field[sx, sy]) == false)
                    {
                        foreach (CQB_Cell cell in grid.CQB_Rooms[field[sx, sy]].CQB_RoomCells)
                        {
                            if (cell.X == sx && cell.Y == sy)
                            {
                                cell.Walls[3] = 2;
                                grid.CQB_Rooms[field[sx, sy]].ConnectedRoomIDs.Add(field[sx + 1, sy]);
                            }
                        }
                        foreach (CQB_Cell cell in grid.CQB_Rooms[field[sx + 1, sy]].CQB_RoomCells)
                        {
                            if (cell.X == sx + 1 && cell.Y == sy)
                            {
                                cell.Walls[1] = 2;
                                grid.CQB_Rooms[field[sx + 1, sy]].ConnectedRoomIDs.Add(field[sx, sy]);
                            }
                        }
                        DoorCount++;
                    }
                    else
                    {
                        sx++;
                        
                    }
                }
                else if (sy != 4 && direction == 'r')
                {
                    if (field[sx, sy] != field[sx, sy + 1] && grid.CQB_Rooms[field[sx, sy + 1]].ConnectedRoomIDs.Contains(field[sx, sy]) == false)
                    {
                        foreach (CQB_Cell cell in grid.CQB_Rooms[field[sx, sy]].CQB_RoomCells)
                        {
                            if (cell.X == sx && cell.Y == sy)
                            {
                                cell.Walls[2] = 2;
                                grid.CQB_Rooms[field[sx, sy]].ConnectedRoomIDs.Add(field[sx, sy +1]);
                            }
                        }
                        foreach (CQB_Cell cell in grid.CQB_Rooms[field[sx, sy + 1]].CQB_RoomCells)
                        {
                            if (cell.X == sx && cell.Y == sy + 1)
                            {
                                cell.Walls[0] = 2;
                                grid.CQB_Rooms[field[sx, sy + 1]].ConnectedRoomIDs.Add(field[sx, sy]);
                            }
                        }
                        DoorCount++;
                    }
                    else
                    {
                        sy++;
                    }
                }
            } */
        }
        private static void RandomiseRoomsProto(int[,] Field)
        {



            Random rnd = new Random();
            int currX = rnd.Next(0, 5);
            int currY = rnd.Next(0, 5);
            int total = 5 * 5;
            int CurRoom = 0;
            bool game = false;
            while (game == false)
            {
                CurRoom++;
                while (Field[currX, currY] != 0)
                {
                    currX = rnd.Next(0, 5);
                    currY = rnd.Next(0, 5);
                }
                int curnext = 0;
                Field[currX, currY] = CurRoom;
                total--;
                int kl = rnd.Next(3, 4);
                for (int i = 0; i < kl; i++)
                {

                    List<char> Neighbours = new List<char>() { 'l', 'u', 'r', 'd' };
                    if (currX == 0 || Field[currX - 1, currY] != 0)
                    {
                        for (int j = 0; j < Neighbours.Count(); j++)
                        {

                            if (Neighbours[j] == 'l')
                            {
                                Neighbours.RemoveAt(j);
                            }

                        }
                    }
                    if (currY == 0 || Field[currX, currY - 1] != 0)
                    {
                        for (int j = 0; j < Neighbours.Count(); j++)
                        {

                            if (Neighbours[j] == 'u')
                            {
                                Neighbours.RemoveAt(j);
                            }

                        }
                    }
                    if (currX == 4 || Field[currX + 1, currY] != 0)
                    {
                        for (int j = 0; j < Neighbours.Count(); j++)
                        {

                            if (Neighbours[j] == 'r')
                            {
                                Neighbours.RemoveAt(j);
                            }

                        }
                    }
                    if (currY == 4 || Field[currX, currY + 1] != 0)
                    {
                        for (int j = 0; j < Neighbours.Count(); j++)
                        {

                            if (Neighbours[j] == 'd')
                            {
                                Neighbours.RemoveAt(j);
                            }

                        }
                    }

                    if (Neighbours.Count() != 0)
                    {

                        int inextroom = rnd.Next(Neighbours.Count() - 1);
                        char nextroom = Neighbours[inextroom];
                        if (nextroom == 'l')
                        {
                            Field[currX - 1, currY] = CurRoom;
                            total--;
                            curnext++;
                            currX--;
                        }
                        else if (nextroom == 'u')
                        {
                            Field[currX, currY - 1] = CurRoom;
                            total--;
                            curnext++;
                            currY--;
                        }
                        else if (nextroom == 'r')
                        {
                            Field[currX + 1, currY] = CurRoom;
                            total--;
                            curnext++;
                            currX++;
                        }
                        else if (nextroom == 'd')
                        {
                            Field[currX, currY + 1] = CurRoom;
                            total--;
                            curnext++;
                            currY++;
                        }


                    }
                    else
                    {
                        if (curnext > 0)
                        {

                            break;
                        }

                        List<char> ANeighbours = new List<char>();

                        if (currX != 0 && Field[currX - 1, currY] != CurRoom)
                        {

                            ANeighbours.Add('l');

                        }
                        if (currY != 0 && Field[currX, currY - 1] != CurRoom)
                        {

                            ANeighbours.Add('u');

                        }
                        if (currX != 4 && Field[currX + 1, currY] != CurRoom)
                        {

                            ANeighbours.Add('r');

                        }
                        if (currY != 4 && Field[currX, currY + 1] != CurRoom)
                        {

                            ANeighbours.Add('d');

                        }


                        int inextroom = rnd.Next(ANeighbours.Count() - 1);
                        char nextroom = ANeighbours[inextroom];
                        if (nextroom == 'l')
                        {
                            Field[currX, currY] = Field[currX - 1, currY];
                            total--;
                            break;
                        }
                        else if (nextroom == 'u')
                        {
                            Field[currX, currY] = Field[currX, currY - 1];
                            total--;
                            break;
                        }
                        else if (nextroom == 'r')
                        {
                            Field[currX, currY] = Field[currX + 1, currY];
                            total--;
                            break;
                        }
                        else if (nextroom == 'd')
                        {
                            Field[currX, currY] = Field[currX, currY + 1];
                            total--;
                            break;
                        }
                    }

                }
                int numa = 0;
                for (int u = 0; u < 5; u++)
                {
                    for (int v = 0; v < 5; v++)
                    {
                        if (Field[u, v] == 0)
                            numa++;
                    }
                }
                if (numa == 0)
                    game = true;



            }
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Field[i, j] = Field[i, j] - 1;
                }
            }


        }
        static void Main(string[] args)
        {
            Init();
        }
    }
}