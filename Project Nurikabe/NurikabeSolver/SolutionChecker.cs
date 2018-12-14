using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NurikabeSolver {
    class SolutionChecker {

        NurikabeSolver solver;

        private List<Point> emptyCells;
        private List<Point> noneFullCells;
        private List<Point> numberCells;

        public SolutionChecker(NurikabeSolver solver) {

            this.solver = solver;
        }

        public void CountCells() {

            CountBlackCells();
            CountFullCells();
        }

        public bool GridIsCorrect() {

            if (solver.currentNumberOfBlackCells > solver.maxNumberOfBlackCells) {
                return false;
            } else if (solver.currentNumberOfFullCells > solver.maxNumberOfFullCells) {
                return false;
            } else if (FourBlacks()) {
                return false;
            } else if (NumbersAreConnected()) {
                return false;
            } else if (ThereAreIsolatedBlackCells()) {
                return false;
            } else if (NumberHasTooManyFullCells()) {
                return false;
            } else if (NumberIsClosedTooEarly()) {
                return false;
            } else if (ClosedFullsWithoutNumber()) {
                return false;
            }

            return true;
        }
        
        private bool FourBlacks() {

            for (int i = 0; i < (solver.gridHeight - 1); i++) {
                for (int j = 0; j < (solver.gridWidth - 1); j++) {
                    if (solver.CellIsBlack(i, j) && solver.CellIsBlack(i, (j + 1)) &&
                        solver.CellIsBlack((i + 1), j) && solver.CellIsBlack((i + 1), (j + 1))) {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool NumbersAreConnected() {

            for (int i = 0; i < solver.gridHeight; i++) {
                for (int j = 0; j < solver.gridWidth; j++) {
                    if (solver.CellIsFull(i, j) && solver.GetCellId(i, j) > 0) {

                        int currentCellId = solver.GetCellId(i, j);

                        if ((i - 1) >= 0) {
                            if ((solver.CellIsFull((i - 1), j) || solver.CellIsNumber((i - 1), j)) &&
                                solver.GetCellId((i - 1), j) > 0 && solver.GetCellId((i - 1), j) != currentCellId) {
                                return true;
                            }
                        }

                        if ((i + 1) < solver.gridHeight) {
                            if ((solver.CellIsFull((i + 1), j) || solver.CellIsNumber((i + 1), j)) &&
                                solver.GetCellId((i + 1), j) > 0 && solver.GetCellId((i + 1), j) != currentCellId) {
                                return true;
                            }
                        }

                        if ((j - 1) >= 0) {
                            if ((solver.CellIsFull(i, (j - 1)) || solver.CellIsNumber(i, (j - 1))) &&
                                solver.GetCellId(i, (j - 1)) > 0 && solver.GetCellId(i, (j - 1)) != currentCellId) {
                                return true;
                            }
                        }

                        if ((j + 1) < solver.gridWidth) {
                            if ((solver.CellIsFull(i, (j + 1)) || solver.CellIsNumber(i, (j + 1))) &&
                                solver.GetCellId(i, (j + 1)) > 0 && solver.GetCellId(i, (j + 1)) != currentCellId) {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool ThereAreIsolatedBlackCells() {
            /*
            if (solver.currentNumberOfBlackCells == solver.maxNumberOfBlackCells &&
                solver.currentNumberOfFullCells == solver.maxNumberOfFullCells) {
                return false;
            }
            */
            
            for (int i = 0; i < solver.gridHeight; i++) {
                for (int j = 0; j < solver.gridWidth; j++) {
                    if (solver.passedBlackCells.Count > 0) {
                        solver.passedBlackCells = new List<Point>();
                    }
                    if (solver.CellIsBlack(i, j) && !solver.BlackCellHasWayOut(i, j, 0, 0)) {                        
                        if (solver.passedBlackCells.Count == solver.maxNumberOfBlackCells) {
                            return false;
                        }
                        
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Rekurzivna funkcija. Gleda kolko imamo crnih polja koja su povezana u cjelinu.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="unallowedDirection">1 - up. 2 - right. 3 - down. 4 - left.</param>
        /// <returns></returns>
        private void CountConnectedBlackCells(int row, int column, int unallowedDirection, int cellsVisited) {

            if (cellsVisited == 1) {
                solver.passedBlackCells = new List<Point>();
            }
            solver.passedBlackCells.Add(new Point(row, column));

            if (cellsVisited > solver.maxNumberOfBlackCells) {
                return;
            }

            // up
            if (unallowedDirection != 1 && (row - 1) >= 0) {
                if (solver.CellIsBlack((row - 1), column) && !solver.BlackCellIsAlreadyPassed((row - 1), column)) {
                    CountConnectedBlackCells(((row - 1)), column, 3, ++cellsVisited);
                }
            }

            // right
            if (unallowedDirection != 2 && (column + 1) < solver.gridWidth) {
                if (solver.CellIsBlack(row, (column + 1)) && !solver.BlackCellIsAlreadyPassed(row, (column + 1))) {
                    CountConnectedBlackCells(row, (column + 1), 4, ++cellsVisited);
                }
            }

            // down
            if (unallowedDirection != 3 && (row + 1) < solver.gridHeight) {
                if (solver.CellIsBlack((row + 1), column) && !solver.BlackCellIsAlreadyPassed((row + 1), column)) {
                    CountConnectedBlackCells((row + 1), column, 1, ++cellsVisited);
                }
            }

            // left
            if (unallowedDirection != 4 && (column - 1) >= 0) {
                if (solver.CellIsBlack(row, (column - 1)) && !solver.BlackCellIsAlreadyPassed(row, (column - 1))) {
                    CountConnectedBlackCells(row, (column - 1), 2, ++cellsVisited);
                }
            }
        }

        private void CountBlackCells() {

            solver.currentNumberOfBlackCells = 0;
            for (int i = 0; i < solver.gridHeight; i++) {
                for (int j = 0; j < solver.gridWidth; j++) {
                    if (solver.CellIsBlack(i, j)) {
                        ++solver.currentNumberOfBlackCells;
                    }
                }
            }
        }

        /// <summary>
        /// Broji kolko ima brojeva i punih ćelija!
        /// </summary>
        private void CountFullCells() {

            solver.currentNumberOfFullCells = 0;
            for (int i = 0; i < solver.gridHeight; i++) {
                for (int j = 0; j < solver.gridWidth; j++) {
                    if (solver.CellIsFull(i, j) || solver.CellIsNumber(i, j)) {
                        ++solver.currentNumberOfFullCells;
                    }
                }
            }
        }

        private bool NumberHasTooManyFullCells() {

            for (int i = 0; i < solver.gridHeight; i++) {
                for (int j = 0; j < solver.gridWidth; j++) {
                    if (solver.CellIsNumber(i, j) && solver.GetCellCounter(i, j) > solver.GetCellNumberValue(i, j)) {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool NumberIsClosedTooEarly() {

            for (int i = 0; i < solver.gridHeight; i++) {
                for (int j = 0; j < solver.gridWidth; j++) {
                    if (solver.CellIsNumber(i, j) && solver.GetCellCounter(i, j) < solver.GetCellNumberValue(i, j)) {

                        int value = solver.GetCellNumberValue(i, j);
                        int counter = solver.GetCellCounter(i, j);

                        CountPossibleFullPlacesForNumber(i, j, (value - counter));
                        if ((value - counter) > (emptyCells.Count + noneFullCells.Count())) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void CountPossibleFullPlacesForNumber(int row, int column, int cellsNeeded) {

            solver.passedFullCells = new List<Point>();
            emptyCells = new List<Point>();
            noneFullCells = new List<Point>();

            //solver.passedFullCells.Add(new Point(row, column));
            CountPossibleFullPlacesForNumber(row, column, 0, cellsNeeded);
        }

        private void CountPossibleFullPlacesForNumber(int row, int column, int unallowedDirection, int cellsNeeded) {

            solver.passedFullCells.Add(new Point(row, column));

            if ((emptyCells.Count + noneFullCells.Count()) >= cellsNeeded) {
                return;
            }

            // up
            if (unallowedDirection != 1 && (row - 1) >= 0) {
                if (solver.CellIsFull((row - 1), column) && !solver.FullCellIsAlreadyPassed((row - 1), column)) {
                    if (solver.GetCellId((row - 1), column) == 0) {
                        noneFullCells.Add(new Point((row - 1), column));
                    }
                    CountPossibleFullPlacesForNumber(((row - 1)), column, 3, cellsNeeded);
                } else if (solver.CellIsEmpty((row - 1), column) && !emptyCells.Contains(new Point((row - 1), column))) {
                    emptyCells.Add(new Point((row - 1), column));
                    CountPossibleFullPlacesForNumber(((row - 1)), column, 3, cellsNeeded);
                }
            }

            // right
            if (unallowedDirection != 2 && (column + 1) < solver.gridWidth) {
                if (solver.CellIsFull(row, (column + 1)) && !solver.FullCellIsAlreadyPassed(row, (column + 1))) {
                    if (solver.GetCellId(row, (column + 1)) == 0) {
                        noneFullCells.Add(new Point(row, (column + 1)));
                    }
                    CountPossibleFullPlacesForNumber(row, (column + 1), 4, cellsNeeded);
                } else if (solver.CellIsEmpty(row, (column + 1)) && !emptyCells.Contains(new Point(row, (column + 1)))) {
                    emptyCells.Add(new Point(row, (column + 1)));
                    CountPossibleFullPlacesForNumber(row, (column + 1), 4, cellsNeeded);
                }
            }

            // down
            if (unallowedDirection != 3 && (row + 1) < solver.gridHeight) {
                if (solver.CellIsFull((row + 1), column) && !solver.FullCellIsAlreadyPassed((row + 1), column)) {
                    if (solver.GetCellId((row + 1), column) == 0) {
                        noneFullCells.Add(new Point((row + 1), column));
                    }
                    CountPossibleFullPlacesForNumber((row + 1), column, 1, cellsNeeded);
                } else if (solver.CellIsEmpty((row + 1), column) && !emptyCells.Contains(new Point((row + 1), column))) {
                    emptyCells.Add(new Point((row + 1), column));
                    CountPossibleFullPlacesForNumber((row + 1), column, 1, cellsNeeded);
                }
            }

            // left
            if (unallowedDirection != 4 && (column - 1) >= 0) {
                if (solver.CellIsFull(row, (column - 1)) && !solver.FullCellIsAlreadyPassed(row, (column - 1))) {
                    if (solver.GetCellId(row, (column - 1)) == 0) {
                        noneFullCells.Add(new Point(row, (column - 1)));
                    }
                    CountPossibleFullPlacesForNumber(row, (column - 1), 2, cellsNeeded);
                } else if (solver.CellIsEmpty(row, (column - 1)) && !emptyCells.Contains(new Point(row, (column - 1)))) {
                    emptyCells.Add(new Point(row, (column - 1)));
                    CountPossibleFullPlacesForNumber(row, (column - 1), 2, cellsNeeded);
                }
            }
        }

        private bool ClosedFullsWithoutNumber() {

            for (int i = 0; i < solver.gridHeight; i++) {
                for (int j = 0; j < solver.gridWidth; j++) {
                    if (solver.CellIsFull(i, j) && solver.GetCellId(i, j) == 0) {
                        FullsAreClosed(i, j, 0, 1);
                        if (emptyCells.Count == 0 && numberCells.Count == 0) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void FullsAreClosed(int row, int column, int unallowedDirection, int cellsVisited) {

            if (cellsVisited == 1) {
                solver.passedFullCells = new List<Point>();
                emptyCells = new List<Point>();
                numberCells = new List<Point>();
            }
            solver.passedFullCells.Add(new Point(row, column));
            
            // up
            if (unallowedDirection != 1 && (row - 1) >= 0) {
                if (solver.CellIsFull((row - 1), column) && !solver.FullCellIsAlreadyPassed((row - 1), column)) {
                    FullsAreClosed((row - 1), column, 3, ++cellsVisited);
                } else if (solver.CellIsEmpty((row - 1), column)) {
                    emptyCells.Add(new Point((row - 1), column));
                    return;
                } else if (solver.CellIsNumber((row - 1), column)) {
                    numberCells.Add(new Point((row - 1), column));
                    return;
                }
            }

            // right
            if (unallowedDirection != 2 && (column + 1) < solver.gridWidth) {
                if (solver.CellIsFull(row, (column + 1)) && !solver.FullCellIsAlreadyPassed(row, (column + 1))) {
                    FullsAreClosed(row, (column + 1), 4, ++cellsVisited);
                } else if (solver.CellIsEmpty(row, (column + 1))) {
                    emptyCells.Add(new Point(row, (column + 1)));
                    return;
                } else if (solver.CellIsNumber(row, (column + 1))) {
                    numberCells.Add(new Point(row, (column + 1)));
                    return;
                }
            }

            // down
            if (unallowedDirection != 3 && (row + 1) < solver.gridHeight) {
                if (solver.CellIsFull((row + 1), column) && !solver.FullCellIsAlreadyPassed((row + 1), column)) {
                    FullsAreClosed((row + 1), column, 1, ++cellsVisited);
                } else if (solver.CellIsEmpty((row + 1), column)) {
                    emptyCells.Add(new Point((row + 1), column));
                    return;
                } else if (solver.CellIsNumber((row + 1), column)) {
                    numberCells.Add(new Point((row + 1), column));
                    return;
                }
            }

            // left
            if (unallowedDirection != 4 && (column - 1) >= 0) {
                if (solver.CellIsFull(row, (column - 1)) && !solver.FullCellIsAlreadyPassed(row, (column - 1))) {
                    FullsAreClosed(row, (column - 1), 2, ++cellsVisited);
                } else if (solver.CellIsEmpty(row, (column - 1))) {
                    emptyCells.Add(new Point(row, (column - 1)));
                    return;
                } else if (solver.CellIsNumber(row, (column - 1))) {
                    numberCells.Add(new Point(row, (column - 1)));
                    return;
                }
            }
        }
        
    }
}
