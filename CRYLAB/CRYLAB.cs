using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using OpenTK;

namespace CRYLAB
{
    public partial class CRYLAB : Form
    {
        public SuperCell parent;
        public List<SuperCell> superList;
        public Plotter plotter;

        public CRYLAB()
        {
            InitializeComponent();
        }

        private void LoadParent()
        {
            openFileDialog.Multiselect = false;
            AtomsPerCellDialog dlg = new AtomsPerCellDialog();
            DialogResult result = dlg.ShowDialog();

            while (result == DialogResult.OK && !dlg.isNumber)
            {
                MessageBox.Show("That is not a number!");
                result = dlg.ShowDialog(this);
            }

            if (result == DialogResult.OK)
            {
                result = openFileDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    parent = SuperCell.ReadMol2_complex(openFileDialog.FileName, dlg.atomsPerCell);
                    if (parent.isError)
                    {
                        Calculator.HandleErrors(parent);
                        parent = null;
                    }
                    else
                    {
                        baseStructureTextBox.Text = Path.GetFileNameWithoutExtension(parent.filePath);
                    }
                }
            }
        }

        private void LoadChildren()
        {
            openFileDialog.Multiselect = true;

            if (parent == null) return; //TODO: Throw an error here instead!

            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                List<SuperCell> newChildren = Calculator.BatchImport(openFileDialog.FileNames, parent);
                superList.AddRange(newChildren);
                foreach (SuperCell child in newChildren)
                {
                    childListBox.Items.Add(Path.GetFileNameWithoutExtension(child.filePath));
                }
            }
        }

        private PlotStyle GetPlotStyle()
        {
            if (vectorsRadio.Checked) return PlotStyle.Directions;
            else if (moleculesRadio.Checked) return PlotStyle.Molecules;
            else if (curlsRadio.Checked) return PlotStyle.Curl;
            else return PlotStyle.Centers;
        }

        private void CRYLAB_Load(object sender, EventArgs e)
        {
            superList = new List<SuperCell>();
            plotter = new Plotter();
            plotter.colors  = new OpenTK.Graphics.Color4[] {Color.Aquamarine, Color.Aquamarine};
            plotter.molColors = new OpenTK.Graphics.Color4[] { Color.Gray, Color.White, Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet, Color.Brown };
            plotter.bgColor = Color.Black;
        }

        private void CRYLAB_Click(object sender, EventArgs e)
        {
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (parent != null)
            {
                plotter.Plot(parent, GetPlotStyle());
            }
        }

        private void baseStructureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadParent();
        }

        private void childStructuresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadChildren();
        }

        private void structureMatFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (plotter.isRunning)
            //    plotter.Pause();
        }

        private void browseParentButton_Click(object sender, EventArgs e)
        {
            LoadParent();
        }

        private void plotParentButton_Click(object sender, EventArgs e)
        {
            if (parent == null)
            {
                MessageBox.Show("Please first load a parent structure.");
            }
            else
            {
                plotter.Plot(parent, GetPlotStyle());
            }
        }

        private void plotSelectedButton_Click(object sender, EventArgs e)
        {
            if (childListBox.SelectedIndices.Count>1)
            {
                MessageBox.Show("I can only plot one at a time :(");
            }
            else if (childListBox.SelectedIndices.Count == 0)
            {
                MessageBox.Show("Please select at least one child structure.");
            }
            else
            {
                plotter.Plot(superList[childListBox.SelectedIndex], GetPlotStyle());
            }
        }

        private void browseChildrenButton_Click(object sender, EventArgs e)
        {
            LoadChildren();
        }

        private void removeSelectedButton_Click(object sender, EventArgs e)
        {
            if (childListBox.SelectedIndices.Count > 0)
            {
                int count = 0;
                List<int> selectedIndices = new List<int>();
                foreach (int index in childListBox.SelectedIndices)
                {
                    selectedIndices.Add(index);
                }
                foreach (int index in selectedIndices)
                {
                    superList.RemoveAt(index-count);
                    childListBox.Items.RemoveAt(index-count);
                    count++;
                }
            }
        }

        private void plotNextButton_Click(object sender, EventArgs e)
        {
            if (childListBox.SelectedIndices.Count == 1 && childListBox.SelectedIndex < (childListBox.Items.Count - 1))
            {
                int currentSelected = childListBox.SelectedIndex;
                childListBox.SetSelected(currentSelected + 1, true);
                childListBox.SetSelected(currentSelected, false);
            }
            else
            {
                int currentSelected = childListBox.SelectedIndex;
                childListBox.SetSelected(0, true);
                childListBox.SetSelected(currentSelected, false);
            }

            plotter.Plot(superList[childListBox.SelectedIndex], GetPlotStyle());
        }

        private void cloneButton_Click(object sender, EventArgs e)
        {
            if (childListBox.SelectedIndices.Count == 0)
            {
                SuperCell newChild = parent;
                newChild.isParent = false;
                superList.Add(newChild);
                childListBox.Items.Add(Path.GetFileNameWithoutExtension(newChild.filePath));
            }
            else
            {
                foreach (int index in childListBox.SelectedIndices)
                {
                    SuperCell newChild = superList[index];
                    superList.Add(newChild);
                    childListBox.Items.Add(Path.GetFileNameWithoutExtension(newChild.filePath));
                }
            }
        }

    }
}

