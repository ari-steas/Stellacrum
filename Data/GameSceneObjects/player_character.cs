using Godot;
using System;

public partial class player_character : CharacterBody3D
{
	GameScene scene;


	#region movement

	public const float Speed = 1.0f;
	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	private float lastX = 0, lastY = 0, lastZ = 0;
	private bool _dampenersEnabled = true;

	#endregion


	private long nextPlaceTime;	

	private SpotLight3D light;
	private Camera3D camera;
	private hud_scene HUD;

	public PlaceBox PlayerPlaceBox { get; private set; }
	public Vector3 lookPosition;

	public RayCast3D interactCast;

    public override void _Ready()
    {
		scene = GetParent<GameScene>();

        light = GetNode<SpotLight3D>("SpotLight3D");
		camera = GetNode<Camera3D>("PlayerCamera");
		interactCast = GetNode<RayCast3D>("InteractCast");
		interactCast.TargetPosition = new (0, 0, -10);

        PlayerPlaceBox = GetNode<PlaceBox>("PlaceBox");
		HUD = GetNode<hud_scene>("HudScene");

		// Wait 1 second after scene start to place blocks
		nextPlaceTime = DateTime.Now.Ticks + 10_000_000;
    }

	private bool justLookedAtGrid = false;
    public override void _Process(double delta)
    {
		if (!scene.isActive)
			return;

		InputHandler();
		
		if (interactCast.IsColliding())
		{
			if ((interactCast.GetCollider() as Node3D).GetParent() is CubeGrid grid)
			{
				if (!justLookedAtGrid)
				{
					justLookedAtGrid = true;

					RemoveChild(PlayerPlaceBox);
					grid.AddChild(PlayerPlaceBox);
				
					// Clamp to 90 degree rotations
					PlayerPlaceBox.Rotation = SnapLocal(PlayerPlaceBox.Rotation);
				}

				lookPosition = grid.PlaceProjectionGlobal(interactCast);
			}
		}
		else
		{
			lookPosition = ToGlobal(interactCast.TargetPosition);

			if (justLookedAtGrid)
			{
				Vector3 rot = PlayerPlaceBox.GlobalRotation;
				PlayerPlaceBox.GetParent().RemoveChild(PlayerPlaceBox);
				AddChild(PlayerPlaceBox);
				PlayerPlaceBox.GlobalRotation = rot;

				justLookedAtGrid = false;
			}
		}

		if (PlayerPlaceBox.IsInsideTree())
			PlayerPlaceBox.GlobalPosition = lookPosition;
    }

	private Vector3 SnapLocal(Vector3 rotation)
	{
		Vector3 mod = rotation % nd;

		// Attempts to round to closest snap rotation
		if (mod.X > nd/2)
			rotation.X += nd - mod.X;
		else
			rotation.X -= mod.X;

		if (mod.Y > nd/2)
			rotation.Y += nd - mod.Y;
		else
			rotation.Y -= mod.Y;

		if (mod.Z > nd/2)
			rotation.Z += nd - mod.Z;
		else
			rotation.Z -= mod.Z;

		return rotation;
	}

	private void InputHandler()
	{
		// Prevents placing 60 blocks per second. Rate-limited to 6 per second.
		if (PlayerPlaceBox.IsHoldingBlock && nextPlaceTime < DateTime.Now.Ticks)
		{
			// TODO: variable distance
			if (Input.IsActionJustPressed("MousePressL"))
			{
				scene.TryPlaceBlock(PlayerPlaceBox.CurrentBlockId, interactCast, PlayerPlaceBox.Rotation);
				nextPlaceTime = DateTime.Now.Ticks + 1_000_000;
			}
			
			if (Input.IsActionJustPressed("MousePressR"))
			{
				scene.RemoveBlock();
				nextPlaceTime = DateTime.Now.Ticks + 1_000_000;
			}
		}

		// Surely there's a better way to do this, but would be a waste of dev time.
		if (Input.IsActionJustPressed("Toolbar0"))
			PlayerPlaceBox.SetBlock(HUD.Toolbar[0]);
		if (Input.IsActionJustPressed("Toolbar1"))
			PlayerPlaceBox.SetBlock(HUD.Toolbar[1]);
		if (Input.IsActionJustPressed("Toolbar2"))
			PlayerPlaceBox.SetBlock(HUD.Toolbar[2]);
		if (Input.IsActionJustPressed("Toolbar3"))
			PlayerPlaceBox.SetBlock(HUD.Toolbar[3]);
		if (Input.IsActionJustPressed("Toolbar4"))
			PlayerPlaceBox.SetBlock(HUD.Toolbar[4]);
		if (Input.IsActionJustPressed("Toolbar5"))
			PlayerPlaceBox.SetBlock(HUD.Toolbar[5]);
		if (Input.IsActionJustPressed("Toolbar6"))
			PlayerPlaceBox.SetBlock(HUD.Toolbar[6]);
		if (Input.IsActionJustPressed("Toolbar7"))
			PlayerPlaceBox.SetBlock(HUD.Toolbar[7]);
		if (Input.IsActionJustPressed("Toolbar8"))
			PlayerPlaceBox.SetBlock(HUD.Toolbar[8]);
		if (Input.IsActionJustPressed("Toolbar9"))
			PlayerPlaceBox.SetBlock(HUD.Toolbar[9]);

		// Why IsActionJustReleased? God knows. I sure don't.
		if (Input.IsActionJustReleased("MoveBlockOut"))
			interactCast.TargetPosition = new(0, 0, MoveTowardsFloat(interactCast.TargetPosition.Z, -20, 0.5f));
		if (Input.IsActionJustReleased("MoveBlockIn"))
			interactCast.TargetPosition = new(0, 0, MoveTowardsFloat(interactCast.TargetPosition.Z, -2, 0.5f));

		// Block rotation
		if (PlayerPlaceBox.IsHoldingBlock)
		{
			// Rotate 90 degrees when looking at grid
			if (interactCast.GetCollider() is CubeBlock)
			{
				if (Input.IsActionJustPressed("BlockRotateX+"))
					PlayerPlaceBox.RotateObjectLocal(Vector3.Right, nd);
				if (Input.IsActionJustPressed("BlockRotateX-"))
					PlayerPlaceBox.RotateObjectLocal(Vector3.Right, -nd);
				if (Input.IsActionJustPressed("BlockRotateY+"))
					PlayerPlaceBox.RotateObjectLocal(Vector3.Up, nd);
				if (Input.IsActionJustPressed("BlockRotateY-"))
					PlayerPlaceBox.RotateObjectLocal(Vector3.Up, -nd);
				if (Input.IsActionJustPressed("BlockRotateZ+"))
					PlayerPlaceBox.RotateObjectLocal(Vector3.Forward, nd);
				if (Input.IsActionJustPressed("BlockRotateZ-"))
					PlayerPlaceBox.RotateObjectLocal(Vector3.Forward, -nd);
			}
			else // Else rotate 90 degrees per second
			{
				if (Input.IsActionPressed("BlockRotateX+"))
					PlayerPlaceBox.RotateObjectLocal(Vector3.Right, ohd);
				if (Input.IsActionPressed("BlockRotateX-"))
					PlayerPlaceBox.RotateObjectLocal(Vector3.Right, -ohd);
				if (Input.IsActionPressed("BlockRotateY+"))
					PlayerPlaceBox.RotateObjectLocal(Vector3.Up, ohd);
				if (Input.IsActionPressed("BlockRotateY-"))
					PlayerPlaceBox.RotateObjectLocal(Vector3.Up, -ohd);
				if (Input.IsActionPressed("BlockRotateZ+"))
					PlayerPlaceBox.RotateObjectLocal(Vector3.Forward, ohd);
				if (Input.IsActionPressed("BlockRotateZ-"))
					PlayerPlaceBox.RotateObjectLocal(Vector3.Forward, -ohd);
			}
		}
	}

