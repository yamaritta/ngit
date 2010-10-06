using System.IO;
using NGit;
using NGit.Errors;
using NGit.Storage.File;
using NGit.Util;
using Sharpen;

namespace NGit.Storage.File
{
	/// <summary>The configuration file that is stored in the file of the file system.</summary>
	/// <remarks>The configuration file that is stored in the file of the file system.</remarks>
	public class FileBasedConfig : StoredConfig
	{
		private readonly FilePath configFile;

		private long lastModified;

		private readonly FS fs;

		/// <summary>Create a configuration with no default fallback.</summary>
		/// <remarks>Create a configuration with no default fallback.</remarks>
		/// <param name="cfgLocation">the location of the configuration file on the file system
		/// 	</param>
		/// <param name="fs">
		/// the file system abstraction which will be necessary to perform
		/// certain file system operations.
		/// </param>
		public FileBasedConfig(FilePath cfgLocation, FS fs) : this(null, cfgLocation, fs)
		{
		}

		/// <summary>The constructor</summary>
		/// <param name="base">the base configuration file</param>
		/// <param name="cfgLocation">the location of the configuration file on the file system
		/// 	</param>
		/// <param name="fs">
		/// the file system abstraction which will be necessary to perform
		/// certain file system operations.
		/// </param>
		public FileBasedConfig(Config @base, FilePath cfgLocation, FS fs) : base(@base)
		{
			configFile = cfgLocation;
			this.fs = fs;
		}

		protected internal override bool NotifyUponTransientChanges()
		{
			// we will notify listeners upon save()
			return false;
		}

		/// <returns>location of the configuration file on disk</returns>
		public FilePath GetFile()
		{
			return configFile;
		}

		/// <summary>Load the configuration as a Git text style configuration file.</summary>
		/// <remarks>
		/// Load the configuration as a Git text style configuration file.
		/// <p>
		/// If the file does not exist, this configuration is cleared, and thus
		/// behaves the same as though the file exists, but is empty.
		/// </remarks>
		/// <exception cref="System.IO.IOException">the file could not be read (but does exist).
		/// 	</exception>
		/// <exception cref="NGit.Errors.ConfigInvalidException">the file is not a properly formatted configuration file.
		/// 	</exception>
		public override void Load()
		{
			lastModified = GetFile().LastModified();
			try
			{
				FromText(RawParseUtils.Decode(IOUtil.ReadFully(GetFile())));
			}
			catch (FileNotFoundException)
			{
				Clear();
			}
			catch (IOException e)
			{
				IOException e2 = new IOException(MessageFormat.Format(JGitText.Get().cannotReadFile
					, GetFile()));
				Sharpen.Extensions.InitCause(e2, e);
				throw e2;
			}
			catch (ConfigInvalidException e)
			{
				throw new ConfigInvalidException(MessageFormat.Format(JGitText.Get().cannotReadFile
					, GetFile()), e);
			}
		}

		/// <summary>Save the configuration as a Git text style configuration file.</summary>
		/// <remarks>
		/// Save the configuration as a Git text style configuration file.
		/// <p>
		/// <b>Warning:</b> Although this method uses the traditional Git file
		/// locking approach to protect against concurrent writes of the
		/// configuration file, it does not ensure that the file has not been
		/// modified since the last read, which means updates performed by other
		/// objects accessing the same backing file may be lost.
		/// </remarks>
		/// <exception cref="System.IO.IOException">the file could not be written.</exception>
		public override void Save()
		{
			byte[] @out = Constants.Encode(ToText());
			LockFile lf = new LockFile(GetFile(), fs);
			if (!lf.Lock())
			{
				throw new IOException(MessageFormat.Format(JGitText.Get().cannotLockFile, GetFile
					()));
			}
			try
			{
				lf.SetNeedStatInformation(true);
				lf.Write(@out);
				if (!lf.Commit())
				{
					throw new IOException(MessageFormat.Format(JGitText.Get().cannotCommitWriteTo, GetFile
						()));
				}
			}
			finally
			{
				lf.Unlock();
			}
			lastModified = lf.GetCommitLastModified();
			// notify the listeners
			FireConfigChangedEvent();
		}

		public override string ToString()
		{
			return GetType().Name + "[" + GetFile().GetPath() + "]";
		}

		/// <returns>
		/// returns true if the currently loaded configuration file is older
		/// than the file on disk
		/// </returns>
		public virtual bool IsOutdated()
		{
			return GetFile().LastModified() != lastModified;
		}
	}
}
