using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projekt_Nurikabe {
    public partial class Form1 : Form {

        private CaptureGrid captureGrid;

        public Form1() {

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {

            captureGrid = new CaptureGrid(imageBox1, imageBox2);
            captureGrid.Start();

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {

            captureGrid.Stop(true);
        }

    }
}
