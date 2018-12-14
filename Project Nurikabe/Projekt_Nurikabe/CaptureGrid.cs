using Emgu.CV.UI;
using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Emgu.CV.Structure;
using System.Windows.Forms;
using Emgu.CV.Util;
using System.Drawing;

namespace Projekt_Nurikabe {
    class CaptureGrid {
        
        private ImageBox imageBoxMain;
        private VideoCapture camera;
        private bool _enable;
        private Thread _refreshThread;        
        private readonly MethodInvoker _refreshMethodInvoker;

        public Mat _frameImage;
        private Image<Bgr, byte> processedFrameImage;

        private Detect detect;

        public CaptureGrid(ImageBox imageBoxMain, ImageBox resultImageBox) {
            
            this.imageBoxMain = imageBoxMain;
            this.imageBoxMain.SizeMode = PictureBoxSizeMode.StretchImage;
            resultImageBox.SizeMode = PictureBoxSizeMode.StretchImage;

            detect = new Detect(resultImageBox);

            camera = new VideoCapture(0);
            _refreshMethodInvoker = Refresh;
            //_refreshThread = new Thread(CallBack);
        }

        private void Refresh() {

            processedFrameImage = _frameImage.ToImage<Bgr, byte>();
            _frameImage = detect.FindGridAndSolve(processedFrameImage);

            imageBoxMain.Image = _frameImage;            
        }

        private void CallBack() {
            while (_enable) {
                _frameImage = camera.QueryFrame();

                if (!imageBoxMain.InvokeRequired) {
                    imageBoxMain.Image = _frameImage;
                } else {
                    imageBoxMain.Invoke(_refreshMethodInvoker);
                }
            }
        }

        public void Start() {

            if (_enable) {
                return;
            }
            _enable = true;
            _refreshThread = new Thread(CallBack);
            _refreshThread.Start();
        }

        public void Stop(bool force) {

            _enable = false;
            if (force) {
                _refreshThread.Abort();
            }

        }

        public void SaveImage() {

            _frameImage.Save("captured_image.png");
        }


    }
}
