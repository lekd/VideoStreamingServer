using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HttpPanoVideoStreamingServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CamRetriever panoCam;
        CamRetriever tableCam;
        bool isStreaming = false;
        StreamingServer panoServer;
        StreamingServer tableCamServer;
        public MainWindow()
        {
            InitializeComponent();
            cbTableCam.SelectionChanged += CbTableCam_SelectionChanged;
            string[] cameraDevices = CamRetriever.getCameraList();
            cbTableCam.ItemsSource = cameraDevices;
            if(cameraDevices.Length > 0)
            {
                cbTableCam.SelectedIndex = 0;
            }
            StartPanoCam();
        }

        private void CbTableCam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StartTableCam();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
            CloseServers();
            CloseCameras();
        }

        private void btnStartStop_Click(object sender, RoutedEventArgs e)
        {
            
            if (!isStreaming)
            {    
                panoServer = new StreamingServer(panoCam.GrabFrames());
                panoServer.Start(Properties.Settings.Default.Pano_Port);
                tableCamServer = new StreamingServer(tableCam.GrabFrames());
                tableCamServer.Start(Properties.Settings.Default.TableVideo_Port);
                btnStartStop.Content = "Stop";
                cbTableCam.IsEnabled = false;
            }
            else
            {
                CloseServers();
                
                cbTableCam.IsEnabled = true;
                btnStartStop.Content = "Stream";
            }
            isStreaming = !isStreaming;
        }

        private void NewFrameAvailableEvent(int camIndex, Bitmap frameBmp)
        {
            if (camIndex == CamRetriever.getPanoCamIndex())
            {
                Action displayaction = delegate
                {
                    BitmapImage imgSrc = Utilities.ToBitmapImage(frameBmp, ImageFormat.Jpeg);
                    panoDisplayer.Source = imgSrc;
                };
                panoDisplayer.Dispatcher.Invoke(displayaction);
            }
            else
            {
                frameBmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                Action displayaction = delegate
                {
                    BitmapImage imgSrc = Utilities.ToBitmapImage(frameBmp, ImageFormat.Jpeg);
                    tableDisplayer.Source = imgSrc;
                };
                tableDisplayer.Dispatcher.Invoke(displayaction);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
        void CloseCameras()
        {
            panoCam?.Close();
            panoCam = null;
            tableCam?.Close();
            tableCam = null;
        }
        void CloseServers()
        {
            panoServer?.Dispose();
            panoServer = null;
            tableCamServer?.Dispose();
            tableCamServer = null;
        }
        void StartPanoCam()
        {
            panoCam = new CamRetriever(CamRetriever.getPanoCamIndex());
            panoCam.CropArea = new RectangleF(Properties.Settings.Default.Pano_Crop_Left,
                                                Properties.Settings.Default.Pano_Crop_Top,
                                                Properties.Settings.Default.Pano_Crop_Width,
                                                Properties.Settings.Default.Pano_Crop_Height);
            panoCam.NewFrameAvailableEvent += NewFrameAvailableEvent;
            panoCam.Start();
        }
        void StartTableCam()
        {
            tableCam?.Close();
            int tableCamIndex = cbTableCam.SelectedIndex >= 0 ? cbTableCam.SelectedIndex : 0;
            tableCam = new CamRetriever(tableCamIndex);
            tableCam.CropArea = new RectangleF(Properties.Settings.Default.Table_Crop_Left,
                                                Properties.Settings.Default.Table_Crop_Top,
                                                Properties.Settings.Default.Table_Crop_Width,
                                                Properties.Settings.Default.Table_Crop_Height);
            tableCam.NewFrameAvailableEvent += NewFrameAvailableEvent;
            tableCam.Start();
        }
    }
}
