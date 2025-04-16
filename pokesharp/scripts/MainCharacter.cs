using Godot;
using System;
using System.Diagnostics;
using System.Numerics;

public partial class MainCharacter : CharacterBody2D
{
    // posibilidad de presionar el "Shift" para correr
    private int max_speed = 110;
    private AnimatedSprite2D animatedSprite2D;
    private Godot.Vector2 last_direction = new Godot.Vector2(0,1);
    private Godot.Vector2 input_dir = new Godot.Vector2();
    private bool is_running = false;
    public bool in_grass = false;
    public int etapaJuego = 0;

    public override void _Ready()
    {
        animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        AddToGroup("player");
    }

    public void StopPlayer()
    {
        string nameAnim = animatedSprite2D.Animation;
        string[] splitted = nameAnim.Split("_");

        if (!splitted[0].Equals("idle")) {
            string newAnim = "idle_" + splitted[1];
            animatedSprite2D.Play(newAnim);
        }
    }

    public void FreezePlayer()
    {
        Game.ChangeState(0);
        SetPhysicsProcess(false);
        string nameAnim = animatedSprite2D.Animation;
        string[] splitted = nameAnim.Split("_");

        if (!splitted[0].Equals("idle")) {
            string newAnim = "idle_" + splitted[1];
            animatedSprite2D.Play(newAnim);
        }
    }

    public void UnfreezePlayer()
    {
        // Reiniciamos su última dirección para que no la actualice después de descongelarlo
        last_direction = Godot.Vector2.Zero;
        Game.ChangeState(1);
        SetPhysicsProcess(true);
    }
    
	public override void _PhysicsProcess(double delta)
	{
        // Sacamos el nodo del juego para consultar sus variables
        var gameNode = GetNode<Game>("../../Game");

        etapaJuego = Game.EstadoJuego;

        if (etapaJuego != 1) {
            return;
        }

        // Direcciones del personaje
        Godot.Vector2 raw_direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        Godot.Vector2 direction = Godot.Vector2.Zero;

        // Se prioriza la dirección horizontal sobre la vertical
        if (raw_direction.X != 0) {
            direction = new Godot.Vector2(raw_direction.X, 0);
        } else if (raw_direction.Y != 0) {
            direction = new Godot.Vector2(0, raw_direction.Y);
        }

        if (Input.IsActionJustPressed("sacarCoords")) {
            GD.Print("X: " + Position.X + " | Y: " + Position.Y);
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
            bool _isEntering = gameNode.IsEntering;

            if (!_isEntering) {
                play_idle_animation(last_direction);
                last_direction = Godot.Vector2.Zero;
            }
        }
    }

    private void play_walk_animation(Godot.Vector2 direction) {
        if (direction.X > 0) {
            if (is_running) {
                if (in_grass) {
                    animatedSprite2D.Play("run_right_grass");
                } else {
                    animatedSprite2D.Play("run_right");
                }
            } else {
                if (in_grass) {
                    animatedSprite2D.Play("walk_right_grass");
                } else {
                    animatedSprite2D.Play("walk_right");
                }
            }
        } else if (direction.X < 0) {
            if (is_running) {
                if (in_grass) {
                    animatedSprite2D.Play("run_left_grass");
                } else {
                    animatedSprite2D.Play("run_left");
                }
            } else {
                if (in_grass) {
                    animatedSprite2D.Play("walk_left_grass");
                } else {
                    animatedSprite2D.Play("walk_left");
                }
            }
        } else if (direction.Y > 0) {
            if (is_running) {
                if (in_grass) {
                    animatedSprite2D.Play("run_down_grass");
                } else {
                    animatedSprite2D.Play("run_down");
                }
            } else {
                if (in_grass) {
                    animatedSprite2D.Play("walk_down_grass");
                } else {
                    animatedSprite2D.Play("walk_down");
                }
            }
        } else if (direction.Y < 0) {
            if (is_running) {
                if (in_grass) {
                    animatedSprite2D.Play("run_up_grass");
                } else {
                    animatedSprite2D.Play("run_up");
                }
            } else {
                if (in_grass) {
                    animatedSprite2D.Play("walk_up_grass");
                } else {
                    animatedSprite2D.Play("walk_up");
                }
            }
        }
    }

    private void play_idle_animation(Godot.Vector2 direction) {
        if (direction.X > 0) {
            if (in_grass)
                animatedSprite2D.Play("idle_right_grass");
            else
                animatedSprite2D.Play("idle_right");
        } else if (direction.X < 0) {
            if (in_grass)
                animatedSprite2D.Play("idle_left_grass");
            else
                animatedSprite2D.Play("idle_left");
        } else if (direction.Y > 0) {
            if (in_grass)
                animatedSprite2D.Play("idle_down_grass");
            else
                animatedSprite2D.Play("idle_down");
            
        } else if (direction.Y < 0) {
            if (in_grass)
                animatedSprite2D.Play("idle_up_grass");
            else
                animatedSprite2D.Play("idle_up");
        }
    }
}
