using System;
using System.Collections.Generic;
using System.Text;
using NGit;
using NGit.Errors;
using NGit.Storage.Pack;
using NGit.Transport;
using Sharpen;

namespace NGit.Transport
{
	/// <summary>Push implementation using the native Git pack transfer service.</summary>
	/// <remarks>
	/// Push implementation using the native Git pack transfer service.
	/// <p>
	/// This is the canonical implementation for transferring objects to the remote
	/// repository from the local repository by talking to the 'git-receive-pack'
	/// service. Objects are packed on the local side into a pack file and then sent
	/// to the remote repository.
	/// <p>
	/// This connection requires only a bi-directional pipe or socket, and thus is
	/// easily wrapped up into a local process pipe, anonymous TCP socket, or a
	/// command executed through an SSH tunnel.
	/// <p>
	/// This implementation honors
	/// <see cref="Transport.IsPushThin()">Transport.IsPushThin()</see>
	/// option.
	/// <p>
	/// Concrete implementations should just call
	/// <see cref="BasePackConnection.Init(Sharpen.InputStream, Sharpen.OutputStream)">BasePackConnection.Init(Sharpen.InputStream, Sharpen.OutputStream)
	/// 	</see>
	/// and
	/// <see cref="BasePackConnection.ReadAdvertisedRefs()">BasePackConnection.ReadAdvertisedRefs()
	/// 	</see>
	/// methods in constructor or before any use. They
	/// should also handle resources releasing in
	/// <see cref="BasePackConnection.Close()">BasePackConnection.Close()</see>
	/// method if needed.
	/// </remarks>
	public abstract class BasePackPushConnection : BasePackConnection, PushConnection
	{
		internal static readonly string CAPABILITY_REPORT_STATUS = "report-status";

		internal static readonly string CAPABILITY_DELETE_REFS = "delete-refs";

		internal static readonly string CAPABILITY_OFS_DELTA = "ofs-delta";

		internal static readonly string CAPABILITY_SIDE_BAND_64K = "side-band-64k";

		private readonly bool thinPack;

		private bool capableDeleteRefs;

		private bool capableReport;

		private bool capableSideBand;

		private bool capableOfsDelta;

		private bool sentCommand;

		private bool writePack;

		/// <summary>Time in milliseconds spent transferring the pack data.</summary>
		/// <remarks>Time in milliseconds spent transferring the pack data.</remarks>
		private long packTransferTime;

		/// <summary>Create a new connection to push using the native git transport.</summary>
		/// <remarks>Create a new connection to push using the native git transport.</remarks>
		/// <param name="packTransport">the transport.</param>
		internal BasePackPushConnection(PackTransport packTransport) : base(packTransport
			)
		{
			thinPack = transport.IsPushThin();
		}

		/// <exception cref="NGit.Errors.TransportException"></exception>
		public virtual void Push(ProgressMonitor monitor, IDictionary<string, RemoteRefUpdate
			> refUpdates)
		{
			MarkStartedOperation();
			DoPush(monitor, refUpdates);
		}

		protected internal override TransportException NoRepository()
		{
			// Sadly we cannot tell the "invalid URI" case from "push not allowed".
			// Opening a fetch connection can help us tell the difference, as any
			// useful repository is going to support fetch if it also would allow
			// push. So if fetch throws NoRemoteRepositoryException we know the
			// URI is wrong. Otherwise we can correctly state push isn't allowed
			// as the fetch connection opened successfully.
			//
			try
			{
				transport.OpenFetch().Close();
			}
			catch (NotSupportedException)
			{
			}
			catch (NoRemoteRepositoryException e)
			{
				// Fall through.
				// Fetch concluded the repository doesn't exist.
				//
				return e;
			}
			catch (TransportException)
			{
			}
			// Fall through.
			return new TransportException(uri, JGitText.Get().pushNotPermitted);
		}

