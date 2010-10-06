using NGit;
using NGit.Errors;
using Sharpen;

namespace NGit
{
	public class RepositoryResolveTest : SampleDataRepositoryTestCase
	{
		/// <exception cref="System.IO.IOException"></exception>
		public virtual void TestObjectId_existing()
		{
			NUnit.Framework.Assert.AreEqual("49322bb17d3acc9146f98c97d078513228bbf3c0", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0").Name);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void TestObjectId_nonexisting()
		{
			NUnit.Framework.Assert.AreEqual("49322bb17d3acc9146f98c97d078513228bbf3c1", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c1").Name);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void TestObjectId_objectid_implicit_firstparent()
		{
			NUnit.Framework.Assert.AreEqual("6e1475206e57110fcef4b92320436c1e9872a322", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0^").Name);
			NUnit.Framework.Assert.AreEqual("1203b03dc816ccbb67773f28b3c19318654b0bc8", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0^^").Name);
			NUnit.Framework.Assert.AreEqual("bab66b48f836ed950c99134ef666436fb07a09a0", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0^^^").Name);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void TestObjectId_objectid_self()
		{
			NUnit.Framework.Assert.AreEqual("49322bb17d3acc9146f98c97d078513228bbf3c0", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0^0").Name);
			NUnit.Framework.Assert.AreEqual("49322bb17d3acc9146f98c97d078513228bbf3c0", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0^0^0").Name);
			NUnit.Framework.Assert.AreEqual("49322bb17d3acc9146f98c97d078513228bbf3c0", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0^0^0^0").Name);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void TestObjectId_objectid_explicit_firstparent()
		{
			NUnit.Framework.Assert.AreEqual("6e1475206e57110fcef4b92320436c1e9872a322", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0^1").Name);
			NUnit.Framework.Assert.AreEqual("1203b03dc816ccbb67773f28b3c19318654b0bc8", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0^1^1").Name);
			NUnit.Framework.Assert.AreEqual("bab66b48f836ed950c99134ef666436fb07a09a0", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0^1^1^1").Name);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void TestObjectId_objectid_explicit_otherparents()
		{
			NUnit.Framework.Assert.AreEqual("6e1475206e57110fcef4b92320436c1e9872a322", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0^1").Name);
			NUnit.Framework.Assert.AreEqual("f73b95671f326616d66b2afb3bdfcdbbce110b44", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0^2").Name);
			NUnit.Framework.Assert.AreEqual("d0114ab8ac326bab30e3a657a0397578c5a1af88", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0^3").Name);
			NUnit.Framework.Assert.AreEqual("d0114ab8ac326bab30e3a657a0397578c5a1af88", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0^03").Name);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void TestRef_refname()
		{
			NUnit.Framework.Assert.AreEqual("49322bb17d3acc9146f98c97d078513228bbf3c0", db.Resolve
				("master^0").Name);
			NUnit.Framework.Assert.AreEqual("6e1475206e57110fcef4b92320436c1e9872a322", db.Resolve
				("master^").Name);
			NUnit.Framework.Assert.AreEqual("6e1475206e57110fcef4b92320436c1e9872a322", db.Resolve
				("refs/heads/master^1").Name);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void TestDistance()
		{
			NUnit.Framework.Assert.AreEqual("49322bb17d3acc9146f98c97d078513228bbf3c0", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0~0").Name);
			NUnit.Framework.Assert.AreEqual("6e1475206e57110fcef4b92320436c1e9872a322", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0~1").Name);
			NUnit.Framework.Assert.AreEqual("1203b03dc816ccbb67773f28b3c19318654b0bc8", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0~2").Name);
			NUnit.Framework.Assert.AreEqual("bab66b48f836ed950c99134ef666436fb07a09a0", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0~3").Name);
			NUnit.Framework.Assert.AreEqual("bab66b48f836ed950c99134ef666436fb07a09a0", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0~03").Name);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void TestTree()
		{
			NUnit.Framework.Assert.AreEqual("6020a3b8d5d636e549ccbd0c53e2764684bb3125", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0^{tree}").Name);
			NUnit.Framework.Assert.AreEqual("02ba32d3649e510002c21651936b7077aa75ffa9", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0^^{tree}").Name);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void TestHEAD()
		{
			NUnit.Framework.Assert.AreEqual("6020a3b8d5d636e549ccbd0c53e2764684bb3125", db.Resolve
				("HEAD^{tree}").Name);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void TestDerefCommit()
		{
			NUnit.Framework.Assert.AreEqual("49322bb17d3acc9146f98c97d078513228bbf3c0", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0^{}").Name);
			NUnit.Framework.Assert.AreEqual("49322bb17d3acc9146f98c97d078513228bbf3c0", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0^{commit}").Name);
			// double deref
			NUnit.Framework.Assert.AreEqual("6020a3b8d5d636e549ccbd0c53e2764684bb3125", db.Resolve
				("49322bb17d3acc9146f98c97d078513228bbf3c0^{commit}^{tree}").Name);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void TestDerefTag()
		{
			NUnit.Framework.Assert.AreEqual("17768080a2318cd89bba4c8b87834401e2095703", db.Resolve
				("refs/tags/B").Name);
			NUnit.Framework.Assert.AreEqual("d86a2aada2f5e7ccf6f11880bfb9ab404e8a8864", db.Resolve
				("refs/tags/B^{commit}").Name);
			NUnit.Framework.Assert.AreEqual("032c063ce34486359e3ee3d4f9e5c225b9e1a4c2", db.Resolve
				("refs/tags/B10th").Name);
			NUnit.Framework.Assert.AreEqual("d86a2aada2f5e7ccf6f11880bfb9ab404e8a8864", db.Resolve
				("refs/tags/B10th^{commit}").Name);
			NUnit.Framework.Assert.AreEqual("d86a2aada2f5e7ccf6f11880bfb9ab404e8a8864", db.Resolve
				("refs/tags/B10th^{}").Name);
			NUnit.Framework.Assert.AreEqual("d86a2aada2f5e7ccf6f11880bfb9ab404e8a8864", db.Resolve
				("refs/tags/B10th^0").Name);
			NUnit.Framework.Assert.AreEqual("d86a2aada2f5e7ccf6f11880bfb9ab404e8a8864", db.Resolve
				("refs/tags/B10th~0").Name);
			NUnit.Framework.Assert.AreEqual("0966a434eb1a025db6b71485ab63a3bfbea520b6", db.Resolve
				("refs/tags/B10th^").Name);
			NUnit.Framework.Assert.AreEqual("0966a434eb1a025db6b71485ab63a3bfbea520b6", db.Resolve
				("refs/tags/B10th^1").Name);
			NUnit.Framework.Assert.AreEqual("0966a434eb1a025db6b71485ab63a3bfbea520b6", db.Resolve
				("refs/tags/B10th~1").Name);
			NUnit.Framework.Assert.AreEqual("2c349335b7f797072cf729c4f3bb0914ecb6dec9", db.Resolve
				("refs/tags/B10th~2").Name);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void TestDerefBlob()
		{
			NUnit.Framework.Assert.AreEqual("fd608fbe625a2b456d9f15c2b1dc41f252057dd7", db.Resolve
				("spearce-gpg-pub^{}").Name);
			NUnit.Framework.Assert.AreEqual("fd608fbe625a2b456d9f15c2b1dc41f252057dd7", db.Resolve
				("spearce-gpg-pub^{blob}").Name);
			NUnit.Framework.Assert.AreEqual("fd608fbe625a2b456d9f15c2b1dc41f252057dd7", db.Resolve
				("fd608fbe625a2b456d9f15c2b1dc41f252057dd7^{}").Name);
			NUnit.Framework.Assert.AreEqual("fd608fbe625a2b456d9f15c2b1dc41f252057dd7", db.Resolve
				("fd608fbe625a2b456d9f15c2b1dc41f252057dd7^{blob}").Name);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void TestDerefTree()
		{
			NUnit.Framework.Assert.AreEqual("032c063ce34486359e3ee3d4f9e5c225b9e1a4c2", db.Resolve
				("refs/tags/B10th").Name);
			NUnit.Framework.Assert.AreEqual("856ec208ae6cadac25a6d74f19b12bb27a24fe24", db.Resolve
				("032c063ce34486359e3ee3d4f9e5c225b9e1a4c2^{tree}").Name);
			NUnit.Framework.Assert.AreEqual("856ec208ae6cadac25a6d74f19b12bb27a24fe24", db.Resolve
				("refs/tags/B10th^{tree}").Name);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void TestParseGitDescribeOutput()
		{
			ObjectId exp = db.Resolve("b");
			AssertEquals(exp, db.Resolve("B-g7f82283"));
			// old style
			AssertEquals(exp, db.Resolve("B-6-g7f82283"));
			// new style
			AssertEquals(exp, db.Resolve("B-6-g7f82283^0"));
			AssertEquals(exp, db.Resolve("B-6-g7f82283^{commit}"));
			try
			{
				db.Resolve("B-6-g7f82283^{blob}");
				NUnit.Framework.Assert.Fail("expected IncorrectObjectTypeException");
			}
			catch (IncorrectObjectTypeException)
			{
			}
			// Expected
			AssertEquals(db.Resolve("b^1"), db.Resolve("B-6-g7f82283^1"));
			AssertEquals(db.Resolve("b~2"), db.Resolve("B-6-g7f82283~2"));
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void TestParseLookupPath()
		{
			ObjectId b2_txt = Id("10da5895682013006950e7da534b705252b03be6");
			ObjectId b3_b2_txt = Id("e6bfff5c1d0f0ecd501552b43a1e13d8008abc31");
			ObjectId b_root = Id("acd0220f06f7e4db50ea5ba242f0dfed297b27af");
			ObjectId master_txt = Id("82b1d08466e9505f8666b778744f9a3471a70c81");
			AssertEquals(b2_txt, db.Resolve("b:b/b2.txt"));
			AssertEquals(b_root, db.Resolve("b:"));
			AssertEquals(master_txt, db.Resolve(":master.txt"));
			AssertEquals(b3_b2_txt, db.Resolve("b~3:b/b2.txt"));
			NUnit.Framework.Assert.IsNull("no FOO", db.Resolve("b:FOO"));
			NUnit.Framework.Assert.IsNull("no b/FOO", db.Resolve("b:b/FOO"));
			NUnit.Framework.Assert.IsNull("no b/FOO", db.Resolve(":b/FOO"));
			NUnit.Framework.Assert.IsNull("no not-a-branch:", db.Resolve("not-a-branch:"));
		}

		private static ObjectId Id(string name)
		{
			return ObjectId.FromString(name);
		}
	}
}
