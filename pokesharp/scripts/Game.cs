using Godot;
using System;
using System.Diagnostics;
using System.Numerics;

public partial class Game : Node2D
{
    
	[Export] public NodePath MapContainerPath;
	private Node2D mapContainer;

	public bool IsEntering = false;
	/*
    Estados Juego
     Estado -1 - Menu de inicio (El jugador no deberia de existir)
     Estado 0 - Juego en Pausa (No se puede mover y el resto de cosas tampoco)
     Estado 1 - Corriendo juego (Todo normal)
     Estado 2 - En combate (No se puede mover el jugador)
    */
    public int estadoJuego = 1;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		mapContainer = GetNode<Node2D>(MapContainerPath);

		//ChangeMap();
	}

	public async void StartBattle()
	{
		// Obtener la transición global
		BattleTransition transition = (BattleTransition) GetNode("/root/Transitions/BattleTransition");

		// Iniciar la animación de transición y esperar a que termine
		await transition.StartTransition();
	}

    

	public async void ChangeMap(String scenePath, bool _isInterior, float _xSpawnPoint, float _ySpawnPoint) {
		// Obtener la transición global
		DoorTransition transition = (DoorTransition) GetNode("/root/Transitions/DoorTransition");

		// Iniciar la animación de transición y esperar a que termine
		await transition.StartTransition();
		
		// Borrar las anteriores escenas cargadas
		foreach (Node child in mapContainer.GetChildren()) {
			child.QueueFree();
		}

		// Cargar nueva escena
		PackedScene newMap = (PackedScene) ResourceLoader.Load(scenePath);
		Node2D newMapInstance = (Node2D) newMap.Instantiate();
		mapContainer.AddChild(newMapInstance);

		// Teletransportar al jugador
		Godot.Vector2 posSpawnPoint = new Godot.Vector2(0.0f, 0.0f);

		CharacterBody2D player = (CharacterBody2D)GetNode<Node2D>("Player");
		AnimatedSprite2D sprite2D = (AnimatedSprite2D)player.GetNode<Node2D>("AnimatedSprite2D");
		
		// Avisa de que esta entrando y hace una cuenta atras para evitar cambios de movimientos
		IsEntering = true;

		Timer timer = new Timer();
		timer.WaitTime = 2.0f;
		timer.OneShot = true;
		timer.Timeout += () => IsEntering = false;

		AddChild(timer);
		timer.Start();

		// Dependiendo si saliendo de un interior (casa) mirará hacia abajo y usará las coordenadas del json
		if (_isInterior) {
			posSpawnPoint = new Godot.Vector2(_xSpawnPoint, _ySpawnPoint);
			
			GD.Print("Entrando a exterior");

			sprite2D.Play("idle_down");
		} else {
			posSpawnPoint = new Godot.Vector2(_xSpawnPoint, _ySpawnPoint);

			GD.Print("Entrando a interior");

			sprite2D.Play("idle_up");
		}

		player.Position = posSpawnPoint;

		await transition.EndTransition();
	}
}
