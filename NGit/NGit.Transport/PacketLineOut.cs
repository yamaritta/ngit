using NGit;
using Sharpen;

namespace NGit.Transport
{
	/// <summary>Write Git style pkt-line formatting to an output stream.</summary>
	/// <remarks>
	/// Write Git style pkt-line formatting to an output stream.
	/// <p>
	/// This class is not thread safe and may issue multiple writes to the underlying
	/// stream for each method call made.
	/// <p>
	/// This class performs no buffering on its own. This makes it suitable to
	/// interleave writes performed by this class with writes performed directly
	/// against the underlying OutputStream.
	/// </remarks>
	public class PacketLineOut
	{
		private readonly OutputStream @out;

		private readonly byte[] lenbuffer;

		/// <summary>Create a new packet line writer.</summary>
		/// <remarks>Create a new packet line writer.</remarks>
		/// <param name="outputStream">stream.</param>
		public PacketLineOut(OutputStream outputStream)
		{
			@out = outputStream;
			lenbuffer = new byte[5];
		}

		/// <summary>Write a UTF-8 encoded string as a single length-delimited packet.</summary>
		/// <remarks>Write a UTF-8 encoded string as a single length-delimited packet.</remarks>
		/// <param name="s">string to write.</param>
		/// <exception cref="System.IO.IOException">
		/// the packet could not be written, the stream is corrupted as
		/// the packet may have been only partially written.
		/// </exception>
		public virtual void WriteString(string s)
		{
			WritePacket(Constants.Encode(s));
		}

		/// <summary>Write a binary packet to the stream.</summary>
		/// <remarks>Write a binary packet to the stream.</remarks>
		/// <param name="packet">
		/// the packet to write; the length of the packet is equal to the
		/// size of the byte array.
		/// </param>
		/// <exception cref="System.IO.IOException">
		/// the packet could not be written, the stream is corrupted as
		/// the packet may have been only partially written.
		/// </exception>
		public virtual void WritePacket(byte[] packet)
		{
			FormatLength(packet.Length + 4);
			@out.Write(lenbuffer, 0, 4);
			@out.Write(packet);
		}

		/// <summary>Write a packet end marker, sometimes referred to as a flush command.</summary>
		/// <remarks>
		/// Write a packet end marker, sometimes referred to as a flush command.
		/// <p>
		/// Technically this is a magical packet type which can be detected
		/// separately from an empty string or an empty packet.
		/// <p>
		/// Implicitly performs a flush on the underlying OutputStream to ensure the
		/// peer will receive all data written thus far.
		/// </remarks>
		/// <exception cref="System.IO.IOException">
		/// the end marker could not be written, the stream is corrupted
		/// as the end marker may have been only partially written.
		/// </exception>
		public virtual void End()
		{
			FormatLength(0);
			@out.Write(lenbuffer, 0, 4);
			Flush();
		}

		/// <summary>Flush the underlying OutputStream.</summary>
		/// <remarks>
		/// Flush the underlying OutputStream.
		/// <p>
		/// Performs a flush on the underlying OutputStream to ensure the peer will
		/// receive all data written thus far.
		/// </remarks>
		/// <exception cref="System.IO.IOException">the underlying stream failed to flush.</exception>
		public virtual void Flush()
		{
			@out.Flush();
		}

		private static readonly byte[] hexchar = new byte[] { (byte)('0'), (byte)('1'), (
			byte)('2'), (byte)('3'), (byte)('4'), (byte)('5'), (byte)('6'), (byte)('7'), (byte
			)('8'), (byte)('9'), (byte)('a'), (byte)('b'), (byte)('c'), (byte)('d'), (byte)(
			'e'), (byte)('f') };

		private void FormatLength(int w)
		{
			FormatLength(lenbuffer, w);
		}

		internal static void FormatLength(byte[] lenbuffer, int w)
		{
			int o = 3;
			while (o >= 0 && w != 0)
			{
				lenbuffer[o--] = hexchar[w & unchecked((int)(0xf))];
				w = (int)(((uint)w) >> 4);
			}
			while (o >= 0)
			{
				lenbuffer[o--] = (byte)('0');
			}
		}
	}
}
