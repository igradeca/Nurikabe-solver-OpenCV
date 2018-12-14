using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.ML;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NurikabeSolver;

namespace Projekt_Nurikabe {
    class Detect {

        private ImageBox resultImageBox;

        private Image<Gray, byte> workImage, gridImage;        
        private LineSegment2D[] lines, verticalLines, horizontalLines;
        private Point[] intersectionPoints;
        private Point[][] points2D;
        private Mat kernel;

        private SVM svm;
        private Matrix<float> detectedCells;
        private int[][][] detectedNumbers;
        private int detectionCounter;

        private bool cellsAreDetectedRight;

        private NurikabeSolver.NurikabeSolver solver;
        private char[][] solvingResult;
        private Image<Bgr, byte> resultImage;

        public Detect(ImageBox resultImageBox) {

            this.resultImageBox = resultImageBox;

            detectionCounter = 0;

            workImage = new Image<Gray, byte>(new Size(640, 480));            
            kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));

            svm = new SVM();
            FileStorage file = new FileStorage("..//svm.txt", FileStorage.Mode.Read);
            svm.Read(file.GetNode("opencv_ml_svm"));
        }

        private void GetImageEdges() {

            CvInvoke.MedianBlur(workImage, workImage, 3);

            // ThresholdType.Binary, 11, 2
            CvInvoke.AdaptiveThreshold(workImage, workImage, 255, AdaptiveThresholdType.GaussianC, ThresholdType.BinaryInv, 101, 3);

            CvInvoke.MedianBlur(workImage, workImage, 3);

            workImage = workImage.MorphologyEx(MorphOp.Dilate, kernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(1.0));
            
            CvInvoke.Canny(workImage, workImage, 80.0, 120.0);
        }

        private void FindLines() {

            // Probabilistic Hough Transform
            double minLineLength = 100; // Minimum line length. Line segments shorter than that are rejected.       100
            double maxLineGap = 40; // Maximum allowed gap between points on the same line to link them.            40
            // 100, 100, 25     minLineLength = 100  maxLineGap = 15
            lines = CvInvoke.HoughLinesP(workImage, 1, Math.PI / 180, 100, minLineLength, maxLineGap);
        }
        
        private double DistanceBetweenTwoPoints(Point point1, Point point2, char axis) {

            double distance = -1.0;
            if (axis == 'X') {
                distance = Math.Sqrt(Math.Pow(point2.X - point1.X, 2));
            } else if (axis == 'Y') {
                distance = Math.Sqrt(Math.Pow(point2.Y - point1.Y, 2));
            }

            return distance;
        }

        private List<LineSegment2D> GroupLines(List<LineSegment2D> lines, char axis, double distanceThreshold = 10.0) {

            List<LineSegment2D> groupedLines = new List<LineSegment2D>();
            int foundLinesCounter, j;
            int averageX1, averageY1, averageX2, averageY2;
            double distanceX1, distanceY1;
            double distanceX2, distanceY2;

            for (int i = 0; i < lines.Count; i++) {                
                j = i + 1;
                foundLinesCounter = 0;

                averageX1 = 0;
                averageY1 = 0;
                averageX2 = 0;
                averageY2 = 0;

                while (j < lines.Count) {
                    distanceX1 = DistanceBetweenTwoPoints(lines[i].P1, lines[j].P1, 'X');
                    distanceY1 = DistanceBetweenTwoPoints(lines[i].P1, lines[j].P1, 'Y');

                    distanceX2 = DistanceBetweenTwoPoints(lines[i].P2, lines[j].P2, 'X');
                    distanceY2 = DistanceBetweenTwoPoints(lines[i].P2, lines[j].P2, 'Y');

                    if ((distanceX1 <= distanceThreshold && distanceX2 <= distanceThreshold && axis == 'X') ||
                        (distanceY1 <= distanceThreshold && distanceY2 <= distanceThreshold && axis == 'Y')) {

                        averageX1 += lines[j].P1.X;
                        averageY1 += lines[j].P1.Y;

                        averageX2 += lines[j].P2.X;
                        averageY2 += lines[j].P2.Y;

                        foundLinesCounter += 1;
                        lines.RemoveAt(j);
                    } else {
                        break;
                    }
                }

                if (foundLinesCounter >= 1) {
                    averageX1 /= foundLinesCounter;
                    averageY1 /= foundLinesCounter;

                    averageX2 /= foundLinesCounter;
                    averageY2 /= foundLinesCounter;

                    groupedLines.Add(new LineSegment2D(
                        new Point(averageX1, averageY1),
                        new Point(averageX2, averageY2)
                        ));
                }
            }

            return groupedLines;
        }

        private double CalculateK(LineSegment2D line) {

            double k;
            if ((line.P2.X - line.P1.X) == 0) {
                k = double.PositiveInfinity;
            } else {
                k = (line.P2.Y - line.P1.Y) / (line.P2.X - line.P1.X);
            }

            return k;
        }

        private double CalculateL(LineSegment2D line, double k) {

            return line.P1.Y - (k * line.P1.X);
        }

        /// <summary>
        /// When both k1 and k2 are not infinite.
        /// </summary>
        private Point CalculateIntersectionPoint(LineSegment2D line1, LineSegment2D line2, double k1, double k2) {

            double l1, l2, D, Dx, Dy;

            l1 = CalculateL(line1, k1);
            l2 = CalculateL(line2, k2);

            D = (-1 * k1) + k2;
            Dx = l1 - l2;
            Dy = (-1 * k1 * l2) + (k2 * l1);

            return new Point((int)(Dx / D), (int)(Dy / D));
        }
        
        private Point[] LinesIntersection(List<LineSegment2D> verticalLines, List<LineSegment2D> horizontalLines) {

            List<Point> intersectionPoints = new List<Point>();
            Point newPoint;
            double k1, k2;

            foreach (LineSegment2D hLine in horizontalLines) {
                foreach (LineSegment2D vLine in verticalLines) {

                    k1 = CalculateK(vLine);
                    k2 = CalculateK(hLine);

                    if (double.IsInfinity(k1)) {
                        intersectionPoints.Add(new Point(vLine.P1.X, (int)((k2 * vLine.P1.X) + CalculateL(hLine, k2))));

                    } else if (double.IsInfinity(k2)) {
                        intersectionPoints.Add(new Point(hLine.P1.X, (int)((k1 * hLine.P1.X) + CalculateL(vLine, k1))));

                    } else if (!double.IsInfinity(k1) && !double.IsInfinity(k1)) {
                        newPoint = CalculateIntersectionPoint(vLine, hLine, k1, k2);
                        intersectionPoints.Add(newPoint);
                    }
                }
            }

            return intersectionPoints.ToArray();
        }
        
        private void ProcessLines(Image<Bgr, byte> frameImage, int angleThreshold = 25) {

            List<LineSegment2D> verticalLines = new List<LineSegment2D>();
            List<LineSegment2D> horizontalLines = new List<LineSegment2D>();

            foreach (LineSegment2D line in lines) {
                double angle = line.GetExteriorAngleDegree(new LineSegment2D(new Point(1, 0), new Point(0, 0)));
                angle = Math.Abs(angle);

                if (angle >= (90 - angleThreshold) && angle <= (90 + angleThreshold)) {
                    verticalLines.Add(line);
                } else if (angle >= (180 - angleThreshold) && angle <= (180 + angleThreshold)) {
                    horizontalLines.Add(line);
                }
            }
            
            verticalLines = verticalLines.OrderBy(line => line.P1.X).ToList();
            horizontalLines = horizontalLines.OrderBy(line => line.P1.Y).ToList();

            verticalLines = GroupLines(verticalLines, 'X');
            horizontalLines = GroupLines(horizontalLines, 'Y');

            intersectionPoints = LinesIntersection(verticalLines, horizontalLines);

            this.verticalLines = verticalLines.ToArray();
            this.horizontalLines = horizontalLines.ToArray();
        }

        private bool IsNurikabeGridDetected() {

            switch (intersectionPoints.Length) {
                case (36):                      // 5 x 5
                    return true;
                case (64):                      // 7 x 7
                    return true;
                case (121):                     // 10x10
                    return true;
                case (169):                     // 12x12
                    return true;
                case (256):                     // 15x15
                    return true;
                case (441):                     // 20x20
                    return true;
            }

            return false;
        }

        private Rectangle CropGrid(int offset = 5) {

            if (intersectionPoints.Length < 36) {
                return new Rectangle();
            }

            Point upperLeftP;
            Size rectSize;

            upperLeftP = new Point(
                intersectionPoints[0].X, 
                intersectionPoints[0].Y
                );
            //upperLeftP.Offset(-offset, -offset);

            rectSize = new Size(
                Math.Abs(intersectionPoints[(intersectionPoints.Length - 1)].X - upperLeftP.X),
                Math.Abs(intersectionPoints[(intersectionPoints.Length - 1)].Y - upperLeftP.Y)
                );
            
            if ((rectSize.Width + upperLeftP.X) < upperLeftP.X ||
                (rectSize.Height + upperLeftP.Y) < upperLeftP.Y ||
                (rectSize.Width + upperLeftP.X) >= 640 ||
                (rectSize.Height + upperLeftP.Y) >= 480) {
                return Rectangle.Empty;
            }

            return new Rectangle(
                upperLeftP,
                rectSize
                );
        }

        private int GetNumberOfIntersectionPoints() {

            switch (intersectionPoints.Length) {
                case (36):                      // 5 x 5
                    return 6;
                case (64):                      // 7 x 7
                    return 8;
                case (121):                     // 10x10
                    return 11;
                case (169):                     // 12x12
                    return 13;
                case (256):                     // 15x15
                    return 16;
                case (441):                     // 20x20
                    return 21;
            }
            return 0;
        }

        private Point[][] TransferPointsTo2dArray() {

            Point[][] points2D;
            int numberOfIntersectionPoints;

            numberOfIntersectionPoints = GetNumberOfIntersectionPoints();
            points2D = new Point[numberOfIntersectionPoints][];            

            for (int i = 0; i < numberOfIntersectionPoints; i++) {
                points2D[i] = new Point[numberOfIntersectionPoints];
                for (int j = 0; j < numberOfIntersectionPoints; j++) {
                    points2D[i][j] = intersectionPoints[(i * numberOfIntersectionPoints) + j];
                }
            }

            return points2D;
        }

        private void SliceDetectedGrid(Rectangle cropRectangle, int locationOffset = 5, double sizeOffset = 0.75) {

            
            Rectangle cell;

            //CvInvoke.AdaptiveThreshold(gridImage, gridImage, 255, AdaptiveThresholdType.GaussianC, ThresholdType.BinaryInv, 31, 8);
            CvInvoke.AdaptiveThreshold(gridImage, gridImage, 255, AdaptiveThresholdType.GaussianC, ThresholdType.BinaryInv, 51, 8);
            
            points2D = TransferPointsTo2dArray();

            Point start;
            Size size;

            Image<Gray, byte> temp;
            detectedCells = new Matrix<float>((points2D.Length - 1) * (points2D.Length - 1), 36 * 36);

            for (int i = 0; i < points2D.Length - 1; i++) {
                for (int j = 0; j < points2D[i].Length - 1; j++) {
                    start = new Point(points2D[i][j].X - cropRectangle.X, points2D[i][j].Y - cropRectangle.Y);
                    start.Offset(locationOffset, locationOffset);

                    size = new Size(
                        Math.Abs((points2D[i + 1][j + 1].X - cropRectangle.X) - start.X),
                        Math.Abs((points2D[i + 1][j + 1].Y - cropRectangle.Y) - start.Y)
                        );

                    size.Width = (int)(sizeOffset * size.Width);
                    size.Height = (int)(sizeOffset * size.Height);

                    cell = new Rectangle(start, size);
                    CvInvoke.Rectangle(workImage, cell, new MCvScalar(200));
                    
                    if (size.Width > 0 && size.Height > 0
                        && start.X > 0 && start.Y > 0
                        && start.X < (gridImage.Width + start.X) && start.Y < (gridImage.Height + start.Y)
                        && (start.X + size.Width) < gridImage.Width && (start.Y + size.Height) < gridImage.Height) {

                        gridImage.ROI = cell;
                        //gridImage.Save("..//detected_cells//" + ((i * (points2D.Length - 1)) + j).ToString() + ".png");
                        temp = gridImage.Clone();
                        temp = temp.Resize(36, 36, Inter.Nearest);
                        
                        for (int k = 0; k < 36; k++) {
                            for (int m = 0; m < 36; m++) {
                                detectedCells[((i * (points2D.Length - 1)) + j), ((k * 36) + m)] = (float)temp.Data[k, m, 0];
                            }
                        }
                        
                        gridImage.ROI = Rectangle.Empty;
                    } else {
                        cellsAreDetectedRight = false;
                        return;
                    }
                }
            }

            cellsAreDetectedRight = true;
        }

        private void DetectNumbers() {

            int gridSize = GetNumberOfIntersectionPoints() - 1;

            if (detectionCounter == 0 || gridSize != detectedNumbers.Length || detectionCounter == 5) {
                detectedNumbers = new int[gridSize][][];

                for (int i = 0; i < detectedNumbers.Length; i++) {
                    detectedNumbers[i] = new int[gridSize][];
                    for (int j = 0; j < gridSize; j++) {
                        detectedNumbers[i][j] = new int[5];
                    }
                }

                detectionCounter = 0;
            }

            int k = 0, m = 0;
            for (int i = 0; i < detectedCells.Rows; i++) {
                Matrix<float> row = detectedCells.GetRow(i);
                float predict = svm.Predict(row);

                detectedNumbers[k][m][detectionCounter] = (int)predict;

                m++;
                if (m >= gridSize) {
                    m = 0;
                    k++;
                }
            }

            detectionCounter++;
        }

        private void SolveNurikabe() {

            char[][] gridToSolve = new char[detectedNumbers.Length][];
            int most;
            for (int i = 0; i < detectedNumbers.Length; i++) {
                gridToSolve[i] = new char[detectedNumbers.Length];
                for (int j = 0; j < detectedNumbers.Length; j++) {

                    most = detectedNumbers[i][j].GroupBy(x => x).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
                    gridToSolve[i][j] = most.ToString().ToCharArray()[0];
                }
            }

            solver = new NurikabeSolver.NurikabeSolver();
            solver.SetNewPuzzle(gridToSolve);
            solver.Begin();
            solvingResult = solver.GetResult();
        }

        private void DrawResult(Image<Bgr, byte> frameImage) {

            Point start;
            Size size;

            for (int i = 0; i < solvingResult.Length; i++) {                
                for (int j = 0; j < solvingResult[i].Length; j++) {

                    if (solvingResult[i][j] == 'B') {

                        start = new Point(points2D[i][j].X, points2D[i][j].Y);
                        size = new Size(
                            Math.Abs(points2D[i + 1][j + 1].X - start.X),
                            Math.Abs(points2D[i + 1][j + 1].Y - start.Y)
                            );

                        CvInvoke.Rectangle(
                            frameImage,
                            new Rectangle(start, size),
                            new MCvScalar(0,0,0),
                            -1
                            );
                    } else if (solvingResult[i][j] == 'F') {

                        start = new Point(
                            (points2D[i][j].X + points2D[i + 1][j + 1].X) / 2,
                            (points2D[i][j].Y + points2D[i + 1][j + 1].Y) / 2
                            );

                        CvInvoke.Circle(
                            frameImage,
                            start,
                            3,
                            new MCvScalar(0, 0, 0),
                            3
                            );
                    }
                }
            }
        }

        public Mat FindGridAndSolve(Image<Bgr, byte> frameImage) {

            workImage = frameImage.Convert<Gray, byte>();

            GetImageEdges();

            FindLines();
            ProcessLines(frameImage);
            
            if (IsNurikabeGridDetected()) {

                Rectangle cropRectangle = CropGrid();

                if (!cropRectangle.IsEmpty) {

                    workImage = frameImage.Convert<Gray, byte>();
                    workImage.ROI = cropRectangle;

                    gridImage = workImage.Copy();

                    SliceDetectedGrid(cropRectangle);

                    if (cellsAreDetectedRight) {
                        DetectNumbers();

                        if (detectionCounter == 5) {
                            SolveNurikabe();
                            resultImage = frameImage.Copy();
                            DrawResult(resultImage);

                            resultImageBox.Image = resultImage;
                        }
                    }
                    
                    
                }
            }

            
            foreach (LineSegment2D line in verticalLines) {
                CvInvoke.Line(frameImage, line.P1, line.P2, new MCvScalar(0, 255, 0), 2);
            }
            foreach (LineSegment2D line in horizontalLines) {
                CvInvoke.Line(frameImage, line.P1, line.P2, new MCvScalar(255, 0, 0), 2);
            }
            
            /*
            foreach (Point point in intersectionPoints) {
                CvInvoke.Circle(frameImage, point, 2, new MCvScalar(0, 0, 255), 2);
            }
            */

            return frameImage.Mat;
        }


    }
}
