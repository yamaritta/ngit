using NSch;
using NSch.Jce;
using Sharpen;

namespace NSch.Jce
{
	public class HMACSHA196 : MAC
	{
		private static readonly string name = "hmac-sha1-96";

		private const int bsize = 12;

		private Mac mac;

		public virtual int GetBlockSize()
		{
			return bsize;
		}

		/// <exception cref="System.Exception"></exception>
		public virtual void Init(byte[] key)
		{
			if (key.Length > 20)
			{
				byte[] tmp = new byte[20];
				System.Array.Copy(key, 0, tmp, 0, 20);
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

		private readonly byte[] _buf20 = new byte[20];

		public virtual void DoFinal(byte[] buf, int offset)
		{
			try
			{
				mac.DoFinal(_buf20, 0);
			}
			catch (ShortBufferException)
			{
			}
			System.Array.Copy(_buf20, 0, buf, 0, 12);
		}

		public virtual string GetName()
		{
			return name;
		}
	}
}
