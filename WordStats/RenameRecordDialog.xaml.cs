using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WordStats.Db;
using WordStats.Entity;

namespace WordStats
{
    /// <summary>
    /// Interaction logic for RenameRecordDialog.xaml
    /// </summary>
    public partial class RenameRecordDialog : Window
    {
        private ExamRepository examRepository;

        private Exam exam;

        public bool IsNameChanged { get; private set; }

        public RenameRecordDialog(ExamRepository examRepository, Exam exam)
        {
            InitializeComponent();

            recordName.Text = exam.Name;
            recordName.Focus();

            this.examRepository = examRepository;
            this.exam = exam;
            IsNameChanged = false;
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            string name = recordName.Text.Trim();
            examRepository.UpdateName(exam.Id, name);
            exam.Name = name;
            IsNameChanged = true;
            Close();
        }
    }
}
