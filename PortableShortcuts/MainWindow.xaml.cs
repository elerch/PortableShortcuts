using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PortableShortcuts
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Checked(object sender, RoutedEventArgs e)
        {
            ChangeCheckedState(true, AllAppsTreeView.Items);
        }

        private void Unchecked(object sender, RoutedEventArgs e)
        {
            ChangeCheckedState(false, AllAppsTreeView.Items);
        }

        private static void ChangeCheckedState(bool isChecked, ItemCollection ic) {
            foreach (var obj in ic) {
                var dir = obj as DirectoryEntry;
                if (dir != null) 
                    ChangeState(isChecked, dir);
            }
        }

        private static void ChangeState(bool isChecked, DirectoryEntry dir)
        {
            dir.IsIncluded = !dir.IsDirectory && isChecked;
            foreach (var subDirectory in dir.Entries) {
                var subDir = subDirectory as DirectoryEntry;
                if(subDir != null)
                    ChangeState(isChecked, subDir);
            }
        }

    }
}
