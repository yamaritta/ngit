using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using NGit;
using NGit.Errors;
using NGit.Storage.File;
using NGit.Transport;
using NGit.Util;
using NGit.Util.IO;
using Sharpen;

namespace NGit.Transport
{
	/// <summary>Transport over HTTP and FTP protocols.</summary>
	/// <remarks>
	/// Transport over HTTP and FTP protocols.
	/// <p>
	/// If the transport is using HTTP and the remote HTTP service is Git-aware
	/// (speaks the "smart-http protocol") this client will automatically take
	/// advantage of the additional Git-specific HTTP extensions. If the remote
	/// service does not support these extensions, the client will degrade to direct
	/// file fetching.
	/// <p>
	/// If the remote (server side) repository does not have the specialized Git
	/// support, object files are retrieved directly through standard HTTP GET (or
	/// binary FTP GET) requests. This make it easy to serve a Git repository through
	/// a standard web host provider that does not offer specific support for Git.
	/// </remarks>
	/// <seealso cref="WalkFetchConnection">WalkFetchConnection</seealso>
	public class TransportHttp : HttpTransport, WalkTransport, PackTransport
	{
		private static readonly string SVC_UPLOAD_PACK = "git-upload-pack";

		private static readonly string SVC_RECEIVE_PACK = "git-receive-pack";

		private static readonly string userAgent = ComputeUserAgent();

		internal static bool CanHandle(URIish uri)
		{
			if (!uri.IsRemote())
			{
				return false;
			}
			string s = uri.GetScheme();
			return "http".Equals(s) || "https".Equals(s) || "ftp".Equals(s);
		}

		private static string ComputeUserAgent()
		{
			string version;
			Assembly pkg = typeof(NGit.Transport.TransportHttp).Assembly;
			if (pkg != null && pkg.GetImplementationVersion() != null)
			{
				version = pkg.GetImplementationVersion();
			}
			else
			{
				version = "unknown";
			}
			//$NON-NLS-1$
			return "JGit/" + version;
		}

		private sealed class _SectionParser_141 : Config.SectionParser<TransportHttp.HttpConfig
			>
		{
			public _SectionParser_141()
			{
			}

			//$NON-NLS-1$
			public TransportHttp.HttpConfig Parse(Config cfg)
			{
				return new TransportHttp.HttpConfig(cfg);
			}
		}

		private static readonly Config.SectionParser<TransportHttp.HttpConfig> HTTP_KEY = 
			new _SectionParser_141();

		private class HttpConfig
		{
			internal readonly int postBuffer;

			internal HttpConfig(Config rc)
			{
				postBuffer = rc.GetInt("http", "postbuffer", 1 * 1024 * 1024);
			}
		}

		private readonly Uri baseUrl;

		private readonly Uri objectsUrl;

		private readonly TransportHttp.HttpConfig http;

		private readonly ProxySelector proxySelector;

		private bool useSmartHttp = true;

		/// <exception cref="System.NotSupportedException"></exception>
		protected internal TransportHttp(Repository local, URIish uri) : base(local, uri)
		{
			try
			{
				string uriString = uri.ToString();
				if (!uriString.EndsWith("/"))
				{
					uriString += "/";
				}
				baseUrl = new Uri(uriString);
				objectsUrl = new Uri(baseUrl, "objects/");
			}
			catch (UriFormatException e)
			{
				throw new NotSupportedException(MessageFormat.Format(JGitText.Get().invalidURL, uri
					), e);
			}
			http = local.GetConfig().Get(HTTP_KEY);
			proxySelector = ProxySelector.GetDefault();
		}

		/// <summary>Toggle whether or not smart HTTP transport should be used.</summary>
		/// <remarks>
		/// Toggle whether or not smart HTTP transport should be used.
		/// <p>
		/// This flag exists primarily to support backwards compatibility testing
		/// within a testing framework, there is no need to modify it in most
		/// applications.
		/// </remarks>
		/// <param name="on">
		/// if
		/// <code>true</code>
		/// (default), smart HTTP is enabled.
		/// </param>
		public virtual void SetUseSmartHttp(bool on)
		{
			useSmartHttp = on;
		}

