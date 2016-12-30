using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordStats.Entity;

namespace WordStats.Db
{
    public class ExamWordCountRepository
    {
        private SQLite sqlite;

        public ExamWordCountRepository(SQLite sqlite)
        {
            this.sqlite = sqlite;
        }

        public void DeleteWordCount(int examId, int wordId)
        {
            string sql = "DELETE FROM exam_words_count WHERE exam_id = @examId AND word_id = @wordId;";
            sqlite.ExecuteUpdate(sql, new List<SQLiteParameter> {
                new SQLiteParameter("@examId", examId),
                new SQLiteParameter("@wordId", wordId)
            });
        }

        public void SaveWordCount(int examId, int wordId, int count)
        {
            string sql = "INSERT INTO exam_words_count (exam_id, word_id, word_count) VALUES (@examId, @wordId, @count);";
            sqlite.ExecuteUpdate(sql, new List<SQLiteParameter> {
                new SQLiteParameter("@examId", examId),
                new SQLiteParameter("@wordId", wordId),
                new SQLiteParameter("@count", count)
            });
        }

        public Dictionary<Word, int> GetWordStatsByExam(int examId)
        {
            string sql = "SELECT stats.word_id, word.value, stats.word_count FROM exam_words_count AS stats JOIN words AS word ON word.id = stats.word_id WHERE stats.exam_id = @examId;";
            SQLiteDataReader reader = sqlite.ExecuteSelect(sql, new List<SQLiteParameter>
            {
                new SQLiteParameter("@examId", examId)
            });
            Dictionary<Word, int> stats = new Dictionary<Word, int>();
            while (reader.Read())
            {
                Word word = new Word();
                word.Id = reader.GetInt32(0);
                word.Value = reader.GetString(1);
                int count = reader.GetInt32(2);
                stats[word] = count;
            }
            return stats;
        }

        public void DeleteForExamId(int examId)
        {
            string sql = "DELETE FROM exam_words_count WHERE exam_id = @id;";
            sqlite.ExecuteUpdate(sql, new List<SQLiteParameter> {
                new SQLiteParameter("@id", examId)
            });
        }

        public Dictionary<Word, int> GetGlobalStats()
        {
            string sql = "SELECT stats.word_id, word.value, stats.word_count FROM exam_words_count AS stats JOIN words AS word ON word.id = stats.word_id";
            SQLiteDataReader reader = sqlite.ExecuteSelect(sql);
            Dictionary<Word, int> stats = new Dictionary<Word, int>();
            while (reader.Read())
            {
                Word word = new Word();
                word.Id = reader.GetInt32(0);
                word.Value = reader.GetString(1);
                int count = reader.GetInt32(2);
                stats[word] = count;
            }
            return stats;
        }
    }
}
