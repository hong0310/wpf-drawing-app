using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;



namespace DrawingBoard
{
    public partial class MainWindow : Window
    {
        List<DrawStack.Data> ShapeStack = new List<DrawStack.Data>();
        Point resizeStartLocation1, resizeStartLocation2;
        Shape ingShape;
        int ShapeStackCount = -1;
        int ShapeStackCountMax = -1;
        Ellipse[] ShapeControlDot = new Ellipse[8];
        Rectangle background = new Rectangle();
        Brush fillColor = Brushes.White, lineColor = Brushes.Black;
        double lineStroke = 1;
        Line drawLine;
        int lineKinds;
        Rectangle drawRectangle;
        Point mousePoint1, mousePoint2;
        Point controlDotMousePoint1, controlDotMousePoint2;
        Point movePoint1, movePoint2;
        Point MoveStartPoint1, MoveStartPoint2;
        bool onClickDrawShape = false;
        bool onClickContorlDot = false;
        bool onClickShape = false;
        bool onSelectShape = false;
        int shapeKind = 1;
        public MainWindow()
        {
            InitializeComponent();
            background.Fill = Brushes.White;
            background.Cursor = Cursors.Cross;
            MainCanvas.Children.Add(background);
            SettingShapeControlDot();
        }

        /*   ---   LOAD IMAGE   ---   */
        private void Load(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Image files | *.bmp; *.jpg; *.png";
            openDialog.FilterIndex = 1;
            if (openDialog.ShowDialog() == true)
            {
                imagePicture.Source = new BitmapImage(new Uri(openDialog.FileName));
            }
        }





        /*   ---   DRAW LINE OR RECTANGLE   ---   */
        public Shape DrawShape(double x1, double y1, double x2, double y2)
        {
            Shape result;
            switch (shapeKind)
            {
                case 1:
                    result = DrawLine(x1, y1, x2, y2);
                    break;

                case 2:
                    result = DrawRectangle(x1, y1, x2, y2);
                    break;
                
                //set default to draw lines
                default:
                    result = DrawLine(0, 0, 0, 0);
                    break;
            }
            result.MouseDown += Shape_MouseDown;
            result.Cursor = Cursors.Hand;
            ingShape = SelectShape();
            SetSizeShape(ingShape, x1, y1, x2, y2);
            return result;
        }

        /*   ---   LINE   ---   */
        public Line DrawLine(double x1, double y1, double x2, double y2)
        {
            drawLine = new Line();
            drawLine.Stroke = lineColor;
            drawLine.StrokeThickness = lineStroke;
            return drawLine;
        }
        /*   ---   RECTANGLE   ---   */
        public Rectangle DrawRectangle(double x1, double y1, double x2, double y2)
        {
            drawRectangle = new Rectangle();
            drawRectangle.Fill = fillColor;
            drawRectangle.Stroke = lineColor;
            drawRectangle.StrokeThickness = lineStroke;
            return drawRectangle;
        }
        void SetSizeShape(Shape shape, double x1, double y1, double x2, double y2)
        {
            switch (shape.ToString().Split('.')[shape.ToString().Split('.').Length - 1])
            {
                case "Line":
                    SetSizeLine((Line)shape, x1, y1, x2, y2);
                    break;
                case "Rectangle":
                    SetSizeRectangle((Rectangle)shape, x1, y1, x2, y2);
                    break; 
            }
        }


        /*   ---   MAKE   ---   */

        //get position of mouse
        public Point GetMousePosition()
        {
            return Mouse.GetPosition(MainCanvas);
        }

        //line
        private void SetSizeLine(Line shape, double x1, double y1, double x2, double y2)
        {
            shape.X1 = x1;
            shape.Y1 = y1;
            shape.X2 = x2;
            shape.Y2 = y2;
        }
        //rectangle
        private void SetSizeRectangle(Rectangle shape, double x1, double y1, double x2, double y2)
        {
            shape.Width = (x1 > x2 ? x1 : x2) - (x1 < x2 ? x1 : x2);
            shape.Height = (y1 > y2 ? y1 : y2) - (y1 < y2 ? y1 : y2);
            Canvas.SetLeft(shape, (x1 < x2 ? x1 : x2));
            Canvas.SetTop(shape, (y1 < y2 ? y1 : y2));
        }



