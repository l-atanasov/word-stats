using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WordStats.Db
{
    public class SQLite : IDisposable
    {
        private SQLiteConnection connection = null;
        private string file;

        public SQLite(string file)
        {
            this.file = file;
        }

        public void OpenConnection()
        {
            if (!File.Exists(file))
            {
                CreateDatabaseFromScratch();
            }
            else
            {
                OpenConnectionToExistingDatabase();
            }
        }

        private void CreateDatabaseFromScratch()
        {
            SQLiteConnection.CreateFile(file);
            OpenConnectionToExistingDatabase();
            using(Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WordStats.Db.CreateDb.sql"))
            using(StreamReader reader = new StreamReader(stream))
            {
                string sql = reader.ReadToEnd();
                ExecuteUpdate(sql);
            }
        }

        private void OpenConnectionToExistingDatabase()
        {
            connection = new SQLiteConnection("Data Source=" + file + ";version=3");
            connection.Open();
        }

        public int ExecuteUpdate(string sql)
        {
            return ExecuteUpdate(sql, null);
        }

        public int ExecuteUpdate(string sql, ICollection<SQLiteParameter> parameters)
        {
            SQLiteCommand command = CreateCommand(sql, parameters);
            return command.ExecuteNonQuery();
        }

        public SQLiteDataReader ExecuteSelect(string sql)
        {
            return ExecuteSelect(sql, null);
        }

        public SQLiteDataReader ExecuteSelect(string sql, ICollection<SQLiteParameter> parameters)
        {
            SQLiteCommand command = CreateCommand(sql, parameters);
            return command.ExecuteReader();
        }

        public int GetLastInsertRowId()
        {
            SQLiteDataReader reader = ExecuteSelect("SELECT last_insert_rowid()");
            reader.Read();
            return reader.GetInt32(0);
        }

        private SQLiteCommand CreateCommand(string sql, ICollection<SQLiteParameter> parameters)
        {
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            if (parameters != null)
            {
                foreach (SQLiteParameter param in parameters)
                {
                    command.Parameters.Add(param);
                }
            }
            return command;
        }

        public SQLiteTransaction BeginTransaction()
        {
            return connection.BeginTransaction();
        }

        public void Dispose()
        {
            if (connection != null)
            {
                try
                {
                    connection.Close();
                }
                catch (ObjectDisposedException ignored)
                {
                }
                connection = null;
            }
        }
    }
}
