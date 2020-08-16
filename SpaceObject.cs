using System;
using System.Xml.Serialization;

namespace SpaceCollision
{
	[Serializable]
	[XmlInclude(typeof(Comet))]
	[XmlInclude(typeof(Asteroid))]
	[XmlInclude(typeof(Planet))]
	[XmlInclude(typeof(Sun))]
	[XmlInclude(typeof(BlackHole))]
	[XmlInclude(typeof(FreeObject))]
	public class SpaceObject : ICloneable
	{
		private double r, m;
		public Point Position { get; set; }
		public Point Velocity { get; set; }
		public double Radius
		{
			get { return r; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException();
				r = value;
			}
		}
		public double Mass
		{
			get { return m; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException();
				m = value;
			}
		}
		public double Speed
		{
			get
			{
				return Math.Sqrt(
					Velocity.X * Velocity.X + 
					Velocity.Y * Velocity.Y + 
					Velocity.Z * Velocity.Z);
			}
		}
		/// <summary>
		///		Moves this object in some time taking into account
		///		the gravity of other objects (excluding this object).
		/// </summary>
		/// <remarks><b><i>The time intervals should be very short to make an accurate line movement.</i></b></remarks>
		public void GravityMove(SpaceObject[] objects, double t)
		{
			foreach (SpaceObject i in objects)
				if (this != i)
				{
					double d = Position.DistanceTo(i.Position),
						g = Space.G * i.Mass / (d * d); // gravitational acceleration
					Velocity.X += g * t * ((i.Position.X - Position.X) / d);
					Velocity.Y += g * t * ((i.Position.Y - Position.Y) / d);
					Velocity.Z += g * t * ((i.Position.Z - Position.Z) / d);
				}
			Position = Move(t);
		}
		/// <summary>Finds a new position of this object in some time.</summary>
		/// <returns>its new position</returns>
		public Point Move(double t)
		{
			return new Point(
				Position.X + Velocity.X * t,
				Position.Y + Velocity.Y * t,
				Position.Z + Velocity.Z * t);
		}
		public object Clone()
		{
			return new SpaceObject() {
				Position = this.Position.Clone() as Point,
				Velocity = this.Velocity.Clone() as Point,
				Mass = this.Mass,
				Radius = this.Radius};
		}
		public override string ToString()
		{
			return GetType().Name;
		}
	}
}
