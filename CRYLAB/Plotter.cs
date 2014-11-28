//#define _debug

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Threading;

namespace CRYLAB
{
    public enum PlotStyle
    {
        Molecules,
        Centers,
        Directions,
        Curl
    }

    public enum FieldLineStyle
    {
        None,
        Single,
        Full
    }

    public enum Interactivity
    {
        None,
        Dislocation,
        FrankRead
    }

    public struct MiscInfo
    {
        public Color4[] colorOrder;
        public List<Color4> colorList;
        public double minLength;
        public double maxLength;
        public Vector3d planeNormal;
        public Vector3d pointOnPlane;
    }

    public class Plotter
    {
        private Thread runGameThread;
        private Thread calcFieldLinesThread;
        private CRYLAB mainForm;

        private bool firstRun = true;
        public bool isRunning = false;
        private bool isPaused = false;
        private bool pauseFieldLines = false;
        private bool exitGame = false;

        private PlotStyle plotStyle;
        private FieldLineStyle fieldLineStyle;
        private double fieldLineDensity = 1.0;
        private SuperCell currentCell;
        private List<Vector3d> displayList;
        private List<FieldLine> fieldLineList;

        private Vector3d eye;
        private Vector3d target;
        private Vector3d up;
        private Vector3d right;
        public double theta = 0;
        public double phi = - Math.PI/2.0;
        public double r;

        private double[] extents;
        private int viewWidth;
        private int viewHeight;
        private int viewX0;
        private int viewY0;
        private double gameWidth;
        private double gameHeight;
        private double gameDepth;
        private double depthClipPlane;

        private bool rotationLocked = false;
        private bool mouseDown = false;
        private bool ctrlDown = false;
        int mouseX;
        int mouseY;
        bool mouseChanged;
        private MouseButton mouseButton;

        public Color4[] colors;
        public Color4[] molColors;
        public Color4 bgColor;
        public MiscInfo miscInfo;

        private bool grabScreenshot;
        Bitmap screenshot;

        private List<Vector3d> pointBuffer;
        private Interactivity interactivity = Interactivity.None;

        public Plotter(CRYLAB form)
        {
            mainForm = form;
        }
        
        public void PlotInit()
        {
            using (var game = new GameWindow())
            {
                if (firstRun)
                {
                    game.Load += game_Load;
                    game.Resize += game_Resize;
                    game.UpdateFrame += game_UpdateFrame;
                    game.RenderFrame += game_RenderFrame;
                    game.MouseDown += game_MouseDown;
                    game.MouseUp += game_MouseUp;
                    game.Mouse.Move += Mouse_Move;
                    game.Mouse.WheelChanged += Mouse_WheelChanged;
                    game.Closing += game_Closing;


                    firstRun = false;
                }
                game.Run(60);
            }
        }

        void game_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isRunning = false;
            isPaused = false;
            firstRun = true;
            mouseDown = false;
            pauseFieldLines = false;
        }

        void Mouse_WheelChanged(object sender, MouseWheelEventArgs e)
        {
            gameWidth /= Math.Exp(e.Delta / 10.0);
            gameHeight /= Math.Exp(e.Delta / 10.0);
            gameWidth = Calculator.Clip((extents[1] - extents[0]) / 20.0, (extents[1] - extents[0]) * 20.0, gameWidth);
            gameHeight = Calculator.Clip((extents[3] - extents[2]) / 20.0, (extents[3] - extents[2]) * 20.0, gameHeight);
        }
        
        void Mouse_Move(object sender, MouseMoveEventArgs e)
        {
            if (mouseDown)
            {
                switch (mouseButton)
                {
                    case MouseButton.Left:
                        if (!rotationLocked)
                        {
                            if (ctrlDown)
                            {
                                gameWidth /= Math.Exp(e.YDelta / 20.0);
                                gameHeight /= Math.Exp(e.YDelta / 20.0);
                                gameWidth = Calculator.Clip((extents[1] - extents[0]) / 20.0, (extents[1] - extents[0]) * 20.0, gameWidth);
                                gameHeight = Calculator.Clip((extents[3] - extents[2]) / 20.0, (extents[3] - extents[2]) * 20.0, gameHeight);
                            }
                            else
                            {
                                theta -= (double)e.YDelta * Math.PI / (200.0);
                                theta = Calculator.Clip(0, Math.PI, theta);
                                phi -= (double)e.XDelta * Math.PI / 200.0;
                                UpdateView();
                            }
                        }
                        break;
                    case MouseButton.Right:
                        target -= e.XDelta * right * gameWidth / (double)viewWidth - e.YDelta * up * gameHeight / (double)viewHeight;
                        eye -= e.XDelta * right * gameWidth / (double)viewWidth - e.YDelta * up * gameHeight / (double)viewHeight;
                        break;
                }
            }
            if (mouseX != e.X || mouseY != e.Y)
            {
                mouseChanged = true;
                mouseX = e.X;
                mouseY = e.Y;
            }
        }
        
