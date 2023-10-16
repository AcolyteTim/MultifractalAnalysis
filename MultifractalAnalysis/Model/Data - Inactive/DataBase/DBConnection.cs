using Microsoft.Office.Interop.Excel;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MultifractalAnalysis.Model.Data.DataBase
{
    internal class DBConnection
    {
        public DBConnection(string uid, string password) 
        {
            SetConnection(uid, password);
        }

        private const string _server = "52.29.239.198"; // http://sql7.freesqldatabase.com
        private const string _database = "sql7622483";
        private MySqlConnection _connection;

        public MySqlConnection SetConnection(string uid, string password)
        {
            string connectionString;
            connectionString = "SERVER=" + _server + "; PORT = 3306 ;" + "DATABASE=" + _database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            _connection = new MySqlConnection(connectionString);
            return _connection;
        }

        public void OpenConnetion()
        {
            _connection.Open();
        }

        public void CloseConnetion()
        {
            _connection.Close();
        }

        public MySqlConnection GetConnection()
        {
            return _connection;
        }
    }
}
