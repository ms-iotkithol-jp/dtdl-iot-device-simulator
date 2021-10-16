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
    /// DiscretePane.xaml の相互作用ロジック
    /// </summary>
    public partial class DiscretePane : Window
    {
        public DiscretePane(DiscreteTelemetryParameter tp)
        {
            InitializeComponent();
            target = tp;
            this.Loaded += DiscretePane_Loaded;
        }

        private void DiscretePane_Loaded(object sender, RoutedEventArgs e)
        {
            if (target.TPTypeName == "integer")
            {
                tbNoise.Visibility = Visibility.Hidden;
                tblNoise.Visibility = Visibility.Hidden;
            }
            this.DataContext = target;
        }

        DiscreteTelemetryParameter target;

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
