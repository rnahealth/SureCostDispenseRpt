using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SureCostDispenseRpt
{
    public class AropProperties
    {
        public string NPI { get; set; }
        public string DateTime { get; set; }
        public string TransactionNumber { get; set; }
        public string RxNumber { get; set; }
        public string PayorStatus { get; set; }
        public string Quantity { get; set; }
        public string NDC { get; set; }
    }

    public class AROPFunctions
    {
        public Nondbfct.Nondbfct m_nondbfct;

        public AROPFunctions(Nondbfct.Nondbfct nondbfct)
        {
            m_nondbfct = nondbfct;
        }



        public string GetArScript(int script_num, string key)
        {
            string sqlstr = "";
            string key1 = "";
            string key2 = "";
            string dept = "";
            key = key.PadRight(30, ' ');
            if (script_num == 0)
            {
                dept = key.Substring(0, 2);     //dept
                key1 = key.Substring(2, 10);    //billdate from (MM/DD/YYYY)
                key2 = key.Substring(12, 10);    //billdate to   (MM/DD/YYYY) 

                if (!m_nondbfct.isNumric(dept, 2)) throw new Exception("Is not Valid number :" + dept);
                if (!m_nondbfct.IsDate(key1)) throw new Exception("Is not Valid Date :" + key1);
                if (!m_nondbfct.IsDate(key2)) throw new Exception("Is not Valid Date :" + key1);
                sqlstr = "";
                sqlstr += " SELECT ADEPT";
                sqlstr += ",TO_CHAR(ADATEF, 'YYYY-MM-DD') AS ADATEF";
                sqlstr += ",LPAD(ASCII(SUBSTR(ATIMES, 1, 1)), 2, '0') || LPAD(ASCII(SUBSTR(ATIMES, 2, 1)), 2, '0') || LPAD(ASCII(SUBSTR(ATIMES, 3, 1)), 2, '0') AS ATIMES";
                sqlstr += ",ARXNO";
                sqlstr += ",AQTY";
                sqlstr += ",SUBSTR(APYACCT,21,11) AS NDC";

                sqlstr += " FROM AROP";
                sqlstr += string.Format(" WHERE adept = '{0}' and\n", dept);
                sqlstr += string.Format(" ADATEF between to_date('{0}', 'MM/DD/YYYY') and to_date('{1}', 'MM/DD/YYYY')\n", key1, key2);
                sqlstr += " and arcdcd = 'A' and\n";
                sqlstr += " apric != ' ' and\n";
                sqlstr += " aartyp in ('CA', 'CH', 'AJ') and\n";
                sqlstr += " aseqno < 'A'\n";
              
                ////DEGUG
                //sqlstr += " AND AACCT = '001023'";  // revers
                //sqlstr += " AND ARXNO ='G46816'";
                //************************* DEBUG.
            }
            else if (script_num == 1)
            {
                dept = key.Substring(0, 2);     //dept
                //key1 = key.Substring(2, 10);    //billdate from (MM/DD/YYYY)
                key1 = key.Substring(8, 4) + key.Substring(2, 2) + key.Substring(5, 2); //MHAX CREATE DATE from (YYYYMMDD)
                key2 = key.Substring(18, 4) + key.Substring(12, 2) + key.Substring(15, 2); //MHAX CREATE DATE from (YYYYMMDD)
                if (!m_nondbfct.isNumric(dept, 2)) throw new Exception("Is not Valid number :" + dept);
                if (!m_nondbfct.isNumric(key1,8)) throw new Exception("Is not Valid Date :" + key1);
                if (!m_nondbfct.isNumric(key2, 8)) throw new Exception("Is not Valid Date :" + key2);
                sqlstr = "";
                sqlstr += " SELECT AR.ADEPT AS ADEPT";
                sqlstr += ",TO_CHAR(AR.ADATEF, 'YYYY-MM-DD') AS ADATEF";
                sqlstr += ",LPAD(ASCII(SUBSTR(ATIMES, 1, 1)), 2, '0') || LPAD(ASCII(SUBSTR(ATIMES, 2, 1)), 2, '0') || LPAD(ASCII(SUBSTR(ATIMES, 3, 1)), 2, '0') AS ATIMES";
                 sqlstr += ",AR.ARXNO AS ARXNO";
                sqlstr += ",AQTY";
                sqlstr += ",SUBSTR(APYACCT,21,11) AS NDC";

                sqlstr += " FROM MHAX MH \n";
                sqlstr += " INNER JOIN AROP AR ON AR.ADEPT = MH.ADEPT AND AR.AACCT = MH.AACCT AND ";
                sqlstr += " AR.APATNM = MH.APATNM AND AR.AITEM = MH.AITEM AND AR.ADATEF = MH.ADATEF AND AR.ASEQNO = MH.ASEQNO\n";
                sqlstr += $" WHERE MH.ADEPT = '{dept}' AND MH.CURRENTDATE >= '{key1}' AND MH.CURRENTDATE <= '{key2}'\n";
                sqlstr += " AND AR.ARCDCD = 'A' AND\n";
                sqlstr += " AR.APRIC != ' ' AND\n";
                sqlstr += " AR.AARTYP IN ('CA', 'CH', 'AJ') AND\n";
                sqlstr += " AR.ASEQNO < 'A'\n";
                ////DEGUG
                //sqlstr += " AND AR.AACCT = '001023'";  // revers
                //sqlstr += " AND AR.ARXNO ='G46816'";
                //************************* DEBUG.
                sqlstr += " ORDER BY AR.ADEPT,AR.AACCT,AR.APATNM,AR.AITEM,AR.ADATEF,AR.ASEQNO";
            }
            sqlstr = sqlstr.Replace('\n', ' ');
            return sqlstr;
        }
    }
}
