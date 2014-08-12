using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using OpenTK;
using MathNet.Numerics.LinearAlgebra.Double;

namespace CRYLAB
{
    public partial class CRYLAB : Form
    {
        public SuperCell parent;
        public List<SuperCell> superList;
        public Plotter plotter;

        public MousePositionFormat mousePositionFormat = MousePositionFormat.Fractional;

        public CRYLAB()
        {
            InitializeComponent();
        }

        private void LoadParent()
        {
            if (parent != null)
            {
                DialogResult msgResult = MessageBox.Show("This will clear all current supercells. Are you sure?", "Load new parent", MessageBoxButtons.YesNo);
                if (msgResult == DialogResult.No) return;
            }
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
                        superList.Clear();
                        childListBox.Items.Clear();
                        baseStructureTextBox.Text = Path.GetFileNameWithoutExtension(parent.filePath);
                        browseChildrenButton.Enabled = true;
                        cloneButton.Enabled = true;
                        removeSelectedButton.Enabled = true;
                        plotParentButton.Enabled = true;
                        childStructuresToolStripMenuItem.Enabled = true;
                    }
                }
            }
        }

        private void LoadChildren()
        {
            openFileDialog.Multiselect = true;

            if (parent == null)
            {
                MessageBox.Show("Please load a parent structure first.");
                return;
            }

            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                List<SuperCell> newChildren = Calculator.BatchImport(openFileDialog.FileNames, parent, this);
                superList.AddRange(newChildren);
                foreach (SuperCell child in newChildren)
                {
                    childListBox.Items.Add(Path.GetFileNameWithoutExtension(child.filePath));
                }
                if (childListBox.Items.Count > 0)
                {
                    plotSelectedButton.Enabled = true;
                    plotNextButton.Enabled = true;
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

        private FieldLineStyle GetFieldLines()
        {
            if (fieldLinesCheckBox.Checked)
            {
                if (singleRadio.Checked) return FieldLineStyle.Single;
                else return FieldLineStyle.Full;
            }
            else return FieldLineStyle.None;
        }

        private void Screenshot()
        {

            saveFileDialog.Filter = Extensions.Images;
            DialogResult result = saveFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                ImageFormat format;
                string filename = saveFileDialog.FileName;
                string extension = Path.GetExtension(filename);
                if (extension == ".png") format = ImageFormat.Png;
                else if (extension == ".gif") format = ImageFormat.Gif;
                else if (extension == ".jpg") format = ImageFormat.Jpeg;
                else format = ImageFormat.Bmp;

                Bitmap screenshot = plotter.Screenshot();
                if (screenshot != null) screenshot.Save(filename, format);
                else MessageBox.Show("Plot something first!");
            }
        }

        private void EditPlotOptions()
        {
            PlotOptions blah = new PlotOptions(plotter);
            blah.Owner = this;
            blah.Show();
        }

        private void CRYLAB_Load(object sender, EventArgs e)
        {
            superList = new List<SuperCell>();
            plotter = new Plotter(this);
            plotter.colors  = new OpenTK.Graphics.Color4[] {Color.Aquamarine, Color.Black};
            plotter.molColors = new OpenTK.Graphics.Color4[] { Color.Gray, Color.White, Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet, Color.Brown };
            plotter.bgColor = Color.Black;
        }

        private void CRYLAB_Click(object sender, EventArgs e)
        {
            if (progressBar.Value == 100) progressBar.Value = 0;
        }

        private void CRYLAB_FormClosing(object sender, FormClosingEventArgs e)
        {
            plotter.Exit();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Screenshot();
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
            //if (superList.Count == 0) return;
            //List<double> displacementNorms = new List<double>();
            //MathNet.Numerics.LinearAlgebra.Matrix<double> displacements = superList[0].centroids - superList[0].parent.centroids;
            //for (int i = 0; i < displacements.RowCount; i++)
            //{
            //    displacementNorms.Add(displacements.Row(i).L2Norm());
            //}

            //using (StreamWriter file = new StreamWriter(@"C:\Users\Gago\Desktop\displacementNorms.txt"))
            //{
            //    for (int i = 0; i < displacementNorms.Count; i++)
            //    {
            //        file.WriteLine(displacementNorms[i]);
            //    }
            //}

            //List<MathNet.Numerics.LinearAlgebra.Matrix<double>> E = superList[0].FirstOrderStrain();

            //using (StreamWriter file = new StreamWriter(@"C:\Users\Gago\Desktop\firstOrderStrain.txt"))
            //{
            //    for (int i = 0; i < E.Count; i++)
            //    {
            //        string line = "";
            //        for (int j = 0; j < 3; j++)
            //        {
            //            for (int k = 0; k < 3; k++)
            //            {
            //                line += " " + E[i][j, k];
            //            }
            //        }
            //        file.WriteLine(line);
            //    }
            //}

            //List<MathNet.Numerics.LinearAlgebra.Matrix<double>> T = superList[0].totalDirectionDerivatives;

            //using (StreamWriter file = new StreamWriter(@"C:\Users\Gago\Desktop\torqueGradient.txt"))
            //{
            //    for (int i = 0; i < E.Count; i++)
            //    {
            //        string line = "";
            //        MathNet.Numerics.LinearAlgebra.Matrix<double> blah = T[i];
            //        blah += blah.Transpose();
            //        blah /= 2.0;
            //        for (int j = 0; j < 3; j++)
            //        {
            //            for (int k = 0; k < 3; k++)
            //            {
            //                line += " " + blah[j, k];
            //            }
            //        }
            //        file.WriteLine(line);
            //    }
            //}
            
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
                if (!fieldLinesCheckBox.Checked)
                {
                    plotter.Plot(parent, GetPlotStyle());
                }
                else
                {
                    plotter.PlotWithFieldLines(parent, GetPlotStyle(), GetFieldLines(), 1.0);
                }
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
                MessageBox.Show("Please select a child structure.");
            }
            else
            {
                if (!fieldLinesCheckBox.Checked)
                {
                    plotter.Plot(superList[childListBox.SelectedIndex], GetPlotStyle());
                }
                else
                {

                    plotter.PlotWithFieldLines(superList[childListBox.SelectedIndex], GetPlotStyle(), GetFieldLines(), 1.0);
                }
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
            if (childListBox.Items.Count == 0)
            {
                plotNextButton.Enabled = false;
                plotSelectedButton.Enabled = false;
            }
        }

        private void plotNextButton_Click(object sender, EventArgs e)
        {
            if (childListBox.Items.Count == 0) return;
            if (childListBox.SelectedIndices.Count == 0) childListBox.SelectedIndex = childListBox.Items.Count - 1;
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
            childListBox.SelectedIndex %= childListBox.Items.Count;

            plotSelectedButton_Click(null, null);
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

        private void fieldLinesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (fieldLinesCheckBox.Checked) fieldLinesPanel.Enabled = true;
            else fieldLinesPanel.Enabled = false;
        }

        private void plotOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditPlotOptions();
        }

        private void testButton_Click(object sender, EventArgs e)
        {
            if (parent != null)
            {
                parent.Dislocate(DenseVector.OfArray(new double[] { 19, 25, 0 }), DenseVector.OfArray(new double[] { 0, 0, 1 }), DenseVector.OfArray(new double[] { 0, 0, 1 }), DenseVector.OfArray(new double[] { 1, 0, 0 }), 5.0);
                //parent.Dislocate(DenseVector.OfArray(new double[] { 29, 25, 0 }), DenseVector.OfArray(new double[] { 0, 0, 1 }), DenseVector.OfArray(new double[] { 0, 0, -1 }), DenseVector.OfArray(new double[] { 1, 0, 0 }), 5.0);
            }
        }
    }
}

