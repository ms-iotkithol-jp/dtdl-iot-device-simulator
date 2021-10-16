using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace WpfAppIoTDeviceSimulator
{
    /// <summary>
    /// EnumPane.xaml の相互作用ロジック
    /// </summary>
    public partial class EnumPane : Window
    {
        public EnumPane(EnumTelemetryParameter tp)
        {
            InitializeComponent();
            target = tp;
            this.Loaded += EnumPane_Loaded;
        }

        ObservableCollection<string> enumValues = new ObservableCollection<string>();

        private void EnumPane_Loaded(object sender, RoutedEventArgs e)
        {
            int index = 0;
            int selectedIndex = -1;
            foreach(var v in target.GetEnumValues())
            {
                enumValues.Add(v);
                if (v == target.Current)
                {
                    selectedIndex = index;
                }
                index++;
            }
            this.cbCurrent.ItemsSource = enumValues;
            this.cbCurrent.SelectedIndex = selectedIndex;

        }

        EnumTelemetryParameter target;

        private void cbCurrent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            target.Current = cbCurrent.SelectedItem as string;
        }

        private void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            target.Store();
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            target.Reset();
        }
    }
}
