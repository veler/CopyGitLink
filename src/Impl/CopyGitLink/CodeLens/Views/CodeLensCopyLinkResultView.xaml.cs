#nullable enable

using CopyGitLink.CodeLens.ViewModels;
using CopyGitLink.Def;
using CopyGitLink.Def.Models;
using Microsoft.VisualStudio.Text.Editor;
using System.Windows.Controls;

namespace CopyGitLink.CodeLens.Views
{
    /// <summary>
    /// Interaction logic for CodeLensCopyLinkResultView.xaml
    /// </summary>
    public partial class CodeLensCopyLinkResultView : UserControl
    {
        public CodeLensCopyLinkResultView(
            IRepositoryService repositoryService,
            ICopyLinkService copyLinkService,
            ITextView textView,
            CodeLensCopyLinkResult result)
        {
            InitializeComponent();

            DataContext = new CodeLensCopyLinkResultViewModel(repositoryService, copyLinkService, textView, result.ApplicableSpan);
        }

        private void UrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UrlTextBox.SelectAll();
        }

        private void UserControl_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var viewModel = DataContext as CodeLensCopyLinkResultViewModel;
            if (viewModel != null && viewModel.LinkGenerated)
            {
                UrlTextBox.SelectAll();
            }
        }
    }
}
