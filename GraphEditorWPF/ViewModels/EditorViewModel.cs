using GraphEditorWPF.Commands;
using GraphEditorWPF.EventArguments;
using GraphEditorWPF.EventArguments.NodeElementArguments;
using GraphEditorWPF.Models;
using GraphEditorWPF.Models.Context;
using GraphEditorWPF.Models.EdgeModels;
using GraphEditorWPF.Models.NodeModels;
using GraphEditorWPF.Types;
using GraphEditorWPF.ViewModels;
using GraphEditorWPF.ViewModels.Dialogs;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Devices.Enumeration;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Path = Windows.UI.Xaml.Shapes.Path;

namespace GraphEditorWPF.ViewModels
{
    public partial class EditorView : Page
    {
        private Graph _graph = new Graph();
        private CanvasArea _canvas;
        private ContextParams _contextParams;
        private CanvasElement _selectedElement;
        private HistoryController _historyController;
        private Mode _currentMode = Mode.Normal;
        private Ellipse _nodeShadow;

        private bool _addingEdge = false;

        public EditorView()
        {
            this.InitializeComponent();

            _canvas = new CanvasArea(MainCanvas);
            _canvas.Clicked += CanvasClicked;
            _canvas.Moved += CanvasMoved;
            _canvas.Translated += NodeMoved;
            _canvas.Zoomed += CanvasZoomed;

            _historyController = new HistoryController();
        }

        public bool SideBarOpened { get; set; }

        public double MaxZoom 
        { 
            get { return _canvas.ZoomRange.Max; } 
        }

        public double MinZoom 
        { 
            get { return _canvas.ZoomRange.Min; } 
        }

        public CanvasArea Area 
        { 
            get { return _canvas; } 
        }

        public HistoryController History 
        { 
            get { return _historyController; } 
        }

        public Graph Graph 
        { 
            get { return _graph; } 
            set { _graph = value; }
        }

        public void Undo()
        {
            _historyController.Undo();
        }

        public void Redo()
        {
            _historyController.Redo();
        }

        private void ButtonUndoClicked(object sender, RoutedEventArgs e)
        {
            Undo();
        }

        private void ButtonRedoClicked(object sender, RoutedEventArgs e)
        {
            Redo();
        }

        private void ZoomInClicked(object sender, RoutedEventArgs e)
        {
            _canvas.ZoomOriginCenter();
            _canvas.Zoom *= 1.1;
        }

        private void ZoomOutClicked(object sender, RoutedEventArgs e)
        {
            _canvas.ZoomOriginCenter();
            _canvas.Zoom /= 1.1;
        }

