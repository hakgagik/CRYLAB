//#define _DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.IO;
using System.Drawing;
using OpenTK;


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

        public void Translate(Vector<double> displacement)
        {
            centroid += displacement;
            atoms.SetColumn(0, atoms.Column(0) + displacement[0]);
            atoms.SetColumn(1, atoms.Column(1) + displacement[1]);
            atoms.SetColumn(2, atoms.Column(2) + displacement[2]);
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
    public enum SuperCellError
    {
        NoError,
        Child_StructureMismatch,
        Base_NoBonds,
        Base_NoLattice,
        Base_CouldntFindOrder,
        Base_SingleMol,
        Base_LineCell
    }
    public class SuperCell
    {
        public List<Molecule> mols;
        public Matrix<double> centroids;
        public Matrix<double> directions;
        public Matrix<double> curls;
        public List<Matrix<double>> totalDirectionDerivatives;

        public string filePath;
        public bool isError;
        public SuperCellError errorType;

        public bool isParent;
        public SuperCell parent = null;
        public int[] superCellSize;
        public Matrix<double> latticeVectors;
        public double[,] latticeParams;

        public int[,] order;

        public string[] types;
        public int[,] bonds;
        public int[] bondTypes;

        public SuperCell() { }

        public SuperCell(SuperCell parent)
        {
            isParent = false;
            superCellSize = parent.superCellSize;
            latticeVectors = parent.latticeVectors;
            latticeParams = parent.latticeParams;
            order = parent.order;
            types = parent.types;
            bonds = parent.bonds;
            bondTypes = parent.bondTypes;
            errorType = SuperCellError.NoError;
        }

        public static SuperCell ReadMol2_simple(String filePath, SuperCell parent){

            StreamReader reader = File.OpenText(filePath);
            string line;
            bool startReading = false;

            SuperCell superCell = new SuperCell(parent);
            superCell.isError = false;
            superCell.errorType = SuperCellError.NoError;
            superCell.filePath = filePath;
            superCell.parent = parent;

            int atomsPerCell = superCell.order.Length;
            int molsPerCell = superCell.order.GetLength(0);
            int atomsPerMol = superCell.order.GetLength(1);

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
                            if (line == "@<TRIPOS>BOND")
                            {
                                superCell.isError = true;
                                superCell.errorType = SuperCellError.Child_StructureMismatch;
                                return superCell;
                            }
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

            superCell.centroids = DenseMatrix.Create(numMols, 3, 0);
            superCell.directions = DenseMatrix.Create(numMols, 3, 0);
            int count = 0;
            foreach (Molecule mol in superCell.mols)
            {
                superCell.centroids.SetRow(count, mol.centroid);
                superCell.directions.SetRow(count, mol.direction);
                count++;
            }

            superCell.CalculateDirectionDerivatives();

            return superCell;
        }

        public static SuperCell ReadMol2_complex(string fileName, int atomsPerCell)
        {
            SuperCell superCell = new SuperCell();
            superCell.isError = false;
            superCell.isParent = true;
            superCell.errorType = SuperCellError.NoError;
            superCell.filePath = fileName;

            #region RawDataReader
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
                superCell.isError = true;
                superCell.errorType = SuperCellError.Base_NoBonds;
                return superCell;
            }

            string latticeLine;
            if (!latticePresent)
            {
                superCell.isError = true;
                superCell.errorType = SuperCellError.Base_NoLattice;
                return superCell;
            }
            else
            {
                latticeLine = reader.ReadLine();
            }
            #endregion

            #region OrderFinder
            Queue<int[]> bondsQueue = new Queue<int[]>();
            Queue<int> bondTypesQueue = new Queue<int>();
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
                    bondTypesQueue.Enqueue(int.Parse(items[3]));
                    numBonds++;
                }
            }

            Node headNode = new Node();
            Node thisAtom;
            superCell.bonds = new int[bondsQueue.Count, 2];
            superCell.bondTypes = new int[bondsQueue.Count];
            int count = 0;

            while (bondsQueue.Count > 0)
            {
                tempBond = bondsQueue.Dequeue();
                superCell.bonds[count, 0] = tempBond[0];
                superCell.bonds[count, 1] = tempBond[1];
                superCell.bondTypes[count] = bondTypesQueue.Dequeue();
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
                superCell.isError = true;
                superCell.errorType = SuperCellError.Base_CouldntFindOrder;
                return superCell;
            }
            #endregion

            #region RawDataProcessor
            int numAtoms = atomLines.Count;
            int molsPerCell = superCell.order.GetLength(0);
            int atomsPerMol = superCell.order.GetLength(1);
            int numCells = numAtoms / atomsPerCell;
            int numMols = molsPerCell*numCells;

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

            superCell.centroids = DenseMatrix.Create(numMols, 3, 0);
            superCell.directions = DenseMatrix.Create(numMols, 3, 0);
            count = 0;
            foreach (Molecule mol in superCell.mols)
            {
                superCell.centroids.SetRow(count, mol.centroid);
                superCell.directions.SetRow(count, mol.direction);
                count++;
            }

            #endregion

            #region LatticeFinder
            string[] latticeItems = latticeLine.Split(' ');
            double[,] incompleteLattice = new double[2, 3] { { double.Parse(latticeItems[0]), double.Parse(latticeItems[1]), double.Parse(latticeItems[2]) }, 
                                                             { double.Parse(latticeItems[3]), double.Parse(latticeItems[4]), double.Parse(latticeItems[5]) } };

            superCell.latticeParams = new double[2, 3] { { 0.0, 0.0, 0.0 }, { incompleteLattice[1, 0], incompleteLattice[1, 1], incompleteLattice[1, 2] } };
            superCell.superCellSize = new int[3] { 0, 0, 0 };
            superCell.latticeVectors = DenseMatrix.OfArray(new double[,] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } });
            bool[] cellDone = new bool[3] { false, false, false };

            double tempA;
            double tempB;
            double tempC;

            int cellA=0;
            int cellB=0;
            int cellC=0;

            if (numMols > 1)
            {
                for (int i = 0; i < 3; i++)
                {
                    tempA = (superCell.mols[molsPerCell].centroid - superCell.mols[0].centroid).L2Norm();
                    double tempLatticeRatio = incompleteLattice[0, i] / tempA;
                    if (Math.Abs(tempLatticeRatio - Math.Round(tempLatticeRatio)) < 0.001)
                    {
                        cellA = (int)Math.Round(tempLatticeRatio);
                        superCell.superCellSize[i] = cellA;
                        superCell.latticeParams[0, i] = tempA;
                        cellDone[i] = true;
                        superCell.latticeVectors.SetColumn(i, superCell.mols[molsPerCell].centroid - superCell.mols[0].centroid);
                        break;
                    }

                }
            }
            else
            {
                superCell.isError = true;
                superCell.errorType = SuperCellError.Base_SingleMol;
                return superCell;
            }


            if (numMols > cellA)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (cellDone[i] == true) continue;
                    tempB = (superCell.mols[cellA*molsPerCell].centroid - superCell.mols[0].centroid).L2Norm();
                    double tempLatticeRatio = incompleteLattice[0, i] / tempB;
                    if (Math.Abs(tempLatticeRatio - Math.Round(tempLatticeRatio)) < 0.001)
                    {
                        cellB = (int)Math.Round(tempLatticeRatio);
                        superCell.superCellSize[i] = cellB;
                        superCell.latticeParams[0, i] = tempB;
                        cellDone[i] = true;
                        superCell.latticeVectors.SetColumn(i, superCell.mols[cellA*molsPerCell].centroid - superCell.mols[0].centroid);
                        break;
                    }
                }
            }
            else
            {
                superCell.isError = true;
                superCell.errorType = SuperCellError.Base_LineCell;
                return superCell;
            }

            if (numMols > cellA * cellB)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (cellDone[i] == true) continue;
                    tempC = (superCell.mols[cellA * cellB * molsPerCell].centroid - superCell.mols[0].centroid).L2Norm();
                    double tempLatticeRatio = incompleteLattice[0, i] / tempC;
                    if (Math.Abs(tempLatticeRatio - Math.Round(tempLatticeRatio)) < 0.001)
                    {
                        cellC = (int)Math.Round(tempLatticeRatio);
                        superCell.superCellSize[i] = cellC;
                        superCell.latticeParams[0, i] = tempC;
                        cellDone[i] = true;
                        superCell.latticeVectors.SetColumn(i, superCell.mols[cellA*cellB*molsPerCell].centroid - superCell.mols[0].centroid);
                        break;
                    }
                }
            }
            else
            {
                int cIndex = 3;
                for (int i = 0; i < 3; i++)
                {
                    if (cellDone[i] == false) cIndex = i;
                }
                superCell.superCellSize[cIndex] = 1;
                superCell.latticeParams[0,cIndex] = incompleteLattice[0, cIndex];
                superCell.latticeVectors.SetColumn(cIndex, FindCVect(superCell.latticeVectors.Column((cIndex + 1) % 3), superCell.latticeVectors.Column((cIndex + 2) % 3), superCell.latticeParams[1, (cIndex + 1) % 3], superCell.latticeParams[1, (cIndex + 2) % 3], superCell.latticeParams[0, cIndex]));
            }
            #endregion

            superCell.CalculateDirectionDerivatives();

            return superCell;
        }

        public static Vector<double> FindCVect(Vector<double> aVect, Vector<double> bVect, double alpha, double beta, double C)
        {

            aVect /= aVect.L2Norm();
            bVect /= bVect.L2Norm();
            alpha *= Math.PI / 180.0;
            beta *= Math.PI / 180.0;

            Vector<double> planeNormal = Calculator.CrossProduct(aVect, bVect);
            planeNormal /= planeNormal.L2Norm();
            double theta = Math.Acos(planeNormal[2]);
            double phi = Math.Atan2(planeNormal[1], planeNormal[0]);

            Matrix<double> Rz = DenseMatrix.OfArray(new double[,] { { Math.Cos(phi), Math.Sin(phi), 0},
                                                                    {-Math.Sin(phi), Math.Cos(phi), 0},
                                                                    {     0,             0,         1} });
            Matrix<double> Ry = DenseMatrix.OfArray(new double[,] { { Math.Cos(theta), 0, -Math.Sin(theta)},
                                                                    {     0,           1,       0         },
                                                                     {Math.Sin(theta), 0,  Math.Cos(theta)} });

            aVect = Ry * Rz * aVect;
            bVect = Ry * Rz * bVect;
            

            double cx1 = (Math.Cos(beta) * bVect[1] - Math.Cos(alpha) * aVect[1]);
            double cx2 = (aVect[0] * bVect[1] - bVect[0] * aVect[1]);

            double cx = cx1 / cx2;
            double cy = (Math.Cos(beta) * bVect[0] - Math.Cos(alpha) * aVect[0]) / (aVect[1] * bVect[0] - bVect[1] * aVect[0]);

            Vector<double> cVect = DenseVector.OfArray(new double[] { cx, cy, Math.Sqrt(1 - cx * cx - cy * cy) });
            cVect = C * Rz.Inverse() * Ry.Inverse() * cVect;

            return cVect;
        }

        public void CalculateDirectionDerivatives()
        {
            curls = DenseMatrix.Create(centroids.RowCount, 3, 0.0);
            totalDirectionDerivatives = new List<Matrix<double>>();

            for (int point = 0; point < centroids.RowCount; point++)
            {
                Matrix<double> tempMatrix = DenseMatrix.Create(3, 3, 0);
                List<int> neighbors = Calculator.NearestNeighbors(centroids.Row(point), this);
                Matrix<double> divideBy = DenseMatrix.Create(3, 3, 0);
                foreach (int neighbor in neighbors)
                {
                    if (neighbor != point)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                if (Math.Abs(centroids.Row(neighbor)[j] - centroids.Row(point)[j]) > 0.001)
                                {
                                    tempMatrix[i, j] += (directions.Row(neighbor)[i] - directions.Row(point)[i]) / (centroids.Row(neighbor)[j] - centroids.Row(point)[j]);
                                    divideBy[i, j]++;
                                }
                            }
                        }
                    }
                }
                tempMatrix.PointwiseDivide(divideBy);
                curls.SetRow(point, DenseVector.OfArray(new double[] { tempMatrix[2, 1] - tempMatrix[1, 2], tempMatrix[0, 2] - tempMatrix[2, 0], tempMatrix[1, 0] - tempMatrix[0, 1] }));
                totalDirectionDerivatives.Add(tempMatrix);
            }
        }

        public List<Matrix<double>> FirstOrderStrain()
        {
            Matrix<double> parentCentroids;
            if (isParent) parentCentroids = centroids * latticeVectors.Inverse().Transpose();
            else parentCentroids = parent.centroids * latticeVectors.Inverse().Transpose();
            List<Matrix<double>> strainTensors = new List<Matrix<double>>();
            Matrix<double> displacements = centroids * latticeVectors.Inverse().Transpose() - parentCentroids;


            for (int point = 0; point < displacements.RowCount; point++)
            {
                strainTensors.Add(DenseMatrix.Create(3, 3, 0));
                List<int> neighbors = Calculator.NearestNeighbors(centroids.Row(point), this);
                Matrix<double> divideBy = DenseMatrix.Create(3, 3, 0);
                foreach (int neighbor in neighbors)
                {
                    if (neighbor != point)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                if (Math.Abs(parentCentroids.Row(neighbor)[j] - parentCentroids.Row(point)[j]) > 0.0001)
                                {
                                    strainTensors[point][i, j] = (displacements.Row(neighbor)[i] - displacements.Row(point)[i]) / (parentCentroids.Row(neighbor)[j] - parentCentroids.Row(point)[j]);
                                    divideBy[i, j]++;
                                }
                            }
                        }
                    }
                }
                strainTensors[point].PointwiseDivide(divideBy);
                strainTensors[point] += strainTensors[point].Transpose();
                strainTensors[point] /= 2.0;
            }
            return strainTensors;
        }

        public void Dislocate(Vector<double> location, Vector<double> sense, Vector<double> burgers, Vector<double> tearDirection, double radius)
        {
            if (Math.Abs(Calculator.CrossProduct(sense, tearDirection).L2Norm()) < 0.01) throw new NotImplementedException();
            Matrix<double> centralizer = DenseMatrix.Create(3, 3, 0);
            centralizer.SetColumn(2, sense);
            centralizer.SetColumn(0, (Calculator.CrossProduct(Calculator.CrossProduct(sense, tearDirection), sense)).Normalize(2));
            centralizer.SetColumn(1, Calculator.CrossProduct(centralizer.Column(2), centralizer.Column(0)));

            Matrix<double> rotatedCentroids = centroids * latticeVectors.Inverse().Transpose() * centralizer;
            location = centralizer.Transpose() * location;
            burgers = latticeVectors * burgers;
            Matrix<double> displacement = DenseMatrix.Create(centroids.RowCount, 3, 0);

            for (int i = 0; i < rotatedCentroids.RowCount; i++)
            {
                Vector<double> point = rotatedCentroids.Row(i) - location;
                double rho = Math.Sqrt(point[0] * point[0] + point[1] * point[1]);
                double theta = Math.Atan2(point[1], point[0]);
                double multiplier = 1;
                if (rho < radius) multiplier *= rho / radius;
                multiplier *= theta / (Math.PI * 2.0);
                centroids.SetRow(i, centroids.Row(i) + burgers * multiplier);
                mols[i].Translate(burgers * multiplier);
            }

        }
    }
}
