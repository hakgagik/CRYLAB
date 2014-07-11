using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CRYLAB
{
    public partial class AtomsPerCellDialog : Form
    {
        public int atomsPerCell;
        public bool isNumber = false;

        public AtomsPerCellDialog()
        {
            InitializeComponent();
        }

        private void textBox1_MouseDown(object sender, MouseEventArgs e)
        {
            textBox1.SelectAll();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            isNumber = int.TryParse(textBox1.Text, out atomsPerCell);
        }

    }
}
