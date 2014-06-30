using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;

namespace CRYLAB
{
    public partial class CRYLAB : Form
    {
        public SuperCell parent;
        public Queue<SuperCell> superQueue;

        public CRYLAB()
        {
            InitializeComponent();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //double[,] testAtoms = new double[,] { { 1, 0, 0 }, { 1.5, 0.003, 0 }, { 0.9, 0.0032, 0.001 }, { 1.123, 0, 0.00023 } };
            //Molecule testMols = new Molecule(testAtoms);
            //MessageBox.Show(testMols.direction.ToString());
        }

        private void withBondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int atomsPerCell = 26;
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                SuperCell superCell = SuperCell.ReadMol2_complex(openFileDialog.FileName, atomsPerCell);
            }
        }

        private void withoutBondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                SuperCell superCell = new SuperCell();
                superCell = SuperCell.ReadMol2_simple(filePath, superCell);
            }
        }

        private void toolStripStatusLabel_Click(object sender, EventArgs e)
        {
        }
    }
}

