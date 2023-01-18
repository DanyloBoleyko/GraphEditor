﻿using GraphEditorWPF.Models.NodeModels;
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
    public class RemoveEdgeCommand : ICommand
    {
        private EdgeElement _edge;
        private NodeElement _fromNode;
        private NodeElement _toNode;
        private Graph _graph;

        public RemoveEdgeCommand(EdgeElement edge, NodeElement from, Graph graph)
        {
            _edge = edge;
            _graph = graph;
            _fromNode = from;
            _toNode = null;
        }

        public RemoveEdgeCommand(EdgeElement edge, NodeElement from, NodeElement to, Graph graph)
        {
            _edge = edge;
            _graph= graph;
            _fromNode = from;
            _toNode = to;
        }

        public string Name
        {
            get { return "AddNode"; }
        }

        public void Execute()
        {
            _graph.Edges.Remove(_edge.Edge);
            _edge.Remove();
        }

        public void UnExecute()
        {
            if (_toNode == null)
            {
                _edge.Add(_fromNode);
            }
            else
            {
                _edge.Add(_fromNode, _toNode);
                _graph.Edges.Add(_edge.Edge);
            }
        }
    }
}