		/// <summary>Push one or more objects and update the remote repository.</summary>
		/// <remarks>Push one or more objects and update the remote repository.</remarks>
		/// <param name="monitor">progress monitor to receive status updates.</param>
		/// <param name="refUpdates">update commands to be applied to the remote repository.</param>
		/// <exception cref="NGit.Errors.TransportException">if any exception occurs.</exception>
		protected internal virtual void DoPush(ProgressMonitor monitor, IDictionary<string
			, RemoteRefUpdate> refUpdates)
		{
			try
			{
				WriteCommands(refUpdates.Values, monitor);
				if (writePack)
				{
					WritePack(refUpdates, monitor);
				}
				if (sentCommand)
				{
					if (capableReport)
					{
						ReadStatusReport(refUpdates);
					}
					if (capableSideBand)
					{
						// Ensure the data channel is at EOF, so we know we have
						// read all side-band data from all channels and have a
						// complete copy of the messages (if any) buffered from
						// the other data channels.
						//
						int b = @in.Read();
						if (0 <= b)
						{
							throw new TransportException(uri, MessageFormat.Format(JGitText.Get().expectedEOFReceived
								, (char)b));
						}
					}
				}
			}
			catch (TransportException e)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new TransportException(uri, e.Message, e);
			}
			finally
			{
				Close();
			}
		}

		/// <exception cref="System.IO.IOException"></exception>
		private void WriteCommands(ICollection<RemoteRefUpdate> refUpdates, ProgressMonitor
			 monitor)
		{
			string capabilities = EnableCapabilities(monitor);
			foreach (RemoteRefUpdate rru in refUpdates)
			{
				if (!capableDeleteRefs && rru.IsDelete())
				{
					rru.SetStatus(RemoteRefUpdate.Status.REJECTED_NODELETE);
					continue;
				}
				StringBuilder sb = new StringBuilder();
				Ref advertisedRef = GetRef(rru.GetRemoteName());
				ObjectId oldId = (advertisedRef == null ? ObjectId.ZeroId : advertisedRef.GetObjectId
					());
				sb.Append(oldId.Name);
				sb.Append(' ');
				sb.Append(rru.GetNewObjectId().Name);
				sb.Append(' ');
				sb.Append(rru.GetRemoteName());
				if (!sentCommand)
				{
					sentCommand = true;
					sb.Append(capabilities);
				}
				pckOut.WriteString(sb.ToString());
				rru.SetStatus(RemoteRefUpdate.Status.AWAITING_REPORT);
				if (!rru.IsDelete())
				{
					writePack = true;
				}
			}
			if (monitor.IsCancelled())
			{
				throw new TransportException(uri, JGitText.Get().pushCancelled);
			}
			pckOut.End();
			outNeedsEnd = false;
		}

		private string EnableCapabilities(ProgressMonitor monitor)
		{
			StringBuilder line = new StringBuilder();
			capableReport = WantCapability(line, CAPABILITY_REPORT_STATUS);
			capableDeleteRefs = WantCapability(line, CAPABILITY_DELETE_REFS);
			capableOfsDelta = WantCapability(line, CAPABILITY_OFS_DELTA);
			capableSideBand = WantCapability(line, CAPABILITY_SIDE_BAND_64K);
			if (capableSideBand)
			{
				@in = new SideBandInputStream(@in, monitor, GetMessageWriter());
				pckIn = new PacketLineIn(@in);
			}
			if (line.Length > 0)
			{
				Sharpen.Runtime.SetCharAt(line, 0, '\0');
			}
			return line.ToString();
		}

		/// <exception cref="System.IO.IOException"></exception>
		private void WritePack(IDictionary<string, RemoteRefUpdate> refUpdates, ProgressMonitor
			 monitor)
		{
			IList<ObjectId> remoteObjects = new AList<ObjectId>(GetRefs().Count);
			IList<ObjectId> newObjects = new AList<ObjectId>(refUpdates.Count);
			long start;
			PackWriter writer = new PackWriter(transport.GetPackConfig(), local.NewObjectReader
				());
			try
			{
				foreach (Ref r in GetRefs())
				{
					remoteObjects.AddItem(r.GetObjectId());
				}
				Sharpen.Collections.AddAll(remoteObjects, additionalHaves);
				foreach (RemoteRefUpdate r_1 in refUpdates.Values)
				{
					if (!ObjectId.ZeroId.Equals(r_1.GetNewObjectId()))
					{
						newObjects.AddItem(r_1.GetNewObjectId());
					}
				}
				writer.SetThin(thinPack);
				writer.SetDeltaBaseAsOffset(capableOfsDelta);
				writer.PreparePack(monitor, newObjects, remoteObjects);
				start = Runtime.CurrentTimeMillis();
				writer.WritePack(monitor, monitor, @out);
			}
			finally
			{
				writer.Release();
			}
			@out.Flush();
			packTransferTime = Runtime.CurrentTimeMillis() - start;
		}

