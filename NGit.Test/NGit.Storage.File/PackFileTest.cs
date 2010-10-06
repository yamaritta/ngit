using ICSharpCode.SharpZipLib.Zip.Compression;
using NGit;
using NGit.Errors;
using NGit.Junit;
using NGit.Revwalk;
using NGit.Storage.File;
using NGit.Storage.Pack;
using NGit.Transport;
using NGit.Util;
using Sharpen;

namespace NGit.Storage.File
{
	public class PackFileTest : LocalDiskRepositoryTestCase
	{
		private int streamThreshold = 16 * 1024;

		private TestRng rng;

		private FileRepository repo;

		private TestRepository<FileRepository> tr;

		private WindowCursor wc;

		/// <exception cref="System.Exception"></exception>
		protected override void SetUp()
		{
			base.SetUp();
			WindowCacheConfig cfg = new WindowCacheConfig();
			cfg.SetStreamFileThreshold(streamThreshold);
			WindowCache.Reconfigure(cfg);
			rng = new TestRng(Sharpen.Extensions.GetTestName(this));
			repo = CreateBareRepository();
			tr = new TestRepository<FileRepository>(repo);
			wc = (WindowCursor)repo.NewObjectReader();
		}

		/// <exception cref="System.Exception"></exception>
		protected override void TearDown()
		{
			if (wc != null)
			{
				wc.Release();
			}
			WindowCache.Reconfigure(new WindowCacheConfig());
			base.TearDown();
		}

		/// <exception cref="System.Exception"></exception>
		public virtual void TestWhole_SmallObject()
		{
			int type = Constants.OBJ_BLOB;
			byte[] data = rng.NextBytes(300);
			RevBlob id = tr.Blob(data);
			tr.Branch("master").Commit().Add("A", id).Create();
			tr.PackAndPrune();
			NUnit.Framework.Assert.IsTrue("has blob", wc.Has(id));
			ObjectLoader ol = wc.Open(id);
			NUnit.Framework.Assert.IsNotNull("created loader", ol);
			NUnit.Framework.Assert.AreEqual(type, ol.GetType());
			NUnit.Framework.Assert.AreEqual(data.Length, ol.GetSize());
			NUnit.Framework.Assert.IsFalse("is not large", ol.IsLarge());
			NUnit.Framework.Assert.IsTrue("same content", Arrays.Equals(data, ol.GetCachedBytes
				()));
			ObjectStream @in = ol.OpenStream();
			NUnit.Framework.Assert.IsNotNull("have stream", @in);
			NUnit.Framework.Assert.AreEqual(type, @in.GetType());
			NUnit.Framework.Assert.AreEqual(data.Length, @in.GetSize());
			byte[] data2 = new byte[data.Length];
			IOUtil.ReadFully(@in, data2, 0, data.Length);
			NUnit.Framework.Assert.IsTrue("same content", Arrays.Equals(data2, data));
			NUnit.Framework.Assert.AreEqual("stream at EOF", -1, @in.Read());
			@in.Close();
		}

		/// <exception cref="System.Exception"></exception>
		public virtual void TestWhole_LargeObject()
		{
			int type = Constants.OBJ_BLOB;
			byte[] data = rng.NextBytes(streamThreshold + 5);
			RevBlob id = tr.Blob(data);
			tr.Branch("master").Commit().Add("A", id).Create();
			tr.PackAndPrune();
			NUnit.Framework.Assert.IsTrue("has blob", wc.Has(id));
			ObjectLoader ol = wc.Open(id);
			NUnit.Framework.Assert.IsNotNull("created loader", ol);
			NUnit.Framework.Assert.AreEqual(type, ol.GetType());
			NUnit.Framework.Assert.AreEqual(data.Length, ol.GetSize());
			NUnit.Framework.Assert.IsTrue("is large", ol.IsLarge());
			try
			{
				ol.GetCachedBytes();
				NUnit.Framework.Assert.Fail("Should have thrown LargeObjectException");
			}
			catch (LargeObjectException tooBig)
			{
				NUnit.Framework.Assert.AreEqual(MessageFormat.Format(JGitText.Get().largeObjectException
					, id.Name), tooBig.Message);
			}
			ObjectStream @in = ol.OpenStream();
			NUnit.Framework.Assert.IsNotNull("have stream", @in);
			NUnit.Framework.Assert.AreEqual(type, @in.GetType());
			NUnit.Framework.Assert.AreEqual(data.Length, @in.GetSize());
			byte[] data2 = new byte[data.Length];
			IOUtil.ReadFully(@in, data2, 0, data.Length);
			NUnit.Framework.Assert.IsTrue("same content", Arrays.Equals(data2, data));
			NUnit.Framework.Assert.AreEqual("stream at EOF", -1, @in.Read());
			@in.Close();
		}

