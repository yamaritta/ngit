using System.Collections.Generic;
using System.IO;
using NGit;
using NGit.Storage.File;
using Sharpen;

namespace NGit.Storage.File
{
	public class ReflogReaderTest : SampleDataRepositoryTestCase
	{
		internal static byte[] oneLine = Sharpen.Runtime.GetBytesForString("da85355dfc525c9f6f3927b876f379f46ccf826e 3e7549db262d1e836d9bf0af7e22355468f1717c A O Thor Too <authortoo@wri.tr> 1243028200 +0200\tcommit: Add a toString for debugging to RemoteRefUpdate\n"
			);

		internal static byte[] twoLine = Sharpen.Runtime.GetBytesForString(("0000000000000000000000000000000000000000 c6734895958052a9dbc396cff4459dc1a25029ab A U Thor <thor@committer.au> 1243028201 -0100\tbranch: Created from rr/renamebranchv4\n"
			 + "c6734895958052a9dbc396cff4459dc1a25029ab 54794942a18a237c57a80719afed44bb78172b10 Same A U Thor <same.author@example.com> 1243028202 +0100\trebase finished: refs/heads/rr/renamebranch5 onto c6e3b9fe2da0293f11eae202ec35fb343191a82d\n"
			));

		internal static byte[] twoLineWithAppendInProgress = Sharpen.Runtime.GetBytesForString
			(("0000000000000000000000000000000000000000 c6734895958052a9dbc396cff4459dc1a25029ab A U Thor <thor@committer.au> 1243028201 -0100\tbranch: Created from rr/renamebranchv4\n"
			 + "c6734895958052a9dbc396cff4459dc1a25029ab 54794942a18a237c57a80719afed44bb78172b10 Same A U Thor <same.author@example.com> 1243028202 +0100\trebase finished: refs/heads/rr/renamebranch5 onto c6e3b9fe2da0293f11eae202ec35fb343191a82d\n"
			 + "54794942a18a237c57a80719afed44bb78172b10 "));

		internal static byte[] aLine = Sharpen.Runtime.GetBytesForString("1111111111111111111111111111111111111111 3e7549db262d1e836d9bf0af7e22355468f1717c A U Thor <thor@committer.au> 1243028201 -0100\tbranch: change to a\n"
			);

		internal static byte[] masterLine = Sharpen.Runtime.GetBytesForString("2222222222222222222222222222222222222222 3e7549db262d1e836d9bf0af7e22355468f1717c A U Thor <thor@committer.au> 1243028201 -0100\tbranch: change to master\n"
			);

		internal static byte[] headLine = Sharpen.Runtime.GetBytesForString("3333333333333333333333333333333333333333 3e7549db262d1e836d9bf0af7e22355468f1717c A U Thor <thor@committer.au> 1243028201 -0100\tbranch: change to HEAD\n"
			);

		/// <exception cref="System.Exception"></exception>
		public virtual void TestReadOneLine()
		{
			SetupReflog("logs/refs/heads/master", oneLine);
			ReflogReader reader = new ReflogReader(db, "refs/heads/master");
			ReflogReader.Entry e = reader.GetLastEntry();
			AssertEquals(ObjectId.FromString("da85355dfc525c9f6f3927b876f379f46ccf826e"), e.GetOldId
				());
			AssertEquals(ObjectId.FromString("3e7549db262d1e836d9bf0af7e22355468f1717c"), e.GetNewId
				());
			NUnit.Framework.Assert.AreEqual("A O Thor Too", e.GetWho().GetName());
			NUnit.Framework.Assert.AreEqual("authortoo@wri.tr", e.GetWho().GetEmailAddress());
			NUnit.Framework.Assert.AreEqual(120, e.GetWho().GetTimeZoneOffset());
			NUnit.Framework.Assert.AreEqual("2009-05-22T23:36:40", Iso(e.GetWho()));
			NUnit.Framework.Assert.AreEqual("commit: Add a toString for debugging to RemoteRefUpdate"
				, e.GetComment());
		}

		private string Iso(PersonIdent id)
		{
			SimpleDateFormat fmt;
			fmt = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss");
			fmt.SetTimeZone(id.GetTimeZone());
			return fmt.Format(id.GetWhen());
		}

