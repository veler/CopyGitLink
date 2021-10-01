using System.Diagnostics;
using System.Windows.Controls;

namespace CopyGitLink.Controls
{
    /// <summary>
    /// Interaction logic for CreateGitRepositoryUserControl.xaml
    /// </summary>
    public partial class CreateGitRepositoryUserControl : UserControl
    {
        public CreateGitRepositoryUserControl()
        {
            InitializeComponent();
        }

        private void LearnMoreHyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
