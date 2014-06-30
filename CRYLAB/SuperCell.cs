//#define _DEBUG

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

    public class Node
    {
        public Node Parent;
        public List<Node> Children;
        public int AtomNumber;

        public Node()
        {
            Children = new List<Node>();
        }

        public Node(int mol)
        {
            this.AtomNumber = mol;
            Parent = null;
            Children = new List<Node>();
        }

        public Node(int mol, Node parent)
        {
            AtomNumber = mol;
            parent.AddChild(this);
            Children = new List<Node>();
        }

        public void AddChild(Node child)
        {
            Children.Add(child);
            child.Parent = this;
        }

        public void ChangeParent(Node parent)
        {
            if (Parent.Parent != null)
            {
                Parent.ChangeParent(this);
            }
            Parent.Children.Remove(this);
            parent.AddChild(this);
        }
        public Node SearchChildren(int molNumber)
        {
            if (AtomNumber == molNumber)
            {
                return this;
            }
            else
            {
                Node result;
                foreach (Node node in Children)
                {
                    result = node.SearchChildren(molNumber);
                    if (result != null)
                    {
                        return result;
                    }
                }
                return null;
            }
        }

        public int[,] FindOrder()
        {
            List<int> tempList = Children[0].TraverseTree();
            int molsPerCell = Children.Count;
            int atomsPerMol = tempList.Count;
            int[,] order = new int[molsPerCell,atomsPerMol];

            int count = 0;
            foreach (Node child in Children)
            {
                int[] tempArray = child.TraverseTree().ToArray();
                Array.Sort(tempArray);
                for (int i = 0; i < atomsPerMol; i++)
                {
                    order[count, i] = tempArray[i]-1;
                }
                count++;
            }

            return order;
        }

        private List<int> TraverseTree()
        {
            List<int> atomList = new List<int>();
            atomList.Add(AtomNumber);
            foreach (Node node in Children)
            {
                atomList.AddRange(node.TraverseTree());
            }

            return atomList;
        }
    }


    public class SuperCell
    {
        public List<Molecule> mols;
        public Matrix<double> centroids;
        public Matrix<double> directions;


        public bool isParent;
        public int[] superCellSize;
        public Matrix<double> latticeVectors;
        public double[,] latticeParams;

        public int[,] order;

        public string[] types;
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

            superCell.mols = new List<Molecule>();

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
                            superCell.mols.Add(new Molecule(thisMol, true));
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
        public static SuperCell ReadMol2_complex(string fileName, int atomsPerCell)
        {
            SuperCell superCell = new SuperCell();

            //public List<Molecule> mols;
            //public Matrix<double> centroids;
            //public Matrix<double> directions;


            //public bool isParent;
            //public int[] superCellSize;
            //public Matrix<double> latticeVectors;
            //public double[,] latticeParams;

            //public int[,] order;

            //public string[] types;
            //public int[,] bonds;
            //public string[] bondTypes;

            Queue<string> atomLines = new Queue<string>();
            List<string> bondLines = new List<string>();
            StreamReader reader = File.OpenText(fileName);
            string line;
            bool startReading = false;
            bool bondsPresent = false;
            bool latticePresent = false;

            while (true)
            {
                if (startReading)
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line == "@<TRIPOS>BOND")
                        {
                            bondsPresent = true;
                            break;
                        }
                        else
                        {
                            atomLines.Enqueue(line);
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

            if (bondsPresent)
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line == "@<TRIPOS>CRYSIN")
                    {
                        latticePresent = true;
                        break;
                    }
                    else
                    {
                        bondLines.Add(line);
                    }
                }
            }
            else
            {
                return null;
            }

            if (!latticePresent)
            {
                return null;
            }

            Queue<int[]> bondsQueue = new Queue<int[]>();
            int[] tempBond;
            int numBonds = 0;

            while (true)
            {
                string[] items = bondLines[bondsQueue.Count].Split(' ');
                tempBond = new int[2] { int.Parse(items[1]), int.Parse(items[2]) };
                if (tempBond[0] > atomsPerCell || tempBond[1] > atomsPerCell)
                {
                    break;
                }
                else
                {
                    bondsQueue.Enqueue(tempBond);
                    numBonds++;
                }
            }

            Node headNode = new Node();
            Node thisAtom;
            superCell.bonds = new int[bondsQueue.Count, 2];
            int count = 0;

            while (bondsQueue.Count > 0)
            {
                tempBond = bondsQueue.Dequeue();
                superCell.bonds[count, 0] = tempBond[0];
                superCell.bonds[count, 1] = tempBond[1];
                thisAtom = headNode.SearchChildren(tempBond[0]);
                if (thisAtom != null)
                {
                    Node otherAtom = headNode.SearchChildren(tempBond[1]);
                    if (otherAtom != null)
                    {
                        otherAtom.ChangeParent(thisAtom);
                    }
                    else
                    {
                        thisAtom.AddChild(new Node(tempBond[1]));
                    }
                }
                else
                {
                    Node otherAtom = headNode.SearchChildren(tempBond[1]);
                    if (otherAtom != null)
                    {
                        otherAtom.AddChild(new Node(tempBond[0]));
                    }
                    else
                    {
                        Node tempNode = new Node(tempBond[0]);
                        headNode.AddChild(tempNode);
                        tempNode.AddChild(new Node(tempBond[1]));
                    }
                }
                count++;
            }

            if (headNode.Children.Count > 0)
            {
                superCell.order = headNode.FindOrder();
            }
            else
            {
                return null;
            }
            int numAtoms = atomLines.Count;
            int molsPerCell = superCell.order.GetLength(0);
            int atomsPerMol = superCell.order.GetLength(1);
            int numCells = numAtoms / atomsPerCell;

            superCell.types = new string[atomsPerCell];
            superCell.mols = new List<Molecule>();

            for (int cell = 0; cell < numCells; cell++)
            {
                double[,] atomsBuffer = new double[atomsPerCell, 3];
                for (int atom = 0; atom < atomsPerCell; atom++)
                {
                    string[] items = atomLines.Dequeue().Split(' ');
                    if (cell == 0)
                    {
                        if (items[5].Length > 2)
                        {
                            superCell.types[atom] = items[5].Remove(items[5].Length - 2);
                        }
                        else
                        {
                            superCell.types[atom] = items[5];
                        }
                    }

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
                    superCell.mols.Add(new Molecule(thisMol, true));
                }

            }

            return superCell;
        }
    }
}
