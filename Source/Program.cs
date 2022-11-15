using System;
using System.Threading;
using System.Windows.Forms;

namespace EdcHost;

/// <summary>
/// The program
/// </summary>
static class Program
{
    /// <summary>
    /// The mutex ID for restricting one instance
    /// </summary>
    private const string MutexId = "EdcHost";

    [STAThread]
    static void Main()
    {
        // The program can only have one instance.
        using Mutex mutex = new Mutex(false, MutexId);
        if (!mutex.WaitOne(0, false))
        {
            MessageBox.Show("The program is already running.", "Nahida complained:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

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
                "Nahida said:",
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
            "Nahida choked and said:",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
    }
}