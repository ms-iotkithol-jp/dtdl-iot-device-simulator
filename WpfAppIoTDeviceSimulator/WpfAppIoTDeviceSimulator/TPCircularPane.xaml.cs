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

namespace WpfAppIoTDeviceSimulator
{
    /// <summary>
    /// CircularPane.xaml の相互作用ロジック
    /// </summary>
    public partial class CircularPane : Window
    {
        public CircularPane(CircularTelemetryParameter tp)
        {
            InitializeComponent();
            target = tp;
            this.Loaded += CircularPane_Loaded;
        }

        private void CircularPane_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = target;
        }

        CircularTelemetryParameter target;

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
