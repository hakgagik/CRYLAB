//#define _debug

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public enum FieldLines
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
    }

    public class Plotter
    {
        private Thread runGameThread;

        private bool firstRun = true;
        public bool isRunning = false;
        private bool isPaused = false;

        private PlotStyle plotStyle;
        private FieldLines fieldLines;
        private double fieldLineDensity = 1.0;
        private SuperCell currentCell;
        private List<Vector3d> displayList;

        private Vector3d eye;
        private Vector3d target;
        private Vector3d up;
        private Vector3d right;
        public double theta = 0;
        public double phi = - Math.PI/2.0;
        public double r;

        private double[] extents;
        private int width;
        private int height;
        private double gameWidth;
        private double gameHeight;
        private double gameDepth;
        private double depthClipPlane;

        private bool rotationLocked = false;
        private bool mouseDown = false;
        private MouseButton mouseButton;

        public Color4[] colors;
        public Color4[] molColors;
        public Color4 bgColor;
        public MiscInfo miscInfo;

        
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
                            theta = Calculator.Clip(0,Math.PI,theta);
                            phi -= (double)e.XDelta * Math.PI / 200.0;
                            UpdateView();
                        }
                        break;
                    case MouseButton.Right:
                        target -= e.XDelta * right * gameWidth / (double)width - e.YDelta * up * gameHeight / (double)height;
                        eye -= e.XDelta * right * gameWidth / (double)width - e.YDelta * up * gameHeight / (double)height;
                        break;
                }
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
        }
        
        void game_RenderFrame(object sender, FrameEventArgs e)
        {
            if (!isPaused)
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();

                PlotPicker();

                ((GameWindow)sender).SwapBuffers();
            }
        }
        
        void game_UpdateFrame(object sender, FrameEventArgs e)
        {
            if (!isPaused)
            {
                GameWindow game = (GameWindow)sender;
                width = game.Width;
                height = game.Height;
                if (game.Keyboard[Key.Escape])
                {
                    game.Exit();
                }
            }
        }
        
        void game_Resize(object sender, EventArgs e)
        {
            GameWindow game = (GameWindow)sender;
            GL.Viewport(0, 0, game.Width, game.Height);
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
                    switch (fieldLines)
                    {
                        case FieldLines.Single:
                            RenderSingleFieldLines();
                            break;
                        case FieldLines.Full:
                            RenderFullFieldLines();
                            break;
                    }
                    break;
                case PlotStyle.Curl:
                    RenderVectorField();
                    switch (fieldLines)
                    {
                        case FieldLines.Single:
                            RenderSingleFieldLines();
                            break;
                        case FieldLines.Full:
                            RenderFullFieldLines();
                            break;
                    }
                    break;
            }
        }
        
        public void Plot(SuperCell superCell, PlotStyle plotStyle)
        {
            Pause();
            SetPlotInfo(superCell, plotStyle);
            UnPause();
        }
        
        public void PlotWithFieldLines(SuperCell superCell, PlotStyle PlotStyle, FieldLines FieldLines, double density)
        {
            currentCell = superCell;
            plotStyle = PlotStyle;
            fieldLines = FieldLines;
            fieldLineDensity = density;
            if (!isRunning)
            {
                PlotInit();
                isRunning = true;
            }
            throw new NotImplementedException();
        }// Unfinished!

        private void SetPlotInfo(SuperCell superCell, PlotStyle thisPlotStyle)
        {
            currentCell = superCell;
            plotStyle = thisPlotStyle;
            fieldLines = FieldLines.None;
            miscInfo = new MiscInfo();
            displayList = new List<Vector3d>();
            extents = new double[] { double.MaxValue, double.MinValue, double.MaxValue, double.MinValue, double.MaxValue, double.MinValue };
            target = new Vector3d(0.0, 0.0, 0.0);
            up = new Vector3d(0.0, 1.0, 0.0);
            right = new Vector3d(1.0, 0.0, 0.0);
            rotationLocked = false;
            theta = 0.0;
            phi = -Math.PI / 2;

            if (!isRunning)
            {
                runGameThread = new Thread(PlotInit);
                runGameThread.Start();
                isRunning = true;
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
                    for (int i = 0; i < superCell.centroids.RowCount; i++)
                    {
                        Vector<double> tempCentroid = superCell.centroids.Row(i);
                        Vector3d centroid = new Vector3d(tempCentroid[0], tempCentroid[1], tempCentroid[2]);
                        extents = Calculator.UpdateExtents(extents, centroid);
                        target += centroid;
                        displayList.Add(centroid);
                        Vector<double> direction = superCell.directions.Row(i);
                        displayList.Add(new Vector3d(direction[0], direction[1], direction[2]));
                    }
                    break;
                case PlotStyle.Curl:
                    if (superCell.curls == null)
                    {
                        superCell.curls = Calculator.CalculateCurls(superCell);
                    }
                    for (int i = 0; i < superCell.centroids.RowCount; i++)
                    {
                        Vector<double> tempCentroid = superCell.centroids.Row(i);
                        Vector3d centroid = new Vector3d(tempCentroid[0], tempCentroid[1], tempCentroid[2]);
                        extents = Calculator.UpdateExtents(extents, centroid);
                        target += centroid;
                        displayList.Add(centroid);
                        Vector<double> curls = superCell.curls.Row(i);
                        displayList.Add(new Vector3d(curls[0], curls[1], curls[2]));
                    }
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
        
        public void UpdateView()
        {
            Vector3d displacement = r * (new Vector3d(Math.Sin(theta) * Math.Cos(phi), Math.Sin(theta) * Math.Sin(phi), Math.Cos(theta)));
            eye = target + displacement;
            right = new Vector3d(-Math.Sin(phi), Math.Cos(phi), 0.0);
            up = -Vector3d.Normalize(Vector3d.Cross(right, displacement));
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
            throw new NotImplementedException();
        }
        
        private void RenderSingleFieldLines()
        {
            throw new NotImplementedException();
        }
        
        private void RenderFullFieldLines()
        {
            throw new NotImplementedException();
        }
    }
}
