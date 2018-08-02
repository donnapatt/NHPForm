using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HalconDotNet;
using System.Globalization;
using Excel = Microsoft.Office.Interop.Excel;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using System.IO;
using BitmapHImageConverter;
using System.Drawing.Imaging;
using System.Threading;

namespace NHP_Container_OCR_Form
{
    public partial class Form1 : Form
    {
        // Framegrabber
        HFramegrabber topView = new HFramegrabber();
        HFramegrabber bottomView = new HFramegrabber();

        HDevEngine engine = new HDevEngine();

        // Engineer Mode for debugger
        Boolean engMode = false;
        //
        private DateTime t;
        private DataGridView dgv;
        private HDevProgram program;
        private HDevProgramCall programCall;
        private HDevProcedure ocr_NHP;
        private HDevProcedureCall ocr_NHP_Call;
        private HDevProcedure charArray2String;
        private HDevProcedureCall charArray2String_Call;
        private const int numWin = 4;

        private HWindow[] mainWindowList = new HWindow[4];
        private HWindow[] sideWindowList = new HWindow[4];

        private String keyUpload;
        private Label license;
        private Label container;

        private int licThres;
        private int conThres;

        private double conXi;
        private double conYi;
        private double conXf;
        private double conYf;

        private double licXi;
        private double licYi;
        private double licXf;
        private double licYf;


        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {

            String path = @".\hdev\ocr_mvtec_using_a_camera.hdev";
            String procPath = @".\procedure";
            engine.SetProcedurePath(procPath);
            program = new HDevProgram(path);
            programCall = new HDevProgramCall(program);
            ocr_NHP = new HDevProcedure(program, "ocr_NHP");
            ocr_NHP_Call = new HDevProcedureCall(ocr_NHP);
            charArray2String = new HDevProcedure(program, "charArray2String");
            charArray2String_Call = new HDevProcedureCall(charArray2String);
            t = DateTime.Now;

            dgv = dataGridView1;
            mainWindowList[0] = hSmartWindowControl1.HalconWindow;
            mainWindowList[1] = hSmartWindowControl2.HalconWindow;
            //mainWindowList[2] = hSmartWindowControl3.HalconWindow;
            //mainWindowList[3] = hSmartWindowControl4.HalconWindow;
            //hSmartWindowControl5.Hide();
            //hSmartWindowControl6.Hide();
            sideWindowList[0] = hSmartWindowControl3.HalconWindow;
            sideWindowList[1] = hSmartWindowControl4.HalconWindow;
            //sideWindowList[2] = hSmartWindowControl7.HalconWindow;
            //sideWindowList[3] = hSmartWindowControl8.HalconWindow;
            //hSmartWindowControl7.Hide();
            //hSmartWindowControl8.Hide();

            initSetup();
            setCamera();
            updateSetting();

            dgv.AutoSize = false;
            this.dgv.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dgv.RowHeadersDefaultCellStyle.BackColor = Color.DarkSalmon;
        }