		/// <exception cref="System.Exception"></exception>
		public virtual void TestDelta_SmallObjectChain()
		{
			ObjectInserter.Formatter fmt = new ObjectInserter.Formatter();
			byte[] data0 = new byte[512];
			Arrays.Fill(data0, unchecked((byte)unchecked((int)(0xf3))));
			ObjectId id0 = fmt.IdFor(Constants.OBJ_BLOB, data0);
			TemporaryBuffer.Heap pack = new TemporaryBuffer.Heap(64 * 1024);
			PackHeader(pack, 4);
			ObjectHeader(pack, Constants.OBJ_BLOB, data0.Length);
			Deflate(pack, data0);
			byte[] data1 = Clone(unchecked((int)(0x01)), data0);
			byte[] delta1 = Delta(data0, data1);
			ObjectId id1 = fmt.IdFor(Constants.OBJ_BLOB, data1);
			ObjectHeader(pack, Constants.OBJ_REF_DELTA, delta1.Length);
			id0.CopyRawTo(pack);
			Deflate(pack, delta1);
			byte[] data2 = Clone(unchecked((int)(0x02)), data1);
			byte[] delta2 = Delta(data1, data2);
			ObjectId id2 = fmt.IdFor(Constants.OBJ_BLOB, data2);
			ObjectHeader(pack, Constants.OBJ_REF_DELTA, delta2.Length);
			id1.CopyRawTo(pack);
			Deflate(pack, delta2);
			byte[] data3 = Clone(unchecked((int)(0x03)), data2);
			byte[] delta3 = Delta(data2, data3);
			ObjectId id3 = fmt.IdFor(Constants.OBJ_BLOB, data3);
			ObjectHeader(pack, Constants.OBJ_REF_DELTA, delta3.Length);
			id2.CopyRawTo(pack);
			Deflate(pack, delta3);
			Digest(pack);
			byte[] raw = pack.ToByteArray();
			IndexPack ip = IndexPack.Create(repo, new ByteArrayInputStream(raw));
			ip.SetFixThin(true);
			ip.Index(NullProgressMonitor.INSTANCE);
			ip.RenameAndOpenPack();
			NUnit.Framework.Assert.IsTrue("has blob", wc.Has(id3));
			ObjectLoader ol = wc.Open(id3);
			NUnit.Framework.Assert.IsNotNull("created loader", ol);
			NUnit.Framework.Assert.AreEqual(Constants.OBJ_BLOB, ol.GetType());
			NUnit.Framework.Assert.AreEqual(data3.Length, ol.GetSize());
			NUnit.Framework.Assert.IsFalse("is large", ol.IsLarge());
			NUnit.Framework.Assert.IsNotNull(ol.GetCachedBytes());
			NUnit.Framework.Assert.IsTrue(Arrays.Equals(data3, ol.GetCachedBytes()));
			ObjectStream @in = ol.OpenStream();
			NUnit.Framework.Assert.IsNotNull("have stream", @in);
			NUnit.Framework.Assert.AreEqual(Constants.OBJ_BLOB, @in.GetType());
			NUnit.Framework.Assert.AreEqual(data3.Length, @in.GetSize());
			byte[] act = new byte[data3.Length];
			IOUtil.ReadFully(@in, act, 0, data3.Length);
			NUnit.Framework.Assert.IsTrue("same content", Arrays.Equals(act, data3));
			NUnit.Framework.Assert.AreEqual("stream at EOF", -1, @in.Read());
			@in.Close();
		}