		/// <exception cref="NGit.Errors.TransportException"></exception>
		/// <exception cref="System.NotSupportedException"></exception>
		public override FetchConnection OpenFetch()
		{
			string service = SVC_UPLOAD_PACK;
			try
			{
				HttpURLConnection c = Connect(service);
				InputStream @in = OpenInputStream(c);
				try
				{
					if (IsSmartHttp(c, service))
					{
						ReadSmartHeaders(@in, service);
						return new TransportHttp.SmartHttpFetchConnection(this, @in);
					}
					else
					{
						// Assume this server doesn't support smart HTTP fetch
						// and fall back on dumb object walking.
						//
						return NewDumbConnection(@in);
					}
				}
				finally
				{
					@in.Close();
				}
			}
			catch (NotSupportedException err)
			{
				throw;
			}
			catch (TransportException err)
			{
				throw;
			}
			catch (IOException err)
			{
				throw new TransportException(uri, JGitText.Get().errorReadingInfoRefs, err);
			}
		}

		/// <exception cref="System.IO.IOException"></exception>
		/// <exception cref="NGit.Errors.PackProtocolException"></exception>
		private FetchConnection NewDumbConnection(InputStream @in)
		{
			TransportHttp.HttpObjectDB d = new TransportHttp.HttpObjectDB(this, objectsUrl);
			BufferedReader br = ToBufferedReader(@in);
			IDictionary<string, Ref> refs;
			try
			{
				refs = d.ReadAdvertisedImpl(br);
			}
			finally
			{
				br.Close();
			}
			if (!refs.ContainsKey(Constants.HEAD))
			{
				// If HEAD was not published in the info/refs file (it usually
				// is not there) download HEAD by itself as a loose file and do
				// the resolution by hand.
				//
				HttpURLConnection conn = HttpOpen(new Uri(baseUrl, Constants.HEAD));
				int status = HttpSupport.Response(conn);
				switch (status)
				{
					case HttpURLConnection.HTTP_OK:
					{
						br = ToBufferedReader(OpenInputStream(conn));
						try
						{
							string line = br.ReadLine();
							if (line != null && line.StartsWith(RefDirectory.SYMREF))
							{
								string target = Sharpen.Runtime.Substring(line, RefDirectory.SYMREF.Length);
								Ref r = refs.Get(target);
								if (r == null)
								{
									r = new ObjectIdRef.Unpeeled(RefStorage.NEW, target, null);
								}
								r = new SymbolicRef(Constants.HEAD, r);
								refs.Put(r.GetName(), r);
							}
							else
							{
								if (line != null && ObjectId.IsId(line))
								{
									Ref r = new ObjectIdRef.Unpeeled(RefStorage.NETWORK, Constants.HEAD, ObjectId.FromString
										(line));
									refs.Put(r.GetName(), r);
								}
							}
						}
						finally
						{
							br.Close();
						}
						break;
					}

					case HttpURLConnection.HTTP_NOT_FOUND:
					{
						break;
					}

					default:
					{
						throw new TransportException(uri, MessageFormat.Format(JGitText.Get().cannotReadHEAD
							, status, conn.GetResponseMessage()));
					}
				}
			}
			WalkFetchConnection wfc = new WalkFetchConnection(this, d);
			wfc.Available(refs);
			return wfc;
		}

		private BufferedReader ToBufferedReader(InputStream @in)
		{
			return new BufferedReader(new InputStreamReader(@in, Constants.CHARSET));
		}

