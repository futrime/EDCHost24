using System.IO;

namespace EdcHost;

/// <summary>
/// Logger class provides logging functionality.
/// </summary>
internal class Logger {
  private string _logFilePath;

  public Logger(string logFilePath) {
    _logFilePath = logFilePath;

    string dir = Path.GetDirectoryName(_logFilePath);
    if (!Directory.Exists(dir)) {
      Directory.CreateDirectory(dir);
    }
  }
  
  public void Debug(string message) {
    using (StreamWriter sw = File.AppendText(_logFilePath)) {
      sw.WriteLine(message);
    }
  }
}