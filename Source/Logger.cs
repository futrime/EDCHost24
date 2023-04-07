using System.IO;
using System;

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
    WriteToFile($"{GetCurrentTime()} [DEBUG] {message}");
  }

  public void Info(string message) {
    WriteToFile($"{GetCurrentTime()} [INFO]  {message}");
  }

  public void Warn(string message) {
    WriteToFile($"{GetCurrentTime()} [WARN]  {message}");
  }

  public void Error(string message) {
    WriteToFile($"{GetCurrentTime()} [ERROR] {message}");
  }

  private string GetCurrentTime() {
    return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
  }

  private void WriteToFile(string text) {
    using (StreamWriter sw = File.AppendText(_logFilePath)) {
      sw.WriteLine(text);
    }
  }
}