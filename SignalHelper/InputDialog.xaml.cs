using System.Windows;

namespace SignalHelper {
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window {
        public InputDialog() {
            InitializeComponent();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e) {
            SignalType = SignalTypeInput.Text.ToLower();

            IsCancelled = false;
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;

        public string SignalType { get; private set; } = "default";
        public bool IsCancelled { get; private set; } = true;
    }
}
