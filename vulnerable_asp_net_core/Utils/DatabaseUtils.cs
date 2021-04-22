using System.Data.SQLite;
using System.IO;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace vulnerable_asp_net_core.Utils
{
    public class DatabaseUtils
    {
        private static string _db = "dummy.db";
        
        public static SQLiteConnection _con;
        
        public static string GetConnectionString()
        {
            return "DataSource=" +_db + ";" ;
        }

        public static void init()
        {
            if(File.Exists(_db))
                File.Delete(_db);
            
            SQLiteConnection.CreateFile(_db);
            
            _con = new SQLiteConnection(GetConnectionString());
            _con.Open();
            new SQLiteCommand("create table users(id INT, name varchar(20), pw varchar(20))", _con).ExecuteNonQuery();
            new SQLiteCommand("INSERT INTO users(id, name, pw) VALUES (1, \"alice\", \"alicepw\")", _con).ExecuteNonQuery();
            new SQLiteCommand("INSERT INTO users(id, name, pw) VALUES (2, \"bob\", \"bobpw\")", _con).ExecuteNonQuery();
            new SQLiteCommand("INSERT INTO users(id, name, pw) VALUES (3, \"claire\", \"clairepw\")", _con).ExecuteNonQuery();
            //_con.Close();
        }
    }
}