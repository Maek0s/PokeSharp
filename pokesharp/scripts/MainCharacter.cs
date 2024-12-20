using Godot;
using System;

public partial class MainCharacter : CharacterBody2D
{
	public float Speed = 300.0f;

    private Vector2 _velocity = Vector2.Zero;

	public override void _PhysicsProcess(double delta)
	{
		// Reinicia la velocidad
        _velocity = Vector2.Zero;

        // Movimiento basado en la entrada del usuario
        if (Input.IsActionPressed("ui_right"))
            _velocity.X += 1;
        if (Input.IsActionPressed("ui_left"))
            _velocity.X -= 1;
        if (Input.IsActionPressed("ui_down"))
            _velocity.Y += 1;
        if (Input.IsActionPressed("ui_up"))
            _velocity.Y -= 1;

        // Normaliza para evitar velocidad diagonal mayor y aplica la velocidad
        _velocity = _velocity.Normalized() * Speed;

		Velocity = _velocity;
        MoveAndSlide();
	}
}
