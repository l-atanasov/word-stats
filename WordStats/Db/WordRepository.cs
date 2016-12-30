using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordStats.Entity;

namespace WordStats.Db
{
    public class WordRepository
    {
        private SQLite sqlite;

        public WordRepository(SQLite sqlite)
        {
            this.sqlite = sqlite;
        }

        public Word Save(string value)
        {
            string sql = "INSERT INTO words (value) VALUES (@value);";
            sqlite.ExecuteUpdate(sql, new List<SQLiteParameter> {
                new SQLiteParameter("@value", value)
            });
            Word word = new Word();
            word.Id = sqlite.GetLastInsertRowId();
            word.Value = value;
            return word;
        }

        public Word FindByValue(string value)
        {
            string sql = "SELECT id, value FROM words WHERE value = @value;";
            SQLiteDataReader reader = sqlite.ExecuteSelect(sql, new List<SQLiteParameter> {
                new SQLiteParameter("@value", value)
            });
            Word word = null;
            if (reader.Read())
            {
                word = new Word();
                word.Id = reader.GetInt32(0);
                word.Value = reader.GetString(1);
            }
            return word;
        }
    }
}
