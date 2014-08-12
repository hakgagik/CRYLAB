using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using OpenTK;
using OpenTK.Graphics;

namespace CRYLAB
{
    public class FieldLine : List<Vector3d>
    {
        public List<double> Strength;
        public List<Color4> Colors;

        public FieldLine()
            : base()
        {
            Strength = new List<double>();
            Colors = new List<Color4>();
        }

        public void Add(Vector3d vector, double strength)
        {
            base.Add(vector);
            Strength.Add(strength);
        }

        public void AddRange(FieldLine fieldLine)
        {
            base.AddRange(fieldLine);
            Strength.AddRange(fieldLine.Strength);
        }

        new public void Reverse()
        {
            base.Reverse();
            Strength.Reverse();
        }


    }

    public struct Extensions
    {
        public static string Images = "Portable Network Graphics|*.png|JPEG|*.jpg|GIF|*.gif|Bitmap|*.bmp";
        public static string Mol2 = "Mol2 Files|*.mol2";
    }

    public enum MousePositionFormat
    {
        Cartesian,
        Fractional,
        Screen
    }

    static class Calculator
    {
        public static List<SuperCell> BatchImport(string[] filenames, SuperCell parent, CRYLAB form)
        {
            List<SuperCell> superList = new List<SuperCell>();
            for (int i = 0; i < filenames.Length; i++)
            {
                string filename = filenames[i];
                SuperCell child = SuperCell.ReadMol2_simple(filename, parent);
                if (child.isError)
                {
                    HandleErrors(child);
                }
                else
                {
                    superList.Add(child);
                }
                form.progressBar.Value = (int)((double)(i + 1) / (double)filenames.Length * 100.0);
            }
            form.progressBar.Value = 100;
            return superList;
        }

