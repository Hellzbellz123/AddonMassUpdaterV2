using System.Windows;

namespace AddonUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MakeUseableLink MLU = new MakeUseableLink();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MLU.FileCheck();
            MakeUseableLink.done = false;
        }

        public void MessageBoxNoLinks()
        {
            MessageBox.Show("Please add links to the links.txt file.");
        }

        public static void IsDone()
        {
            MessageBox.Show("Downloads are finished");
        }
    }
}