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
using PdfViewModel;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Data.Pdf;
using Windows.Storage.Pickers;
using PDFShowcase.UWP.Model;
using PDFShowcase.UWP.Converter;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace PDFShowcase.UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private PdfDocViewModel pdfDataSourceZoomedInView;
        private PdfDocViewModel pdfDataSourceZoomedOutView;
        private PdfPageViewModel pdfPageViewModel;
        private PdfDocument pdfDocument;
        private StorageFile loadedFile;
        private GridView zoomedOutView;
        private ListView zoomedInView;
        private PageModel pageModel;

        public MainPage()
        {
            this.InitializeComponent();
            this.pageModel = new PageModel();
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Size pageSize;

            pageSize.Width = Window.Current.Bounds.Width;
            pageSize.Height = Window.Current.Bounds.Height;

            this.semanticZoom.Width = this.zoomedInView.Width = pageSize.Width;
            this.semanticZoom.Height = this.zoomedInView.Height = pageSize.Height;
            
        }

        /// <summary> 
        /// This overridden function is called whenever this page is navigated to 
        /// </summary> 
        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            // Load and render PDF file from the APPX Assets 
            this.LoadDefaultFile();
        }

        /// <summary> 
        /// This function loads PDF file from the assets 
        /// </summary> 
        private async void LoadDefaultFile()
        {
            // Getting installed location of this app 
            StorageFolder installedLocation = Package.Current.InstalledLocation;

            // Get the sample file from the assets folder 
            this.loadedFile = await installedLocation.GetFileAsync("Assets\\SanDisk_SecureAccess_QSG.pdf");
            this.LoadPDF(this.loadedFile);
        }

        /// <summary> 
        /// Function to load the PDF file selected by the user 
        /// </summary> 
        /// <param name="pdfFile">StorageFile object of the selected PDF file</param> 
        private async void LoadPDF(StorageFile pdfFile)
        {
            // Creating async operation to load the PDF file and render pages in zoomed-in and zoomed-out view 
            // For password protected documents one needs to call the function as is, handle the exception  
            // returned from LoadFromFileAsync and then call it again by providing the appropriate document  
            // password. 
            this.pdfDocument = await PdfDocument.LoadFromFileAsync(pdfFile);
            if (this.pdfDocument != null)
            {
                InitializeZoomedInView();
                InitializeZoomedOutView();

                this.SizeChanged += MainPage_SizeChanged;
                
            }
        }

        /// <summary> 
        /// Function to initialize ZoomedInView of Semantic Zoom control 
        /// </summary> 
        private void InitializeZoomedInView()
        {
            // Page Size is set to zero for items in main view so that pages of original size are rendered 
            Size pageSize;

            pageSize.Width = Window.Current.Bounds.Width;
            pageSize.Height = Window.Current.Bounds.Height;

            // Main view items are rendered on a VSIS surface as they can be resized (optical zoom) 
            this.zoomedInView = new ListView();
            this.zoomedInView.Style = this.zoomedInViewStyle;
            this.zoomedInView.ItemTemplate = this.zoomedInViewItemTemplate;
            this.zoomedInView.ItemsPanel = this.zoomedInViewItemsPanelTemplate;
            this.zoomedInView.Template = this.zoomedInViewControlTemplate;
            this.pdfDataSourceZoomedInView = new PdfDocViewModel(pdfDocument, pageSize, SurfaceType.VirtualSurfaceImageSource);
            this.zoomedInView.ItemsSource = this.pdfDataSourceZoomedInView;

            this.pageModel.PageSize = this.pdfDataSourceZoomedInView.Count;
            this.zoomedInView.Width = pageSize.Width;
            this.zoomedInView.Height = pageSize.Height;

            this.semanticZoom.ZoomedInView = zoomedInView;
        }

        /// <summary> 
        /// Function to initialize ZoomedOutView of Semantic Zoom control 
        /// </summary> 
        private void InitializeZoomedOutView()
        {
            // Page Size is set to zero for items in main view so that pages of original size are rendered 
            Size pageSize;

            // Page size for thumbnail view is set to 300px as this gives good view of the thumbnails on all resolutions 
            pageSize.Width = (double)this.Resources["thumbnailWidth"];
            pageSize.Height = (double)this.Resources["thumbnailHeight"];

            // Thumbnail view items are rendered on a SIS surface as they are of fixed size 
            this.pdfDataSourceZoomedOutView = new PdfDocViewModel(pdfDocument, pageSize, SurfaceType.SurfaceImageSource);

            this.zoomedOutView = new GridView();
            this.zoomedOutView.Style = this.zoomedOutViewStyle;
            this.zoomedOutView.ItemTemplate = this.zoomedOutViewItemTemplate;
            this.zoomedOutView.ItemsPanel = this.zoomedOutViewItemsPanelTemplate;
            this.zoomedOutView.ItemContainerStyle = this.zoomedOutViewItemContainerStyle;
            this.zoomedOutView.ItemsSource = this.pdfDataSourceZoomedOutView;
            this.semanticZoom.ZoomedOutView = this.zoomedOutView;
        }

        /// <summary> 
        /// Event Handler for handling application suspension 
        /// </summary> 
        public void OnSuspending(object sender, SuspendingEventArgs args)
        {
            // Hint to the driver that the app is entering an idle state and that its memory 
            // can be temporarily used for other apps. 
            this.pdfDataSourceZoomedInView.Trim();
            this.pdfDataSourceZoomedOutView.Trim();
        }

        /// <summary> 
        /// Open File click handler for command bar 
        /// This function loads the PDF file selected by the user 
        /// </summary> 
        private async void OnOpenFileClick(object sender, RoutedEventArgs e)
        {
            // Launching FilePicker 
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.FileTypeFilter.Add(".pdf");

            // Creating async operation for PickSingleFileAsync 
            StorageFile pdfFile = await openPicker.PickSingleFileAsync();
            if (pdfFile != null)
            {
                // Validating if selected file is not the same as file currently loaded 
                if (this.loadedFile.Path != pdfFile.Path)
                {
                    this.loadedFile = pdfFile;
                    LoadPDF(pdfFile);
                }
            }
        }

        /// <summary> 
        /// Event Handler for ViewChanged event of ScrollViewer for zoomedout view 
        /// This method is invoked to recreate VSIS surface of new width/height and re-render the page image at high resolution 
        /// </summary> 
        /// <param name="sender">Scroll Viewer</param> 
        /// <param name="e">ScrollViewerViewChangedEventArgs</param> 
        private void EventHandlerViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
            {
                var scrollViewer = sender as ScrollViewer;
                if (scrollViewer != null)
                {
                    // Reloading pages at new zoomFactor 
                    this.pdfDataSourceZoomedInView.UpdatePages(scrollViewer.ZoomFactor);
                }
            }
        }

        /// <summary> 
        /// Event handler for ViewChangeStarted event for SemanticZoom 
        /// </summary> 
        /// <param name="e">SemanticZoomViewChangedEventArgs</param> 
        private void EventHandlerViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            PdfPageViewModel sourceItem = e.SourceItem.Item as PdfPageViewModel;
            if (sourceItem != null)
            {
                int pageIndex = (int)(sourceItem.PageIndex);
                if (this.pdfDataSourceZoomedInView.Count > pageIndex)
                {
                    // Transitioning from Zooomed Out View to Zoomed In View 
                    if (this.semanticZoom.IsZoomedInViewActive)
                    {
                        // Getting destination item from Zoomed-In-View 
                        PdfPageViewModel destinationItem = (PdfPageViewModel)this.pdfDataSourceZoomedInView[pageIndex];

                        if (destinationItem != null)
                        {
                            e.DestinationItem.Item = destinationItem;
                        }
                    }
                    // Transitioning from Zooomed In View to Zoomed Out View 
                    else
                    {
                        // Getting destination item from Zoomed-In-View 
                        PdfPageViewModel destinationItem = (PdfPageViewModel)this.pdfDataSourceZoomedOutView[pageIndex];
                        
                        if (destinationItem != null)
                        {
                            e.DestinationItem.Item = destinationItem;
                        }
                    }
                }
            }
        }
        
        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            //var pageIndex = Int32.Parse(textBox.Text);
            var pdfPageViewModel = (PdfPageViewModel)textBox.DataContext;
            textBox.Text = (pdfPageViewModel.PageIndex + 1).ToString();
        }
    }
    
}
