using System;
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
        try
        {
            Application.Run(new MainWindow());
        }
        catch (System.IO.FileNotFoundException e)
        {
            MessageBox.Show(
                $"The file {e.FileName} is missing. Please check if you have extracted all files to the folder wherein the program locate.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
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