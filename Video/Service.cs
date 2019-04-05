#region using
using GEUTEBRUECK.Gng.Wrapper.Actions;
using GEUTEBRUECK.Gng.Wrapper.Actions.ViewerActions;
using GEUTEBRUECK.Gng.Wrapper.DBI;
using GEUTEBRUECK.Gng.Wrapper.MediaPlayer;
using Hit.GutService.Hubs;
using Hit.LoggerLibrary;
using System;
using System.Reflection;
using System.Threading;
#endregion

namespace Hit.GutService.Video
{
    public static class Service
    {
        private static OffscreenViewer v = new OffscreenViewer();
        private static Thread sendingFrames;
        private static bool play;
        private const long MediaChID = 1; //camera ID

        private static void SendingFrames()
        {
            VideoHub videoHub = new VideoHub();

            while (play)
            {
                try
                {
                    Thread.Sleep(GetTimeSleep()); // we calculate time span for 25FPS 
                    videoHub.Send(v.GetFrame());
                }
                catch (Exception ex)
                {
                    Logger.ERROR(MethodBase.GetCurrentMethod(), "Error sending frames", ex);
                }
            }
        }

        private static int GetTimeSleep()
        {
            int sleep = 32; //default

            if (v.images.Count > 50)
                sleep = 30;
            else if (v.images.Count <= 50 && v.images.Count >= 30)
                sleep = 32;
            else if (v.images.Count < 30 && v.images.Count >= 20)
                sleep = 35;
            else if (v.images.Count < 20 && v.images.Count >= 10)
                sleep = 40;
            else if (v.images.Count < 10)
                sleep = 42;

            return sleep;
        }

        public static void Play()
        {

            v.Viewer(GngViewerPlayMode.pmPlayForward, MediaChID);
            play = true;

            //wait until we have images in queue
            while (v.images.Count < 50) ;

            sendingFrames = new Thread(() => SendingFrames());
            sendingFrames.Start();
        }

        public static void Stop()
        {
            try
            {
                play = false;
                v.Viewer(GngViewerPlayMode.pmPlayStop, MediaChID);
                sendingFrames.Abort();

            }
            catch (Exception)
            {
                //todo
            }
            finally
            {
                //v.Dispose();
                //Server.Disconnect();
                //GC.SuppressFinalize(v);
            }
        }

        public static void Picture()
        {
            //Send first frame

            VideoHub videoHub = new VideoHub();

            v.Viewer(GngViewerPlayMode.pmPlayForward, MediaChID);

            //wait until we have images in queue
            while (v.images.Count < 1) ;

            videoHub.Send(v.GetFrame());

            v.Dispose();
        }

        public static void Dispose()
        {
            try
            {
                play = false;
                sendingFrames.Abort();
            }
            catch (Exception)
            {
                //todo
            }
            finally
            {
                v.Dispose();
                Server.Disconnect();
                GC.SuppressFinalize(v);
            }
        }

        //TODO
        public static void Gplc()
        {
            GngMediaChannelID gngMediaChannelID = new GngMediaChannelID(1);

            GCore_Action_ViewerSetPlayMode gCore_Action_ViewerSetPlayMode
                                         = new GCore_Action_ViewerSetPlayMode(gngMediaChannelID, GngPlcViewerPlayMode.vpmPlayForward, 1);


        }
    }
}
