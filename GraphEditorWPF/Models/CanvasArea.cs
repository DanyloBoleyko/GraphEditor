using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using GraphEditorWPF.Types;
using GraphEditorWPF.Models.NodeModels;
using GraphEditorWPF.EventArguments;
using GraphEditorWPF.Utils;
using GraphEditorWPF.Models.EdgeModels;
using System.Drawing;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using Rectangle = Windows.UI.Xaml.Shapes.Rectangle;
using Newtonsoft.Json.Linq;
using Windows.Foundation;
using Point = Windows.Foundation.Point;

namespace GraphEditorWPF.Models
{
    // click event arguments
    public class PointerClickEventArgs : EventArgs
    {
        public PointerRoutedEventArgs Args;
        public bool IsLeftButtonClicked;
        public bool IsRightButtonClicked;
        public bool IsMiddleButtonClicked;
    }

    // line draw arguments
    public class LineArgs : EventArgs
    {
        public Coordinates From;
        public Coordinates To;
        public Line Element;
    }

    public class CanvasArea
    {
        private Canvas _canvas;
        private ScaleTransform _scale;
        private TranslateTransform _translate;
        private Coordinates _offset = new Coordinates();
        private double _X = 0;
        private double _Y = 0;
        private Range _zoomRange = new Range(1.0, 20.0);
        private double _zoom = 1.0;
        private Coordinates _dragStartPosition = new Coordinates();

        private List<CanvasElement> _elements = new List<CanvasElement>();
        private List<CanvasElement> _selectedElements = new List<CanvasElement>();
        private List<UIElement> _dragElements = new List<UIElement>();
        private List<Rectangle> _gridLines = new List<Rectangle>();

        private bool _clickTracked = false;
        private bool _dragStarted = false;
        private CanvasElement _capturedLine = null;
        private Rectangle _selectionZone = null;

        public event EventHandler<CanvasArgs> Clicked;
        public event EventHandler<CanvasArgs> Pressed;
        public event EventHandler<CanvasArgs> Released;
        public event EventHandler<CanvasArgs> Canceled;
        public event EventHandler<CanvasArgs> ClickCanceled;
        public event EventHandler<CanvasArgs> Translated;
        public event EventHandler<CanvasArgs> Moved;
        public event EventHandler<CanvasArgs> Zoomed;
        private CanvasArgs _args = null;

        /// <summary>
        /// Returns Canvas element
        /// </summary>
        public Canvas Canvas
        {
            get { return _canvas; }
            set { _canvas = value; }
        }

        public List<Rectangle> GridLines
        {
            get { return _gridLines; }
        }

        /// <summary>
        /// Returns min and max zoom values
        /// </summary>
        public Range ZoomRange
        {
            get { return _zoomRange; }
        }

        /// <summary>
        /// Returns zoom value
        /// </summary>
        public double Zoom
        {
            get { return _zoom; }
            set {
                _zoom = Math.Clamp(value, _zoomRange.Min, _zoomRange.Max);

                if (_scale == null) return;

                _scale.ScaleX = _zoom;
                _scale.ScaleY = _zoom;

                if (Zoomed != null)
                {
                    Zoomed(this, new CanvasArgs
                    {
                        Position = new Coordinates(_scale.CenterX, _scale.CenterY),
                        Source = _canvas
                    });
                }
            }
        }

        public void ZoomOriginCenter()
        {
            _scale.CenterX = _canvas.ActualWidth / 2;
            _scale.CenterY = _canvas.ActualHeight / 2;
        }

        /// <summary>
        /// List of CanvasElements
        /// </summary>
        public List<CanvasElement> Children
        {
            get { return _elements; }
            set { _elements = value; }
        }

        /// <summary>
        /// Indicates if elements on canvas can be moved by dragging
        /// </summary>
        public bool AllowElementMove { get; set; }

        public bool IsEmpty
        {
            get { return _elements.Count == 0; }
        }

        /// <summary>
        /// Creates CanvasArea object
        /// </summary>
        /// <param name="canvas"></param>
        public CanvasArea(Canvas canvas)
        {
            AllowElementMove = true;

            _scale = new ScaleTransform();
            _translate = new TranslateTransform();

            var transforms = new TransformGroup();
            transforms.Children.Add(_scale);
            transforms.Children.Add(_translate);

            _canvas = canvas;
            _canvas.PointerPressed += PointerPressed;
            _canvas.PointerMoved += PointerMoved;
            _canvas.PointerReleased += PointerReleased;
            _canvas.PointerCanceled += PointerCanceled;
            _canvas.PointerWheelChanged += WheelScrolled;

            _canvas.RenderTransform = transforms;

            DrawGrid();
        }

