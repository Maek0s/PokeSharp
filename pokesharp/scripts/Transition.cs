using Godot;
using System;
using System.Threading.Tasks;

public partial class Transition : CanvasLayer
{
	private AnimationPlayer _animationPlayer;
	private Camera2D _camera;
	private CharacterBody2D _player;

	public override void _Ready()
	{
		Layer = -1;
		_animationPlayer = (AnimationPlayer)GetNode<Node>("AnimationPlayer");

		_player = (CharacterBody2D) GetNode("/root/Game/Player");
		_camera = (Camera2D) _player.GetNode("CameraFollow");
	}

	// Inicia la transición
	public async Task StartTransition()
	{
		_camera.PositionSmoothingEnabled = false;
        _player.SetPhysicsProcess(false);

		Layer = 1;

		await PlayAnimation("transition");
	}

	// Finaliza la transición
	public async Task EndTransition()
	{
		await PlayAnimation("transition", true);
		Layer = -1;

		_camera.PositionSmoothingEnabled = true;
		_player.SetPhysicsProcess(true);
	}

	private async Task PlayAnimation(string animName, bool backwards = false)
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

		if (backwards)
			_animationPlayer.PlayBackwards(animName);
		else
			_animationPlayer.Play(animName);

		await tcs.Task;
	}
}
