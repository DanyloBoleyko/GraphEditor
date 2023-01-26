using GraphEditorWPF.EventArguments.NodeElementArguments;
using GraphEditorWPF.Models.EdgeModels;
using GraphEditorWPF.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace GraphEditorWPF.Models
{
    public class Label
    {
        private TextBlock _element;
        private CanvasElement _linkedElement;

        public event EventHandler<string> LabelChanged;

        public Label(string text = "")
        {
            _element = new TextBlock
            {
                Text = text,
                FontSize = 6,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.White),
                TextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center,
            };
            Init();
        }

        public TextBlock Element
        {
            get { return _element; }
            set { _element = value; Init(); }
        }

        public CanvasElement LinkedElement
        {
            get { return _linkedElement; }
        }

        public string Text
        {
            get { return _element.Text; }
            set 
            { 
                _element.Text = value;

                if (LabelChanged != null)
                {
                    LabelChanged(this, _element.Text);
                }
            }
        }

        public void MoveTo(Coordinates coords)
        {
            if (_element == null) return;

            Canvas.SetTop(_element, coords.Y - (Math.Max(_element.ActualSize.Y, 8) / 2));
            Canvas.SetLeft(_element, coords.X - (Math.Max(_element.ActualSize.X, 2) / 2));

            if (_linkedElement == null) return;

            Canvas.SetZIndex(_element, Canvas.GetZIndex(_linkedElement.Element));
        }

        public void LinkTo(NodeElement nodeElement)
        {
            _linkedElement = nodeElement;
            _element.Name = "Label_" + nodeElement.Node.Key;
            _element.FontWeight = FontWeights.Bold;
            _element.Foreground = (SolidColorBrush) Application.Current.Resources["NodeLabelColor"];

            var defaultSize = nodeElement.Element.ActualSize.X;
            var textPadding = 8;

            _element.SizeChanged += (object sender, SizeChangedEventArgs e) =>
            {
                var ellipse = (Ellipse) nodeElement.Element;
                
                if (nodeElement.Element.ActualSize.X <= _element.ActualSize.X + textPadding)
                {
                    ellipse.Width = _element.ActualSize.X + textPadding;
                    ellipse.Height = _element.ActualSize.X + textPadding;
                } 
                else
                {
                    ellipse.Width = defaultSize;
                    ellipse.Height = defaultSize;
                }

                MoveTo(nodeElement.LabelPosition);
            };

            _element.Loaded += (object sender, RoutedEventArgs e) =>
            {
                _element = sender as TextBlock;
                MoveTo(nodeElement.LabelPosition);
            };

            nodeElement.Canvas.Children.Add(_element);

            nodeElement.Added += (object sender, NodeElementArgs e) =>
            {
                MoveTo(nodeElement.LabelPosition);
            };

            nodeElement.Moved += (object sender, NodeElementArgs e) =>
            {
                MoveTo(nodeElement.LabelPosition);
            };

            nodeElement.Removed += (object sender, NodeElementArgs e) =>
            {
                nodeElement.Canvas.Children.Remove(_element);
            };
        }

        public void LinkTo(EdgeElement edgeElement)
        {
            _linkedElement = edgeElement;
            _element.Name = "Label_" + edgeElement.Edge.Key;
            _element.FontWeight = FontWeights.Bold;
            _element.Foreground = (SolidColorBrush)Application.Current.Resources["EdgeLabelColor"];

            _element.SizeChanged += (object sender, SizeChangedEventArgs e) =>
            {
                MoveTo(edgeElement.LabelPosition);
            };

            _element.Loaded += (object sender, RoutedEventArgs e) =>
            {
                _element = sender as TextBlock;
                MoveTo(edgeElement.LabelPosition);
            };

            edgeElement.Canvas.Children.Add(_element);

            edgeElement.Edge.WeightChanged += (object s, double weight) =>
            {
                _element.Text = weight.ToString();
            };

            edgeElement.Moved += (object sender, EdgeElementArgs e) =>
            {
                MoveTo(edgeElement.LabelPosition);
            };

            edgeElement.Removed += (object sender, EdgeElementArgs e) =>
            {
                edgeElement.Canvas.Children.Remove(_element);
            };
        }

        private void Init()
        {
            if (LabelChanged != null)
            {
                LabelChanged(this, _element.Text);
            }
            _element.RegisterPropertyChangedCallback(TextBlock.TextProperty, LabelTextChanged);
        }

        private void LabelTextChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (dp == TextBlock.TextProperty)
            {
                if (LabelChanged != null)
                {
                    LabelChanged(this, ((TextBlock)sender).Text);
                }
            }
        }
    }
}
