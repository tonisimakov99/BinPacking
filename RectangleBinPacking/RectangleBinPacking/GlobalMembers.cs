//using System;
//using System.Collections.Generic;

//public static class GlobalMembers
//{
//	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//	//ORIGINAL LINE: #define debug_assert(x) assert(x)
//	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//	//ORIGINAL LINE: #define debug_assert(x)


//	static void Main(int argc, string[] args)
//	{

//		if (argc < 5 || argc % 2 != 1)
//		{
//			Console.Write("Usage: MaxRectsBinPackTest binWidth binHeight w_0 h_0 w_1 h_1 w_2 h_2 ... w_n h_n\n");
//			Console.Write("       where binWidth and binHeight define the size of the bin.\n");
//			Console.Write("       w_i is the width of the i'th rectangle to pack, and h_i the height.\n");
//			Console.Write("Example: MaxRectsBinPackTest 256 256 30 20 50 20 10 80 90 20\n");
//		}


//		// Create a bin to pack to, use the bin size from command line.
//		MaxRectsBinPack bin = new MaxRectsBinPack();
//		int binWidth = Convert.ToInt32(args[1]);
//		int binHeight = Convert.ToInt32(args[2]);
//		Console.Write("Initializing bin to size {0:D}x{1:D}.\n", binWidth, binHeight);
//		bin.Init(binWidth, binHeight);

//		// Pack each rectangle (w_i, h_i) the user inputted on the command line.
//		for (int i = 3; i < argc; i += 2)
//		{
//			// Read next rectangle to pack.
//			int rectWidth = Convert.ToInt32(args[i]);
//			int rectHeight = Convert.ToInt32(args[i + 1]);
//			Console.Write("Packing rectangle of size {0:D}x{1:D}: ", rectWidth, rectHeight);

//			// Perform the packing.
//			MaxRectsBinPack.FreeRectChoiceHeuristic heuristic = MaxRectsBinPack.RectBestShortSideFit; // This can be changed individually even for each rectangle packed.
//			Rect packedRect = bin.Insert(rectWidth, rectHeight, heuristic);

//			// Test success or failure.
//			if (packedRect.height > 0)
//			{
//				Console.Write("Packed to (x,y)=({0:D},{1:D}), (w,h)=({2:D},{3:D}). Free space left: {4:f2}%\n", packedRect.x, packedRect.y, packedRect.width, packedRect.height, 100.0f - bin.Occupancy() * 100.0f);
//			}
//			else
//			{
//				Console.Write("Failed! Could not find a proper position to pack this rectangle into. Skipping this one.\n");
//			}
//		}
//		Console.Write("Done. All rectangles packed.\n");
//	}


//	public static uint64_t tick()
//	{
//	#if WIN32
//		LARGE_INTEGER ddwTimer = new LARGE_INTEGER();
//		bool success = QueryPerformanceCounter(ddwTimer);
//		assume(success != false);
//		MARK_UNUSED(success);
//		return ddwTimer.QuadPart;
//	#elif __APPLE__
//		return mach_absolute_time();
//	#elif _POSIX_MONOTONIC_CLOCK
//		timespec t = new timespec();
//		clock_gettime(CLOCK_MONOTONIC, t);
//		return t.tv_sec * 1000 * 1000 * 1000 + t.tv_nsec;
//	#elif _POSIX_C_SOURCE
//		timeval t = new timeval();
//		gettimeofday(t, null);
//		return t.tv_sec * 1000 * 1000 + t.tv_usec;
//	#else
//		return clock();
//	#endif
//	}

//	public static uint64_t ticksPerSec()
//	{
//	#if WIN32
//		uint64_t ddwTimerFrequency = new uint64_t();
////C++ TO C# CONVERTER TODO TASK: There is no equivalent to 'reinterpret_cast' in C#:
//		QueryPerformanceFrequency(reinterpret_cast<LARGE_INTEGER>(ddwTimerFrequency));
//		return new uint64_t(ddwTimerFrequency);
//	#elif __APPLE__
//		mach_timebase_info_data_t timeBaseInfo = new mach_timebase_info_data_t();
//		mach_timebase_info(timeBaseInfo);
//		return 1000000000UL * (uint64_t)timeBaseInfo.denom / (uint64_t)timeBaseInfo.numer;
//	#elif _POSIX_MONOTONIC_CLOCK
//		return 1000 * 1000 * 1000;
//	#elif _POSIX_C_SOURCE || __APPLE__
//		return 1000 * 1000;
//	#else
//		return CLOCKS_PER_SEC;
//	#endif
//	}

//	public static bool AreDisjoint(Rect a, Rect b)
//	{
//		return a.x >= b.x + b.width || a.x + a.width <= b.x || a.y >= b.y + b.height || a.y + a.height <= b.y;
//	}

