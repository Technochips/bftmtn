using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace bftmtn
{
	class Program
	{
		static List<BrainfuckStream> bfs;
		static byte[] mem;
		static short port;

		static Task writeIn;
		static void Main(string[] args)
		{
			bool networked = args[0] == "n";
			mem = new byte[30000];
			bfs = new List<BrainfuckStream>();
			if(args.Length == 0)
				bfs.Add(new BrainfuckStream("+[,.]", mem));
			else
			{
				if (!networked)
				{
					for (int i = 0; i < args.Length; i++)
					{
						bfs.Add(new BrainfuckStream(args[i], mem));
					}
				}
				else
				{
					port = short.Parse(args[1]);
					for(int i = 4; i < args.Length; i++)
					{
						bfs.Add(new BrainfuckStream(args[i], mem));
					}
				}
			}
			if (!networked)
			{
				writeIn = new Task(new Action(WriteIn));
				writeIn.Start();

				int b = bfs[0].ReadByte();
				while (b > -1)
				{
					Console.Write(Convert.ToChar(b));
					b = bfs[0].ReadByte();
				}

				Console.WriteLine("\n\nDONE. " + bfs.Count + " simulation(s).");

				{
					int i = 1;
					foreach (BrainfuckStream bf in bfs)
					{
						bf.Wait();
						Console.WriteLine("Sim " + i + " confirmed done");
						i++;
					}
				}
				Console.WriteLine("PRESS KEY TO EXIT");
				writeIn.Wait();
			}
			else
			{
				while (true)
				{
					TcpListener tcp = new TcpListener(IPAddress.Any, port);
					tcp.Start();
					TcpClient client = tcp.AcceptTcpClient();
					NetworkStream ns = client.GetStream();
					Console.WriteLine("TCP RECEIVED");
					BrainfuckStream pin = new BrainfuckStream(args[2], mem);
					Console.WriteLine("INPUT PROGRAM RUNNING");
					BrainfuckStream pout = new BrainfuckStream(args[3], mem);
					Console.WriteLine("OUTPUT PROGRAM RUNNING");

					//input
					Task task = new Task(new Action(() =>
					{
						int bi = ns.ReadByte();
						if (bi == -1) return;
						do
						{
							pin.WriteByte((byte)bi);
							Console.Write(Convert.ToChar(bi));
							try
							{
								bi = ns.ReadByte();
							}
							catch
							{
								bi = -1;
							}
						}
						while (!pin.done && bi > -1);
					}));
					task.Start();
					//output
					int b = pout.ReadByte();
					while (b > -1)
					{
						Console.Write(Convert.ToChar(b));
						ns.WriteByte((byte)b);
						b = pout.ReadByte();
					}
					ns.Close();
					client.Close();
					tcp.Stop();
				}
			}
		}
		static void WriteIn()
		{
			do
			{
				bfs[0].WriteByte((byte)Console.ReadKey(true).KeyChar);
			}
			while (!bfs[0].done);
		}
	}
}
