#region using
using GEUTEBRUECK.Gng.Wrapper.MediaPlayer;
using Hit.LoggerLibrary;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
#endregion

namespace Hit.GutService.Video
{
    public class OffscreenViewer:IDisposable
    {

        #region  declarations
        private GngOffscreenViewer m_GngOffscreenViewer;
        private GngDecompBuffer m_GngDecompBuffer1;   // handles to two decompression buffer objects
        private GngDecompBuffer m_GngDecompBuffer2;   // during the image in one decompression buffer is rendered or processed,
        private GngDecompBuffer m_NewPicDecompBuffer; // the next image can allready be decompressed in the other
        private object m_CSBuffer = new Object();     // make the Flag "m_NewPicDecompressed" thread-safe
        private bool m_NewPicDecompressed = false;    // decompression buffer
        private object m_CSBuffer1 = new Object();    // make the first decompression buffer thread-safe
        private object m_CSBuffer2 = new Object();    // make the second decompression buffer thread-safe
        public CircularBuffer<byte[]> images = new CircularBuffer<byte[]>(10000000); //Buffer qeueu for smooth playing
        private const int width  = 768;
        private const int heigth = 576;
        #endregion

        public void StartPlay(GngViewerConnectData connectData)
        {
            if (m_GngOffscreenViewer == null)
                CreateOffscreenViewer();

            m_GngOffscreenViewer.ConnectDB(connectData, GngViewerPlayMode.pmPlayForward);
        }

        private void CreateOffscreenViewer()
        {
            DestroyOffscreenViewer();
            if (m_GngOffscreenViewer == null)
            {
                // create two decompression buffer object instances
                m_GngDecompBuffer1 = new GngDecompBuffer();
                m_GngDecompBuffer1.SetBufferSize(width, heigth, GngDecompBufferFormat.dbfRGB32);
                m_GngDecompBuffer2 = new GngDecompBuffer();
                m_GngDecompBuffer2.SetBufferSize(width, heigth, GngDecompBufferFormat.dbfRGB32);

                // create the offscreen viewer object instance
                m_GngOffscreenViewer = new GngOffscreenViewer(m_GngDecompBuffer1);

                m_NewPicDecompBuffer = m_GngDecompBuffer1;

                m_GngOffscreenViewer.GetTextParams(out GngViewTextParams Params);
                // display a timestamp in the viewer
                Params.InsertPicInfo = true;
                Params.FontSize = 20;
                m_GngOffscreenViewer.SetTextParams(Params);

                m_GngOffscreenViewer.SetOffscreenViewerSize(width, heigth, true); 
                m_GngOffscreenViewer.Refresh();

                // set callbacks of the offscreen viewer objects
                OffscreenViewerAcceptCallbackDelegate OffscreenViewerAcceptCallbackDelegateInstance = new OffscreenViewerAcceptCallbackDelegate(GngOffscreenViewer_AcceptCallbackDelegate);
                m_GngOffscreenViewer.SetOffscreenViewerAcceptCallBack(OffscreenViewerAcceptCallbackDelegateInstance);
                OffscreenViewerCallbackDelegate OffscreenViewerCallbackDelegateInstance = new OffscreenViewerCallbackDelegate(GngOffscreenViewer_CallbackDelegate);
                m_GngOffscreenViewer.SetOffscreenViewerCallBack(OffscreenViewerCallbackDelegateInstance);

            }
        }

        private void DestroyOffscreenViewer()
        {
            if (m_GngOffscreenViewer != null)
            {
                m_GngOffscreenViewer.Disconnect(true);
                m_GngOffscreenViewer.CloseCustomDrawCallBack();
                m_GngOffscreenViewer.CloseOffscreenViewerAcceptCallBack();
                m_GngOffscreenViewer.CloseOffscreenViewerCallBack();
                m_GngOffscreenViewer.Dispose();
                m_GngOffscreenViewer = null;
            }
            m_NewPicDecompBuffer = null;
            if (m_GngDecompBuffer1 != null)
            {
                m_GngDecompBuffer1.Dispose();
                m_GngDecompBuffer1 = null;
            }
            if (m_GngDecompBuffer2 != null)
            {
                m_GngDecompBuffer2.Dispose();
                m_GngDecompBuffer2 = null;
            }
        }

        private void GngOffscreenViewer_CallbackDelegate(OffscreenViewerCallbackEventArgs e)
        {
            // check whether image in buffer is valid
            if (e.PicData.GngMediaDesc != null)
            {
                // new decompressed picture available
                lock (m_CSBuffer)
                {
                    m_NewPicDecompressed = true;
                    if (e.OffscreenBufferHandle.IsNativeBufferIdentical(m_GngDecompBuffer2))
                    {
                        Monitor.Exit(m_CSBuffer2);
                        m_NewPicDecompBuffer = m_GngDecompBuffer2;
                    }
                    else
                    {
                        Monitor.Exit(m_CSBuffer1);
                        m_NewPicDecompBuffer = m_GngDecompBuffer1;
                    }
                }

                CreateFrame();

                e.ImageWasHandled = true;
            }
        }

        private void GngOffscreenViewer_AcceptCallbackDelegate(OffscreenViewerAcceptCallbackEventArgs e)
        {
            // check whether image in buffer is valid
            if (e.PicData.GngMediaDesc != null)
            {
                // switch between the both decompression buffers
                if (e.OffscreenBufferHandle.IsNativeBufferIdentical(m_GngDecompBuffer2))
                {
                    Monitor.Enter(m_CSBuffer1);
                    e.OffscreenBufferHandle = m_GngDecompBuffer1;
                }
                else
                {
                    Monitor.Enter(m_CSBuffer2);
                    e.OffscreenBufferHandle = m_GngDecompBuffer2;
                }
                e.DoDecompressImage = true;
            }
        }

        private void CreateFrame()
        {
            // check if new decompressed picture available
            lock (m_CSBuffer)
            {
                if (m_NewPicDecompressed)
                {
                    if (m_NewPicDecompBuffer.IsNativeBufferIdentical(m_GngDecompBuffer2))
                        Monitor.Enter(m_CSBuffer2);
                    else
                        Monitor.Enter(m_CSBuffer1);

                    IntPtr BufPointer;
                    Int64 BufWidth, BufHeight, BufPitch;
                    m_NewPicDecompBuffer.GetBufPointer(out BufPointer, out BufWidth, out BufHeight, out BufPitch);

                    SendFrameToQueue((int)BufWidth, (int)BufHeight, (int)BufPitch, BufPointer);

                    if (m_NewPicDecompBuffer.IsNativeBufferIdentical(m_GngDecompBuffer2))
                        Monitor.Exit(m_CSBuffer2);
                    else
                        Monitor.Exit(m_CSBuffer1);

                    m_NewPicDecompressed = false;

                }
            }
        }

        private void SendFrameToQueue(int BufWidth, int BufHeight, int BufPitch, IntPtr BufPointer)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                using (Bitmap bmp = new Bitmap(BufWidth, BufHeight, BufPitch, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, BufPointer))
                //using (Bitmap _bmp = new Bitmap(bmp, 640, 360))
                {
                    bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    //send frame to queue
                    images.Enqueue(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                Logger.ERROR(MethodBase.GetCurrentMethod(), "Create image error", ex);
            }
        }

        public byte[] GetFrame()
        {
            Console.WriteLine(images.Count);
            return images.Dequeue();
        }

        public void Dispose()
        {
            DestroyOffscreenViewer();
            images.Clear();
        }

    }
}