        /*   ---   MOVE   ---   */
        private void Shape_MouseDown(object sender, MouseButtonEventArgs e)
        { 
            onClickShape = true;
            movePoint1 = GetMousePosition();
        }

        void MoveShape(Shape shape)
        {
            movePoint2 = GetMousePosition();
            switch (shapeKind)
            {
                case 1:
                    MoveLine((Line)shape, movePoint1.X, movePoint1.Y, movePoint2.X, movePoint2.Y);
                    break;
                case 2:
                    MoveRectangle((Rectangle)shape, movePoint1.X, movePoint1.Y, movePoint2.X, movePoint2.Y);
                    break;
            }

            controlDotMousePoint1.X += (movePoint2.X - movePoint1.X);
            controlDotMousePoint1.Y += (movePoint2.Y - movePoint1.Y);
            controlDotMousePoint2.X += (movePoint2.X - movePoint1.X);
            controlDotMousePoint2.Y += (movePoint2.Y - movePoint1.Y);
            ShowShapeControlDot(controlDotMousePoint1.X, controlDotMousePoint1.Y, controlDotMousePoint2.X, controlDotMousePoint2.Y);

            movePoint1 = movePoint2;
        }
        //move rectangle
        private void MoveRectangle(Rectangle shape, double x1, double y1, double x2, double y2)
        {
            Canvas.SetLeft(shape, Canvas.GetLeft(shape) + (x2 - x1));
            Canvas.SetTop(shape, Canvas.GetTop(shape) + (y2 - y1));
        }
        //move line
        private void MoveLine(Line shape, double x1, double y1, double x2, double y2)
        {
            shape.X1 += (x2 - x1);
            shape.Y1 += (y2 - y1);
            shape.X2 += (x2 - x1);
            shape.Y2 += (y2 - y1);
        }
       
   


        /*   ---   SET 8 DOTS TO CONTROL RESIZE   ---   */
        void SettingShapeControlDot()
        {
            for(int i = 0; i < 8; i++)
            {
                ShapeControlDot[i] = new Ellipse();
                ShapeControlDot[i].Width = 5;
                ShapeControlDot[i].Height = 5;
                ShapeControlDot[i].Fill = Brushes.Black;
                ShapeControlDot[i].Stroke = Brushes.Black;
                ShapeControlDot[i].StrokeThickness = 2;
                ShapeControlDot[i].MouseDown += ShapeControlDot_MouseDown;
                ShapeControlDot[i].MouseDown += ShapeControlDot_MouseDown;
                ShapeControlDot[i].Name = ("ID_" + (i + 1).ToString());
                Panel.SetZIndex(ShapeControlDot[i], 10);
                MainCanvas.Children.Add(ShapeControlDot[i]);
            }
            ShapeControlDot[0].Cursor = Cursors.SizeNWSE;
            ShapeControlDot[1].Cursor = Cursors.SizeNS;
            ShapeControlDot[2].Cursor = Cursors.SizeNESW;
            ShapeControlDot[3].Cursor = Cursors.SizeWE;
            ShapeControlDot[4].Cursor = Cursors.SizeWE;
            ShapeControlDot[5].Cursor = Cursors.SizeNESW;
            ShapeControlDot[6].Cursor = Cursors.SizeNS;
            ShapeControlDot[7].Cursor = Cursors.SizeNWSE;

        }

       

