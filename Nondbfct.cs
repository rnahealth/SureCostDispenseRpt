using System;
//using System.Data.OracleClient;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace Nondbfct
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class Nondbfct
	{
		//public System.Data.OracleClient.OracleConnection m_connect;
		public string m_ConnectionString;
		public string m_dept;
		public string m_database;
		public string m_stationid;
		public string m_diskdirve;
		public string m_login;
        public string m_Rph;
		public string m_returnstr;
		public string m_helpstring;
        public static char FS = (char)28;
        public static char SS = (char)30;
        public static char GS = (char)29;
        public static char EOS = (char)3;
        public static char ETX = (char)0;


        public Nondbfct()
		{
			//
			// TODO: Add constructor logic here
			//
			m_helpstring ="";
			//m_connect = null;
			m_ConnectionString = null;
			m_dept = null;
			m_database = null;
			m_stationid = null;
			m_diskdirve = null;
			m_login = null;
            m_Rph = null;
		}

        public static Process RunningInstance()
        {
            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);
            //MessageBox.Show(current.ProcessName,"Program running");
            //Loop through the running processes in with the same name 
            foreach (Process process in processes)
            {
                //Ignore the current process 
                if (process.Id != current.Id)
                {
                    //Make sure that the process is running from the exe file. 
                    if (Assembly.GetExecutingAssembly().Location.
                         Replace("/", "\\") == current.MainModule.FileName)
                    {
                        ////Return the other process instance.  
                        //MessageBox.Show("Program running");
                        return process;
                    }
                }
            }
            //No other instance was found, return null.  
            return null;
        }
        public static int call_SFTP_Part_Send(
            string exe_path,
            string local_path,
            string FileName,
            string PWD = "netrx",
            string USID = "netrx.",
            string portnum = "22",
            string urlAddress = "207.210.225.242",
            string subdirectory = "habib")
        {
            string prog_exe = "";
            string instruction_file = "";
            string buf_wr = string.Empty;
            string tmpStr = string.Empty;
            string list_file_name = string.Empty;
            int error_cnt = 0;
            string l_FileName = "";
            int i = -1;


            System.IO.StreamWriter filew = null;
            prog_exe = exe_path + "\\RnaSFtpClient.exe";
            if (File.Exists(prog_exe) == false)
                return -1;
            instruction_file = exe_path + "\\SFTPLIST.RNA";
            try
            { 
                if (File.Exists(instruction_file) == true)
                    File.Delete(instruction_file);
                filew = new StreamWriter(instruction_file);
               //'URL`edi1.amerisourcebergen.com`22`020061481`Wr8stu',
                buf_wr = "URL`";
                buf_wr += urlAddress;
                buf_wr += "`";
                buf_wr += portnum;  // port number
                buf_wr += "`";
                buf_wr += USID;     // login id
                buf_wr += "`";
                buf_wr += PWD;      //password
                filew.WriteLine(buf_wr);

                //PUT`11122233.IMX`/rna/polling/list`11122233.IMS`D:\RNAFTPDR`ACK`11122233003.sts`/rna/polling/sts
                //==>PUT`REMOTE FILE NAME`REMOTE PATH`LOCAL FILENAME`LOCAL PATH
                buf_wr = "";
                buf_wr += "PUT`";
                //REMOTE FILE
                i = FileName.LastIndexOf("\\");
                if (i >= 0)
                    l_FileName = FileName.Substring(i + 1, FileName.Length - (i + 1)).Trim();
                else
                {
                    i = FileName.LastIndexOf("/");
                    if (i >= 0)
                        l_FileName = FileName.Substring(i + 1, FileName.Length - (i + 1)).Trim();
                    else
                        l_FileName = FileName;
                }
                tmpStr = l_FileName.Trim();
                buf_wr += tmpStr;
                buf_wr += "`";
                //remote directory
                tmpStr = subdirectory.Trim();
                buf_wr += tmpStr;
                buf_wr += "`";
                //local file
                buf_wr += l_FileName;
                buf_wr += "`";
                //localdir directory
                 buf_wr += local_path.Trim();
                 buf_wr += "`";
                filew.WriteLine(buf_wr);
                filew.Close();
                filew = null;
                RunSystem(exe_path, prog_exe + " " + instruction_file);
                Thread.Sleep(50000);
                error_cnt = 0;
            }
            catch (Exception ex)
            {
                buf_wr = "call_SFTP_Part-->" + ex.Message;
                WriteLog("SFTP->" + ex.Message, "SFTP.txt");
                error_cnt = -1;
            }
            if (filew != null)
                filew.Close();
            return error_cnt;
        }
        public static int RunSystem(string curPath, string arg_parameter)
        {
            ProcessStartInfo procStartInfo;
            Process proc;
            string cmdLine = "";

            try
            {
                procStartInfo = new ProcessStartInfo(); //  cmdLine = "cmd.exe /c copy mstupdt.???? mstupdxx.seq");

                procStartInfo.UseShellExecute = false;
                procStartInfo.FileName = "CMD.EXE";
                procStartInfo.Arguments = "/C " + arg_parameter;

                procStartInfo.RedirectStandardOutput = true;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = false;
                // Now we create a process, assign its ProcessStartInfo and start it
                proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();
                if (result != "")
                    WriteLog(result, "mediFile.txt");
            }
            catch (Exception ex)
            {
                cmdLine = "RunSystem:: " + arg_parameter + " " + ex.Message;
                WriteLog(cmdLine, "mediFile.txt");
                return (-1);
            }
            return 0;
        }


        //public bool connect_oracle(string conn_str,string db_name,string dept)
        //{



        //    if (m_connect == null)
        //    {
        //        try
        //        {
        //            m_connect = new OracleConnection(conn_str);
        //            m_connect.Open();
        //            //m_connect.Close();
        //            m_ConnectionString = conn_str;
        //            m_dept = dept;
        //            m_database = db_name;

        //        }
        //        catch (Exception ex)
        //        {
        //            string str = "";
        //            str = "Oracle Connection Error \n" + ex.Message;
        //            WriteLog(str, "LogFile.Txt");
        //            return false;
        //        }
        //    }
        //    else if (m_connect.State.ToString() != "Open")
        //    {
        //        m_connect.Close();
        //        try
        //        {
        //            m_connect = new OracleConnection(conn_str);
        //            m_connect.Open();
        //            m_ConnectionString = conn_str;
        //            m_dept = dept;
        //            m_database = db_name;
        //        }
        //        catch (Exception ex)
        //        {
        //            string str = "";
        //            str = "Oracle Connection Error \n" + ex.Message;
        //            WriteLog(str, "LogFile.Txt");
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        string queryString = "";
        //        System.Data.OracleClient.OracleDataReader reader1;
        //        System.Data.OracleClient.OracleCommand myCommand;
        //        try
        //        {
        //            m_connect.Close();
        //            queryString = "SELECT XDEPT FROM XDPT";
        //            if (m_connect.State.ToString() != "Open")
        //                m_connect.Open();
        //            myCommand = new System.Data.OracleClient.OracleCommand(queryString, m_connect);
        //            reader1 = myCommand.ExecuteReader();
        //            if (reader1.Read())
        //            {
        //                m_ConnectionString = conn_str;
        //                m_dept = dept;
        //                m_database = db_name;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            string str = "";
        //            str = "Oracle Connection Error \n" + ex.Message;
        //            WriteLog(str, "LogFile.Txt");
        //            return false;
        //        }
        //    }
        //    return true;
        //}

        //public int update_prnt_hist_into_prnt(string billing_enddate)
        //{
        //    string insert_string = "";
        //    string str;
        //    System.Data.OracleClient.OracleCommand myCommand;
        //    int update_flag;

        //    insert_string = "INSERT INTO PRNT(TINDIS,TINTAX,TINDED,TINCOP,";
        //    insert_string += "TINMAX,TRCDCD,TDEPT1,TKEY1,TDEPT2,TKEY2,TDATA,TDATA2,";
        //    insert_string += "FILLER) VALUES(0,0,0,0,0,'A','" + m_dept + "','PARKER','";
        //    insert_string +=  m_dept + "','";
        //    str = "I01" + string.Format("{0:yyyyMMddHHmm}", DateTime.Now) + "'";
        //    for (int i = str.Length; i < 20;i++)
        //        str += "||chr(0)";
        //    insert_string +=  str + ",'";
        //    //tdata
        //    str = billing_enddate + "  "+ m_Rph + string.Format("{0:MM/dd/yyyy HH:mm}", DateTime.Now);
        //    str += "  " + m_stationid;
        //    str = str.PadRight(50,' ').Substring(0,50);
        //    insert_string += str + "','";
        //    //tdata2
        //    str = "";
        //    str = str.PadRight(16,' ');
        //    insert_string += str + "','";       
        //    //filler
        //    str = "";
        //    str = str.PadRight(119,' ');
        //    insert_string += str + "')";
        //    try
        //    {
        //        if (m_connect.State.ToString() != "Open")
        //            m_connect.Open();
        //        myCommand = new System.Data.OracleClient.OracleCommand(insert_string, m_connect);
        //        update_flag = myCommand.ExecuteNonQuery();
        //    }
        //    catch(Exception ex)
        //    {
        //        str = "Prnt update \n" + ex.Message;
        //        WriteLog(str, "LogFile.Txt");
        //        return -1;

        //    }
        //    return (update_flag);
        //}




        public void memcpy(char [] temps, char [] tempd,int len)
			{
			for (int i = 0; i < len; i++)
				temps[i] = tempd[i];
			}
