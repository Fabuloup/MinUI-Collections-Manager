using System.Windows;

namespace MinuiCollectionsManager;

public partial class InputDialog : Window
{
    public InputDialog(string question)
    {
        InitializeComponent();
        lblQuestion.Content = question;
    }

    public string ResponseText
    {
        get { return txtResponse.Text; }
        set { txtResponse.Text = value; }
    }

    private void btnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}