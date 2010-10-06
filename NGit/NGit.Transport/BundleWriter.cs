using System;
using System.Collections.Generic;
using System.IO;
using NGit;
using NGit.Revwalk;
using NGit.Storage.Pack;
using Sharpen;

namespace NGit.Transport
{
	/// <summary>Creates a Git bundle file, for sneaker-net transport to another system.</summary>
	/// <remarks>
	/// Creates a Git bundle file, for sneaker-net transport to another system.
	/// <p>
	/// Bundles generated by this class can be later read in from a file URI using
	/// the bundle transport, or from an application controlled buffer by the more
	/// generic
	/// <see cref="TransportBundleStream">TransportBundleStream</see>
	/// .
	/// <p>
	/// Applications creating bundles need to call one or more <code>include</code>
	/// calls to reflect which objects should be available as refs in the bundle for
	/// the other side to fetch. At least one include is required to create a valid
	/// bundle file, and duplicate names are not permitted.
	/// <p>
	/// Optional <code>assume</code> calls can be made to declare commits which the
	/// recipient must have in order to fetch from the bundle file. Objects reachable
	/// from these assumed commits can be used as delta bases in order to reduce the
	/// overall bundle size.
	/// </remarks>
	public class BundleWriter
	{
		private readonly Repository db;

		private readonly IDictionary<string, ObjectId> include;

		private readonly ICollection<RevCommit> assume;

		private PackConfig packConfig;

		/// <summary>Create a writer for a bundle.</summary>
		/// <remarks>Create a writer for a bundle.</remarks>
		/// <param name="repo">repository where objects are stored.</param>
		public BundleWriter(Repository repo)
		{
			db = repo;
			include = new SortedDictionary<string, ObjectId>();
			assume = new HashSet<RevCommit>();
		}

		/// <summary>Set the configuration used by the pack generator.</summary>
		/// <remarks>Set the configuration used by the pack generator.</remarks>
		/// <param name="pc">
		/// configuration controlling packing parameters. If null the
		/// source repository's settings will be used.
		/// </param>
		public virtual void SetPackConfig(PackConfig pc)
		{
			this.packConfig = pc;
		}

		/// <summary>Include an object (and everything reachable from it) in the bundle.</summary>
		/// <remarks>Include an object (and everything reachable from it) in the bundle.</remarks>
		/// <param name="name">
		/// name the recipient can discover this object as from the
		/// bundle's list of advertised refs . The name must be a valid
		/// ref format and must not have already been included in this
		/// bundle writer.
		/// </param>
		/// <param name="id">object to pack. Multiple refs may point to the same object.</param>
		public virtual void Include(string name, AnyObjectId id)
		{
			if (!Repository.IsValidRefName(name))
			{
				throw new ArgumentException(MessageFormat.Format(JGitText.Get().invalidRefName, name
					));
			}
			if (include.ContainsKey(name))
			{
				throw new InvalidOperationException(JGitText.Get().duplicateRef + name);
			}
			include.Put(name, id.ToObjectId());
		}

		/// <summary>Include a single ref (a name/object pair) in the bundle.</summary>
		/// <remarks>
		/// Include a single ref (a name/object pair) in the bundle.
		/// <p>
		/// This is a utility function for:
		/// <code>include(r.getName(), r.getObjectId())</code>.
		/// </remarks>
		/// <param name="r">the ref to include.</param>
		public virtual void Include(Ref r)
		{
			Include(r.GetName(), r.GetObjectId());
		}

		/// <summary>Assume a commit is available on the recipient's side.</summary>
		/// <remarks>
		/// Assume a commit is available on the recipient's side.
		/// <p>
		/// In order to fetch from a bundle the recipient must have any assumed
		/// commit. Each assumed commit is explicitly recorded in the bundle header
		/// to permit the recipient to validate it has these objects.
		/// </remarks>
		/// <param name="c">
		/// the commit to assume being available. This commit should be
		/// parsed and not disposed in order to maximize the amount of
		/// debugging information available in the bundle stream.
		/// </param>
		public virtual void Assume(RevCommit c)
		{
			if (c != null)
			{
				assume.AddItem(c);
			}
		}

		/// <summary>Generate and write the bundle to the output stream.</summary>
		/// <remarks>
		/// Generate and write the bundle to the output stream.
		/// <p>
		/// This method can only be called once per BundleWriter instance.
		/// </remarks>
		/// <param name="monitor">progress monitor to report bundle writing status to.</param>
		/// <param name="os">
		/// the stream the bundle is written to. The stream should be
		/// buffered by the caller. The caller is responsible for closing
		/// the stream.
		/// </param>
		/// <exception cref="System.IO.IOException">
		/// an error occurred reading a local object's data to include in
		/// the bundle, or writing compressed object data to the output
		/// stream.
		/// </exception>
		public virtual void WriteBundle(ProgressMonitor monitor, OutputStream os)
		{
			PackConfig pc = packConfig;
			if (pc == null)
			{
				pc = new PackConfig(db);
			}
			PackWriter packWriter = new PackWriter(pc, db.NewObjectReader());
			try
			{
				HashSet<ObjectId> inc = new HashSet<ObjectId>();
				HashSet<ObjectId> exc = new HashSet<ObjectId>();
				Sharpen.Collections.AddAll(inc, include.Values);
				foreach (RevCommit r in assume)
				{
					exc.AddItem(r.Id);
				}
				packWriter.SetThin(exc.Count > 0);
				packWriter.PreparePack(monitor, inc, exc);
				TextWriter w = new OutputStreamWriter(os, Constants.CHARSET);
				w.Write(NGit.Transport.TransportBundleConstants.V2_BUNDLE_SIGNATURE);
				w.Write('\n');
				char[] tmp = new char[Constants.OBJECT_ID_STRING_LENGTH];
				foreach (RevCommit a in assume)
				{
					w.Write('-');
					a.CopyTo(tmp, w);
					if (a.RawBuffer != null)
					{
						w.Write(' ');
						w.Write(a.GetShortMessage());
					}
					w.Write('\n');
				}
				foreach (KeyValuePair<string, ObjectId> e in include.EntrySet())
				{
					e.Value.CopyTo(tmp, w);
					w.Write(' ');
					w.Write(e.Key);
					w.Write('\n');
				}
				w.Write('\n');
				w.Flush();
				packWriter.WritePack(monitor, monitor, os);
			}
			finally
			{
				packWriter.Release();
			}
		}
	}
}
