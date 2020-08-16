using System;

namespace SpaceCollision
{
	[Serializable]
	public class Point : ICloneable
	{
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		public Point() { }
		public Point(double x, double y, double z)
		{
			X = x; Y = y; Z = z;
		}
		public Point(string x, string y, string z)
		{
			Console.Write(x);
			X = double.Parse(Console.ReadLine());
			Console.Write(y);
			Y = double.Parse(Console.ReadLine());
			Console.Write(z);
			Z = double.Parse(Console.ReadLine());
		}
		/// <summary>Calculates the distance to another point.</summary>
		public double DistanceTo(Point a)
		{
			return Math.Sqrt(
				Math.Pow(a.X - X, 2) +
				Math.Pow(a.Y - Y, 2) +
				Math.Pow(a.Z - Z, 2));
		}
		/// <summary>Finds the middle point between this and another point.</summary>
		public Point Middle(Point a)
		{
			return new Point(
				(X + a.X) / 2,
				(Y + a.Y) / 2,
				(Z + a.Z) / 2);
		}
		/// <summary>
		///		Finds the point with some precision between this point and
		///		another one located on the distance from this point.
		/// </summary>
		public Point Move(Point a, double distance, double precision = 1e-10)
		{
			Point target, left = this, right = a;
			while (true)
			{
				double cur_dist = DistanceTo(target = left.Middle(right));
				if (cur_dist + precision >= distance &&
					cur_dist - precision <= distance)
					return target;
				if (cur_dist < distance)
					left = target;
				else right = target;
			}
		}
		public object Clone()
		{
			return new Point(X, Y, Z);
		}
		public override string ToString()
		{
			return $"({X}; {Y}; {Z})";
		}
	}
}
