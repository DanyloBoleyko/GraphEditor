using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace GraphEditorWPF.ViewModels.Dialogs
{
    public partial class InfoDialog : Page
    {
        public string InfoText = "";

        public InfoDialog()
        {
            this.InitializeComponent();
        }

        public TextBlock TextBlock
        {
            get { return this.InfoTextBlock; }
        }
    }
}
