using System;
using scalar = System.Int64;

namespace DecimalNavigation
{
	/// <summary>
	/// Represents an 2D axis aligned bounding box (AABB).
	/// </summary>
	/// \ingroup MathAPI
	[Serializable]
	public struct AABB2D
	{
		public Point2D Min;
		public Point2D Max;

		public readonly Point2D Center => (Min + Max) / 2;

		internal readonly scalar Perimeter =>
			(Max.X - Min.X) * (Max.Y - Min.Y);


		public bool Contains(in Point2D point)
		{
			return Min.X <= point.X && point.X <= Max.X &&
				   Min.Y <= point.Y && point.Y <= Max.Y;
		}

		public static void CreateMerged(in AABB2D original, in AABB2D additional, out AABB2D result)
		{
			result.Min = Point2D.Min(original.Min, additional.Min);
			result.Max = Point2D.Max(original.Max, additional.Max);
		}

		public readonly bool NotDisjoint(in AABB2D box)
		{
			return Max.X >= box.Min.X && Min.X <= box.Max.X && Max.Y >= box.Min.Y && Min.Y <= box.Max.Y;
		}

		public readonly bool Disjoint(in AABB2D box)
		{
			return !(Max.X >= box.Min.X && Min.X <= box.Max.X && Max.Y >= box.Min.Y && Min.Y <= box.Max.Y);
		}
	}
}
