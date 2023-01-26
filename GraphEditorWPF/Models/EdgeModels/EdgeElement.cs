using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using GraphEditorWPF.Types;
using GraphEditorWPF.Models.NodeModels;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using Windows.UI;
using GraphEditorWPF.EventArguments.NodeElementArguments;
using System.Xml.Linq;
using System.Numerics;
using Windows.Foundation;

namespace GraphEditorWPF.Models.EdgeModels
{
    public class EdgeElement : CanvasElement
    {
        private Edge _edge = new Edge();
        private EdgeType _type = EdgeType.Directional;
        private NodeElement _fromNode;
        private NodeElement _toNode;

        public EdgeElement(Edge edge = null, NodeElement fromNode = null, NodeElement toNode = null)
        {
            _edge = edge ?? new Edge();
            _fromNode = fromNode;
            _toNode = toNode;
        }

        public EdgeElement(CanvasArea canvas)
        {
            _canvas = canvas;
        }

        public event EventHandler<EdgeElementArgs> Moved;
        public event EventHandler<EdgeElementArgs> Added;
        public event EventHandler<EdgeElementArgs> Removed;
        public event EventHandler<EdgeElementArgs> Reversed;
        public event EventHandler<EdgeElementArgs> Clicked;
        public event EventHandler<EdgeElementArgs> Pressed;
        public event EventHandler<EdgeElementArgs> Released;

        public Edge Edge
        {
            get { return _edge; }
            set { _edge = value; }
        }

        public EdgeType Type
        {
            get { return _type; }
            set { 
                _type = value;
                if (_fromNode == null || _toNode == null) return;
                RedrawArrow(_fromNode.Center, _toNode.Center);
            }
        }

        public NodeElement From
        {
            get { return _fromNode; }
            set 
            { 
                _fromNode = value;
                _edge.StartNode = _fromNode?.Node;
            }
        }

        public NodeElement To
        {
            get { return _toNode; }
            set 
            { 
                _toNode = value;
                _edge.EndNode = _toNode?.Node;
            }
        }

        /// <summary>
        /// Links start position of edge to a node
        /// </summary>
        /// <param name="element"></param>
        public void LinkStartTo(NodeElement element)
        {
            if (From != null)
            {
                From.Moved -= StartMoved;
            }
            From = element;

            if (!From.LinkedEdges.Contains(this))
            {
                From.LinkedEdges.Add(this);
            }

            From.Moved += StartMoved;
        }

        /// <summary>
        /// Links end position of edge to a node
        /// </summary>
        /// <param name="element"></param>
        public void LinkEndTo(NodeElement element)
        {
            if (To != null)
            {
                To.Moved -= EndMoved;
            }
            To = element;

            if (!To.LinkedEdges.Contains(this) && _type == EdgeType.Static)
            {
                To.LinkedEdges.Add(this);
            }

            To.Moved += EndMoved;
        }

        private void UpdateEdgePosition()
        {
            if (From != null)
            {
                Edge.Params.From = From.Center;
            }
            if (To != null)
            {
                Edge.Params.To = To.Center;
            }
        }

