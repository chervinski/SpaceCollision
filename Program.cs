using System;

namespace SpaceCollision
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			Space space = new Space();
			while (true)
			{
				Console.WriteLine("\t\tSpace Collision\n\nSimulate\n\nAdd object\n\nRemove object\n\nClear all\n\nSave objects\n\nLoad objects\n\n\n");
				Display(space);
				try
				{
					switch (Choose(6, 14, 2, 1))
					{
						case 1: Simulate(space); break;
						case 2: Add(space); break;
						case 3: Remove(space); break;
						case 4: space.Objects.Clear(); break;
						case 5: space.Save(GetFileName()); break;
						case 6: space.Load(GetFileName()); break;
						default: return;
					}
				}
				catch (Exception ex)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine(Environment.NewLine + ex.Message);
					Console.ForegroundColor = ConsoleColor.Gray;
					Pause();
				}
				Console.Clear();
			}
		}
		public static void Display(Space space)
		{
			Console.WriteLine($"Number of the space objects: {space.Objects.Count}");
			int i = 0;
			foreach (var o in space.Objects)
			{
				Console.WriteLine($"\n#{++i} {o}");
				Console.WriteLine("\tPosition: " + o.Position);
				Console.WriteLine("\tVelocity: " + o.Velocity);
				Console.WriteLine("\tRadius: " + o.Radius);
				Console.WriteLine("\tMass: " + o.Mass);
			}
		}
		public static void Simulate(Space space)
		{
			/*****WHETHER TO TAKE INTO ACCOUNT GRAVITY OR NOT*****/
			string[] options = new string[] { "Yes", "No" };
			if (space.Objects.Count != 2)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Orbit detection will not work because the number of objects is not 2");
			} else {
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Orbit detection will work because the number of objects is 2.");
			}
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine("Take into account gravity?\n");
			int use_gravity = Choose(options, 3, 1);
			if (use_gravity == 0) return;
			/****************************************************/

			/*****TIME SETTING*****/
			Console.Write("Simulation time (s): ");
			double dt = 0, time = double.Parse(Console.ReadLine());
			if (time < 0) throw new ArgumentOutOfRangeException();
			if (use_gravity == 1)
			{
				Console.Write("Timestep (s) (should be short): ");
				dt = double.Parse(Console.ReadLine());
				if (dt < 0) throw new ArgumentOutOfRangeException();
			}
			Console.Clear();
			/**********************/

			/*****CALCULATION*****/
			int[][] collided;
			double orbit_deteced_time = 0;
			time = use_gravity == 1 ?
				time = space.Simulate(time, dt, out collided, out orbit_deteced_time, (p) => Console.Write($"\r{p}%")) :
				time = space.SimulateWithoutGravity(time, out collided);
			Console.Clear();
			/********************/

			/*****RESULTS*****/
			if (use_gravity == 1 && space.Objects.Count == 2)
			{
				if (orbit_deteced_time == Space.NotHappened)
					Console.WriteLine("No orbits were detected.");
				else
				{
					Console.Write($"In {orbit_deteced_time} s was detected that ");
					if (space.Objects[0].Mass < space.Objects[1].Mass)
						Console.WriteLine($"#1 {space.Objects[0]} enters orbit of #2 {space.Objects[1]}.");
					else if (space.Objects[0].Mass > space.Objects[1].Mass)
						Console.WriteLine($"#2 {space.Objects[1]} enters orbit of #1 {space.Objects[0]}.");
					else
						Console.WriteLine("these objects (with equal mass) enter one orbit.");
				}
			}

			if (time == Space.NotHappened)
				Console.WriteLine("No collisions were detected.");
			else
			{
				Console.WriteLine($"The first collision in {time} s between:");
				for (int i = 0; i < collided.GetLength(0); ++i) // unlikely to be more than 1 at the same moment
					Console.WriteLine("#{0} {1} and #{2} {3}",
						collided[i][0] + 1, space.Objects[collided[i][0]],
						collided[i][1] + 1, space.Objects[collided[i][1]]);
			}
			Console.WriteLine();
			Pause();
			/*****************/
		}
		public static void Add(Space space)
		{	
			Type[] types = new Type[] { typeof(Comet), typeof(Asteroid), typeof(Planet), typeof(Sun), typeof(BlackHole), typeof(FreeObject) };
			string[] type_names = new string[types.Length];
			for (int i = 0; i < types.Length; ++i)
				type_names[i] = types[i].Name;

			Console.WriteLine("Type of object:\n");
			int choice = Choose(type_names, 2, 1);
			if (choice-- == 0) return;
			SpaceObject space_object = types[choice].GetConstructor(Type.EmptyTypes).Invoke(Type.EmptyTypes) as SpaceObject;

			if (space_object.Position == default(Point)) // maybe a constructor has already written the values of these fields
			{
				Console.WriteLine("Start position (m):");
				space_object.Position = new Point("\tx: ", "\ty: ", "\tz: ");
			}
			if (space_object.Velocity == default(Point))
			{
				Console.WriteLine("\nVelocity (m/s):");
				space_object.Velocity = new Point("\tVx: ", "\tVy: ", "\tVz: ");
			}
			if (space_object.Radius == default(double))
			{
				Console.Write("\nRadius (m): ");
				space_object.Radius = double.Parse(Console.ReadLine());
			}
			if (space_object.Mass == default(double))
			{
				Console.Write("\nMass (kg): ");
				space_object.Mass = double.Parse(Console.ReadLine());
			}

			space.Objects.Add(space_object);
		}
		public static void Remove(Space space)
		{
			if (space.Objects.Count == 0) return;
			string[] options = new string[space.Objects.Count];
			int i = 0;
			foreach (var o in space.Objects)
				options[i] = $"#{++i} {o}";

			Console.WriteLine("Object to remove:");
			int choice = Choose(options, 2, 1);
			if (choice-- == 0) return;
			space.Objects.RemoveAt(choice);
		}
		public static string GetFileName()
		{
			Console.Write("The file name: ");
			return Console.ReadLine();
		}
		/// <summary>Simplified version of <see cref="Choose(int, int, int, int)"/> which also prints options.</summary>
		/// <param name="options">options to print</param>
		/// <param name="y_start">start y coordinate of the cursor</param>
		/// <param name="space">lines between options</param>
		/// <returns>the picked option or 0 if the user pressed ESC.</returns>
		public static int Choose(string[] options, int y_start, int space)
		{
			int max_length = 0;
			Console.CursorTop = y_start;
			foreach (string option in options)
			{
				Console.Write(option);
				if (option.Length > max_length)
					max_length = option.Length;
				for (int i = 0; i < space + 1; ++i)
					Console.WriteLine();
			}
			return Choose(options.Length, max_length + 1, y_start, space);
		}
		/// <summary>Makes a user to pick some option (printed before) with the keyboard arrows in the console.</summary>
		/// <param name="n_options">number of the options</param>
		/// <param name="x">x coordinate of the cursor</param>
		/// <param name="y_start">start y coordinate of the cursor</param>
		/// <param name="space">lines between options</param>
		/// <returns>the picked option or 0 if the user pressed ESC.</returns>
		/// <seealso cref="Choose(string[], int, int)"/>
		public static int Choose(int n_options, int x, int y_start, int space)
		{
			const string arrow = "<-", remove_arrow = "  ";
			int y = y_start, last_line = y_start + ((n_options - 1) * (space + 1)), option = 1;
			ConsoleKey key;

			Console.CursorVisible = false;
			Console.SetCursorPosition(x, y);
			Console.Write(arrow);

			while (true)
			{
				key = Console.ReadKey(true).Key;
				if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow
					|| key == ConsoleKey.Enter || key == ConsoleKey.Escape)
				{
					Console.SetCursorPosition(x, y);
					Console.Write(remove_arrow);

					if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow)
					{
						if (key == ConsoleKey.UpArrow)
						{
							if (y > y_start)
							{
								--option;
								y -= space + 1;
							}
							else
							{
								option = n_options;
								y = last_line;
							}
						}
						else
						{
							if (y < last_line)
							{
								++option;
								y += space + 1;
							}
							else
							{
								option = 1;
								y = y_start;
							}
						}
						Console.SetCursorPosition(x, y);
						Console.Write(arrow);
					}
					else
					{
						Console.CursorVisible = true;
						Console.SetCursorPosition(0, 0);
						Console.Clear();
						return key == ConsoleKey.Enter ? option : 0;
					}
				}
			}
		}
		public static void Pause()
		{
			Console.Write("Press any key to continue . . .");
			Console.ReadKey(true);
		}
	}
}