		/// <exception cref="System.NotSupportedException"></exception>
		/// <exception cref="NGit.Errors.TransportException"></exception>
		public override PushConnection OpenPush()
		{
			string service = SVC_RECEIVE_PACK;
			try
			{
				HttpURLConnection c = Connect(service);
				InputStream @in = OpenInputStream(c);
				try
				{
					if (IsSmartHttp(c, service))
					{
						ReadSmartHeaders(@in, service);
						return new TransportHttp.SmartHttpPushConnection(this, @in);
					}
					else
					{
						if (!useSmartHttp)
						{
							string msg = JGitText.Get().smartHTTPPushDisabled;
							throw new NotSupportedException(msg);
						}
						else
						{
							string msg = JGitText.Get().remoteDoesNotSupportSmartHTTPPush;
							throw new NotSupportedException(msg);
						}
					}
				}
				finally
				{
					@in.Close();
				}
			}
			catch (NotSupportedException err)
			{
				throw;
			}
			catch (TransportException err)
			{
				throw;
			}
			catch (IOException err)
			{
				throw new TransportException(uri, JGitText.Get().errorReadingInfoRefs, err);
			}
		}

		public override void Close()
		{
		}

		// No explicit connections are maintained.
		/// <exception cref="NGit.Errors.TransportException"></exception>
		/// <exception cref="System.NotSupportedException"></exception>
		private HttpURLConnection Connect(string service)
		{
			Uri u;
			try
			{
				StringBuilder b = new StringBuilder();
				b.Append(baseUrl);
				if (b[b.Length - 1] != '/')
				{
					b.Append('/');
				}
				b.Append(Constants.INFO_REFS);
				if (useSmartHttp)
				{
					b.Append(b.IndexOf("?") < 0 ? '?' : '&');
					b.Append("service=");
					b.Append(service);
				}
				u = new Uri(b.ToString());
			}
			catch (UriFormatException e)
			{
				throw new NotSupportedException(MessageFormat.Format(JGitText.Get().invalidURL, uri
					), e);
			}
			try
			{
				HttpURLConnection conn = HttpOpen(u);
				if (useSmartHttp)
				{
					string expType = "application/x-" + service + "-advertisement";
					conn.SetRequestProperty(HttpSupport.HDR_ACCEPT, expType + ", */*");
				}
				else
				{
					conn.SetRequestProperty(HttpSupport.HDR_ACCEPT, "*/*");
				}
				int status = HttpSupport.Response(conn);
				switch (status)
				{
					case HttpURLConnection.HTTP_OK:
					{
						return conn;
					}

					case HttpURLConnection.HTTP_NOT_FOUND:
					{
						throw new NoRemoteRepositoryException(uri, MessageFormat.Format(JGitText.Get().URLNotFound
							, u));
					}

					case HttpURLConnection.HTTP_FORBIDDEN:
					{
						throw new TransportException(uri, MessageFormat.Format(JGitText.Get().serviceNotPermitted
							, service));
					}

					default:
					{
						string err = status + " " + conn.GetResponseMessage();
						throw new TransportException(uri, err);
					}
				}
			}
			catch (NotSupportedException e)
			{
				throw;
			}
			catch (TransportException e)
			{
				throw;
			}
			catch (IOException e)
			{
				throw new TransportException(uri, MessageFormat.Format(JGitText.Get().cannotOpenService
					, service), e);
			}
		}

		/// <exception cref="System.IO.IOException"></exception>
		internal HttpURLConnection HttpOpen(Uri u)
		{
			Proxy proxy = HttpSupport.ProxyFor(proxySelector, u);
			HttpURLConnection conn = (HttpURLConnection)u.OpenConnection(proxy);
			conn.SetUseCaches(false);
			conn.SetRequestProperty(HttpSupport.HDR_ACCEPT_ENCODING, HttpSupport.ENCODING_GZIP
				);
			conn.SetRequestProperty(HttpSupport.HDR_PRAGMA, "no-cache");
			//$NON-NLS-1$
			conn.SetRequestProperty(HttpSupport.HDR_USER_AGENT, userAgent);
			conn.SetConnectTimeout(GetTimeout() * 1000);
			conn.SetReadTimeout(GetTimeout() * 1000);
			return conn;
		}

