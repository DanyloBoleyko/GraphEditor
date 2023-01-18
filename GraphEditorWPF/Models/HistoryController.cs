using GraphEditorWPF.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphEditorWPF.Models
{
    public class HistoryController
    {
        private Stack<ICommand> _undoStack = new Stack<ICommand>();
        private Stack<ICommand> _redoStack = new Stack<ICommand>();

        public Stack<ICommand> UndoStack
        {
            get { return _undoStack; }
        }

        public Stack<ICommand> RedoStack
        {
            get { return _redoStack; }
        }

        /// <summary>
        /// Undo last command
        /// </summary>
        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                var command = _undoStack.Pop();
                command.UnExecute();
                _redoStack.Push(command);
            }
        }

        /// <summary>
        /// Redo last command
        /// </summary>
        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                var command = _redoStack.Pop();
                command.Execute();
                _undoStack.Push(command);
            }
        }

        /// <summary>
        /// Execute command
        /// </summary>
        /// <param name="command"></param>
        public void Execute(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
        }

        /// <summary>
        /// Pushes command to a history list
        /// </summary>
        /// <param name="command"></param>
        public void Push(ICommand command)
        {
            _undoStack.Push(command);
            _redoStack.Clear();
        }
    }
}
