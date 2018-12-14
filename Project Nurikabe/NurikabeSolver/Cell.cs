using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NurikabeSolver {
    public class Cell {

        public char charValue { get; set; }
        public int intValue { get; set; }
        public int id { get; set; }
        public Point location { get;}
        public int counter { get; set; }

        public Cell(int row, int column, char value, int id) {

            location = new Point(row, column);
            charValue = value;
            SetIntValue();
            this.id = id;
            counter = 1;
        }

        public Cell(int row, int column, char value, int id, int fullCounter) {

            location = new Point(row, column);
            charValue = value;
            SetIntValue();
            this.id = id;
            this.counter = fullCounter;
        }

        private void SetIntValue() {

            if (charValue == 't') {
                intValue = 10;
            } else if (charValue == 'e') {
                intValue = 11;
            } else if (charValue == 'w') {
                intValue = 12;
            } else if (charValue == 'h') {
                intValue = 13;
            } else if (charValue != 'B' && charValue != 'F' && charValue != '0') {
                intValue = int.Parse(charValue.ToString());
            }
        }

    }
}
