using GraphEditorWPF.EventArguments.NodeElementArguments;
using GraphEditorWPF.Models.EdgeModels;
using GraphEditorWPF.Models.NodeModels;
using GraphEditorWPF.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace GraphEditorWPF.Models
{
    public class NodeElement : CanvasElement
    {
        private Node _node = new Node();
        private ObservableCollection<EdgeElement> _linkedEdges = new ObservableCollection<EdgeElement>();

        public event EventHandler<NodeElementArgs> Moved;
        public event EventHandler<NodeElementArgs> Added;
        public event EventHandler<NodeElementArgs> Removed;
        public event EventHandler<NodeElementArgs> Clicked;
        public event EventHandler<NodeElementArgs> Pressed;
        public event EventHandler<NodeElementArgs> Released;

        public NodeElement(CanvasArea canvas, string key = null)
        {
            _canvas = canvas;

            _node.Key = key;

            _label.LabelChanged += (object sender, string text) =>
            {
                _node.Label = text;
            };

            BindLinkedEdges();
        }

        /// <summary>
        /// Node object
        /// </summary>
        public Node Node {
            get { return _node; }
            set { _node = value; }
        }

        /// <summary>
        /// List of linked to the node edges
        /// </summary>
        public ObservableCollection<EdgeElement> LinkedEdges
        {
            get { return _linkedEdges; }
            set { _linkedEdges = value; BindLinkedEdges(); }
        }

        /// <summary>
        /// Return center coordinates of a node element
        /// </summary>
        public Coordinates Center
        {
            get { 
                if (_element == null)
                {
                    return new Coordinates(0, 0);
                }
                var x = Canvas.GetLeft(_element) + (_element.ActualSize.X / 2);
                var y = Canvas.GetTop(_element) + (_element.ActualSize.Y / 2);

                return new Coordinates(x, y); 
            }
        }

        /// <summary>
        /// Returns label position of a node element
        /// </summary>
        public Coordinates LabelPosition
        {
            get { 
                if (_element == null)
                {
                    return new Coordinates();
                }
                var x = Canvas.GetLeft(_element) + (_element.ActualSize.X / 2);
                var y = Canvas.GetTop(_element) + (_element.ActualSize.Y / 2);

                return new Coordinates(x, y); 
            }
        }

        /// <summary>
        /// Moves element to given coordinates
        /// </summary>
        /// <param name="coords"></param>
        public void MoveTo(Coordinates coords)
        {
            MoveTo(coords.X, coords.Y);
        }

        /// <summary>
        /// Moves element to given coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MoveTo(double x, double y)
        {
            Canvas.SetTop(_element, y);
            Canvas.SetLeft(_element, x);

            _node.Params.Position = Center;

            if (Moved != null)
            {
                NodeElementArgs args = new NodeElementArgs
                {
                    Source = _element,
                    Position = new Coordinates(x, y),
                };
                Moved(this, args);
            }
        }

        /// <summary>
        /// Moves element by vector
        /// </summary>
        /// <param name="vector"></param>
        public new void MoveBy(Vector2 vector)
        {
            var top = Canvas.GetTop(_element);
            var left = Canvas.GetLeft(_element);

            MoveTo(new Coordinates(left + vector.X, top + vector.Y));
        }

        /// <summary>
        /// Adds element to given coordinates
        /// </summary>
        /// <param name="coords"></param>
        public void Add(Coordinates coords)
        {
            if (_element == null)
            {
                _element = CreateElement();
            }

            _canvas.Canvas.Children.Add(_element);
            _canvas.Children.Add(this);

            Canvas.SetTop(_element, coords.Y - (_element.ActualSize.Y / 2));
            Canvas.SetLeft(_element, coords.X - (_element.ActualSize.X / 2));

            _node.Params.Position = Center;

            var nodeElements = _canvas.Children.Where((elem) => elem.GetType() == typeof(NodeElement));
            _label.Text = nodeElements.Count().ToString();

            _label.LinkTo(this);

            _label.Element.PointerEntered += PointerEnters;
            _label.Element.PointerExited += PointerExits;

            SelectionChanged += SelectionChanges;

            if (Added != null)
            {
                NodeElementArgs args = new NodeElementArgs
                {
                    Source = _element,
                    Position = coords,
                };
                Added(this, args);
            }
        }

        /// <summary>
        /// Removes element
        /// </summary>
        public void Remove()
        {
            foreach (var element in _linkedEdges.ToList())
            {
                element?.Remove();
            }

            _canvas.Canvas.Children.Remove(_element);
            _canvas.Children.Remove(this);

            if (Removed != null)
            {
                NodeElementArgs args = new NodeElementArgs
                {
                    Source = _element,
                };
                Removed(this, args);
            }
        }

        /// <summary>
        /// Creates node element
        /// </summary>
        /// <returns>Ellipse as UIElement</returns>
        public UIElement CreateElement()
        {
            Ellipse ellipse = new Ellipse
            {
                Name = _node.Key,
                Width = 16,
                Height = 16,
                // Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 200, 255)),
                Fill = (Brush) Application.Current.Resources["NodeColor"],
                StrokeThickness = 1,
                Stroke = (Brush) Application.Current.Resources["NodeBorderColor"],
            };

            Canvas.SetZIndex(ellipse, 10);

            //Point pos = new Point(Canvas.GetLeft(ellipse), Canvas.GetTop(ellipse));
            //Size size = new Size(ellipse.ActualWidth, ellipse.ActualHeight);

            //var rect = new Rect(pos, size);

            ellipse.PointerEntered += PointerEnters;
            ellipse.PointerExited += PointerExits;

            ellipse.SizeChanged += (object sender, SizeChangedEventArgs e) =>
            {
                var rootVisual = ElementCompositionPreview.GetElementVisual(ellipse);
                rootVisual.CenterPoint = new Vector3(rootVisual.Size / 2, 0);
            };

            ellipse.Loaded += (object sender, RoutedEventArgs e) =>
            {
                var rootVisual = ElementCompositionPreview.GetElementVisual(ellipse);
                rootVisual.CenterPoint = new Vector3(rootVisual.Size / 2, 0);
            };

            return ellipse;
        }

        /// <summary>
        /// Sets spring animation
        /// </summary>
        /// <param name="rootVisual"></param>
        /// <param name="value"></param>
        /// <returns>Composition animation</returns>
        private CompositionAnimation SpringAnimation(Visual rootVisual, float value)
        {
            rootVisual.CenterPoint = new Vector3(rootVisual.Size / 2, 0);

            var animation = rootVisual.Compositor.CreateSpringVector3Animation();
            animation.FinalValue = new Vector3(value);

            return animation;
        }

        private void ScaleElement(object sender, bool scale = false)
        {
            var rootVisual = ElementCompositionPreview.GetElementVisual(_element ?? sender as UIElement);
            var scaleValue = 1.0f + (4 / rootVisual.Size.X);

            rootVisual.StartAnimation("Scale", SpringAnimation(rootVisual, scale ? scaleValue : 1.0f));
        }

        /// <summary>
        /// Plays animation on pointer enters the node
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PointerEnters(object sender, PointerRoutedEventArgs e)
        {
            ScaleElement(sender, true);
        }

        /// <summary>
        /// Plays animation on pointer exits the node
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PointerExits(object sender, PointerRoutedEventArgs e)
        {
            ScaleElement(sender, false);
        }

        /// <summary>
        /// When selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectionChanges(object sender, bool selection)
        {
            if (_element == null) return;

            var ellipse = (Ellipse) _element;

            if (selection)
            {
                _element.PointerEntered -= PointerEnters;
                _element.PointerExited -= PointerExits;

                ellipse.Fill = (Brush) Application.Current.Resources["NodeColorSelected"];
                ellipse.Stroke = (Brush) Application.Current.Resources["NodeBorderColorSelected"];

                _label.Element.PointerEntered -= PointerEnters;
                _label.Element.PointerExited -= PointerExits;

                ScaleElement(_element, true);
            } 
            else
            {
                _element.PointerEntered += PointerEnters;
                _element.PointerExited += PointerExits;

                ellipse.Fill = (Brush) Application.Current.Resources["NodeColor"];
                ellipse.Stroke = (Brush) Application.Current.Resources["NodeBorderColor"];

                _label.Element.PointerEntered += PointerEnters;
                _label.Element.PointerExited += PointerExits;

                ScaleElement(_element, false);
            }
        }

        private void BindLinkedEdges()
        {
            _linkedEdges.CollectionChanged += LinkedEdgesChanged;
        }

        private void LinkedEdgesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _node.Edges.Clear();
            
            foreach (var edge in _linkedEdges)
            {
                _node.Edges.Add(edge.Edge);
            }
        }
    }
}
