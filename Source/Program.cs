using System;
using System.Windows.Forms;

namespace EdcHost;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainWindow());
    }
}