		/// <exception cref="System.IO.IOException"></exception>
		internal InputStream OpenInputStream(HttpURLConnection conn)
		{
			InputStream input = conn.GetInputStream();
			if (HttpSupport.ENCODING_GZIP.Equals(conn.GetHeaderField(HttpSupport.HDR_CONTENT_ENCODING
				)))
			{
				input = new GZIPInputStream(input);
			}
			return input;
		}

		internal virtual IOException WrongContentType(string expType, string actType)
		{
			string why = MessageFormat.Format(JGitText.Get().expectedReceivedContentType, expType
				, actType);
			return new TransportException(uri, why);
		}

		private bool IsSmartHttp(HttpURLConnection c, string service)
		{
			string expType = "application/x-" + service + "-advertisement";
			string actType = c.GetContentType();
			return expType.Equals(actType);
		}

		/// <exception cref="System.IO.IOException"></exception>
		private void ReadSmartHeaders(InputStream @in, string service)
		{
			// A smart reply will have a '#' after the first 4 bytes, but
			// a dumb reply cannot contain a '#' until after byte 41. Do a
			// quick check to make sure its a smart reply before we parse
			// as a pkt-line stream.
			//
			byte[] magic = new byte[5];
			IOUtil.ReadFully(@in, magic, 0, magic.Length);
			if (magic[4] != '#')
			{
				throw new TransportException(uri, MessageFormat.Format(JGitText.Get().expectedPktLineWithService
					, RawParseUtils.Decode(magic)));
			}
			PacketLineIn pckIn = new PacketLineIn(new UnionInputStream(new ByteArrayInputStream
				(magic), @in));
			string exp = "# service=" + service;
			string act = pckIn.ReadString();
			if (!exp.Equals(act))
			{
				throw new TransportException(uri, MessageFormat.Format(JGitText.Get().expectedGot
					, exp, act));
			}
			while (pckIn.ReadString() != PacketLineIn.END)
			{
			}
		}

		internal class HttpObjectDB : WalkRemoteObjectDatabase
		{
			private readonly Uri objectsUrl;

			internal HttpObjectDB(TransportHttp _enclosing, Uri b)
			{
				this._enclosing = _enclosing;
				// for now, ignore the remaining header lines
				this.objectsUrl = b;
			}

			internal override URIish GetURI()
			{
				return new URIish(this.objectsUrl);
			}

			/// <exception cref="System.IO.IOException"></exception>
			internal override ICollection<WalkRemoteObjectDatabase> GetAlternates()
			{
				try
				{
					return this.ReadAlternates(WalkRemoteObjectDatabase.INFO_HTTP_ALTERNATES);
				}
				catch (FileNotFoundException)
				{
				}
				// Fall through.
				try
				{
					return this.ReadAlternates(WalkRemoteObjectDatabase.INFO_ALTERNATES);
				}
				catch (FileNotFoundException)
				{
				}
				// Fall through.
				return null;
			}

			/// <exception cref="System.IO.IOException"></exception>
			internal override WalkRemoteObjectDatabase OpenAlternate(string location)
			{
				return new TransportHttp.HttpObjectDB(_enclosing, new Uri(this.objectsUrl, location));
			}

			/// <exception cref="System.IO.IOException"></exception>
			internal override ICollection<string> GetPackNames()
			{
				ICollection<string> packs = new AList<string>();
				try
				{
					BufferedReader br = this.OpenReader(WalkRemoteObjectDatabase.INFO_PACKS);
					try
					{
						for (; ; )
						{
							string s = br.ReadLine();
							if (s == null || s.Length == 0)
							{
								break;
							}
							if (!s.StartsWith("P pack-") || !s.EndsWith(".pack"))
							{
								throw this.InvalidAdvertisement(s);
							}
							packs.AddItem(Sharpen.Runtime.Substring(s, 2));
						}
						return packs;
					}
					finally
					{
						br.Close();
					}
				}
				catch (FileNotFoundException)
				{
					return packs;
				}
			}

