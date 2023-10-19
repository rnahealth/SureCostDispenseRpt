using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rebex.Net;
using System.IO;

namespace RnaFTPSCommunication_SSL_TLS
{
    public class RnaSFTPCommunication
    {
        public string m_url { get; set; }
        public string m_pwd { get; set; }
        public string m_usr { get; set; }
        public int m_portnum { get; set; }
        public string m_passphrase { get; set; }
        public bool m_isConnected { get; set; }
        public string m_localPath { get; set; }
        public string m_localFilename { get; set; }
        public string m_romotepath { get; set; }
        public string m_romoteFilename { get; set; }
        private Ftp m_sftp;                         // SFTP session
        private bool m_isWorking;                    // determines whether any operation is running
        private string m_ftprootDir;
        Nondbfct.Nondbfct _nonDbf = new Nondbfct.Nondbfct();
        //        string[] arg_array;
        public RnaSFTPCommunication(string urlAddress, string loginid, string password, string portnum)
        {
            m_url = urlAddress;
            m_pwd = password;
            m_usr = loginid;
            //PORT NUMBER
            if (_nonDbf.isNumric(portnum.Trim(), portnum.Trim().Length))
                m_portnum = Convert.ToInt32(portnum.Trim());
            else
                m_portnum = 0;
            m_isConnected = false;
            m_localPath = "";
            m_localFilename = "";
            m_romotepath = "";
            m_romoteFilename = "";
            m_sftp = null;
            m_isWorking = false;
            m_ftprootDir = "";
            m_passphrase = "";
        }
        public void disconnect_sftp()
        {
            if (m_sftp != null)
            {
                m_sftp.Disconnect();
                m_sftp.Dispose();
                m_sftp = null;
                m_isWorking = false;
            }
        }

        public int connect_sftp()
        {
            if (m_sftp != null)
            {
                if (m_isWorking == true)
                    return (-5);
                m_sftp.Disconnect();
                m_sftp.Dispose();
                m_sftp = null;
            }

            if (m_url.Trim().Length <= 5)
                return (-1);                 // url address error
            if (m_portnum <= 0)
                return (-2);                 // portnumber error
            if (m_usr.Trim() == "")
                return (-3);                 // login or user name error
                                             // if (m_ssh_privatekeyfilename.Trim() == "")
           if (m_pwd.Trim().Length <= 3)
              return (-4);  // password error

            // create sftp object            // create FTP client instance
            m_sftp = new Rebex.Net.Ftp();
            try
            {
                m_sftp.Timeout = 30000;
                // m_sftp.Options |= SftpOptions.UseLargeBuffers;
                try
                {
                    m_isWorking = true;
                    m_sftp.Passive = true;
                    m_sftp.Settings.SslAllowedSuites = Rebex.Net.TlsCipherSuite.Secure;
                    m_sftp.Settings.SslAllowedVersions = Rebex.Net.TlsVersion.SSL30 | Rebex.Net.TlsVersion.TLS10 | Rebex.Net.TlsVersion.TLS11 | Rebex.Net.TlsVersion.TLS12;
                    m_sftp.Settings.SslAcceptAllCertificates = true;
                    // set UTF-8 encoding for commands and responses
                    m_sftp.Encoding = Encoding.UTF8;
                    // use large transfer buffers
                    m_sftp.Settings.UseLargeBuffers = true;
                    m_sftp.TransferType = FtpTransferType.Binary;


                    //memberdata.SureCostgso.com:7935
                    //Explicit FTP over TLS
                    //Username: RNAFTP
                    //Username = RNAFTP
                    //Password = b@dArt70

                    // disable MLST extension
                    m_sftp.EnabledExtensions &= ~FtpExtensions.MachineProcessingList;
                    // m_sftp.Connect("memberdata.SureCostgso.com", 7935, SslMode.Explicit);
                     m_sftp.Connect(m_url, m_portnum, SslMode.Explicit);
                    // authenticate securely
                }
                catch (Exception ex)
                {
                    //NEW CODE 03/27/2018 HABIB
                    Nondbfct.Nondbfct.WriteLog("Connect(IsCompleted)::" + ex.Message, "RnaftpsClient.txt");
                    return (-1);
                }
                finally
                {
                    //btnStop.Visible = false;
                    m_isWorking = false;
                }
                m_sftp.Login(m_usr, m_pwd);
            }
            catch (Exception ex)
            {
                 Nondbfct.Nondbfct.WriteLog("Connect::" + ex.Message, "RnaftpsClient.txt");
                m_sftp.Dispose();
                m_sftp = null;
                return (-1);
            }
            finally
            {
                m_isWorking = false;
            }
            m_ftprootDir = m_sftp.GetCurrentDirectory();
            return (0);
        }


