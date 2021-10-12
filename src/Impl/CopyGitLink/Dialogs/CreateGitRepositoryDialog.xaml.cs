#nullable enable

using Microsoft.VisualStudio.PlatformUI;
using System.Windows;
using System.Windows.Input;

namespace CopyGitLink.Dialogs
{
    /// <summary>
    /// Interaction logic for CreateGitRepositoryDialog.xaml
    /// </summary>
    public partial class CreateGitRepositoryDialog : DialogWindow
    {
        public CreateGitRepositoryDialog()
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
        }

        /// <summary>
        /// Ensure that our custom caption area can handle dialog move
        /// </summary
        private void CaptionArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