//	public static bool AllRectsDisjoint(List<Rect> packed)
//	{
//		for (size_t i = 0; i < packed.Count; ++i)
//		{
//			for (size_t j = i + 1; j < packed.Count; ++j)
//			{
//				if (!AreDisjoint(packed[i], packed[j]))
//				{
//					return false;
//				}
//			}
//		}
//		return true;
//	}

//	static int Main()
//	{
//		MaxRectsBinPack pack = new MaxRectsBinPack(1024 * 8, 1024 * 8, true);

//		List<Rect> packed = new List<Rect>();
//		RandomNumbers.Seed(12412);
//		uint64_t t0 = tick();
//		for (int i = 1; i < 1024 * 1024; ++i)
//		{
//			int a = (RandomNumbers.NextNumber() % 128) + 1;
//			int b = (RandomNumbers.NextNumber() % 128) + 1;
//			Rect r = pack.Insert(a, b, MaxRectsBinPack.RectBestShortSideFit);
//			if (r.width == 0)
//			{
//				break;
//			}
//			packed.Add(r);
//		}
//		uint64_t t1 = tick();
//		Console.Write("Packing {0:D} rectangles took {1:f} msecs. All rects disjoint: {2}. Occupancy: {3:f}\n", (int)packed.Count, (t1 - t0) * 1000.0 / ticksPerSec(), AllRectsDisjoint(packed) ? "yes" : "NO!", pack.Occupancy());
//	}


////C++ TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
//	public static BOOST_PYTHON_MODULE(boost_binpacker_ext UnnamedParameter)
//	{

//		class_<Rect>("Rect").add_property("x", Rect.x).add_property("y", Rect.y).add_property("width", Rect.width).add_property("height", Rect.height);

//		// MaxRectBinpack
//		enum_<MaxRectsBinPack.FreeRectChoiceHeuristic>("MaxRectsFreeRectChoiceHeuristic").value("RectBestShortSideFit", MaxRectsBinPack.RectBestShortSideFit).value("RectBestLongSideFit", MaxRectsBinPack.RectBestLongSideFit).value("RectBestAreaFit", MaxRectsBinPack.RectBestAreaFit).value("RectBottomLeftRule", MaxRectsBinPack.RectBottomLeftRule).value("RectContactPointRule", MaxRectsBinPack.RectContactPointRule);

//		MaxRectsInsertDelegate MaxRectsInsert = MaxRectsBinPack.Insert;

//		class_<MaxRectsBinPack>("MaxRectsBinPack", init<int, int>()).def(init<>()).def("init", MaxRectsBinPack.Init).def("insert", MaxRectsInsert).def("occupancy", MaxRectsBinPack.Occupancy);

//		// GuillotineBinPack
//		enum_<GuillotineBinPack.FreeRectChoiceHeuristic>("GuillotineFreeRectChoiceHeuristic").value("RectBestAreaFit", GuillotineBinPack.RectBestAreaFit).value("RectBestShortSideFit", GuillotineBinPack.RectBestShortSideFit).value("RectBestLongSideFit", GuillotineBinPack.RectBestLongSideFit).value("RectWorstAreaFit", GuillotineBinPack.RectWorstAreaFit).value("RectWorstShortSideFit", GuillotineBinPack.RectWorstShortSideFit).value("RectWorstLongSideFit", GuillotineBinPack.RectWorstLongSideFit);
//		enum_<GuillotineBinPack.GuillotineSplitHeuristic>("GuillotineSplitHeuristic").value("SplitShorterLeftoverAxis", GuillotineBinPack.SplitShorterLeftoverAxis).value("SplitLongerLeftoverAxis", GuillotineBinPack.SplitLongerLeftoverAxis).value("SplitMinimizeArea", GuillotineBinPack.SplitMinimizeArea).value("SplitMaximizeArea", GuillotineBinPack.SplitMaximizeArea).value("SplitShorterAxis", GuillotineBinPack.SplitShorterAxis).value("SplitLongerAxis", GuillotineBinPack.SplitLongerAxis);

//		GuillotineInsertDelegate GuillotineInsert = GuillotineBinPack.Insert;

//		class_<GuillotineBinPack>("GuillotineBinPack", init<int, int>()).def(init<>()).def("init", GuillotineBinPack.Init).def("insert", GuillotineInsert).def("occupancy", GuillotineBinPack.Occupancy);

//		// ShelfBinPack
//		enum_<ShelfBinPack.ShelfChoiceHeuristic>("ShelfChoiceHeuristic").value("ShelfNextFit", ShelfBinPack.ShelfNextFit).value("ShelfFirstFit", ShelfBinPack.ShelfFirstFit).value("ShelfBestAreaFit", ShelfBinPack.ShelfBestAreaFit).value("ShelfWorstAreaFit", ShelfBinPack.ShelfWorstAreaFit).value("ShelfBestHeightFit", ShelfBinPack.ShelfBestHeightFit).value("ShelfBestWidthFit", ShelfBinPack.ShelfBestWidthFit).value("ShelfWorstWidthFit", ShelfBinPack.ShelfWorstWidthFit);

