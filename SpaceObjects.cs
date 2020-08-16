namespace SpaceCollision
{
	public class Comet : SpaceObject { }
	public class Asteroid : SpaceObject { }
	public class Planet : SpaceObject { }
	public class Sun : SpaceObject
	{
		public Sun()
		{
			Mass = 1.989e30;
			Radius = 696340000;
		}
	}
	public class BlackHole : SpaceObject { }
	public class FreeObject : SpaceObject { }
}
