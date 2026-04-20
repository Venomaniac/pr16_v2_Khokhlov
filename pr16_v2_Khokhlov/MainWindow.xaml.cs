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
    public class Student
    {
        public string LastName { get; set; }
        public string Group { get; set; }
    }
    public partial class MainWindow : Window
    {
        private ObservableCollection<Student> allStudents = new ObservableCollection<Student>();
        private ObservableCollection<Student> displayedStudents = new ObservableCollection<Student>();
        public MainWindow()
        {
            InitializeComponent();
            lst_box.ItemsSource = displayedStudents;
        }
        private void RefreshDisplay()
        {
            var filtered = allStudents.AsEnumerable();
            string filterText = Filt_txt.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(filterText))
            {
                filtered = filtered.Where(s =>
                    s.LastName.ToLower().Contains(filterText) ||
                    s.Group.ToLower().Contains(filterText));
            }
            string sortBy = "";
            if (Sort_cmb.SelectedItem is ComboBoxItem selected)
                sortBy = selected.Content.ToString();

            switch (sortBy)
            {
                case "По алфавиту (А-Я)":
                    filtered = filtered.OrderBy(s => s.LastName);
                    break;
                case "По алфавиту (Я-А)":
                    filtered = filtered.OrderByDescending(s => s.LastName);
                    break;
                case "По группе":
                    filtered = filtered.OrderBy(s => s.Group).ThenBy(s => s.LastName);
                    break;
                default:
                    filtered = filtered.OrderBy(s => s.LastName);
                    break;
            }
            displayedStudents.Clear();
            foreach (var student in filtered)
            {
                displayedStudents.Add(student);
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string lastName = txtInput.Text.Trim();
            string group = "";
            if (cmbGroup.SelectedItem is ComboBoxItem selectedItem)
                group = selectedItem.Content.ToString();
            else
                group = cmbGroup.Text;
            if (string.IsNullOrEmpty(lastName))
            {
                MessageBox.Show("Введите фамилию студента!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(group))
            {
                MessageBox.Show("Выберите или введите группу!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            allStudents.Add(new Student { LastName = lastName, Group = group });
            RefreshDisplay();
            txtInput.Clear();
            cmbGroup.SelectedIndex = -1;
        }

        private void Delete_lst(object sender, RoutedEventArgs e)
        {
            if (lst_box.SelectedItem is Student selected)
            {
                var result = MessageBox.Show($"Удалить студента {selected.LastName}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    allStudents.Remove(selected);
                    RefreshDisplay();
                    MessageBox.Show("Студент удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    using (StreamWriter writer = new StreamWriter(saveDialog.FileName, false, System.Text.Encoding.UTF8))
                    {
                        writer.WriteLine("Фамилия,Группа");
                        foreach (var student in allStudents)
                        {
                            writer.WriteLine($"{student.LastName},{student.Group}");
                        }
                    }
                    MessageBox.Show($"Сохранено {allStudents.Count} студентов в файл:\n{saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Clear_List(object sender, RoutedEventArgs e)
        {
            if (allStudents.Count == 0)
            {
                MessageBox.Show("Список уже пуст!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var result = MessageBox.Show("Очистить весь список студентов?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                allStudents.Clear();
                RefreshDisplay();
                MessageBox.Show("Список очищен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    var newStudents = new ObservableCollection<Student>();
                    using (StreamReader reader = new StreamReader(openDialog.FileName, System.Text.Encoding.UTF8))
                    {
                        string line;
                        bool isFirstLine = true;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (string.IsNullOrWhiteSpace(line))
                                continue;
                            if (isFirstLine && (line.Contains("Фамилия") || line.Contains("LastName")))
                            {
                                isFirstLine = false;
                                continue;
                            }
                            isFirstLine = false;
                            string[] parts = line.Split(',');
                            if (parts.Length >= 2)
                            {
                                string lastName = parts[0];
                                string group = parts[1];
                                if (!string.IsNullOrEmpty(lastName) && !string.IsNullOrEmpty(group))
                                {
                                    newStudents.Add(new Student { LastName = lastName, Group = group });
                                }
                            }
                        }
                    }

                    if (newStudents.Count > 0)
                    {
                        var result = MessageBox.Show($"Найдено {newStudents.Count} студентов.\nЗаменить текущий список?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            allStudents.Clear();
                            foreach (var student in newStudents)
                            {
                                allStudents.Add(student);
                            }
                            RefreshDisplay();
                            MessageBox.Show($"Загружено {newStudents.Count} студентов!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("В файле не найдено данных о студентах!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Filt_txt_TextChanged(object sender, EventArgs e)
        {
            RefreshDisplay();
        }
        private void Sort_cmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshDisplay();
        }
    }
}
