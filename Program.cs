using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bftmtn
{
	class Program
	{
		static BrainfuckStream bfs;

		static Task writeIn;
		static void Main(string[] args)
		{
			bfs = new BrainfuckStream(args.Length > 0 ? args[0] : "+[,.]");
			writeIn = new Task(new Action(WriteIn));
			writeIn.Start();

			int b = bfs.ReadByte();
			while(b > -1)
			{
				Console.Write(Convert.ToChar(b));
				b = bfs.ReadByte();
			}

			Console.Write("\n\nDONE");
			writeIn.Wait();
		}
		static void WriteIn()
		{
			while(!bfs.done)
			{
				bfs.WriteByte((byte)Console.ReadKey(true).KeyChar);
			}
		}
	}
}
