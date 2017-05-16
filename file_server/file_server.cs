using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using Linklaget;
using Transportlaget;
using System.Threading;

namespace tcp
{
	class file_server
	{
		/// <summary>
		/// The PORT
		/// </summary>
		const int PORT = 9000;
		/// <summary>
		/// The BUFSIZE
		/// </summary>
		const int BUFSIZE = 1000;

		string fileName{ get; set; }

		///<summary>
		/// Initializes a new instance of the <see cref="file_server"/> class.
		/// Opretter en socket.
		/// Venter på en connect fra en klient.
		/// Modtager filnavn
		/// Finder filstørrelsen
		/// Kalder metoden sendFile
		/// Lukker socketen og programmet
		/// </summary>
		private file_server ()
		{
			
			long size  = 0;
			Console.WriteLine (" >> Server Started");
			//Link link = new Link (1000,"FILE_SERVER");

			Transport trans = new Transport(1000,"FILE_SERVER");

			while (true) {

				Console.WriteLine (" >> Accept connection from NEW client"); 

				size = 0;

				do {

					// Reads filename from client
					fileName = receiveFilename(trans);

					// Send file size to client
					size = sendFileSize(trans, fileName);

				} while(size == 0); //Check if file exist 
					

				// Send the actual file to client
				sendFile (trans, fileName, size);

				Console.WriteLine (" >> Connection closed with THIS client");
				//break;
			}
		}

		private string receiveFilename(Transport trans)
		{
			byte[] receiver = new byte[BUFSIZE];

			//reads filename from client
			int size = trans.receive(ref receiver);

			string name = System.Text.Encoding.UTF8.GetString (receiver);

			Console.WriteLine ("Filname received:\n" + name);

			return name;
		}


		private long sendFileSize(Transport trans,string fileName)
		{

			//checks if file exist
			long size = LIB.check_File_Exists (fileName); 

			//Send size to client
			Console.WriteLine (" >> Size of file: " + size + " long");

			var sizeInBytes = BitConverter.GetBytes (size); // System.Text.Encoding.UTF8.GetBytes(size.ToString());

			Console.WriteLine ("SIZE OF ARRAY "+sizeInBytes.Length);

			trans.send (sizeInBytes, sizeInBytes.Length);

			return size;
		}

		private void sendFile(Transport trans,string fileName, long size)
		{
			var fs = new FileStream (fileName, FileMode.Open);

			// Send file in chunks
			Console.WriteLine (" >> Sending file...");

			byte[] sendBytes = new byte[BUFSIZE];
			int count;

			while ((count = fs.Read (sendBytes, 0, BUFSIZE)) > 0) {
				trans.send (sendBytes, count);
			}

			fs.Close ();

			Console.WriteLine (" >> Send complete");
		}

		public static byte[] TrimEnd(byte[] array)
		{
			byte[] newArray = new byte [array.Length];

			Array.Copy(array,newArray, array.Length);

			int lastIndex = Array.FindLastIndex(array, b => b != 0);

			Array.Resize(ref newArray, lastIndex + 1);

			return newArray;
		}

		public static void Main (string[] args)
		{
			Console.WriteLine ("Server starts...");
			new file_server ();
		}

		/*public static void Main()
		{

			Console.WriteLine ("AWAITING MESSAGE");

			Transport trans = new Transport (1000, "File_client");

			byte[] buffer = new byte[1000];

			var Thirdbyte = System.Text.Encoding.UTF8.GetBytes("KKK");

			int i = 0;
			while (i++ != 2) {
				trans.receive (ref buffer);
			
				//Thread.Sleep (100);

				Console.WriteLine ("First Received applikation");

				Console.WriteLine ("SENDING");

				trans.send (Thirdbyte, Thirdbyte.Length);

				 
			}

		}*/
	}
}