        /// <summary>
        /// Moves edge by vector
        /// </summary>
        /// <param name="vector"></param>
        public new void MoveBy(Vector2 vector)
        {
            Line element = (Line) _element;

            element.X1 += vector.X;
            element.Y1 += vector.Y;

            element.X2 += vector.X;
            element.Y2 += vector.Y;

            UpdateEdgePosition();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Draw()
        {
            _canvas.Canvas.Children.Add(_element);
            string output = JsonConvert.SerializeObject(_edge);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StartMoved(object sender, NodeElementArgs e)
        {
            MoveStartTo((NodeElement) sender);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void EndMoved(object sender, NodeElementArgs e)
        {
            MoveEndTo((NodeElement) sender);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public void MoveStartTo(NodeElement node)
        {
            MoveStartTo(node.Center);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coords"></param>
        public void MoveStartTo(Coordinates coords)
        {
            if (_toNode == null) return;

            RedrawArrow(coords, _toNode.Center);

            if (Moved != null)
            {
                Moved(this, new EdgeElementArgs
                {
                    Source = _toNode.Element,
                    Properties = null,
                    Position = _toNode.Center
                });
            }

            //Line element = (Line) _element;
            //element.X1 = coords.X;
            //element.Y1 = coords.Y;
        }

        /// <summary>
        /// Moves end position to given coordinates
        /// </summary>
        /// <param name="node"></param>
        public void MoveEndTo(NodeElement node)
        {
            MoveEndTo(node.Center);
        }

        /// <summary>
        /// Moves end position to given coordinates
        /// </summary>
        /// <param name="coords"></param>
        public void MoveEndTo(Coordinates coords)
        {
            if (_fromNode == null) return;

            RedrawArrow(_fromNode.Center, coords);

            if (Moved != null)
            {
                Moved(this, new EdgeElementArgs
                {
                    Source = _fromNode.Element,
                    Properties = null,
                    Position = _fromNode.Center
                });
            }
        }

        /// <summary>
        /// Adds edge
        /// </summary>
        /// <param name="from"></param>
        public void Add(NodeElement from)
        {
            Add(from, from);
            To.Moved -= EndMoved;
            To.LinkedEdges.Remove(this);
            To = null;
        }

        /// <summary>
        /// Adds edge
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void Add(NodeElement from, NodeElement to)
        {
            if (_element == null)
            {
                _element = CreateElement();
            }

            From = from;
            To = to;

            if (!_canvas.Canvas.Children.Contains(_element))
            {
                _canvas.Canvas.Children.Add(_element);
                _canvas.Children.Add(this);

                From.LinkedEdges.Add(this);
                To.LinkedEdges.Add(this);
            }

            RedrawArrow(from.Center, to.Center);

            From.Moved += StartMoved;
            To.Moved += EndMoved;

            if (From != To)
            {
                _label.Text = _edge.Weight.ToString();
                _label.LinkTo(this);
            }
        }

        /// <summary>
        /// Removes edge
        /// </summary>
        public void Remove()
        {
            _canvas.Canvas.Children.Remove(_element);
            _canvas.Children.Remove(this);

            if (From != null)
            {
                From.Moved -= StartMoved;
                From.LinkedEdges.Remove(this);
            }
            if (To != null)
            {
                To.Moved -= EndMoved;
                To.LinkedEdges.Remove(this);
            }            

            if (Removed != null)
            {
                Removed(this, new EdgeElementArgs
                {
                    Source = Element,
                    Properties = null,
                    Position = null,
                });
            }
        }

        /// <summary>
        /// Reverse direction of the edge
        /// </summary>
        public void Reverse()
        {
            _canvas.Canvas.Children.Remove(_element);
            _canvas.Children.Remove(this);

            From.Moved -= StartMoved;
            To.Moved -= EndMoved;

            From.LinkedEdges.Remove(this);
            To.LinkedEdges.Remove(this);

            if (_element == null)
            {
                _element = CreateElement();
            }

            var from = From;

            From = To;
            To = from;

            if (!_canvas.Canvas.Children.Contains(_element))
            {
                _canvas.Canvas.Children.Add(_element);
                _canvas.Children.Add(this);

                From.LinkedEdges.Add(this);

                if (_type == EdgeType.Static)
                    To.LinkedEdges.Add(this);
            }

            RedrawArrow(_fromNode.Center, _toNode.Center);

            From.Moved += StartMoved;
            To.Moved += EndMoved;

            if (Reversed != null)
            {
                Reversed(this, new EdgeElementArgs
                {
                    Source = Element,
                    Properties = null,
                    Position = null,
                });
            }
        }

        /// <summary>
        /// Checks if same edge is exists
        /// </summary>
        /// <returns>true if exists and false if does not</returns>
        private bool IsDublicate()
        {
            var collection = _canvas.Children.Where((elem) => {
                if (elem.GetType() != typeof(EdgeElement))
                    return false;

                EdgeElement edge = (EdgeElement)elem;
                return edge.From.Equals(_fromNode) && edge.To.Equals(_toNode);
            });

            return collection.Count() > 0;
        }

        /// <summary>
        /// Creates edge element
        /// </summary>
        /// <returns>Path as UIElement</returns>
        private UIElement CreateElement()
        {
            return DrawArrow(new Point(), new Point());
        }

        /// <summary>
        /// Draws arrow in given points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns>Path as UIElement</returns>
        private UIElement DrawArrow(Point p1, Point p2)
        {
            Path path = new Path();
            path.Data = GetArrow(p1, p2);
            path.StrokeThickness = 2;
            path.Stroke = path.Fill = (Brush) Application.Current.Resources["EdgeColor"];

            return path;
        }

        /// <summary>
        /// Redraws arrow
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        private void RedrawArrow(Coordinates p1, Coordinates p2)
        {
            RedrawArrow(new Point(p1.X, p1.Y), new Point(p2.X, p2.Y));
        }

        /// <summary>
        /// Redraws arrow
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        private void RedrawArrow(Point p1, Point p2)
        {
            if (_element == null) return;

            (_element as Path).Data = GetArrow(p1, p2);

            UpdateEdgePosition();
        }

        /// <summary>
        /// Calculates arrow position and rotation
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns>PathGeometry</returns>
        private PathGeometry GetArrowGeomentry(Point p1, Point p2)
        {
            double theta = Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)) * 180 / Math.PI;
            double distance = Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow((p2.Y - p1.Y), 2));
            double radius = 2;
            double arrowLength = 4;
            double arrowWidth = 2;

            if (_toNode != null)
            {
                radius += _toNode.Element.ActualSize.Y / 2;
            }

            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();

            double percent = (radius + distance) / distance;

            Point p = new Point(p1.X + ((p2.X - p1.X) / percent), p1.Y + ((p2.Y - p1.Y) / percent));
            pathFigure.StartPoint = p;

            Point lpoint = new Point(p.X + arrowWidth, p.Y + arrowLength);
            Point rpoint = new Point(p.X - arrowWidth, p.Y + arrowLength);

            LineSegment seg1 = new LineSegment();
            seg1.Point = lpoint;
            pathFigure.Segments.Add(seg1);

            LineSegment seg2 = new LineSegment();
            seg2.Point = rpoint;
            pathFigure.Segments.Add(seg2);

            LineSegment seg3 = new LineSegment();
            seg3.Point = p;
            pathFigure.Segments.Add(seg3);

            pathGeometry.Figures.Add(pathFigure);
            RotateTransform transform = new RotateTransform();
            transform.Angle = theta + 90;
            transform.CenterX = p.X;
            transform.CenterY = p.Y;
            pathGeometry.Transform = transform;

            return pathGeometry;
        }

        /// <summary>
        /// Returns arrows with line
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns>GeometryGroup</returns>
        private GeometryGroup GetArrow(Point p1, Point p2)
        {
            GeometryGroup lineGroup = new GeometryGroup();

            if (_type == EdgeType.Directional || _type == EdgeType.Bidirectional)
            {
                var pathGeometry = GetArrowGeomentry(p1, p2);
                lineGroup.Children.Add(pathGeometry);
            }
            if (_type == EdgeType.Bidirectional)
            {
                var pathGeometry = GetArrowGeomentry(p2, p1);
                lineGroup.Children.Add(pathGeometry);
            }

            LineGeometry connectorGeometry = new LineGeometry();
            connectorGeometry.StartPoint = p1;
            connectorGeometry.EndPoint = p2;
            lineGroup.Children.Add(connectorGeometry);
            return lineGroup;
        }

        public Coordinates LabelPosition
        {
            get
            {
                var p1 = _fromNode.Center;
                var p2 = _toNode?.Center ?? _fromNode.Center;

                return new Coordinates((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);

                double theta = Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)) * 180 / Math.PI;
                double distance = Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow((p2.Y - p1.Y), 2));
            }
        }
    }
}
