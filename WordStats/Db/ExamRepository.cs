using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordStats.Entity;

namespace WordStats.Db
{
    public class ExamRepository
    {
        private SQLite sqlite;

        public ExamRepository(SQLite sqlite)
        {
            this.sqlite = sqlite;
        }

        public Exam Save(string name)
        {
            string sql = "INSERT INTO exams (name, content) VALUES (@name, @content);";
            sqlite.ExecuteUpdate(sql, new List<SQLiteParameter> {
                new SQLiteParameter("@name", name),
                new SQLiteParameter("@content", "")
            });
            Exam exam = new Exam();
            exam.Id = sqlite.GetLastInsertRowId();
            exam.Name = name;
            exam.Content = "";
            return exam;
        }

        public void UpdateName(int id, string name)
        {
            string sql = "UPDATE exams SET name=@name WHERE id=@id;";
            sqlite.ExecuteUpdate(sql, new List<SQLiteParameter> {
                new SQLiteParameter("@name", name),
                new SQLiteParameter("@id", id)
            });
        }

        public void UpdateContent(int id, string content)
        {
            string sql = "UPDATE exams SET content=@content WHERE id=@id;";
            sqlite.ExecuteUpdate(sql, new List<SQLiteParameter> {
                new SQLiteParameter("@content", content),
                new SQLiteParameter("@id", id)
            });
        }

        public List<Exam> GetAll()
        {
            string sql = "SELECT id, name FROM exams ORDER BY name ASC, id ASC";
            SQLiteDataReader reader = sqlite.ExecuteSelect(sql);
            List<Exam> exams = new List<Exam>();
            while (reader.Read())
            {
                Exam exam = new Exam();
                exam.Id = reader.GetInt32(0);
                exam.Name = reader.GetString(1);
                exams.Add(exam);
            }
            return exams;
        }

        public string GetContent(int id)
        {
            string sql = "SELECT content FROM exams WHERE id = @id;";
            SQLiteDataReader reader = sqlite.ExecuteSelect(sql, new List<SQLiteParameter> {
                new SQLiteParameter("@id", id)
            });
            reader.Read();
            return reader.GetString(0);
        }

        public void DeleteExam(int id)
        {
            string sql = "DELETE FROM exams WHERE id = @id;";
            sqlite.ExecuteUpdate(sql, new List<SQLiteParameter>
            {
                new SQLiteParameter("@id", id)
            });
        }

    }
}
