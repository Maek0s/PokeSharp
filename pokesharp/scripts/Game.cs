using Godot;
using System;
using System.Diagnostics;
using System.Numerics;

public partial class Game : Node2D
{
	[Export] public NodePath MapContainerPath;
	private Node2D mapContainer;
	public bool IsEntering = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		mapContainer = GetNode<Node2D>(MapContainerPath);
		//ChangeMap();
	}

	public async void ChangeMap(String scenePath, bool _isInterior, float _xSpawnPoint, float _ySpawnPoint) {
		// Obtener la transición global
		Transition transition = (Transition) GetNode("/root/Transition");

		// **1. Iniciar la animación de transición y esperar a que termine**
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
		// sino es así usará el spawnpoint de la propia escena para ver su punto de spawn
		if (_isInterior) {
			posSpawnPoint = new Godot.Vector2(_xSpawnPoint, _ySpawnPoint);
			
			sprite2D.Play("idle_down");
		} else {
			Node2D spawnPoint = newMapInstance.GetNode<Node2D>("SpawnPoint");
			posSpawnPoint = new Godot.Vector2(spawnPoint.Position.X - 16, spawnPoint.Position.Y - 85);

			sprite2D.Play("idle_up");
		}

		player.Position = posSpawnPoint;

		await transition.EndTransition();
	}
}
