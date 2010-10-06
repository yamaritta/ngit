using System;
using NGit;
using NGit.Util.IO;
using Sharpen;

namespace NGit.Util.IO
{
	/// <summary>Triggers an interrupt on the calling thread if it doesn't complete a block.
	/// 	</summary>
	/// <remarks>
	/// Triggers an interrupt on the calling thread if it doesn't complete a block.
	/// <p>
	/// Classes can use this to trip an alarm interrupting the calling thread if it
	/// doesn't complete a block within the specified timeout. Typical calling
	/// pattern is:
	/// <pre>
	/// private InterruptTimer myTimer = ...;
	/// void foo() {
	/// try {
	/// myTimer.begin(timeout);
	/// // work
	/// } finally {
	/// myTimer.end();
	/// }
	/// }
	/// </pre>
	/// <p>
	/// An InterruptTimer is not recursive. To implement recursive timers,
	/// independent InterruptTimer instances are required. A single InterruptTimer
	/// may be shared between objects which won't recursively call each other.
	/// <p>
	/// Each InterruptTimer spawns one background thread to sleep the specified time
	/// and interrupt the thread which called
	/// <see cref="Begin(int)">Begin(int)</see>
	/// . It is up to the
	/// caller to ensure that the operations within the work block between the
	/// matched begin and end calls tests the interrupt flag (most IO operations do).
	/// <p>
	/// To terminate the background thread, use
	/// <see cref="Terminate()">Terminate()</see>
	/// . If the
	/// application fails to terminate the thread, it will (eventually) terminate
	/// itself when the InterruptTimer instance is garbage collected.
	/// </remarks>
	/// <seealso cref="TimeoutInputStream">TimeoutInputStream</seealso>
	public sealed class InterruptTimer
	{
		private readonly InterruptTimer.AlarmState state;

		private readonly InterruptTimer.AlarmThread thread;

		internal readonly InterruptTimer.AutoKiller autoKiller;

		/// <summary>Create a new timer with a default thread name.</summary>
		/// <remarks>Create a new timer with a default thread name.</remarks>
		public InterruptTimer() : this("JGit-InterruptTimer")
		{
		}

		/// <summary>Create a new timer to signal on interrupt on the caller.</summary>
		/// <remarks>
		/// Create a new timer to signal on interrupt on the caller.
		/// <p>
		/// The timer thread is created in the calling thread's ThreadGroup.
		/// </remarks>
		/// <param name="threadName">name of the timer thread.</param>
		public InterruptTimer(string threadName)
		{
			state = new InterruptTimer.AlarmState();
			autoKiller = new InterruptTimer.AutoKiller(state);
			thread = new InterruptTimer.AlarmThread(threadName, state);
			thread.Start();
		}

		/// <summary>Arm the interrupt timer before entering a blocking operation.</summary>
		/// <remarks>Arm the interrupt timer before entering a blocking operation.</remarks>
		/// <param name="timeout">
		/// number of milliseconds before the interrupt should trigger.
		/// Must be &gt; 0.
		/// </param>
		public void Begin(int timeout)
		{
			if (timeout <= 0)
			{
				throw new ArgumentException(MessageFormat.Format(JGitText.Get().invalidTimeout, timeout
					));
			}
			Sharpen.Thread.Interrupted();
			state.Begin(timeout);
		}

		/// <summary>Disable the interrupt timer, as the operation is complete.</summary>
		/// <remarks>Disable the interrupt timer, as the operation is complete.</remarks>
		public void End()
		{
			state.End();
		}

		/// <summary>Shutdown the timer thread, and wait for it to terminate.</summary>
		/// <remarks>Shutdown the timer thread, and wait for it to terminate.</remarks>
		public void Terminate()
		{
			state.Terminate();
			try
			{
				thread.Join();
			}
			catch (Exception)
			{
			}
		}

		internal sealed class AlarmThread : Sharpen.Thread
		{
			internal AlarmThread(string name, InterruptTimer.AlarmState q) : base(q)
			{
				//
				SetName(name);
				SetDaemon(true);
			}
		}

		internal sealed class AutoKiller
		{
			private readonly InterruptTimer.AlarmState state;

			internal AutoKiller(InterruptTimer.AlarmState s)
			{
				// The trick here is, the AlarmThread does not have a reference to the
				// AutoKiller instance, only the InterruptTimer itself does. Thus when
				// the InterruptTimer is GC'd, the AutoKiller is also unreachable and
				// can be GC'd. When it gets finalized, it tells the AlarmThread to
				// terminate, triggering the thread to exit gracefully.
				//
				state = s;
			}

			~AutoKiller()
			{
				state.Terminate();
			}
		}

		internal sealed class AlarmState : Runnable
		{
			private Sharpen.Thread callingThread;

			private long deadline;

			private bool terminated;

			public AlarmState()
			{
				callingThread = Sharpen.Thread.CurrentThread();
			}

			public void Run()
			{
				lock (this)
				{
					while (!terminated && callingThread.IsAlive())
					{
						try
						{
							if (0 < deadline)
							{
								long delay = deadline - Now();
								if (delay <= 0)
								{
									deadline = 0;
									callingThread.Interrupt();
								}
								else
								{
									Sharpen.Runtime.Wait(this, delay);
								}
							}
							else
							{
								Sharpen.Runtime.Wait(this, 1000);
							}
						}
						catch (Exception)
						{
						}
					}
				}
			}

			// Treat an interrupt as notice to examine state.
			internal void Begin(int timeout)
			{
				lock (this)
				{
					if (terminated)
					{
						throw new InvalidOperationException(JGitText.Get().timerAlreadyTerminated);
					}
					callingThread = Sharpen.Thread.CurrentThread();
					deadline = Now() + timeout;
					Sharpen.Runtime.NotifyAll(this);
				}
			}

			internal void End()
			{
				lock (this)
				{
					if (0 == deadline)
					{
						Sharpen.Thread.Interrupted();
					}
					else
					{
						deadline = 0;
					}
					Sharpen.Runtime.NotifyAll(this);
				}
			}

			internal void Terminate()
			{
				lock (this)
				{
					if (!terminated)
					{
						deadline = 0;
						terminated = true;
						Sharpen.Runtime.NotifyAll(this);
					}
				}
			}

			private static long Now()
			{
				return Runtime.CurrentTimeMillis();
			}
		}
	}
}
