using System;
using System.Collections.Generic;
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
using System.Xml.Linq;

namespace FileOutput
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> selectedFiles = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        // 选择单个文件
        private void BtnSelectSingle_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == true)
            {
                selectedFiles.Clear();
                selectedFiles = null;
                selectedFiles = new List<string>();
                selectedFiles.Add(openFileDialog.FileName);
                lstFiles.ItemsSource = selectedFiles;
                txtStatus.Text = $"已选择1个文件：{openFileDialog.FileName}";
            }
        }

        // 选择多个文件
        private void BtnSelectMultiple_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                selectedFiles.Clear();
                selectedFiles = null;
                selectedFiles = new List<string>();
                selectedFiles.AddRange(openFileDialog.FileNames);
                lstFiles.ItemsSource = selectedFiles;
                txtStatus.Text = $"已选择{selectedFiles.Count}个文件";
            }
        }

        // 转存为Base64
        private void BtnSaveToXml_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFiles.Count == 0)
            {
                txtStatus.Text = "请先选择文件";
                return;
            }

            foreach (var file in selectedFiles)
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.Extension.Equals(@".a"))
                    {
                        continue;
                    }
                    // 读取文件字节流并转换为Base64
                    //var bytes = File.ReadAllBytes(file);
                    //var base64String = Convert.ToBase64String(bytes);

                    //创建日期和修改日期
                    //DateTime creationTime = File.GetCreationTime(file);
                    //DateTime lastWriteTime = File.GetLastWriteTime(file);

                    // 将字符串转换为UTF8编码的字节数组
                    byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(System.IO.Path.GetFileName(file));
                    // 将字节数组转换为Base64字符串
                    string nameBase64String = Convert.ToBase64String(nameBytes);

                    string saveFilePath = new FileInfo(file).DirectoryName + "\\" + nameBase64String + ".a";
                    File.Move(file, saveFilePath);
                }
                catch (Exception ex)
                {
                    txtStatus.Text = $"处理文件{file}时出错：{ex.Message}";
                    return;
                }
            }

            txtStatus.Text = $"已成功将文件转存为Base64";
        }

        // 还原文件
        private void BtnRestoreFromXml_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFiles.Count == 0)
            {
                txtStatus.Text = "请先选择文件";
                return;
            }

            foreach (var file in selectedFiles)
            {
                //DateTime creationTime = Convert.ToDateTime(record.Element("CreationTime")?.Value);
                //DateTime lastWriteTime = Convert.ToDateTime(record.Element("LastWriteTime")?.Value);
                FileInfo fileInfo = new FileInfo(file);
                // 将Base64字符串转换为字节数组
                if (!fileInfo.Extension.Equals(@".a"))
                {
                    continue;
                }
                byte[] nameBytes = Convert.FromBase64String(fileInfo.Name.Remove(fileInfo.Name.Length - 2, 2));
                // 将字节数组转换为UTF8编码的字符串
                string fileName = System.Text.Encoding.UTF8.GetString(nameBytes);

                string saveFilePath = fileInfo.DirectoryName + "\\" + fileName;
                File.Move(file, saveFilePath);
            }
            txtStatus.Text = $"已成功还原文件";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FolderSelectDialog folderSelectDialog = new FolderSelectDialog();
            if (folderSelectDialog.ShowDialog())
            {
                txtStatus.Text = "开始全文件夹转换";
                string[] folders = Directory.GetDirectories(folderSelectDialog.FileName, "*.*", SearchOption.AllDirectories);
                folders = folders.Concat(new string[1] { folderSelectDialog.FileName }).ToArray();
                foreach (string folder in folders)
                {
                    string[] files = Directory.GetFiles(folder);
                    foreach (string file in files)
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        if (fileInfo.Extension.Equals(@".a"))
                        {
                            continue;
                        }

                        // 将字符串转换为UTF8编码的字节数组
                        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(System.IO.Path.GetFileName(file));
                        // 将字节数组转换为Base64字符串
                        string nameBase64String = Convert.ToBase64String(nameBytes);

                        string saveFilePath = new FileInfo(file).DirectoryName + "\\" + nameBase64String + ".a";
                        File.Move(file, saveFilePath);
                    }
                }
                txtStatus.Text = "全文件夹转换完成";
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            FolderSelectDialog folderSelectDialog = new FolderSelectDialog();
            if (folderSelectDialog.ShowDialog())
            {
                txtStatus.Text = "开始全文件夹还原";
                string[] folders = Directory.GetDirectories(folderSelectDialog.FileName, "*.*", SearchOption.AllDirectories);
                folders = folders.Concat(new string[1] { folderSelectDialog.FileName }).ToArray();
                foreach (string folder in folders)
                {
                    string[] files = Directory.GetFiles(folder);
                    foreach (string file in files)
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        if (!fileInfo.Extension.Equals(@".a"))
                        {
                            continue;
                        }
                        byte[] nameBytes = Convert.FromBase64String(fileInfo.Name.Remove(fileInfo.Name.Length - 2, 2));
                        // 将字节数组转换为UTF8编码的字符串
                        string fileName = System.Text.Encoding.UTF8.GetString(nameBytes);

                        string saveFilePath = fileInfo.DirectoryName + "\\" + fileName;
                        File.Move(file, saveFilePath);
                    }
                }
                txtStatus.Text = "全文件夹还原完成";
            }
        }
    }
}
