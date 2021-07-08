using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using BarcodeReader = ZXing.Presentation.BarcodeReader;


namespace QSC_Test_Automation
{
    class QR_Reader
    {
        private readonly BarcodeReader reader = new BarcodeReader();
        public void capture_image(List<string> cameraip, string QRcode, string path, string vcount, out Tuple<bool, string, string> result)
        {
            result = new Tuple<bool, string, string>(false, string.Empty, string.Empty);
            int retry = 0;          
            try
            {
                //download image
                if (cameraip.Count > 0)
                {
                    if (cameraip[0].ToString() != "Not Applicable")
                    {
                    retryQR:
                        if (downloadimage(cameraip[0].ToString(), path, vcount))
                        {
                            //private readonly BarcodeReader reader = new BarcodeReader();
                            //BitmapImage bImage = new BitmapImage(new Uri(@path + "\\snapshot.jpg"));
                            BitmapImage bImage = new BitmapImage();
                            bImage.BeginInit();
                            bImage.UriSource = (new Uri(@path + "\\" + vcount + ".jpg"));
                            bImage.CacheOption = BitmapCacheOption.OnLoad;
                            bImage.EndInit();
                            var output = reader.Decode(bImage);

                            if (output != null && output.ToString().ToUpper().Trim() == QRcode.ToUpper().Trim())
                            {
                                result = new Tuple<bool, string, string>(true, string.Empty, string.Empty);
                            }
                            else if (output != null && output.ToString().ToUpper() != QRcode.ToUpper())
                            {
                                result = new Tuple<bool, string, string>(false, "QR code not matched", output.ToString());
                            }
                            else
                            {
                                retry++;
                                if (retry < 3)
                                {
                                    Thread.Sleep(15000);                                   
                                    goto retryQR;
                                }

                                result = new Tuple<bool, string, string>(false, "No QR code detected", string.Empty);
                            }
                        }
                        else
                        {
                            if (IsCorePresent(cameraip[0].ToString()))
                                result = new Tuple<bool, string, string>(false, "Unable to download Preview image", string.Empty);
                            else
                                result = new Tuple<bool, string, string>(false, "camera not available", string.Empty);
                        }
                    }
                    else
                    {
                        result = new Tuple<bool, string, string>(false, "camera ip is set to Not Applicable", string.Empty);
                    }
                }
                else
                {
                    result = new Tuple<bool, string, string>(false, "camera not available", string.Empty);
                }
            }
            catch (Exception ex)
            {
                result = new Tuple<bool, string, string>(false, ex.Message.ToString(), string.Empty);
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
            }
        }

        private bool downloadimage(string cameraip, string path, string vcount)
        {
            try
            {
                if(File.Exists(path + "\\" + vcount + ".jpg"))                
                    File.Delete(path + "\\" + vcount + ".jpg");                

                int i = 0;
            download:
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile("http://" + cameraip + "/snapshot.jpg", path + "\\" + vcount + ".jpg");
                    webClient.Dispose();
                }

                if (File.Exists(Path.Combine(path, vcount + ".jpg")))
                {
                    FileInfo getfile = new FileInfo(Path.Combine(path, vcount + ".jpg"));

                    if (getfile.Length > 0)
                        return true;
                    else
                    {
                        if (i == 4)
                            return false;

                        i++;
                        Thread.Sleep(2000);
                        goto download;
                    }
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
                return false;
            }
        }

        //private bool downloadimage(string cameraip,string path,string vcount)
        //{

        //    try
        //    {
        //        using (WebClient webClient = new WebClient())
        //        {
        //            webClient.DownloadFile("http://" + cameraip + "/snapshot.jpg", path + "\\"+vcount+".jpg");
        //            webClient.Dispose();
        //        }
        //        //File.Move(path + "\\snapshot.jpg", "D:\\1.jpg");
              
        //        if (File.Exists(Path.Combine(path, vcount + ".jpg")))
        //                return true;
        //            else
        //                return false;
        //    }
        //    catch(Exception ex)
        //    {
        //        return false;
        //        DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
        //    }
        //}

        public bool IsCorePresent(string coreipcheck)
        {
            bool success = false;
            int trial = 0;
            try
            {
                if ((coreipcheck != null) && (coreipcheck != string.Empty) && (coreipcheck != "Not Applicable"))
                {
                    while ((!success) & (trial < 4))
                    {
                        Ping ping = new Ping();
                        PingReply pingReply = ping.Send(coreipcheck);

                        if (pingReply.Status == IPStatus.Success)
                        {
                            success = true;

                        }
                        else
                        {
                            trial = trial + 1;
                            Thread.Sleep(2000);
                        }

                    }
                }
                return success;
            }
            catch { return success; }
        }

        //public void image()
        //{
        //    try
        //    {
        //        var originalbmp = new Bitmap(Bitmap.FromFile("E:\\qr codes 1080p60.png"));
        //        //BitmapImage originalbmp = new BitmapImage(new Uri("E:\\color.png"));

        //        bool Rowchanged = false;
        //        int row = 0;
        //        int column = 0;
        //        int Rend = originalbmp.Width / 2;
        //        int Cend = originalbmp.Height / 2;
        //        string filename = "E:\\Pic1.png";

        //        split(row, Rend, column, Cend, filename, originalbmp);

        //        column = originalbmp.Height / 2;
        //        Cend = originalbmp.Height;
        //        filename = "E:\\Pic2.png";


        //        split(row, Rend, column, Cend, filename, originalbmp);

        //        row = originalbmp.Width / 2;
        //        Rend = originalbmp.Width;
        //        column = 0;
        //        Cend = originalbmp.Height / 2;
        //        filename = "E:\\Pic3.png";

        //        split(row, Rend, column, Cend, filename, originalbmp);

        //        column = originalbmp.Height / 2;
        //        Cend = originalbmp.Height;
        //        filename = "E:\\Pic4.png";

        //        split(row, Rend, column, Cend, filename, originalbmp);


        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //}

        //public void split(int row, int Rend, int column, int Cend, string filename, Bitmap original)
        //{
        //    try
        //    {
        //        var newbmp = new Bitmap(original.Width / 2, original.Height / 2);
        //        int k = 0;
        //        int x = 0;
        //        for (int i = row; i < Rend; i++)
        //        {
        //            for (int j = column; j < Cend; j++)
        //            {
        //                newbmp.SetPixel(k, x, original.GetPixel(i, j));
        //                x++;
        //            }
        //            k++;
        //            x = 0;
        //        }
        //        newbmp.Save(filename.Replace(".", "_BlackAnd White"));
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}





    }
}
