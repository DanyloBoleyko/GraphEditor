using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input;
using Windows.UI.Xaml.Input;

namespace GraphEditorWPF.Utils
{
    public class PointerProperties
    {
        private bool _leftButton = false;
        private bool _middleButton = false;
        private bool _rightButton = false;
        private bool _shiftPressed = false;
        private bool _ctrlPressed = false;

        public PointerProperties(bool leftButton = false, bool middleButton = false, bool rightButton = false)
        {
            _leftButton = leftButton;
            _middleButton = middleButton;
            _rightButton = rightButton;
        }

        public PointerProperties(PointerPoint pointer)
        {
            _leftButton = pointer.Properties.IsLeftButtonPressed;
            _middleButton = pointer.Properties.IsLeftButtonPressed;
            _rightButton = pointer.Properties.IsLeftButtonPressed;
        }

        public bool LeftButtonRised
        { 
            get { return _leftButton; }
            set { _leftButton = value; }
        }

        public bool MiddleButtonRised 
        { 
            get { return _middleButton; }
            set { _middleButton = value; }
        }

        public bool RightButtonRised 
        { 
            get { return _rightButton; }
            set { _rightButton = value; }
        }

        public bool ShiftRised 
        { 
            get { return _shiftPressed; }
            set { _shiftPressed = value; }
        }

        public bool CtrlRised 
        { 
            get { return _ctrlPressed; }
            set { _ctrlPressed = value; }
        }
    }
}
