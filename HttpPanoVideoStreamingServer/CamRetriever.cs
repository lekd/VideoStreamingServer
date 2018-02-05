using DirectShowLib;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpPanoVideoStreamingServer
{
    public delegate void NewFrameAvailableHandler(int camIndex, Bitmap frameData);
    public class CamRetriever
    {
        const int UPDATE_INTERVAL = 80;
        public event NewFrameAvailableHandler NewFrameAvailableEvent;
        protected VideoCapture camCapture;
        private DateTime lastUpdate;
        public int CamIndex { get; set; }
        public Bitmap CurrentFrame { get; set; }
        public RectangleF CropArea { get; set; }
        private bool isRunning = false;
        public CamRetriever(int camIndex)
        {
            CamIndex = camIndex;
            lastUpdate = DateTime.Now;
            CropArea = new RectangleF(0, 0, 1, 1);
        }
        public void Start()
        {
            if (CamIndex >= 0)
            {
                camCapture = new VideoCapture(CamIndex);
                if (camCapture != null && camCapture.Ptr != IntPtr.Zero)
                {
                    camCapture.ImageGrabbed += ProcessFrame;
                    camCapture.Start();
                    isRunning = true;
                }
            }
        }
        public void Close()
        {
            isRunning = false;
            if (camCapture != null)
            {
                camCapture.Stop();
                camCapture.Dispose();
            }
        }
        Mat originFrame = new Mat();
        private void ProcessFrame(object sender, EventArgs arg)
        {
            if (camCapture != null && camCapture.Ptr != IntPtr.Zero)
            {
                try
                {
                    camCapture.Retrieve(originFrame, 0);
                    Bitmap bmp = originFrame.Bitmap;
                    if (CropArea != null)
                    {
                        bmp = Utilities.CropBitmap(bmp, CropArea.Left, CropArea.Top, CropArea.Width, CropArea.Height);
                    }
                    CurrentFrame = bmp;
                    if (NewFrameAvailableEvent != null)
                    {
                        NewFrameAvailableEvent(CamIndex, bmp);
                    }
                }
                catch
                {

                }
                
            }
        }
        public IEnumerable<Image> GrabFrames()
        {
            while (isRunning)
            {
                yield return CurrentFrame;
            }
            yield break;
        }
        public static string[] getCameraList()
        {
            DsDevice[] capDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            string[] cameraNames = new string[capDevices.Length];
            for (int i = 0; i < capDevices.Length; i++)
            {
                cameraNames[i] = capDevices[i].Name;
            }
            return cameraNames;
        }
        public static int getPanoCamIndex()
        {
            DsDevice[] capDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            for (int i = 0; i < capDevices.Length; i++)
            {
                if (capDevices[i].Name.Contains("UVC Capture") && capDevices[i].Name.Contains("RICOH"))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
