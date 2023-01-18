using GraphEditorWPF.Models.NodeModels;
using GraphEditorWPF.Models.EdgeModels;
using GraphEditorWPF.Types;
using GraphEditorWPF.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using GraphEditorWPF.Models;

namespace GraphEditorWPF.Commands
{
    public class ReverseEdgeCommand : ICommand
    {
        private EdgeElement _edge;

        public ReverseEdgeCommand(EdgeElement edge)
        {
            _edge = edge;
        }

        public string Name
        {
            get { return "ReverseEdge"; }
        }

        public void Execute()
        {
            _edge.Reverse();
        }

        public void UnExecute()
        {
            _edge.Reverse();
        }
    }
}
