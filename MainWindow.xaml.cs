using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Renci.SshNet;
using Renci.SshNet.Sftp;

/*
 *     Mohamed Ahsan habib  
 *     1. Can be run two way 1. Duble clicking or 2. From the dose.  
 *     
 *      SureCostDispenseRpt.exe          --> executable file (Require) 
        SureCostDispenseRpt.exe.config   -->Configuration file(Require)

    1.	This program can be run two different option.
    2.	Most common use by double clicking and answer all question and click Export button.
    3.	Schedule to run daily with command line parameter. (SureCostDispenseRpt.exe B)  or (SureCostDispenseRpt.exe BYYYYMMDDYYYYMMDD)  ( B Followed by FromData, ToDate)
        a.	Update the configuration file for all the department. (“RnaDeptList = 04,05,10…”)
        b.	Schedule as following.
        c.	File will be created base on MHAX and AROP file. MHAX file is the logged file for AROP with timestamp. Daily file includes all changes of AROP’s on that day and day before.
        d.	If user want to collect data within three weeks from now then create a file and named it “.\data\DateFile.gmd” and have a valid date range format (YYYYMMDDYYYYMMDD). Data will be created by Scheduler with next day pooling  or run immediately user can execute the program from dose prompt “SureCostDispenseRpt.exe B”. At the end “.\data\DateFile.gmd”  will be deleted.
        e.	All the data will be into “..\data” directory and pharmacy management can archive or delete manually. 
        f.	Daily File can be upload into SFTP location with some little setup of configuration file. Need following information.
            i.	URL Address or IP Address
            ii.	Login ID
            iii.	Password
            iv.	Port number.
    4.	Need setup the configuration file.
        RnaDB = RNA2	            --> need for manually or Schedule execution.
        RnaDeptList = 03,04,05      -->need for Schedule execution. (department list)
        TransferFile=True or False  --> need for Schedule execution.(Flag for transfer automatically)

            If  TransferFile set True then following need value need to set:
            URLAddress = ftp.rnahealth.com	--> URL address for transfer
            USERID = mhabib		            --> User Name for transfer
            PWD = Rnaholding14800!		    --> Password for Transfer 
            Portnum  = 22			        --> Port number 
            TransferFileName = SureCost      --> Prefix of file name
            Remotepath = InBox              --> Remote path 

 */
