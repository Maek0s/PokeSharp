using Godot;
using GodotPlugins.Game;
using System;
using System.Threading.Tasks;

public partial class DoorTransition : CanvasLayer
{
    private AnimationPlayer _animationPlayer;
	private Camera2D _camera;
	private MainCharacter _player;

	public override void _Ready()
	{
		Layer = -1;
		_animationPlayer = (AnimationPlayer) GetNode<Node>("AnimationPlayer");

		if (HasNode("/root/Game/Player"))
		{
			_player = GetNode<MainCharacter>("/root/Game/Player");
			_camera = _player.GetNode<Camera2D>("CameraFollow");
		}
		else
		{
			GD.Print("[⚠️] Player no está disponible todavía.");
		}
	}

	// Inicia la transición / Starts the transition
	public async Task StartTransition(bool isADoor)
	{
		_camera.PositionSmoothingEnabled = false;
        _player.FreezePlayer();

		Node listSounds = GetNode("/root/Game/SFX");
		if (isADoor)
		{
			GD.Print("Sonando, isAdoor ", isADoor);
			AudioStreamPlayer2D audio = (AudioStreamPlayer2D) listSounds.GetNode("doorOpen");

			audio.Play();
		}

		Layer = 1;

		await PlayAnimation("transition", isADoor: isADoor);
	}

	// Finaliza la transición / Stops the transition
	public async Task EndTransition()
	{
		await PlayAnimation("transition", true);
		Layer = -1;

		_camera.PositionSmoothingEnabled = true;
		_player.UnfreezePlayer();
	}

	/// <summary>
	/// Reproduce la animación solicitada de una forma u otra (hacia delante o hacia atras)
	/// Plays the requested animation one way or the other (forward or backward).
	/// </summary>
	/// <param name="animName">Nombre de la animación / Animation name</param>
	/// <param name="backwards">Hacia delante o hacia atras / Forward or backward</param>
	/// <returns></returns>
	
	private async Task PlayAnimation(string animName, bool backwards = false, bool isADoor = false)
	{
		var tcs = new TaskCompletionSource<bool>();

		void OnAnimationFinished(StringName finishedAnim)
		{
			if (finishedAnim == animName)
			{
				_animationPlayer.AnimationFinished -= OnAnimationFinished;
				tcs.SetResult(true);
			}
		}

		_animationPlayer.AnimationFinished += OnAnimationFinished;

		if (backwards) {
			_animationPlayer.PlayBackwards(animName);

			if (isADoor)
			{
				// Reproduce el sonido de cerrar la puerta / Plays the sound of closing the door
				Node listSounds = GetNode("/root/Game/SFX");
				AudioStreamPlayer2D audio = (AudioStreamPlayer2D) listSounds.GetNode("doorClose");
				audio.Play();
			}
		} else
			_animationPlayer.Play(animName);

		await tcs.Task;
	}
}
