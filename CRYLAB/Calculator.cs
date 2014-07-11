using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using OpenTK;
using System.Windows.Forms;

namespace CRYLAB
{
    class Calculator
    {
        public static List<SuperCell> BatchImport(string[] filenames, SuperCell parent)
        {
            List<SuperCell> superList = new List<SuperCell>();
            foreach (string filename in filenames)
            {
                SuperCell child = SuperCell.ReadMol2_simple(filename, parent);
                if (child.isError)
                {
                    HandleErrors(child);
                }
                else
                {
                    superList.Add(child);
                }
            }
            return superList;
        }

        public static Matrix<double> CalculateCurls(SuperCell superCell)
        {
            throw new NotImplementedException();
        }

        public static double Clip(double min, double max, double input)
        {
            input = Math.Max(Math.Min(input, max), min);
            return input;
        }

        public static double[] UpdateExtents(double[] extents, Vector3d centroid)
        {
            extents[0] = Math.Min(extents[0], centroid[0]);
            extents[1] = Math.Max(extents[1], centroid[0]);
            extents[2] = Math.Min(extents[2], centroid[1]);
            extents[3] = Math.Max(extents[3], centroid[1]);
            extents[4] = Math.Min(extents[4], centroid[2]);
            extents[5] = Math.Max(extents[5], centroid[2]);
            return extents;
        }

        public static void HandleErrors(SuperCell superCell)
        {
            string errorString = "The file at " + superCell.filePath + " could not be loaded for the following reason:\n\n";


            switch (superCell.errorType)
            {
                case SuperCellError.NoError:
                    errorString += "I have no idea what happened...";
                    break;
                case SuperCellError.Child_StructureMismatch:
                    errorString += "This child tructure does not match the parent structure.";
                    break;
                case SuperCellError.Base_NoBonds:
                    errorString += "The .mol2 file does not contain bond information. Was it edited?";
                    break;
                case SuperCellError.Base_NoLattice:
                    errorString += "The .mol2 file does not contain crystal lattice information.\n\n";
                    errorString += "Hint: To use this file as a parent structure, make sure the last two lines directly following bond information are:\n\n";
                    errorString += "@<TRIPOS>CRYSIN\n";
                    errorString += "(a*CellA) (b*CellB) (c*CellC) alpha beta gamma\n\n";
                    errorString += "Where CellA, CellB, and CellC are number of cells in the a, b, and c directions.";
                    break;
                case SuperCellError.Base_CouldntFindOrder:
                    errorString += "Could not find discrete molecules with the specified number of atoms per cell.";
                    break;
                case SuperCellError.Base_SingleMol:
                    errorString += "CRYLAB does not currently support importing a single molecule.";
                    break;
                case SuperCellError.Base_LineCell:
                    errorString += "CRYLAB does not currently support single line supercells";
                    break;
            }

            MessageBox.Show(errorString);
        }
    }
}
