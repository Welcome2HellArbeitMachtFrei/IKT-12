using System;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Transportlaget;
using Linklaget;
using tcp;

namespace client
{
	class file_client
	{
		const int BUFSIZE = 1000;

		Transport trans;

		private file_client (string[] args)
		{
			string fileName = "";

			trans = new Transport (1000, "FILE_CLIENT");

			long size = 0;

			while (size == 0) { //iteration until file 

				if(args.Length != 1)
					fileName = SendRequestFromInput();
				else
					fileName = SendRequestFromArg(args[1]);

				size = readSize ();

				if (size == 0)
					Console.WriteLine ("File dosn't exist, try again");
			}


			receiveFile (fileName, size);

		}
			
		private string SendRequestFromInput ()
		{
			Console.WriteLine ("Write the name of the file:");
			string request = Console.ReadLine ();

			var bytes2Send = System.Text.Encoding.UTF8.GetBytes(request);
			trans.send(bytes2Send, bytes2Send.Length);

			return request;
		}

		private string SendRequestFromArg (string request)
		{
			var bytes2Send = System.Text.Encoding.UTF8.GetBytes(request);
			trans.send(bytes2Send, bytes2Send.Length);
			return request;
		}


		private long readSize ()
		{
			var tempBuf = new byte[BUFSIZE];

			int length = trans.receive (ref tempBuf);

			long value = 0;
			for (int i = 0; i < length; i++)
			{
				value += ((long) tempBuf[i] & 0xffL) << (8 * i);
			}

			var size = value;//(tempBuf, 0);

			Console.WriteLine ("Size is: {0} bytes", size);

			return size;
		}

		private void receiveFile (String fileName, long size)
		{
			// Commence download
			Console.WriteLine("Downloading...");

			byte[] inStream = new byte[BUFSIZE];
			long totalReceived = 0;

			// Setup FileStream which writes to desktop
			string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + LIB.extractFileName(fileName);
			FileStream fs = new FileStream (path, FileMode.Create);

			// Download file chunk by chunk and write it to FileStream path
			do
			{
				int count = trans.receive(ref inStream);


				fs.Write(inStream, 0, count);
				totalReceived += count;

				string diff = totalReceived + "/" + size;

				Console.Write("\r{0}%   " + diff + " bytes", String.Format("{0:0}", 100*totalReceived/size));
			}
			while (totalReceived != size);

			Console.WriteLine ("\nDownload complete");
		}
			

		public static void Main (string[] args)
		{
			Console.WriteLine ("Client starts...");
			new file_client (args);
		}


		/*public static void Main()
		{
			
			var firstbyte = System.Text.Encoding.UTF8.GetBytes ("JKSMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM");

			byte[] buffer = new byte[1000];
			int i = 0;
			Transport trans = new Transport (1000, "FILE_CLIENT");
			while (i++ != 2) {
				
				//Thread.Sleep (100);

				Console.WriteLine ("First SEND");
				trans.send (firstbyte, firstbyte.Length);

				trans.receive (ref buffer);

				Console.WriteLine ("-------------------------------");


			}
				

		}*/


	}
}
