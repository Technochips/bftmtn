using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bftmtn
{
	class Program
	{
		static List<BrainfuckStream> bfs;
		static byte[] mem;

		static Task writeIn;
		static void Main(string[] args)
		{
			mem = new byte[30000];
			bfs = new List<BrainfuckStream>();
			if(args.Length == 0)
				bfs.Add(new BrainfuckStream("+[,.]", mem));
			else
			{
				for(int i = 0; i < args.Length; i++)
				{
					bfs.Add(new BrainfuckStream(args[i], mem));
				}
			}
			writeIn = new Task(new Action(WriteIn));
			writeIn.Start();

			int b = bfs[0].ReadByte();
			while(b > -1)
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