namespace SureCostDispenseRpt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string m_cmdParameter;      //20180912
        string m_FileName = string.Empty;
        Nondbfct.Nondbfct m_nondbf = new Nondbfct.Nondbfct();
        string debugLogging = "N";

        public object MessageBoxButtons { get; private set; }

        public MainWindow()
        {
            m_FileName = string.Empty;
            if (Nondbfct.Nondbfct.RunningInstance() != null)
                Application.Current.Shutdown();


            string [] args = System.Environment.GetCommandLineArgs();
            if (args.Length > 1)
                m_cmdParameter = args[1];
            else
                m_cmdParameter = " ";

            if (m_cmdParameter.PadRight(1, ' ').Substring(0, 1) == "B")
            {
                btnExportBilling_Click(null, null);
                btnExit_Click(null, null);
            }
            else
            {
                InitializeComponent();
            }
        }

 
    private void btnExportBilling_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path_of_exe = Directory.GetCurrentDirectory();
                string local_path = path_of_exe;
                string urlAddress = "";// "207.210.225.242";
                string portnum = "";//"22";
                string USID = "";//"netrx";
                string PWD = "";//"netrx.";
                string remotepath = "";//"incoming";
                string TransferFile_flag = ""; //"True";
                string tmpStr = "";
                string Beg_date = "", end_date = "";

                debugLogging = Properties.Settings.Default.EnableDebugLog;

                if (debugLogging != "Y" && debugLogging != "y")
                {
                    debugLogging = "N";
                }

                //memberdata.SureCostgso.com:7935
                //Explicit FTP over TLS
                //Username = RNAFTP
                //Password = b@dArt70
                //remotedir = inbox
                //MY_SFTP = new RnaSFTPCommunication("memberdata.SureCostgso.com", "RNAFTP", "b@dArt70", "7935");


                if (m_cmdParameter.PadRight(1, ' ').Substring(0, 1) == "B")
                {
                    m_cmdParameter = m_cmdParameter.PadRight(17, ' ');
                    tmpStr = m_cmdParameter.Substring(1, 8);
                    if (m_nondbf.isNumric(tmpStr, 8) == true)
                    {
                        tmpStr = tmpStr.Substring(4, 2) + "/" + tmpStr.Substring(6, 2) + "/" + tmpStr.Substring(0, 4);
                        if (m_nondbf.IsDate(tmpStr) == true)
                        {
                            Beg_date = tmpStr;
                            tmpStr = m_cmdParameter.Substring(9, 8);
                            if (m_nondbf.isNumric(tmpStr, 8) == true)
                            {
                                tmpStr = tmpStr.Substring(4, 2) + "/" + tmpStr.Substring(6, 2) + "/" + tmpStr.Substring(0, 4);
                                if (m_nondbf.IsDate(tmpStr) == true)
                                    end_date = tmpStr;
                            }
                        }
                    }

                    if (m_nondbf.IsDate(Beg_date) == false)
                    {
                        Beg_date = DateTime.Now.AddDays(-1).ToString("MM/dd/yyyy");
                        end_date = Beg_date;
                    }
                    else if (m_nondbf.IsDate(end_date) == false)
                    {
                        end_date = DateTime.Now.ToString("MM/dd/yyyy");
                    }

                    if (GetDataFromMHAXByDaily(Beg_date, end_date) == 0)
                    {
                        TransferFile_flag = Properties.Settings.Default.UploadToFTP;

                        if (TransferFile_flag.PadRight(1, ' ').Substring(0, 1) == "Y")
                        {
                            urlAddress = Properties.Settings.Default.URLAddress;
                            portnum = Properties.Settings.Default.Portnum;
                            USID = Properties.Settings.Default.USERID;
                            PWD = Properties.Settings.Default.PWD;
                            remotepath = Properties.Settings.Default.Remotepath;
                            local_path = local_path + @"\data";

                            if (debugLogging == "Y")
                            {
                                Nondbfct.Nondbfct.WriteLog("URL: " + urlAddress, "SureCostDebug.txt");
                                Nondbfct.Nondbfct.WriteLog("Port: " + portnum, "SureCostDebug.txt");
                                Nondbfct.Nondbfct.WriteLog("User: " + USID, "SureCostDebug.txt");
                                Nondbfct.Nondbfct.WriteLog("Password: " + PWD, "SureCostDebug.txt");
                            }

                            if (urlAddress != "" && portnum != "" && USID != "" && PWD != "")
                            {
                                if (debugLogging == "Y")
                                {
                                    Nondbfct.Nondbfct.WriteLog("Begin FTP Connection", "SureCostDebug.txt");
                                }

                                if (uploadFileToFTP(urlAddress, portnum, USID, PWD, m_FileName, remotepath, m_FileName, local_path) == false)
                                {
                                    Nondbfct.Nondbfct.WriteLog("Error: Could not Connect --(SureCostDispenseRpt)", "SureCostDispenseRpt.txt");
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (GetDataFromAropByDateRange() == 0)
                    {
                        TransferFile_flag = SureCostDispenseRpt.Properties.Settings.Default.UploadToFTP;
                        if (TransferFile_flag.PadRight(1, ' ').Substring(0, 1) == "Y")
                        {
                            if (MessageBox.Show("Do you want to upload the file to SureCost FTP site?", "SureCost Dispense Rpt", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                // get info. from config file.
                                urlAddress = Properties.Settings.Default.URLAddress;
                                portnum = Properties.Settings.Default.Portnum;
                                USID = Properties.Settings.Default.USERID;
                                PWD = Properties.Settings.Default.PWD;
                                remotepath = Properties.Settings.Default.Remotepath;
                                local_path = local_path + @"\data";
                                if (urlAddress != "" && portnum != "" && USID != "" && PWD != "")
                                {
                                    if (uploadFileToFTP(urlAddress, portnum, USID, PWD, m_FileName, remotepath, m_FileName, local_path) == false)
                                    {
                                        Nondbfct.Nondbfct.WriteLog("Error: Could not Connect --(SureCostDispenseRpt)", "SureCostDispenseRpt.txt");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Nondbfct.Nondbfct.WriteLog("btnExportBilling->" + ex.Message, "SureCostDispenseRpt.txt");
                m_FileName = string.Empty;
            }
        }
 
       private int GetDataFromMHAXByDaily(string BData, string EDate)
        {
            string strFromDate = "";
            string strToDate = "";
            string[] LDeptList = null;
            System.IO.StreamWriter file_rpt = null;
            string path = Directory.GetCurrentDirectory();
            string tmp_string, ARscripts;
            AROPFunctions myARfun = new AROPFunctions(m_nondbf);
            m_FileName = string.Empty;

            path = path + "\\data";
            strFromDate = BData;
            strToDate = EDate;

            m_FileName = path + "\\dispense_data_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".csv";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (File.Exists(m_FileName))
                File.Delete(m_FileName);
            file_rpt = new StreamWriter(m_FileName);

            try
            {
                string linex1 = "npi,dateTime,TransactionNumber,RxNumber,payorstatus,quantity,ndc";
                file_rpt.WriteLine(linex1);

                tmp_string = SureCostDispenseRpt.Properties.Settings.Default.RnaDeptList;
                LDeptList = tmp_string.Split(',');
                for (int i = 0; i < LDeptList.Length; i++)
                {
                    m_nondbf.m_dept = LDeptList[i];
                    tmp_string = m_nondbf.m_dept;                 // pre-load pharmacy infomation
                    tmp_string += strFromDate;
                    tmp_string += strToDate;
                    ARscripts = myARfun.GetArScript(1, tmp_string);
                    //debug file ********************************
                    //ARscripts = myARfun.GetArScript(1, m_nondbf.m_dept + "03/13/201808/13/2018");
                    //ARscripts = myARfun.GetArScript(1, m_nondbf.m_dept + "03/13/201803/14/2018");
                    //************************
                    CreateSureCostData(ARscripts, file_rpt);
                }
            }
            catch(Exception ex)
            {
                Nondbfct.Nondbfct.WriteLog("AROP&MHAX->" + ex.Message, "SureCostDispenseRpt.txt");
                m_FileName = string.Empty;
            }
            if (file_rpt != null)
                file_rpt.Close();

            if (m_FileName == "")
                return -1;
            else
                return (0);
        }


        private int GetDataFromAropByDateRange()
        {
            DateTime dtBillingDate;
            string  strBillingDate;
            string  ARscripts = "";
            System.IO.StreamWriter file_rpt = null;
            string  path = Directory.GetCurrentDirectory();
            AROPFunctions myARfun = new AROPFunctions(m_nondbf);

            m_nondbf.m_dept = cmbDept.SelectedValue.ToString().PadRight(2, ' ').Substring(0, 2);
            path = path + "\\data";
            ARscripts = DateTime.Now.ToString("yyyyMMdd");
            try
            {
                m_FileName = path + "\\dispense_data_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".csv";

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                //Report file 
                if (File.Exists(m_FileName))
                    File.Delete(m_FileName);
                file_rpt = new StreamWriter(m_FileName);


                strBillingDate = m_nondbf.m_dept;                 // pre-load pharmacy infomation
                dtBillingDate = dtpFromDate.SelectedDate.Value;
                strBillingDate += dtBillingDate.ToString("MM/dd/yyyy");
                dtBillingDate = dtpToDate.SelectedDate.Value;
                strBillingDate += dtBillingDate.ToString("MM/dd/yyyy");

                string linex1 = "npi,dateTime,TransactionNumber,RxNumber,payorstatus,quantity,ndc";
                file_rpt.WriteLine(linex1);

                ARscripts = myARfun.GetArScript(0, strBillingDate);
                //ARscripts = myARfun.GetArScript(0, "0403/13/201809/13/2018");
                CreateSureCostData(ARscripts, file_rpt);
            }
            catch (Exception ex)
            {
                Nondbfct.Nondbfct.WriteLog("AROP->" + ex.Message, "SureCostDispenseRpt.txt");
                MessageBox.Show("Error:: Arop "+ ex.Message);
                return (-1);
            }
            if (file_rpt != null)
                file_rpt.Close();
            MessageBox.Show("done");
            return (0);
        }


        private void CreateSureCostData(string scripts, System.IO.StreamWriter file_rpt)
        {
            double tmp_double = 0.0;
            string path = Directory.GetCurrentDirectory();
            string FileName = string.Empty;
            int pvt_cnt = 0;
            string currentRX = string.Empty;

            try
            {
                string pharmacyNPI = GetPharmacyNPI(m_nondbf.m_dept);

                try
                {
                    if (m_cmdParameter.PadRight(1, ' ').Substring(0, 1) != "B")
                        pbStatus.Visibility = Visibility.Visible;
                    var Arop_table = OracleDataAccesslayer.OracleDataAccesslayer.GetDataTable(scripts).AsEnumerable();
                    foreach (var r in Arop_table)
                    {
                        pvt_cnt++;
                        if (m_cmdParameter.PadRight(1, ' ').Substring(0, 1) != "B")
                        {
                            if ((pvt_cnt % 100) == 0)
                            {
                                pbStatus.Value += 1;
                                if (pbStatus.Value >= 100)
                                    pbStatus.Value = 0;
                                DoEvents();
                            }
                        }
                        AropProperties arop_rec = new AropProperties();

                        arop_rec.NPI = pharmacyNPI;
                        arop_rec.RxNumber = r["ARXNO"].ToString().Replace("\0", "");
                        currentRX = arop_rec.RxNumber;

                        arop_rec.DateTime = r["ADATEF"].ToString().Replace("\0", "");
                        arop_rec.TransactionNumber = string.Empty;
                        arop_rec.RxNumber = r["ARXNO"].ToString().Replace("\0", "");
                        arop_rec.Quantity = r["AQTY"].ToString().Replace("\0", "");
                        tmp_double = m_nondbf.GetDouble(arop_rec.Quantity);
                        if (tmp_double < 0.01)
                            continue;

                        arop_rec.NDC = string.Empty;

                        if (arop_rec.RxNumber.CompareTo("000000") == 0)          // stock item
                            continue;
                        if (m_nondbf.isNumric(arop_rec.RxNumber.PadRight(6, ' ').Substring(1, 5), 5) == false)
                            continue;

                        
                        if (tmp_double < 0.01)
                            continue;
                        
                        arop_rec.NDC = r["NDC"].ToString().Replace("\0", "");

                        string dataLine = string.Empty;

                        dataLine += arop_rec.NPI + ",";
                        dataLine += arop_rec.DateTime + ",";
                        dataLine += arop_rec.TransactionNumber + ",";
                        dataLine += m_nondbf.m_dept + m_nondbf.convertRx6to7(arop_rec.RxNumber) + ",";
                        dataLine += arop_rec.PayorStatus + ",";
                        dataLine += arop_rec.Quantity + ",";
                        dataLine += arop_rec.NDC;

                        file_rpt.WriteLine(dataLine);



                    }
                }
                catch (Exception ex)
                {
                    Nondbfct.Nondbfct.WriteLog("AROP-XXX->" + currentRX + "--" + ex.Message, "SureCostDispenseRpt.txt");
                }
            }
            catch (Exception ex)
            {
                Nondbfct.Nondbfct.WriteLog("btnExportBilling_Click ::" + ex.Message, "SureCostDispenseRpt.txt");
            }
            if (m_cmdParameter.PadRight(1, ' ').Substring(0, 1) != "B")
            { 
                pbStatus.Value = 0;
                pbStatus.Visibility = Visibility.Hidden;
            }
        }

        public string GetPharmacyNPI(string key)
        {
            string sqlstr = string.Empty;
            string dept = string.Empty;
            dept = key.PadRight(2, ' ').Substring(0, 2).Trim();     //dept
            sqlstr += "SELECT XDEPT, SUBSTR(XBADR,1, 15) AS NPI ,XRNAID,XPNAM FROM XDPT";
            sqlstr += string.Format(" WHERE XDEPT = '{0}'", dept);

            try
            {
                var XDPTtable = OracleDataAccesslayer.OracleDataAccesslayer.GetDataTable(sqlstr).AsEnumerable();

                foreach (var r in XDPTtable)
                {
                    return r["NPI"].ToString().Replace("\0", "").Trim();
                }
            }
            catch (Exception ex)
            {
                Nondbfct.Nondbfct.WriteLog("XDPT->" + ex.Message, "GeriMedDispenseRpt.txt");
            }
            return string.Empty;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                  new Action(delegate { }));
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            int i;

            string LDeptList_string = SureCostDispenseRpt.Properties.Settings.Default.RnaDeptList;
            string[] names;

            names = LDeptList_string.Split(',');
            for (i = 0; i < names.Length; i++)
            {
                cmbDept.Items.Add(names[i]);
            }
            if (names.Length > 0)
                cmbDept.SelectedIndex = 0;

            dtpFromDate.SelectedDate = DateTime.Now.AddDays(-1).Date;
            dtpToDate.SelectedDate = DateTime.Now.AddDays(-1).Date;

        }

        public bool uploadFileToFTP(string m_url, string m_portnum, string m_usr, string m_pwd,
                                    string remote_file, string remote_path, string local_file, string local_path)
        {
            try
            {
                int port = int.Parse(m_portnum);

                using (SftpClient sftp = new SftpClient(m_url, port, m_usr, m_pwd))
                {
                    if (debugLogging == "Y")
                    {
                        Nondbfct.Nondbfct.WriteLog("Connecting...", "SureCostDebug.txt");
                    }

                    sftp.Connect();
                    FileInfo fi = new FileInfo(local_file);
                    var connected = sftp.IsConnected;

                    if (debugLogging == "Y")
                    {
                        if (connected)
                        {
                            Nondbfct.Nondbfct.WriteLog("Connected", "SureCostDebug.txt");
                        }
                        else
                        {
                            Nondbfct.Nondbfct.WriteLog("Not Connected", "SureCostDebug.txt");
                        }
                    }

                    if (debugLogging == "Y")
                    {
                        Nondbfct.Nondbfct.WriteLog("Changing remote directory...", "SureCostDebug.txt");
                    }

                    sftp.ChangeDirectory(remote_path);

                    if (debugLogging == "Y")
                    {
                        Nondbfct.Nondbfct.WriteLog("Uploading file...", "SureCostDebug.txt");
                    }

                    sftp.UploadFile(fi.OpenRead(), fi.Name, true);

                    if (debugLogging == "Y")
                    {
                        Nondbfct.Nondbfct.WriteLog("Disconnecting...", "SureCostDebug.txt");
                    }

                    sftp.Disconnect();

                    if (debugLogging == "Y")
                    {
                        Nondbfct.Nondbfct.WriteLog("Disconnected", "SureCostDebug.txt");
                    }
                }
            }
            catch (Renci.SshNet.Common.SshConnectionException)
            {
                Nondbfct.Nondbfct.WriteLog("uploadFileToFTP::" + "Cannot connect to the server.", "RnaSftpClient.txt");
                return false;
            }
            catch (System.Net.Sockets.SocketException)
            {
                Nondbfct.Nondbfct.WriteLog("uploadFileToFTP::" + "Unable to establish the socket.", "RnaSftpClient.txt");
                return false;
            }
            catch (Renci.SshNet.Common.SshAuthenticationException)
            {
                Nondbfct.Nondbfct.WriteLog("uploadFileToFTP::" + "Authentication of SSH session failed.", "RnaSftpClient.txt");
                return false;
            }
            catch (Exception ex)
            {
                Nondbfct.Nondbfct.WriteLog("uploadFileToFTP::" + ex.Message, "RnaSftpClient.txt");
                return false;
            }
            return true;
        }
    }
}
