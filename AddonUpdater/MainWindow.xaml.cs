using System;
using System.Threading;
using System.Windows;
using HtmlAgilityPack;

namespace AddonUpdater
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string downloadlink = null;
            string WorkingLink = "https://wow.curseforge.com/projects/details"
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(WorkingLink);
            foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes("//p/a"))
            {
                var hrefValue = node.Attributes["href"]?.Value;
                downloadlink = "https://www.curseforge.com" + hrefValue;
                Console.WriteLine(downloadlink);
            }
            //MakeUseableLink.FileCheck();
            //MakeUseableLink.done = false;
        }

        public void MessageBoxNoLinks()
        {
            //MessageBox.Show("Please add links to the links.txt file.");
        }

        public void CurrentLink()
        {
            //MessageBox.Show("");
        }
    }
}