        void game_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mouseDown = false;
        }
        
        void game_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseDown = true;
            mouseButton = e.Button;
            if (interactivity !=Interactivity.None && e.Button == MouseButton.Left)
            {
                pointBuffer.Add(Calculator.Num2TK(ProjectToPlane(e.X, e.Y)));
            }
            else if (e.Button == MouseButton.Right)
            {
                CancelInteractivity();
            }
        }
        
        void game_RenderFrame(object sender, FrameEventArgs e)
        {
            if (!isPaused)
            {
                GL.ClearColor(bgColor);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();

                PlotPicker();
                GL.Color4(colors[0]);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(extents[0], 0, 0);
                GL.Vertex3(extents[1], 0, 0);
                GL.Vertex3(0, extents[2], 0);
                GL.Vertex3(0, extents[3], 0);
                GL.Vertex3(0, 0, extents[4]);
                GL.Vertex3(0, 0, extents[5]);
                GL.End();

                ((GameWindow)sender).SwapBuffers();

            }
        }
        
        void game_UpdateFrame(object sender, FrameEventArgs e)
        {
            GameWindow game = (GameWindow)sender;
            if (!isPaused)
            {
                if (game.Keyboard[Key.Escape])
                {
                    game.Exit();
                }

                if (game.Keyboard[Key.ControlLeft] || game.Keyboard[Key.ControlRight]) ctrlDown = true;
                else ctrlDown = false;

                if (grabScreenshot)
                {
                    Pause();
                    if (GraphicsContext.CurrentContext == null) throw new GraphicsContextMissingException();
                    screenshot = new Bitmap(game.ClientSize.Width, game.ClientSize.Height);
                    System.Drawing.Imaging.BitmapData data = screenshot.LockBits(game.ClientRectangle, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    GL.ReadPixels(0, 0, game.ClientSize.Width, game.ClientSize.Height, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
                    screenshot.UnlockBits(data);

                    screenshot.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    grabScreenshot = false;
                    UnPause();
                }

                if (fieldLineStyle == FieldLineStyle.Single && mouseChanged)
                {
                    mouseChanged = false;
                    calcFieldLinesThread = new Thread(GrabFieldLinesFromMouse);
                    calcFieldLinesThread.Start();
                }
            }
            Vector<double> mousePosition = DenseVector.Create(1, 0);
            int numel = 0;

            switch (mainForm.mousePositionFormat)
            {
                case MousePositionFormat.Cartesian:
                    mousePosition = ProjectToPlane(mouseX, mouseY);
                    numel = 3;
                    break;
                case MousePositionFormat.Fractional:
                    mousePosition = currentCell.latticeVectors.Solve(ProjectToPlane(mouseX, mouseY));
                    numel = 2;
                    break;
                case MousePositionFormat.Screen:
                    mousePosition = DenseVector.OfArray(new double[] { mouseX, mouseY });
                    numel = 2;
                    break;
            }

            string labelText = "Mouse Position: (";
            for (int dim = 0; dim < numel; dim++)
            {
                if (dim != 0) labelText += ", ";
                labelText += mousePosition[dim];
            }
            labelText += ")";
            mainForm.mousePositionLabel.Text = labelText;
            
            if (exitGame)
            {
                game.Exit();
            }
        }
        
        void game_Resize(object sender, EventArgs e)
        {
            GameWindow game = (GameWindow)sender;
            if (game.Width * (gameHeight / gameWidth) < game.Height)
            {
                viewX0 = (game.Width - (int)(game.Height * gameWidth / gameHeight)) / 2;
                viewY0 = 0;
                viewWidth = (int)(game.Height * gameWidth / gameHeight);
                viewHeight = game.Height;
                GL.Viewport(viewX0, 0, viewWidth, game.Height);
            }
            else
            {
                viewX0 = 0;
                viewY0 = (int)(game.Height - (int)(game.Width * (gameHeight / gameWidth))) / 2;
                viewWidth = game.Width;
                viewHeight = (int)(game.Width * (gameHeight / gameWidth));
                GL.Viewport(0, viewY0, game.Width, viewHeight);
            }
        }
        
        void game_Load(object sender, EventArgs e)
        {
            GameWindow game = (GameWindow)sender;
            game.VSync = VSyncMode.On;
            GL.ClearColor(bgColor);
            GL.ShadeModel(ShadingModel.Smooth);
        }
        
        private void PlotPicker()
        {
            switch (plotStyle)
            {
                case PlotStyle.Molecules:
                    RenderMolecules();
                    break;
                case PlotStyle.Centers:
                    RenderCentroids();
                    break;
                case PlotStyle.Directions:
                    RenderVectorField();
                    break;
                case PlotStyle.Curl:
                    RenderVectorField();
                    break;
            }
            switch (fieldLineStyle)
            {
                case FieldLineStyle.Single:
                    RenderSingleFieldLines();
                    break;
                case FieldLineStyle.Full:
                    RenderFullFieldLines();
                    break;
            }
            switch (interactivity)
            {
                case Interactivity.Dislocation:
                    RenderDislocation();
                    break;
                case Interactivity.FrankRead:
                    RenderFrankRead();
                    break;
            }
        }
        
        public void Plot(SuperCell superCell, PlotStyle plotStyle)
        {
            Pause();
            SetPlotInfo(superCell, plotStyle);
            UnPause();
            if (!isRunning)
            {
                runGameThread = new Thread(PlotInit);
                runGameThread.Start();
                isRunning = true;
            }
        }
        
        public void PlotWithFieldLines(SuperCell superCell, PlotStyle plotStyle, FieldLineStyle fieldLines, double density)
        {
            Pause();
            SetPlotInfo(superCell, plotStyle);
            this.fieldLineStyle = fieldLines;
            fieldLineList = new List<FieldLine>();
            fieldLineList.Add(new FieldLine());
            if (fieldLines == FieldLineStyle.Full) GrabAllFieldLines();
            UnPause();
            if (!isRunning)
            {
                runGameThread = new Thread(PlotInit);
                runGameThread.Start();
                isRunning = true;
            }
        }

        public void DrawDislocation()
        {
            if (isPaused) return;
            interactivity = Interactivity.Dislocation;
            rotationLocked = true;
            pointBuffer = new List<Vector3d>();
        }

        private void SetPlotInfo(SuperCell superCell, PlotStyle thisPlotStyle)
        {
            currentCell = superCell;
            plotStyle = thisPlotStyle;
            fieldLineStyle = FieldLineStyle.None;
            miscInfo = new MiscInfo();
            displayList = new List<Vector3d>();
            extents = new double[] { double.MaxValue, double.MinValue, double.MaxValue, double.MinValue, double.MaxValue, double.MinValue };
            up = new Vector3d(0.0, 1.0, 0.0);
            right = new Vector3d(1.0, 0.0, 0.0);
            miscInfo.planeNormal = Calculator.Num2TK(Calculator.CrossProduct(superCell.latticeVectors.Column(0), superCell.latticeVectors.Column(1)));
            miscInfo.pointOnPlane = Calculator.Num2TK(superCell.centroids.Row(0));
            rotationLocked = false;
            Vector3d oldTarget = target;
            if (!isRunning)
            {
                target = new Vector3d(0.0, 0.0, 0.0);
                theta = 0.0;
                phi = -Math.PI / 2;
            }
            switch (plotStyle)
            {
                case PlotStyle.Molecules:
                    string[] distinctTypes = superCell.types.Distinct().ToArray();
                    miscInfo.colorOrder = new Color4[superCell.types.Length];
                    miscInfo.colorList = new List<Color4>();
                    for (int i = 0; i < superCell.types.Length; i++)
                    {
                        int index = 0;
                        while (true)
                        {
                            if (superCell.types[i] == distinctTypes[index]) break;
                            index++;
                        }
                        miscInfo.colorOrder[i] = molColors[index];
                    }
                    for (int i = 0; i < superCell.mols.Count; i++)
                    {
                        Vector3d myCentroid = Calculator.Num2TK(superCell.mols[i].centroid);
                        for (int j = 0; j < superCell.bonds.GetLength(0); j++)
                        {
                            miscInfo.colorList.Add(miscInfo.colorOrder[superCell.bonds[j, 0]-1]);
                            Vector3d atom = Calculator.Num2TK(superCell.mols[i].atoms.Row(superCell.bonds[j, 0] - 1)) + myCentroid;
                            extents = Calculator.UpdateExtents(extents, atom);
                            target += atom;
                            displayList.Add(atom);

                            miscInfo.colorList.Add(miscInfo.colorOrder[superCell.bonds[j, 1]-1]);
                            atom = Calculator.Num2TK(superCell.mols[i].atoms.Row(superCell.bonds[j, 1] - 1)) + myCentroid;
                            extents = Calculator.UpdateExtents(extents, atom);
                            target += atom;
                            displayList.Add(atom);
                        }
                    }
                    break;
                case PlotStyle.Centers:
                    for (int i = 0; i < superCell.centroids.RowCount; i++)
                    {
                        Vector3d centroid = Calculator.Num2TK(superCell.centroids.Row(i));
                        extents = Calculator.UpdateExtents(extents, centroid);
                        target += centroid;
                        displayList.Add(centroid);
                    }
                    break;
                case PlotStyle.Directions:
                    GrabDirections(superCell,false);
                    break;
                case PlotStyle.Curl:
                    GrabDirections(superCell,true);
                    break;
            }
            target /= (double)displayList.Count;
            if (!isRunning)
            {
                gameWidth = (extents[1] - extents[0]) * 1.1;
                gameHeight = (extents[3] - extents[2]) * 1.1;
                gameDepth = extents[5] - extents[4];
                depthClipPlane = Math.Max(Math.Max(gameWidth, gameDepth), gameDepth);
                eye = new Vector3d(target.X, target.Y, depthClipPlane * 2.0);
            }
            else
            {
                target = oldTarget;
                UpdateView();
            }
            r = (eye - target).Length;
        }

        public void Pause()
        {
            isPaused = true;
        }

        public void UnPause()
        {
            isPaused = false;
        }

        public void Exit()
        {
            exitGame = true;
        }

        public Bitmap Screenshot()
        {
            if (isRunning)
            {
                grabScreenshot = true;
                while (grabScreenshot) ;
                return screenshot;
            }
            else return null;
        }
        
        public void UpdateView()
        {
            Vector3d displacement = r * (new Vector3d(Math.Sin(theta) * Math.Cos(phi), Math.Sin(theta) * Math.Sin(phi), Math.Cos(theta)));
            eye = target + displacement;
            right = new Vector3d(-Math.Sin(phi), Math.Cos(phi), 0.0);
            up = -Vector3d.Normalize(Vector3d.Cross(right, displacement));
        }

        public void GrabDirections(SuperCell superCell, bool curls)
        {
            double multiplier;
            if (superCell.isParent) multiplier = 1;
            else multiplier = 10;
            if (plotStyle == PlotStyle.Directions) multiplier *= 10;
            for (int i = 0; i < superCell.centroids.RowCount; i++)
            {
                Vector3d centroid = Calculator.Num2TK(superCell.centroids.Row(i));
                extents = Calculator.UpdateExtents(extents, centroid);
                Vector3d direction;
                if (curls)
                {
                    if (superCell.isParent)
                    {
                        direction = Calculator.Num2TK(superCell.curls.Row(i));
                    }
                    else
                    {
                        direction = Calculator.Num2TK(superCell.curls.Row(i) - superCell.parent.curls.Row(i));
                    }
                }
                else
                {
                    if (superCell.isParent)
                    {
                        direction = Calculator.Num2TK(superCell.directions.Row(i));
                    }
                    else
                    {
                        direction = Calculator.Num2TK(superCell.directions.Row(i) - superCell.parent.directions.Row(i));
                    }
                }
                
                target += 2 * centroid;
                displayList.Add(centroid - 0.5 * direction * multiplier);
                displayList.Add(centroid + 0.5 * direction * multiplier);
            }
        }

        public Vector<double> ProjectToPlane(int x, int y)
        {
            double X = (double)x - (double)viewWidth / 2.0;
            double Y = (double)viewHeight / 2.0 - (double)y;

            X += viewX0;
            Y += viewY0;

            X *= gameWidth / (double)viewWidth;
            Y *= gameHeight / (double)viewHeight;

            Vector3d screenPoint = eye + right * X + up * Y;
            Vector3d lineDirection = eye-target;
            Vector3d POI = lineDirection * (Vector3d.Dot(miscInfo.pointOnPlane - screenPoint, miscInfo.planeNormal) / Vector3d.Dot(lineDirection, miscInfo.planeNormal)) + screenPoint;

            return Calculator.TK2Num(POI);
        }

        public void GrabFieldLinesFromMouse()
        {
            Vector<double> seed = ProjectToPlane(mouseX, mouseY);
            pauseFieldLines = true;
            if (currentCell.isParent)
            {
                fieldLineList[0] = Calculator.GetFieldLine(currentCell.directions, seed, currentCell, 5.0);
            }
            else
            {
                fieldLineList[0] = Calculator.GetFieldLine(currentCell.directions - currentCell.parent.directions, seed, currentCell, 5.0);
            }
            pauseFieldLines = false;
        }

        public void GrabAllFieldLines()
        {
            double max = double.MinValue;
            double min = double.MaxValue;
            FieldLine currentFieldLine;
            List<Matrix<double>> strains = currentCell.FirstOrderStrain();
            Vector<double> Ezz = DenseVector.Create(strains.Count, 0);
            for (int i = 0; i < strains.Count; i++)
            {
                Ezz[i] = strains[i][2, 2];
            }
            for (int i = 0; i < currentCell.centroids.RowCount; i++)
            {
                pauseFieldLines = true;
                if (currentCell.isParent)
                {
                    currentFieldLine = Calculator.GetFieldLine(currentCell.directions, currentCell.centroids.Row(i), currentCell, 5.0);

                }
                else
                {
                    currentFieldLine = Calculator.GetFieldLine(currentCell.directions - currentCell.parent.directions, currentCell.centroids.Row(i), currentCell, 5.0);
                }
                //for (int j = 0; j < currentFieldLine.Count; j++)
                //{
                //    currentFieldLine.Strength[j] = Calculator.Interpolate(Calculator.NearestNeighbors(Calculator.TK2Num(currentFieldLine[j]), currentCell), Ezz, currentCell, Calculator.TK2Num(currentFieldLine[j]), 5.0);
                //}

                fieldLineList.Add(currentFieldLine);
                if (currentFieldLine.Strength.Min() != 0)
                {
                    max = Math.Max(currentFieldLine.Strength.Max(), max);
                    min = Math.Min(currentFieldLine.Strength.Min(), min);
                }
                mainForm.progressBar.Value = (int)(100.0 * (i+1) / currentCell.centroids.RowCount);
            }
            double logMin = Math.Log(min);
            double range = Math.Log(max) - logMin;
            if (range > 0.0001)
            {
                foreach (FieldLine fieldLine in fieldLineList)
                {
                    for (int i = 0; i < fieldLine.Count; i++)
                    {
                        if (fieldLine.Strength[i] == 0) fieldLine.Colors.Add(colors[1]);
                        else
                        {
                            float ratio = (float)((Math.Log(fieldLine.Strength[i]) - logMin) / range);
                            fieldLine.Colors.Add(new Color4(colors[1].R * (1 - ratio) + colors[0].R * ratio, colors[1].G * (1 - ratio) + colors[0].G * ratio, colors[1].B * (1 - ratio) + colors[0].B * ratio, colors[1].A * (1 - ratio) + colors[0].A * ratio));
                        }
                    }
                }
            }
            else
            {
                Color4 midColor = new Color4(0.5f * colors[0].R + 0.5f * colors[1].R, 0.5f * colors[0].G + 0.5f * colors[1].G, 0.5f * colors[0].B + 0.5f * colors[1].B, 0.5f * colors[0].A + 0.5f * colors[1].A);
                foreach (FieldLine fieldLine in fieldLineList)
                {
                    for (int i = 0; i < fieldLine.Count; i++)
                    {
                        fieldLine.Colors.Add(midColor);
                    }
                }
            }
            mainForm.progressBar.Value = 100;
            pauseFieldLines = false;
        }
        
        private void RenderMolecules()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.Ortho(-(gameWidth) / 2.0, (gameWidth) / 2.0, -(gameHeight) / 2.0, (gameHeight) / 2.0, 0, depthClipPlane * 5);
            GL.MatrixMode(MatrixMode.Modelview);
            Matrix4d lookAt = Matrix4d.LookAt(eye, target, up);
            GL.LoadMatrix(ref lookAt);

            GL.Begin(PrimitiveType.Lines);
            int count = 0;
            foreach (Vector3d centroid in displayList)
            {
                if (isPaused) break;
                GL.Color4(miscInfo.colorList[count]);
                GL.Vertex3(centroid);
                count++;
            }
            GL.End();
        }
        
        private void RenderCentroids()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.Ortho(-(gameWidth) / 2.0, (gameWidth) / 2.0, -(gameHeight) / 2.0, (gameHeight) / 2.0, 0, depthClipPlane * 5);
            GL.MatrixMode(MatrixMode.Modelview);
            Matrix4d lookAt = Matrix4d.LookAt(eye, target, up);
            GL.LoadMatrix(ref lookAt);

            GL.Begin(PrimitiveType.Points);
            GL.Color4(colors[0]);
            foreach (Vector3d centroid in displayList)
            {
                if (isPaused) break;
                GL.Vertex3(centroid);
            }
            GL.End();
        }
        
        private void RenderVectorField()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.Ortho(-(gameWidth) / 2.0, (gameWidth) / 2.0, -(gameHeight) / 2.0, (gameHeight) / 2.0, 0, depthClipPlane * 5);
            GL.MatrixMode(MatrixMode.Modelview);
            Matrix4d lookAt = Matrix4d.LookAt(eye, target, up);
            GL.LoadMatrix(ref lookAt);

            GL.Begin(PrimitiveType.Lines);
            for (int i = 0; i < displayList.Count; i += 2)
            {
                GL.Color4(colors[1]);
                if (isPaused) break;
                GL.Vertex3(displayList[i]);
                GL.Color4(colors[0]);
                if (isPaused)
                {
                    GL.Vertex3(0, 0, 0);
                    break;
                }
                GL.Vertex3(displayList[i + 1]);
            }
            GL.End();            
        }

        private void RenderSingleFieldLines()
        {
            if (isPaused) return;
            if (pauseFieldLines) return;
            if (fieldLineList.Count == 0) return;
            GL.Begin(PrimitiveType.LineStrip);
            foreach (Vector3d point in fieldLineList[0])
            {
                GL.Vertex3(point);
                if (isPaused) break;
                if (pauseFieldLines) break;
            }
            GL.End();
        }
        
        private void RenderFullFieldLines()
        {
            if (isPaused) return;
            if (pauseFieldLines) return;
            if (fieldLineList.Count == 0) return;
            bool breakout = false;

            foreach (FieldLine line in fieldLineList)
            {
                if (pauseFieldLines) break;
                GL.Begin(PrimitiveType.LineStrip);
                for (int i = 0; i < line.Count(); i++ )
                {
                    if (pauseFieldLines)
                    {
                        breakout = true;
                        break;
                    }
                    GL.Color4(line.Colors[i]);
                    if (pauseFieldLines)
                    {
                        breakout = true;
                        break;
                    }
                    GL.Vertex3(line[i]);
                }
                GL.End();
                if (pauseFieldLines || breakout) break;
            }
        }

        private void RenderDislocation()
        {
            GL.Begin(PrimitiveType.Lines);
            if (pointBuffer.Count > 0)
            {
                GL.Vertex3(pointBuffer[0]);
            }
            if (pointBuffer.Count > 1)
            {
                GL.Vertex3(pointBuffer[1]);
            }
            else
            {
                GL.Vertex3(Calculator.Num2TK(ProjectToPlane(mouseX, mouseY)));
            }
            GL.End();
            if (pointBuffer.Count > 1)
            {
                double radius = (Calculator.Num2TK(ProjectToPlane(mouseX, mouseY)) - pointBuffer[1]).Length;
                radius = Math.Max(radius, 0.5);
                GL.Begin(PrimitiveType.LineLoop);
                

                GL.End();
            }
        }

        private void RenderFrankRead()
        {

        }

        private void CancelInteractivity()
        {
        }
    }
}

