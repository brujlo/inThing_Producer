using inThing.MODEL;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace inThing.DAL
{
    public class NpgSql
    {
        //private string cs = "Host=localhost;Username=postgres;Password=postgres;Database=inthing";
        private string cs;
        private NpgsqlConnection con = new NpgsqlConnection();

        public NpgSql(string _cs) { cs = _cs; }

        private void Connect()
        {
            con = new NpgsqlConnection(cs);

            if(con.State != ConnectionState.Open)
                con.Open();
        }

        private void Disconnect()
        {
            if (con.State == ConnectionState.Open)
            {
                con.Close();
                con.Dispose();
            }
        }

        public bool AddActivityToDB(Activity act)
        {
            try
            {
                Connect();

                string query = "INSERT INTO inthing.activities(activity, type, participants, price, link, key, accessibility) " +
                "VALUES(" +
                "'" + act.activity +
                "', '" + act.type +
                "', " + act.participants +
                ", " + act.price.ToString().Replace(',','.') +
                ", '" + act.link +
                "', '" + act.key +
                "', " + act.accessibility.ToString().Replace(',', '.') + ")";


                var cmd = new NpgsqlCommand(query, con);

                cmd.ExecuteNonQuery();

                Disconnect();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
    }
}


