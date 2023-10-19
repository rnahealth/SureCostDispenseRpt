using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Configuration;
using System.Threading;
using System.Data.OleDb;
using Renci.SshNet;
using Renci.SshNet.Sftp;


namespace OracleDataAccesslayer
{
    public class OracleDataAccesslayer
    {
        private static object sqlLock = new object();

        public OracleDataAccesslayer()
        {
        }

        static public OleDbConnection GetConnection()
        {
            int counter = 0;
            OleDbConnection Cn = null;
            //while (counter < 10)
            {
                string LRnaDb = SureCostDispenseRpt.Properties.Settings.Default.RnaDB;
                             

                //string DbName = ConfigurationManager.AppSettings.GetValues("RNADb")[0];
                try
                {
                    if (Cn != null)
                    {
                        Cn.Close();
                        Cn.Dispose();
                        Cn = null;
                    }
                    Cn = new OleDbConnection(@"Provider=OraOLEDB.Oracle;user id=RNAPGM;data source=" + LRnaDb + ";password=rna_rna_202;Persist Security Info=True");
                    //    Cn = new OleDbConnection(@"Provider=MSDAORA.1;Password=rna_rna_202;User ID=RNAPGM;Data Source=" + MainForm.DbName + ";Persist Security Info=True");
                    Cn.Open();
                    counter = 10;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("A transport-level error has occurred when receiving results from the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.") && counter < 10)
                    {
                        Thread.Sleep(1000);
                        counter += 1;
                    }
                    else
                    {
                        counter = 10;
                        throw e;
                    }
                }
            }
            return Cn;
        }

        public static int ExecuteSQL(string SqlQuery, OleDbTransaction T = null)
        {
            int RetVal = -1;
            int counter = 0;
            //while (counter < 10)
            {
                try
                {
                //    Write_SQL_In_Log(SqlQuery);

                    OleDbCommand Cmd = GetCommand(SqlQuery, T);
                    if (Cmd.Connection.ConnectionString.Trim() == "")
                        Cmd.Connection = GetConnection();

                    if (Cmd.Connection.State != ConnectionState.Open && Cmd.Connection.State != ConnectionState.Connecting)
                        Cmd.Connection.Open();

                    if (Cmd.Connection.State == ConnectionState.Closed)
                        Cmd.Connection.Open();
                    else
                    {
                        RetVal = Cmd.ExecuteNonQuery();
                        Cmd = GetCommand("commit", T);
                        Cmd.ExecuteNonQuery();
                        //Zack Date: 11/30/2011 Don't Dispose Connection If transaction is being used
                        if (T == null)
                        {
                            Cmd.Connection.Close();
                            Cmd.Connection.Dispose();
                            Cmd.Dispose();
                            Cmd = null;
                        }
                        counter = 10;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("A transport-level error has occurred when receiving results from the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.") && counter < 10)
                    {
                        Thread.Sleep(1000);
                        counter += 1;
                    }
                    else
                    {
                        counter = 10;
                        throw e;
                    }
                }
            }
            return RetVal;
        }

        public static int ExecuteSQL(string SqlQuery, OleDbConnection Cn, OleDbTransaction T = null)
        {
            int RetVal = -1;
            int counter = 0;
            //while (counter < 10)
            {
                try
                {
                    if (Cn != null)
                    {
                        if (Cn.State != ConnectionState.Open && Cn.State != ConnectionState.Connecting)
                            Cn.Open();

                        OleDbCommand Cmd = null;
                        if (T == null)
                            Cmd = new OleDbCommand(SqlQuery, Cn);
                        else
                            Cmd = new OleDbCommand(SqlQuery, Cn, T);
                        RetVal = Cmd.ExecuteNonQuery();
                        counter = 10;
                    }
                    else
                    {
                        counter = 10;
                        throw new Exception("Connection Not Initialized");
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("A transport-level error has occurred when receiving results from the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.") && counter < 10)
                    {
                        Thread.Sleep(1000);
                        counter += 1;
                    }
                    else
                    {
                        counter = 10;
                        throw e;
                    }
                }
            }
            return RetVal;
        }

