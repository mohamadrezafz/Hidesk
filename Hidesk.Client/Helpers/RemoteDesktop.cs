using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hidesk.Client.Helpers
{
    public class RemoteDesktop
    {
        public static MemoryStream CaptureScreenToMemoryStream(int quality)
        {
            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(new Point(0, 0), new Point(0, 0), bmp.Size);
            g.Dispose();


            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo ici = null;

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == "image/jpeg")
                    ici = codec;
            }

            var ep = new EncoderParameters();
            ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);

            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, ici, ep);
            ms.Position = 0;
            bmp.Dispose();

            return ms;
        }



        public static BinaryFormatter binaryFormatter;

        //[Obsolete]
        //public static Bitmap GetScreen(int quality)
        //{

        //    Bitmap desktopScreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        //    Graphics screen = Graphics.FromImage(desktopScreen);
        //    screen.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, desktopScreen.Size, CopyPixelOperation.SourceCopy);
        //    return desktopScreen;
        //}

        public static void SerializeScreen(NetworkStream netStream, Bitmap image)
        {
            if (binaryFormatter == null) binaryFormatter = new BinaryFormatter();

            try
            {
                binaryFormatter.Serialize(netStream, image);
            }
            catch (Exception e)
            {
                throw new ArgumentNullException();
            }

        }

        public static object DeserializeScreen(NetworkStream netStream)
        {
            if (binaryFormatter == null) binaryFormatter = new BinaryFormatter();

            return binaryFormatter.Deserialize(netStream);
        }

        // STUDY THE CODE
        [StructLayout(LayoutKind.Sequential)]
        struct CURSORINFO
        {
            public Int32 cbSize;
            public Int32 flags;
            public IntPtr hCursor;
            public POINTAPI ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct POINTAPI
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

        const Int32 CURSOR_SHOWING = 0x00000001;

        public static MemoryStream CaptureScreen(bool CaptureMouse , int quality)
        {
            Bitmap result = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format24bppRgb);
            MemoryStream ms = new MemoryStream();
            try
            {
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

                    if (CaptureMouse)
                    {
                        CURSORINFO pci;
                        pci.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(CURSORINFO));

                        if (GetCursorInfo(out pci))
                        {
                            if (pci.flags == CURSOR_SHOWING)
                            {
                                DrawIcon(g.GetHdc(), pci.ptScreenPos.x, pci.ptScreenPos.y, pci.hCursor);
                                g.ReleaseHdc();
                            }
                        }
                    }


                    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
                    ImageCodecInfo ici = null;

                    foreach (ImageCodecInfo codec in codecs)
                    {
                        if (codec.MimeType == "image/jpeg")
                            ici = codec;
                    }

                    var ep = new EncoderParameters();
                    ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);

              
                    result.Save(ms, ici, ep);
                    ms.Position = 0;
                    result.Dispose();

                    return ms;
                }
            }
            catch
            {
                result = null;
            }

            return ms;
        }





        //public static Boolean sendMouseInput = true;
        //public static BinaryWriter binaryWriter;
        //public const string CommandCursor = "RECIEVECURSORPOSITION";
        //public void SendTransmission()
        //{
        //    if (sendMouseInput == true)
        //    {
        //        Point startingPoint = new Point(0, 0);
        //        Point endingPoint = new Point(0, 0);
        //        Point deltaPoint = new Point(0, 0);
        //        //while (Status != StatusEnum.Disconnected)
        //        //{
        //            if (sendMouseInput == true)
        //            {

        //                try
        //                {

        //                    deltaPoint.X = endingPoint.X - startingPoint.X;                                         // how to handle delta properly. AKA how to structure
        //                    deltaPoint.Y = endingPoint.Y - startingPoint.Y;

        //                    startingPoint.X = Cursor.Position.X;
        //                    startingPoint.Y = Cursor.Position.Y;

        //                    binaryWriter.Write(CommandCursor);
        //                    binaryWriter.Write(deltaPoint.X);
        //                    binaryWriter.Write(deltaPoint.Y);
        //                    binaryWriter.Flush();

        //                    Thread.Sleep(30);
        //                    endingPoint.X = Cursor.Position.X;
        //                    endingPoint.Y = Cursor.Position.Y;
        //                }
        //                catch (Exception ex)
        //                {

        //                }
        //            }
        //            else
        //            {
        //                // Do nothing.
        //            }


        //       // }
        //    }


        //}

    }
}
