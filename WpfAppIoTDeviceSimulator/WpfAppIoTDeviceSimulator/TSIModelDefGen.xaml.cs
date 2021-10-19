using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// TSIModelDef.xaml の相互作用ロジック
    /// </summary>
    public partial class TSIModelDefGen : Window
    {
        public TSIModelDefGen(string timeSeriesId, string typeId = null, string typeName = null)
        {
            InitializeComponent();
            TimeSeriesId = timeSeriesId;
            TypeId = typeId;
            TypeName = typeName;
            this.Loaded += TSIModelDefGen_Loaded;
        }

        private void TSIModelDefGen_Loaded(object sender, RoutedEventArgs e)
        {
            lbHierachies.ItemsSource = hierarchies;
            tbTimeSeriesId.Text = TimeSeriesId;
            tbTypeId.Text = TypeId;
            tbTimeSeriesName.Text = TimeSeriesId;
            CheckInstanceGenStatus();
        }

        void CheckInstanceGenStatus()
        {
            if ((!(string.IsNullOrEmpty(tbTimeSeriesId.Text))) && (!(string.IsNullOrEmpty(tbTypeId.Text))))
            {
                buttonInstanceGen.IsEnabled = true;
            }
        }

        public string TimeSeriesId { get; set; }
        public string TypeId { get; set; }
        public string TypeName { get; set; }

        ObservableCollection<string> hierarchies = new ObservableCollection<string>();

        private void buttonLoadHierachiesFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter= "TSI Hierachies(*.json)|*.json";
            if (dialog.ShowDialog() == true)
            {
                tbLoadHierachiesModelFileName.Text = dialog.FileName;
                using (var fs = File.OpenRead(dialog.FileName))
                {
                    using (var reader = new StreamReader(fs))
                    {
                        var content = reader.ReadToEnd();
                        try
                        {
                            dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                            foreach (dynamic elem in json.put)
                            {
                                tbHierachiesId.Text = elem.id;
                                tbHierachiesName.Text = elem.name;
                                dynamic instanceFieldNames = elem.source.instanceFieldNames;
                                hierarchies.Clear();
                                foreach (var ifn in instanceFieldNames)
                                {
                                    hierarchies.Add((string)ifn);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new ArgumentOutOfRangeException("heiarchies.json", ex.Message);
                        }
                        tbGenContent.Text = content;
                        tbSavedFileName.Text = dialog.FileName;
                    }
                }
            }
        }

        private void buttonLoadTypeFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "TSI Type(*.json)|*.json";
            if (dialog.ShowDialog() == true)
            {
                tbLoadTypeModelFileName.Text = dialog.FileName;
                using (var fs = File.OpenRead(dialog.FileName))
                {
                    using (var reader = new StreamReader(fs))
                    {
                        var content = reader.ReadToEnd();
                        try
                        {
                            dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                            foreach (dynamic elem in json.put)
                            {
                                tbTypeId.Text = elem.id;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new ArgumentOutOfRangeException("type.json", ex.Message);
                        }
                        tbGenContent.Text = content;
                        tbSavedFileName.Text = dialog.FileName;
                        CheckInstanceGenStatus();
                    }
                }
            }

        }

        

        private void buttonHierachiesGen_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbHierachiesName.Text))
            {
                MessageBox.Show("Please input 'Hierachies Name'");
                return;
            }
            if (string.IsNullOrEmpty(tbHierachiesId.Text))
            {
                tbHierachiesId.Text = Guid.NewGuid().ToString();
            }
            var generator = new TSIHierachiesGenerator(tbHierachiesId.Text, tbHierachiesName.Text, hierarchies.ToArray());
            tbGenContent.Text = generator.Generate();
            tbSavedFileName.Text = "";
        }

        private void buttonHierachyAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbInstanceName.Text))
            {
                MessageBox.Show("Please input instance name");
                return;
            }
            var c = hierarchies.Where(e => e == tbInstanceName.Text);
            if (c.Count() > 0)
            {
                MessageBox.Show("You can't add existed name");
                return;
            }
            hierarchies.Add(tbInstanceName.Text);
            UpdateHiearchiesDef();
        }

        private void UpdateHiearchiesDef()
        {
            tbInstanceHierachies.Text = "";
            for(int i = 0; i < hierarchies.Count - 1; i++)
            {
                if (!string.IsNullOrEmpty(tbInstanceHierachies.Text))
                {
                    tbInstanceHierachies.Text += ":";
                }
                tbInstanceHierachies.Text += $"{hierarchies[i]}0";
            }
            if (!string.IsNullOrEmpty(tbInstanceHierachies.Text))
            {
                tbInstanceHierachies.Text += ":";
            }
            tbInstanceHierachies.Text += tbTimeSeriesId.Text;
        }

        private void buttonHierachyMgmt_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbInstanceName.Text))
            {
                MessageBox.Show("Please select instance at the list box");
                return;
            }
            if (sender.ToString().EndsWith("Up"))
            {
                var current = hierarchies.IndexOf(tbInstanceName.Text);
                if (current > 0)
                {
                    var item = tbInstanceName.Text;
                    hierarchies.Remove(item);
                    hierarchies.Insert(current - 1, item);
                }
            }
            else if (sender.ToString().EndsWith("Down"))
            {
                var current = hierarchies.IndexOf(tbInstanceName.Text);
                if (current < hierarchies.Count - 1)
                {
                    var item = tbInstanceName.Text;
                    hierarchies.Remove(item);
                    hierarchies.Insert(current+1, item);
                }
            }
            else if (sender.ToString().EndsWith("Delete"))
            {
                hierarchies.Remove(tbInstanceName.Text);
            }
            UpdateHiearchiesDef();
        }

        private void lbHierachies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tbInstanceName.Text = lbHierachies.SelectedItem as string;
        }

        private void buttonInstanceGen_Click(object sender, RoutedEventArgs e)
        {
            var instances = tbInstanceHierachies.Text.Split(new char[] { ':' });
            if (instances.Length != hierarchies.Count-1)
            {
                MessageBox.Show("Please input instance hierachies. Need the same number of items separated by a ':'");
                return;
            }
            var model = new Dictionary<string, string>();
            int i = 0;
            for (; i < instances.Length; i++)
            {
                model.Add(hierarchies[i], instances[i]);
            }
            model.Add(hierarchies[i], tbTimeSeriesId.Text);
            var generator = new TSIInstanceGenerator(tbTimeSeriesId.Text, tbTimeSeriesName.Text, tbTypeId.Text, tbHierachiesId.Text, model);
            tbGenContent.Text = generator.Generate();
            tbSavedFileName.Text = "";
        }

        private void buttonSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter= "TSI hiearchies instance(*.json)|*.json";
            if (dialog.ShowDialog() == true)
            {
                using (var fs = File.Create(dialog.FileName))
                {
                    using (var writer = new StreamWriter(fs))
                    {
                        writer.Write( tbGenContent.Text);
                        writer.Flush();
                    }
                }
            }
        }
    }
}
