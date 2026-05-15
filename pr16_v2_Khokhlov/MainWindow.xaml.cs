using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace pr16_v2_Khokhlov
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            lst_box.ItemsSource = Student.DisplayedStudents;
        }
        private Student GetStudentFromInput()
        {
            string lastName = txtInput.Text;
            string group = txtGroup.Text;
            string specialty = txtSpec.Text;
            string discipline = txtDisc.Text;
            int grade = 0;
            if (!string.IsNullOrEmpty(txtMark.Text))
            {
                int.TryParse(txtMark.Text, out grade);
            }

            return new Student
            {
                LastName = lastName,
                Group = group,
                Specialty = specialty,
                Discipline = discipline,
                Grade = grade
            };
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Student newStudent = GetStudentFromInput();

            if (Student.IsValid(newStudent, out string errorMessage))
            {
                Student.AddStudent(newStudent);
                ClearInputs();
                RefreshAll();
                MessageBox.Show("Студент успешно добавлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(errorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void RefreshAll()
        {
            string filterText = Filt_txt.Text.ToLower();
            string sortBy = "";
            if (Sort_cmb.SelectedItem is ComboBoxItem selected)
                sortBy = selected.Content.ToString();
            Student.RefreshDisplay(filterText, sortBy);
        }

        private void Delete_lst(object sender, RoutedEventArgs e)
        {
            if (lst_box.SelectedItem is Student selected)
            {
                var result = MessageBox.Show($"Удалить студента {selected.LastName}?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Student.DeleteStudent(selected);
                    RefreshAll();
                    MessageBox.Show("Студент удален!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите студента для удаления!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Save_List(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*",
                    DefaultExt = ".csv",
                    FileName = $"students_{DateTime.Now:yyyyMMdd_HHmmss}"
                };
                if (saveDialog.ShowDialog() == true)
                {
                    Student.SaveToFile(saveDialog.FileName);
                    MessageBox.Show($"Сохранено {Student.TotalCount} студентов в файл:\n{saveDialog.FileName}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Clear_List(object sender, RoutedEventArgs e)
        {
            if (Student.TotalCount == 0)
            {
                MessageBox.Show("Список уже пуст!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var result = MessageBox.Show("Очистить весь список студентов?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Student.ClearAllStudents();
                RefreshAll();
                MessageBox.Show("Список очищен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Load_List(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*",
                    DefaultExt = ".csv"
                };
                if (openDialog.ShowDialog() == true)
                {
                    var result = MessageBox.Show("Заменить текущий список студентов?",
                        "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        Student.LoadFromFile(openDialog.FileName);
                        RefreshAll();
                        MessageBox.Show($"Данные загружены из файла!",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ClearInputs()
        {
            txtInput.Clear();
            txtGroup.Clear();
            txtSpec.Clear();
            txtDisc.Clear();
            txtMark.Clear();
        }
        private void Filt_txt_TextChanged(object sender, EventArgs e)
        {
            RefreshAll();
        }
        private void Sort_cmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshAll();
        }
    }
}
