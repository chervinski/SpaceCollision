using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

namespace SpaceCollision
{
	public class Space
	{
		private XmlSerializer formatter = new XmlSerializer(typeof(List<SpaceObject>));
		public List<SpaceObject> Objects { get; set; } = new List<SpaceObject>();
		public const double G = 6.674e-11;
		public const double NotHappened = -1;
		/// <summary>
		///		Calculates the time until the first collision (the following behavior is unknown)
		///		and time when an orbit was detected (works only for 2 objects).
		/// </summary>
		/// <param name="max_time">maximal time of the simulation</param>
		/// <param name="dt">time step (less = slower works, but more accurate result)</param>
		/// <param name="collided">pairs of the identifiers of collided objects (unlikely to be more than 1)</param>
		/// <param name="orbit_deteced_time">time when orbit was detected or <see cref="NotHappened"/></param>
		/// <param name="plus_percent">what to do when the percentage of the work done increased</param>
		/// <returns>the time until the first collision or <see cref="NotHappened"/></returns>
		public double Simulate(double max_time, double dt, out int[][] collided, out double orbit_deteced_time, Action<int> plus_percent)
		{
			SpaceObject[] objects = new SpaceObject[Objects.Count];
			for (int i = 0; i < objects.Length; ++i)
				objects[i] = Objects[i].Clone() as SpaceObject;

			collided = null;
			orbit_deteced_time = NotHappened;
			double passed_time = 0;
			int percent_done = 0;

			if (objects.Length == 0 || objects.Length == 1)
				return NotHappened;

			/*****ORBIT DETECTION PART*****/
			bool detect_orbit = objects.Length == 2;
			double prev_dist = objects[0].Position.DistanceTo(objects[1].Position),
				dist_changes = 0;   // if it happens 3 times, it is in orbit
			bool? approching = null;// (approching-leaving-approching OR leaving-approching-leaving)
			/******************************/

			for (; passed_time != max_time; passed_time += dt)
			{
				if (passed_time > max_time) // time is not a multiple of dt
				{
					dt -= passed_time - max_time;
					passed_time = max_time;
				}

				foreach (SpaceObject i in objects)
					i.GravityMove(objects, dt);

				if ((collided = Collided()) != null)
					return passed_time;

				/*****ORBIT DETECTION PART*****/
				if (detect_orbit && dist_changes != 3)
				{
					double current_dist = objects[0].Position.DistanceTo(objects[1].Position);

					if (prev_dist == current_dist && (objects[0].Speed > 0 || objects[1].Speed > 0))
					{ // absolute circular orbit (unlikely)
						dist_changes = 3; // instant conclusion
						orbit_deteced_time = passed_time;
					}
					else if (approching != null && (
							approching == true && prev_dist - current_dist < 0 ||
							approching == false && prev_dist - current_dist > 0))
					{
						approching = approching == true ? false : true;
						if (++dist_changes == 3)
							orbit_deteced_time = passed_time;
					}
					else if (approching == null) // the first time
						approching = prev_dist - current_dist > 0;
				}
				/******************************/

				if ((int)(passed_time / max_time * 100) > percent_done) // to display the current situation
					plus_percent(percent_done = (int)(passed_time / max_time * 100));
			}
			return NotHappened;
		}
		/// <summary>Calculates the time until the first collision not taking into account gravity</summary>
		/// <returns>the time until the first collision or <see cref="NotHappened"/></returns>
		public double SimulateWithoutGravity(double max_time, out int[][] collided)
		{
			List<int[]> list_collided = new List<int[]>();
			double min_time = NotHappened, cur_time;

			for (int i = 0; i < Objects.Count - 1; i++)
				for (int j = i + 1; j < Objects.Count; j++)
				{
					Console.WriteLine($"{i} {j}");
					if ((cur_time = TimeUntillCollision(i, j, max_time)) != NotHappened &&
						(min_time == NotHappened || cur_time <= min_time))
					{
						if (cur_time < min_time)
							list_collided.Clear();
						min_time = cur_time;
						list_collided.Add(new int[] { i, j });
					}
				}

			collided = list_collided.Count > 0 ? list_collided.ToArray() : null;
			return min_time;
		}
		/// <summary>Finds the collided objects in the current state.</summary>
		/// <returns>array[pairs][2] of their identifiers (unlikely to be more than 1 pair).</returns>
		private int[][] Collided()
		{
			List<int[]> collided = new List<int[]>();
			for (int i = 0; i < Objects.Count - 1; i++)
				for (int j = i + 1; j < Objects.Count; j++)
					if (Objects[i].Position.DistanceTo(Objects[j].Position) <= Objects[i].Radius + Objects[j].Radius)
						collided.Add(new int[] { i, j });
			return collided.Count > 0 ? collided.ToArray() : null;
		}
		/// <summary>Calculates the time until a collision between 2 objects (identifiers in the array) with some precision.</summary>
		/// <returns>the time until a collision or <see cref="NotHappened"/></returns>
		private double TimeUntillCollision(int ia, int ib, double max_time, double precision = 1e-10)
		{
			SpaceObject a = Objects[ia], b = Objects[ib];
			double time, collision_dist = a.Radius + b.Radius, last_dist = -1;// -1 to assign the value the first time

			if (a.Position.DistanceTo(b.Position) <= collision_dist)
				return 0; // already in the collision zone
			if (a.Speed == 0 && b.Speed == 0)
				return NotHappened;
			if (a.Speed == 0 || b.Speed == 0)
			{
				SpaceObject temp = a;
				a = b;
				b = temp;
			}

			Point a_move, b_move,
				a_end = a.Move(max_time),
				b_end = b.Move(max_time),
				left = a.Position, right = a_end;

			while (true)
			{
				a_move = left.Middle(right);
				time = a_move.DistanceTo(a.Position) / a.Speed;
				b_move = b.Position.Move(b_end, b.Speed * time); // movement of another object in the same time
				double cur_dist = a_move.DistanceTo(b_move);

				if (cur_dist + precision >= collision_dist && // they ~touched with their surfaces
					cur_dist - precision <= collision_dist &&
					(a.Position.DistanceTo(a_move) < a.Position.DistanceTo(b_move) || // and it's not the opposite side
					b.Radius + precision >= 0 && // but if radius ~= 0, then ok
					b.Radius - precision <= 0))
					return time;

				if (last_dist == (last_dist = left.DistanceTo(right)))
					return NotHappened;
				if (cur_dist > collision_dist && a.Position.DistanceTo(a_move) < a.Position.DistanceTo(b_move))
					left = a_move;
				else right = a_move;
			}
		}
		public void Save(string file)
		{
			using (FileStream fs = new FileStream(file, FileMode.OpenOrCreate))
			{
				formatter.Serialize(fs, Objects);
			}
		}
		public void Load(string file)
		{
			using (FileStream fs = new FileStream(file, FileMode.Open))
			{
				Objects = formatter.Deserialize(fs) as List<SpaceObject>;
			}
		}
	}
}
