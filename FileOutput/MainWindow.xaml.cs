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

        // 转存为Base64并保存到XML
        private void BtnSaveToXml_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFiles.Count == 0)
            {
                txtStatus.Text = "请先选择文件";
                return;
            }

            string saveFilePath = null;

            FolderSelectDialog folderSelectDialog = new FolderSelectDialog();
            folderSelectDialog.Title = "请选择要保存xml的路径";

            if (folderSelectDialog.ShowDialog())
            {
                saveFilePath = folderSelectDialog.FileName;
            }

            if (string.IsNullOrEmpty(saveFilePath))
            {
                txtStatus.Text = "请先选择保存路径";
                return;
            }

            foreach (var file in selectedFiles)
            {
                try
                {
                    var xmlData = new List<XElement>();

                    // 读取文件字节流并转换为Base64
                    var bytes = File.ReadAllBytes(file);
                    var base64String = Convert.ToBase64String(bytes);

                    //创建日期和修改日期
                    DateTime creationTime = File.GetCreationTime(file);
                    DateTime lastWriteTime = File.GetLastWriteTime(file);

                    // 将字符串转换为UTF8编码的字节数组
                    byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(System.IO.Path.GetFileName(file));
                    // 将字节数组转换为Base64字符串
                    string nameBase64String = Convert.ToBase64String(nameBytes);

                    // 保存到XML文件
                    XDocument doc = new XDocument(
                        new XDeclaration("1.0", "UTF-8", null),
                        // 创建XML节点
                        new XElement("File",
                            new XElement("CreationTime", creationTime),
                            new XElement("LastWriteTime", lastWriteTime),
                            new XElement("FileName", nameBase64String),
                            new XElement("Base64Data", base64String))
                    );
                    while (File.Exists(saveFilePath + "\\" + nummber.Text + ".xml"))
                    {
                        nummber.Text = (int.Parse(nummber.Text) + 1).ToString();
                    }
                    doc.Save(saveFilePath + "\\" + nummber.Text + ".xml");
                    nummber.Text = (int.Parse(nummber.Text) + 1).ToString();
                }
                catch (Exception ex)
                {
                    txtStatus.Text = $"处理文件{file}时出错：{ex.Message}";
                    return;
                }
            }

            txtStatus.Text = $"已成功将{selectedFiles.Count}个文件转存为Base64并保存到{saveFilePath}";
        }

        // 从XML还原文件
        private void BtnRestoreFromXml_Click(object sender, RoutedEventArgs e)
        {
            List<XElement> fileRecords = new List<XElement>();

            FolderSelectDialog folderSelect = new FolderSelectDialog();
            folderSelect.Title = "请选择要还原的目录";
            if (folderSelect.ShowDialog())
            {
                string[] files = Directory.GetFiles(folderSelect.FileName, "*.xml");
                foreach (string file in files)
                {
                    var doc = XDocument.Load(file);
                    fileRecords.Add(doc.Element("File"));
                }
            }

            if (fileRecords.Count == 0)
            {
                txtStatus.Text = "XML文件中没有可还原的记录";
                return;
            }

            FolderSelectDialog folderSelectDialog = new FolderSelectDialog();
            folderSelectDialog.Title = "请选择保存目录";
            if (folderSelectDialog.ShowDialog())
            {
                foreach (var record in fileRecords)
                {
                    DateTime creationTime = Convert.ToDateTime(record.Element("CreationTime")?.Value);
                    DateTime lastWriteTime = Convert.ToDateTime(record.Element("LastWriteTime")?.Value);

                    // 将Base64字符串转换为字节数组
                    byte[] nameBytes = Convert.FromBase64String(record.Element("FileName")?.Value);
                    // 将字节数组转换为UTF8编码的字符串
                    string fileName = System.Text.Encoding.UTF8.GetString(nameBytes);

                    var base64Data = record.Element("Base64Data")?.Value;

                    if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(base64Data))
                        continue;

                    // 解码Base64并保存文件
                    var bytes = Convert.FromBase64String(base64Data);
                    var savePath = System.IO.Path.Combine(folderSelectDialog.FileName, fileName);
                    if (File.Exists(savePath)
                        && MessageBox.Show($"文件【{fileName}】已存在，是否替换？", "警告", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    {
                        continue;
                    }

                    File.WriteAllBytes(savePath, bytes);
                    File.SetCreationTime(savePath, creationTime);
                    File.SetLastWriteTime(savePath, lastWriteTime);
                }
                txtStatus.Text = $"已成功还原{fileRecords.Count}个文件到{folderSelectDialog.FileName}";
            }
        }
    }
}
