using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Raster;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly int _maxX;
    private readonly int _maxY;
    delegate void Build(int x0, int y0, int x1, int y1);

    private int _radius;
    private bool _isCircle = false;

    private Build? _build = null;

    private (Rectangle? First, Rectangle? Second) _pair;

    public MainWindow()
    {
        InitializeComponent();

        _pair = new(null, null);
        bool flag = true;

        _maxX = MainGrid.Columns / 2;
        _maxY = MainGrid.Rows / 2;

        for (int i = 0; i < MainGrid.Rows; i++)
        {
            for (int j = 0; j < MainGrid.Columns; j++)
            {
                var rect = new Rectangle
                {
                    Fill = flag ? Brushes.White : Brushes.LightGray
                };

                rect.MouseLeftButtonUp += PixelChosen;
                rect.MouseMove += PixelMouseEnter;
                rect.MouseLeave += PixelMouseLeave;
                MainGrid.Children.Add(rect);

                flag = !flag;
            }

            if (MainGrid.Columns % 2 == 0)
            {
                flag = !flag;
            }
        }

        //SetPixel(-3, 2);
        //StepAlg(1, 2, 3, 8);
        //BrezAlg(1, 2, 3, 8);
    }

    private void BrezAlg(int x0, int y0, int x1, int y1)
    {
        var steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);

        if (steep)
        {
            (x0, y0) = (y0, x0);
            (x1, y1) = (y1, x1);
        }

        if (x0 > x1)
        {
            (x0, x1) = (x1, x0);
            (y0, y1) = (y1, y0);
        }

        var dx = x1 - x0;
        var dy = Math.Abs(y1 - y0);
        var error = dx / 2;
        var yStep = y1 > y0 ? 1 : -1;
        var y = y0;

        for (var x = x0; x <= x1; x++)
        {
            SetPixel(steep ? y : x, steep ? x : y);
            error -= dy;
            if (error < 0)
            {
                y += yStep;
                error += dx;
            }
        }
    }

    private void StepAlg(int x0, int y0, int x1, int y1)
    {
        var ta = y1 - y0;
        var tb = x1 - x0;
        var k = (decimal)ta / tb;
        var b = y0 - ta * x0 / (decimal)tb;

        for (var x = x0; x <= x1; x++)
        {
            var y = (int)Math.Round(k * x + b);
            SetPixel(x, y);
        }
    }

    private void SetPixel(int x, int y)
    {
        if (Math.Abs(x) <= _maxX && Math.Abs(y) <= _maxX)
        {
            (MainGrid.Children[(MainGrid.Rows / 2 - y) * MainGrid.Columns + MainGrid.Columns / 2 + x] as Rectangle)!.Fill = Brushes.Black;
        }
    }

    private void PixelMouseEnter(object sender, MouseEventArgs e)
    {
        if (_pair.First != sender && _pair.Second is null && !_isCircle)
        {
            (sender as Rectangle)!.Fill = Brushes.DodgerBlue;
        }
    }

    private void PixelMouseLeave(object sender, MouseEventArgs e)
    {
        if (_pair.First != sender && _pair.Second != sender && _pair.Second is null && !_isCircle)
        {
            var index = MainGrid.Children.IndexOf(sender as Rectangle);

            var old = index % 2 == 0 ? Brushes.White : Brushes.LightGray;

            (sender as Rectangle)!.Fill = old;
        }
    }

    private void PixelChosen(object sender, MouseEventArgs e)
    {
        if (_isCircle)
        {
            var c = GetPixelsCoordinates((sender as Rectangle)!);
            BrezCircle(c.X, c.Y, _radius);
            Status.Text = "Окружность построена.";
            Cursor = Cursors.Arrow;
        }
        else if (_build is not null)
        {
            if (_pair.First is null)
            {
                _pair.First = (sender as Rectangle)!;
                _pair.First.Fill = Brushes.Black;
                Status.Text = "Выберите вторую точку.";
            }
            else if (_pair.Second is null)
            {
                _pair.Second = (sender as Rectangle)!;
                _pair.Second.Fill = Brushes.Black;
                BuildLine();
            }
        }
    }

    private void BuildLine()
    {
        var f = GetPixelsCoordinates(_pair.First!);
        var s = GetPixelsCoordinates(_pair.Second!);
        _build!.Invoke(f.X, f.Y, s.X, s.Y);
        Status.Text = "Линия построена.";
        Cursor = Cursors.Arrow;
    }

    private (int X, int Y) GetPixelsCoordinates(Rectangle rect)
    {
        var index = MainGrid.Children.IndexOf(rect);
        return (-_maxX + index % (2 * _maxX + 1), _maxY - index / (2 * _maxX + 1));
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.C)
        {
            Clear();
            Status.Text = "Очищено.";
        }
    }

    private void Clear()
    {
        _isCircle = false;
        Status.Text = "";
        _pair.First = null;
        _pair.Second = null;
        _build = null;
        Cursor = Cursors.Arrow;

        bool flag = true;

        for (int i = 0; i < MainGrid.Rows; i++)
        {
            for (int j = 0; j < MainGrid.Columns; j++)
            {
                (MainGrid.Children[i * MainGrid.Rows + j] as Rectangle).Fill = flag ? Brushes.White : Brushes.LightGray;

                flag = !flag;
            }

            if (MainGrid.Columns % 2 == 0)
            {
                flag = !flag;
            }
        }
    }

    private void StepBuild_Click(object sender, RoutedEventArgs e)
    {
        Clear();
        _build = StepAlg;
        Status.Text = "Для построения линии нужно выбрать две точки. Выберите первую точку.";
        Cursor = Cursors.Hand;
    }

    private void BrezBuild_Click(object sender, RoutedEventArgs e)
    {
        Clear();
        _build = BrezAlg;
        Status.Text = "Для построения линии нужно выбрать две точки. Выберете первую точку.";
        Cursor = Cursors.Hand;
    }

    private void CdaBuild_Click(object sender, RoutedEventArgs e)
    {
        Clear();
        _build = DDA;
        Status.Text = "Выберете две точки на плоскости.";
        Cursor = Cursors.Hand;
    }

    private void BrezCircleBuild_Click(object sender, RoutedEventArgs e)
    {
        Clear();
        _isCircle = true;
        if (!Int32.TryParse(Radius.Text, out _radius) && _radius <= 0)
        {
            Status.Text = "Радиус должен быть положительным целочисленным числом.";
            return;
        }
        Status.Text = "Выберете центр окружности.";
        Cursor = Cursors.Hand;
    }

    private void DDA(int x0, int y0, int x1, int y1)
    {
        var dx = x1 - x0;
        var dy = y1 - y0;

        var steps = Math.Abs(dx) > Math.Abs(dy) ? Math.Abs(dx) : Math.Abs(dy);

        var incX = dx / (float)steps;
        var incY = dy / (float)steps;

        float x = x0;
        float y = y0;
        for (var i = 0; i <= steps; i++)
        {
            SetPixel((int)Math.Round(x), (int)Math.Round(y));
            x += incX;
            y += incY;
        }
    }

    private void Circle(int xc, int yc, int x, int y)
    {
        SetPixel(xc + x, yc + y);
        SetPixel(xc - x, yc + y);
        SetPixel(xc + x, yc - y);
        SetPixel(xc - x, yc - y);
        SetPixel(xc + y, yc + x);
        SetPixel(xc - y, yc + x);
        SetPixel(xc + y, yc - x);
        SetPixel(xc - y, yc - x);
    }

    private void BrezCircle(int xc, int yc, int r)
    {
        int x = 0, y = r;
        int d = 3 - 2 * r;
        Circle(xc, yc, x, y);

        while (y >= x)
        {
            x++;

            if (d > 0)
            {
                y--;
                d = d + 4 * (x - y) + 10;
            }
            else
            {
                d = d + 4 * x + 6;
            }

            Circle(xc, yc, x, y);
        }
    }
}