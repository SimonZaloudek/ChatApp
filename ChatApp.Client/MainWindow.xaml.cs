using System.Windows;
using ChatApp.Client.ViewModels;

namespace ChatApp.Client
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;

            _viewModel.Messages.CollectionChanged += (_, _) =>
            {
                if (MessageList.Items.Count > 0)
                    MessageList.ScrollIntoView(MessageList.Items[MessageList.Items.Count - 1]);
            };
        }

        protected override async void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            await _viewModel.DisposeAsync();
        }
    }
}
