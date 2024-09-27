using System.Text;

namespace Logger
{   
    public enum MES_TYPE
    {
        Info, Success, Warn, Error
    }

    public struct LoggerSettings
    {
        public bool showInfo = true, showSuccess = true, showWarn = true, showError = true;
        public bool showDate = true, showTime = true;
        public bool showType;
        public ConsoleColor colorInfo = ConsoleColor.White;
        public ConsoleColor colorSuccess = ConsoleColor.Green;
        public ConsoleColor colorWarn = ConsoleColor.DarkYellow;
        public ConsoleColor colorError = ConsoleColor.Red;
        public LoggerSettings() { }
    }

    internal class LoggedMessage
    {
        public MES_TYPE type;
        public DateTime dateTime;
        public string message;

        public LoggedMessage(MES_TYPE type, DateTime dateTime, string message)
        {
            this.type = type;
            this.dateTime = dateTime;
            this.message = message;
        }
    }

    // Class For asynchronous logging
    public class AsyncLogger
    {
        private readonly TextWriter _output;
        private readonly bool _consoleOutput;
        private readonly LoggerSettings _settings;
        private readonly Queue<LoggedMessage> _messageQueue;
        private readonly object _lockObject = new object();
        private bool _running = true;

        public AsyncLogger(TextWriter? output = null) : this (new LoggerSettings(), output) { }

        public AsyncLogger(LoggerSettings settings, TextWriter? output = null)
        {
            if (output == null)
            {
                _output = Console.Out;
                _consoleOutput = true;
            }
            else _output = output;
            _settings = settings;
            _messageQueue = new Queue<LoggedMessage>();
            Thread loggerThread = new Thread(ProcessQueue);
            loggerThread.Start();
        }

        public void LogInfo(string message) => Log(message, MES_TYPE.Info);
        public void LogSuccess(string message) => Log(message, MES_TYPE.Success);
        public void LogError(string message) => Log(message, MES_TYPE.Error);
        public void LogWarn(string message) => Log(message, MES_TYPE.Warn);

        // Appends Queue with new message to log
        public void Log(string message, MES_TYPE type)
        {
            lock (_lockObject)
            {
                var loggedMes = new LoggedMessage(type, DateTime.Now, message);
                _messageQueue.Enqueue(loggedMes);
                Monitor.Pulse(_lockObject);
            }
        }

        private void ProcessQueue()
        {
            while (_running)
            {
                LoggedMessage? message = null;

                lock (_lockObject)
                {
                    while (_messageQueue.Count == 0 && _running)
                    {
                        Monitor.Wait(_lockObject);
                    }

                    if (_messageQueue.Count > 0)
                    {
                        message = _messageQueue.Dequeue();
                    }
                }

                if (message != null)
                {
                    WriteMessage(message);
                }
            }
        }
        public void Stop()
        {
            lock (_lockObject)
            {
                _running = false;
                Monitor.PulseAll(_lockObject); // Ensure we exit the waiting state
            }
        }

        private void WriteMessage(LoggedMessage message)
        {
            bool show;
            string type = "";
            ConsoleColor color = default;

            switch (message.type)
            {
                case MES_TYPE.Info:
                    show = _settings.showInfo;
                    type = "Information";
                    color = _settings.colorInfo;
                    break;
                case MES_TYPE.Success:
                    show = _settings.showSuccess;
                    type = "Success";
                    color = _settings.colorSuccess;
                    break;
                case MES_TYPE.Error:
                    show = _settings.showError;
                    type = "Error";
                    color = _settings.colorError;
                    break;
                case MES_TYPE.Warn:
                    show = _settings.showWarn;
                    type = "Warning";
                    color = _settings.colorWarn;
                    break;
                default:
                    show = false;
                    break;
            }

            if (!show) return;
            if (_consoleOutput) Console.ForegroundColor = color;
            StringBuilder builder = new StringBuilder();
            if (_settings.showDate) builder.Append(message.dateTime.Date.ToString()).Append(" ");
            if (_settings.showTime) builder.Append(message.dateTime.TimeOfDay.ToString()).Append(" ");
            if (_settings.showType) builder.Append('[').Append(type).Append("] ");
            builder.Append(message.message);

            lock (_output)
            {
                _output.WriteLine(builder.ToString());
            }

            if (_consoleOutput) Console.ResetColor();
        }
    }
}