        /// <summary>
        /// Find CanvasElement which has such element as linked or as label
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Found element or null</returns>
        public CanvasElement Find(UIElement element)
        {
            IEnumerable<CanvasElement> canvasElements = _elements.Where((CanvasElement elem) => {
                return elem.Element == element || elem.Label.Element == element;
            });

            if (canvasElements.Count() > 0)
            {
                return canvasElements.First();
            }
            return null;
        }

        /// <summary>
        /// Find CanvasElement which has such element as linked or as label
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Found element or null</returns>
        public CanvasElement Find(string key)
        {
            IEnumerable<CanvasElement> canvasElements = _elements.Where((CanvasElement elem) => {
                if (elem.GetType() == typeof(NodeElement))
                {
                    return ((NodeElement)elem).Node.Key == key;
                }
                else if (elem.GetType() == typeof(EdgeElement))
                {
                    return ((EdgeElement)elem).Edge.Key == key;
                }
                return false;
            });

            if (canvasElements.Count() > 0)
            {
                return canvasElements.First();
            }
            return null;
        }

        /// <summary>
        /// Triggered when mouse wheel is scrolled on top of canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WheelScrolled(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(_canvas);

            // set zoom origin to mouse position
            _scale.CenterX = pointer.RawPosition.X;
            _scale.CenterY = pointer.RawPosition.Y;

            // zooming
            if (pointer.Properties.MouseWheelDelta > 0)
            {
                _zoom *= 1.1;
            }
            else
            {
                _zoom /= 1.1;
            }

            _zoom = Math.Clamp(_zoom, _zoomRange.Min, _zoomRange.Max);

            _scale.ScaleX = _zoom;
            _scale.ScaleY = _zoom;

            if (Zoomed != null)
            {
                Zoomed(this, new CanvasArgs
                {
                    Position = new Coordinates(_scale.CenterX, _scale.CenterY),
                    Source = _canvas
                });
            }
        }

        private UIElement GetPointerSource(object source)
        {
            var element = (UIElement)source;

            if (element.GetType() == typeof(Rectangle))
            {
                if (((Rectangle)element).Name.Contains("Grid"))
                    return _canvas;
            }

            return element;
        }

        /// <summary>
        /// Triggered when pointer is pressed on canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(_canvas);
            var source = e.OriginalSource;
            var properties = pointer.Properties;

            var labelSource = Find(source as UIElement);

            if (labelSource != null)
            {
                source = labelSource.Element;
            }
            
            _args = new CanvasArgs
            {
                Properties = new PointerProperties
                {
                    LeftButtonRised = properties.IsLeftButtonPressed,
                    RightButtonRised = properties.IsRightButtonPressed,
                    MiddleButtonRised = properties.IsMiddleButtonPressed,
                },
                Source = GetPointerSource(source),
                Position = (Coordinates) pointer.Position,
            };

            _X = pointer.RawPosition.X;
            _Y = pointer.RawPosition.Y;

            _clickTracked = true;

            _dragStarted = true;
            _dragStartPosition = (Coordinates) pointer.Position;

            _dragElements.Clear();

            var element = Find(source as UIElement);

            if (pointer.Properties.IsRightButtonPressed)
            {
                foreach (var child in _elements.Where((elem) => elem.GetType() == typeof(NodeElement)))
                {
                    _dragElements.Add(child.Element);
                }
            }
            else if (pointer.Properties.IsLeftButtonPressed && AllowElementMove)
            {
                if (element != null && element.GetType() == typeof(NodeElement))
                {
                    _dragElements.Add(element.Element);
                }
            }

            if (pointer.Properties.IsLeftButtonPressed)
            {
                SelectionChange(element);
            }

            //SelectionRectangleInit((Coordinates) pointer.Position);

