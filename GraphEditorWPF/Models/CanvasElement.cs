using GraphEditorWPF.Types;
using System;
using System.Numerics;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace GraphEditorWPF.Models
{
    public class CanvasElement
    {
        protected UIElement _element;
        protected Label _label = new Label();
        protected bool _selected = false;
        protected CanvasArea _canvas;

        public event EventHandler<bool> SelectionChanged;

        public CanvasElement(CanvasArea canvas = null, UIElement element = null)
        {
            _element = element;
            _canvas = canvas;
        }

        public CanvasElement(CanvasArea canvas)
        {
            _canvas = canvas;
        }

        public UIElement Element { 
            get { return _element; } 
            set 
            {
                _element = value;

                //_element.PointerPressed += ElementPressed;
                //_element.PointerMoved += ElementMoved;
                //_element.PointerEntered += PointerEntered;
                //_element.PointerExited += ElementExited;
                //_element.PointerReleased += ElementReleased;
            }
        }

        public Label Label { 
            get { return _label; } 
            set { _label = value; }
        }

        public bool IsSelected { 
            get { return _selected; } 
            set 
            { 
                _selected = value;
                if (SelectionChanged != null)
                {
                    SelectionChanged(this, _selected);
                }
            }
        }

        public Canvas Canvas { 
            get { return _canvas.Canvas; } 
        }

        public CanvasArea CanvasArea { 
            get { return _canvas; } 
            set { _canvas = value; }
        }

        public void MoveBy(Vector2 vector)
        {

        }

        public void ApplyScale()
        {

        }

        private void ElementPressed(object sender, PointerRoutedEventArgs e)
        {

        }

        private void ElementReleased(object sender, PointerRoutedEventArgs e)
        {

        }

        private void ElementMoved(object sender, PointerRoutedEventArgs e)
        {

        }

        private void PointerEntered(object sender, PointerRoutedEventArgs e)
        {

        }

        private void ElementExited(object sender, PointerRoutedEventArgs e)
        {

        }
    }
}