        private void setData(String con, String lic, String date, String timeIn, String timeOut)
        {
            int n;
            n = dgv.Rows.Add();
            dgv.Rows[n].DefaultCellStyle.BackColor = Color.LightSalmon;
            dgv.Rows[n].DefaultCellStyle.ForeColor = Color.Black;

            dgv.Rows[n].Cells[0].Value = con;
            dgv.Rows[n].Cells[1].Value = lic;
            dgv.Rows[n].Cells[2].Value = date;
            dgv.Rows[n].Cells[3].Value = timeIn;
            dgv.Rows[n].Cells[4].Value = timeOut;


        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                excel.Visible = true;
                Microsoft.Office.Interop.Excel.Workbook workbook = excel.Workbooks.Add(System.Reflection.Missing.Value);
                Microsoft.Office.Interop.Excel.Worksheet sheet1 = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[1];
                int StartCol = 1;
                int StartRow = 1;
                int j = 0, i = 0;
                //Write Headers
                for (j = 0; j < dgv.Columns.Count; j++)
                {
                    Microsoft.Office.Interop.Excel.Range myRange = (Microsoft.Office.Interop.Excel.Range)sheet1.Cells[StartRow, StartCol + j];
                    myRange.Value2 = dgv.Columns[j].HeaderText;
                }
                StartRow++;
                //Write datagridview content
                for (i = 0; i < dgv.Rows.Count; i++)
                {
                    for (j = 0; j < dgv.Columns.Count; j++)
                    {
                        try
                        {
                            Microsoft.Office.Interop.Excel.Range myRange = (Microsoft.Office.Interop.Excel.Range)sheet1.Cells[StartRow + i, StartCol + j];
                            myRange.NumberFormat = "General";
                            myRange.Value2 = dgv[j, i].Value == null ? "" : dgv[j, i].Value;
                        }
                        catch
                        {
                            ;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            clearAllWindow();
            HTuple _, height, width, lic, con;
            HRegion licRegion, conRegion;
            HImage conImage, licImage;

            try
            {
                {
                    licImage = bottomView.GrabImageAsync(-1);
                    HRegion ROI = new HRegion();
                    ROI.GenRectangle1(licXi, licYi, licXf, licYf);
                    ocr_NHP_Call.SetInputIconicParamObject("Image", licImage);
                    ocr_NHP_Call.SetInputIconicParamObject("ROI", ROI);
                    ocr_NHP_Call.SetInputCtrlParamTuple("typeChk", "license");
                    ocr_NHP_Call.SetInputCtrlParamTuple("threshold", licThres);
                    ocr_NHP_Call.Execute();
                    licRegion = ocr_NHP_Call.GetOutputIconicParamRegion("Characters");
                    lic = ocr_NHP_Call.GetOutputCtrlParamTuple("result");
                    charArray2String_Call.SetInputCtrlParamTuple("inputArray", lic);
                    charArray2String_Call.Execute();
                    lic = charArray2String_Call.GetOutputCtrlParamTuple("result");
                }
                {
                    conImage = topView.GrabImageAsync(-1);
                    HRegion ROI = new HRegion();
                    ROI.GenRectangle1(conXi, conYi, conXf, conYf);
                    ocr_NHP_Call.SetInputIconicParamObject("Image", conImage);
                    ocr_NHP_Call.SetInputIconicParamObject("ROI", ROI);
                    ocr_NHP_Call.SetInputCtrlParamTuple("typeChk", "container");
                    ocr_NHP_Call.SetInputCtrlParamTuple("threshold", conThres);
                    ocr_NHP_Call.Execute();
                    conRegion = ocr_NHP_Call.GetOutputIconicParamRegion("Characters");
                    con = ocr_NHP_Call.GetOutputCtrlParamTuple("result");
                    charArray2String_Call.SetInputCtrlParamTuple("inputArray", con);
                    charArray2String_Call.Execute();
                    con = charArray2String_Call.GetOutputCtrlParamTuple("result");
                }


                conImage.GetImagePointer1(out _, out width, out height);
                conImage = conImage.RotateImage(180.0, "constant");
                mainWindowList[0].SetPart(0, 0, height.I, width.I);
                mainWindowList[1].SetPart(0, 0, height.I, width.I);


                sideWindowList[0].SetPart(0, 0, height.I, width.I);
                sideWindowList[1].SetPart(0, 0, height.I, width.I);

                mainWindowList[0].DispImage(licImage);
                mainWindowList[1].DispImage(conImage);
                /*lic = programCall.GetCtrlVarTuple("license");
                con = programCall.GetCtrlVarTuple("container");
                */
                license = licenseLabel;
                container = containerLabel;
                /*
                licRegion = programCall.GetIconicVarRegion("Characters");
                conRegion = programCall.GetIconicVarRegion("Characters2");
                */
                sideWindowList[0].SetColored(12);
                sideWindowList[1].SetColored(12);
                sideWindowList[0].DispObj(licRegion);
                sideWindowList[1].DispObj(conRegion);

                license.Text = lic.S;
                container.Text = con.S;
                t = DateTime.Now;
                String date = t.ToString("dd-MM-yyyy");
                String timeIn = t.ToString("HH:mm:ss");
                //Console.WriteLine(time);
                //Console.WriteLine(time.Substring(0, 10));
                //Console.WriteLine(time.Substring(0, 7));
                setData(con.S, lic.S, date, timeIn, "");
                uploadToCloud(licImage, conImage, lic.S, con.S);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        private void setCamera()
        {
            if (topView != null)
            {
                topView.Dispose();
                topView = new HFramegrabber();
            }
            if (bottomView != null)
            {
                bottomView.Dispose();
                bottomView = new HFramegrabber();
            }
            if (engMode == true)
            {
                topView.OpenFramegrabber("DirectShow", 1, 1, 0, 0, 0, 0, "default", 8, "rgb", -1, "false", "default", "[0] Integrated Webcam", 0, -1);
            }
            else
            {
                bottomView.OpenFramegrabber("GigEVision2", 0, 0, 0, 0, 0, 0, "progressive", -1, "default", -1, "false", "default", "licenseCam", 0, -1);
                topView.OpenFramegrabber("GigEVision2", 0, 0, 0, 0, 0, 0, "progressive", -1, "default", -1, "false", "default", "containerCam", 0, -1);
            }
        }

        private void uploadToCloud(HImage licImage, HImage conImage, String licenseTxt, String containerTxt)
        {
            uploadDB(licenseTxt, containerTxt, licImage, conImage);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            uploadDB("license1234", "container5678", new HImage(), new HImage());

            uploadSto(new HImage(), new HImage(), keyUpload);
        }

        private async void uploadDB(String licenseTxt, String containerTxt, HImage licImage, HImage conImage)
        {
            {
                var firebase = new FirebaseClient("https://nhp-container.firebaseio.com/");
                var count = await firebase
                    .Child("count")
                    .OnceSingleAsync<int>();
                Console.WriteLine("Count : " + count);
                t = DateTime.Now;
                String date = t.ToString("dd-MM-yyyy");
                String time = t.ToString("HH:mm:ss");
                var post = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    Date = date,
                    Time = time,
                    License = licenseTxt,
                    Container = containerTxt
                });
                // add new item to list of data and let the client generate new key for you (done offline)
                await firebase
                  .Child("car")
                  .Child((count + 1).ToString())
                  .PutAsync(post);
                // note that there is another overload for the PostAsync method which delegates the new key generation to the firebase server
                //Console.WriteLine($"Key for the new license: {upload.Key}");
                count += 1;
                await firebase
                     .Child("count")
                     .PutAsync(count);
                keyUpload = (count).ToString();
            }

            {
                Console.WriteLine("key : " + keyUpload);
                // Get any Stream - it can be FileStream, MemoryStream or any other type of Stream
                //var stream = File.Open(@"C:\Users\donna\Desktop\save.jpg", FileMode.Open);
                Image timage1, timage2;
                topView.GrabImageStart(-1);
                if (engMode)
                {
                    var image = topView.GrabImageAsync(-1);
                    var image2 = topView.GrabImageAsync(-1);
                    timage1 = himage2image(image);
                    timage2 = himage2image(image2);
                }
                else
                {
                    timage1 = himage2image(licImage);
                    timage2 = himage2image(conImage);
                }
                var st1 = ToStream(timage1, ImageFormat.Jpeg);
                var st2 = ToStream(timage2, ImageFormat.Jpeg);
                // Constructr FirebaseStorage, path to where you want to upload the file and Put it there
                var task1 = new FirebaseStorage("nhp-container.appspot.com")
                    .Child("car")
                    .Child(keyUpload)
                    .Child("licImage.jpg")
                    .PutAsync(st1);
                var task2 = new FirebaseStorage("nhp-container.appspot.com")
                    .Child("car")
                    .Child(keyUpload)
                    .Child("conImage.jpg")
                    .PutAsync(st2);
                // Track progress of the upload
                task1.Progress.ProgressChanged += (s1, e1) => Console.WriteLine($"Progress: {e1.Percentage} %");
                task2.Progress.ProgressChanged += (s2, e2) => Console.WriteLine($"Progress: {e2.Percentage} %");
                // await the task to wait until upload completes and get the download url
                var downloadUrl1 = await task1;
                Console.WriteLine("Upload Complete, URL = " + downloadUrl1);
                var downloadUrl2 = await task2;
                Console.WriteLine("Upload Complete, URL = " + downloadUrl2);
            }
        }

        private async void uploadSto(HImage licImage, HImage conImage, String key)
        {

        }

        public Stream ToStream(Image image, ImageFormat format)
        {
            var stream = new System.IO.MemoryStream();
            image.Save(stream, format);
            stream.Position = 0;
            return stream;
        }

        public Image himage2image(HImage inputImage)
        {
            var bitImage = BitmapHImageConverter.BitmapHImageConverter.HImage2Bitmap(inputImage);
            Image outImage = (Image)bitImage;
            return outImage;
        }

        private void updateSetting()
        {
            licThres = Convert.ToInt32(licThresVal.Text);
            conThres = Convert.ToInt32(conThresVal.Text);

            conXi = Convert.ToDouble(conXiVal.Text);
            conYi = Convert.ToDouble(conYiVal.Text);
            conXf = Convert.ToDouble(conXfVal.Text);
            conYf = Convert.ToInt32(conYfVal.Text);

            licXi = Convert.ToDouble(licXiVal.Text);
            licYi = Convert.ToDouble(licYiVal.Text);
            licXf = Convert.ToDouble(licXfVal.Text);
            licYf = Convert.ToDouble(licYfVal.Text);
        }

        private void applySettingButton_Click(object sender, EventArgs e)
        {
            updateSetting();
            writeFile();
        }
        private void clearAllWindow()
        {
            for (int i = 0; i < mainWindowList.Length; i++)
            {
                if (mainWindowList[i] != null)
                {
                    mainWindowList[i].ClearWindow();
                }
                if(sideWindowList[i] != null)
                {
                    sideWindowList[i].ClearWindow();
                }
            }
        }

        private void showButton_Click(object sender, EventArgs e)
        {
            clearAllWindow();
            int Width, Height;
            HRegion _1 = new HRegion();
            HRegion _2 = new HRegion();
            HImage i1 = bottomView.GrabImageAsync(-1);
            HImage i2 = topView.GrabImageAsync(-1);
            i2 = i2.RotateImage(180.0, "constant");
            i1.GetImagePointer1(out _, out Width, out Height);
            updateSetting();
            _1.GenRectangle1(licXi, licYi, licXf, licYf);
            _2.GenRectangle1(conXi, conYi, conXf, conYf);

            mainWindowList[0].SetPart(0, 0, Height, Width);
            mainWindowList[1].SetPart(0, 0, Height, Width);

            mainWindowList[0].DispObj(i1);
            mainWindowList[1].DispObj(i2);
            mainWindowList[0].SetDraw("margin");
            mainWindowList[1].SetDraw("margin");
            mainWindowList[0].SetColor("red");
            mainWindowList[1].SetColor("red");
            mainWindowList[0].DispObj(_1);
            mainWindowList[1].DispObj(_2);
        }

        private void CaptureButton_Click(object sender, EventArgs e)
        {
            clearAllWindow();
            HTuple _, height, width, lic, con;
            HRegion licRegion, conRegion;
            HImage conImage, licImage;

            try
            {
                {
                    licImage = bottomView.GrabImageAsync(-1);
                    HRegion ROI = new HRegion();
                    ROI.GenRectangle1(licXi, licYi, licXf, licYf);
                    ocr_NHP_Call.SetInputIconicParamObject("Image", licImage);
                    ocr_NHP_Call.SetInputIconicParamObject("ROI", ROI);
                    ocr_NHP_Call.SetInputCtrlParamTuple("typeChk", "license");
                    ocr_NHP_Call.SetInputCtrlParamTuple("threshold", licThres);
                    ocr_NHP_Call.Execute();
                    licRegion = ocr_NHP_Call.GetOutputIconicParamRegion("Characters");
                    lic = ocr_NHP_Call.GetOutputCtrlParamTuple("result");
                    charArray2String_Call.SetInputCtrlParamTuple("inputArray", lic);
                    charArray2String_Call.Execute();
                    lic = charArray2String_Call.GetOutputCtrlParamTuple("result");
                }
                {
                    conImage = topView.GrabImageAsync(-1);
                    HRegion ROI = new HRegion();
                    ROI.GenRectangle1(conXi, conYi, conXf, conYf);
                    ocr_NHP_Call.SetInputIconicParamObject("Image", conImage);
                    ocr_NHP_Call.SetInputIconicParamObject("ROI", ROI);
                    ocr_NHP_Call.SetInputCtrlParamTuple("typeChk", "container");
                    ocr_NHP_Call.SetInputCtrlParamTuple("threshold", conThres);
                    ocr_NHP_Call.Execute();
                    conRegion = ocr_NHP_Call.GetOutputIconicParamRegion("Characters");
                    con = ocr_NHP_Call.GetOutputCtrlParamTuple("result");
                    charArray2String_Call.SetInputCtrlParamTuple("inputArray", con);
                    charArray2String_Call.Execute();
                    con = charArray2String_Call.GetOutputCtrlParamTuple("result");
                }


                conImage.GetImagePointer1(out _, out width, out height);
                conImage = conImage.RotateImage(180.0, "constant");
                mainWindowList[0].SetPart(0, 0, height.I, width.I);
                mainWindowList[1].SetPart(0, 0, height.I, width.I);


                sideWindowList[0].SetPart(0, 0, height.I, width.I);
                sideWindowList[1].SetPart(0, 0, height.I, width.I);

                mainWindowList[0].DispImage(licImage);
                mainWindowList[1].DispImage(conImage);
                /*lic = programCall.GetCtrlVarTuple("license");
                con = programCall.GetCtrlVarTuple("container");
                */
                license = licenseLabel;
                container = containerLabel;
                /*
                licRegion = programCall.GetIconicVarRegion("Characters");
                conRegion = programCall.GetIconicVarRegion("Characters2");
                */
                sideWindowList[0].SetColored(12);
                sideWindowList[1].SetColored(12);
                sideWindowList[0].DispObj(licRegion);
                sideWindowList[1].DispObj(conRegion);

                license.Text = lic.S;
                container.Text = con.S;
                t = DateTime.Now;
                String date = t.ToString("dd-MM-yyyy");
                String timeIn = t.ToString("HH:mm:ss");
                //Console.WriteLine(time);
                //Console.WriteLine(time.Substring(0, 10));
                //Console.WriteLine(time.Substring(0, 7));
                setData(con.S, lic.S, date, timeIn, "");
                uploadToCloud(licImage, conImage, lic.S, con.S);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void reconnectButton_Click(object sender, EventArgs e)
        {
            if(topView != null)
            {
                topView.Dispose();
                topView = new HFramegrabber();
            }
            if(bottomView != null)
            {
                bottomView.Dispose();
                bottomView = new HFramegrabber();
            }
            setCamera();
        }

        private void initSetup()
        {
            String setupPath = @"..\Setup.txt";
            string[] lines = System.IO.File.ReadAllLines(setupPath);

            var licThres = lines[0];
            var conThres = lines[1];
            string[] licPosition = lines[2].Split(',');
            string[] conPosition = lines[3].Split(',');
            TextBox[] licTextbox = { licXiVal, licYiVal, licXfVal, licYfVal };
            TextBox[] conTextbox = { conXiVal, conYiVal, conXfVal, conYfVal };
            licThresVal.Text = licThres;
            conThresVal.Text = conThres;
            for(int i = 0; i < 4; i++)
            {
                licTextbox[i].Text = licPosition[i];
                conTextbox[i].Text = conPosition[i];
            }
        }

        private void writeFile()
        {
            String setupPath = @"..\Setup.txt";
            string[] lines = { licThresVal.Text, conThresVal.Text, licXiVal.Text + ","+licYiVal.Text + ","+licXfVal.Text + ","+licYfVal.Text, conXiVal.Text + "," + conYiVal.Text + "," + conXfVal.Text + "," + conYfVal.Text };
            System.IO.File.WriteAllLines(setupPath, lines);
        }
    }
}