		/// <exception cref="System.Exception"></exception>
		public virtual void TestReadTwoLine()
		{
			SetupReflog("logs/refs/heads/master", twoLine);
			ReflogReader reader = new ReflogReader(db, "refs/heads/master");
			IList<ReflogReader.Entry> reverseEntries = reader.GetReverseEntries();
			NUnit.Framework.Assert.AreEqual(2, reverseEntries.Count);
			ReflogReader.Entry e = reverseEntries[0];
			AssertEquals(ObjectId.FromString("c6734895958052a9dbc396cff4459dc1a25029ab"), e.GetOldId
				());
			AssertEquals(ObjectId.FromString("54794942a18a237c57a80719afed44bb78172b10"), e.GetNewId
				());
			NUnit.Framework.Assert.AreEqual("Same A U Thor", e.GetWho().GetName());
			NUnit.Framework.Assert.AreEqual("same.author@example.com", e.GetWho().GetEmailAddress
				());
			NUnit.Framework.Assert.AreEqual(60, e.GetWho().GetTimeZoneOffset());
			NUnit.Framework.Assert.AreEqual("2009-05-22T22:36:42", Iso(e.GetWho()));
			NUnit.Framework.Assert.AreEqual("rebase finished: refs/heads/rr/renamebranch5 onto c6e3b9fe2da0293f11eae202ec35fb343191a82d"
				, e.GetComment());
			e = reverseEntries[1];
			AssertEquals(ObjectId.FromString("0000000000000000000000000000000000000000"), e.GetOldId
				());
			AssertEquals(ObjectId.FromString("c6734895958052a9dbc396cff4459dc1a25029ab"), e.GetNewId
				());
			NUnit.Framework.Assert.AreEqual("A U Thor", e.GetWho().GetName());
			NUnit.Framework.Assert.AreEqual("thor@committer.au", e.GetWho().GetEmailAddress()
				);
			NUnit.Framework.Assert.AreEqual(-60, e.GetWho().GetTimeZoneOffset());
			NUnit.Framework.Assert.AreEqual("2009-05-22T20:36:41", Iso(e.GetWho()));
			NUnit.Framework.Assert.AreEqual("branch: Created from rr/renamebranchv4", e.GetComment
				());
		}

		/// <exception cref="System.Exception"></exception>
		public virtual void TestReadWhileAppendIsInProgress()
		{
			SetupReflog("logs/refs/heads/master", twoLineWithAppendInProgress);
			ReflogReader reader = new ReflogReader(db, "refs/heads/master");
			IList<ReflogReader.Entry> reverseEntries = reader.GetReverseEntries();
			NUnit.Framework.Assert.AreEqual(2, reverseEntries.Count);
			ReflogReader.Entry e = reverseEntries[0];
			AssertEquals(ObjectId.FromString("c6734895958052a9dbc396cff4459dc1a25029ab"), e.GetOldId
				());
			AssertEquals(ObjectId.FromString("54794942a18a237c57a80719afed44bb78172b10"), e.GetNewId
				());
			NUnit.Framework.Assert.AreEqual("Same A U Thor", e.GetWho().GetName());
			NUnit.Framework.Assert.AreEqual("same.author@example.com", e.GetWho().GetEmailAddress
				());
			NUnit.Framework.Assert.AreEqual(60, e.GetWho().GetTimeZoneOffset());
			NUnit.Framework.Assert.AreEqual("2009-05-22T22:36:42", Iso(e.GetWho()));
			NUnit.Framework.Assert.AreEqual("rebase finished: refs/heads/rr/renamebranch5 onto c6e3b9fe2da0293f11eae202ec35fb343191a82d"
				, e.GetComment());
		}

		// while similar to testReadTwoLine, we can assume that if we get the last entry
		// right, everything else is too
		/// <exception cref="System.Exception"></exception>
		public virtual void TestReadRightLog()
		{
			SetupReflog("logs/refs/heads/a", aLine);
			SetupReflog("logs/refs/heads/master", masterLine);
			SetupReflog("logs/HEAD", headLine);
			NUnit.Framework.Assert.AreEqual("branch: change to master", db.GetReflogReader("master"
				).GetLastEntry().GetComment());
			NUnit.Framework.Assert.AreEqual("branch: change to a", db.GetReflogReader("a").GetLastEntry
				().GetComment());
			NUnit.Framework.Assert.AreEqual("branch: change to HEAD", db.GetReflogReader("HEAD"
				).GetLastEntry().GetComment());
		}

		/// <exception cref="System.Exception"></exception>
		public virtual void TestNoLog()
		{
			NUnit.Framework.Assert.AreEqual(0, db.GetReflogReader("master").GetReverseEntries
				().Count);
			NUnit.Framework.Assert.IsNull(db.GetReflogReader("master").GetLastEntry());
		}

		/// <exception cref="System.IO.FileNotFoundException"></exception>
		/// <exception cref="System.IO.IOException"></exception>
		private void SetupReflog(string logName, byte[] data)
		{
			FilePath logfile = new FilePath(db.Directory, logName);
			if (!logfile.GetParentFile().Mkdirs() && !logfile.GetParentFile().IsDirectory())
			{
				throw new IOException("oops, cannot create the directory for the test reflog file"
					 + logfile);
			}
			FileOutputStream fileOutputStream = new FileOutputStream(logfile);
			try
			{
				fileOutputStream.Write(data);
			}
			finally
			{
				fileOutputStream.Close();
			}
		}
	}
}
