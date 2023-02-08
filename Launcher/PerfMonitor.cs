using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Launcher {
	class PerfMonitor {
		Stopwatch SWatch;
		Stopwatch PrintWatch;
		Stopwatch Elapsed;
		bool FirstAdd;

		long BestFrameTime;
		long WorstFrameTime;
		long[] FrameTimes;
		int FrameTimeCount;
		int SampleCount;

		public PerfMonitor() {
			FirstAdd = true;
			FrameTimes = new long[1024];
			FrameTimeCount = 0;
			SampleCount = 0;
		}

		public void Start() {
			SWatch = Stopwatch.StartNew();
			PrintWatch = Stopwatch.StartNew();
			Elapsed = Stopwatch.StartNew();
		}

		void Add(long MS) {
			if (FirstAdd) {
				FirstAdd = false;
				BestFrameTime = MS;
				WorstFrameTime = MS;
			}

			if (FrameTimeCount >= FrameTimes.Length) {
				FrameTimeCount = 0;
			}

			if (MS < BestFrameTime)
				BestFrameTime = MS;

			if (MS > WorstFrameTime)
				WorstFrameTime = MS;

			FrameTimes[FrameTimeCount] = MS;
			FrameTimeCount++;
			SampleCount++;
		}

		long CalcAverage() {
			int Len = SampleCount >= FrameTimes.Length ? FrameTimes.Length : SampleCount;
			long Sum = FrameTimes[0];

			for (int i = 1; i < Len; i++) {
				Sum += FrameTimes[i];
			}

			return Sum / Len;
		}

		void WriteClr(ConsoleColor Clr, string Str) {
			Console.ForegroundColor = Clr;
			Console.Write(Str);
		}

		void PrintStats(bool CustomColor = false) {
			ConsoleColor FG = ConsoleColor.Gray;

			if (CustomColor)
				FG = ConsoleColor.Green;

			long Average = CalcAverage();


			WriteClr(FG, "Frames: ");
			WriteClr(ConsoleColor.White, SampleCount.ToString());

			WriteClr(FG, "; Elapsed [ms]: ");
			WriteClr(ConsoleColor.White, Elapsed.ElapsedMilliseconds.ToString());

			WriteClr(FG, "; Avg [ms]: ");
			WriteClr(ConsoleColor.White, Average.ToString());

			WriteClr(FG, "; Avg [FPS]: ");
			WriteClr(ConsoleColor.White, string.Format("{0:0.00}", 1.0f / (Average / 1000.0f)));

			WriteClr(FG, "; Best [ms]: ");
			WriteClr(ConsoleColor.White, BestFrameTime.ToString());

			WriteClr(FG, "; Worst [ms]: ");
			WriteClr(ConsoleColor.White, WorstFrameTime.ToString());

			Console.WriteLine();

			//Console.WriteLine("Frames: {0}; Frame avg: {1} ms / {2:0.00} FPS; Total time: {3} ms; Best: {4} ms; Worst: {5} ms", SampleCount, Average, 1.0f / (Average / 1000.0f), Elapsed.ElapsedMilliseconds, BestFrameTime, WorstFrameTime);



			if (CustomColor)
				Console.ResetColor();
		}

		public void Trigger() {
			if (SampleCount == 1000) {
				PrintStats(true);
			}

			Add(SWatch.ElapsedMilliseconds);
			SWatch.Restart();

			if (PrintWatch.ElapsedMilliseconds > 2000) {
				PrintWatch.Restart();

				PrintStats();
			}
		}
	}
}
