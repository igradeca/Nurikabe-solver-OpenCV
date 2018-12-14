using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NurikabeSolver {
    class Node {

        public int currentNumberOfBlackCells { get; set; }
        public int currentNumberOfFullCells { get; set; }
        public Cell[][] currentGrid { get; set; }
        public List<Node> children { get; set; }
        public bool correct { get; set; }
        public bool finished { get; set; }
        public int level { get; set; }

        public Node(Cell[][] puzzle, int currentNumberOfBlackCells, int currentNumberOfFullCells, int level) {

            this.currentNumberOfBlackCells = currentNumberOfBlackCells;
            this.currentNumberOfFullCells = currentNumberOfFullCells;

            currentGrid = MakeCopy(puzzle);
            
            children = new List<Node>();
            correct = true;
            finished = false;

            this.level = level;
        }

        private Cell[][] MakeCopy(Cell[][] inputPuzzle) {

            Cell[][] grid;
            int gridHeight = inputPuzzle.Length;
            int gridWidth = inputPuzzle[0].Length;

            grid = new Cell[gridHeight][];
            for (int i = 0; i < gridHeight; i++) {
                grid[i] = new Cell[gridWidth];
                for (int j = 0; j < gridWidth; j++) {
                    grid[i][j] = new Cell(i, j, inputPuzzle[i][j].charValue, inputPuzzle[i][j].id, inputPuzzle[i][j].counter);
                }
            }

            return grid;
        }

        public void AddCell(Cell value) {

            currentGrid[value.location.X][value.location.Y] = new Cell(
                value.location.X,
                value.location.Y,
                value.charValue,
                value.id,
                value.counter
                );            
        }


    }
}
