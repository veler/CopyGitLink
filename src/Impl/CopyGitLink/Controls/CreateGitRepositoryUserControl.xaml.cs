#nullable enable

using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CopyGitLink.Controls
{
    /// <summary>
    /// Interaction logic for CreateGitRepositoryUserControl.xaml
    /// </summary>
    public partial class CreateGitRepositoryUserControl : UserControl
    {
        private const string FormatIndicator = "{0}";

        public CreateGitRepositoryUserControl()
        {
            InitializeComponent();

            int boldInjectionIndex = Strings.CreateGitRepositoryUserControlIndication.IndexOf(FormatIndicator);
            if (boldInjectionIndex == -1)
            {
                IndicationTextBlock.Inlines.Add(new Run(Strings.CreateGitRepositoryUserControlIndication));
            }
            else
            {
                IndicationTextBlock.Inlines.Add(new Run(Strings.CreateGitRepositoryUserControlIndication.Substring(0, boldInjectionIndex)));
                IndicationTextBlock.Inlines.Add(new Bold(new Run(Strings.CreateGitRepositoryUserControlIndicationCommand)));
                IndicationTextBlock.Inlines.Add(new Run(Strings.CreateGitRepositoryUserControlIndication.Substring(boldInjectionIndex + FormatIndicator.Length)));
            }
        }

        private void LearnMoreHyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
