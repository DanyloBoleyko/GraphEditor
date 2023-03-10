using GraphEditorWPF.Types;
using GraphEditorWPF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace GraphEditorWPF.EventArguments
{
    public class CanvasArgs
    {
        public UIElement Source { get; set; }
        public PointerProperties Properties { get; set; }
        public Coordinates Position { get; set; }
        public Coordinates PreviousPosition { get; set; }
    }
}