		/// <exception cref="System.Exception"></exception>
		public virtual void TestDelta_LargeObjectChain()
		{
			ObjectInserter.Formatter fmt = new ObjectInserter.Formatter();
			byte[] data0 = new byte[streamThreshold + 5];
			Arrays.Fill(data0, unchecked((byte)unchecked((int)(0xf3))));
			ObjectId id0 = fmt.IdFor(Constants.OBJ_BLOB, data0);
			TemporaryBuffer.Heap pack = new TemporaryBuffer.Heap(64 * 1024);
			PackHeader(pack, 4);
			ObjectHeader(pack, Constants.OBJ_BLOB, data0.Length);
			Deflate(pack, data0);
			byte[] data1 = Clone(unchecked((int)(0x01)), data0);
			byte[] delta1 = Delta(data0, data1);
			ObjectId id1 = fmt.IdFor(Constants.OBJ_BLOB, data1);
			ObjectHeader(pack, Constants.OBJ_REF_DELTA, delta1.Length);
			id0.CopyRawTo(pack);
			Deflate(pack, delta1);
			byte[] data2 = Clone(unchecked((int)(0x02)), data1);
			byte[] delta2 = Delta(data1, data2);
			ObjectId id2 = fmt.IdFor(Constants.OBJ_BLOB, data2);
			ObjectHeader(pack, Constants.OBJ_REF_DELTA, delta2.Length);
			id1.CopyRawTo(pack);
			Deflate(pack, delta2);
			byte[] data3 = Clone(unchecked((int)(0x03)), data2);
			byte[] delta3 = Delta(data2, data3);
			ObjectId id3 = fmt.IdFor(Constants.OBJ_BLOB, data3);
			ObjectHeader(pack, Constants.OBJ_REF_DELTA, delta3.Length);
			id2.CopyRawTo(pack);
			Deflate(pack, delta3);
			Digest(pack);
			byte[] raw = pack.ToByteArray();
			IndexPack ip = IndexPack.Create(repo, new ByteArrayInputStream(raw));
			ip.SetFixThin(true);
			ip.Index(NullProgressMonitor.INSTANCE);
			ip.RenameAndOpenPack();
			NUnit.Framework.Assert.IsTrue("has blob", wc.Has(id3));
			ObjectLoader ol = wc.Open(id3);
			NUnit.Framework.Assert.IsNotNull("created loader", ol);
			NUnit.Framework.Assert.AreEqual(Constants.OBJ_BLOB, ol.GetType());
			NUnit.Framework.Assert.AreEqual(data3.Length, ol.GetSize());
			NUnit.Framework.Assert.IsTrue("is large", ol.IsLarge());
			try
			{
				ol.GetCachedBytes();
				NUnit.Framework.Assert.Fail("Should have thrown LargeObjectException");
			}
			catch (LargeObjectException tooBig)
			{
				NUnit.Framework.Assert.AreEqual(MessageFormat.Format(JGitText.Get().largeObjectException
					, id3.Name), tooBig.Message);
			}
			ObjectStream @in = ol.OpenStream();
			NUnit.Framework.Assert.IsNotNull("have stream", @in);
			NUnit.Framework.Assert.AreEqual(Constants.OBJ_BLOB, @in.GetType());
			NUnit.Framework.Assert.AreEqual(data3.Length, @in.GetSize());
			byte[] act = new byte[data3.Length];
			IOUtil.ReadFully(@in, act, 0, data3.Length);
			NUnit.Framework.Assert.IsTrue("same content", Arrays.Equals(act, data3));
			NUnit.Framework.Assert.AreEqual("stream at EOF", -1, @in.Read());
			@in.Close();
		}

		private byte[] Clone(int first, byte[] @base)
		{
			byte[] r = new byte[@base.Length];
			System.Array.Copy(@base, 1, r, 1, r.Length - 1);
			r[0] = unchecked((byte)first);
			return r;
		}

		/// <exception cref="System.IO.IOException"></exception>
		private byte[] Delta(byte[] @base, byte[] dest)
		{
			ByteArrayOutputStream tmp = new ByteArrayOutputStream();
			DeltaEncoder de = new DeltaEncoder(tmp, @base.Length, dest.Length);
			de.Insert(dest, 0, 1);
			de.Copy(1, @base.Length - 1);
			return tmp.ToByteArray();
		}

		/// <exception cref="System.IO.IOException"></exception>
		private void PackHeader(TemporaryBuffer.Heap pack, int cnt)
		{
			byte[] hdr = new byte[8];
			NB.EncodeInt32(hdr, 0, 2);
			NB.EncodeInt32(hdr, 4, cnt);
			pack.Write(Constants.PACK_SIGNATURE);
			pack.Write(hdr, 0, 8);
		}

		/// <exception cref="System.IO.IOException"></exception>
		private void ObjectHeader(TemporaryBuffer.Heap pack, int type, int sz)
		{
			byte[] buf = new byte[8];
			int nextLength = (int)(((uint)sz) >> 4);
			buf[0] = unchecked((byte)((nextLength > 0 ? unchecked((int)(0x80)) : unchecked((int
				)(0x00))) | (type << 4) | (sz & unchecked((int)(0x0F)))));
			sz = nextLength;
			int n = 1;
			while (sz > 0)
			{
				nextLength = (int)(((uint)nextLength) >> 7);
				buf[n++] = unchecked((byte)((nextLength > 0 ? unchecked((int)(0x80)) : unchecked(
					(int)(0x00))) | (sz & unchecked((int)(0x7F)))));
				sz = nextLength;
			}
			pack.Write(buf, 0, n);
		}

		/// <exception cref="System.IO.IOException"></exception>
		private void Deflate(TemporaryBuffer.Heap pack, byte[] content)
		{
			Deflater deflater = new Deflater();
			byte[] buf = new byte[128];
			deflater.SetInput(content, 0, content.Length);
			deflater.Finish();
			do
			{
				int n = deflater.Deflate(buf, 0, buf.Length);
				if (n > 0)
				{
					pack.Write(buf, 0, n);
				}
			}
			while (!deflater.IsFinished);
			deflater.Finish();
		}

		/// <exception cref="System.IO.IOException"></exception>
		private void Digest(TemporaryBuffer.Heap buf)
		{
			MessageDigest md = Constants.NewMessageDigest();
			md.Update(buf.ToByteArray());
			buf.Write(md.Digest());
		}
	}
}
