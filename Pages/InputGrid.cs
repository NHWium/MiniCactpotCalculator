using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniCactpot.Pages
{
    public partial class InputGrid
    {
        int[,] Grid;
        int[] Payout;
        double[] PayoutChance;
        List<int> AvailableNumbers;
        Selection[] Row;
        Selection[] Column;
        Selection[] Diagonal;
        int[,] GridStatus;
        List<(int, int)> Hint;
        const int NORMAL = 0;
        const int SUGGESTED = 1;
        const int ACTIVE = 2;
        const int HINTED = 4;

        public InputGrid()
        {
            ResetOrInitialize();
        }

        void ResetOrInitialize()
        {
            Grid = new int[3, 3] {
                { 0, 0, 0 },
                { 0, 0, 0 },
                { 0, 0, 0 }
            };
            Payout = new int[25] { 0, 0, 0, 0, 0, 0, //0-5
                10000,  // 6
                36,     // 7
                720,    // 8
                360,    // 9
                80,     //10
                252,    //11
                108,    //12
                72,     //13
                54,     //14
                180,    //15
                72,     //16
                180,    //17
                119,    //18
                36,     //19
                306,    //20
                1080,   //21
                144,    //22
                1800,   //23
                3600    //24
            };
            PayoutChance = new double[25];
            Row = new Selection[3] {
                new Selection(this, (0, 0), (0, 1), (0, 2), "Row0"),
                new Selection(this, (1, 0), (1, 1), (1, 2), "Row1"),
                new Selection(this, (2, 0), (2, 1), (2, 2), "Row2")
            };
            Column = new Selection[3] {
                new Selection(this, (0, 0), (1, 0), (2, 0), "Column0"),
                new Selection(this, (0, 1), (1, 1), (2, 1), "Column1"),
                new Selection(this, (0, 2), (1, 2), (2, 2), "Column2")
            };
            Diagonal = new Selection[2] {
                new Selection(this, (0, 0), (1, 1), (2, 2), "Diagonal0"),
                new Selection(this, (2, 0), (1, 1), (0, 2), "Diagonal1")
            };
            GridStatus = new int[3, 3] {
                { NORMAL, NORMAL, NORMAL },
                { NORMAL, NORMAL, NORMAL },
                { NORMAL, NORMAL, NORMAL }
            };
            FindUsed();
            CalculateAllPayouts();
            GetHint();
        }

        void Selected(int x, int y, string value)
        {
            bool parsed = int.TryParse(value, out Grid[x, y]);
            if (!parsed) Grid[x, y] = 0;
            FindUsed();
            CalculateAllPayouts();
            GetHint();
        }

        void FindUsed()
        {
            AvailableNumbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    if (Grid[x, y] != 0) AvailableNumbers.Remove(Grid[x, y]);
                }
            }
        }

        bool Used(int value)
        {
            return !AvailableNumbers.Contains<int>(value);
        }

        int SelectionCount
        {
            get
            {
                int result = 0;
                var e = Grid.GetEnumerator();
                while (e.MoveNext())
                {
                    if (!e.Current.Equals(0)) result++;
                }
                return result;
            }
        }

        void CalculateAllPayouts()
        {
            List<Selection> scores = new();
            scores.Add(Diagonal[0]);
            scores.Add(Diagonal[1]);
            scores.Add(Row[0]);
            scores.Add(Row[1]);
            scores.Add(Row[2]);
            scores.Add(Column[0]);
            scores.Add(Column[1]);
            scores.Add(Column[2]);
            List<Selection>.Enumerator enumScores = scores.GetEnumerator();
            while (enumScores.MoveNext()) {
                PrepareSelection(enumScores.Current);
            }
            double highscore = scores.Max(x => x.Result);
            double count = scores.FindAll(x => x.Result.Equals(highscore)).Count;
            List<Selection>.Enumerator enumHighscores = scores.FindAll(x => x.Result.Equals(highscore)).GetEnumerator();
            while (count > 0 && enumHighscores.MoveNext())
            {
                if (count <= 5) {
                    enumHighscores.Current.Suggested = true;
                    enumHighscores.Current.SetStatus();
                }
                if (count > 5) count = 0;
                else count--;
                if (count == 0)
                    UpdatePayoutChance(CalculateChance(enumHighscores.Current));
            }
        }

        double CalculatePayout(int[] sums)
        {
            int combinations = sums[0];
            double payout = 0.00;
            for (int i = 6; i < 25; i++)
            {
                payout += Payout[i] * sums[i] / combinations;
            }
            return payout;
        }

        double PrepareSelection(Selection selection)
        {
            selection.Suggested = false;
            selection.Active = false;
            selection.ResetStatus();
            selection.Result = CalculatePayout(CalculateChance(selection));
            return selection.Result;
        }

        void UpdateActiveSelection(Selection selection)
        {
            if (!selection.Active) {
                CalculateAllPayouts();
                selection.Active = true;
                selection.SetStatus();
                UpdatePayoutChance(CalculateChance(selection));
            }
            else CalculateAllPayouts();
        }

        void UpdatePayoutChance(int[] sums) 
        {
            int combinations = sums[0];
            for (int i = 6; i < 25; i++)
            {
                PayoutChance[i] = 100.00 * sums[i] / combinations;
            }
            PayoutChance[0] = combinations;
        }

        int[] CalculateChance(Selection selection) 
        {
            int[] sums = new int[25];

            List<int>.Enumerator firstEnum;
            if (selection.First != 0) firstEnum = (new List<int> { selection.First }).GetEnumerator();
            else firstEnum = AvailableNumbers.GetEnumerator();
            while(firstEnum.MoveNext()) {
                List<int>.Enumerator secondEnum;
                if (selection.Second != 0) secondEnum = (new List<int> { selection.Second }).GetEnumerator();
                else secondEnum = AvailableNumbers.GetEnumerator();
                while (secondEnum.MoveNext()) {
                    List<int>.Enumerator thirdEnum;
                    if (selection.Third != 0) thirdEnum = (new List<int> { selection.Third }).GetEnumerator();
                    else thirdEnum = AvailableNumbers.GetEnumerator();
                    while (thirdEnum.MoveNext()) {
                        if (firstEnum.Current != secondEnum.Current && firstEnum.Current != thirdEnum.Current && secondEnum.Current != thirdEnum.Current) {
                            sums[firstEnum.Current + secondEnum.Current + thirdEnum.Current]++;
                            sums[0]++; // used to carry number of permutations
                        }
                    }
                }
            }
            return sums;
        }

        int GetStatus(int x, int y) {
            if (Hint.Contains((x, y))) return (GridStatus[x, y] + HINTED);
            return GridStatus[x, y];
        }

        void GetHint() {
            Hint = new List<(int, int)>();
            if (AvailableNumbers.Count < 9 && AvailableNumbers.Count > 5) {
                if (Grid[1, 1] == 0) Hint.Add((1, 1));
                else {
                    if (AvailableNumbers.Count > 6 && Grid[0, 0] == 0 && Grid[2, 2] == 0 && Grid[2, 0] == 0 && Grid[0, 2] == 0) {
                        if (Grid[1, 0] == 0 && Grid[0, 1] == 0) Hint.Add((0, 0)); // top left
                        if (Grid[1, 0] == 0 && Grid[2, 1] == 0) Hint.Add((2, 0)); // top right
                        if (Grid[1, 2] == 0 && Grid[0, 1] == 0) Hint.Add((0, 2)); // bottom left
                        if (Grid[1, 2] == 0 && Grid[2, 1] == 0) Hint.Add((2, 2)); // bottom right
                    }
                    else if (AvailableNumbers.Count > 6 && Grid[1, 0] == 0 && Grid[1, 2] == 0 && Grid[0, 1] == 0 && Grid[2, 1] == 0)
                    {
                        if (Grid[0, 0] == 0 && Grid[2, 0] == 0) Hint.Add((1, 0)); // top
                        if (Grid[0, 2] == 0 && Grid[2, 2] == 0) Hint.Add((1, 2)); // bottom
                        if (Grid[0, 0] == 0 && Grid[0, 2] == 0) Hint.Add((0, 1)); // left
                        if (Grid[2, 0] == 0 && Grid[2, 2] == 0) Hint.Add((2, 1)); // right
                    }
                    if (AvailableNumbers.Count == 6)
                    {
                        // What is the best here
                    }
                }
            }
        }

        class Selection 
        {
            public InputGrid InputGrid;
            public readonly string Name;
            public double Result;
            public bool Active;
            public bool Suggested;
            public (int, int) FirstCoords;
            public (int, int) SecondCoords;
            public (int, int) ThirdCoords;
            public int First
            {
                get { return InputGrid.Grid[FirstCoords.Item1, FirstCoords.Item2]; }
                set { InputGrid.Grid[FirstCoords.Item1, FirstCoords.Item2] = value; }
            }
            public int Second
            {
                get { return InputGrid.Grid[SecondCoords.Item1, SecondCoords.Item2]; }
                set { InputGrid.Grid[SecondCoords.Item1, SecondCoords.Item2] = value; }
            }
            public int Third
            {
                get { return InputGrid.Grid[ThirdCoords.Item1, ThirdCoords.Item2]; }
                set { InputGrid.Grid[ThirdCoords.Item1, ThirdCoords.Item2] = value; }
            }
            public int FirstStatus { 
                get { return InputGrid.GridStatus[FirstCoords.Item1, FirstCoords.Item2]; }
                set { InputGrid.GridStatus[FirstCoords.Item1, FirstCoords.Item2] = value; }
            }
            public int SecondStatus {
                get { return InputGrid.GridStatus[SecondCoords.Item1, SecondCoords.Item2]; }
                set { InputGrid.GridStatus[SecondCoords.Item1, SecondCoords.Item2] = value; }
            }
            public int ThirdStatus {
                get { return InputGrid.GridStatus[ThirdCoords.Item1, ThirdCoords.Item2]; }
                set { InputGrid.GridStatus[ThirdCoords.Item1, ThirdCoords.Item2] = value; }
            }

            public Selection (InputGrid inputGrid, (int, int) first, (int, int) second, (int, int) third, string name) 
            {
                InputGrid = inputGrid;
                ResetOrInitialize();
                FirstCoords = first;
                SecondCoords = second;
                ThirdCoords = third;
                Name = name;
            }

            public void ResetOrInitialize()
            {
                Result = 0.00;
                Active = false;
                Suggested = false;
            }

            public void ResetStatus() {
                FirstStatus = InputGrid.NORMAL;
                SecondStatus = InputGrid.NORMAL;
                ThirdStatus = InputGrid.NORMAL;
            }
            public void SetStatus()
            {
                int status = InputGrid.NORMAL;
                if (Suggested) status += InputGrid.SUGGESTED;
                if (Active) status += InputGrid.ACTIVE;
                FirstStatus |= status;
                SecondStatus |= status;
                ThirdStatus |= status;
            }
            public int GetStatus(int x = -1, int y = -1)
            {
                int status = InputGrid.NORMAL;
                if (Suggested) status += InputGrid.SUGGESTED;
                if (Active) status += InputGrid.ACTIVE;
                return status;
            }
            public string GetResult() 
            {
                return $"{Math.Round(Result)} MPG";
            }
        }

    }
}
