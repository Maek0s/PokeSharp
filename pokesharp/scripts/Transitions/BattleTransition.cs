using Godot;
using GodotPlugins.Game;
using System;
using System.Threading.Tasks;

public partial class BattleTransition : CanvasLayer
{
    private AnimationPlayer _animationPlayer;
	private Camera2D _camera;
	private MainCharacter _player;
    private Vector2 _originalZoom;

	public override void _Ready()
	{
		Layer = -1;
        
		_animationPlayer = (AnimationPlayer) GetNode<Node>("AnimationPlayer");

		if (HasNode("/root/Game/Player"))
		{
			_player = GetNode<MainCharacter>("/root/Game/Player");
			_camera = _player.GetNode<Camera2D>("CameraFollow");
            _originalZoom = _camera.Zoom; // Guardamos el zoom original al inicio
		}
		else
		{
			GD.Print("[⚠️] Player no está disponible todavía.");
		}

	}

	// Inicia la transición / Starts the transition
	public async Task StartTransition()
	{
		Layer = 1;

        // Obtener duración de la animación
        float animDuration = _animationPlayer.GetAnimation("battle").Length;
        float halfTime = animDuration / 2.0f;

        var playAnimTask = PlayAnimation("battle");

        // Usamos un Tween para asegurar que el zoom ocurra a la mitad de la animación
        var tween = GetTree().CreateTween();
        tween.TweenInterval(halfTime); // Esperar hasta la mitad
        tween.TweenProperty(_camera, "zoom", new Vector2(3, 3), 1.0f); // Hacer zoom en 1s

        await playAnimTask;
	}

    // funciona mal, no quita el zoom o no se
    public async Task LetCamera()
    {
        var tween = GetTree().CreateTween();
        tween.TweenProperty(_camera, "zoom", _originalZoom, 0.5f);
        await ToSignal(tween, "finished");
    }

    private async Task ZoomCamera(float zoomFactor, float duration)
    {
        var tween = GetTree().CreateTween();
        tween.TweenProperty(_camera, "zoom", new Vector2(zoomFactor, zoomFactor), duration);
        await ToSignal(tween, "finished");
    }

	private async Task PlayAnimation(string animName)
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
        
        _animationPlayer.Play(animName);

        await tcs.Task;

        Layer = -1;
    }

}
