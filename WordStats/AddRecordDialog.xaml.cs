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
    /// Interaction logic for AddRecordDialog.xaml
    /// </summary>
    public partial class AddRecordDialog : Window
    {
        private ExamRepository examRepository;

        public Exam Exam { get; private set; }

        public AddRecordDialog(ExamRepository examRepository)
        {
            InitializeComponent();

            recordName.Focus();

            this.examRepository = examRepository;
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            string name = recordName.Text.Trim();
            Exam = examRepository.Save(name);
            Close();
        }
    }
}