        void ShowShapeControlDot(double x1, double y1, double x2, double y2)
        {
            Canvas.SetLeft(ShapeControlDot[0], (x1 < x2 ? x1 : x2) - 2);
            Canvas.SetTop(ShapeControlDot[0], (y1 < y2 ? y1 : y2) - 2);

            Canvas.SetLeft(ShapeControlDot[1], ((x1 < x2 ? x1 : x2) + (((x1 > x2 ? x1 : x2) - (x1 < x2 ? x1 : x2)) / 2)) - 2);
            Canvas.SetTop(ShapeControlDot[1], (y1 < y2 ? y1 : y2) - 2);

            Canvas.SetLeft(ShapeControlDot[2], (x1 > x2 ? x1 : x2) - 2);
            Canvas.SetTop(ShapeControlDot[2], (y1 < y2 ? y1 : y2) - 2);

            Canvas.SetLeft(ShapeControlDot[3], (x1 < x2 ? x1 : x2) - 2);
            Canvas.SetTop(ShapeControlDot[3], ((y1 < y2 ? y1 : y2) + (((y1 > y2 ? y1 : y2) - (y1 < y2 ? y1 : y2)) / 2)) - 2);

            Canvas.SetLeft(ShapeControlDot[4], (x1 > x2 ? x1 : x2) - 2);
            Canvas.SetTop(ShapeControlDot[4], ((y1 < y2 ? y1 : y2) + (((y1 > y2 ? y1 : y2) - (y1 < y2 ? y1 : y2)) / 2)) - 2);

            Canvas.SetLeft(ShapeControlDot[5], (x1 < x2 ? x1 : x2) - 2);
            Canvas.SetTop(ShapeControlDot[5], (y1 > y2 ? y1 : y2) - 2);

            Canvas.SetLeft(ShapeControlDot[6], ((x1 < x2 ? x1 : x2) + (((x1 > x2 ? x1 : x2) - (x1 < x2 ? x1 : x2)) / 2)) - 2);
            Canvas.SetTop(ShapeControlDot[6], (y1 > y2 ? y1 : y2) - 2);

            Canvas.SetLeft(ShapeControlDot[7], (x1 > x2 ? x1 : x2) - 2);
            Canvas.SetTop(ShapeControlDot[7], (y1 > y2 ? y1 : y2) - 2);

            for(int i = 0; i < 8; i++)
            {
                ShapeControlDot[i].Visibility = Visibility.Visible;
            }
            controlDotMousePoint1 = new Point(x1, y1);
            controlDotMousePoint2 = new Point(x2, y2);
        }