//		class_<ShelfBinPack>("ShelfBinPack", init<int, int, bool>()).def(init<>()).def("init", ShelfBinPack.Init).def("insert", ShelfBinPack.Insert).def("occupancy", ShelfBinPack.Occupancy);

//		// SkylineBinPack
//		  enum_<SkylineBinPack.LevelChoiceHeuristic>("SkylineLevelChoiceHeuristic").value("LevelBottomLeft", SkylineBinPack.LevelBottomLeft).value("LevelMinWasteFit", SkylineBinPack.LevelMinWasteFit);

//		SkylineInsertDelegate SkylineInsert = SkylineBinPack.Insert;

//		class_<SkylineBinPack>("SkylineBinPack", init<int, int, bool>()).def(init<>()).def("init", SkylineBinPack.Init).def("insert", SkylineInsert).def("occupancy", SkylineBinPack.Occupancy);
//	}

//	 public delegate Rect MaxRectsInsertDelegate(int UnnamedParameter, int UnnamedParameter2, MaxRectsBinPack.FreeRectChoiceHeuristic UnnamedParameter3);

//	 public delegate Rect GuillotineInsertDelegate(int UnnamedParameter, int UnnamedParameter2, bool UnnamedParameter3, GuillotineBinPack.FreeRectChoiceHeuristic UnnamedParameter4, GuillotineBinPack.GuillotineSplitHeuristic UnnamedParameter5);

//	 public delegate Rect SkylineInsertDelegate(int UnnamedParameter, int UnnamedParameter2, SkylineBinPack.LevelChoiceHeuristic UnnamedParameter3);

//}

//namespace rbp
//{
//	public static class GlobalMembers
//	{
//	/// @return True if r fits inside freeRect (possibly rotated).
//	public static bool Fits(RectSize r, Rect freeRect)
//	{
//		return (r.width <= freeRect.width && r.height <= freeRect.height) || (r.height <= freeRect.width && r.width <= freeRect.height);
//	}

//	/// @return True if r fits perfectly inside freeRect, i.e. the leftover area is 0.
//	public static bool FitsPerfectly(RectSize r, Rect freeRect)
//	{
//		return (r.width == freeRect.width && r.height == freeRect.height) || (r.height == freeRect.width && r.width == freeRect.height);
//	}


//	/// Returns 0 if the two intervals i1 and i2 are disjoint, or the length of their overlap otherwise.
//	public static int CommonIntervalLength(int i1start, int i1end, int i2start, int i2end)
//	{
//		if (i1end < i2start || i2end < i1start)
//		{
//			return 0;
//		}
//		return Math.Min(i1end, i2end) - Math.Max(i1start, i2start);
//	}

//	/// Performs a lexicographic compare on (rect short side, rect long side).
//	/// @return -1 if the smaller side of a is shorter than the smaller side of b, 1 if the other way around.
//	///   If they are equal, the larger side length is used as a tie-breaker.
//	///   If the rectangles are of same size, returns 0.
////C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
//	//int CompareRectShortSide(Rect a, Rect b);

//	/// Performs a lexicographic compare on (x, y, width, height).
////C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
//	//int NodeSortCmp(Rect a, Rect b);

///*
//#include "clb/Algorithm/Sort.h"

//int CompareRectShortSide(const Rect &a, const Rect &b)
//{
//	using namespace std;

//	int smallerSideA = min(a.width, a.height);
//	int smallerSideB = min(b.width, b.height);

//	if (smallerSideA != smallerSideB)
//		return clb::sort::TriCmp(smallerSideA, smallerSideB);

//	// Tie-break on the larger side.
//	int largerSideA = max(a.width, a.height);
//	int largerSideB = max(b.width, b.height);

//	return clb::sort::TriCmp(largerSideA, largerSideB);
//}
//*/
///*
//int NodeSortCmp(const Rect &a, const Rect &b)
//{
//	if (a.x != b.x)
//		return clb::sort::TriCmp(a.x, b.x);
//	if (a.y != b.y)
//		return clb::sort::TriCmp(a.y, b.y);
//	if (a.width != b.width)
//		return clb::sort::TriCmp(a.width, b.width);
//	return clb::sort::TriCmp(a.height, b.height);
//}
//*/

//	/// Returns true if a is contained in b.
//	public static bool IsContainedIn(Rect a, Rect b)
//	{
//		return a.x >= b.x && a.y >= b.y && a.x + a.width <= b.x + b.width && a.y + a.height <= b.y + b.height;
//	}
//	}
//}