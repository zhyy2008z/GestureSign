﻿using System;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using GestureSign.Common.Applications;
using GestureSign.Common.Configuration;
using GestureSign.Common.Gestures;
using GestureSign.Common.Input;
using GestureSignDaemon.Input;
using GestureSign.Common.InterProcessCommunication;

namespace GestureSignDaemon
{
    class MessageProcessor : IMessageProcessor
    {
        internal static event EventHandler<Point> OnGotTouchPoint;
        public void ProcessMessages(NamedPipeServerStream server)
        {
            BinaryFormatter binForm = new BinaryFormatter();
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    server.CopyTo(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    object data = binForm.Deserialize(memoryStream);

                    if (data is string)
                    {
                        var message = data.ToString();
                        TouchCapture.Instance.MessageWindow.Invoke(new Action(() =>
                        {
                            switch (message)
                            {
                                //case "Guide":
                                //    GestureSignDaemon.Input.TouchCapture.Instance.MessageWindow.PointsIntercepted += InitializationRatio.MessageWindow_PointsIntercepted;
                                //    break;
                                case "StartTeaching":
                                    {
                                        if (TouchCapture.Instance.State == CaptureState.UserDisabled)
                                            TrayManager.Instance.ToggleDisableGestures();
                                        if (!AppConfig.Teaching)
                                            TrayManager.Instance.StartTeaching();
                                        break;
                                    }
                                case "EnableTouchCapture":
                                    TouchCapture.Instance.EnableTouchCapture();
                                    break;
                                case "DisableTouchCapture":
                                    TouchCapture.Instance.DisableTouchCapture();
                                    break;
                                case "LoadApplications":
                                    ApplicationManager.Instance.LoadApplications().Wait();
                                    break;
                                case "LoadGestures":
                                    GestureManager.Instance.LoadGestures().Wait();
                                    break;
                            }
                        }));
                    }
                    else if (data is Point)
                    {
                        if (OnGotTouchPoint != null)
                        {
                            OnGotTouchPoint(this, (Point)data);
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }
        }

    }
}