        private void LineButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            shapeKind = 1;
        }
        private void RectangleButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            shapeKind = 2;
        }
       
      

        string selectID;
        private void ShapeControlDot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            selectID = (sender as DependencyObject).GetValue(NameProperty) as string;

            onClickContorlDot = true;

        }

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!onClickShape)
            {
                onClickDrawShape = true;
                mousePoint1 = GetMousePosition();
                if (!onClickContorlDot) MainCanvas.Children.Add(DrawShape(mousePoint1.X, mousePoint1.Y, mousePoint1.X, mousePoint1.Y));
            }
            if (!onClickShape && !onClickContorlDot)
            {
                onSelectShape = false;
            }
            if(onClickShape)
            {
                switch (ingShape.ToString().Split('.')[ingShape.ToString().Split('.').Length - 1])
                {
                    case "Line":
                        MoveStartPoint1 = new Point(((Line)ingShape).X1, ((Line)ingShape).Y1);
                        MoveStartPoint2 = new Point(((Line)ingShape).X2, ((Line)ingShape).Y2);
                        break;
                    case "Rectangle":
                        MoveStartPoint1 = new Point(Canvas.GetLeft(((Rectangle)ingShape)), Canvas.GetTop(((Rectangle)ingShape)));
                        MoveStartPoint2 = new Point(MoveStartPoint1.X + ((Rectangle)ingShape).Width, MoveStartPoint1.Y + ((Rectangle)ingShape).Height);
                        break;
                   
                    default:
                        MoveStartPoint1 = new Point(0, 0);
                        MoveStartPoint2 = new Point(0, 0);
                        break;
                }
            }
            if(onClickContorlDot)
            {
                resizeStartLocation1 = controlDotMousePoint1;
                resizeStartLocation2 = controlDotMousePoint2;
            }
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if(onClickContorlDot)
            {
                switch (selectID)
                {
                    case "ID_1":
                        controlDotMousePoint1 = GetMousePosition();
                        break;
                    case "ID_2":
                        controlDotMousePoint1.Y = GetMousePosition().Y;
                        break;
                    case "ID_3":
                        controlDotMousePoint2 = GetMousePosition();
                        break;
                    case "ID_4":
                        controlDotMousePoint1.X = GetMousePosition().X;
                        break;
                    case "ID_5":
                        controlDotMousePoint2.X = GetMousePosition().X;
                        break;
                    case "ID_6":
                        controlDotMousePoint2 = GetMousePosition();
                        break;
                    case "ID_7":
                        controlDotMousePoint2.Y = GetMousePosition().Y;
                        break;
                    case "ID_8":
                        controlDotMousePoint2 = GetMousePosition();
                        break;
                }
                if (shapeKind != 1)
                {
                    ShowShapeControlDot(controlDotMousePoint1.X, controlDotMousePoint1.Y, controlDotMousePoint2.X, controlDotMousePoint2.Y);
                    SetSizeShape(ingShape, controlDotMousePoint1.X, controlDotMousePoint1.Y, controlDotMousePoint2.X, controlDotMousePoint2.Y);
                }
                else
                {
                    if(lineKinds == 1){
                        ShowShapeControlDot(controlDotMousePoint1.X, controlDotMousePoint1.Y, controlDotMousePoint2.X, controlDotMousePoint2.Y);
                        SetSizeShape(ingShape, (controlDotMousePoint1.X < controlDotMousePoint2.X ? controlDotMousePoint1.X : controlDotMousePoint2.X), (controlDotMousePoint1.Y < controlDotMousePoint2.Y ? controlDotMousePoint1.Y : controlDotMousePoint2.Y), (controlDotMousePoint1.X > controlDotMousePoint2.X ? controlDotMousePoint1.X : controlDotMousePoint2.X), (controlDotMousePoint1.Y > controlDotMousePoint2.Y ? controlDotMousePoint1.Y : controlDotMousePoint2.Y));
                    }
                    else
                    {
                        ShowShapeControlDot(controlDotMousePoint1.X, controlDotMousePoint1.Y, controlDotMousePoint2.X, controlDotMousePoint2.Y);
                        SetSizeShape(ingShape, (controlDotMousePoint1.X < controlDotMousePoint2.X ? controlDotMousePoint1.X : controlDotMousePoint2.X), (controlDotMousePoint1.Y > controlDotMousePoint2.Y ? controlDotMousePoint1.Y : controlDotMousePoint2.Y), (controlDotMousePoint1.X > controlDotMousePoint2.X ? controlDotMousePoint1.X : controlDotMousePoint2.X), (controlDotMousePoint1.Y < controlDotMousePoint2.Y ? controlDotMousePoint1.Y : controlDotMousePoint2.Y));
                    }
                }
            }
            else if(onClickShape)
            {
                MoveShape(ingShape);
            }
            else if (onClickDrawShape)
            {
                mousePoint2 = GetMousePosition();

                switch (shapeKind)
                {
                    case 1:
                        drawLine.X1 = mousePoint1.X;
                        drawLine.Y1 = mousePoint1.Y;
                        drawLine.X2 = mousePoint2.X;
                        drawLine.Y2 = mousePoint2.Y;
                        break;
                    case 2:
                        drawRectangle.Width = (mousePoint1.X > mousePoint2.X ? mousePoint1.X : mousePoint2.X) - (mousePoint1.X < mousePoint2.X ? mousePoint1.X : mousePoint2.X);
                        drawRectangle.Height = (mousePoint1.Y > mousePoint2.Y ? mousePoint1.Y : mousePoint2.Y) - (mousePoint1.Y < mousePoint2.Y ? mousePoint1.Y : mousePoint2.Y);
                        Canvas.SetLeft(drawRectangle, (mousePoint1.X < mousePoint2.X ? mousePoint1.X : mousePoint2.X));
                        Canvas.SetTop(drawRectangle, (mousePoint1.Y < mousePoint2.Y ? mousePoint1.Y : mousePoint2.Y));
                        break;
                   
   
                }
            }
        }
        
        private void Button_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mousePoint2 = GetMousePosition();

            if (onClickContorlDot == false && onClickShape == false && onClickDrawShape == true)
            {
                if (shapeKind == 1)
                {
                    if ((mousePoint1.X <= mousePoint2.X && mousePoint1.Y <= mousePoint2.Y) || (mousePoint1.X >= mousePoint2.X && mousePoint1.Y >= mousePoint2.Y))
                    {
                        lineKinds = 1;
                    }
                    else
                    {
                        lineKinds = 2;
                    }
                }
                controlDotMousePoint1 = new Point(mousePoint1.X < mousePoint2.X ? mousePoint1.X : mousePoint2.X, mousePoint1.Y < mousePoint2.Y ? mousePoint1.Y : mousePoint2.Y);
                controlDotMousePoint2 = new Point(mousePoint1.X > mousePoint2.X ? mousePoint1.X : mousePoint2.X, mousePoint1.Y > mousePoint2.Y ? mousePoint1.Y : mousePoint2.Y);
                ShowShapeControlDot(controlDotMousePoint1.X, controlDotMousePoint1.Y, controlDotMousePoint2.X, controlDotMousePoint2.Y);
                onSelectShape = true;

                ingShape = SelectShape();
                InputStack("Make", controlDotMousePoint1, controlDotMousePoint2);
            }
            else if(onClickContorlDot)
            {
                InputStack("Resize", resizeStartLocation1, resizeStartLocation2);
               
            }
            else if(onClickShape)
            {
                InputStack("Move", MoveStartPoint1, MoveStartPoint2);
            }
            onClickDrawShape = false;
            onClickContorlDot = false;
            onClickShape = false;
        }

        private Shape SelectShape()
        {
            switch(shapeKind)
            {
                case 1:
                    return drawLine;
                case 2:
                    return drawRectangle;
                
            }
            return drawLine;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Z))
            {
                if(ShapeStackCount >= 0)
                {
                    switch (ShapeStack[ShapeStackCount].type)
                    {
                        case "Make":
                            ShapeStack[ShapeStackCount].shape.Visibility = Visibility.Collapsed;
                            break;
                        case "Move":
                            ingShape = ShapeStack[ShapeStackCount].shape;
                            break;
                        case "Resize":
                            SetSizeShape(ShapeStack[ShapeStackCount].shape, ShapeStack[ShapeStackCount].location1.X, ShapeStack[ShapeStackCount].location1.Y, ShapeStack[ShapeStackCount].location2.X, ShapeStack[ShapeStackCount].location2.Y);
                            ShowShapeControlDot(ShapeStack[ShapeStackCount].location1.X, ShapeStack[ShapeStackCount].location1.Y, ShapeStack[ShapeStackCount].location2.X, ShapeStack[ShapeStackCount].location2.Y);
                            break;
                        case "LineColorChange":
                            ShapeStack[ShapeStackCount].shape.Stroke = ShapeStack[ShapeStackCount].lineColor;
                            break;
                        case "FillColorChange":
                            ShapeStack[ShapeStackCount].shape.Fill = ShapeStack[ShapeStackCount].fillColor;
                            break;
                    }
                    ShapeStackCount--;
                }
            }
            if(Keyboard.IsKeyDown(Key.Delete))
            {
                if(onSelectShape)
                {
                    ingShape.Visibility = Visibility.Collapsed;
                    onClickContorlDot = false;
                    onClickDrawShape = false;
                    onClickShape = false;
                }
                InputStack("Delete", controlDotMousePoint1, controlDotMousePoint2);
            }
        }
        //change line color and fill color
        private void InputStack(string type, Point location1, Point location2)
        {
            try
            {
                DrawStack.Data stack = new DrawStack.Data();
                stack.shape = ingShape;
                stack.type = type;
                stack.location1 = location1;
                stack.location2 = location2;
                stack.fillColor = ingShape.Fill;
                stack.lineColor = ingShape.Stroke;
                stack.lineStroke = ingShape.StrokeThickness;
                for (int i = ShapeStackCount + 1; i <= ShapeStackCountMax; i++)
                {
                    ShapeStack.RemoveAt(ShapeStackCount + 1);
                }
                ShapeStack.Add(stack);
                ShapeStackCount++;
                ShapeStackCountMax = ShapeStackCount;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        private void LineColorPicker_Changed(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            lineColor = new SolidColorBrush(e.NewValue.Value);
            if(onSelectShape)
            {
                InputStack("LineColorChange", controlDotMousePoint1, controlDotMousePoint2);
                ingShape.Stroke = lineColor;
            }
        }
        private void FillColorPicker_Changed(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            fillColor = new SolidColorBrush(e.NewValue.Value);
            if (onSelectShape)
            {
                InputStack("FillColorChange", controlDotMousePoint1, controlDotMousePoint2);
                ingShape.Fill = fillColor;
            }
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            background.Height = e.NewSize.Height - 88;
            background.Width = e.NewSize.Width - 15;
        }

        

        
    }
}
