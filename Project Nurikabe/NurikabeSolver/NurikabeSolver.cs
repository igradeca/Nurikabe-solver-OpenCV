using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NurikabeSolver
{
    public class NurikabeSolver
    {

        private Random rand;
        internal bool moveIsMade;
        private Node root;

        private Heuristic heuristic;
        private SolutionChecker check;

        public Cell[][] puzzle;
        internal Dictionary<int, Point> numbers;
        internal int gridHeight, gridWidth;

        internal int totalNumberOfCells;

        internal int currentNumberOfBlackCells;
        internal int maxNumberOfBlackCells;

        internal int currentNumberOfFullCells;
        internal int maxNumberOfFullCells;

        internal List<Point> passedBlackCells;
        internal List<Point> passedFullCells;

        public NurikabeSolver() {

            heuristic = new Heuristic(this);
            check = new SolutionChecker(this);
        }

        public void SetNewPuzzle(char[][] inputPuzzle) {

            gridHeight = inputPuzzle.Length; // kolko ima redova
            gridWidth = inputPuzzle[0].Length; // kolko ima stupaca
            FillNewPuzzle(inputPuzzle);
            AppendFullsToNumbers();

            currentNumberOfBlackCells = 0;

            totalNumberOfCells = gridHeight * gridWidth;
            CountNumberOfNeededFullCells();
            maxNumberOfBlackCells = totalNumberOfCells - maxNumberOfFullCells;

            rand = new Random();
        }

        public void Begin() {

            heuristic.Start();

            root = new Node(puzzle, currentNumberOfBlackCells, 0, 0);
            SolveForNode(root);
        }

        public char[][] GetResult() {

            char[][] solvingResult = new char[puzzle.Length][];
            for (int i = 0; i < solvingResult.Length; i++) {
                solvingResult[i] = new char[puzzle[i].Length];
                for (int j = 0; j < solvingResult[i].Length; j++) {
                    solvingResult[i][j] = puzzle[i][j].charValue;
                }
            }

            return solvingResult;
        }

        private void SolveForNode(Node node) {

            SwitchPuzzlePointer(
                node.currentGrid,
                currentNumberOfBlackCells,
                currentNumberOfFullCells
                );

            heuristic.Basic();

            check.CountCells();
            if (!check.GridIsCorrect()) {
                node.correct = false;
                return;
            }

            if (currentNumberOfBlackCells == maxNumberOfBlackCells &&
                currentNumberOfFullCells == maxNumberOfFullCells) {
                node.finished = true;
                return;
            }

            BranchNode(node);

            for (int i = 0; i < node.children.Count; i++) {
                SolveForNode(node.children[i]);
                if (node.children[i].finished == true) {
                    node.finished = true;
                    //Console.WriteLine("Solved!");
                    return;
                } else {
                    node.children.RemoveAt(i);
                    --i;
                }
            }
        }

        private void BranchNode(Node node) {

            Point firstEmpty = GetEmptyCandidate();

            node.children = new List<Node>();

            //int randVal = 1;
            int randVal = rand.Next(0, 2);
            for (int i = 0; i < 2; i++) {
                node.children.Add(new Node(
                node.currentGrid,
                currentNumberOfBlackCells,
                currentNumberOfFullCells,
                firstEmpty.X)
                );

                char charVal = (i == randVal) ? 'B' : 'F';
                int idVal = (i == randVal) ? -1 : 0;
                Cell toAdd = new Cell(firstEmpty.X, firstEmpty.Y, charVal, idVal, 0);
                node.children[i].AddCell(toAdd);
            }

        }

        private Point GetEmptyCandidate() {
            /*
            for (int i = 0; i < gridHeight; i++) {
                for (int j = 0; j < gridWidth; j++) {
                    if (CellIsEmpty(i, j)) {
                        return new Point(i, j);
                    }
                }
            }            
            */

            for (int i = gridHeight - 1; i >= 0; i--) {
                for (int j = gridWidth - 1; j >= 0; j--) {
                    if (CellIsEmpty(i, j)) {
                        return new Point(i, j);
                    }
                }
            }

            return new Point();
        }

        /// <summary>
        /// Broji ćelije koje su broj i koje trebaju biti pune.
        /// </summary>
        public void CountNumberOfNeededFullCells() {

            maxNumberOfFullCells = 0;
            currentNumberOfFullCells = 0;
            for (int i = 0; i < gridHeight; i++) {
                for (int j = 0; j < gridWidth; j++) {

                    if (CellIsNumber(i, j)) {
                        maxNumberOfFullCells += puzzle[i][j].intValue;
                    }
                }
            }
        }

        public void FillNewPuzzle(char[][] puzzle) {

            numbers = new Dictionary<int, Point>();

            int id = 1;
            this.puzzle = new Cell[gridHeight][];
            for (int i = 0; i < gridHeight; i++) {
                this.puzzle[i] = new Cell[gridWidth];
                for (int j = 0; j < gridWidth; j++) {

                    if (puzzle[i][j] != '0' && puzzle[i][j] != 'F' && puzzle[i][j] != 'B') {
                        this.puzzle[i][j] = new Cell(i, j, puzzle[i][j], id);
                        numbers.Add(id, new Point(i, j));
                        ++id;
                    } else if (puzzle[i][j] == 'F') {
                        this.puzzle[i][j] = new Cell(i, j, puzzle[i][j], 0);
                    } else {
                        this.puzzle[i][j] = new Cell(i, j, puzzle[i][j], -1);
                    }

                }
            }
        }

        /// <summary>
        /// Ova funkcija služi samo za to ako ručno dodamo koje puno polje.
        /// Služi čisto radi testiranja. Ako postoji neko puno polje onda to
        /// polje dodijeli susjednom broju ili punom polju (po id-u) 
        /// udaljenom za 1 polje.
        /// </summary>
        public void AppendFullsToNumbers() {

            for (int i = 0; i < gridHeight; i++) {
                for (int j = 0; j < gridWidth; j++) {
                    if (CellIsFull(i, j) && GetCellId(i, j) == 0) {

                        if ((i - 1) >= 0 && (CellIsFull((i - 1), j) || CellIsNumber((i - 1), j))) {
                            puzzle[i][j].id = puzzle[(i - 1)][j].id;
                            puzzle[(i - 1)][j].counter++;
                        } else if ((i + 1) < gridHeight && (CellIsFull((i + 1), j) || CellIsNumber((i + 1), j))) {
                            puzzle[i][j].id = puzzle[(i + 1)][j].id;
                            puzzle[(i + 1)][j].counter++;
                        } else if ((j - 1) >= 0 && (CellIsFull(i, (j - 1)) || CellIsNumber(i, (j - 1)))) {
                            puzzle[i][j].id = puzzle[i][(j - 1)].id;
                            puzzle[i][(j - 1)].counter++;
                        } else if ((j + 1) < gridWidth && (CellIsFull(i, (j + 1)) || CellIsNumber(i, (j + 1)))) {
                            puzzle[i][j].id = puzzle[i][(j + 1)].id;
                            puzzle[i][(j + 1)].counter++;
                        }

                    }
                }
            }
        }

        public void AdjustCellsTypeCounters() {

            currentNumberOfBlackCells = 0;
            currentNumberOfFullCells = 0;
            for (int i = 0; i < gridHeight; i++) {
                for (int j = 0; j < gridWidth; j++) {
                    if (CellIsBlack(i, j)) {
                        ++currentNumberOfBlackCells;
                    } else if (CellIsFull(i, j)) {
                        ++currentNumberOfFullCells;
                    }
                }
            }
        }

        public void SwitchPuzzlePointer(Cell[][] puzzle, int currentNumberOfBlackCells, int currentNumberOfFullCells) {

            this.puzzle = puzzle;
            this.currentNumberOfBlackCells = currentNumberOfBlackCells;
            this.currentNumberOfFullCells = currentNumberOfFullCells;
        }

        public bool BlackCellHasWayOut(int row, int column, int unallowedDirection, int cellsVisited) {

            if (cellsVisited == 0) {
                passedBlackCells = new List<Point>();
            }
            passedBlackCells.Add(new Point(row, column));

            if (cellsVisited > maxNumberOfBlackCells) {
                return false;
            }

            // up
            if (unallowedDirection != 1 && (row - 1) >= 0) {
                if (CellIsEmpty(((row - 1)), column)) {
                    return true;
                } else if (CellIsBlack((row - 1), column) &&
                    !BlackCellIsAlreadyPassed((row - 1), column)) {
                    if (BlackCellHasWayOut(((row - 1)), column, 3, ++cellsVisited)) {
                        return true;
                    }
                }
            }

            // right
            if (unallowedDirection != 2 && (column + 1) < gridWidth) {
                if (CellIsEmpty(row, (column + 1))) {
                    return true;
                } else if (CellIsBlack(row, (column + 1)) &&
                    !BlackCellIsAlreadyPassed(row, (column + 1))) {
                    if (BlackCellHasWayOut(row, (column + 1), 4, ++cellsVisited)) {
                        return true;
                    }
                }
            }

            // down
            if (unallowedDirection != 3 && (row + 1) < gridHeight) {
                if (CellIsEmpty((row + 1), column)) {
                    return true;
                } else if (CellIsBlack((row + 1), column) &&
                    !BlackCellIsAlreadyPassed((row + 1), column)) {
                    if (BlackCellHasWayOut((row + 1), column, 1, ++cellsVisited)) {
                        return true;
                    }
                }
            }

            // left
            if (unallowedDirection != 4 && (column - 1) >= 0) {
                if (CellIsEmpty(row, (column - 1))) {
                    return true;
                } else if (CellIsBlack(row, (column - 1)) && !BlackCellIsAlreadyPassed(row, (column - 1))) {
                    if (BlackCellHasWayOut(row, (column - 1), 2, ++cellsVisited)) {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool BlackCellIsAlreadyPassed(int row, int column) {

            for (int i = 0; i < passedBlackCells.Count; i++) {
                if (passedBlackCells[i].X == row && passedBlackCells[i].Y == column) {
                    return true;
                }
            }
            return false;
        }

        public bool FullCellIsAlreadyPassed(int row, int column) {

            for (int i = 0; i < passedFullCells.Count; i++) {
                if (passedFullCells[i].X == row && passedFullCells[i].Y == column) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// t - ten 10, e - eleven 11, w - twelve 12, h - thirteen 13
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public int GetCellNumberValue(int row, int column) {

            int cellIntegerValue = 0;
            char cellValue = puzzle[row][column].charValue;

            if (CellIsNumber(row, column)) {
                if (cellValue == 't') {
                    cellIntegerValue = 10;
                } else if (cellValue == 'e') {
                    cellIntegerValue = 11;
                } else if (cellValue == 'w') {
                    cellIntegerValue = 12;
                } else if (cellValue == 'h') {
                    cellIntegerValue = 13;
                } else {
                    cellIntegerValue = int.Parse(cellValue.ToString());
                }
            }

            return cellIntegerValue;
        }

        public int GetCellId(int row, int column) {

            return puzzle[row][column].id;
        }

        public int GetCellCounter(int row, int column) {

            return puzzle[row][column].counter;
        }

        public bool CellIsNumberOne(int row, int column) {

            if (puzzle[row][column].charValue == '1') {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gleda da li je ćelija broj od 1 do 13.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool CellIsNumber(int row, int column) {

            for (int i = 49; i <= 57; i++) {
                if (puzzle[row][column].charValue == Convert.ToChar(i)) {
                    return true;
                }
            }

            // t - ten 10, e - eleven 11, w - twelve 12, h - thirteen 13
            if (puzzle[row][column].charValue == 't' || puzzle[row][column].charValue == 'e'
                || puzzle[row][column].charValue == 'w' || puzzle[row][column].charValue == 'h') {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gleda da li je ćelija puna.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool CellIsFull(int row, int column) {

            if (puzzle[row][column].charValue == 'F') {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gleda da li je ćelija crno polje.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool CellIsBlack(int row, int column) {

            if (puzzle[row][column].charValue == 'B') {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gleda da li je ćelija prazna.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool CellIsEmpty(int row, int column) {

            if (puzzle[row][column].charValue == '0') {
                return true;
            }
            return false;
        }

        public void UpdateCell(int row, int column, char value, int newId) {

            if (CellIsEmpty(row, column)) {
                if (value == 'B' && currentNumberOfBlackCells < maxNumberOfBlackCells) {
                    puzzle[row][column].charValue = value;
                    //grid.UpdateCell(row, column, value);

                    ++currentNumberOfBlackCells;
                    moveIsMade = true;
                } else if (value == 'F' && currentNumberOfFullCells < maxNumberOfFullCells) {
                    puzzle[row][column].charValue = value;
                    puzzle[row][column].id = newId;
                    if (newId > 0) {
                        IncrementNumberFullCounterById(newId);
                    }
                    //grid.UpdateCell(row, column, value);

                    ++currentNumberOfFullCells;
                    moveIsMade = true;
                }
            } else if (CellIsFull(row, column) && GetCellId(row, column) == 0 && newId > 0) { // Ako puna ćelija nekog broja naleti 
                puzzle[row][column].id = newId;                                               // na ćeliju koja ne pripada nikome.
                IncrementNumberFullCounterById(newId);
                //grid.UpdateCell(row, column, value);

                moveIsMade = true;
            }
        }

        public void IncrementNumberFullCounterById(int id) {

            for (int i = 0; i < gridHeight; i++) {
                for (int j = 0; j < gridWidth; j++) {
                    if (CellIsNumber(i, j) && GetCellId(i, j) == id) {
                        puzzle[i][j].counter++;
                        if (puzzle[i][j].counter == puzzle[i][j].intValue) {
                            CloseFinishedNumberById(id);
                        }
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Broj koji ima dovoljan broj punih polja okruži sa crnim poljima.
        /// </summary>
        /// <param name="cellNumberId"></param>
        public void CloseFinishedNumberById(int cellNumberId) {

            for (int i = 0; i < gridHeight; i++) {
                for (int j = 0; j < gridWidth; j++) {

                    if (GetCellId(i, j) == cellNumberId) {
                        SurroundCellWithBlacks(i, j);
                    }
                }
            }
        }

        // Okružuje ćeliju horizontalno i vertikalno sa crnim poljima.
        public void SurroundCellWithBlacks(int row, int column) {

            if ((row - 1) >= 0) {
                UpdateCell((row - 1), column, 'B', -1);
            }
            if ((row + 1) < gridHeight) {
                UpdateCell((row + 1), column, 'B', -1);
            }

            if ((column - 1) >= 0) {
                UpdateCell(row, (column - 1), 'B', -1);
            }
            if ((column + 1) < gridWidth) {
                UpdateCell(row, (column + 1), 'B', -1);
            }
        }

    }
}
