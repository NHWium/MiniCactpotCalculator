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
        List<Selection> Selections;
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
            Selections = new List<Selection>
            {
                { new Selection(this, "Column", 0, (0, 0), (0, 1), (0, 2)) },
                { new Selection(this, "Column", 1, (1, 0), (1, 1), (1, 2)) },
                { new Selection(this, "Column", 2, (2, 0), (2, 1), (2, 2)) },
                { new Selection(this, "Row", 0, (0, 0), (1, 0), (2, 0)) },
                { new Selection(this, "Row", 1, (0, 1), (1, 1), (2, 1)) },
                { new Selection(this, "Row", 2, (0, 2), (1, 2), (2, 2)) },
                { new Selection(this, "Diagonal", 0, (0, 0), (1, 1), (2, 2)) },
                { new Selection(this, "Diagonal", 1, (2, 0), (1, 1), (0, 2)) }
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

        Selection GetSelection(string name, int location) {
            return Selections.Find((x => x.Name == name && x.Location == location));
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
            List<Selection>.Enumerator enumScores = Selections.GetEnumerator();
            while (enumScores.MoveNext()) {
                PrepareSelection(enumScores.Current);
            }
            double highscore = Selections.Max(x => x.Result);
            List<Selection> highSelections = Selections.FindAll(x => x.Result.Equals(highscore));
            int count = highSelections.Count;
            List<Selection>.Enumerator enumHighscores = highSelections.GetEnumerator();
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
                //Never leave the center bare
                if (Grid[1, 1] == 0) 
                    Hint.Add((1, 1));
                else if (AvailableNumbers.Count > 6) {
                    // if all the corners are clear, the second choice should be a corner
                    if (Grid[0, 0] == 0 && Grid[2, 2] == 0 && Grid[2, 0] == 0 && Grid[0, 2] == 0) {
                        // Hint at corners with no adjacent tile revealed
                        if (Grid[1, 0] == 0 && Grid[0, 1] == 0) Hint.Add((0, 0)); // top left
                        if (Grid[1, 0] == 0 && Grid[2, 1] == 0) Hint.Add((2, 0)); // top right
                        if (Grid[1, 2] == 0 && Grid[0, 1] == 0) Hint.Add((0, 2)); // bottom left
                        if (Grid[1, 2] == 0 && Grid[2, 1] == 0) Hint.Add((2, 2)); // bottom right
                    }
                    // if a corner was revealed, the second choice should be a middle
                    else if (Grid[1, 0] == 0 && Grid[1, 2] == 0 && Grid[0, 1] == 0 && Grid[2, 1] == 0)
                    {
                        // Hint at middles with no adjacent corner revealed
                        if (Grid[0, 0] == 0 && Grid[2, 0] == 0) Hint.Add((1, 0)); // top
                        if (Grid[0, 2] == 0 && Grid[2, 2] == 0) Hint.Add((1, 2)); // bottom
                        if (Grid[0, 0] == 0 && Grid[0, 2] == 0) Hint.Add((0, 1)); // left
                        if (Grid[2, 0] == 0 && Grid[2, 2] == 0) Hint.Add((2, 1)); // right
                    }
                }
                else
                {
                    // If there is one clear best selection, with two reveals, reveal the third tile
                    double highscore = Selections.Max(x => x.Result);
                    List<Selection> highSelectionsWithOneMissing = Selections.FindAll(x => x.Result.Equals(highscore) && x.RevealCount == 2);
                    if (highSelectionsWithOneMissing.Count == 1)
                    {
                        Selection selection = highSelectionsWithOneMissing.FirstOrDefault<Selection>();
                        if (selection.First == 0) Hint.Add(selection.FirstCoords);
                        if (selection.Second == 0) Hint.Add(selection.SecondCoords);
                        if (selection.Third == 0) Hint.Add(selection.ThirdCoords);
                    }
                    else
                    {
                        // If there is an intersection of two non-revealed selections, hint at the intersections
                        List<Selection> emptySelections = Selections.FindAll(x => x.RevealCount == 0);
                        if (emptySelections.Contains(GetSelection("Column", 0)) && emptySelections.Contains(GetSelection("Row", 0)))
                            Hint.Add((0, 0));
                        if (emptySelections.Contains(GetSelection("Column", 2)) && emptySelections.Contains(GetSelection("Row", 0)))
                            Hint.Add((2, 0));
                        if (emptySelections.Contains(GetSelection("Column", 0)) && emptySelections.Contains(GetSelection("Row", 2)))
                            Hint.Add((0, 2));
                        if (emptySelections.Contains(GetSelection("Column", 2)) && emptySelections.Contains(GetSelection("Row", 2)))
                            Hint.Add((2, 2));
                        // If there is an intersection between an non-revealed selection and a selection with a high score, hint at it
                        if (Hint.Count == 0) 
                        {
                            List<Selection> highSelections = Selections.FindAll(x => x.Result.Equals(highscore) && x.RevealCount < 3);
                            if (emptySelections.Contains(GetSelection("Column", 0)) && highSelections.Contains(GetSelection("Row", 0)))
                                Hint.Add((0, 0));
                            else if (emptySelections.Contains(GetSelection("Column", 0)) && highSelections.Contains(GetSelection("Row", 1)))
                                Hint.Add((0, 1));
                            else if (emptySelections.Contains(GetSelection("Column", 0)) && highSelections.Contains(GetSelection("Row", 2)))
                                Hint.Add((0, 2));
                            else if (emptySelections.Contains(GetSelection("Column", 0)) && highSelections.Contains(GetSelection("Diagonal", 0)))
                                Hint.Add((0, 0));
                            else if (emptySelections.Contains(GetSelection("Column", 0)) && highSelections.Contains(GetSelection("Diagonal", 1)))
                                Hint.Add((0, 2));
                            else if (emptySelections.Contains(GetSelection("Column", 1)) && highSelections.Contains(GetSelection("Row", 0)))
                                Hint.Add((1, 0));
                            else if (emptySelections.Contains(GetSelection("Column", 1)) && highSelections.Contains(GetSelection("Row", 1)))
                                Hint.Add((1, 1));
                            else if (emptySelections.Contains(GetSelection("Column", 1)) && highSelections.Contains(GetSelection("Row", 2)))
                                Hint.Add((1, 2));
                            else if (emptySelections.Contains(GetSelection("Column", 2)) && highSelections.Contains(GetSelection("Row", 0)))
                                Hint.Add((2, 0));
                            else if (emptySelections.Contains(GetSelection("Column", 2)) && highSelections.Contains(GetSelection("Row", 1)))
                                Hint.Add((2, 1));
                            else if (emptySelections.Contains(GetSelection("Column", 2)) && highSelections.Contains(GetSelection("Row", 2)))
                                Hint.Add((2, 2));
                            else if (emptySelections.Contains(GetSelection("Column", 2)) && highSelections.Contains(GetSelection("Diagonal", 0)))
                                Hint.Add((2, 2));
                            else if (emptySelections.Contains(GetSelection("Column", 2)) && highSelections.Contains(GetSelection("Diagonal", 1)))
                                Hint.Add((2, 0));
                            else if (emptySelections.Contains(GetSelection("Row", 0)) && highSelections.Contains(GetSelection("Column", 0)))
                                Hint.Add((0, 0));
                            else if (emptySelections.Contains(GetSelection("Row", 0)) && highSelections.Contains(GetSelection("Column", 1)))
                                Hint.Add((1, 0));
                            else if (emptySelections.Contains(GetSelection("Row", 0)) && highSelections.Contains(GetSelection("Column", 2)))
                                Hint.Add((2, 0));
                            else if (emptySelections.Contains(GetSelection("Row", 0)) && highSelections.Contains(GetSelection("Diagonal", 0)))
                                Hint.Add((0, 0));
                            else if (emptySelections.Contains(GetSelection("Row", 0)) && highSelections.Contains(GetSelection("Diagonal", 1)))
                                Hint.Add((2, 0));
                            else if (emptySelections.Contains(GetSelection("Row", 1)) && highSelections.Contains(GetSelection("Column", 0)))
                                Hint.Add((0, 1));
                            else if (emptySelections.Contains(GetSelection("Row", 1)) && highSelections.Contains(GetSelection("Column", 1)))
                                Hint.Add((1, 1));
                            else if (emptySelections.Contains(GetSelection("Row", 1)) && highSelections.Contains(GetSelection("Column", 2)))
                                Hint.Add((2, 1));
                            else if (emptySelections.Contains(GetSelection("Row", 2)) && highSelections.Contains(GetSelection("Column", 0)))
                                Hint.Add((0, 2));
                            else if (emptySelections.Contains(GetSelection("Row", 2)) && highSelections.Contains(GetSelection("Column", 1)))
                                Hint.Add((1, 2));
                            else if (emptySelections.Contains(GetSelection("Row", 2)) && highSelections.Contains(GetSelection("Column", 2)))
                                Hint.Add((2, 2));
                            else if (emptySelections.Contains(GetSelection("Row", 2)) && highSelections.Contains(GetSelection("Diagonal", 0)))
                                Hint.Add((2, 2));
                            else if (emptySelections.Contains(GetSelection("Row", 2)) && highSelections.Contains(GetSelection("Diagonal", 1)))
                                Hint.Add((0, 2));
                        }
                        if (Hint.Count == 0)
                        {
                            if (emptySelections.Contains(GetSelection("Row", 0)) && GetSelection("Diagonal", 0).RevealCount == 1)
                                Hint.Add((0, 0));
                            else if (emptySelections.Contains(GetSelection("Row", 0)) && GetSelection("Diagonal", 1).RevealCount == 1)
                                Hint.Add((2, 0));
                            else if (emptySelections.Contains(GetSelection("Row", 0)) && GetSelection("Diagonal", 0).RevealCount == 2 && GetSelection("Diagonal", 1).RevealCount == 2)
                                Hint.Add((1, 0));
                            else if (emptySelections.Contains(GetSelection("Row", 2)) && GetSelection("Diagonal", 0).RevealCount == 1)
                                Hint.Add((2, 2));
                            else if (emptySelections.Contains(GetSelection("Row", 2)) && GetSelection("Diagonal", 1).RevealCount == 1)
                                Hint.Add((0, 2));
                            else if (emptySelections.Contains(GetSelection("Row", 2)) && GetSelection("Diagonal", 0).RevealCount == 2 && GetSelection("Diagonal", 1).RevealCount == 2)
                                Hint.Add((1, 2));
                            else if (emptySelections.Contains(GetSelection("Column", 0)) && GetSelection("Diagonal", 0).RevealCount == 1)
                                Hint.Add((0, 0));
                            else if (emptySelections.Contains(GetSelection("Column", 0)) && GetSelection("Diagonal", 1).RevealCount == 1)
                                Hint.Add((0, 2));
                            else if (emptySelections.Contains(GetSelection("Column", 0)) && GetSelection("Diagonal", 0).RevealCount == 2 && GetSelection("Diagonal", 1).RevealCount == 2)
                                Hint.Add((0, 1));
                            else if (emptySelections.Contains(GetSelection("Column", 2)) && GetSelection("Diagonal", 0).RevealCount == 1)
                                Hint.Add((2, 2));
                            else if (emptySelections.Contains(GetSelection("Column", 2)) && GetSelection("Diagonal", 1).RevealCount == 1)
                                Hint.Add((2, 0));
                            else if (emptySelections.Contains(GetSelection("Column", 2)) && GetSelection("Diagonal", 0).RevealCount == 2 && GetSelection("Diagonal", 1).RevealCount == 2)
                                Hint.Add((2, 1));
                        }
                    }
                }
            }
        }

        class Selection 
        {
            public InputGrid InputGrid;
            public readonly string Name;
            public readonly int Location;
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
            public int RevealCount
            {
                get
                {
                    int count = 0;
                    if (First != 0) count++;
                    if (Second != 0) count++;
                    if (Third != 0) count++;
                    return count;
                }
            }
            public Selection (InputGrid inputGrid, string name, int location, (int, int) first, (int, int) second, (int, int) third) 
            {
                InputGrid = inputGrid;
                Name = name;
                Location = location;
                FirstCoords = first;
                SecondCoords = second;
                ThirdCoords = third;
                ResetOrInitialize();
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
            public int GetStatus()
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
