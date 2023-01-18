using GraphEditorWPF.Models;
using GraphEditorWPF.Types;
using GraphEditorWPF.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace GraphEditorWPF.Commands
{
    public class RemoveNodeCommand : ICommand
    {
        private NodeElement _node;
        private Coordinates _coords;
        private Graph _graph;

        public RemoveNodeCommand(NodeElement node, Coordinates coords, Graph graph)
        {
            _node = node;
            _coords = coords;
            _graph = graph;
        }

        public string Name
        {
            get { return "RemoveNode"; }
        }

        public void Execute()
        {
            _node.Remove();
            if (_graph != null)
            {
                _graph.Nodes.Remove(_node.Node);
            }
        }

        public void UnExecute()
        {
            _node.Add(_coords);
            if (_graph != null)
            {
                _graph.Nodes.Add(_node.Node);
            }
        }
    }
}
