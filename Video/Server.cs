#region using
using GEUTEBRUECK.Gng.Wrapper.DBI;
using Hit.LoggerLibrary;
using System;
using System.Reflection;
#endregion

namespace Hit.GutService.Video
{
    class Server
    {
        private static GngServer m_GngServer;
        private static string server = "vita.hit.si";
        private static string username = "sysadmin";
        private static string password = "masterkey";

        public static GngServer Connect()
        {
            if (m_GngServer == null)
            {
                // create a server object instance
                m_GngServer = new GngServer();

                String EncodedPassword = DBIHelperFunctions.EncodePassword(password);
                // initialize the connection parameters  
                using (GngServerConnectParams ConnectParams = new GngServerConnectParams(server, username, EncodedPassword))
                {
                    m_GngServer.SetConnectParams(ConnectParams);
                    // connect to the server
                    GngServerConnectResult connectResult = m_GngServer.Connect();

                    if (connectResult == GngServerConnectResult.connectOk)
                        Logger.INFO(MethodBase.GetCurrentMethod(), connectResult.ToString());
                    else
                        Logger.ERROR(MethodBase.GetCurrentMethod(), "Napaka pri connect na GngServer "  + server + " Error=" + connectResult.ToString(), null);

                    return m_GngServer;
                }
            }

            return m_GngServer;

        }

        public static void Disconnect()
        {
            if (m_GngServer != null)
            {
                m_GngServer.Disconnect(System.Threading.Timeout.Infinite);
                m_GngServer.Dispose();
                m_GngServer = null;
            }
        }
    }
}
