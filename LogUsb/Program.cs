using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LogUsb
{
    class Program
    {
        public static string writePath = @"D:\LogUsb";
        static ManagementEventWatcher w = null;
        private static object sync = new object();

        static void Main(string[] args)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(writePath);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
                Console.WriteLine($"Файл лога создан в : {writePath}");
            }
            else
                Console.WriteLine($"Файл лога находится в : {writePath}");
            AddInsetUSBHandler();
            AddRemoveUSBHandler();
            while (true) { }
        }

        public static void Write(_Exception ex)
        {
            try
            {
                string pathLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
                if (!Directory.Exists(pathLog))
                    Directory.CreateDirectory(pathLog);
                string filename = Path.Combine(pathLog, string.Format("{0}_{1:dd.MM.yyy}.log", AppDomain.CurrentDomain.FriendlyName, DateTime.Now));
                string fullText = string.Format("[{0:dd.MM.yyy HH:mm:ss.fff}] [{1}.{2}()] {3}\r\n", DateTime.Now, ex.TargetSite.DeclaringType, ex.TargetSite.Name, ex.Message);
                lock (sync)
                {
                    File.AppendAllText(filename, fullText, Encoding.GetEncoding("Windows-1251"));
                }
            }
            catch
            {

            }
        }

        public static async Task Logs(string str)
        {
            string date = DateTime.Now.ToString();
            try
            {
                using (StreamWriter sw = new StreamWriter($"{writePath}\\Log.txt", true, System.Text.Encoding.Default))
                {
                    await sw.WriteLineAsync(str + DateTime.Now);
                    sw.Close();
                }
                using (StreamWriter sw = new StreamWriter($"{writePath}\\Log.txt", true, System.Text.Encoding.Default))
                {
                    await sw.WriteLineAsync(str + DateTime.Now + "\n");
                    await sw.WriteAsync(str + DateTime.Now + "\n");
                    sw.Close();
                }
                Console.WriteLine("Запись в лог выполнена");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void AddRemoveUSBHandler()
        {
            WqlEventQuery q;
            ManagementScope scope = new ManagementScope("root\\CIMV2");
            scope.Options.EnablePrivileges = true;
            try
            {
                q = new WqlEventQuery();
                q.EventClassName = "__InstanceDeletionEvent";
                q.WithinInterval = new TimeSpan(0, 0, 3);
                q.Condition = @"TargetInstance ISA 'Win32_USBHub'";
                w = new ManagementEventWatcher(scope, q);
                w.EventArrived += new EventArrivedEventHandler(USBRemoved);
                w.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (w != null)
                    w.Stop();
            }
        }
        static void AddInsetUSBHandler()
        {
            WqlEventQuery q;
            ManagementScope scope = new ManagementScope("root\\CIMV2");
            scope.Options.EnablePrivileges = true;
            try
            {
                q = new WqlEventQuery();
                q.EventClassName = "__InstanceCreationEvent";
                q.WithinInterval = new TimeSpan(0, 0, 3);
                q.Condition = @"TargetInstance ISA 'Win32_USBHub'";
                w = new ManagementEventWatcher(scope, q);
                w.EventArrived += new EventArrivedEventHandler(USBAdded);
                w.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (w != null)
                    w.Stop();
            }
        }

        public static void USBAdded(object sender, EventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("USB устройство вставлено " + DateTime.Now);
            Task.Run(()=> Logs("USB устройство вставлено " + DateTime.Now));
        }
        public static void USBRemoved(object sender, EventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("USB устройство извлечено " + DateTime.Now);
            Task.Run(() => Logs("USB устройство извлечено " + DateTime.Now));
        }
    }
}