		/// <exception cref="System.IO.IOException"></exception>
		private void ReadStatusReport(IDictionary<string, RemoteRefUpdate> refUpdates)
		{
			string unpackLine = ReadStringLongTimeout();
			if (!unpackLine.StartsWith("unpack "))
			{
				throw new PackProtocolException(uri, MessageFormat.Format(JGitText.Get().unexpectedReportLine
					, unpackLine));
			}
			string unpackStatus = Sharpen.Runtime.Substring(unpackLine, "unpack ".Length);
			if (!unpackStatus.Equals("ok"))
			{
				throw new TransportException(uri, MessageFormat.Format(JGitText.Get().errorOccurredDuringUnpackingOnTheRemoteEnd
					, unpackStatus));
			}
			string refLine;
			while ((refLine = pckIn.ReadString()) != PacketLineIn.END)
			{
				bool ok = false;
				int refNameEnd = -1;
				if (refLine.StartsWith("ok "))
				{
					ok = true;
					refNameEnd = refLine.Length;
				}
				else
				{
					if (refLine.StartsWith("ng "))
					{
						ok = false;
						refNameEnd = refLine.IndexOf(" ", 3);
					}
				}
				if (refNameEnd == -1)
				{
					throw new PackProtocolException(MessageFormat.Format(JGitText.Get().unexpectedReportLine2
						, uri, refLine));
				}
				string refName = Sharpen.Runtime.Substring(refLine, 3, refNameEnd);
				string message = (ok ? null : Sharpen.Runtime.Substring(refLine, refNameEnd + 1));
				RemoteRefUpdate rru = refUpdates.Get(refName);
				if (rru == null)
				{
					throw new PackProtocolException(MessageFormat.Format(JGitText.Get().unexpectedRefReport
						, uri, refName));
				}
				if (ok)
				{
					rru.SetStatus(RemoteRefUpdate.Status.OK);
				}
				else
				{
					rru.SetStatus(RemoteRefUpdate.Status.REJECTED_OTHER_REASON);
					rru.SetMessage(message);
				}
			}
			foreach (RemoteRefUpdate rru_1 in refUpdates.Values)
			{
				if (rru_1.GetStatus() == RemoteRefUpdate.Status.AWAITING_REPORT)
				{
					throw new PackProtocolException(MessageFormat.Format(JGitText.Get().expectedReportForRefNotReceived
						, uri, rru_1.GetRemoteName()));
				}
			}
		}

		/// <exception cref="System.IO.IOException"></exception>
		private string ReadStringLongTimeout()
		{
			if (timeoutIn == null)
			{
				return pckIn.ReadString();
			}
			// The remote side may need a lot of time to choke down the pack
			// we just sent them. There may be many deltas that need to be
			// resolved by the remote. Its hard to say how long the other
			// end is going to be silent. Taking 10x the configured timeout
			// or the time spent transferring the pack, whichever is larger,
			// gives the other side some reasonable window to process the data,
			// but this is just a wild guess.
			//
			int oldTimeout = timeoutIn.GetTimeout();
			int sendTime = (int)Math.Min(packTransferTime, 28800000L);
			try
			{
				timeoutIn.SetTimeout(10 * Math.Max(sendTime, oldTimeout));
				return pckIn.ReadString();
			}
			finally
			{
				timeoutIn.SetTimeout(oldTimeout);
			}
		}
	}
}
