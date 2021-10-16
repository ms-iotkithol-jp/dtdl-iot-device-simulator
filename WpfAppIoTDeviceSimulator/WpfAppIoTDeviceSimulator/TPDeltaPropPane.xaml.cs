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
    /// DeltaPropPan.xaml の相互作用ロジック
    /// </summary>
    public partial class DeltaPropPan : Window
    {
        public DeltaPropPan(DeltaPropTelemetryParameter tp)
        {
            InitializeComponent();
            target = tp;
            this.Loaded += DeltaPropPan_Loaded;
        }

        private void DeltaPropPan_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = target;
        }

        DeltaPropTelemetryParameter target;

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
