using NSch;
using NSch.Jce;
using Sharpen;

namespace NSch.Jce
{
	public class HMACSHA1 : MAC
	{
		private static readonly string name = "hmac-sha1";

		private const int bsize = 20;

		private Mac mac;

		public virtual int GetBlockSize()
		{
			return bsize;
		}

		/// <exception cref="System.Exception"></exception>
		public virtual void Init(byte[] key)
		{
			if (key.Length > bsize)
			{
				byte[] tmp = new byte[bsize];
				System.Array.Copy(key, 0, tmp, 0, bsize);
				key = tmp;
			}
			SecretKeySpec skey = new SecretKeySpec(key, "HmacSHA1");
			mac = Mac.GetInstance("HmacSHA1");
			mac.Init(skey);
		}

		private readonly byte[] tmp = new byte[4];

		public virtual void Update(int i)
		{
			tmp[0] = unchecked((byte)((int)(((uint)i) >> 24)));
			tmp[1] = unchecked((byte)((int)(((uint)i) >> 16)));
			tmp[2] = unchecked((byte)((int)(((uint)i) >> 8)));
			tmp[3] = unchecked((byte)i);
			Update(tmp, 0, 4);
		}

		public virtual void Update(byte[] foo, int s, int l)
		{
			mac.Update(foo, s, l);
		}

		public virtual void DoFinal(byte[] buf, int offset)
		{
			try
			{
				mac.DoFinal(buf, offset);
			}
			catch (ShortBufferException)
			{
			}
		}

		public virtual string GetName()
		{
			return name;
		}
	}
}
