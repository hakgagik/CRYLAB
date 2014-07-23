using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics;

namespace CRYLAB
{
    public partial class PlotOptions : Form
    {
        Plotter plotter;
        Button[] mainColors;
        Button[] atomColors;
        Label[] atomLabels;

        public PlotOptions(Plotter plotter)
        {
            InitializeComponent();

            this.plotter = plotter;
        }

        private void PlotOptions_Load(object sender, EventArgs e)
        {
            backgroundButton.BackColor = (Color)plotter.bgColor;
            mainColors = new Button[] { main1Button, main2Button };
            atomColors = new Button[] { atom1Button, atom2Button, atom3Button, atom4Button, atom5Button, atom6Button, atom7Button, atom8Button, atom9Button, atom10Button };
            atomLabels = new Label[] { atom1Label, atom2Label, atom3Label, atom4Label, atom5Label, atom6Label, atom7Label, atom8Label, atom9Label, atom10Label };

            for (int i = 0; i < 2; i++)
            {
                mainColors[i].BackColor = (Color)plotter.colors[i];
            }

            for (int i = 0; i < 10; i++)
            {
                atomColors[i].BackColor = (Color)plotter.molColors[i];
            }

            if (((CRYLAB)this.Owner).parent != null)
            {
                string[] parentTypes = ((CRYLAB)this.Owner).parent.types.Distinct().ToArray();
                for (int i = 0; i < parentTypes.Length; i++)
                {
                    atomLabels[i].Text = "(" + parentTypes[i] + ")";
                }
            }
        }

        private void ChangeColor(Button button)
        {
            DialogResult result = colorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                button.BackColor = colorDialog.Color;
            }
        }

        private void backgroundButton_Click(object sender, EventArgs e)
        {
            ChangeColor((Button)sender);
        }

        private void main1Button_Click(object sender, EventArgs e)
        {
            ChangeColor((Button)sender);
        }

        private void main2Button_Click(object sender, EventArgs e)
        {
            ChangeColor((Button)sender);
        }

        private void atom1Button_Click(object sender, EventArgs e)
        {
            ChangeColor((Button)sender);
        }

        private void atom2Button_Click(object sender, EventArgs e)
        {
            ChangeColor((Button)sender);
        }

        private void atom3Button_Click(object sender, EventArgs e)
        {
            ChangeColor((Button)sender);
        }

        private void atom4Button_Click(object sender, EventArgs e)
        {
            ChangeColor((Button)sender);
        }

        private void atom5Button_Click(object sender, EventArgs e)
        {
            ChangeColor((Button)sender);
        }

        private void atom6Button_Click(object sender, EventArgs e)
        {
            ChangeColor((Button)sender);
        }

        private void atom7Button_Click(object sender, EventArgs e)
        {
            ChangeColor((Button)sender);
        }

        private void atom8Button_Click(object sender, EventArgs e)
        {
            ChangeColor((Button)sender);
        }

        private void atom9Button_Click(object sender, EventArgs e)
        {
            ChangeColor((Button)sender);
        }

        private void atom10Button_Click(object sender, EventArgs e)
        {
            ChangeColor((Button)sender);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            plotter.bgColor = backgroundButton.BackColor;
            for (int i = 0; i < 2; i++)
            {
                plotter.colors[i] = mainColors[i].BackColor;
            }
            for (int i = 0; i < 10; i++)
            {
                plotter.molColors[i] = atomColors[i].BackColor;
            }
        }
    }
}