	// 1.5 degrees, in radians
	private const float ohd = Mathf.Pi/120;
	// 90 degrees in radians
	private const float nd = Mathf.Pi/2;

	public override void _PhysicsProcess(double delta)
	{
		if (!scene.isActive)
			return;

		HandleMovement(delta);
		HandleRotation(delta);
		lastX = 0;
		lastY = 0;
		lastZ = 0;
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseMotion motion)
		{
			lastX = -motion.Relative.X;
			lastY = -motion.Relative.Y;
			return;
		}
	}

	private void HandleRotation(double delta)
	{
		if (Input.IsActionPressed("RotateClockwise"))
			lastZ = 1;

		if (Input.IsActionPressed("RotateAntiClockwise"))
			lastZ = -1;

		RotateObjectLocal(Vector3.Up, (float)(lastX*delta));
		RotateObjectLocal(Vector3.Right, (float)(lastY*delta));
		RotateObjectLocal(Vector3.Forward, (float)(lastZ*delta));
	}

	private void HandleMovement(double delta)
	{
		Vector3 velocity = Velocity;
		
		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.

		Vector2 horizontalInput = Input.GetVector("MoveLeft", "MoveRight", "MoveForward", "MoveBackward");
		float verticalInput = Input.GetAxis("MoveDown", "MoveUp");

		Vector3 inputDir = new (horizontalInput.X, verticalInput, horizontalInput.Y);


		Vector3 direction = Quaternion * inputDir;
		if (direction != Vector3.Zero)
			velocity += ClampIndividual(direction, 1) * Speed;
		else if (_dampenersEnabled)
			velocity = IndividualDampen(velocity, Vector3.Zero, Speed*1.15f);
		
		Velocity = velocity;
		MoveAndSlide();
	}

	private void _ToggleDampeners(bool enabled)
	{
		_dampenersEnabled = enabled;
	}

	private void _ToggleLight(bool enabled)
	{
		light.Visible = enabled;
	}

	private void _ToggleThirdPerson(bool enabled)
	{
		if (enabled)
			camera.Position = new Vector3(0, 0.5f, 0);
		else
			camera.Position = new Vector3(0, 1.5f, 3f);
	}

	private Vector3 IndividualDampen(Vector3 vel, Vector3 endVel, float delta)
	{
		// Simulates directional jetpack thrusters, rather than a gimballed system
		vel = Quaternion.Inverse() * vel;
		endVel = Quaternion.Inverse() * endVel;

		return Quaternion * new Vector3(
			MoveTowardsFloat(vel.X, endVel.X, delta),
			MoveTowardsFloat(vel.Y, endVel.Y, delta),
			MoveTowardsFloat(vel.Z, endVel.Z, delta)
		);
	}

	private float MoveTowardsFloat(float value, float endValue, float delta)
	{
		float evDelta = Math.Abs(value - endValue);

		if (evDelta > delta)
		{
			if (value > endValue)
				return value - delta;
			if (value < endValue)
				return value + delta;
		}

		return endValue;
	}

	private Vector3 ClampIndividual(Vector3 vec, float max)
	{
		if (vec.X > max)
			vec.X = max;
		if (vec.Y > max)
			vec.Y = max;
		if (vec.Z > max)
			vec.Z = max;

        return vec;
	}
}
