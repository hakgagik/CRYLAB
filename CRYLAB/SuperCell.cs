#define _DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.IO;


// JUST TESTING
using System.Windows.Forms;

namespace CRYLAB
{
    public class Molecule
    {
        public Matrix<double> atoms;
        public int numAtoms;
        public Vector<double> centroid;
        public Vector<double> direction;
        public Vector<double> S;
        public Matrix<double> VT;
        public Molecule(double[,] inputAtoms, bool getDirections)
        {
            numAtoms = inputAtoms.GetLength(0);
            atoms = DenseMatrix.OfArray(inputAtoms);
            centroid = atoms.ColumnSums() / (double)numAtoms;

            if (getDirections)
            {
                for (int i = 0; i < 3; i++)
                {
                    atoms.SetColumn(i, atoms.Column(i) - centroid[i]);
                }
                direction = atoms.Svd(true).VT.Row(0);
                if ((atoms.Row(atoms.RowCount - 1) - atoms.Row(0)).DotProduct(direction) < 0)
                {
                    direction *= -1;
                }
            }
        }

    }

    public class SuperCell
    {
        public Queue<Molecule> mols;
        public Matrix<double> centroids;
        public Matrix<double> directions;


        public bool isParent;
        public int[] superCellSize;
        public Matrix<double> latticeVectors;
        public double[,] latticeParams;

        public int[,] order;

        public char[] types;
        public int[,] bonds;
        public string[] bondTypes;

        public SuperCell() { }

        public SuperCell(SuperCell parent)
        {
            isParent = false;
            superCellSize = parent.superCellSize;
            latticeVectors = parent.latticeVectors;
            latticeParams = parent.latticeParams;

            order = parent.order;

        }

        public static SuperCell ReadMol2_simple(String filePath, SuperCell parent){

            StreamReader reader = File.OpenText(filePath);
            string line;
            bool startReading = false;

            SuperCell superCell = new SuperCell(parent);

#if _DEBUG
            superCell.superCellSize = new int[] { 38, 38, 1 };
            superCell.order = new int[,] { { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 } };
#endif

            int atomsPerCell = superCell.order.Length;
            int atomsPerMol = superCell.order.GetLength(0);
            int molsPerCell = superCell.order.GetLength(1);

#if _DEBUG

            atomsPerMol = superCell.order.GetLength(1);
            molsPerCell = superCell.order.GetLength(0);

#endif

            int numMols = superCell.superCellSize[0] * superCell.superCellSize[1] * superCell.superCellSize[2];
            int numCells = numMols * molsPerCell;
            double[,] atoms = new double[atomsPerMol, 3];


            Queue<string> molLines = new Queue<string>();
            superCell.mols = new Queue<Molecule>();

            while (true)
            {
                if (startReading)
                {
                    for (int cell = 0; cell < numCells; cell++)
                    {
                        double[,] atomsBuffer = new double[atomsPerCell, 3];
                        for (int atom = 0; atom < atomsPerCell; atom++)
                        {
                            line = reader.ReadLine();
                            string[] items = line.Split(' ');

                            atomsBuffer[atom, 0] = double.Parse(items[2]);
                            atomsBuffer[atom, 1] = double.Parse(items[3]);
                            atomsBuffer[atom, 2] = double.Parse(items[4]);
                        }
                        for (int mol = 0; mol < molsPerCell; mol++)
                        {
                            double[,] thisMol = new double[atomsPerMol, 3];
                            for (int atom = 0; atom < atomsPerMol; atom++)
                            {
                                thisMol[atom, 0] = atomsBuffer[superCell.order[mol, atom], 0];
                                thisMol[atom, 1] = atomsBuffer[superCell.order[mol, atom], 1];
                                thisMol[atom, 2] = atomsBuffer[superCell.order[mol, atom], 2];
                            }
                            superCell.mols.Enqueue(new Molecule(thisMol, true));
                        }
                    }
                    break;
                }
                else
                {
                    line = reader.ReadLine();
                    if (line == "@<TRIPOS>ATOM") startReading = true;
                }
            }

            Matrix<double> centroids = DenseMatrix.Create(numMols, 3, 0);
            Matrix<double> directions = DenseMatrix.Create(numMols, 3, 0);
            int count = 0;
            foreach (Molecule mol in superCell.mols)
            {
                centroids.SetRow(count, mol.centroid);
                directions.SetRow(count, mol.direction);
            }

            return superCell;
        }
        public static SuperCell ReadMol2_complex(string fileName)
        {
            SuperCell superCell = new SuperCell();









            return superCell;
        }
    }
}
