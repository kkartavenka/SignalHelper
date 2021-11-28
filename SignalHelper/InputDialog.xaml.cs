﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
        }

        public string SignalType { get; private set; } = "default";
    }
}