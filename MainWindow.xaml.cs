using Meta;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace MetadataAnalyzer
{
    public partial class MainWindow : Window
    {
        private string selectedFilePath = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                selectedFilePath = dlg.FileName;
                txtLog.AppendText($"[+] Put file: {selectedFilePath}\n");
                txtLog.ScrollToEnd();
            }
        }

        private void BtnAnalyze_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
            {
                MessageBox.Show("First select the file!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Загружаем паттерны поиска
                FlagScanner.LoadPatterns(txtPatterns.Text);

                // Запуск анализа
                txtLog.AppendText($"[*] Analysis : {selectedFilePath}\n");
                string result = StegoAnalyzer.CheckStego(selectedFilePath);
                txtLog.AppendText(result);
                txtLog.ScrollToEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Analysis: {ex.Message}");
            }
        }

        private void BtnSaveLog_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Text files (*.txt)|*.txt";

            if (dlg.ShowDialog() == true)
            {
                File.WriteAllText(dlg.FileName, txtLog.Text);
                MessageBox.Show("Log saved.", "good", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    selectedFilePath = files[0];
                    txtLog.AppendText($"[+] Droped file's: {selectedFilePath}\n");
                    txtLog.ScrollToEnd();
                }
            }
        }
    }
}
