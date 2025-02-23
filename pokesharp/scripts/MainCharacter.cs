using Godot;
using System;
using System.Diagnostics;
using System.Numerics;

public partial class MainCharacter : CharacterBody2D
{
    // posibilidad de presionar el "Shift" para correr
    private int max_speed = 110;
    private AnimatedSprite2D animatedSprite2D;
    private Godot.Vector2 last_direction = new Godot.Vector2(1,0);

    private Godot.Vector2 input_dir = new Godot.Vector2();
    private bool is_running = false;

    public override void _Ready()
    {
        animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }

	public override void _PhysicsProcess(double delta)
	{
        Godot.Vector2 raw_direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        Godot.Vector2 direction = Godot.Vector2.Zero;

        // Se prioriza la dirección horizontal sobre la vertical
        if (raw_direction.X != 0) {
            direction = new Godot.Vector2(raw_direction.X, 0);
        } else if (raw_direction.Y != 0) {
            direction = new Godot.Vector2(0, raw_direction.Y);
        }

        // Ajusta la velocidad dependiendo de si está corriendo o no
        if (Input.IsActionPressed("run")) {
            is_running = true;
            max_speed = 160;
        } else {
            max_speed = 110;
            is_running = false;
        }
        
        Velocity = direction * max_speed;
        
        MoveAndSlide();

        // Animaciones
        if (direction.Length() > 0) {
            last_direction = direction;
            play_walk_animation(direction);
        } else {
            play_idle_animation(last_direction);
        }

    }

    private void play_walk_animation(Godot.Vector2 direction) {
        if (direction.X > 0) {
            if (is_running) {
                animatedSprite2D.Play("run_right");
            } else {
                animatedSprite2D.Play("walk_right");
            }
        } else if (direction.X < 0) {
            if (is_running) {
                animatedSprite2D.Play("run_left");
            } else {
                animatedSprite2D.Play("walk_left");
            }
        } else if (direction.Y > 0) {
            if (is_running) {
                animatedSprite2D.Play("run_down");
            } else {
                animatedSprite2D.Play("walk_down");
            }
        } else if (direction.Y < 0) {
            if (is_running) {
                animatedSprite2D.Play("run_up");
            } else {
                animatedSprite2D.Play("walk_up");
            }
        }
    }

    private void play_idle_animation(Godot.Vector2 direction) {
        if (direction.X > 0) {
            animatedSprite2D.Play("idle_right");
        } else if (direction.X < 0) {
            animatedSprite2D.Play("idle_left");
        } else if (direction.Y > 0) {
            animatedSprite2D.Play("idle_down");
        } else if (direction.Y < 0) {
            animatedSprite2D.Play("idle_up");
        }
    }
}
