using GraphEditorWPF.Models;
using GraphEditorWPF.Types;
using GraphEditorWPF.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GraphEditorWPF.Commands
{
    public class MoveNodeCommand : ICommand
    {
        private CanvasElement _element;
        private Coordinates _from;
        private Coordinates _to;

        public MoveNodeCommand(CanvasElement element, Coordinates from, Coordinates to)
        {
            _from = from;
            _to = to;
            _element = element;
        }

        public string Name
        {
            get { return "MoveNode"; }
        }

        public void Execute()
        {
            if (_element.GetType() == typeof(NodeElement))
            {
                var nodeElement = (NodeElement) _element;
                nodeElement.MoveTo(_to);
            }
        }

        public void UnExecute()
        {
            if (_element == null) return;

            if (_element.GetType() == typeof(NodeElement))
            {
                var nodeElement = (NodeElement) _element;
                nodeElement.MoveTo(_from);
            }
        }
    }
}
