using GraphEditorWPF.Models;
using GraphEditorWPF.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static System.Net.WebRequestMethods;

namespace GraphEditorWPF.ViewModels
{
    public sealed partial class MainView : Page
    {
        public StorageFile openedFile;

        public MainView()
        {
            this.InitializeComponent();
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            // Hide default title bar.
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            UpdateTitleBarLayout(coreTitleBar);

            // Set XAML element as a draggable region.
            Window.Current.SetTitleBar(AppTitleBar);

            // Register a handler for when the size of the overlaid caption control changes.
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            MainFrame.Navigate(typeof(EditorView));
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            AppTitleBar.Height = coreTitleBar.Height;

            // Ensure the custom title bar does not overlap window caption controls
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender);
        }

        public void ButtonUndoClicked(object sender, RoutedEventArgs e)
        {
            var page = MainFrame.Content as EditorView;
            page.Undo();
        }

        public void ButtonRedoClicked(object sender, RoutedEventArgs e)
        {
            var page = MainFrame.Content as EditorView;
            page.Redo();
        }

        public void CloseWindowClicked(object sender, RoutedEventArgs e)
        {
            ApplicationView.GetForCurrentView().TryConsolidateAsync();
        }

        private async Task WriteGraphToFile(StorageFile file)
        {
            var page = MainFrame.Content as EditorView;
            await Windows.Storage.FileIO.WriteTextAsync(file, page.Graph.ToJson(Newtonsoft.Json.Formatting.Indented));
        }

        private async Task ReadGraphFromFile(StorageFile file)
        {
            var page = MainFrame.Content as EditorView;
            page.ClearAll();
            string json = await Windows.Storage.FileIO.ReadTextAsync(file);
            page.Graph.FromJson(json);
            page.LoadGraph();
        }

        private async Task<StorageFile> FileSavePicker()
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("JSON data", new List<string>() { ".json" });
            savePicker.SuggestedFileName = "New Graph";

            var file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                Windows.Storage.CachedFileManager.DeferUpdates(file);

                return file;
            }
            else
            {
                return null;
            }
        }

        private async Task<StorageFile> FileOpenPicker()
        {
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".json");

            var file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                Windows.Storage.CachedFileManager.DeferUpdates(file);

                return file;
            }
            else
            {
                return null;
            }
        }

        private async Task SaveFileDialog()
        {
            var file = await FileSavePicker();
            if (file == null) return;

            openedFile = file;

            await WriteGraphToFile(file);

            Windows.Storage.Provider.FileUpdateStatus status = await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);

            if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
            {
                //this.textBlock.Text = "File " + file.Name + " was saved.";
            }
            else
            {
                //this.textBlock.Text = "File " + file.Name + " couldn't be saved.";
            }
        }

        private async Task OpenFileDialog()
        {
            var file = await FileOpenPicker();
            if (file == null) return;

            openedFile = file;

            await ReadGraphFromFile(file);

            Windows.Storage.Provider.FileUpdateStatus status = await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);

            if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
            {
                //this.textBlock.Text = "File " + file.Name + " was saved.";
            }
            else
            {
                //this.textBlock.Text = "File " + file.Name + " couldn't be saved.";
            }
        }

        private void CreateNew()
        {
            var page = MainFrame.Content as EditorView;
            openedFile = null;
            page.ClearAll();
        }

        public async void SaveClicked(object sender, RoutedEventArgs e)
        {
            if (openedFile == null)
            {
                await SaveFileDialog();
            }
            else
            {
                await WriteGraphToFile(openedFile);
            }
        }

        public async void SaveAsClicked(object sender, RoutedEventArgs e)
        {
            await SaveFileDialog();
        }

        public async void OpenClicked(object sender, RoutedEventArgs e)
        {
            await OpenFileDialog();
        }

        public async void NewClicked(object sender, RoutedEventArgs e)
        {
            var page = MainFrame.Content as EditorView;

            if (!page.Area.IsEmpty)
            {
                ContentDialog dialog = new ContentDialog();

                dialog.Title = "Save changes?";
                dialog.PrimaryButtonText = "Save";
                dialog.SecondaryButtonText = "Delete";
                dialog.CloseButtonText = "Cancel";
                dialog.DefaultButton = ContentDialogButton.Primary;
                dialog.Content = new SaveDialog();

                var content = (SaveDialog) dialog.Content;
                content.Text = "You have unsaved changes, do you want to save them?";

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    SaveClicked(sender, e);
                }
                else if (result == ContentDialogResult.Secondary)
                {
                    CreateNew();
                }
            }
            else
            {
                CreateNew();
            }
        }

        public void AddNodeClicked(object sender, RoutedEventArgs e)
        {
            var page = MainFrame.Content as EditorView;
            page.SwitchMode(Types.Mode.AddingNode);
        }

        public void AddEdgeClicked(object sender, RoutedEventArgs e)
        {
            var page = MainFrame.Content as EditorView;
            page.SwitchMode(Types.Mode.AddingEdge);
        }

        public void AddOrientedEdgeClicked(object sender, RoutedEventArgs e)
        {
            var page = MainFrame.Content as EditorView;
            page.SwitchMode(Types.Mode.AddingOrientedEdge);
        }

        public void EraseClicked(object sender, RoutedEventArgs e)
        {
            var page = MainFrame.Content as EditorView;
            page.SwitchMode(Types.Mode.Erasing);
        }

        public void DefaultClicked(object sender, RoutedEventArgs e)
        {
            var page = MainFrame.Content as EditorView;
            page.SwitchMode(Types.Mode.Normal);
        }
    }
}