            if (Pressed != null)
            {
                Pressed(this, _args);
            }
        }

        /// <summary>
        /// Triggered when pointer is moved on top of canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(_canvas);
            var properties = pointer.Properties;

            if (_dragStarted) 
            {
                double movedX = pointer.RawPosition.X - _X;
                double movedY = pointer.RawPosition.Y - _Y;

                foreach (var child in _dragElements)
                {
                    var canvasElement = Find(child);

                    if (canvasElement != null)
                    {
                        Vector2 moveVector = new Vector2((float) movedX, (float) movedY);

                        if (canvasElement.GetType() == typeof(NodeElement))
                        {
                            var nodeElement = (NodeElement) canvasElement;
                            nodeElement.MoveBy(moveVector);
                        }
                    }
                    else
                    {
                        MoveElement(child, movedX, movedY);
                    }
                }

                _clickTracked = false;

                if (ClickCanceled != null)
                {
                    ClickCanceled(this, _args);
                }
            }

            _X = pointer.RawPosition.X;
            _Y = pointer.RawPosition.Y;

            //SelectionUpdate((Coordinates) pointer.RawPosition);

            if (Moved != null)
            {
                Moved(this, new CanvasArgs
                {
                    Source = GetPointerSource(e.OriginalSource),
                    Properties = new PointerProperties
                    {
                        LeftButtonRised = properties.IsLeftButtonPressed,
                        RightButtonRised = properties.IsRightButtonPressed,
                        MiddleButtonRised = properties.IsMiddleButtonPressed,
                    },
                    Position = (Coordinates) pointer.RawPosition,
                });
            }
        }

        /// <summary>
        /// Triggered when pointer released on top of canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(_canvas);
            var element = Find(e.OriginalSource as UIElement);

            if (_clickTracked && Clicked != null && _args != null)
            {
                Clicked(this, new CanvasArgs
                {
                    Source = _args.Source,
                    Position = (Coordinates) pointer.Position,
                    Properties = _args.Properties
                });

                if (_args.Properties.RightButtonRised || _args.Properties.LeftButtonRised)
                {
                    SelectionChange(element);
                }
            }

            if (Released != null && _args != null)
            {
                if (element != null)
                {
                    _args.Source = element.Element;
                }
                _args.Source = GetPointerSource(e.OriginalSource);
                
                _args.Position = (Coordinates)pointer.Position;

                Released(this, _args);
            }

            _dragElements.Clear();

            if (_dragStarted && Translated != null)
            {
                Translated(this, new CanvasArgs
                {
                    PreviousPosition = _dragStartPosition,
                    Position = (Coordinates) pointer.Position,
                    Source = _args.Source,
                    Properties = _args.Properties,
                });
            }

            //SelectionEnd();

            _dragStarted = false;
            _clickTracked = false;
        }

        /// <summary>
        /// Triggered when pointer event canceled
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            if (Canceled != null)
            {
                Canceled(this, _args);
            }
            
            if (ClickCanceled != null)
            {
                ClickCanceled(this, _args);
            }
        }

        /// <summary>
        /// Moves element by coordinate values
        /// </summary>
        /// <param name="child"></param>
        /// <param name="movedX"></param>
        /// <param name="movedY"></param>
        private void MoveElement(UIElement child, double movedX, double movedY)
        {
            var top = Canvas.GetTop(child);
            var left = Canvas.GetLeft(child);

            Canvas.SetTop(child, top + movedY);
            Canvas.SetLeft(child, left + movedX);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawLineFrom(UIElement child, double x, double y)
        {
            _capturedLine = Find(child);
            _canvas.PointerMoved += DrawLineMove;
            _canvas.PointerReleased += DrawLineEnd;
            //_canvas.PointerCanceled += DrawLineEnd;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrawLineMove(object sender, PointerRoutedEventArgs e)
        {
            if (_capturedLine == null) return;

            var pointer = e.GetCurrentPoint(_canvas);

            (_capturedLine as EdgeElement).MoveEndTo((Coordinates) pointer.Position);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrawLineEnd(object sender, PointerRoutedEventArgs e)
        {
            var source = e.OriginalSource;
            if (source.GetType() == typeof(Ellipse) || source.GetType() == typeof(TextBlock))
            {
                var element = (UIElement) source;

                var found = Find(element);
                double x = 0;
                double y = 0;

                if (found != null)
                {
                    element = found.Element;

                    x = Canvas.GetLeft(element) + (element.ActualSize.X / 2);
                    y = Canvas.GetTop(element) + (element.ActualSize.Y / 2);

                    (_capturedLine as EdgeElement).MoveEndTo(new Coordinates(x, y));
                }

                if (Released != null) 
                {
                    _args.Position = new Coordinates(x, y);
                    _args.Source = (UIElement) source;

                    Released(this, _args);
                }
            }
            else
            {
                var index = _canvas.Children.IndexOf(_capturedLine.Element);

                if (index >= 0)
                    _canvas.Children.RemoveAt(index);
            }
            _capturedLine = null;
            _canvas.PointerMoved -= DrawLineMove;
            _canvas.PointerReleased -= DrawLineEnd;
            //_canvas.PointerCanceled -= DrawLineEnd;
        }

        public void DrawGrid()
        {
            for (var i = 0; i < 10; ++i)
            {
                var rect = new Rectangle();
                rect.Width = _canvas.Width;
                _gridLines.Add(rect);
            }

            //foreach (var elem in _gridLines)
            //{
            //    _canvas.Children.Remove(elem);
            //}
            //_gridLines.Clear();

            //var gridline = new Windows.UI.Xaml.Shapes.Rectangle();
            //gridline.Name = "Grid";
            //_gridLines.Add(gridline);
            //_canvas.Children.Add(gridline);

            //Canvas.SetZIndex(gridline, 2);

            //gridline.Loaded += (object sender, RoutedEventArgs e) =>
            //{
            //    gridline.Width = _canvas.ActualWidth;
            //    gridline.Height = _canvas.ActualHeight;

            //    Canvas.SetTop(gridline, 0);
            //    Canvas.SetLeft(gridline, 0);

            //    var brush = new LinearGradientBrush();
            //    for (var i = 0; i < 1; i++)
            //    {
            //        var stop = new GradientStop();
            //        stop.Offset = 0.0f;
            //        stop.Color = Colors.Transparent;
            //        brush.GradientStops.Add(stop);

            //        stop = new GradientStop();
            //        stop.Offset = 0.01f;
            //        stop.Color = ((SolidColorBrush)Application.Current.Resources["GridColor"]).Color;
            //        brush.GradientStops.Add(stop);

            //        stop = new GradientStop();
            //        stop.Offset = 0.13f;
            //        stop.Color = ((SolidColorBrush)Application.Current.Resources["GridColor"]).Color;
            //        brush.GradientStops.Add(stop);

            //        stop = new GradientStop();
            //        stop.Offset = 0.14f;
            //        stop.Color = Colors.Transparent;
            //        brush.GradientStops.Add(stop);
            //    }
            //    brush.StartPoint = new Point(0.0, 0);
            //    brush.EndPoint = new Point(10.0, 0);
            //    brush.SpreadMethod = GradientSpreadMethod.Repeat;
            //    brush.MappingMode = BrushMappingMode.Absolute;

            //    var transform = new CompositeTransform();
            //    transform.CenterX = 0.5;
            //    transform.CenterY = 0.5;

            //    brush.RelativeTransform = transform;
            //    gridline.Fill = brush;
            //};
        }

        private void SelectionChange(CanvasElement element)
        {
            var list = new List<CanvasElement>();
            list.Add(element);
            SelectionChange(list);
        }

        private void SelectionChange(List<CanvasElement> elements)
        {
            if (elements == null) return;

            foreach (var elem in _selectedElements)
            {
                elem.IsSelected = false;
            }

            _selectedElements.Clear();

            foreach (var elem in elements.Where(e => e != null && e?.GetType() == typeof(NodeElement)))
            {
                _selectedElements.Add(elem);
            }

            foreach (var elem in _selectedElements)
            {
                elem.IsSelected = true;
            }
        }

        private void SelectionRectangleInit(Coordinates from)
        {
            _selectionZone = new Rectangle
            {
                Width = 0,
                Height = 0,
                Fill = new SolidColorBrush(Colors.Transparent),
                Stroke = new SolidColorBrush(Colors.Aqua),
            };
            _canvas.Children.Add(_selectionZone);

            Canvas.SetLeft(_selectionZone, from.X);
            Canvas.SetTop(_selectionZone, from.Y);
        }

        private void SelectionUpdate(Coordinates to)
        {
            if (_selectionZone == null) return;

            var width = to.X - Canvas.GetLeft(_selectionZone);
            var height = to.Y - Canvas.GetTop(_selectionZone);

            if (width < 0)
            {
                Canvas.SetLeft(_selectionZone, Canvas.GetLeft(_selectionZone) + width);
            }

            if (height < 0)
            {
                Canvas.SetTop(_selectionZone, Canvas.GetTop(_selectionZone) + height);
            }

            _selectionZone.Width = Math.Abs(width);
            _selectionZone.Height = Math.Abs(height);
        }

        private void SelectionEnd()
        {
            _canvas.Children.Remove(_selectionZone);
            _selectionZone = null;
        }

        public void ClearAll()
        {
            _canvas.Children.Clear();
            _dragElements.Clear();
            _selectedElements.Clear();
            _elements.Clear();

            _scale.ScaleX = 1.0f;
            _scale.ScaleY = 1.0f;
        }
    }
}