			/// <exception cref="System.IO.IOException"></exception>
			internal override WalkRemoteObjectDatabase.FileStream Open(string path)
			{
				Uri @base = this.objectsUrl;
				Uri u = new Uri(@base, path);
				HttpURLConnection c = this._enclosing.HttpOpen(u);
				switch (HttpSupport.Response(c))
				{
					case HttpURLConnection.HTTP_OK:
					{
						InputStream @in = this._enclosing.OpenInputStream(c);
						int len = c.GetContentLength();
						return new WalkRemoteObjectDatabase.FileStream(@in, len);
					}

					case HttpURLConnection.HTTP_NOT_FOUND:
					{
						throw new FileNotFoundException(u.ToString());
					}

					default:
					{
						throw new IOException(u.ToString() + ": " + HttpSupport.Response(c) + " " + c.GetResponseMessage
							());
					}
				}
			}

			/// <exception cref="System.IO.IOException"></exception>
			/// <exception cref="NGit.Errors.PackProtocolException"></exception>
			internal virtual IDictionary<string, Ref> ReadAdvertisedImpl(BufferedReader br)
			{
				SortedDictionary<string, Ref> avail = new SortedDictionary<string, Ref>();
				for (; ; )
				{
					string line = br.ReadLine();
					if (line == null)
					{
						break;
					}
					int tab = line.IndexOf('\t');
					if (tab < 0)
					{
						throw this.InvalidAdvertisement(line);
					}
					string name;
					ObjectId id;
					name = Sharpen.Runtime.Substring(line, tab + 1);
					id = ObjectId.FromString(Sharpen.Runtime.Substring(line, 0, tab));
					if (name.EndsWith("^{}"))
					{
						name = Sharpen.Runtime.Substring(name, 0, name.Length - 3);
						Ref prior = avail.Get(name);
						if (prior == null)
						{
							throw this.OutOfOrderAdvertisement(name);
						}
						if (prior.GetPeeledObjectId() != null)
						{
							throw this.DuplicateAdvertisement(name + "^{}");
						}
						avail.Put(name, new ObjectIdRef.PeeledTag(RefStorage.NETWORK, name, prior.GetObjectId
							(), id));
					}
					else
					{
						Ref prior = avail.Put(name, new ObjectIdRef.PeeledNonTag(RefStorage.NETWORK, name
							, id));
						if (prior != null)
						{
							throw this.DuplicateAdvertisement(name);
						}
					}
				}
				return avail;
			}

			private PackProtocolException OutOfOrderAdvertisement(string n)
			{
				return new PackProtocolException(MessageFormat.Format(JGitText.Get().advertisementOfCameBefore
					, n, n));
			}

			private PackProtocolException InvalidAdvertisement(string n)
			{
				return new PackProtocolException(MessageFormat.Format(JGitText.Get().invalidAdvertisementOf
					, n));
			}

			private PackProtocolException DuplicateAdvertisement(string n)
			{
				return new PackProtocolException(MessageFormat.Format(JGitText.Get().duplicateAdvertisementsOf
					, n));
			}

			internal override void Close()
			{
			}

			private readonly TransportHttp _enclosing;
			// We do not maintain persistent connections.
		}

		internal class SmartHttpFetchConnection : BasePackFetchConnection
		{
			/// <exception cref="NGit.Errors.TransportException"></exception>
			internal SmartHttpFetchConnection(TransportHttp _enclosing, InputStream advertisement
				) : base(_enclosing)
			{
				this._enclosing = _enclosing;
				this.statelessRPC = true;
				this.Init(advertisement, DisabledOutputStream.INSTANCE);
				this.outNeedsEnd = false;
				this.ReadAdvertisedRefs();
			}

