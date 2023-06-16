using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Net;

namespace Test2
{
    public partial class MainForm : Form
    {
        private List<List<PointF>> graphs; // Массив точек
        private bool isAxesDrawn; //Нарисована ли ось абцис
        private float scale; //Величина графика

        private int selectedGraphIndex; // Индекс выбранного графика
        private Color selectedGraphColor; // Цвет выбранного графика
        private bool isGraphSelected; // Флаг, указывающий, выбран ли график
        public MainForm()
        {
            InitializeComponent();
            graphs = new List<List<PointF>>();
            isAxesDrawn = false;
            scale = 10.0f;

            selectedGraphIndex = -1;
            selectedGraphColor = Color.Red;
            isGraphSelected = false;

            panel1.MouseClick += Panel1_MouseClick;
            panel1.Focus();
            panel1.PreviewKeyDown += panel1_PreviewKeyDown; // Добавляем обработчик события KeyDown
        }
        private void MainForm_Load(object sender, EventArgs e) { 

        }
        private void Panel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Проверяем, находится ли щелчок мыши в пределах какого-либо графика
                for (int i = 0; i < graphs.Count; i++)
                {
                    List<PointF> points = graphs[i];
                    PointF[] scaledPoints = ScalePoints(points);

                    GraphicsPath path = new GraphicsPath();
                    path.AddLines(scaledPoints);

                    if (IsPointInGraph(e.Location, scaledPoints))
                    {
                        isGraphSelected = !isGraphSelected;
                        selectedGraphIndex = i;
                        selectedGraphColor = !isGraphSelected ? Color.Blue : Color.Red;

                        // Перерисовываем графики
                        RedrawGraphs();

                        break;
                    }
                }
            }
        }
        private bool IsPointInGraph(PointF location, PointF[] scaledPoints)
        {
            const float tolerance = 15f; // Допуск на попадание в область графика

            for (int i = 1; i < scaledPoints.Length; i++)
            {
                PointF p1 = scaledPoints[i - 1];
                PointF p2 = scaledPoints[i];

                // Создаем прямоугольник вокруг отрезка
                RectangleF rect = new RectangleF(p1, new SizeF(p2.X - p1.X, p2.Y - p1.Y));

                // Увеличиваем размеры прямоугольника с учетом допуска
                rect.Inflate(tolerance, tolerance);

                // Проверяем, находится ли точка в прямоугольнике
                if (rect.Contains(location))
                {
                    return true;
                }
            }

            return false;
        }
        private void openFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files (*.txt)|*.txt";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (!isAxesDrawn)
                {
                    DrawAxes();
                    isAxesDrawn = true;
                }
                foreach (string fileName in openFileDialog.FileNames)
                {
                        List<PointF> points = ReadPointsFromFile(fileName);
                        graphs.Add(points);
                        DrawGraph(points);
                }
            }
        }
        private List<PointF> ReadPointsFromFile(string fileName)
        {
            List<PointF> points = new List<PointF>();

            string[] lines = File.ReadAllLines(fileName);
            foreach (string line in lines)
            {
                if (line == "gap")
                {
                    points.Add(new PointF(100000, 100000));
                }
                else {
                    string[] coordinates = line.Split(':');
                    if (coordinates.Length == 2)
                    {
                        float x, y;
                        if (float.TryParse(coordinates[0], out x) && float.TryParse(coordinates[1], out y))
                        {
                            points.Add(new PointF(x, y));
                        }
                    }
                }
            }

            return points;
        }
        private void DrawGraph(List<PointF> points)
        {
            using (Graphics g = panel1.CreateGraphics())
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                Pen pen = new Pen(Color.Blue);
                if (isGraphSelected && graphs.IndexOf(points) == selectedGraphIndex)
                {
                    pen.Color = selectedGraphColor; // Используем выбранный цвет для выбранного графика
                }
                PointF[] scaledPoints = ScalePoints(points);
                for (int i = 0; i < points.Count - 1; i++)
                {
                    if (((points[i].X == 100000 && points[i].Y == 100000) || (points[i+1].X == 100000 && points[i+1].Y == 100000)))
                    {
                        continue;
                    }
                    else
                    {
                        g.DrawLine(pen, scaledPoints[i], scaledPoints[i+1]);
                    }
                }
            }
        }
        private void RedrawGraphs()
        {
            panel1.Refresh(); // Очищаем панель для перерисовки графиков
            panel1.Controls.Clear();
            if (isAxesDrawn)
            {
                DrawAxes();
            }
            // Перерисовываем все графики
            foreach (List<PointF> points in graphs)
            {
                DrawGraph(points);
            }
        }
        private PointF[] ScalePoints(List<PointF> points)
        {
            PointF[] scaledPoints = new PointF[points.Count];
            float scaleX = panel1.Width / (scale * 10);
            float scaleY = panel1.ClientSize.Height / (scale * 10);
            float centerY = panel1.Height / 2;
            float centerX = panel1.Width / 2;
            float xRes, yRes;
            for (int i = 0; i < points.Count; i++)
            {
                xRes = points[i].X;
                yRes = points[i].Y;
                float scaledX = centerX + 45 - xRes * scaleX;
                float scaledY = centerY + 5 - yRes * scaleY;
                scaledPoints[i] = new PointF(scaledX, scaledY);
            }
            return scaledPoints;
        }
        private void DrawAxes()
        {
            using (Graphics g = panel1.CreateGraphics())
            {
                float centerX = panel1.Width / 2;
                float centerY = panel1.Height / 2;
                Pen pen = new Pen(Color.LightGray, 1.1f);
                for (int i = 0; i < panel1.Height; i += 10)
                {
                    g.DrawLine(pen, 0, i, panel1.Width - 10, i);
                }
                for (int i = 0; i <= panel1.Width; i += 10)
                {
                    g.DrawLine(pen, i, 0, i, panel1.Height - 10);
                }
                Pen axisPen = new Pen(Color.Black, 2f);
                g.DrawLine(axisPen, 0, centerY, panel1.Width - 10, centerY);
                g.DrawLine(axisPen, centerX, 0, centerX, panel1.Height - 10);
                for (int i = 0; i < panel1.Width -20; i += 10)
                {
                    g.DrawLine(axisPen, i, centerY - 3, i, centerY + 3);
                }

                for (int i = 20; i < panel1.Height; i += 10)
                {
                    g.DrawLine(axisPen, centerX - 3, i, centerX + 3, i);
                }
                Brush brush = Brushes.Black;
                int axisPadding = -1;
                int axisPaddingWord = 20;
                int xAxisY = panel1.Height - axisPadding;

                // Рисуем стрелку оси ординат
                g.FillPolygon(brush, new PointF[] {
                    new PointF(centerX - 5, axisPadding + 10),
                    new PointF(centerX + 5, axisPadding + 10),
                    new PointF(centerX, axisPadding)
                });

                // Подписываем ось ординат
                string yAxisLabel = "Y";
                SizeF yAxisLabelSize = g.MeasureString(yAxisLabel, Font);
                g.DrawString(yAxisLabel, Font, brush, new PointF(centerX - yAxisLabelSize.Width + 15, axisPaddingWord - 20));

                // Рисуем стрелку оси абсцисс
                g.FillPolygon(brush, new PointF[] {
                    new PointF(panel1.Width - axisPadding - 20, centerY - 5),
                    new PointF(panel1.Width - axisPadding - 20, centerY + 5),
                    new PointF(panel1.Width - axisPadding - 10, centerY)
                });

                // Подписываем ось абсцисс
                string xAxisLabel = "X";
                SizeF xAxisLabelSize = g.MeasureString(xAxisLabel, Font);
                g.DrawString(xAxisLabel, Font, brush, new PointF(panel1.Width - axisPadding - xAxisLabelSize.Width - 10, centerY - 20));
            }
        }
        private void MainForm_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    e.IsInputKey = true;
                    break;
            }
        }
        private List<PointF> MoveGraph(List<PointF> points, int dx, int dy)
        {
            List<PointF> movedPoints = new List<PointF>();
            PointF movedPoint = new PointF();
            foreach (PointF point in points)
            {
                if ((point.X != 100000) && (point.Y != 100000))
                {
                    movedPoint = new PointF(point.X + dx, point.Y + dy);
                }
                else
                {
                    movedPoint = new PointF(point.X, point.Y);
                }
                movedPoints.Add(movedPoint);
            }

            return movedPoints;
        }
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (isGraphSelected)
            {
                List<PointF> selectedGraph = graphs[selectedGraphIndex];

                switch (e.KeyCode)
                {
                    case Keys.Left:
                        graphs[selectedGraphIndex] = MoveGraph(graphs[selectedGraphIndex], 5, 0);
                        break;
                    case Keys.Right:
                        graphs[selectedGraphIndex] = MoveGraph(graphs[selectedGraphIndex], -5, 0);
                        break;
                    case Keys.Up:
                        graphs[selectedGraphIndex] = MoveGraph(graphs[selectedGraphIndex], 0, 10);
                        break;
                    case Keys.Down:
                        graphs[selectedGraphIndex] = MoveGraph(graphs[selectedGraphIndex], 0, -10);
                        break;
                    case Keys.W:
                        MoveSize(selectedGraph, true);
                        break;
                    case Keys.S:
                        MoveSize(selectedGraph, false);
                        break;
                }

                RedrawGraphs();
            }
        }
        private void MoveSize(List<PointF> graph, bool x)
        {
            if (x)
            {
                scale += 1.0f;
                ScalePoints(graph);
            }
            else
            {
                scale -= 1.0f;
                ScalePoints(graph);
            }
        }
        private void panel1_MouseClick_1(object sender, MouseEventArgs e)
        {
            panel1.Focus();
        }
        private void panel1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.W:
                case Keys.S:
                    e.IsInputKey = true;
                    break;
            }
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