        private void CanvasZoomed(object sender, CanvasArgs e)
        {
            ZoomSlider.Value = (sender as CanvasArea).Zoom;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasClicked(object sender, CanvasArgs e)
        {
            var position = e.Position;
            var source = e.Source;

            var x = position.X;
            var y = position.Y;

            if (source == null) return;

            if (e.Properties.RightButtonRised && _currentMode == Mode.Normal)
            {
                ContextMenu.Items.Clear();

                _contextParams = new ContextParams
                {
                    Position = position,
                    Source = source,
                };


                if (source == MainCanvas)
                {
                    var addNode = new MenuFlyoutItem();
                    addNode.Text = "Create node";
                    addNode.Click += ContextAddNode;

                    ContextMenu.Items.Add(addNode);
                }
                // Node context menu items
                else if (source.GetType() == typeof(Ellipse))
                {
                    var linkTo = new MenuFlyoutItem();
                    linkTo.Text = "Link to";
                    linkTo.Click += ContextLinkTo;

                    var renameNode = new MenuFlyoutItem();
                    renameNode.Text = "Rename";
                    renameNode.Click += ContextRenameNode;

                    var removeNode = new MenuFlyoutItem();
                    removeNode.Text = "Remove node";
                    removeNode.Click += ContextRemoveNode;

                    ContextMenu.Items.Add(linkTo);
                    ContextMenu.Items.Add(renameNode);
                    ContextMenu.Items.Add(removeNode);
                }
                // Edge context menu items
                else if (source.GetType() == typeof(Windows.UI.Xaml.Shapes.Path))
                {
                    //var unlinkStart = new MenuFlyoutItem();
                    //unlinkStart.Text = "Unlink start";
                    //unlinkStart.Click += ContextLinkTo;

                    //var unlinkEnd = new MenuFlyoutItem();
                    //unlinkEnd.Text = "Unlink end";
                    //unlinkEnd.Click += ContextLinkTo;

                    var reverseDirection = new MenuFlyoutItem();
                    reverseDirection.Text = "Reverse direction";
                    reverseDirection.Click += ContextReverseEdge;

                    var setWeight = new MenuFlyoutItem();
                    setWeight.Text = "Weight";
                    setWeight.Click += ContextWeightEdge;

                    var removeEdge = new MenuFlyoutItem();
                    removeEdge.Text = "Remove edge";
                    removeEdge.Click += ContextRemoveEdge;

                    //ContextMenu.Items.Add(unlinkStart);
                    //ContextMenu.Items.Add(unlinkEnd);
                    ContextMenu.Items.Add(reverseDirection);
                    ContextMenu.Items.Add(setWeight);
                    ContextMenu.Items.Add(removeEdge);
                }
            }
            else if (e.Properties.RightButtonRised && _currentMode != Mode.Normal) {
                SwitchMode(Mode.Normal);
            }

            if (e.Properties.LeftButtonRised)
            {
                if (_currentMode == Mode.Erasing && source != MainCanvas)
                {
                    RemoveElement(source);
                }

                if (_currentMode == Mode.AddingNode && source == MainCanvas)
                {
                    AddNode(x, y);
                }

                if (_currentMode == Mode.AddingOrientedEdge && source != MainCanvas && !_addingEdge)
                {
                    StartEdge(source as UIElement, EdgeType.Directional);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasMoved(object sender, CanvasArgs e)
        {
            var source = e.Source;

            if (source == null || e.Properties == null) return;

            if (e.Properties.LeftButtonRised)
            {
                if (_currentMode == Mode.Erasing && source != MainCanvas)
                {
                    RemoveElement(source);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        private void RemoveElement(UIElement source)
        {
            var element = _canvas.Find(source);

            if (element == null) return;

            if (element.GetType() == typeof(NodeElement))
            {
                var nodeElement = (NodeElement)element;

                var command = new RemoveNodeCommand(nodeElement, nodeElement.Center, _graph);
                _historyController.Execute(command);
            }
            else if (element.GetType() == typeof(EdgeElement))
            {
                var edgeElement = (EdgeElement)element;

                var command = new RemoveEdgeCommand(edgeElement, edgeElement.From, edgeElement.To, _graph);
                _historyController.Execute(command);
            }
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextOpened(object sender, object e)
        {
            if (ContextMenu.Items.Count == 0 || _contextParams == null)
            {
                ContextMenu.Hide();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextClosed(object sender, object e)
        {
            _contextParams = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextAddNode(object sender, RoutedEventArgs e)
        {
            if (_contextParams.Source == MainCanvas)
                AddNode(_contextParams.Position.X, _contextParams.Position.Y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ContextRenameNode(object sender, RoutedEventArgs e)
        {
            if (_contextParams.Source.GetType() == typeof(Ellipse))
            {
                var nodeElement = _canvas.Find(_contextParams.Source as UIElement);

                if (nodeElement == null) return;
                if (nodeElement.GetType() != typeof(NodeElement)) return;

                ContentDialog dialog = new ContentDialog();

                dialog.Title = "Rename node";
                dialog.PrimaryButtonText = "Save";
                dialog.CloseButtonText = "Cancel";
                dialog.DefaultButton = ContentDialogButton.Primary;
                dialog.Content = new RenameDialog();

                var content = (RenameDialog) dialog.Content;
                content.ElementName = nodeElement.Label.Text;
                content.TextField.Loaded += (object field, RoutedEventArgs args) =>
                {
                    var textBox = (TextBox) field;
                    textBox.SelectAll();
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    nodeElement.Label.Text = content.ElementName;
                }
                content.ElementName = "";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ContextWeightEdge(object sender, RoutedEventArgs e)
        {
            if (_contextParams.Source.GetType() == typeof(Path))
            {
                var element = _canvas.Find(_contextParams.Source as UIElement);

                if (element == null) return;
                if (element.GetType() != typeof(EdgeElement)) return;

                var edgeElement = (EdgeElement) element;

                ContentDialog dialog = new ContentDialog();

                dialog.Title = "Set edge weight";
                dialog.PrimaryButtonText = "Save";
                dialog.CloseButtonText = "Cancel";
                dialog.DefaultButton = ContentDialogButton.Primary;
                dialog.Content = new WeightDialog();

                var content = (WeightDialog) dialog.Content;
                content.EdgeWeight = edgeElement.Edge.Weight;

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    edgeElement.Edge.Weight = content.EdgeWeight;
                    edgeElement.Label.Text = content.EdgeWeight.ToString();
                }
                content.EdgeWeight = 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextRemoveNode(object sender, RoutedEventArgs e)
        {
            if (_contextParams.Source.GetType() == typeof(Ellipse))
                RemoveNode(_contextParams.Source as UIElement);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextLinkTo(object sender, RoutedEventArgs e)
        {
            if (_contextParams.Source.GetType() == typeof(Ellipse))
                StartEdge(_contextParams.Source as UIElement, EdgeType.Directional);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextUnlinkStart(object sender, RoutedEventArgs e)
        {
            if (_contextParams.Source.GetType() == typeof(Ellipse))
                StartEdge(_contextParams.Source as UIElement);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextUnlinkEnd(object sender, RoutedEventArgs e)
        {
            if (_contextParams.Source.GetType() == typeof(Ellipse))
                StartEdge(_contextParams.Source as UIElement);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextRemoveEdge(object sender, RoutedEventArgs e)
        {
            if (_contextParams.Source.GetType() == typeof(Windows.UI.Xaml.Shapes.Path))
            {
                var edgeElement = (EdgeElement) _canvas.Find(_contextParams.Source as UIElement);
                if (edgeElement == null) return;
                edgeElement.Remove();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextReverseEdge(object sender, RoutedEventArgs e)
        {
            if (_contextParams.Source.GetType() == typeof(Windows.UI.Xaml.Shapes.Path))
            {
                var edgeElement = (EdgeElement) _canvas.Find(_contextParams.Source as UIElement);
                if (edgeElement == null) return;

                var command = new ReverseEdgeCommand(edgeElement);
                _historyController.Execute(command);
            }
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void AddNode(double x, double y)
        {
            var nodeElement = new NodeElement(_canvas);

            var command = new AddNodeCommand(nodeElement, new Coordinates(x, y), _graph);
            _historyController.Execute(command);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        private void RemoveNode(object source)
        {
            if (source.GetType() == typeof(Ellipse))
            {
                var nodeElement = (NodeElement) _canvas.Find(source as UIElement);
                if (nodeElement == null) return;

                var command = new RemoveNodeCommand(nodeElement, nodeElement.Center, _graph);
                _historyController.Execute(command);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        private void StartEdge(UIElement element, EdgeType type = EdgeType.Directional)
        {
            var found = _canvas.Find(element);
            if (found.GetType() != typeof(NodeElement)) return;

            var nodeElement = (NodeElement) found;
            if (nodeElement == null) return;

            var edgeElement = new EdgeElement(_canvas);
            edgeElement.Type = type;
            edgeElement.Removed += EdgeRemoved;

            edgeElement.Add(nodeElement);
            edgeElement.LinkStartTo(nodeElement);

            _selectedElement = edgeElement;
            _canvas.DrawLineFrom(edgeElement.Element, edgeElement.From.Center.X, edgeElement.From.Center.Y);

            _canvas.Pressed += EndEdge;
            _canvas.Canceled += RemoveEdge;

            _addingEdge = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EndEdge(object sender, CanvasArgs e)
        {
            _canvas.Pressed -= EndEdge;
            _canvas.Canceled -= RemoveEdge;

            var edgeElement = (EdgeElement) _selectedElement;

            var canvasElement = _canvas.Find(e.Source);
            if (canvasElement == null || canvasElement.GetType() != typeof(NodeElement))
            {
                _addingEdge = false;
                return;
            }

            var nodeElement = (NodeElement) canvasElement;
            edgeElement.LinkEndTo(nodeElement);

            var collection = _canvas.Children.Where((elem) => {
                if (elem.GetType() != typeof(EdgeElement))
                    return false;

                EdgeElement edge = (EdgeElement)elem;
                return edge.From == edgeElement.From && edge.To == edgeElement.To;
            });

            if (collection.Count() > 1)
            {
                edgeElement.Remove();
                _addingEdge = false;
                return;
            }

            collection = _canvas.Children.Where((elem) => {
                if (elem.GetType() != typeof(EdgeElement))
                    return false;

                EdgeElement edge = (EdgeElement)elem;
                return edge.From == edgeElement.From && edge.To == edgeElement.To || 
                    edge.From == edgeElement.To && edge.To == edgeElement.From;
            });

            if (collection.Count() > 1)
            {
                var firstElem = ((EdgeElement)collection.First());
                edgeElement.Edge.Weight = firstElem.Edge.Weight;
                firstElem.Edge.WeightChanged += (object s, double weight) =>
                {
                    edgeElement.Edge.Weight = weight;
                    edgeElement.Removed += (object _, EdgeElementArgs args) =>
                    {
                        firstElem.Remove();
                    };
                };
                edgeElement.Edge.WeightChanged += (object s, double weight) =>
                {
                    firstElem.Edge.Weight = weight;
                    firstElem.Removed += (object _, EdgeElementArgs args) =>
                    {
                        edgeElement.Remove();
                    };
                };
            }

            var command = new AddEdgeCommand(edgeElement, edgeElement.From, edgeElement.To, _graph);
            _historyController.Execute(command);

            _addingEdge = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveEdge(object sender, CanvasArgs e)
        {
            var edgeElement = (EdgeElement) _selectedElement;
            edgeElement.Remove();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NodeMoved(object sender, CanvasArgs e)
        {
            if (e.PreviousPosition == e.Position) return;

            var nodeElement = _canvas.Find(e.Source);
            if (nodeElement == null) return;

            var command = new MoveNodeCommand(nodeElement, e.PreviousPosition, e.Position);
            _historyController.Push(command);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EdgeRemoved(object sender, EdgeElementArgs e)
        {
            var edgeElement = (EdgeElement) sender;
            edgeElement.Removed -= EdgeRemoved;

            var command = new RemoveEdgeCommand(edgeElement, edgeElement.From, edgeElement.To, _graph);
            _historyController.Push(command);
        }

        


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        public void SwitchMode(Mode mode = Mode.Normal)
        {
            ClearButtons();
            _canvas.AllowElementMove = true;

            _canvas.Moved -= ShadowNodeMoved;
            if (_nodeShadow != null)
            {
                _nodeShadow.PointerPressed -= ShadowNodePressed;
                _canvas.Canvas.Children.Remove(_nodeShadow);
            }

            _currentMode = mode;

            switch (mode)
            {
                case Mode.Erasing:
                    _canvas.AllowElementMove = false;
                    EraseButton.IsChecked = true;
                    return;

                case Mode.AddingNode:
                    if (_nodeShadow == null)
                    {
                        _nodeShadow = new NodeElement(_canvas).CreateElement() as Ellipse;
                        _nodeShadow.Name = "NodeShadow";
                        _nodeShadow.Opacity = .25;
                    }

                    _nodeShadow.PointerPressed += ShadowNodePressed;

                    Canvas.SetZIndex(_nodeShadow, 0);
                    Canvas.SetLeft(_nodeShadow, -_nodeShadow.ActualSize.X);
                    Canvas.SetTop(_nodeShadow, -_nodeShadow.ActualSize.Y);

                    if (!_canvas.Canvas.Children.Contains(_nodeShadow))
                        _canvas.Canvas.Children.Add(_nodeShadow);

                    _canvas.Moved += ShadowNodeMoved;
                    AddNodeButton.IsChecked = true;
                    return;

                //case Mode.AddingEdge:
                //    AddEdgeButton.IsChecked = true;
                //    return;

                case Mode.AddingOrientedEdge:
                    AddOrientedEdgeButton.IsChecked = true;
                    return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void EraseSelected(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleButton).IsChecked == true)
            {
                SwitchMode(Mode.Erasing);
            } 
            else
            {
                SwitchMode();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddNodeSelected(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleButton).IsChecked == true)
            {
                SwitchMode(Mode.AddingNode);
            }
            else
            {
                SwitchMode();
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddEdgeSelected(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleButton).IsChecked == true)
            {
                SwitchMode(Mode.AddingEdge);
            }
            else
            {
                SwitchMode();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddOrientedEdgeSelected(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleButton).IsChecked == true)
            {
                SwitchMode(Mode.AddingOrientedEdge);
            }
            else
            {
                SwitchMode();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShadowNodeMoved(object sender, CanvasArgs e)
        {
            Canvas.SetLeft(_nodeShadow, e.Position.X - (_nodeShadow.ActualSize.X / 2));
            Canvas.SetTop(_nodeShadow, e.Position.Y - (_nodeShadow.ActualSize.Y / 2));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShadowNodePressed(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(_canvas.Canvas);

            if (!pointer.Properties.IsLeftButtonPressed) return;

            AddNode(pointer.RawPosition.X, pointer.RawPosition.Y);
        }

        private void ZoomSliderChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_canvas.Zoom == e.NewValue) return;

            _canvas.ZoomOriginCenter();
            _canvas.Zoom = e.NewValue;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ClearButtons()
        {
            EraseButton.IsChecked = false;
            AddNodeButton.IsChecked = false;
            //AddEdgeButton.IsChecked = false;
            AddOrientedEdgeButton.IsChecked = false;
        }

        private async void InfoClicked(object sender, RoutedEventArgs e)
        {
            var text = "";
            foreach (var node in _graph.Nodes)
            {
                text += node.ToString() + "\n";
            }
            text += "\n";
            foreach (var edge in _graph.Edges)
            {
                text += edge.ToString() + "\n";
            }

            ContentDialog dialog = new ContentDialog();

            dialog.Title = "Info";
            dialog.PrimaryButtonText = "Ok";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = new InfoDialog();

            var content = (InfoDialog) dialog.Content;
            content.InfoText = text;

            var result = await dialog.ShowAsync();
        }

        private async void DfsClicked(object sender, RoutedEventArgs e)
        {
            if (_canvas.SelectedElements.Count < 1) return;

            var starterElement = _canvas.SelectedElements.First();

            if (starterElement.GetType() != typeof(NodeElement)) return;

            var nodeElement = (NodeElement) starterElement;
            var text = _graph.DFS(nodeElement.Node);

            ContentDialog dialog = new ContentDialog();

            dialog.Title = "DFS";
            dialog.PrimaryButtonText = "Ok";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = new InfoDialog();

            var content = (InfoDialog) dialog.Content;
            content.InfoText = text;

            var result = await dialog.ShowAsync();
        }

        public void LoadGraph()
        {
            foreach (var node in _graph.Nodes)
            {
                var nodeElement = new NodeElement(_canvas);
                nodeElement.Node = node;
                nodeElement.Label.Text = node.Label;
                nodeElement.Add(new Coordinates(node.Params.X, node.Params.Y));
            }
            foreach (var edge in _graph.Edges)
            {
                var from = _canvas.Find(edge.StartNodeKey) as NodeElement;
                var to = _canvas.Find(edge.EndNodeKey) as NodeElement;

                edge.StartNode = from.Node;
                edge.EndNode = to.Node;

                var edgeElement = new EdgeElement(_canvas);
                edgeElement.Edge = edge;

                if (from != null && to != null)
                {
                    edgeElement.Add(from, to);
                }
            }
        }

        public void ClearAll()
        {
            _graph.Nodes.Clear();
            _graph.Edges.Clear();

            _canvas.ClearAll();
            _canvas.Zoom = _canvas.ZoomRange.Min;

            SwitchMode(Mode.Normal);
        }
    }
}