			/// <exception cref="NGit.Errors.TransportException"></exception>
			protected internal override void DoFetch(ProgressMonitor monitor, ICollection<Ref
				> want, ICollection<ObjectId> have)
			{
				TransportHttp.Service svc = new TransportHttp.Service(_enclosing, TransportHttp.SVC_UPLOAD_PACK
					);
				this.Init(svc.@in, svc.@out);
				base.DoFetch(monitor, want, have);
			}

			private readonly TransportHttp _enclosing;
		}

		internal class SmartHttpPushConnection : BasePackPushConnection
		{
			/// <exception cref="NGit.Errors.TransportException"></exception>
			internal SmartHttpPushConnection(TransportHttp _enclosing, InputStream advertisement
				) : base(_enclosing)
			{
				this._enclosing = _enclosing;
				this.statelessRPC = true;
				this.Init(advertisement, DisabledOutputStream.INSTANCE);
				this.outNeedsEnd = false;
				this.ReadAdvertisedRefs();
			}

			/// <exception cref="NGit.Errors.TransportException"></exception>
			protected internal override void DoPush(ProgressMonitor monitor, IDictionary<string
				, RemoteRefUpdate> refUpdates)
			{
				TransportHttp.Service svc = new TransportHttp.Service(_enclosing, TransportHttp.SVC_RECEIVE_PACK
					);
				this.Init(svc.@in, svc.@out);
				base.DoPush(monitor, refUpdates);
			}

			private readonly TransportHttp _enclosing;
		}

		/// <summary>State required to speak multiple HTTP requests with the remote.</summary>
		/// <remarks>
		/// State required to speak multiple HTTP requests with the remote.
		/// <p>
		/// A service wrapper provides a normal looking InputStream and OutputStream
		/// pair which are connected via HTTP to the named remote service. Writing to
		/// the OutputStream is buffered until either the buffer overflows, or
		/// reading from the InputStream occurs. If overflow occurs HTTP/1.1 and its
		/// chunked transfer encoding is used to stream the request data to the
		/// remote service. If the entire request fits in the memory buffer, the
		/// older HTTP/1.0 standard and a fixed content length is used instead.
		/// <p>
		/// It is an error to attempt to read without there being outstanding data
		/// ready for transmission on the OutputStream.
		/// <p>
		/// No state is preserved between write-read request pairs. The caller is
		/// responsible for replaying state vector information as part of the request
		/// data written to the OutputStream. Any session HTTP cookies may or may not
		/// be preserved between requests, it is left up to the JVM's implementation
		/// of the HTTP client.
		/// </remarks>
		internal class Service
		{
			private readonly string serviceName;

			private readonly string requestType;

			private readonly string responseType;

			private readonly TransportHttp.Service.HttpExecuteStream execute;

			internal readonly UnionInputStream @in;

			internal readonly TransportHttp.Service.HttpOutputStream @out;

			internal HttpURLConnection conn;

			internal Service(TransportHttp _enclosing, string serviceName)
			{
				this._enclosing = _enclosing;
				this.serviceName = serviceName;
				this.requestType = "application/x-" + serviceName + "-request";
				this.responseType = "application/x-" + serviceName + "-result";
				this.execute = new TransportHttp.Service.HttpExecuteStream(this);
				this.@in = new UnionInputStream(this.execute);
				this.@out = new TransportHttp.Service.HttpOutputStream(this);
			}

			/// <exception cref="System.IO.IOException"></exception>
			internal virtual void OpenStream()
			{
				this.conn = this._enclosing.HttpOpen(new Uri(this._enclosing.baseUrl, this.serviceName
					));
				this.conn.SetRequestMethod(HttpSupport.METHOD_POST);
				this.conn.SetInstanceFollowRedirects(false);
				this.conn.SetDoOutput(true);
				this.conn.SetRequestProperty(HttpSupport.HDR_CONTENT_TYPE, this.requestType);
				this.conn.SetRequestProperty(HttpSupport.HDR_ACCEPT, this.responseType);
			}

