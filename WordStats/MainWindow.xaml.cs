using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WordStats.Algorithm;
using WordStats.Db;
using WordStats.Entity;

namespace WordStats
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SQLite database;
        private ExamRepository examRepository;
        private WordRepository wordRepository;
        private ExamWordCountRepository examWordCountRepository;

        private ObservableCollection<Exam> exams = new ObservableCollection<Exam>();

        public MainWindow()
        {
            InitializeComponent();

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = System.IO.Path.Combine(appData, "WordStats");
            System.IO.Directory.CreateDirectory(appFolder);
            database = new SQLite(System.IO.Path.Combine(appFolder, "database.db"));
            database.OpenConnection();

            examRepository = new ExamRepository(database);
            wordRepository = new WordRepository(database);
            examWordCountRepository = new ExamWordCountRepository(database);

            InitRecordsAndGlobalStatsAsync();
        }

        private void InitRecordsAndGlobalStatsAsync()
        {
            Task.Factory.StartNew(() =>
            {
                foreach (Exam exam in examRepository.GetAll())
                {
                    exams.Add(exam);
                }

                Dispatcher.Invoke((Action) (() => {
                    recordsList.ItemsSource = exams;
                    OnNoRecordSelected();
                }));

                SetStatsGridData(examWordCountRepository.GetGlobalStats());
            });
        } 

        ~MainWindow()
        {
            if (database != null)
            {
                database.Dispose();
            }
        }

        private void OnAnalyze(object sender, RoutedEventArgs e)
        {
            string inputText = inputTextTextBox.Text.ToLower();
            string filteredInputText = Regex.Replace(inputText, "\\W", " ");
            string[] words = Regex.Split(filteredInputText, "\\s+");
            List<string> filteredWords = words.Where(word => !Regex.IsMatch(word, "[0-9]") && word.Length > 0)
                .ToList();
            SortedDictionary<string, int> wordsWithCount = new SortedDictionary<string, int>();
            foreach(string word in filteredWords) {
                if (!wordsWithCount.ContainsKey(word))
                {
                    wordsWithCount[word] = 0;
                }
                wordsWithCount[word]++;
            }
            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, int> kvp in wordsWithCount)
            {
                stringBuilder.AppendFormat("{0} -> {1}\n", kvp.Key, kvp.Value);
            }
            MessageBox.Show(stringBuilder.ToString());
        }

        private void OnAddRecordClick(object sender, RoutedEventArgs e)
        {
            AddRecordDialog addRecordWindow = new AddRecordDialog(examRepository);
            addRecordWindow.ShowDialog();
            Exam exam = addRecordWindow.Exam;
            if (exam != null)
            {
                exams.Add(exam);
                recordsList.SelectedIndex = exams.Count - 1;
            }
        }

        private void OnDeleteExam(object sender, RoutedEventArgs e)
        {
            Exam exam = exams[recordsList.SelectedIndex];
            MessageBoxResult result = MessageBox.Show("Наистина ли да бъде изтрит записът '" + exam.Name + "'?", "Потвърждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                DeleteExam(exam);
                exams.RemoveAt(recordsList.SelectedIndex);
            }
        }

        private void DeleteExam(Exam exam)
        {
            using (SQLiteTransaction tx = database.BeginTransaction())
            {
                examWordCountRepository.DeleteForExamId(exam.Id);
                examRepository.DeleteExam(exam.Id);
                tx.Commit();
            }
        }

        private void RenameRecord(object sender, RoutedEventArgs e)
        {
            RenameRecordDialog dialog = new RenameRecordDialog(examRepository, exams[recordsList.SelectedIndex]);
            dialog.ShowDialog();
            if (dialog.IsNameChanged)
            {
                recordsList.Items.Refresh();
            }
        }

        private void OnSelectedRecordChanged(object sender, SelectionChangedEventArgs e)
        {
            Exam selectedRecord = recordsList.SelectedIndex != -1 ? exams[recordsList.SelectedIndex] : null;
            if (selectedRecord == null)
            {
                OnNoRecordSelected();
            }
            else
            {
                SelectRecord(selectedRecord);
            }
        }

        private void SelectRecord(Exam selectedRecord)
        {
            selectedRecordLabel.Visibility = Visibility.Visible;
            inputTextTextBox.Visibility = Visibility.Visible;
            statisticsGrid.Visibility = Visibility.Collapsed;
            buttonsPanel.Visibility = Visibility.Visible;
            globalStatsButton.Visibility = Visibility.Visible;

            selectedRecordLabel.Content = selectedRecord.Name;

            SelectRecordAsync(selectedRecord);
        }

        private void SelectRecordAsync(Exam selectedRecord)
        {
            Task.Factory.StartNew(() =>
            {
                using (SQLiteTransaction tx = database.BeginTransaction())
                {
                    string content = examRepository.GetContent(selectedRecord.Id);
                    Dictionary<Word, int> statistics = examWordCountRepository.GetWordStatsByExam(selectedRecord.Id);
                    tx.Commit();

                    Dispatcher.Invoke((Action) (() =>
                    {
                        selectedRecord.Content = content;
                        inputTextTextBox.Text = content;
                        SetStatsGridData(statistics);

                        cancelButton.Visibility = Visibility.Collapsed;

                        if (content.Length > 0)
                        {
                            inputTextTextBox.IsReadOnly = true;
                            saveButton.Visibility = Visibility.Collapsed;
                            editButton.Visibility = Visibility.Visible;
                            showTextButton.Visibility = Visibility.Collapsed;
                            showStatsButton.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            inputTextTextBox.IsReadOnly = false;
                            saveButton.Visibility = Visibility.Visible;
                            editButton.Visibility = Visibility.Collapsed;
                            showTextButton.Visibility = Visibility.Collapsed;
                            showStatsButton.Visibility = Visibility.Collapsed;
                        }
                    }));
                }
            });
        }

        private void OnNoRecordSelected()
        {
            selectedRecordLabel.Content = "ГЛОБАЛНА СТАТИСТИКА";
            inputTextTextBox.Visibility = Visibility.Collapsed;
            statisticsGrid.Visibility = Visibility.Visible;
            buttonsPanel.Visibility = Visibility.Collapsed;
            globalStatsButton.Visibility = Visibility.Collapsed;

            Task.Factory.StartNew(() =>
            {
                Dictionary<Word, int> globalStats = examWordCountRepository.GetGlobalStats();
                SetStatsGridData(globalStats);
            });
        }

        private void SaveContent(object sender, RoutedEventArgs e)
        {
            string content = inputTextTextBox.Text.Trim();
            int selectedRecordIndex = recordsList.SelectedIndex;
            Task.Factory.StartNew(() =>
            {
                WordCounter counter = new WordCounter();
                Dictionary<string, int> stats = counter.GetWordStatistics(content);
                Exam record = exams[selectedRecordIndex];

                using (SQLiteTransaction tx = database.BeginTransaction())
                {
                    examWordCountRepository.DeleteForExamId(record.Id);
                    examRepository.UpdateContent(record.Id, content);
                    foreach (string wordValue in stats.Keys)
                    {
                        Word word = wordRepository.FindByValue(wordValue);
                        if (word == null)
                        {
                            word = wordRepository.Save(wordValue);
                        }
                        examWordCountRepository.SaveWordCount(record.Id, word.Id, stats[wordValue]);
                    }
                    tx.Commit();
                }

                Dispatcher.Invoke((Action) (() =>
                {
                    saveButton.Visibility = Visibility.Collapsed;
                    cancelButton.Visibility = Visibility.Collapsed;
                    editButton.Visibility = Visibility.Visible;
                    showTextButton.Visibility = Visibility.Collapsed;
                    showStatsButton.Visibility = Visibility.Visible;

                    statisticsGrid.ItemsSource = CreateDataGridDataSource(stats);

                    SelectRecordAsync(record);
                }));
            });
        }

        private void EditContent(object sender, RoutedEventArgs e)
        {
            inputTextTextBox.IsReadOnly = false;
            saveButton.Visibility = Visibility.Visible;
            editButton.Visibility = Visibility.Collapsed;
            showTextButton.Visibility = Visibility.Collapsed;
            showStatsButton.Visibility = Visibility.Collapsed;
            cancelButton.Visibility = Visibility.Visible;
        }

        private void ShowStats(object sender, RoutedEventArgs e)
        {
            statisticsGrid.Visibility = Visibility.Visible;
            inputTextTextBox.Visibility = Visibility.Collapsed;
            saveButton.Visibility = Visibility.Collapsed;
            editButton.Visibility = Visibility.Collapsed;
            showTextButton.Visibility = Visibility.Visible;
            showStatsButton.Visibility = Visibility.Collapsed;
        }

        private void ShowText(object sender, RoutedEventArgs e)
        {
            statisticsGrid.Visibility = Visibility.Collapsed;
            inputTextTextBox.Visibility = Visibility.Visible;
            saveButton.Visibility = Visibility.Collapsed;
            editButton.Visibility = Visibility.Visible;
            showTextButton.Visibility = Visibility.Collapsed;
            showStatsButton.Visibility = Visibility.Visible;
        }

        private void SetStatsGridData(Dictionary<Word, int> stats)
        {
            Dictionary<string, int> transformedStats = new Dictionary<string, int>();
            foreach(Word word in stats.Keys)
            {
                transformedStats[word.Value] = stats[word];
            }
            Dispatcher.Invoke((Action) (() =>
            {
                statisticsGrid.ItemsSource = CreateDataGridDataSource(transformedStats);
            }));
        }

        private List<StatsEntry> CreateDataGridDataSource(Dictionary<string, int> stats)
        {
            int totalCount = stats.Sum(stat => stat.Value);
            List<StatsEntry> entries = new List<StatsEntry>();
            foreach(string word in stats.Keys)
            {
                entries.Add(new StatsEntry {
                    Word = word,
                    Count = stats[word],
                    Percentage = 100 * (decimal) stats[word] / totalCount
                });
            }
            return entries;
        }

        private void CancelEditing(object sender, RoutedEventArgs e)
        {
            cancelButton.Visibility = Visibility.Collapsed;

            int selectedRecordIndex = recordsList.SelectedIndex;
            Exam record = exams[selectedRecordIndex];
            SelectRecordAsync(record);
        }

        private void showGlobalStats(object sender, RoutedEventArgs e)
        {
            recordsList.SelectedIndex = -1;
        }
    }
}
