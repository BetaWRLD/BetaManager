using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using BetaManager.Classes;
using BetaManager.Downloader;
using BetaManager.Models;
using BetaManager.Properties;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace BetaManager
{
    public class Functions
    {
        const uint LOCKFILE_FAIL_IMMEDIATELY = 0x00000001;
        const uint LOCKFILE_EXCLUSIVE_LOCK = 0x00000002;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool LockFileEx(
            IntPtr hFile,
            uint dwFlags,
            uint dwReserved,
            uint nNumberOfBytesToLockLow,
            uint nNumberOfBytesToLockHigh,
            [In] ref System.Threading.NativeOverlapped lpOverlapped
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool UnlockFile(
            IntPtr hFile,
            uint dwFileOffsetLow,
            uint dwFileOffsetHigh,
            uint nNumberOfBytesToUnlockLow,
            uint nNumberOfBytesToUnlockHigh
        );

        [DllImport("dwmapi.dll")]
        public static extern int DwmGetColorizationColor(
            out int pcrColorization,
            [MarshalAs(UnmanagedType.Bool)] out bool pfOpaqueBlend
        );

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr ShellExecute(
            IntPtr hwnd,
            string lpOperation,
            string lpFile,
            string lpParameters,
            string lpDirectory,
            int nShowCmd
        );

        public void UpdateColor(System.Windows.Media.Color newColor) { }

        public static void CopyFile(string sourceFilePath, string destinationFilePath)
        {
            using (
                FileStream sourceStream = new FileStream(
                    sourceFilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite
                )
            )
            {
                using (
                    FileStream destinationStream = new FileStream(
                        destinationFilePath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.ReadWrite
                    )
                )
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;

                    while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        destinationStream.Write(buffer, 0, bytesRead);
                    }
                    destinationStream.Close();
                }
                sourceStream.Close();
            }
        }

        public static bool UnlockFile(string filePath)
        {
            try
            {
                using (
                    var fileStream = new FileStream(
                        filePath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite
                    )
                )
                {
                    IntPtr handle = fileStream.SafeFileHandle.DangerousGetHandle();

                    // Create a new NativeOverlapped with fields set to 0
                    System.Threading.NativeOverlapped overlapped =
                        new System.Threading.NativeOverlapped();
                    if (
                        LockFileEx(
                            handle,
                            LOCKFILE_FAIL_IMMEDIATELY | LOCKFILE_EXCLUSIVE_LOCK,
                            0,
                            uint.MaxValue,
                            uint.MaxValue,
                            ref overlapped
                        )
                    )
                    {
                        UnlockFile(handle, 0, 0, uint.MaxValue, uint.MaxValue);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return false;
        }

        public static string TruncateText(string text, int maxLength)
        {
            if (text.Length <= maxLength)
            {
                return text;
            }
            else
            {
                return text.Substring(0, maxLength);
            }
        }

        public static async Task SleepAsync(int milliseconds)
        {
            if (milliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(milliseconds),
                    "Milliseconds should be greater than 0."
                );
            }

            await Task.Delay(milliseconds);
        }

        public static int? ToInt(string num)
        {
            try
            {
                if (num == null || num.Length < 1)
                    return null;
                return Convert.ToInt32(num);
            }
            catch
            {
                return null;
            }
        }

        public async Task UpdateText(UIElement element, string value, int speed = 350)
        {
            await FadeOut(element, speed);
            ((TextBlock)element).Text = value;
            await FadeIn(element, speed);
        }

        public async Task FadeIn(UIElement element, int time = 150)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation
            {
                From = element.Opacity,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(time)
            };

            var tcs = new TaskCompletionSource<bool>();
            doubleAnimation.Completed += (sender, e) =>
            {
                tcs.SetResult(true);
            };

            element.BeginAnimation(UIElement.OpacityProperty, doubleAnimation);

            await tcs.Task;
        }

        public async Task FadeOut(UIElement element, int time = 150)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation
            {
                From = element.Opacity,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(time)
            };

            var tcs = new TaskCompletionSource<bool>();
            doubleAnimation.Completed += (sender, e) =>
            {
                tcs.SetResult(true);
            };

            element.BeginAnimation(UIElement.OpacityProperty, doubleAnimation);

            await tcs.Task;
        }

        public void DefaultColor()
        {
            UIColors.MainColor = (System.Windows.Media.Color)
                System.Windows.Media.ColorConverter.ConvertFromString("#FAF9F6");

            System.Windows.Application.Current.Resources["mainColorColor"] =
                (System.Windows.Media.Color)
                    System.Windows.Media.ColorConverter.ConvertFromString(WindowsColor());
            System.Windows.Application.Current.Resources["titleColor1"] = new SolidColorBrush(
                (System.Windows.Media.Color)
                    System.Windows.Media.ColorConverter.ConvertFromString("#F1F1F1")
            );
            System.Windows.Application.Current.Resources["titleColor2"] = new SolidColorBrush(
                (System.Windows.Media.Color)
                    System.Windows.Media.ColorConverter.ConvertFromString("White")
            );
            System.Windows.Application.Current.Resources["titleColor3"] = new SolidColorBrush(
                (System.Windows.Media.Color)
                    System.Windows.Media.ColorConverter.ConvertFromString("White")
            );
            System.Windows.Application.Current.Resources["panelColor"] = new SolidColorBrush(
                (System.Windows.Media.Color)
                    System.Windows.Media.ColorConverter.ConvertFromString("#313131")
            );
            System.Windows.Application.Current.Resources["mainColor"] = new SolidColorBrush(
                (System.Windows.Media.Color)
                    System.Windows.Media.ColorConverter.ConvertFromString(WindowsColor())
            );
            System.Windows.Application.Current.Resources["panelColorLight"] = new SolidColorBrush(
                (System.Windows.Media.Color)
                    System.Windows.Media.ColorConverter.ConvertFromString("#515151")
            );
            System.Windows.Application.Current.Resources["plainTextColor"] = new SolidColorBrush(
                (System.Windows.Media.Color)
                    System.Windows.Media.ColorConverter.ConvertFromString("#A0A0A0")
            );
            System.Windows.Application.Current.Resources["primaryBackColorSolid"] =
                new SolidColorBrush(
                    (System.Windows.Media.Color)
                        System.Windows.Media.ColorConverter.ConvertFromString("#111111")
                );
            System.Windows.Application.Current.Resources["secondaryBackColorSolid"] =
                new SolidColorBrush(
                    (System.Windows.Media.Color)
                        System.Windows.Media.ColorConverter.ConvertFromString("#212121")
                );
            System.Windows.Application.Current.Resources["primaryBackColor"] =
                (System.Windows.Media.Color)
                    System.Windows.Media.ColorConverter.ConvertFromString("#111111");
            System.Windows.Application.Current.Resources["secondaryBackColor"] =
                (System.Windows.Media.Color)
                    System.Windows.Media.ColorConverter.ConvertFromString("#212121");
            System.Windows.Application.Current.Resources["secondaryBackColorAlpha"] =
                (System.Windows.Media.Color)
                    System.Windows.Media.ColorConverter.ConvertFromString("#00212121");
        }

        public static long DirectorySize(string folder)
        {
            long size = 0;
            DirectoryInfo d = new DirectoryInfo(folder);
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirectorySize(di.FullName);
            }
            return size;
        }

        public string WindowsColor()
        {
            int colorizationColor;
            bool isOpaqueBlend;

            int result = DwmGetColorizationColor(out colorizationColor, out isOpaqueBlend);

            if (result == 0)
            {
                System.Drawing.Color themeColor = System.Drawing.Color.FromArgb(colorizationColor);
                return "#"
                    + themeColor.R.ToString("X2")
                    + themeColor.G.ToString("X2")
                    + themeColor.B.ToString("X2");
            }
            else
            {
                return "FAF9F6";
            }
        }

        public static void NotifyRunningInstance(string url)
        {
            try
            {
                using (
                    NamedPipeClientStream pipeClient = new NamedPipeClientStream(
                        ".",
                        "BetaManagerPipe",
                        PipeDirection.Out
                    )
                )
                {
                    pipeClient.Connect(3000);
                    using (StreamWriter writer = new StreamWriter(pipeClient))
                    {
                        writer.WriteLine(url);
                    }
                }
            }
            catch (Exception ex)
            {
                Process.GetCurrentProcess().Kill();
            }
        }

        public static List<VPNInterfaceModel> GetNetworkInterfaces()
        {
            List<VPNInterfaceModel> _tempList = new List<VPNInterfaceModel>();
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in networkInterfaces)
            {
                if (
                    ni.OperationalStatus == OperationalStatus.Up
                    && (ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                )
                    _tempList.Add(
                        new VPNInterfaceModel { Name = ni.Name, Description = ni.Description, }
                    );
            }
            return _tempList;
        }

        public static void ListenForProtocol()
        {
            while (true)
            {
                using (
                    NamedPipeServerStream pipeServer = new NamedPipeServerStream(
                        "BetaManagerPipe",
                        PipeDirection.In
                    )
                )
                {
                    pipeServer.WaitForConnection();
                    using (StreamReader reader = new StreamReader(pipeServer))
                    {
                        string url = reader.ReadLine();
                        if (url == "null")
                        {
                            Instances.AppDispatcher.Invoke(async () =>
                            {
                                if (
                                    Instances.MainViewInstance.WindowState != WindowState.Normal
                                    || Instances.MainViewInstance.Visibility != Visibility.Visible
                                )
                                {
                                    Instances.MainViewInstance.Show();
                                    await new Functions().FadeIn(Instances.MainViewInstance, 150);
                                    Instances.MainViewInstance.WindowState = WindowState.Normal;
                                    Instances.MainViewInstance.notifyIcon?.Dispose();
                                }
                                Instances.MainViewInstance.Topmost = true;
                                Instances.MainViewInstance.Topmost = false;
                            });
                            continue;
                        }
                        using (var n = new StartupHandler(url))
                        {
                            if (n.Function == "opengame")
                            {
                                Instances.AppDispatcher.Invoke(() =>
                                {
                                    Instances.MainViewInstance.WindowState = WindowState.Normal;
                                    Instances.MainViewInstance.Topmost = true;
                                    Instances.MainViewInstance.Topmost = false;
                                    List<LibraryGameModel> Games = new List<LibraryGameModel>();
                                    if (File.Exists(Saved.SaveLocation + "User\\Games.json"))
                                        Games = JsonConvert.DeserializeObject<
                                            List<Models.LibraryGameModel>
                                        >(
                                            Functions.Decrypt(
                                                File.ReadAllText(
                                                    Saved.SaveLocation + "User\\Games.json"
                                                )
                                            )
                                        );
                                    else
                                        System.Windows.Forms.MessageBox.Show(
                                            "Couldn't find the library file.",
                                            "BetaManager",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Error
                                        );
                                    if (Games.Count > 0)
                                    {
                                        LibraryGameModel game = Games.Find(
                                            g => g.ID == n.SecondArgument
                                        );
                                        if (game != null)
                                        {
                                            game.LastPlayDate =
                                                DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                            game.LastPlayDateString = Functions.TimeSince(
                                                DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                                            );
                                            int index = Games.FindIndex(x => x.ID == game.ID);

                                            if (index != -1)
                                            {
                                                Games[index] = game;
                                            }
                                            Functions.SaveLibrary(Games);
                                            if (game.GameExe != null)
                                                Functions.StartProccess(game.GameExe);
                                            else
                                                System.Windows.Forms.MessageBox.Show(
                                                    "This game has no executable file.",
                                                    "BetaManager",
                                                    MessageBoxButtons.OK,
                                                    MessageBoxIcon.Error
                                                );
                                        }
                                        else
                                            System.Windows.Forms.MessageBox.Show(
                                                "Couldn't find the provided game in the library.",
                                                "BetaManager",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Error
                                            );
                                    }
                                    else
                                        System.Windows.Forms.MessageBox.Show(
                                            "There are no games in the library.",
                                            "BetaManager",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Error
                                        );
                                });
                            }
                        }
                    }
                }
            }
        }

        public static string getPublicIP()
        {
            try
            {
                WebRequest request = WebRequest.Create("https://api64.ipify.org?format=json");
                using (WebResponse response = request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return Convert.ToString(
                        ((dynamic)JsonConvert.DeserializeObject(reader.ReadToEnd())).ip
                    );
                }
            }
            catch
            {
                return null;
            }
        }

        public static string GetSetting(string name)
        {
            RegistryKey KEY = Registry.CurrentUser.CreateSubKey(@"Software\BetaManager");
            string value;
            try
            {
                value = KEY.GetValue(name)?.ToString();
                if (value == "null")
                    value = null;
            }
            catch
            {
                value = null;
            }
            KEY.Close();
            return value != null ? Decrypt(value.ToString()) : null;
        }

        public static void SaveSetting(string name, string value)
        {
            RegistryKey KEY = Registry.CurrentUser.CreateSubKey(@"Software\BetaManager");
            KEY.SetValue(name, Encrypt(value));
            KEY.Close();
        }

        public static string Decrypt(string value)
        {
            if (value == null)
                return null;
            return new AES256().Decrypt(value, Keys.mainKey);
        }

        public static string Encrypt(string value)
        {
            if (value == null)
                return null;
            return new AES256().Encrypt(value, Keys.mainKey);
        }

        static readonly string[] SizeSuffixes =
        {
            "bytes",
            "KB",
            "MB",
            "GB",
            "TB",
            "PB",
            "EB",
            "ZB",
            "YB"
        };

        public static string FormatSize(long bytes)
        {
            string[] sizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

            int i = 0;
            double size = bytes;

            while (size >= 1024 && i < sizeSuffixes.Length - 1)
            {
                size /= 1024;
                i++;
            }

            return $"{size:0.##} {sizeSuffixes[i]}";
        }

        public static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (value < 0)
            {
                return "-" + SizeSuffix(-value, decimalPlaces);
            }

            int i = 0;
            decimal dValue = (decimal)value;
            while (Math.Round(dValue, decimalPlaces) >= 1000)
            {
                dValue /= 1000;
                i++;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", dValue, SizeSuffixes[i]);
        }

        public static async Task DownloadFileAsync(string url, string destinationPath)
        {
            using (WebClient webClient = new WebClient())
            {
                byte[] fileData = await webClient.DownloadDataTaskAsync(new Uri(url));

                using (
                    FileStream fileStream = new FileStream(
                        destinationPath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.ReadWrite
                    )
                )
                {
                    await fileStream.WriteAsync(fileData, 0, fileData.Length);
                }
            }
        }

        public static string GetFreeDiskSpace(string driveName)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    long freeSpace = drive.AvailableFreeSpace;
                    double mbFreeSpace = (double)freeSpace / Math.Pow(1024, 2);
                    double gbFreeSpace = mbFreeSpace / 1024D;

                    if (freeSpace < Math.Pow(1024, 3))
                    {
                        return mbFreeSpace.ToString("#.00", NumberFormatInfo.InvariantInfo) + " MB";
                    }
                    return gbFreeSpace.ToString("#.00", NumberFormatInfo.InvariantInfo) + " GB";
                }
            }
            return String.Empty;
        }

        public static string GetFileNameAndExtensionFromUrl(string url)
        {
            Uri uri = new Uri(url);
            string fileName = Path.GetFileName(uri.LocalPath);
            string fileExtension = Path.GetExtension(fileName);
            if (!string.IsNullOrEmpty(fileExtension) && fileExtension.StartsWith("."))
            {
                fileExtension = fileExtension.Substring(1);
            }

            string fileNameAndExtension = $"{fileName}.{fileExtension}";

            return fileNameAndExtension;
        }

        public static string RemoveInvalidCharDir(string dir)
        {
            string regSearch =
                new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex rg = new Regex(string.Format("[{0}]", Regex.Escape(regSearch)));
            return rg.Replace(dir, "");
        }

        public static void CreateDesktopShortcut(
            string shortcutName,
            string gameID,
            string appPath,
            string icon = null
        )
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string shortcutPath = Path.Combine(
                    desktopPath,
                    $"{RemoveInvalidCharDir(shortcutName)}.url"
                );

                using (
                    StreamWriter writer = new StreamWriter(shortcutPath, false, Encoding.Unicode)
                )
                {
                    writer.WriteLine("[InternetShortcut]");
                    writer.WriteLine("URL=betamanager://opengame/" + gameID);
                    writer.WriteLine("IconIndex=0");
                    writer.WriteLine("IconFile=" + appPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void RestartAsAdmin()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Process.GetCurrentProcess().MainModule.FileName,
                    Verb = "runas",
                    UseShellExecute = true
                };

                Process.Start(startInfo);
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to restart as admin: " + ex.Message);
            }
        }

        public static void RegisterProtocol()
        {
            try
            {
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey("betamanager"))
                {
                    if (key != null)
                    {
                        key.SetValue("", $"URL:BetaManager Protocol");
                        key.SetValue("URL Protocol", "");

                        using (RegistryKey shellKey = key.CreateSubKey("shell"))
                        using (RegistryKey openKey = shellKey?.CreateSubKey("open"))
                        using (RegistryKey commandKey = openKey?.CreateSubKey("command"))
                        {
                            commandKey?.SetValue(
                                "",
                                $"\"{System.Reflection.Assembly.GetExecutingAssembly().Location}\" \"%1\""
                            );
                        }
                    }
                }
                SettingsModel.RegisteredProtocol = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static byte[] ReadFileAsBytes(string filePath)
        {
            byte[] fileData;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader binaryReader = new BinaryReader(fileStream))
                {
                    fileData = binaryReader.ReadBytes((int)fileStream.Length);
                }
            }

            return fileData;
        }

        public static BitmapImage LoadBitmapFromBytes(byte[] imageData)
        {
            BitmapImage bitmapImage = new BitmapImage();

            using (MemoryStream memoryStream = new MemoryStream(imageData))
            {
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }

            return bitmapImage;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public static byte[] GetResourceAsByteArray(string resourceName)
        {
            var resource = Resources.ResourceManager.GetObject(resourceName);

            if (resource is byte[] byteArray)
            {
                return byteArray;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Resource '{resourceName}' is not a byte array."
                );
            }
        }

        public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            if (bitmapsource == null)
                return null;

            Bitmap bitmap = null;
            MemoryStream outStream = null;

            try
            {
                outStream = new MemoryStream();
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapsource));
                encoder.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            finally
            {
                if (outStream != null)
                {
                    outStream.Close();
                    outStream.Dispose();
                }
            }

            return bitmap;
        }

        public BitmapSource Bitmap2BitmapImage(Bitmap bitmap)
        {
            if (bitmap == null)
                return new BitmapImage();

            IntPtr hBitmap = IntPtr.Zero;
            BitmapSource retval = null;

            try
            {
                hBitmap = bitmap.GetHbitmap();
                retval = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            return retval;
        }

        private static Random random = new Random();
        private const string allowedChars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static string GenerateRandomStringVar(int minLength, int maxLength)
        {
            int length = random.Next(minLength, maxLength + 1);
            char[] result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = allowedChars[random.Next(0, allowedChars.Length)];
            }
            return new string(result);
        }

        public static string GenerateRandomString(int length)
        {
            char[] result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = allowedChars[random.Next(0, allowedChars.Length)];
            }
            return new string(result);
        }

        public static string TimeSince(long oldDateTimestamp)
        {
            if (oldDateTimestamp == 0)
                return "Never";

            DateTimeOffset oldDate = DateTimeOffset.FromUnixTimeSeconds(oldDateTimestamp);
            DateTimeOffset currentDate = DateTimeOffset.Now;
            TimeSpan difference = currentDate - oldDate;

            if (difference.TotalSeconds < 60)
            {
                return $"{(int)difference.TotalSeconds} second{((int)difference.TotalSeconds != 1 ? "s" : "")} ago";
            }
            else if (difference.TotalMinutes < 60)
            {
                return $"{(int)difference.TotalMinutes} minute{((int)difference.TotalMinutes != 1 ? "s" : "")} ago";
            }
            else if (difference.TotalHours < 24)
            {
                return $"{(int)difference.TotalHours} hour{((int)difference.TotalHours != 1 ? "s" : "")} ago";
            }
            else if (difference.TotalDays < 30)
            {
                return $"{(int)difference.TotalDays} day{((int)difference.TotalDays != 1 ? "s" : "")} ago";
            }
            else
            {
                return $">30 Days ago";
            }
        }

        public static List<LibraryGameModel> GetLibraryGames()
        {
            try
            {
                if (File.Exists(Saved.SaveLocation + "User\\Games.json"))
                    return JsonConvert.DeserializeObject<List<Models.LibraryGameModel>>(
                        Functions.Decrypt(File.ReadAllText(Saved.SaveLocation + "User\\Games.json"))
                    );
                else
                    return new List<Models.LibraryGameModel>();
            }
            catch (Exception e)
            {
                Saved.Logger.Log("483357", e.ToString());
                return new List<Models.LibraryGameModel>();
            }
        }

        public static bool SaveLibrary(List<LibraryGameModel> Games = null)
        {
            try
            {
                if (Games == null)
                    Games = Saved.LibraryGames;
                else
                    Saved.LibraryGames = Games;
                Saved.Logger.Log("724726");
                if (Games != null)
                {
                    Saved.Logger.Log("844095");
                    File.WriteAllText(
                        Saved.SaveLocation + "User//Games.json",
                        Encrypt(JsonConvert.SerializeObject(Games, Formatting.Indented))
                    );
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Saved.Logger.Log("483012", e.ToString());
                return false;
            }
        }

        public static void SearchShortcutsOnDesktop(string targetUrl, string directory = null)
        {
            try
            {
                foreach (
                    var shortcutFile in Directory.EnumerateFiles(
                        directory ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        "*.url"
                    )
                )
                {
                    if (UrlFileContainsUrl(shortcutFile, targetUrl))
                    {
                        System.IO.File.Delete(shortcutFile);
                    }
                }

                foreach (
                    var subdirectory in Directory.EnumerateDirectories(
                        directory ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                    )
                )
                {
                    SearchShortcutsOnDesktop(targetUrl, subdirectory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static bool UrlFileContainsUrl(string urlFilePath, string targetUrl)
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(urlFilePath);
                foreach (var line in lines)
                {
                    if (
                        line.StartsWith("URL=", StringComparison.OrdinalIgnoreCase)
                        && line.Substring(4)
                            .Trim()
                            .Equals(targetUrl, StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool AddGameToLibrary(LibraryGameModel game)
        {
            try
            {
                Saved.Logger.Log("74440");
                if (Saved.LibraryGames == null)
                {
                    Saved.Logger.Log("534204");
                    Saved.LibraryGames = GetLibraryGames();
                }
                Saved.LibraryGames.Add(game);
                Saved.Logger.Log("142407");
                SaveLibrary();
                return true;
            }
            catch (Exception e)
            {
                Saved.Logger.Log("250099", e.ToString());
                return false;
            }
        }

        public void SaveDownloadsToXml()
        {
            try
            {
                if (DownloadManager.Instance.TotalDownloads > 0)
                {
                    XElement root = new XElement("Downloads");

                    foreach (DownloadClient download in DownloadManager.Instance.DownloadsList)
                    {
                        XElement xdl = new XElement(
                            "Download",
                            new XElement("PercentageToWidth", download._PercentageToWidth),
                            new XElement("Path", download.Path),
                            new XElement("ResumeButtonIcon", download.PauseResumeButtonIcon),
                            new XElement("ID", download.ID),
                            new XElement("Name", download.Name),
                            new XElement("Picture", download.Picture),
                            new XElement("DownloadSize", download.DownloadSize),
                            new XElement(
                                "Files",
                                JsonConvert.SerializeObject(
                                    download.BitSwarmFiles,
                                    Formatting.Indented
                                )
                            ),
                            new XElement(
                                "BytesDownloaded",
                                download.TorrentModel.BytesDownloaded + download.addBytesDownloaded
                            ),
                            new XElement(
                                "BytesUploaded",
                                download.TorrentModel.BytesUploaded + download.addBytesUploaded
                            ),
                            new XElement("Magnet", download.Magnet.ToString()),
                            new XElement("State", download.Status.ToString())
                        );
                        root.Add(xdl);
                    }

                    XDocument xd = new XDocument();
                    xd.Add(root);
                    File.WriteAllText(
                        Saved.SaveLocation + "Downloads.xml",
                        Functions.Encrypt(xd.ToString())
                    );
                }
                else
                    File.WriteAllText(Saved.SaveLocation + "Downloads.xml", "");
            }
            catch { }
        }

        public async void DownloadPicsAndScreenshots(List<FitGirlGameModel> games)
        {
            int num = 0;
            foreach (FitGirlGameModel game in games)
            {
                if (game.Picture.ToString() == "1")
                {
                    game.Picture = "/BetaManager;component/Resources/NoImage.png";
                    continue;
                }
                if (game.Picture.ToString().Length > 0)
                {
                    string tempFilePath =
                        Saved.SaveLocation + "Games\\Images\\" + Path.GetRandomFileName();
                    string finalFilePath =
                        Saved.SaveLocation
                        + "Games\\Images\\"
                        + Path.GetFileName(game.Picture.ToString());

                    if (File.Exists(tempFilePath))
                    {
                        File.Delete(tempFilePath);
                    }

                    if (!File.Exists(finalFilePath))
                    {
                        try
                        {
                            await Functions.DownloadFileAsync(
                                Saved.Site + game.Picture.ToString(),
                                tempFilePath
                            );
                            File.Move(tempFilePath, finalFilePath);
                            game.Picture = finalFilePath;
                        }
                        catch
                        {
                            game.Picture = "/BetaManager;component/Resources/NoImage.png";
                        }
                    }
                    else
                    {
                        game.Picture = finalFilePath;
                    }
                }
                new Thread(async () =>
                {
                    for (int i = 0; i < game.Screenshots.Count; i++)
                    {
                        string image = game.Screenshots[i];
                        if (image == "1")
                        {
                            game.Screenshots[i] = "/BetaManager;component/Resources/NoImage.png";
                            continue;
                        }
                        string tempFilePath =
                            Saved.SaveLocation
                            + "Games\\Images\\Screenshots\\"
                            + Path.GetRandomFileName();
                        string finalFilePath =
                            Saved.SaveLocation
                            + "Games\\Images\\Screenshots\\"
                            + Path.GetFileName(image);

                        if (File.Exists(tempFilePath))
                        {
                            File.Delete(tempFilePath);
                        }

                        if (!File.Exists(finalFilePath))
                        {
                            try
                            {
                                await Functions.DownloadFileAsync(Saved.Site + image, tempFilePath);
                                File.Move(tempFilePath, finalFilePath);
                                game.Screenshots[i] = finalFilePath;
                            }
                            catch { }
                        }
                        else
                        {
                            game.Screenshots[i] = finalFilePath;
                        }
                    }
                }).Start();
            }
        }

        public static void AddAppToStartup(string name, string appPath)
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(
                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
                true
            );
            registryKey.SetValue(name, "\"" + appPath + "\"" + " --minimized");
        }

        public static void RemoveAppToStartup(string name)
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(
                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
                true
            );
            registryKey.DeleteValue(name);
        }

        public static string ConvertMillisecondsToReadableTimeMonitorMode(long milliseconds)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);

            if (timeSpan.TotalHours >= 1)
            {
                return $"{(int)timeSpan.TotalHours} {((int)timeSpan.TotalHours == 1 ? "hour" : "hours")}";
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                return $"{(int)timeSpan.TotalMinutes} {((int)timeSpan.TotalMinutes == 1 ? "minute" : "minutes")}";
            }
            else
            {
                return $"{(int)timeSpan.TotalSeconds} {((int)timeSpan.TotalSeconds == 1 ? "second" : "seconds")}";
            }
        }

        public static void StartProccess(string path)
        {
            try
            {
                IntPtr result = ShellExecute(
                    IntPtr.Zero,
                    "open",
                    path,
                    "",
                    Path.GetDirectoryName(path),
                    0
                );

                if (result.ToInt32() <= 32)
                {
                    throw new Exception($"Failed to start process. Error code: {result}");
                }
            }
            catch { }
        }

        public async void LoadDownloadsFromXml()
        {
            try
            {
                if (File.Exists(Saved.SaveLocation + "Downloads.xml"))
                {
                    XElement downloads = XElement.Parse(
                        Functions.Decrypt(File.ReadAllText(Saved.SaveLocation + "Downloads.xml"))
                    );
                    if (downloads.HasElements)
                    {
                        IEnumerable<XElement> downloadsList =
                            from el in downloads.Elements()
                            select el;
                        foreach (XElement download in downloadsList)
                        {
                            FitGirlGameModel fitGirlGameModel = await Auth.GetGame(
                                download.Element("ID").Value
                            );
                            DownloadClient downloadClient = new DownloadClient(
                                download.Element("Magnet").Value,
                                download.Element("Path").Value,
                                download.Element("Name").Value,
                                fitGirlGameModel,
                                Convert.ToInt64(download.Element("DownloadSize").Value),
                                true
                            );

                            downloadClient.ID = download.Element("ID").Value;
                            downloadClient.Name = download.Element("Name").Value;
                            downloadClient.DownloadSize = Convert.ToInt64(
                                download.Element("DownloadSize").Value
                            );
                            downloadClient.BitSwarmFiles = JsonConvert.DeserializeObject<
                                List<BitSwarmTorrentFile>
                            >(download.Element("Files").Value);
                            downloadClient.Path = download.Element("Path").Value;
                            downloadClient._PercentageToWidth = Double.Parse(
                                download.Element("PercentageToWidth").Value
                            );
                            downloadClient.addBytesDownloaded = Convert.ToInt64(
                                download.Element("BytesDownloaded").Value
                            );
                            downloadClient.addBytesUploaded = Convert.ToInt64(
                                download.Element("BytesUploaded").Value
                            );
                            downloadClient.DownloadProgressChanged +=
                                downloadClient.DownloadProgressChangedHandler;
                            downloadClient.DownloadCompleted +=
                                downloadClient.DownloadCompletedHandler;
                            downloadClient.DownloadCompleted += Instances
                                .ManagerViewInstance
                                .DownloadCompletedHandler;
                            downloadClient.Picture = (string)download.Element("Picture").Value;

                            downloadClient.PauseResumeButtonIcon = "Resources/Icons/resume.svg";

                            DownloadManager.Instance.DownloadsList.Add(downloadClient);
                        }

                        // Create empty XML file
                        XElement root = new XElement("Downloads");
                        XDocument xd = new XDocument();
                        xd.Add(root);
                    }
                }
            }
            catch
            {
                try
                {
                    File.Delete(Saved.SaveLocation + "Downloads.xml");
                }
                catch { }
            }
        }

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);

        public void SendNotification(string Title, string Message, int State)
        {
            var _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = BetaManager.Properties.Resources.icon;
            _notifyIcon.Visible = true;
            ToolTipIcon icon;

            switch (State)
            {
                case 0:
                    icon = ToolTipIcon.None;
                    break;
                case 1:
                    icon = ToolTipIcon.Info;
                    break;
                case 2:
                    icon = ToolTipIcon.Warning;
                    break;
                case 3:
                    icon = ToolTipIcon.Error;
                    break;
                default:
                    icon = ToolTipIcon.None;
                    break;
            }

            new Thread(() =>
            {
                _notifyIcon.ShowBalloonTip(10000, Title, Message, icon);
                _notifyIcon.Dispose();
            }).Start();
        }

        public static String SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

        public static SecureString StringToSecureString(string password)
        {
            if (password == null)
                throw new ArgumentNullException("password");

            var securePassword = new SecureString();

            foreach (char c in password)
                securePassword.AppendChar(c);

            securePassword.MakeReadOnly();
            return securePassword;
        }
    }
}
