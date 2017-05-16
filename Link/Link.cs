using System;
using System.IO.Ports;

/// <summary>
/// Link.
/// </summary>
using System.Threading;


namespace Linklaget
{
	/// <summary>
	/// Link.
	/// </summary>
	public class Link
	{
		/// <summary>
		/// The DELIMITE for slip protocol.
		/// </summary>
		const byte DELIMITERA = (byte)'A';

		const byte DELIMITERB = (byte)'B';

		const byte DELIMITERC = (byte)'C';

		const byte DELIMITERD = (byte)'D';
		/// <summary>
		/// The buffer for link.
		/// </summary>
		private byte[] buffer;

		private int BUFFER_SIZE;

		/// <summary>
		/// The serial port.
		/// </summary>
		SerialPort serialPort;

		/// <summary>
		/// Initializes a new instance of the <see cref="link"/> class.
		/// </summary>
		public Link (int BUFSIZE, string APP)
		{
			// Create a new SerialPort object with default settings.
			#if DEBUG
			if (APP.Equals ("FILE_SERVER")) {
				serialPort = new SerialPort ("/dev/ttyS1", 115200, Parity.None, 8, StopBits.One);
			} else {
				serialPort = new SerialPort ("/dev/ttyS1", 115200, Parity.None, 8, StopBits.One);
			}
			#else
			serialPort = new SerialPort("/dev/ttyS1",115200,Parity.None,8,StopBits.One);
			#endif
			if (!serialPort.IsOpen)
				serialPort.Open ();

			buffer = new byte[(BUFSIZE * 2)];
			BUFFER_SIZE = BUFSIZE;

			// Uncomment the next line to use timeout
			//serialPort.ReadTimeout = 1000;

			serialPort.DiscardInBuffer ();
			serialPort.DiscardOutBuffer ();
		}

		/// <summary>
		/// Send the specified buf and size.
		/// </summary>
		/// <param name='buf'>
		/// Buffer.
		/// </param>
		/// <param name='size'>
		/// Size.
		/// </param>
		public void send (byte[] buf, int size)
		{
			if (size > BUFFER_SIZE)

				throw new System.ArgumentException(@"Parameter cannot be larger than set buffersize of {BUFFER_SIZE}, was {size}", "size");

			byte[] sendBuf = new byte[BUFFER_SIZE*2+2];

			//	Console.WriteLine ("SIZE " + size.ToString());

			Array.Copy(buf,0,sendBuf,1,size);

			sendBuf [0] = DELIMITERA;

			var counter = 1;

			for (int i = 0 ; i < size; i++) {

				if (buf [i].Equals (DELIMITERA)) {

					sendBuf[counter] = DELIMITERB;
					sendBuf[counter+1] = DELIMITERC;

					counter += 2;
				} else if (buf [i].Equals (DELIMITERB)) {

					sendBuf[counter] = DELIMITERB;
					sendBuf[counter+1] = DELIMITERD;
					counter += 2;
				} 
				else {
					sendBuf[counter] = buf [i];
					counter++;
				}
			}

			sendBuf [counter] = DELIMITERA;

			Console.WriteLine ("\nLink send:\n" + BytesToString(sendBuf));


			//serialPort.Write (buf2Send,0,buf2Send.Length);

			serialPort.BaseStream.WriteAsync(sendBuf,0,sendBuf.Length);

		}

		public static string BytesToString (byte[] byteArray)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder ("{ ");
			for (var i = 0; i < byteArray.Length; i++) {
				var b = byteArray [i];
				sb.Append (b);
				if (i < byteArray.Length - 1) {
					sb.Append (", ");
				}
			}
			sb.Append (" }");
			return sb.ToString ();
		}


		/// <summary>
		/// Receive the specified buf and size.
		/// </summary>
		/// <param name='buf'>
		/// Buffer.
		/// </param>
		/// <param name='size'>
		/// Size.
		/// </param>
		public int receive (ref byte[] buf)
		{
			var foundLastFrame = false;
			var tempBuf = new byte[BUFFER_SIZE * 2+2];
			int byteIndex = 0;

			while (!foundLastFrame) {

				byteIndex = 0;

				while ((byte)serialPort.ReadByte () != DELIMITERA) {};

				Console.WriteLine ("Received DELIMITERA!");

				while (true) { 

					tempBuf [byteIndex] = (byte)serialPort.ReadByte ();


					if (tempBuf [byteIndex] == DELIMITERA) {
						foundLastFrame = true;
						break;
					}

					byteIndex++;
				};

			}

			var returnBuf = new byte[BUFFER_SIZE];

			var counter = 0; 
			var numOfA = 0;

			for (int i = 0; i < byteIndex; i++) {

				if (tempBuf [i].Equals (DELIMITERA)) {

					numOfA++;

					if (numOfA == 2)
						break;
				} 
				else {
					if (tempBuf [i].Equals (DELIMITERB) && tempBuf [i + 1].Equals (DELIMITERC)) {

						returnBuf [counter] = DELIMITERA;
						++counter;
						++i;
					} else if (tempBuf [i].Equals (DELIMITERB) && tempBuf [i + 1].Equals (DELIMITERD)) {

						returnBuf [counter] = DELIMITERB;
						++counter;
						++i;
					} else {


						returnBuf [counter] = tempBuf [i];
						++counter;
					}
				}
			}


			byte[] buf2Receive = new byte[counter];

			Array.Copy (returnBuf,0, buf2Receive, 0, counter); 

			Console.WriteLine ("\nLink receive:\n" + BytesToString (buf2Receive));

			buf = buf2Receive;

			return counter;
		}


	}
}
