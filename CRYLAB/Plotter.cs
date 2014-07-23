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
                            theta -= (double)e.YDelta * Math.PI / 200.0;
                            theta = Calculator.Clip(0, Math.PI, theta);
                            phi -= (double)e.XDelta * Math.PI / 200.0;
                            UpdateView();
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
            Vector<double> seed = ProjectToPlane(e.X, e.Y);
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

                if (grabScreenshot)
                {
                    if (GraphicsContext.CurrentContext == null) throw new GraphicsContextMissingException();
                    screenshot = new Bitmap(game.ClientSize.Width, game.ClientSize.Height);
                    System.Drawing.Imaging.BitmapData data = screenshot.LockBits(game.ClientRectangle, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    GL.ReadPixels(0, 0, game.ClientSize.Width, game.ClientSize.Height, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
                    screenshot.UnlockBits(data);

                    screenshot.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    grabScreenshot = false;
                }

                if (fieldLineStyle == FieldLineStyle.Single && mouseChanged)
                {
                    mouseChanged = false;
                    calcFieldLinesThread = new Thread(GrabFieldLinesFromMouse);
                    calcFieldLinesThread.Start();
                }
            }

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

        private void SetPlotInfo(SuperCell superCell, PlotStyle thisPlotStyle)
        {
            currentCell = superCell;
            plotStyle = thisPlotStyle;
            fieldLineStyle = FieldLineStyle.None;
            miscInfo = new MiscInfo();
            displayList = new List<Vector3d>();
            extents = new double[] { double.MaxValue, double.MinValue, double.MaxValue, double.MinValue, double.MaxValue, double.MinValue };
            target = new Vector3d(0.0, 0.0, 0.0);
            up = new Vector3d(0.0, 1.0, 0.0);
            right = new Vector3d(1.0, 0.0, 0.0);
            Vector<double> tempPlaneNormal = Calculator.CrossProduct(superCell.latticeVectors.Column(0), superCell.latticeVectors.Column(1));
            miscInfo.planeNormal = new Vector3d(tempPlaneNormal[0], tempPlaneNormal[1], tempPlaneNormal[2]);
            miscInfo.pointOnPlane = new Vector3d(superCell.centroids.Row(0)[0], superCell.centroids.Row(0)[1], superCell.centroids.Row(0)[2]);
            rotationLocked = false;
            theta = 0.0;
            phi = -Math.PI / 2;
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
                        Vector3d myCentroid = new Vector3d(superCell.mols[i].centroid[0], superCell.mols[i].centroid[1], superCell.mols[i].centroid[2]);
                        for (int j = 0; j < superCell.bonds.GetLength(0); j++)
                        {
                            Vector<double> tempAtom = superCell.mols[i].atoms.Row(superCell.bonds[j,0]-1);
                            miscInfo.colorList.Add(miscInfo.colorOrder[superCell.bonds[j, 0]-1]);
                            Vector3d atom = (new Vector3d(tempAtom[0], tempAtom[1], tempAtom[2])) + myCentroid;
                            extents = Calculator.UpdateExtents(extents, atom);
                            target += atom;
                            displayList.Add(atom);

                            tempAtom = superCell.mols[i].atoms.Row(superCell.bonds[j, 1]-1);
                            miscInfo.colorList.Add(miscInfo.colorOrder[superCell.bonds[j, 1]-1]);
                            atom = (new Vector3d(tempAtom[0], tempAtom[1], tempAtom[2])) + myCentroid;
                            extents = Calculator.UpdateExtents(extents, atom);
                            target += atom;
                            displayList.Add(atom);
                        }
                    }
                    break;
                case PlotStyle.Centers:
                    for (int i = 0; i < superCell.centroids.RowCount; i++)
                    {
                        Vector<double> tempCentroid = superCell.centroids.Row(i);
                        Vector3d centroid = new Vector3d(tempCentroid[0], tempCentroid[1], tempCentroid[2]);
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
            gameWidth = (extents[1] - extents[0]) * 1.1;
            gameHeight = (extents[3] - extents[2]) * 1.1;
            gameDepth = extents[5] - extents[4];
            depthClipPlane = Math.Max(Math.Max(gameWidth, gameDepth), gameDepth);
            eye = new Vector3d(target.X, target.Y, depthClipPlane * 2.0);
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
            if (superCell.isParent) multiplier = 10;
            else multiplier = 100;
            for (int i = 0; i < superCell.centroids.RowCount; i++)
            {
                Vector<double> tempCentroid = superCell.centroids.Row(i);
                Vector3d centroid = new Vector3d(tempCentroid[0], tempCentroid[1], tempCentroid[2]);
                extents = Calculator.UpdateExtents(extents, centroid);
                Vector<double> tempDirection;
                if (curls)
                {
                    if (superCell.isParent)
                    {
                        tempDirection = superCell.curls.Row(i);
                    }
                    else
                    {
                        tempDirection = superCell.curls.Row(i) - superCell.parent.curls.Row(i);
                    }
                }
                else
                {
                    if (superCell.isParent)
                    {
                        tempDirection = superCell.directions.Row(i);
                    }
                    else
                    {
                        tempDirection = superCell.directions.Row(i) - superCell.parent.directions.Row(i);
                    }
                }
                Vector3d direction = new Vector3d(tempDirection[0], tempDirection[1], tempDirection[2]);
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

            return DenseVector.OfArray(new double[] { POI[0], POI[1], POI[2] });
        }

        public void GrabFieldLinesFromMouse()
        {
            Vector<double> seed = ProjectToPlane(mouseX, mouseY);
            pauseFieldLines = true;
            if (currentCell.isParent)
            {
                if (plotStyle == PlotStyle.Curl)
                {
                    fieldLineList[0] = Calculator.GetFieldLine(currentCell.curls, seed, currentCell, 5.0);
                }
                else
                {
                    fieldLineList[0] = Calculator.GetFieldLine(currentCell.directions, seed, currentCell, 5.0);
                }
            }
            else
            {
                if (plotStyle == PlotStyle.Curl)
                {
                    fieldLineList[0] = Calculator.GetFieldLine(currentCell.curls - currentCell.parent.directions, seed, currentCell, 5.0);
                }
                else 
                {
                    fieldLineList[0] = Calculator.GetFieldLine(currentCell.directions-currentCell.parent.directions, seed, currentCell, 5.0);
                }
            }
            pauseFieldLines = false;
        }

        public void GrabAllFieldLines()
        {
            double max = double.MinValue;
            double min = double.MaxValue;
            FieldLine currentFieldLine;
            for (int i = 0; i < currentCell.centroids.RowCount; i++)
            {
                pauseFieldLines = true;
                if (currentCell.isParent)
                {
                    if (plotStyle == PlotStyle.Curl)
                    {
                        currentFieldLine = Calculator.GetFieldLine(currentCell.curls, currentCell.centroids.Row(i), currentCell, 5.0);
                    }
                    else
                    {
                        currentFieldLine = Calculator.GetFieldLine(currentCell.directions, currentCell.centroids.Row(i), currentCell, 5.0);

                    }
                }
                else
                {
                    if (plotStyle == PlotStyle.Curl)
                    {
                        currentFieldLine = Calculator.GetFieldLine(currentCell.curls - currentCell.parent.curls, currentCell.centroids.Row(i), currentCell, 5.0);
                    }
                    else
                    {
                        currentFieldLine = Calculator.GetFieldLine(currentCell.directions - currentCell.parent.directions, currentCell.centroids.Row(i), currentCell, 5.0);
                    }
                }
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
            foreach (FieldLine fieldLine in fieldLineList)
            {
                for (int i = 0; i < fieldLine.Count; i++)
                {
                    if (fieldLine.Strength[i] == 0) fieldLine.Colors.Add(new Color4(0f, 0f, 1f, 1f));
                    else
                    {
                        double ratio = (Math.Log(fieldLine.Strength[i]) - logMin) / range;
                        fieldLine.Colors.Add(new Color4((float)ratio, 0f, (float)(1 - ratio), 1f));
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
#if _debug
            GL.Color4(Color4.Red);
            GL.Vertex3(target);
            GL.Color4(0, 0, 0, 0);
            GL.Vertex3(displayList[0]);
#endif
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
    }
}

