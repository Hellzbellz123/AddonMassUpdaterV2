using System.Windows;

namespace AddonUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private new readonly Downloader MakeaLink = new Downloader();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MakeaLink.FileCheck();
            Downloader.done = false;
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