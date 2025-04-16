using Godot;
using System;
using System.Diagnostics;
using System.Numerics;

public partial class Game : Node2D
{
	[Export] public NodePath MapContainerPath;
	private static Node2D mapContainer;

	private static Node2D startMenu;
	private static MainCharacter player;
	private static Label fpsDisplay;
	private static CanvasLayer menuInGame;

	public bool IsEntering = false;
	/*
    Estados Juego
     Estado -1 - Menu de inicio (El jugador no deberia de existir)
     Estado 0 - Juego en Pausa (No se puede mover y el resto de cosas tampoco)
     Estado 1 - Corriendo juego (Todo normal)
     Estado 2 - En combate (No se puede mover el jugador)
    */
	public static int EstadoJuego = -1;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		asignarVariables();
		ChangeState(-1);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("menu"))
		{
			TogglePauseMenu();
		}
	}

	public void TogglePauseMenu()
	{
		var pauseMenu = GetNode<CanvasLayer>("/root/Game/inScreen/UI/PauseMenu");

		var mainCharacter = GetNode<MainCharacter>("/root/Game/Player");

		if (EstadoJuego == 1)
		{
			pauseMenu.Visible = true;
			EstadoJuego = 0;
			mainCharacter.FreezePlayer();
		}
		else if (EstadoJuego == 0)
		{
			pauseMenu.Visible = false;
			EstadoJuego = 1;
			mainCharacter.UnfreezePlayer();
		}
	}

	public static void ChangeState(int newState) {
		if (newState >= -1 && newState <= 2) {
			EstadoJuego = newState;

			switch (EstadoJuego) {
				case -1:
					menuInicio();
					break;
				case 0:
					break;
				case 1:
					inGame();
					break;
				case 2:
					break;
				default:
					break;
			}
		} else {
			GD.PrintErr("Estado invalido " + newState);
		}
	}

	public static void menuInicio()
	{
		startMenu.Visible = true;
		mapContainer.Visible = false;
		player.Visible = false;
		fpsDisplay.Visible = false;
		menuInGame.Visible = false;
	}

	public static void inGame()
	{
		startMenu.Visible = false;
		mapContainer.Visible = true;
		player.Visible = true;
		fpsDisplay.Visible = true;
		menuInGame.Visible = false;
		player.SetPhysicsProcess(true);
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

	public void asignarVariables()
	{
		startMenu = GetNode<Node2D>("/root/Game/inScreen/UI/PaginaInicio");
		mapContainer = GetNode<Node2D>(MapContainerPath);
		player = GetNode<MainCharacter>("/root/Game/Player");
		fpsDisplay = GetNode<Label>("/root/Game/inScreen/UI/FPSDisplay");
		menuInGame = GetNode<CanvasLayer>("/root/Game/inScreen/UI/PauseMenu");
	}
}
