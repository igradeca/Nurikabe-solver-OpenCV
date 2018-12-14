using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NurikabeSolver {
    class Heuristic {

        /*
         * 'B' - Black cell
         * 'F' - Filled cell (Dot)
        */
        private NurikabeSolver solver;

        public Heuristic(NurikabeSolver solver) {

            this.solver = solver;
        }

        internal void Start() {

            TechniqueUnreachableSquare();

            for (int i = 0; i < solver.gridHeight; i++) {
                for (int j = 0; j < solver.gridWidth; j++) {

                    TechniqueIslandOfOne(i, j);
                    TechniqueSeparateByOneSquare(i, j);
                    TechniqueDiagonallyAdjacent(i, j);
                    TechniqueSurroundedSquare(i, j);
                    TechniqueExpandableOnlyInTwoDirectionsForNumberTwo(i, j);
                }
            }
            
        }

        internal void Basic() {

            solver.moveIsMade = true;
            while (solver.moveIsMade) {
                solver.moveIsMade = false;
                /*
                for (int i = 0; i < solver.gridHeight; i++) {
                    for (int j = 0; j < solver.gridWidth; j++) {

                        TechniqueSurroundedSquare(i, j);

                        TechniqueExcerptBlackCell(i, j);
                        TechniqueExpandableOnlyInTwoDirectionsForNumberTwo(i, j);
                        TechniqueAvoidingWallArea(i, j);

                        TechniqueExcerptFulls(i, j);
                        TechniqueExcerptNumber(i, j);
                    }
                }
                */
                
                for (int i = solver.gridHeight - 1; i >= 0 ; i--) {
                    for (int j = solver.gridWidth - 1; j >= 0; j--) {

                        TechniqueSurroundedSquare(i, j);

                        TechniqueExcerptBlackCell(i, j);
                        TechniqueExpandableOnlyInTwoDirectionsForNumberTwo(i, j);
                        TechniqueAvoidingWallArea(i, j);

                        TechniqueExcerptFulls(i, j);
                        TechniqueExcerptNumber(i, j);
                    }
                }
                
                if (solver.moveIsMade == false) {
                    break;
                }
                
            }
        }

        /// <summary>
        /// Okružuje horizontalno i vertikalno broj 1 sa crnim poljima.
        /// </summary>
        private void TechniqueIslandOfOne(int i, int j) {

            if (solver.puzzle[i][j].charValue == '1') {
                solver.SurroundCellWithBlacks(i, j);
            }
        }

        /// <summary>
        /// Ako imamo 2 polja koja su vrijednost ili broj, a između njih je prazno polje, to polje onda obojamo u crno.
        /// </summary>
        private void TechniqueSeparateByOneSquare(int i, int j) {

            if (solver.puzzle[i][j].charValue != '1' && solver.CellIsNumber(i, j)
                        && solver.GetCellNumberValue(i, j) == (solver.GetCellCounter(i, j) + 1)) {
                CheckNeighboursHorizontalAndVertical(i, j);
            } else if (solver.CellIsFull(i, j) && solver.GetCellId(i, j) != 0) {

                int row = solver.numbers[solver.GetCellId(i, j)].X;
                int column = solver.numbers[solver.GetCellId(i, j)].Y;

                if (solver.GetCellNumberValue(row, column) == (solver.GetCellCounter(row, column) + 1)) {
                    CheckNeighboursHorizontalAndVertical(i, j);
                }
            }
        }

        /// <summary>
        /// Provjeri da li je ćelija za 2 mjesta od trenutne ćelije broj ili puna. Ako je -> zacrni ćeliju između njih.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        private void CheckNeighboursHorizontalAndVertical(int row, int column) {

            if ((row - 2) >= 0) {
                if (solver.CellIsNumber((row - 2), column) || solver.CellIsFull((row - 2), column)) {
                    solver.UpdateCell((row - 1), column, 'B', -1);
                }
            }
            if ((row + 2) < solver.gridHeight) {
                if (solver.CellIsNumber((row + 2), column) || solver.CellIsFull((row + 2), column)) {
                    solver.UpdateCell((row + 1), column, 'B', -1);
                }
            }

            if ((column - 2) >= 0) {
                if (solver.CellIsNumber(row, (column - 2)) || solver.CellIsFull(row, (column - 2))) {
                    solver.UpdateCell(row, (column - 1), 'B', -1);
                }
            }
            if ((column + 2) < solver.gridWidth) {
                if (solver.CellIsNumber(row, (column + 2)) || solver.CellIsFull(row, (column + 2))) {
                    solver.UpdateCell(row, (column + 1), 'B', -1);
                }
            }
        }

        private void TechniqueDiagonallyAdjacent(int i, int j) {

            if (solver.puzzle[i][j].charValue != '1' && solver.CellIsNumber(i, j)) {
                CheckNeighboursDiagonal(i, j);
            }
        }

        /// <summary>
        /// Provjerava da li ćelija ima po diagonalama za susjede brojeve.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        private void CheckNeighboursDiagonal(int row, int column) {

            if (((row - 1) >= 0) && ((column - 1) >= 0)) {
                if (solver.CellIsNumber((row - 1), (column - 1))) {
                    solver.UpdateCell((row - 1), column, 'B', -1);
                    solver.UpdateCell(row, (column - 1), 'B', -1);
                }
            }
            if (((row - 1) >= 0) && ((column + 1) < solver.gridWidth)) {
                if (solver.CellIsNumber((row - 1), (column + 1))) {
                    solver.UpdateCell((row - 1), column, 'B', -1);
                    solver.UpdateCell(row, (column + 1), 'B', -1);
                }
            }
        }

        /// <summary>
        /// Ako imamo praznu ćeliju koja je okružena horizontalno i vertikalno 
        /// sa crnim poljima, onda tu središnju ćeliju stavit isto da je crna.
        /// </summary>
        private void TechniqueSurroundedSquare(int i, int j) {

            if (solver.CellIsEmpty(i, j) && IsCellSurroundedWithBlack(i, j)) {
                solver.UpdateCell(i, j, 'B', -1);
            }
        }

        /// <summary>
        /// Provjerava da li je određena ćelija okružena (horizontalno i vertikalno) sa crnim poljima.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        internal bool IsCellSurroundedWithBlack(int row, int column) {

            int filledCells = 0;
            if ((row - 1) >= 0) {
                if (solver.CellIsBlack((row - 1), column)) {
                    ++filledCells;
                }
            } else if ((row - 1) < 0) {
                ++filledCells;
            }

            if ((row + 1) < solver.gridHeight) {
                if (solver.CellIsBlack((row + 1), column)) {
                    ++filledCells;
                }
            } else if ((row + 1) >= solver.gridHeight) {
                ++filledCells;
            }

            if ((column - 1) >= 0) {
                if (solver.CellIsBlack(row, (column - 1))) {
                    ++filledCells;
                }
            } else if ((column - 1) < 0) {
                ++filledCells;
            }

            if ((column + 1) < solver.gridWidth) {
                if (solver.CellIsBlack(row, (column + 1))) {
                    ++filledCells;
                }
            } else if ((column + 1) >= solver.gridWidth) {
                ++filledCells;
            }

            if (filledCells == 4) {
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Izvuće crnu ćeliju ako se može širiti samo u jednom smjeru.
        /// </summary>
        private void TechniqueExcerptBlackCell(int i, int j) {

            if (solver.CellIsBlack(i, j)) {
                int wayOutDirection = BlackCellWayOutDirection(i, j);
                if (wayOutDirection != 0) {
                    ExcerptCell(i, j, wayOutDirection, 'B', -1);
                }
            }
        }

        /// <summary>
        /// 1 - up. 2 - right. 3 - down. 4 - left.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private int BlackCellWayOutDirection(int row, int column) {

            int wayOut = 0;
            int wayOutsCounter = 0;

            // up
            if ((row - 1) >= 0 && (!solver.CellIsFull((row - 1), column) && !solver.CellIsNumber((row - 1), column))) {
                if (solver.CellIsEmpty((row - 1), column)) {
                    wayOut = 1;
                    ++wayOutsCounter;
                } else if (solver.BlackCellHasWayOut((row - 1), column, 3, 0)) {
                    ++wayOutsCounter;
                }
            }

            //right
            if ((column + 1) < solver.gridWidth && (!solver.CellIsFull(row, (column + 1)) && !solver.CellIsNumber(row, (column + 1)))) {
                if (solver.CellIsEmpty(row, (column + 1))) {
                    wayOut = 2;
                    ++wayOutsCounter;
                } else if (solver.BlackCellHasWayOut(row, (column + 1), 4, 0)) {
                    ++wayOutsCounter;
                }
            }

            // down
            if ((row + 1) < solver.gridHeight && (!solver.CellIsFull((row + 1), column) && !solver.CellIsNumber((row + 1), column))) {
                if (solver.CellIsEmpty((row + 1), column)) {
                    wayOut = 3;
                    ++wayOutsCounter;
                } else if (solver.BlackCellHasWayOut((row + 1), column, 1, 0)) {
                    ++wayOutsCounter;
                }
            }

            //left
            if ((column - 1) >= 0 && (!solver.CellIsFull(row, (column - 1)) && !solver.CellIsNumber(row, (column - 1)))) {
                if (solver.CellIsEmpty(row, (column - 1))) {
                    wayOut = 4;
                    ++wayOutsCounter;
                } else if (solver.BlackCellHasWayOut(row, (column - 1), 2, 0)) {
                    ++wayOutsCounter;
                }
            }

            if (wayOutsCounter > 1) {
                wayOut = 0;
            }
            return wayOut;
        }

        private void TechniqueExcerptNumber(int i, int j) {

            if (!solver.CellIsNumberOne(i, j) && solver.CellIsNumber(i, j)) {
                FillNumbersCells(i, j);
            }
        }

        internal void FillNumbersCells(int row, int column) {

            int nextRow = row;
            int nextColumn = column;
            int unallowedDirection = 0;
            int cellNumberId = solver.GetCellId(row, column);
            int cellNumberValue = solver.GetCellNumberValue(row, column);
            int newCellsSetCounter = 0;

            for (int i = 1; i < cellNumberValue; i++) {
                int direction = NumberCellWayOutDirection(nextRow, nextColumn, unallowedDirection);
                if (direction != 0) {
                    ExcerptCell(nextRow, nextColumn, direction, 'F', cellNumberId);
                    switch (direction) {
                        case 1:
                            nextRow -= 1;
                            unallowedDirection = 3;
                            break;
                        case 2:
                            nextColumn += 1;
                            unallowedDirection = 4;
                            break;
                        case 3:
                            nextRow += 1;
                            unallowedDirection = 1;
                            break;
                        case 4:
                            nextColumn -= 1;
                            unallowedDirection = 2;
                            break;
                    }
                    ++newCellsSetCounter;
                } else {
                    break;
                }
            }
            /*
            if (newCellsSetCounter == (cellNumberValue - 2)) {
                BoundaryForTwoDirectionsOnly(nextRow, nextColumn);
            }
            */
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="unallowedDirection"></param>
        /// <returns></returns>
        private int NumberCellWayOutDirection(int row, int column, int unallowedDirection) {

            int wayOut = 0;
            int wayOutsCounter = 0;

            // up
            if (unallowedDirection != 1 && (row - 1) >= 0) {
                if (solver.CellIsEmpty((row - 1), column) || solver.CellIsFull((row - 1), column)) {
                    wayOut = 1;
                    ++wayOutsCounter;
                }
            }

            //right
            if (unallowedDirection != 2 && (column + 1) < solver.gridWidth) {
                if (solver.CellIsEmpty(row, (column + 1)) || solver.CellIsFull(row, (column + 1))) {
                    wayOut = 2;
                    ++wayOutsCounter;
                }
            }

            // down
            if (unallowedDirection != 3 && (row + 1) < solver.gridHeight) {
                if (solver.CellIsEmpty((row + 1), column) || solver.CellIsFull((row + 1), column)) {
                    wayOut = 3;
                    ++wayOutsCounter;
                }
            }

            //left
            if (unallowedDirection != 4 && (column - 1) >= 0) {
                if (solver.CellIsEmpty(row, (column - 1)) || solver.CellIsFull(row, (column - 1))) {
                    wayOut = 4;
                    ++wayOutsCounter;
                }
            }

            if (wayOutsCounter > 1) {
                wayOut = 0;
            }
            return wayOut;
        }

        private void TechniqueExcerptFulls(int i, int j) {

            if (solver.CellIsFull(i, j) && solver.GetCellId(i, j) == 0) { // ovaj GetCellId(i, j) == 0 bi mogel biti problematican
                FillFullCells(i, j);
            }
        }

        private void FillFullCells(int row, int column) {

            int nextRow = row;
            int nextColumn = column;
            int unallowedDirection = 0;
            int cellNumberValue = solver.maxNumberOfFullCells;
            int newCellsSetCounter = 0;

            List<int[]> passedFulls = new List<int[]>();
            passedFulls.Add(new int[] { row, column });

            for (int i = 1; i < solver.maxNumberOfFullCells; i++) {
                int direction = FullCellWayOutDirection(nextRow, nextColumn, unallowedDirection);
                if (direction != 0) {
                    ExcerptCell(nextRow, nextColumn, direction, 'F', 0);                    
                    switch (direction) {
                        case 1:
                            nextRow -= 1;
                            unallowedDirection = 3;
                            break;
                        case 2:
                            nextColumn += 1;
                            unallowedDirection = 4;
                            break;
                        case 3:
                            nextRow += 1;
                            unallowedDirection = 1;
                            break;
                        case 4:
                            nextColumn -= 1;
                            unallowedDirection = 2;
                            break;
                    }
                    passedFulls.Add(new int[] { nextRow, nextColumn });
                    ++newCellsSetCounter;
                } else {
                    break;
                }

                if (solver.CellIsNumber(nextRow, nextColumn) || (solver.CellIsFull(nextRow, nextColumn) && solver.GetCellId(nextRow, nextColumn) > 0)) {
                    int newNumberId = solver.GetCellId(nextRow, nextColumn);

                    for (int j = 0; j < (passedFulls.Count - 1); j++) {
                        solver.UpdateCell(passedFulls[j][0], passedFulls[j][1], 'F', newNumberId);
                    }
                    return;
                }
            }
        }

        private int FullCellWayOutDirection(int row, int column, int unallowedDirection) {

            int wayOut = 0;
            int wayOutsCounter = 0;

            // up
            if (unallowedDirection != 1 && (row - 1) >= 0) {
                if (solver.CellIsNumber((row - 1), column) || (solver.CellIsFull((row - 1), column) && solver.GetCellId((row - 1), column) > 0)) {
                    return 1;
                } else if (solver.CellIsEmpty((row - 1), column) || solver.CellIsFull((row - 1), column)) {
                    wayOut = 1;
                    ++wayOutsCounter;
                } 
            }

            //right
            if (unallowedDirection != 2 && (column + 1) < solver.gridWidth) {
                if (solver.CellIsNumber(row, (column + 1)) || (solver.CellIsFull(row, (column + 1)) && solver.GetCellId(row, (column + 1)) > 0)) {
                    return 2;
                } else if (solver.CellIsEmpty(row, (column + 1)) || solver.CellIsFull(row, (column + 1))) {
                    wayOut = 2;
                    ++wayOutsCounter;
                }
            }

            // down
            if (unallowedDirection != 3 && (row + 1) < solver.gridHeight) {
                if (solver.CellIsNumber((row + 1), column) || (solver.CellIsFull((row + 1), column) && solver.GetCellId((row + 1), column) > 0)) {
                    return 3;
                } else if (solver.CellIsEmpty((row + 1), column) || solver.CellIsFull((row + 1), column)) {
                    wayOut = 3;
                    ++wayOutsCounter;
                }
            }

            //left
            if (unallowedDirection != 4 && (column - 1) >= 0) {
                if (solver.CellIsNumber(row, (column - 1)) || (solver.CellIsFull(row, (column - 1)) && solver.GetCellId(row, (column - 1)) > 0)) {
                    return 4;
                } else if (solver.CellIsEmpty(row, (column - 1)) || solver.CellIsFull(row, (column - 1))) {
                    wayOut = 4;
                    ++wayOutsCounter;
                }
            }

            if (wayOutsCounter > 1) {
                wayOut = 0;
            }
            return wayOut;
        }

        /// <summary>
        /// Prema danom smjeru izvlači danu ćeliju za određenu vrijednost i id.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="direction"></param>
        /// <param name="cellValue"></param>
        /// <param name="id"></param>
        private void ExcerptCell(int row, int column, int direction, char cellValue, int id) {

            switch (direction) {
                case 1:             // up
                    solver.UpdateCell((row - 1), column, cellValue, id);
                    break;
                case 2:             // right
                    solver.UpdateCell(row, (column + 1), cellValue, id);
                    break;
                case 3:             // down
                    solver.UpdateCell((row + 1), column, cellValue, id);
                    break;
                case 4:             // left
                    solver.UpdateCell(row, (column - 1), cellValue, id);
                    break;
            }
        }

        /// <summary>
        /// Rekurzivna funkcija. Gleda da li niz crnih polja ima izlaz.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="unallowedDirection">1 - up. 2 - right. 3 - down. 4 - left.</param>
        /// <returns></returns>

        private void TechniqueExpandableOnlyInTwoDirectionsForNumberTwo(int i, int j) {

            if (solver.puzzle[i][j].charValue == '2') {
                BoundaryForTwoDirectionsOnly(i, j);
            }
        }

        private void BoundaryForTwoDirectionsOnly(int row, int column) {

            int expandingDirection;
            expandingDirection = GetDiagonalBoundary(row, column);
            if (expandingDirection != 0) {
                switch (expandingDirection) {
                    case 1:
                        solver.UpdateCell((row - 1), (column + 1), 'B', -1);
                        break;
                    case 2:
                        solver.UpdateCell((row + 1), (column + 1), 'B', -1);
                        break;
                    case 3:
                        solver.UpdateCell((row + 1), (column - 1), 'B', -1);
                        break;
                    case 4:
                        solver.UpdateCell((row - 1), (column - 1), 'B', -1);
                        break;
                }
            }
        }

        /// <summary>
        /// Ako se neka ćelija može proširiti samo u 2 smjera 
        /// pod pravim kutem, vrati dijagonalu u koju se može širiti.
        /// 1 - gore-desno, 2 - dolje-desno, 3 - dolje-lijevo, 4 - gore-lijevo
        /// </summary>
        private int GetDiagonalBoundary(int row, int column) {

            int wayOutsCounter = 0;
            int wayOut = 0;

            // up and right
            if ((row - 1) >= 0 && (column + 1) < solver.gridWidth) {
                if (solver.CellIsEmpty((row - 1), column) && solver.CellIsEmpty(row, (column + 1))) {
                    wayOut = 1;
                    ++wayOutsCounter;
                }
            }

            // down and right
            if ((row + 1) < solver.gridHeight && (column + 1) < solver.gridWidth) {
                if (solver.CellIsEmpty((row + 1), column) && solver.CellIsEmpty(row, (column + 1))) {
                    wayOut = 2;
                    ++wayOutsCounter;
                }
            }

            // down and left
            if ((row + 1) < solver.gridHeight && (column - 1) >= 0) {
                if (solver.CellIsEmpty((row + 1), column) && solver.CellIsEmpty(row, (column - 1))) {
                    wayOut = 3;
                    ++wayOutsCounter;
                }
            }

            //up and left
            if ((row - 1) >= 0 && (column - 1) >= 0) {
                if (solver.CellIsEmpty((row - 1), column) && solver.CellIsEmpty(row, (column - 1))) {
                    wayOut = 4;
                    ++wayOutsCounter;
                }
            }

            if (wayOutsCounter > 1) {
                wayOut = 0;
            }
            return wayOut;
        }

        /// <summary>
        /// Prolazi kroz sve brojeve i gleda koji su im najveći dohvati. One ćelije koje brojevi dohvate
        /// su označene. Na kraju ćelije koje nisu označene oboja u crno jer tam ne može nitko doći.
        /// </summary>
        private void TechniqueUnreachableSquare() {
            
            Cell[][] tempPuzzle = new Cell[solver.gridHeight][];
            for (int i = 0; i < solver.gridHeight; i++) {
                tempPuzzle[i] = new Cell[solver.gridWidth];
                for (int j = 0; j < solver.gridWidth; j++) {
                    tempPuzzle[i][j] = new Cell(i, j, solver.puzzle[i][j].charValue, solver.puzzle[i][j].id);
                }
            }
            for (int i = 0; i < solver.gridHeight; i++) {
                for (int j = 0; j < solver.gridWidth; j++) {

                    if (solver.puzzle[i][j].charValue != '1' && solver.CellIsNumber(i, j)) {
                        int cellValue = solver.GetCellNumberValue(i, j) + 1;
                        MarkCells(ref tempPuzzle, i, j, cellValue);
                    }
                }
            }

            for (int i = 0; i < solver.gridHeight; i++) {
                for (int j = 0; j < solver.gridWidth; j++) {

                    if (tempPuzzle[i][j].charValue != 'X') {
                        solver.UpdateCell(i, j, 'B', -1);
                    }
                }
            }
        }

        /// <summary>
        /// Označava ćelije koje se mogu dosegnuti od određenog broja.
        /// </summary>
        /// <param name="tempPuzzle"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="cellValue"></param>
        private void MarkCells(ref Cell[][] tempPuzzle, int row, int column, int cellValue) {

            for (int i = 0; i < (cellValue - 1); i++) {
                for (int j = 0; j < (cellValue - 1 - i); j++) {
                    if (i != 0 || j != 0) {
                        if ((row + i) < solver.gridHeight && (column + j) < solver.gridWidth) {
                            if (tempPuzzle[(row + i)][(column + j)].charValue != 'X') {
                                tempPuzzle[(row + i)][(column + j)].charValue = 'X';
                            }
                            
                        }
                        if ((row + i) < solver.gridHeight && (column - j) >= 0) {
                            if (tempPuzzle[(row + i)][(column - j)].charValue != 'X') {
                                tempPuzzle[(row + i)][(column - j)].charValue = 'X';
                            }                            
                        }
                        if ((row - i) >= 0 && (column + j) < solver.gridWidth) {
                            if (tempPuzzle[(row - i)][(column + j)].charValue != 'X') {
                                tempPuzzle[(row - i)][(column + j)].charValue = 'X';
                            }                            
                        }
                        if ((row - i) >= 0 && (column - j) >= 0) {
                            if (tempPuzzle[(row - i)][(column - j)].charValue != 'X') {
                                tempPuzzle[(row - i)][(column - j)].charValue = 'X';
                            }                            
                        }
                    }
                }
            }
        }

        private void TechniqueAvoidingWallArea(int i, int j) {

            if (solver.CellIsEmpty(i, j)) {
                CheckAreaForThreeBlacks(i, j);
            }
        }

        private void CheckAreaForThreeBlacks(int row, int column) {

            int[][] startDot = new int[4][];
            if ((row - 1) >= 0 && (column - 1) >= 0) {
                startDot[0] = new int[] { (row - 1), (column - 1) };
            }
            if ((row - 1) >= 0) {
                startDot[1] = new int[] { (row - 1), column };
            }
            if ((column - 1) >= 0) {
                startDot[2] = new int[] { row, (column - 1) };
            }
            startDot[3] = new int[] { row, column };

            for (int i = 0; i < startDot.Length; i++) {
                if (startDot[i] != null) {

                    int blackCellsFound = 0;
                    if (solver.CellIsBlack(startDot[i][0], startDot[i][1])) {
                        ++blackCellsFound;
                    }
                    if ((startDot[i][1] + 1) < solver.gridWidth &&
                        solver.CellIsBlack(startDot[i][0], startDot[i][1] + 1)) {
                        ++blackCellsFound;
                    }
                    if ((startDot[i][0] + 1) < solver.gridHeight &&
                        solver.CellIsBlack(startDot[i][0] + 1, startDot[i][1])) {
                        ++blackCellsFound;
                    }
                    if ((startDot[i][0] + 1) < solver.gridHeight && (startDot[i][1] + 1) < solver.gridWidth &&
                        solver.CellIsBlack(startDot[i][0] + 1, startDot[i][1] + 1)) {
                        ++blackCellsFound;
                    }

                    if (blackCellsFound == 3) {
                        solver.UpdateCell(row, column, 'F', 0);
                        FillFullCells(row, column);
                    }
                }
            }
        }



    }
}