        public static OleDbCommand GetCommand(string SqlQuery, OleDbTransaction T = null)
        {
            OleDbCommand Cmd = null;
            OleDbConnection Cn2 = T == null ? GetConnection() : T.Connection;
            try
            {
                if (Cn2 != null)
                {
                    if (Cn2.State != ConnectionState.Open && Cn2.State != ConnectionState.Connecting)
                        Cn2.Open();

                    if (T == null)
                        Cmd = new OleDbCommand(SqlQuery, Cn2);
                    else
                        Cmd = new OleDbCommand(SqlQuery, T.Connection, T);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return Cmd;
        }

        public static OleDbCommand GetCommand(string SqlQuery, ref OleDbConnection Cn)
        {
            OleDbCommand Cmd = null;
            int counter = 0;
            //while (counter < 10)
            {
                try
                {
                    if (Cn != null)
                    {
                        if (Cn.State != ConnectionState.Open && Cn.State != ConnectionState.Connecting)
                            Cn.Open();

                        Cmd = new OleDbCommand(SqlQuery, Cn);
                        counter = 10;
                    }
                    else
                    {
                        counter = 10;
                        throw new Exception("Connection Not Initialzed");
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("A transport-level error has occurred when receiving results from the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.") && counter < 10)
                    {
                        Thread.Sleep(1000);
                        counter += 1;
                    }
                    counter = 10;
                    throw e;
                }
            }
            return Cmd;
        }

        public static int ExecuteCommand(OleDbCommand Cmd)
        {
            int RetVal = -1;
            int counter = 0;
            //while (counter < 10)
            {
                try
                {
                    if (Cmd.Connection != null)
                    {
                        if (Cmd.Connection.State != ConnectionState.Open && Cmd.Connection.State != ConnectionState.Connecting)
                            Cmd.Connection.Open();

                        if (Cmd.Connection.State == ConnectionState.Closed)
                            Cmd.Connection.Open();

                        RetVal = Cmd.ExecuteNonQuery();
                        counter = 10;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("A transport-level error has occurred when receiving results from the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.") && counter < 10)
                    {
                        Thread.Sleep(1000);
                        counter += 1;
                    }
                    else
                    {
                        counter = 10;
                        throw e;
                    }
                }
            }
            if (Cmd != null)
            {
                Cmd.Connection.Close();
                Cmd.Connection.Dispose();
            }
            return RetVal;
        }

        public static int ExecuteCommandNoClose(OleDbCommand Cmd)
        {
            int RetVal = -1;
            try
            {
                if (Cmd.Connection != null)
                {
                    RetVal = Cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
            }
            return RetVal;
        }

        static public OleDbDataReader ExecuteReader(string SqlQuery)
        {
            //Write_SQL_In_Log(SqlQuery);
            OleDbConnection Cn = GetConnection();
            OleDbCommand Cmd = GetCommand(SqlQuery, ref Cn);
            return Cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        static public OleDbDataReader ExecuteReader(string SqlQuery, OleDbConnection Cn)
        {
            OleDbCommand Cmd = GetCommand(SqlQuery, ref Cn);
            return Cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public static DataSet GetDataSet(string SqlQuery)
        {
            OleDbCommand Cmd = null;
            DataSet ds = new DataSet();
            try
            {
                Cmd = GetCommand(SqlQuery);
                OleDbDataAdapter Adp = new OleDbDataAdapter(Cmd);
                Adp.Fill(ds);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (Cmd != null)
                {
                    if (Cmd.Connection != null)
                    {
                        Cmd.Connection.Close();
                        Cmd.Connection.Dispose();
                    }
                }
            }
            return ds;
        }

        public static DataTable GetDataTable(string SqlQuery, OleDbTransaction trans = null)
        {
            OleDbCommand Cmd = null;
            OleDbDataAdapter Adp = null;
            DataTable dt = new DataTable();

            int counter = 0;
            //while (counter < 10)
            {
                try
                {
                    //Write_SQL_In_Log(SqlQuery);

                    if (trans == null)
                        Cmd = GetCommand(SqlQuery);
                    else
                        Cmd = new OleDbCommand(SqlQuery, trans.Connection, trans);

                    if (Cmd.Connection.ConnectionString.Trim() == "")
                        Cmd.Connection = GetConnection();

                    if (Cmd.Connection.State != ConnectionState.Open && Cmd.Connection.State != ConnectionState.Connecting)
                        Cmd.Connection.Open();

                    if (Cmd.Connection.State == ConnectionState.Closed)
                        Cmd.Connection.Open();


                    Adp = new OleDbDataAdapter(Cmd);
                    Adp.Fill(dt);
                    counter = 10;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("A transport-level error has occurred when receiving results from the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.") && counter < 10)
                    {
                        Thread.Sleep(1000);
                        counter += 1;
                    }
                    else
                    {
                        counter = 10;
                        throw e;
                    }
                }
            }
            if (trans == null)
            {
                if (Adp != null)
                {
                    Adp.Dispose();
                    Adp = null;
                }
                if (Cmd != null)
                {
                    Cmd.Connection.Close();
                    Cmd.Connection.Dispose();
                    Cmd.Dispose();
                    Cmd = null;
                }
            }

            return dt;
        }

        public static DataTable GetDataTable(OleDbCommand Cmd)
        {
            DataTable dt = new DataTable();
            int counter = 0;
            try
            {
                OleDbDataAdapter Adp = new OleDbDataAdapter(Cmd);
                Adp.Fill(dt);
                counter = 10;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("A transport-level error has occurred when receiving results from the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.") && counter < 10)
                {
                    Thread.Sleep(1000);
                    counter += 1;
                }
                else
                {
                    counter = 10;
                    throw e;
                }
            }
            finally
            {
                if (Cmd != null)
                {
                    if (Cmd.Connection != null)
                    {
                        Cmd.Connection.Close();
                        Cmd.Connection.Dispose();
                    }
                }
            }
            return dt;
        }

        public static DataSet GetDataSet(string SqlQuery, OleDbConnection Cn)
        {
            OleDbCommand Cmd = null;
            DataSet ds = new DataSet();
            int counter = 0;
            try
            {
                if (Cn != null)
                {
                    Cmd = new OleDbCommand(SqlQuery, Cn);
                }
                OleDbDataAdapter Adp = new OleDbDataAdapter(Cmd);
                Adp.Fill(ds);
                counter = 10;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("A transport-level error has occurred when receiving results from the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.") && counter < 10)
                {
                    Thread.Sleep(1000);
                    counter += 1;
                }
                else
                {
                    counter = 10;
                    throw e;
                }
            }
            return ds;
        }

        public static object ExecuteScalar(string SqlQuery, OleDbConnection Cn)
        {
            object RetVal = -1;
            int counter = 0;
            //while (counter < 10)
            {
                try
                {
                    if (Cn != null)
                    {
                        OleDbCommand Cmd = new OleDbCommand(SqlQuery, Cn);
                        RetVal = Cmd.ExecuteScalar();
                        counter = 10;
                    }
                    else
                    {
                        counter = 10;
                        throw new Exception("Connection Not Initialized");
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("A transport-level error has occurred when receiving results from the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.") && counter < 10)
                    {
                        Thread.Sleep(1000);
                        counter += 1;
                    }
                    else
                    {
                        counter = 10;
                        throw e;
                    }
                }
            }

            return RetVal;
        }

        public static object ExecuteScalar(string SqlQuery, OleDbTransaction T = null)
        {
            object RetVal = -1;
            int counter = 0;
            //while (counter < 10)
            {
                try
                {
                    //Zack Date: 11/30/2011 Don't Dispose Connection If transaction is not being used
                    if (T == null)
                    {
                        OleDbCommand Cmd = GetCommand(SqlQuery, T);
                        if (Cmd.Connection.ConnectionString.Trim() == "" && T == null)
                            Cmd.Connection = GetConnection();
                        else if (T != null)
                            Cmd.Connection = T.Connection;

                        if (Cmd.Connection.State != ConnectionState.Open && Cmd.Connection.State != ConnectionState.Connecting)
                            Cmd.Connection.Open();

                        if (Cmd.Connection.State == ConnectionState.Closed)
                            Cmd.Connection.Open();
                        else
                        {
                            RetVal = Cmd.ExecuteScalar();
                            if (T == null)
                            {
                                Cmd.Connection.Close();
                                Cmd.Connection.Dispose();
                                Cmd.Dispose();
                                Cmd = null;
                            }

                        }
                        counter = 10;
                    }
                    else
                    {
                        if (T.Connection != null)
                        {
                            if (T.Connection.State != ConnectionState.Open && T.Connection.State != ConnectionState.Connecting)
                                T.Connection.Open();

                            OleDbCommand Cmd = null;
                            Cmd = new OleDbCommand(SqlQuery, T.Connection, T);

                            RetVal = Cmd.ExecuteScalar();
                            counter = 10;
                        }
                        else
                        {
                            counter = 10;
                            throw new Exception("Connection Not Initialized");
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("A transport-level error has occurred when receiving results from the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.") && counter < 10)
                    {
                        Thread.Sleep(1000);
                        counter += 1;
                    }
                    else
                    {
                        counter = 10;
                        throw e;
                    }
                }
            }
            return RetVal;
        }

        public static object ExecuteScalar(string SqlQuery, OleDbConnection Cn, OleDbTransaction T = null)
        {
            object RetVal = -1;
            int counter = 0;
            //while (counter < 10)
            {
                try
                {
                    if (Cn != null)
                    {
                        if (Cn.State != ConnectionState.Open && Cn.State != ConnectionState.Connecting)
                            Cn.Open();

                        OleDbCommand Cmd = null;
                        if (T == null)
                            Cmd = new OleDbCommand(SqlQuery, Cn);
                        else
                            Cmd = new OleDbCommand(SqlQuery, T.Connection, T);

                        RetVal = Cmd.ExecuteScalar();
                        counter = 10;
                    }
                    else
                    {
                        counter = 10;
                        throw new Exception("Connection Not Initialized");
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("A transport-level error has occurred when receiving results from the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.") && counter < 10)
                    {
                        Thread.Sleep(1000);
                        counter += 1;
                    }
                    else
                    {
                        counter = 10;
                        throw e;
                    }
                }
            }
            return RetVal;
        }

        public static object ExecuteScalar(OleDbCommand Cmd)
        {
            object RetVal = null;
            int counter = 0;
            //while (counter < 10)
            {
                try
                {
                    if (Cmd.Connection != null)
                    {
                        RetVal = Cmd.ExecuteScalar();
                        counter = 10;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("A transport-level error has occurred when receiving results from the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.") && counter < 10)
                    {
                        Thread.Sleep(1000);
                        counter += 1;
                    }
                    else
                    {
                        counter = 10;
                        throw e;
                    }
                }
            }
            if (Cmd.Connection != null)
            {
                Cmd.Connection.Close();
                Cmd.Connection.Dispose();
            }
            return RetVal;
        }

        static public OleDbTransaction BeginTransaction()
        {
            OleDbTransaction T = null;
            OleDbConnection Cn = null;
            int counter = 0;
            try
            {
                Cn = GetConnection();
                if (Cn.State != ConnectionState.Open && Cn.State != ConnectionState.Connecting)
                    Cn.Open();

                T = Cn.BeginTransaction();
                counter = 10;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("A transport-level error has occurred when receiving results from the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.") && counter < 10)
                {
                    Thread.Sleep(1000);
                    counter += 1;
                }
                else
                {
                    counter = 10;
                    throw e;
                }
            }
            return T;
        }

        static public void CloseTransaction(OleDbTransaction T)
        {
            try
            {
                if (T.Connection != null)
                {
                    T.Connection.Close();
                    T.Connection.Dispose();
                }
                T.Dispose();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static OleDbParameterCollection GetParameterCollection()
        {
            OleDbCommand Cmd = new OleDbCommand();
            return Cmd.Parameters;
        }

        public static DataSet GetDataSet(string SqlQuery, OleDbParameterCollection SPC)
        {
            OleDbCommand Cmd = null;
            DataSet ds = new DataSet();
            try
            {
                Cmd = GetCommand(SqlQuery);
                foreach (OleDbParameter Sp in SPC)
                {
                    Cmd.Parameters.Add(new OleDbParameter(Sp.ParameterName, Sp.Value));
                }
                OleDbDataAdapter Adp = new OleDbDataAdapter(Cmd);
                Adp.Fill(ds);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (Cmd != null)
                {
                    if (Cmd.Connection != null)
                    {
                        Cmd.Connection.Close();
                        Cmd.Connection.Dispose();
                    }
                }
            }
            return ds;
        }

        private static bool CheckForInjection(string Text)
        {
            bool RetVal = false;
            try
            {
                for (int i = 0; i < Text.Length; i++)
                {
                    if (Text[i] == ' ')
                    {
                        RetVal = false;
                        break;
                    }
                    else
                    {
                        RetVal = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return RetVal;
        }

        private static void Write_SQL_In_Log(string sql)
        {
            if (!Directory.Exists(Environment.CurrentDirectory + @"\Logs\"))
                Directory.CreateDirectory((Environment.CurrentDirectory + @"\Logs\"));

            string FileName = Environment.CurrentDirectory + @"\Logs\SQLs.log";

            string sCurrentTime = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt");
            string Message = sCurrentTime + "\r\n";
            Message += sql + "\r\n\r\n\r\n";

            lock (sqlLock)
            {
                File.AppendAllText(FileName, Message);
                ArchiveFileIfNecessary(FileName);
            }
        }

        private static void ArchiveFileIfNecessary(string sourceFile)
        {
            FileInfo fileInfo = new FileInfo(sourceFile);

            double fileSizeMB = (double)fileInfo.Length / 1024.0 / 1024.0;

            if (fileSizeMB > 10)
            {
                if (!Directory.Exists(Environment.CurrentDirectory + @"\Logs\Archive\"))
                {
                    Directory.CreateDirectory(Environment.CurrentDirectory + @"\Logs\Archive\");
                }

                string date = DateTime.Now.ToString("yyyyMMddHHmmss");
                string destFile = sourceFile.Replace(@"Logs\", @"Logs\Archive\");
                destFile = destFile.Replace(@".log", "-" + date + ".log");

                File.Move(sourceFile, destFile);
            }
        }
    }
}