        public static Vector<double> CrossProduct(Vector<double> a, Vector<double> b)
        {
            //I can't believe I have to implement this manually -_-
            return DenseVector.OfArray(new double[] { a[1]*b[2]-a[2]*b[1],
                                                      a[2]*b[0]-a[0]*b[2],
                                                      a[0]*b[1]-a[1]*b[0]});
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

        public static FieldLine GetFieldLine(Matrix<double> directions, Vector<double> seed, SuperCell superCell, double std)
        {
            FieldLine displayList = new FieldLine();
            FieldLine forwardList = new FieldLine();
            FieldLine backwardList = new FieldLine();

            int pointLimit = 1000;
            double multiplier = 1.0;

            Vector<double> currentPoint = seed;
            if (GetForce(directions, superCell, seed, std, 1.0) != null)
            {
                forwardList.Add(Num2TK(currentPoint), GetForce(directions, superCell, seed, std, 1.0).L2Norm());
            }
            else
            {
                forwardList.Add(Num2TK(currentPoint), 0);
            }
            Vector<double> previousForce;
            Vector<double> currentForce = DenseVector.Create(3, 0);
            Vector<double> previousPoint;
            Vector<double> displacement;
            double acceleration;

            for (int i = 0; i < pointLimit; i++)
            {
                previousPoint = currentPoint;
                previousForce = currentForce;
                currentForce = GetForce(directions, superCell, previousPoint, std, 1.0);

                if (currentForce == null) break;
                displacement = currentForce * multiplier;
                currentPoint = previousPoint + displacement;
                forwardList.Add(Num2TK(currentPoint), currentForce.L2Norm());
                acceleration = (currentForce - previousForce).L2Norm() / displacement.L2Norm();
                multiplier = UpdateMultiplier(acceleration);
                if (multiplier > 50) break;
            }

            currentPoint = seed;
            currentForce = DenseVector.Create(3, 0);
            for (int i = 0; i < pointLimit; i++)
            {
                previousPoint = currentPoint;
                previousForce = currentForce;
                currentForce = GetForce(directions, superCell, previousPoint, std, -1.0);
                if (currentForce == null) break;
                displacement = currentForce * multiplier;
                currentPoint = previousPoint + displacement;
                backwardList.Add(Num2TK(currentPoint),currentForce.L2Norm());
                acceleration = (currentForce - previousForce).L2Norm() / displacement.L2Norm();
                multiplier = UpdateMultiplier(acceleration);
                if (multiplier > 50) break;
            }

            backwardList.Reverse();
            displayList.AddRange(backwardList);
            displayList.AddRange(forwardList);




            return displayList;
        }

        private static double UpdateMultiplier(double acceleration)
        {
            return Math.Pow(acceleration, -0.3);
        }

        private static Vector<double> GetForce(Matrix<double> directions, SuperCell superCell, Vector<double> seed, double std, double direction)
        {
            List<int> nearbyPoints = NearestNeighbors(seed, superCell);
            if (nearbyPoints.Count == 0)
            {
                return null;
            }
            else
            {
                return direction * Interpolate(nearbyPoints, directions, superCell, seed, std);
            }

        }

        public static List<int> NearestNeighbors(Vector<double> point, SuperCell superCell)
        {
            List<int> nearbyPoints = new List<int>();
            Vector<double> coordinates = superCell.latticeVectors.Solve(point - superCell.centroids.Row(0));
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        int[] indicies = new int[] { (int)Math.Floor(coordinates[0] + i), (int)Math.Floor(coordinates[1] + j), (int)Math.Floor(coordinates[2] + k) };
                        if (indicies[0] >= 0 && indicies[0] < superCell.superCellSize[0] && indicies[1] >= 0 && indicies[1] < superCell.superCellSize[1] && indicies[2] >= 0 && indicies[2] < superCell.superCellSize[2])
                        {
                            nearbyPoints.Add(Sub2Ind(indicies, superCell.superCellSize));
                        }
                    }
                }
            }
            return nearbyPoints;
        }

        public static double Interpolate(List<int> nearbyPoints, Vector<double> values, SuperCell superCell, Vector<double> point, double std)
        {
            double result = 0;
            double weight;
            double weights = 0;

            for (int i = 0; i < nearbyPoints.Count; i++)
            {
                Vector<double> position = superCell.centroids.Row(nearbyPoints[i]);
                if (!superCell.isParent)
                {
                    Vector<double> disposition /*lol*/ = position - superCell.parent.centroids.Row(nearbyPoints[i]);
                    for (int dim = 0; dim < 3; dim++)
                    {
                        disposition[dim] %= 1;
                        if (disposition[dim] >= 0.5) disposition[dim]--;
                        else if (disposition[dim] <= -0.05) disposition[dim]++;
                    }
                    position = disposition + superCell.parent.centroids.Row(nearbyPoints[i]);
                }

                Vector<double> displacement = position - point;

                weight = Math.Exp(-displacement.DotProduct(displacement) / (2.0 * std));
                weights += weight;
                result += values[nearbyPoints[i]] * weight;
            }
            return result / weights;
        }

        public static Vector<double> Interpolate(List<int> nearbyPoints, Matrix<double> vectors, SuperCell superCell, Vector<double> point, double std)
        {
            Vector<double> force = DenseVector.Create(3, 0);
            double weight;
            double weights = 0;
            for (int i = 0; i < nearbyPoints.Count; i++)
            {
                Vector<double> position = superCell.centroids.Row(nearbyPoints[i]);
                if (!superCell.isParent)
                {
                    Vector<double> disposition /*lol*/ = position - superCell.parent.centroids.Row(nearbyPoints[i]);
                    for (int dim = 0; dim < 3; dim++)
                    {
                        disposition[dim] %= 1;
                        if (disposition[dim] >= 0.5) disposition[dim]--;
                        else if (disposition[dim] <= -0.05) disposition[dim]++;
                    }
                    position = disposition + superCell.parent.centroids.Row(nearbyPoints[i]);
                }

                Vector<double> displacement = position - point;

                weight = Math.Exp(-displacement.DotProduct(displacement) / (2.0 * std));
                weights += weight;
                force += vectors.Row(nearbyPoints[i]) * weight;
            }
            return force / weights;
        }
        
        public static int Sub2Ind(int[] subscripts, int[] dimensions)
        {
            return subscripts[0] + dimensions[0] * subscripts[1] + dimensions[0] * dimensions[1] * subscripts[2];
        }

        //public static int[] Ind2Sub(int index, int[] dimensions)
        //{
        //    int product;

        //    int[] subscripts = new int[dimensions.Length];
        //    for (int i = 0; i < dimensions.Length; i++)
        //    {
        //        product = 1;
        //        for (int j = i + 1; j < dimensions.Length; j++)
        //        {
        //            product *= dimensions[j];
        //        }
        //        subscripts[i] = index / product;
        //        index = index % product;
        //    }

        //    return subscripts;
        //} 

        public static Vector<double> TK2Num(Vector3d vector)
        {
            return DenseVector.OfArray(new double[] { vector[0], vector[1], vector[2] });
        }

        public static Vector3d Num2TK(Vector<double> vector)
        {
            return new Vector3d(vector[0], vector[1], vector[2]);
        }
    }
}