			/// <exception cref="System.IO.IOException"></exception>
			internal virtual void Execute()
			{
				this.@out.Close();
				if (this.conn == null)
				{
					// Output hasn't started yet, because everything fit into
					// our request buffer. Send with a Content-Length header.
					//
					if (this.@out.Length() == 0)
					{
						throw new TransportException(this._enclosing.uri, JGitText.Get().startingReadStageWithoutWrittenRequestDataPendingIsNotSupported
							);
					}
					// Try to compress the content, but only if that is smaller.
					TemporaryBuffer buf = new TemporaryBuffer.Heap(this._enclosing.http.postBuffer);
					try
					{
						GZIPOutputStream gzip = new GZIPOutputStream(buf);
						this.@out.WriteTo(gzip, null);
						gzip.Close();
						if (this.@out.Length() < buf.Length())
						{
							buf = this.@out;
						}
					}
					catch (IOException)
					{
						// Most likely caused by overflowing the buffer, meaning
						// its larger if it were compressed. Don't compress.
						buf = this.@out;
					}
					this.OpenStream();
					if (buf != this.@out)
					{
						this.conn.SetRequestProperty(HttpSupport.HDR_CONTENT_ENCODING, HttpSupport.ENCODING_GZIP
							);
					}
					this.conn.SetFixedLengthStreamingMode((int)buf.Length());
					OutputStream httpOut = this.conn.GetOutputStream();
					try
					{
						buf.WriteTo(httpOut, null);
					}
					finally
					{
						httpOut.Close();
					}
				}
				this.@out.Reset();
				int status = HttpSupport.Response(this.conn);
				if (status != HttpURLConnection.HTTP_OK)
				{
					throw new TransportException(this._enclosing.uri, status + " " + this.conn.GetResponseMessage
						());
				}
				string contentType = this.conn.GetContentType();
				if (!this.responseType.Equals(contentType))
				{
					this.conn.GetInputStream().Close();
					throw this._enclosing.WrongContentType(this.responseType, contentType);
				}
				this.@in.Add(this._enclosing.OpenInputStream(this.conn));
				this.@in.Add(this.execute);
				this.conn = null;
			}

			internal class HttpOutputStream : TemporaryBuffer
			{
				public HttpOutputStream(Service _enclosing) : base(_enclosing._enclosing.http
					.postBuffer)
				{
					this._enclosing = _enclosing;
				}

				/// <exception cref="System.IO.IOException"></exception>
				protected internal override OutputStream Overflow()
				{
					this._enclosing.OpenStream();
					this._enclosing.conn.SetChunkedStreamingMode(0);
					return this._enclosing.conn.GetOutputStream();
				}

				private readonly Service _enclosing;
			}

			internal class HttpExecuteStream : InputStream
			{
				/// <exception cref="System.IO.IOException"></exception>
				public override int Available()
				{
					this._enclosing.Execute();
					return 0;
				}

				/// <exception cref="System.IO.IOException"></exception>
				public override int Read()
				{
					this._enclosing.Execute();
					return -1;
				}

				/// <exception cref="System.IO.IOException"></exception>
				public override int Read(byte[] b, int off, int len)
				{
					this._enclosing.Execute();
					return -1;
				}

				/// <exception cref="System.IO.IOException"></exception>
				public override long Skip(long n)
				{
					this._enclosing.Execute();
					return 0;
				}

				internal HttpExecuteStream(Service _enclosing)
				{
					this._enclosing = _enclosing;
				}

				private readonly Service _enclosing;
			}

			private readonly TransportHttp _enclosing;
		}
	}
}