//**********************************************************
		public bool isNumric(string buffer,int length)
		{
			int i;	
			if ( buffer == "")
				return (false);
			if ( buffer.Length < length)
				return (false);
			for (i=0; i<length; i++)					// search buffer
			{
				if (buffer[i] < '0' || buffer[i] > '9')	// valid ascii digit?
					return (false);
			}
			return(true);									// all bytes are valid ascii digits
		}

        public int GetNumric(string buffer)
        {

            int temp_int = 0;
            try
            {
                temp_int = Convert.ToInt32(buffer);
            }
            catch
            {
                temp_int = 0;
            }
            return (temp_int);									// all bytes are valid ascii digits
        }

        public string get_right_char_for_amt(string result_buf, int len)
        {
            result_buf = result_buf.Trim().PadLeft(len,'0');
            if (result_buf.Length > len)
                result_buf = result_buf.Substring(result_buf.Length - len, len);
            if (result_buf[len - 1] == '{')
                result_buf = result_buf.Substring(0, len - 1) + "0";
            else if (result_buf[len - 1] == 'A')
                result_buf = result_buf.Substring(0, len - 1) + "1";
            else if (result_buf[len - 1] == 'B')
                result_buf = result_buf.Substring(0, len - 1) + "2";
            else if (result_buf[len - 1] == 'C')
                result_buf = result_buf.Substring(0, len - 1) + "3";
            else if (result_buf[len - 1] == 'D')
                result_buf = result_buf.Substring(0, len - 1) + "4";
            else if (result_buf[len - 1] == 'E')
                result_buf = result_buf.Substring(0, len - 1) + "5";
            else if (result_buf[len - 1] == 'F')
                result_buf = result_buf.Substring(0, len - 1) + "6";
            else if (result_buf[len - 1] == 'G')
                result_buf = result_buf.Substring(0, len - 1) + "7";
            else if (result_buf[len - 1] == 'H')
                result_buf = result_buf.Substring(0, len - 1) + "8";
            else if (result_buf[len - 1] == 'I')
                result_buf = result_buf.Substring(0, len - 1) + "9";
            return (result_buf);
        }

        public int find_segment_value(string databuf, string key, ref string result_buf)
        {
            int index = 0;
            int len = 0;
            int k = 0;
            string Search_str = "";

            databuf = databuf.Trim();
            k = 0;
            result_buf = "";
            if (databuf.Length > 2000)
                databuf = databuf.Substring(0, 2000);

            len = databuf.Length;
            if (len == 0)
                return k;
            Search_str = string.Format("{0}{1}", Convert.ToChar(28), key);
            //index = databuf.IndexOf(Search_str);
            while ((index = databuf.IndexOf(Search_str, index)) > 0)
            {
                index += 3;
                k = index;
                while (index < databuf.Length
                    && databuf[index] != FS   // 0X1C -->28 FIELD SAPARATOR
                    && databuf[index] != GS   // 0X1D -->29 GROUP SAPARATOR
                    && databuf[index] != SS   // 0X1E -->30 SEGMENT SAPARATOR
                    && databuf[index] != EOS  // 0X03 -->03 END OF TEXT 
                    && databuf[index] != ETX  // 0X00 -->00 NULL
                    )
                    index++;
                if (index - k > 0)
                {
                    if (result_buf != "")
                        result_buf = result_buf + "," + databuf.Substring(k, index - k);
                    else
                        result_buf = databuf.Substring(k, index - k);
                }
            }
            result_buf = result_buf.Trim();
            return k;
        }
        public bool IsValidEmail(string strIn) 
		{ 
			// Return true if strIn is in valid e-mail format. 
			return Regex.IsMatch(strIn, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$"); 
		}


        public bool isDouble(string buffer)
		{
			try
			{
			Convert.ToDouble(buffer);
			}
			catch
			{
			return(false);
			}
			return(true);									// all bytes are valid ascii digits
		}

        public double GetDouble(string buffer)
        {
            double temp_double = 0.00;
            try
            {
                temp_double = Convert.ToDouble(buffer);
            }
            catch
            {
                temp_double = 0.0;
            }
            return (temp_double);									// all bytes are valid ascii digits
        }



//********************************************************************************
        public bool IsDate(string sdate)
        {
            DateTime dt;
            try
            {
                dt = DateTime.Parse(sdate);
            }
            catch
            {
                return false;
            }
            return true;
        }
//********************************************************	
		public void memcmp(char [] temps, char [] tempd,int len)
		{
			string tempstr1 = "";
			string tempstr2 ="";
			int i =0;
			for ( i = 0; i < len; i++)
				{
				tempstr1 = tempstr1+ temps[i];
				tempstr2 = tempstr2+ tempd[i];
				}
		//	tempstr1 = tempd;
			i = tempstr1.CompareTo(tempstr2);
			i = 0;
		}


        /****************************************************************************8
        * this function converts a 6 digit rx number into a 7 digit one.
        * If the first character is less than 9, then there is no need to do any
        * conversion .. just add a zero before the 6 digit rx to make it 7 digit
        * otherwise add decimal 55 to the first characrer, and convert the result
        * into a to byte string and append it to the rest of the   5 digits of teh rx
        * input: the 6 digit rx
        * output: the 7 digit rx
        * **************************************************************************/
        public String convertRx6to7(String sixDigitRx)
        {
            try
            {
                char c = Convert.ToChar(sixDigitRx.Substring(0, 1));    //get the first char
                int firstChar = Convert.ToInt16(c);                //get the ascii value of the integer

                //if its between the numeric value 0 and 9
                if (firstChar < 58 && firstChar > 47)    //then we dont have to convert it. just add a zero in teh start    
                    return "0" + sixDigitRx;

                else
                    firstChar = firstChar - 55;        //subtract 55 (&H37) from the first number
                return firstChar.ToString().Substring(0, 2) + sixDigitRx.Substring(1, 5);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                return null;
            }
        }

        //converts the 7 digit rx number to 6 digit, 
        //if firstbyte is 0 just return teh remaining 6 bytyes
        //else add 55 to the first 2bytes and convert to a char and append the bits 3-7 to it
        public String convertRX7to6(String oldRX)
        {
            try
            {
                int convertedNum = 0;
                if (oldRX.Substring(0, 1) == "0")
                    return oldRX.Substring(1, 6); //if first char is "0" then return the rest of the rx
                else
                    convertedNum = Convert.ToInt16(oldRX.Substring(0, 2)) + 55; //add 55(&H37) to the second 2 characters
                char newChar = Convert.ToChar(convertedNum);        //convert it to a char
                String newRX = newChar.ToString() + oldRX.Substring(2, 5);    //append the char at the
                //start of the old rx. The bits 2-6remain the same
                return newRX;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                return null;
            }
        }
        private static void WriteLog(Exception ex)
        {
            string FilePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Replace(@"file:\", "") + @"\LogFiles\";
            if (!Directory.Exists(FilePath))
                Directory.CreateDirectory(FilePath);

            string FileName = FilePath + "Log.txt";

            string Content = string.Empty;
            Content = "Thread #: " + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + "\r\n" +
                      "Date And Time :" + DateTime.Now.ToString() + " \r\n" +
                      "Error : " + ex.Message + "\r\n" +
                      "StackTrace: " + ex.StackTrace + "\r\n";

             if (ex.InnerException != null)
               {
                   Content += "Inner Exception : " + ex.InnerException + "\r\n";
               }

             Content += "\r\n\r\n";
             try
             {
                 File.AppendAllText(FileName, Content);
             }
            catch
             {
             }
        }


        public void DeleteLogFile(string FilesName)
        {
            string FilePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Replace(@"file:\", "") + @"\LogFiles\";
            if (!Directory.Exists(FilePath))
                Directory.CreateDirectory(FilePath);

            string FileName = FilePath + FilesName;

            try
            {
                if (File.Exists(FileName)) 
                    File.Delete(FileName);
            }
            catch
            {
            }
        }


        public static void WriteLog(string Message, string FilesName)
        {
            string FilePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Replace(@"file:\", "") + @"\LogFiles\";
            if (!Directory.Exists(FilePath))
                Directory.CreateDirectory(FilePath);

            string FileName = FilePath + FilesName;

            string Content = string.Empty;
            Content = "Date And Time :" + DateTime.Now.ToString() + " \r\n" +
                                "Message : " + Message + "\r\n";
            Content += "\r\n\r\n";
            try
            {
                File.AppendAllText(FileName, Content);
            }
            catch
            {
            }
        }
//*************************************************************************************************

        public DateTime GetBeginMonth(DateTime mydate)
        {
            return  (new DateTime(mydate.Year,mydate.Month,1));
        }

        public DateTime GetEndMonth(DateTime mydate)
        {
            int l_mon = 0;
            l_mon = mydate.Month;
            DateTime EndMonth = new DateTime(mydate.Year,mydate.Month,28);
            while ( EndMonth.Month == l_mon)
                EndMonth = EndMonth.AddDays(1);
            EndMonth = EndMonth.AddDays(-1);
            return EndMonth;
        }
        public DateTime GetDateTimeFromStr(String mydatestr)
        {
            mydatestr = mydatestr.PadRight(10,' ');
            string year= mydatestr.Substring(6,4);
            string month= mydatestr.Substring(0,2);
            string day = mydatestr.Substring(3,2);

            if (isNumric(year, 4) && isNumric(month, 2) && isNumric(day, 2))
            {
                if (IsDate(mydatestr))
                    return (new DateTime(Convert.ToInt16(mydatestr.Substring(6, 4)), Convert.ToInt16(mydatestr.Substring(0, 2)), Convert.ToInt16(mydatestr.Substring(3, 2))));
                else
                    return (new DateTime(1800, 1, 1));
            }
            else
                return (new DateTime(1800, 1, 1));

        }




	}
}
