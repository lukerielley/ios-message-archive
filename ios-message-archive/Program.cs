using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.SQLite.EF6;
using System.Globalization;

namespace ios_message_archive
{
    class Program
    {
        

        static void Main(string[] args)
        {

            var iOSEpoch = new DateTime(2001, 01, 01);

            /*
            var m_dbConnection = new SQLiteConnection("Data Source=C:\\LaurenMessage\\3d0d7e5fb2ce288813306e4d4636395e047a3d28;Version=3;");

            m_dbConnection.Open();

            var sql = "SELECT * FROM message;";

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

            SQLiteDataReader reader = command.ExecuteReader();

            Console.WriteLine("There were {0} row(s)", reader);

            
            while (reader.Read())
            {
                Console.WriteLine(reader["text"]);
            }
            */

            var dtTicks = 493288858;

            var dt = iOSEpoch.AddSeconds(dtTicks).ToLocalTime();

            Console.WriteLine("{0}, {1}", dt.ToLongDateString(), dt.ToLongTimeString());

            Console.ReadKey();

        }
    }
}
