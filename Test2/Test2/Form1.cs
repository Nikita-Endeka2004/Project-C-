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

namespace Test2
{
    public partial class MainForm : Form
    {
        private List<List<PointF>> graphs;
        private bool isAxesDrawn;
        private float scale;

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
                foreach (string fileName in openFileDialog.FileNames)
                {
                    List<PointF> points = ReadPointsFromFile(fileName);
                    graphs.Add(points);
                    DrawGraph(points);
                }

                if (!isAxesDrawn)
                {
                    DrawAxes();
                    isAxesDrawn = true;
                }
            }
        }

        private List<PointF> ReadPointsFromFile(string fileName)
        {
            List<PointF> points = new List<PointF>();

            string[] lines = File.ReadAllLines(fileName);
            foreach (string line in lines)
            {
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
                g.DrawLines(pen, scaledPoints); 
            }
        }

        private void RedrawGraphs()
        {
            panel1.Refresh(); // Очищаем панель для перерисовки графиков

            // Перерисовываем все графики
            foreach (List<PointF> points in graphs)
            {
                DrawGraph(points);
            }

            if (isAxesDrawn)
            {
                DrawAxes();
            }
        }

        private PointF[] ScalePoints(List<PointF> points)
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            // Находим минимальные и максимальные значения по осям
            foreach (PointF point in points)
            {
                if (point.X < minX)
                    minX = point.X;
                if (point.X > maxX)
                    maxX = point.X;
                if (point.Y < minY)
                    minY = point.Y;
                if (point.Y > maxY)
                    maxY = point.Y;
            }

            // Масштабируем точки относительно размера панели
            PointF[] scaledPoints = new PointF[points.Count];
            float scaleX = panel1.Width / (maxX - minX) / scale;
            float scaleY = panel1.ClientSize.Height / (maxY - minY) / scale;

            for (int i = 0; i < points.Count; i++)
            {
                float x = points[i].X;
                float y = points[i].Y;
                float scaledX = (x - minX) * scaleX;
                float scaledY = panel1.Height - 35 - (y - minY) * scaleY; // инвертируем Y-координату для правильного отображения
                /*points[i] = new PointF(scaledX, scaledY);*/ // Обновляем координаты точек
                scaledPoints[i] = new PointF(scaledX, scaledY);
            }

            return scaledPoints;
        }
        private void DrawAxes()
        {
            using (Graphics g = panel1.CreateGraphics())
            {
                Pen pen = new Pen(Color.Black);
                Brush brush = Brushes.Black;
                int axisPadding = 20;
                int xAxisY = panel1.Height - axisPadding;
                int yAxisX = axisPadding;

                // Рисуем ось ординат
                g.DrawLine(pen, yAxisX, axisPadding, yAxisX, xAxisY);

                // Рисуем стрелку оси ординат
                g.FillPolygon(brush, new PointF[] {
                    new PointF(yAxisX - 5, axisPadding + 10),
                    new PointF(yAxisX + 5, axisPadding + 10),
                    new PointF(yAxisX, axisPadding)
                });

                // Подписываем ось ординат
                string yAxisLabel = "Y";
                SizeF yAxisLabelSize = g.MeasureString(yAxisLabel, Font);
                g.DrawString(yAxisLabel, Font, brush, new PointF(yAxisX - yAxisLabelSize.Width - 5, axisPadding + 10));

                // Рисуем ось абсцисс
                g.DrawLine(pen, yAxisX, xAxisY, panel1.Width - axisPadding, xAxisY);

                // Рисуем стрелку оси абсцисс
                g.FillPolygon(brush, new PointF[] {
                    new PointF(panel1.Width - axisPadding - 10, xAxisY - 5),
                    new PointF(panel1.Width - axisPadding - 10, xAxisY + 5),
                    new PointF(panel1.Width - axisPadding, xAxisY)
                });

                // Подписываем ось абсцисс
                string xAxisLabel = "X";
                SizeF xAxisLabelSize = g.MeasureString(xAxisLabel, Font);
                g.DrawString(xAxisLabel, Font, brush, new PointF(panel1.Width - axisPadding - xAxisLabelSize.Width - 5, xAxisY - xAxisLabelSize.Height - 5));
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

        //private void MoveGraph(List<PointF> graph, float deltaX, float deltaY)
        //{
        //    for (int i = 0; i < graph.Count; i++)
        //    {
        //        PointF point = graph[i];
        //        point.X += deltaX;
        //        point.Y += deltaY;
        //        graph[i] = point;
        //    }
        //}

        private List<PointF> MoveGraph(List<PointF> points, int dx, int dy)
        {
            List<PointF> movedPoints = new List<PointF>();
            //float dx = mouseLocation.X / (100 * scale);
            //float dy = mouseLocation.Y / (100 * scale);

            foreach (PointF point in points)
            {
                PointF movedPoint = new PointF(point.X + dx, point.Y + dy);
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
                        graphs[selectedGraphIndex] = MoveGraph(graphs[selectedGraphIndex], -10, 0);
                        break;
                    case Keys.Right:
                        graphs[selectedGraphIndex] = MoveGraph(graphs[selectedGraphIndex], 10, 0);
                        break;
                    case Keys.Up:
                        graphs[selectedGraphIndex] = MoveGraph(graphs[selectedGraphIndex], 0, -10);
                        break;
                    case Keys.Down:
                        graphs[selectedGraphIndex] = MoveGraph(graphs[selectedGraphIndex], 0, 10);
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
