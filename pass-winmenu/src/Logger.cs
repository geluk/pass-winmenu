using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PassWinmenu
{
	enum LogLevel
	{
		Debug,
		Info,
		Warning,
		Error
	}

	internal class LogLine
	{
		public DateTime DateTime { get; set; }
		public string Message { get; set; }

		public override string ToString()
		{
			var line = $"[{DateTime:HH:mm:ss.fff}] {Message}";
			return line;
		}
	}

	static class Log
	{
		private static StreamWriter writer;
		public static List<LogLine> History { get; } = new List<LogLine>();

		static Log()
		{
			AppDomain.CurrentDomain.UnhandledException += ReportException;
			TaskScheduler.UnobservedTaskException += ReportTaskSchedulerException;
			if (Application.Current != null)
			{
				Application.Current.DispatcherUnhandledException += ReportDispatcherException;
			}
		}

		public static void EnableFileLogging()
		{
			if (writer != null) return;

			try
			{
				writer = new StreamWriter(File.Open("pass-winmenu.log", FileMode.Append, FileAccess.Write, FileShare.Read));

				// If log lines have already been generated, write them now.
				foreach (var line in History)
				{
					writer.WriteLine(line);
				}

				writer.Flush();

				// From now on, we should auto-flush to ensure lines get written to the log file as soon as possible.
				writer.AutoFlush = true;
			}
			catch (Exception e)
			{
				MessageBox.Show($"The log file could not be created: an error occurred ({e.GetType().Name}: {e.Message})", "Failed to create log file");
			}
		}

		private static void ReportTaskSchedulerException(object sender, UnobservedTaskExceptionEventArgs eventArgs)
		{
			SendRaw("An unhandled exception occurred in a background task. Stack trace:");
			LogExceptionAsText(eventArgs.Exception, 0);
		}

		private static void ReportDispatcherException(object sender, DispatcherUnhandledExceptionEventArgs eventArgs)
		{
			SendRaw("An unhandled exception occurred. Stack trace:");
			LogExceptionAsText(eventArgs.Exception, 0);
		}

		private static void ReportException(object sender, UnhandledExceptionEventArgs eventArgs)
		{
			SendRaw("An unhandled exception occurred. Stack trace:");
			LogExceptionAsText(eventArgs.ExceptionObject as Exception, 0);
		}

		public static void ReportException(Exception e)
		{
			LogExceptionAsText(e, 0);
		}

		private static void LogExceptionAsText(Exception e, int level)
		{
			var trace = new StackTrace(e, true);
			var bottomFrame = trace.GetFrame(0);
			var indents = string.Concat(Enumerable.Repeat("  ", level));

			if (e is AggregateException aggr)
			{
				// Don't log the stack trace, instead log all inner exceptions and log their stacktraces instead.
				SendRaw($"{indents}AggregateException in {bottomFrame?.GetFileName()}:{bottomFrame?.GetFileLineNumber()}:{bottomFrame?.GetFileColumnNumber()} - Sub-exceptions:");
				foreach (var inner in aggr.InnerExceptions)
				{
					LogExceptionAsText(inner, ++level);
				}
			}
			else
			{
				SendRaw($"{indents}{e.GetType().Name} ({e.Message}) in {bottomFrame?.GetFileName()}:{bottomFrame?.GetFileLineNumber()}:{bottomFrame?.GetFileColumnNumber()}");

				if (e.InnerException != null)
				{
					LogExceptionAsText(e.InnerException, ++level);
				}
				else
				{
					// Only log the stacktrace if there is no inner exception,
					// otherwise the inner exception can print it instead.
					var stackFrames = trace.GetFrames();
					if (stackFrames == null) return;

					foreach (var frame in stackFrames)
					{
						SendRaw($"{indents}  -> in {frame.GetMethod().ReflectedType?.FullName ?? "unknown"}.{frame.GetMethod().Name} -- {frame.GetFileName()}:{frame.GetFileLineNumber()}:{frame.GetFileColumnNumber()}");
					}
				}
			}
		}

		private static void SendRaw(string message)
		{
			var submissionTime = DateTime.Now;
			var line = new LogLine
			{
				DateTime = submissionTime,
				Message = message
			};

			lock (History)
			{
				History.Add(line);
			}
#if DEBUG
			Console.WriteLine(line);
#endif
			writer?.WriteLine(line);
		}

		public static void Send([Localizable(false)]string message, LogLevel level = LogLevel.Debug)
		{
			SendRaw($"[{GetLevelString(level)}] {message}");
		}

		private static string GetLevelString(LogLevel level)
		{
			switch (level)
			{
				case LogLevel.Debug:
					return "DBG";
				case LogLevel.Info:
					return "INF";
				case LogLevel.Warning:
					return "WRN";
				case LogLevel.Error:
					return "ERR";
				default:
					return "UNKNOWN";
			}
		}
	}
}
