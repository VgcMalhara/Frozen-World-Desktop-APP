using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace FrozenWorld.Views
{
    public enum ModernDialogType
    {
        Success,
        Error,
        Warning,
        Info,
        Confirm
    }

    public partial class ModernDialog : Window
    {
        public bool IsConfirmed { get; private set; }

        public ModernDialog(string title, string message, ModernDialogType type, string confirmText = "OK", string? cancelText = null, bool isDestructive = false)
        {
            InitializeComponent();

            txtTitle.Text = title;
            txtMessage.Text = message;
            btnConfirm.Content = confirmText;

            // Configure Icon & Palette based on dialog type
            ConfigureType(type, isDestructive);

            // If no cancelText is provided, hide cancel button (alert mode)
            if (string.IsNullOrEmpty(cancelText))
            {
                colCancel.Width = new GridLength(0);
                colSpacing.Width = new GridLength(0);
                btnCancel.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnCancel.Content = cancelText;
            }
        }

        private void ConfigureType(ModernDialogType type, bool isDestructive)
        {
            switch (type)
            {
                case ModernDialogType.Success:
                    txtIcon.Text = "✓";
                    txtIcon.Foreground = new SolidColorBrush(Color.FromRgb(16, 185, 129)); // #10B981
                    borderIcon.Background = new SolidColorBrush(Color.FromRgb(236, 253, 245)); // #ECFDF5
                    btnConfirm.Background = new SolidColorBrush(Color.FromRgb(16, 185, 129));
                    break;

                case ModernDialogType.Error:
                    txtIcon.Text = "✕";
                    txtIcon.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // #EF4444
                    borderIcon.Background = new SolidColorBrush(Color.FromRgb(254, 242, 242)); // #FEF2F2
                    btnConfirm.Background = new SolidColorBrush(Color.FromRgb(239, 68, 68));
                    break;

                case ModernDialogType.Warning:
                case ModernDialogType.Confirm:
                    if (isDestructive)
                    {
                        txtIcon.Text = "🗑";
                        txtIcon.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68));
                        borderIcon.Background = new SolidColorBrush(Color.FromRgb(254, 242, 242));
                        btnConfirm.Background = new SolidColorBrush(Color.FromRgb(239, 68, 68));
                    }
                    else
                    {
                        txtIcon.Text = "?";
                        txtIcon.Foreground = new SolidColorBrush(Color.FromRgb(245, 158, 11)); // #F59E0B
                        borderIcon.Background = new SolidColorBrush(Color.FromRgb(255, 251, 235)); // #FFFBEB
                        btnConfirm.Background = new SolidColorBrush(Color.FromRgb(37, 99, 235)); // #2563EB
                    }
                    break;

                case ModernDialogType.Info:
                default:
                    txtIcon.Text = "ℹ";
                    txtIcon.Foreground = new SolidColorBrush(Color.FromRgb(37, 99, 235));
                    borderIcon.Background = new SolidColorBrush(Color.FromRgb(239, 246, 255));
                    btnConfirm.Background = new SolidColorBrush(Color.FromRgb(37, 99, 235));
                    break;
            }
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            DialogResult = false;
            Close();
        }

        // --- Static Helper Methods ---

        private static Window? GetActiveOwner(Window? preferredOwner)
        {
            if (preferredOwner != null) return preferredOwner;
            return Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) ?? Application.Current.MainWindow;
        }

        public static void ShowSuccess(string message, string title = "Success", Window? owner = null)
        {
            var dialog = new ModernDialog(title, message, ModernDialogType.Success, confirmText: "Got it");
            var winOwner = GetActiveOwner(owner);
            if (winOwner != null) dialog.Owner = winOwner;
            dialog.ShowDialog();
        }

        public static void ShowError(string message, string title = "Error", Window? owner = null)
        {
            var dialog = new ModernDialog(title, message, ModernDialogType.Error, confirmText: "Close");
            var winOwner = GetActiveOwner(owner);
            if (winOwner != null) dialog.Owner = winOwner;
            dialog.ShowDialog();
        }

        public static void ShowInfo(string message, string title = "Information", Window? owner = null)
        {
            var dialog = new ModernDialog(title, message, ModernDialogType.Info, confirmText: "OK");
            var winOwner = GetActiveOwner(owner);
            if (winOwner != null) dialog.Owner = winOwner;
            dialog.ShowDialog();
        }

        public static bool ShowConfirm(string message, string title = "Confirm Action", string confirmText = "Confirm", string cancelText = "Cancel", bool isDestructive = false, Window? owner = null)
        {
            var dialog = new ModernDialog(title, message, ModernDialogType.Confirm, confirmText, cancelText, isDestructive);
            var winOwner = GetActiveOwner(owner);
            if (winOwner != null) dialog.Owner = winOwner;
            dialog.ShowDialog();
            return dialog.IsConfirmed;
        }
    }
}

