using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using NSoup;
using NSoup.Nodes;
using NSoup.Select;
using Application = System.Windows.Application;
using ContextMenu = System.Windows.Forms.ContextMenu;
using MenuItem = System.Windows.Forms.MenuItem;

namespace MaskNotifier
{
    public class NotifyManager
    {
        private static NotifyManager INSTANCE;
        private static MainWindow mainWindow;
        private static Thread masterMonitor;
        
        NotifyIcon notifyHandler;
        ContextMenu notifyMenu;
        MenuItem notifyMenuExit;

        private static bool MANAGER_OFF = false;

        private static MaskInfo lastMaskInfo;
        private static EventHandler ballonClickHandler;

        public static NotifyManager GetInstance()
        {
            return INSTANCE ?? (INSTANCE = new NotifyManager());
        }

        public void StartService()
        {
            if (mainWindow == null) mainWindow = new MainWindow();
            InitializeTrayService();
            Initialize();
        }

        public void StopService()
        {
            mainWindow?.Close();
            MANAGER_OFF = true;
        }
        
        public void ShowWindow()
        {
            if (mainWindow == null) mainWindow = new MainWindow();
            mainWindow.Show();
            mainWindow.WindowState = WindowState.Normal;
        }

        public void HideWindow()
        {
            mainWindow?.Hide();
        }

        private void InitializeTrayService()
        {
            if (notifyMenu == null) notifyMenu = new ContextMenu();
            if (notifyMenuExit == null) notifyMenuExit = new MenuItem();
            notifyMenuExit.Index = 0;
            notifyMenuExit.Text = "EXIT";
            notifyMenuExit.Click += delegate(object sender, EventArgs args)
            {
                StopService();
            };
            notifyMenu.MenuItems.Add(notifyMenuExit);
            if (notifyHandler == null) notifyHandler = new NotifyIcon();
            notifyHandler.Icon = new System.Drawing.Icon(@"Resources/tray.ico");
            notifyHandler.Visible = true;
            notifyHandler.Text = "MASK NOTIFIER";
            notifyHandler.DoubleClick += delegate(object senders, EventArgs args)
            {
                ShowWindow();
            };
            notifyHandler.ContextMenu = notifyMenu;
        }

        private void Initialize()
        {
            Thread well1Thread = new Thread(MonitorMask_WELL1);
            well1Thread.Start();
        }

        private void MonitorMask_WELL1() // URL : 
        {
            showMessage(null);
            while (true)
            {
                if (MANAGER_OFF) break;
                string html = GetRequest("http://www.welkeepsmall.com/shop/shopbrand.html?type=X&xcode=023");
                Document doc = NSoupClient.Parse(html);
                Elements elements = doc.Select(".box");

                foreach (var element in elements)
                {
                    Element detailInfo = element.Select(".info").First;
                    var soldout = detailInfo.Select(".soldout");
                    if (soldout == null || soldout.IsEmpty)
                    {
                        MaskInfo maskInfo = new MaskInfo();
                        maskInfo.URL = "http://www.welkeepsmall.com" + element.Select("a").Attr("href");
                        maskInfo.TITLE = detailInfo.Select(".dsc").Text;
                        showMessage(maskInfo);
                        continue;
                    }
                    if (!soldout.Text.Contains("OUT"))
                    {
                        MaskInfo maskInfo = new MaskInfo();
                        maskInfo.URL = "http://www.welkeepsmall.com" + element.Select("a").Attr("href");
                        maskInfo.TITLE = detailInfo.Select(".dsc").Text;
                        showMessage(maskInfo);
                    }
                }
                
                Thread.Sleep(2000);
            }
        }

        public void showMessage(MaskInfo maskInfo)
        {
            if (maskInfo == null)
            {
                notifyHandler.ShowBalloonTip(10000, "마스크 알림 시작", "웰킵스 모니터링중", ToolTipIcon.Info);
                return;
            }
            if (ballonClickHandler != null) notifyHandler.BalloonTipClicked -= ballonClickHandler;
            ballonClickHandler = onMessageClicked;
            notifyHandler.BalloonTipClicked += ballonClickHandler;
            lastMaskInfo = maskInfo;
            notifyHandler.ShowBalloonTip(10000, "마스크 알림", maskInfo.TITLE, ToolTipIcon.Info);
        }

        public static void onMessageClicked(object sender, EventArgs e)
        {
            if (lastMaskInfo == null) return;
            System.Diagnostics.Process.Start(lastMaskInfo.URL);
        }
        
        public static string GetRequest(String url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.Default))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        
    }
    
    public class MaskInfo
    {
        public string TITLE;
        public string URL;
    }

}