using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace bftmtn
{
	public class BrainfuckStream : Stream
	{
		private string code;
		private byte[] mem;
		private int memPointer;
		private int[] loops;
		private int loopPointer;

		public bool done;

		byte[] inputBuffer;
		int inputBufferR;
		int inputBufferW;
		byte[] outputBuffer;
		int outputBufferR;
		int outputBufferW;

		Task brainfuck;

		public BrainfuckStream(string code, int size = 30000, int buffSize = 1024)
		{
			this.code = code;
			mem = new byte[size];
			loops = new int[size];

			inputBuffer = new byte[buffSize];
			outputBuffer = new byte[buffSize];

			memPointer = 0;
			loopPointer = 0;

			inputBufferR = 0;
			inputBufferW = 0;
			outputBufferR = 0;
			outputBufferW = 0;
			
			brainfuck = new Task(new Action(Simulation));
			brainfuck.Start();
		}
		public BrainfuckStream(string code, byte[] mem, int buffSize = 1024)
		{
			this.code = code;
			this.mem = mem;
			loops = new int[mem.Length];

			memPointer = 0;
			loopPointer = 0;

			inputBuffer = new byte[buffSize];
			outputBuffer = new byte[buffSize];

			brainfuck = new Task(new Action(Simulation));
			brainfuck.Start();
		}

		public override bool CanRead => !(done && outputBufferR == outputBufferW);
		public override bool CanSeek => false;
		public override bool CanWrite => !done;

		public override long Length => throw new NotSupportedException();
		public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

		public override void Flush()
		{
			throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (done && outputBufferR == outputBufferW) return 0;
			int c = 0;
			for(int i = 0; i < count; i++)
			{
				if(outputBufferR == outputBufferW)
				{
					SpinWait.SpinUntil(() => { return done || outputBufferR != outputBufferW; });
				}
				if (done && outputBufferR == outputBufferW) break;
				buffer[offset + i] = outputBuffer[outputBufferR];
				outputBufferR++;
				if (outputBufferR >= outputBuffer.Length) outputBufferR = 0;
				c++;
			}
			return c;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (done) return;
			for(int i = 0; i < count; i++)
			{
				inputBuffer[inputBufferW] = buffer[offset + i];
				inputBufferW++;
				if (inputBufferW >= outputBuffer.Length) inputBufferW = 0;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public bool Wait()
		{
			try
			{
				brainfuck.Wait();
				return false;
			}
			catch (ObjectDisposedException)
			{
				return true;
			}
		}

		private void Simulation()
		{
			for (int codePointer = 0; codePointer < code.Length; codePointer++)
			{
				switch (code[codePointer])
				{
					case '+':
						mem[memPointer]++;
						break;
					case '-':
						mem[memPointer]--;
						break;
					case '>':
						memPointer++;
						break;
					case '<':
						memPointer--;
						break;
					case '[':
						loops[loopPointer] = codePointer;
						loopPointer++;
						if (mem[memPointer] == 0)
						{
							int cl = loopPointer - 1;
							bool done = false;
							while (!done)
							{
								codePointer++;
								switch (code[codePointer])
								{
									case '[':
										loops[loopPointer] = codePointer;
										loopPointer++;
										break;
									case ']':
										loopPointer--;
										done = loopPointer == cl;
										break;
								}
							}
						}
						break;
					case ']':
						if (mem[memPointer] == 0) loopPointer--;
						else codePointer = loops[loopPointer - 1];
						break;
					case '.':
						outputBuffer[outputBufferW] = mem[memPointer];
						outputBufferW++;
						if (outputBufferW >= outputBuffer.Length) outputBufferW = 0;
						break;
					case ',':
						if (inputBufferR == inputBufferW)
						{
							SpinWait.SpinUntil(() => { return inputBufferR != inputBufferW; });
						}
						mem[memPointer] = inputBuffer[inputBufferR];
						inputBufferR++;
						if (inputBufferR >= inputBuffer.Length) inputBufferR = 0;
						break;
				}
			}
			done = true;
		}
	}
}
