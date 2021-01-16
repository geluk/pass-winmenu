using System.Collections.Generic;
using System.Diagnostics;

namespace PassWinmenu.Utilities
{
	internal class TimerHelper
	{
		public static TimerHelper Current { get; } = new TimerHelper();

		private List<ExecutionTimer> timers = new List<ExecutionTimer>();

		public IReadOnlyList<ExecutionTimer> Timers => timers;
		public ExecutionTimer CurrentTimer { get; set; }

		public void Start(string name)
		{
			if(CurrentTimer != null)
			{
				CurrentTimer.Stop();
			}
			CurrentTimer = new ExecutionTimer(name);
			timers.Add(CurrentTimer);
			CurrentTimer.Start();
		}

		public void TakeSnapshot(string snapName)
		{
			// No need to do anything if we're not running a timer.
			CurrentTimer?.TakeSnapshot(snapName);
		}
	}

	internal class ExecutionTimer
	{
		private Stopwatch stopwatch;
		private long lastSnapshotMs;
		private List<TimerSnapshot> snapshots { get; set; } = new List<TimerSnapshot>();

		public IReadOnlyList<TimerSnapshot> Snapshots => snapshots;
		public string Name { get; }

		public ExecutionTimer(string name)
		{
			Name = name;
		}

		public void Start()
		{
			stopwatch = Stopwatch.StartNew();
			lastSnapshotMs = 0;
		}

		public void Stop()
		{
			stopwatch.Stop();
		}

		public void TakeSnapshot(string name)
		{
			var elapsed = stopwatch.ElapsedMilliseconds;
			var sinceLast = elapsed - lastSnapshotMs;
			lastSnapshotMs = elapsed;
			snapshots.Add(new TimerSnapshot(name, sinceLast, elapsed));
		}
	}

	[DebuggerDisplay("TimerSnapshot: {Name} - {MsSinceLast}ms since last, {MsTotal}ms total")]
	internal class TimerSnapshot
	{
		public TimerSnapshot(string name, long msSinceLast, long msTotal)
		{
			Name = name;
			MsSinceLast = msSinceLast;
			MsTotal = msTotal;
		}

		public string Name { get; }
		public long MsSinceLast { get; }
		public long MsTotal { get; }
	}
}
