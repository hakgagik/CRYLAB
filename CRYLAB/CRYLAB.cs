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
        public List<SuperCell> superList;

        public CRYLAB()
        {
            InitializeComponent();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            superList = new List<SuperCell>();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //double[,] testAtoms = new double[,] { { 1, 0, 0 }, { 1.5, 0.003, 0 }, { 0.9, 0.0032, 0.001 }, { 1.123, 0, 0.00023 } };
            //Molecule testMols = new Molecule(testAtoms);
            //MessageBox.Show(testMols.direction.ToString());
        }

        private void toolStripStatusLabel_Click(object sender, EventArgs e)
        {
        }

        private void baseStructureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.Multiselect = false;
            if (atomsPerCell_TextBox.Text != null)
            {
                int atomsPerCell = int.Parse(atomsPerCell_TextBox.Text);
                DialogResult result = openFileDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    parent = SuperCell.ReadMol2_complex(openFileDialog.FileName, atomsPerCell);
                }
            }
        }

        private void childStructuresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.Multiselect = true;

            if (parent == null) return; //TODO: Throw an error here instead!

            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                superList.AddRange( HelperFunctions.BatchImport(openFileDialog.FileNames, parent));
            }
        }
    }
}

