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

namespace LIM.Windows
{
    /// <summary>
    /// Interaction logic for LimSettingsWindow.xaml
    /// </summary>
    public partial class LimSettingsWindow : Window
    {
        public LimSettingsWindow()
        {
            InitializeComponent();
            settingGrid.DataContext = GlobalState.UserSettings;
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            GlobalState.UserSettings.Save();

            Close();
        }
    }
}
