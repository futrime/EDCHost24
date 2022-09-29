using System;
using System.Threading;
using System.Windows.Forms;

namespace EdcHost;

/// <summary>
/// The program
/// </summary>
static class Program
{
    [STAThread]
    static void Main()
    {
        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(HandleUnhandledException);

        ApplicationConfiguration.Initialize();
        Application.Run(new MainWindow());
    }

    /// <summary>
    /// Handle all unhandled exceptions.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            "An unhandled exception occurred!\nPlease take a screenshot of this message and report to EDC Commitee!\n\n" +
            e.ExceptionObject.ToString(),
            "Unexpected Exception",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
    }
}