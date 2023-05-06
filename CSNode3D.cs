/// Copyright (C) 2023 - Nick S. All Rights Reserved.  ///
/// -------------------------------------------------- ///
/// This file is made public for the purpose of making ///
/// Life easier for the Godot C# community. This file  ///
/// is not to be sold or used for commercial purposes  ///
/// without attribution and credit to the author.      ///
/// -------------------------------------------------- ///

using System.Reflection;
using System.Collections.Generic;
using Godot;
using System;

namespace Extensions
{
	public partial class CSNode3D : Node3D
	{
		/// <summary>
		/// Finds a child node by path. Given the type of the node, it will return null if the node is not of that type.
		/// </summary>
		/// <typeparam name="T">The node's type.</typeparam>
		/// <param name="path">The pattern to find this node.</param>
		/// <returns>The node or null.</returns>
		public T FindChild<T>(string path) where T : Node3D
		{
			return FindChild(path, false) as T;
		}

		/// <summary>
		/// The position along the "left" axis relative to the orientation of the object.
		/// </summary>
		public double X { get => Position.Y; set => Position = new Vector3((float)value, Position.Y, Position.Z); }

		/// <summary>
		/// The position along the "Up" axis relative to the orientation of the object.
		/// </summary>
		public double Y { get => Position.Y; set => Position = new Vector3(Position.X, (float)value, Position.Y); }

		/// <summary>
		/// The position along the "Forward" axis relative to the orientation of the object.
		/// </summary>
		public double Z { get => Position.Z; set => Position = new Vector3(Position.X, Position.Y, (float)value); }

		/// <summary>
		/// The rotation along the "left" axis relative to the orientation of the object. The makes it rotate up and down.
		/// </summary>
		public double Pitch { get => RotationDegrees.X; set => RotationDegrees = new Vector3((float)value, RotationDegrees.Y, RotationDegrees.Z); }
		/// <summary>
		/// The rotation along the "Up" axis relative to the orientation of the object. The makes it rotate left and right.
		/// </summary>
		public double Yaw { get => RotationDegrees.Y; set => RotationDegrees = new Vector3(RotationDegrees.X, (float)value, RotationDegrees.Z); }

		/// <summary>
		/// The rotation along the "Forward" axis relative to the orientation of the object. The makes it rotate clockwise and counter-clockwise.
		/// </summary>
		public double Roll { get => RotationDegrees.Z; set => RotationDegrees = new Vector3(RotationDegrees.X, RotationDegrees.Y, (float)value); }

		public Vector3 Left { get => -Transform.Basis.X; }
		public Vector3 Up { get => Transform.Basis.Y; }
		public Vector3 Forward { get => Transform.Basis.Z; }

		[System.AttributeUsage(System.AttributeTargets.Method)]
		public partial class OnProcessAttribute : System.Attribute
		{
			public readonly int Priority;
			public OnProcessAttribute() : this(0) { }
			public OnProcessAttribute(int priority)
			{
				Priority = priority;
			}
		}

		[System.AttributeUsage(System.AttributeTargets.Method)]
		public partial class OnReadyAttribute : System.Attribute
		{
			public OnReadyAttribute() { }
		}

		private List<MethodInfo> _processMethods = new();
		public override void _Ready()
		{
			_processMethods = new();

			Assembly assembly = Assembly.GetExecutingAssembly();
			foreach (Type type in assembly.GetTypes())
			{
				foreach (MethodInfo method in type.GetMethods())
				{
					OnProcessAttribute onProcess = method.GetCustomAttribute<OnProcessAttribute>();
					if (onProcess == null) continue;
					if (method.GetParameters().Length > 1)
					{
						GD.PrintErr($"[{type}]: OnProcessAttribute only works with methods that have no parameters or one parameter.");
						continue;
					}

					GD.Print($"[{type}]: Found OnProcessAttribute on {method.Name} with priority {onProcess.Priority}.");
					_processMethods.Add(method);
				}
			}

			foreach (MethodInfo method in GetType().GetMethods())
			{
				if (method.GetCustomAttribute<OnReadyAttribute>() != null)
				{
					GD.Print($"[{GetType()}]: Found OnReadyAttribute on {method.Name}.");
					method.Invoke(this, null);
				}
			}

			base._Ready();
		}
		
		public override void _Process(double delta)
		{
			foreach (MethodInfo method in _processMethods)
			{
				method.Invoke(this, new object[] { delta });
			}

			base._Process(delta);
		}
	}
}
