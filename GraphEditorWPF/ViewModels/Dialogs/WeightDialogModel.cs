using Microsoft.UI.Xaml.Controls;
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
    public partial class WeightDialog : Page
    {
        public double EdgeWeight = 0;

        public WeightDialog()
        {
            this.InitializeComponent();
        }

        public NumberBox NumberField
        {
            get { return this.Field; }
        }
    }
}
