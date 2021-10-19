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
using System.Windows.Shapes;
using Microsoft.Win32;

namespace WpfAppIoTDeviceSimulator
{
    /// <summary>
    /// TSIModelFile.xaml の相互作用ロジック
    /// </summary>
    public partial class TSIModelFile : Window
    {
        public TSIModelFile(string text)
        {
            InitializeComponent();
            content = text;
            this.Loaded += TSIModelFile_Loaded;
        }

        private void TSIModelFile_Loaded(object sender, RoutedEventArgs e)
        {
            tbContent.Text = content;
        }

        string content;

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "TSI Type(*.json)|*.json";
            dialog.FileName = "type.def";
            if (dialog.ShowDialog() == true)
            {
                tbFileName.Text = dialog.FileName;
                using (var fs = File.Create(dialog.FileName))
                {
                    using (var writer = new StreamWriter(fs))
                    {
                        writer.Write(tbContent.Text);
                        writer.Flush();
                    }
                }
            }
        }
    }
}