        public int GetFile_ftps(string remote_file, string remote_path, string local_file, string local_path, string del_flag_str)
        {
            int del_flag;
            int index_i = -1;

            if (m_sftp == null)
                return (-1);   // not connect;
            if (m_isWorking)
                return (0);

            remote_path = remote_path.Replace('\\', '/');

            // Remote File name
            if (remote_file.Trim().Length <= 3)
                return (-1);

            // Remote File path
            if (remote_path.Trim().Length == 0)
                remote_path = "";
            // Local File name
            if (local_file.Trim().Length < 3)
                return (-1);

            // Local File path
            if (local_path.Trim().Length < 3)
                return (-1);

            if (_nonDbf.isNumric(del_flag_str.Trim(), del_flag_str.Trim().Length))
                del_flag = Convert.ToInt16(del_flag_str.Trim());
            else
                del_flag = 0;

            string curr_dir = m_ftprootDir + remote_path;

            if (remote_path.Length > 0)
                if (remote_path.Substring(0, 1) == "/" || remote_path.Substring(0, 1) == "\\")
                    curr_dir = remote_path;
            try
            {
                m_sftp.ChangeDirectory(curr_dir);
                curr_dir = m_sftp.GetCurrentDirectory();

                index_i = remote_file.IndexOf("*");
                if (index_i > 2)
                {
                    // retrieve and display the list of files and directories
                    FtpItemCollection list = m_sftp.GetList();
                    foreach (FtpItem item in list)
                    {
                        if (item.Type ==    FtpItemType.File)
                        {
                            if (item.Name.Length >= index_i)
                            {
                                if (item.Name.Substring(0, index_i) == remote_file.Substring(0, index_i))
                                {
                                    curr_dir = local_path + "\\" + local_file;
                                    m_sftp.GetFile(item.Name, curr_dir);
                                    if (m_sftp.FileExists(item.Name) && del_flag == 1)
                                        m_sftp.DeleteFile(item.Name);
                                    return (0);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // retrieve and display the list of files and directories
                    if (m_sftp.FileExists(remote_file))
                    {
                        curr_dir = local_path + "\\" + local_file;
                        m_sftp.GetFile(remote_file, curr_dir);
                        if (del_flag == 1)
                            m_sftp.DeleteFile(remote_file);
                    }
                }
            }
            catch (Exception ex)
            {
                Nondbfct.Nondbfct.WriteLog("Get_file::" + ex.Message, "RnaSftpClient.txt");
                return (-1);
            }
            return (0);
        }
        public int PutFile_ftps(string remote_file, string remote_path, string local_file, string local_path)
        {
            int imsdata = 0;
            string tmp_str = "";


            if (m_sftp == null)
                return (-1);
            if (m_isWorking)
                return (-1);

            if (local_file.Length > 5)
            {
                if (local_file.Substring(local_file.Length - 4, 4) == ".IMS")
                    imsdata = 1;
            }
            remote_path = remote_path.Replace('\\', '/').Trim();
            remote_file = remote_file.Trim();
            // Remote File name
            if (remote_file.Length <= 3)
                return (-1);

            // Remote File path
            if (remote_path.Length == 0)
                remote_path = "";
            tmp_str = remote_path + "/" + remote_file;

            // Local File name
            if (local_file.Trim().Length < 3)
                return (-1);

            // Local File path
            if (local_path.Trim().Length < 3)
                return (-1);

            //remote_path =  "/out";
            //remote_path =  "/rna/public";
            string curr_dir = m_ftprootDir + remote_path;
            if (remote_path.Length > 0)
                if (remote_path.Substring(0, 1) == "/" || remote_path.Substring(0, 1) == "\\")
                    curr_dir = remote_path;
            try
            {
                m_sftp.ChangeDirectory(curr_dir);
                curr_dir = local_path + "\\" + local_file;
                // upload a local file to server 
                m_sftp.PutFile(curr_dir, remote_file);
                if (imsdata == 1)
                    File.Delete(curr_dir);

                //NEW CODE 03/27/2018 HABIB
                if (m_sftp.FileExists(remote_file) == false)
                    Nondbfct.Nondbfct.WriteLog("Put_file(not exist):: " + tmp_str, "fileLost.txt");

            }
            catch (Exception ex)
            {
                //NEW CODE 03/27/2018 HABIB
                Nondbfct.Nondbfct.WriteLog("Put_file::" + ex.Message, "RnaSftpClient.txt");
                return (-1);
            }
            return (0);
        }

    }
}
