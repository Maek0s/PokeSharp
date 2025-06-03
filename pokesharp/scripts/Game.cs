using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public partial class Game : Node2D
{
	[Export] public NodePath MapContainerPath;

	public static Pokemon PokemonInFight;
	public static Entrenador EntrenadorFighting;
	public static List<Pokemon> ListPokemonInFight = new List<Pokemon>();
	public static bool fight = false;
	public static Player PlayerPlaying;
	private static Node2D mapContainer;

	private static Node2D startMenu;
	private static MainCharacter player;
	private static Camera2D cameraFollow;
	private static Label fpsDisplay;

	// Menus
	private static CanvasLayer menuInGame;
	public static CanvasLayer pauseMenu;
	public static CanvasLayer menuPrincipal;

	public static bool IsEntering = false;
	/*
    Estados Juego
     Estado -1 - Menu de inicio (El jugador no deberia de existir)
     Estado 0 - Juego en Pausa (No se puede mover y el resto de cosas tampoco)
     Estado 1 - Corriendo juego (Todo normal)
     Estado 2 - En combate (No se puede mover el jugador)
    */
	public static int EstadoJuego = -1;

	public override void _Ready()
	{
		asignarVariables();
		ChangeState(-1);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("menu"))
		{
			if (EstadoJuego == 1 || pauseMenu.Visible) {
				TogglePauseMenu();
			}
		} else if (@event.IsActionPressed("menuPrincipal")) {
			if (EstadoJuego == 1 || menuPrincipal.Visible) {
				TogglePrincipalMenu();
			}
		}
	}

	public void TogglePrincipalMenu()
	{
		if (menuPrincipal.Visible) {
			menuPrincipal.Visible = false;
			EstadoJuego = 1;
			player.UnfreezePlayer();
		} else {
			menuPrincipal.Visible = true;
			EstadoJuego = 0;
			player.FreezePlayer();
		}
	}

	public void TogglePauseMenu()
	{
		var pauseMenu = GetNode<CanvasLayer>("/root/Game/inScreen/UI/PauseMenu");

		if (EstadoJuego == 1)
		{
			pauseMenu.Visible = true;
			EstadoJuego = 0;
			player.FreezePlayer();
		}
		else if (EstadoJuego == 0)
		{
			pauseMenu.Visible = false;
			EstadoJuego = 1;
			player.UnfreezePlayer();
		}
	}

	public static async void StartCoroutine(float targetVolume, Node rootNode)
	{
		var battleMusic = rootNode.GetNode<AudioStreamPlayer2D>("SFX/battleMusic");

		float time = 0;
		float startVolume = battleMusic.VolumeDb;

		while (time < 2.5f)
		{
			time += (float)rootNode.GetProcessDeltaTime();
			battleMusic.VolumeDb = Mathf.Lerp(startVolume, targetVolume, time / 2.5f);
			await Task.Delay(5);
		}
		battleMusic.VolumeDb = targetVolume;
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
					fixInterfaces();
					break;
				default:
					break;
			}
		} else {
			GD.PrintErr("Estado invalido " + newState);
		}
	}

	public static void fixInterfaces()
	{
		if (pauseMenu.Visible) {
			pauseMenu.Visible = false;
		}

		if (menuPrincipal.Visible) {
			menuPrincipal.Visible = false;
		}
	}

	public static void menuInicio()
	{
		startMenu.Visible = true;
		var musicBackground = startMenu.GetNode<AudioStreamPlayer2D>("MusicManager/musicBackground");
		musicBackground.Play();
		
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

    public async void ChangeMap(String scenePath, bool _isInterior, float _xSpawnPoint, float _ySpawnPoint, bool isADoor) {
		// Obtener la transición global
		DoorTransition transition = (DoorTransition) GetNode("/root/Transitions/DoorTransition");

		// Iniciar la animación de transición y esperar a que termine
		await transition.StartTransition(isADoor);
		
		// Borrar las anteriores escenas cargadas
		foreach (Node child in mapContainer.GetChildren()) {
			child.QueueFree();
		}

		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		// Cargar nueva escena
		PackedScene newMap = (PackedScene) ResourceLoader.Load(scenePath);
		Node2D newMapInstance = (Node2D) newMap.Instantiate();
		mapContainer.AddChild(newMapInstance);

		await ToSignal(GetTree(), "process_frame");
		asignarLimitesCamera();

		// ahi que ordenar un node para los mapas y que se vauyan liberando creo
		GD.Print("Mapa nuevo agregado: ", newMapInstance.Name);

		if (!isADoor)
		{
			var gameNode = GetNode<Game>("/root/Game");
			gameNode.asignarLimitesCamera();
		}

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
			
			sprite2D.Play("idle_down");
		} else {
			posSpawnPoint = new Godot.Vector2(_xSpawnPoint, _ySpawnPoint);

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
		cameraFollow = player.GetNode<Camera2D>("CameraFollow");
		fpsDisplay = GetNode<Label>("/root/Game/inScreen/UI/FPSDisplay");
		menuInGame = GetNode<CanvasLayer>("/root/Game/inScreen/UI/PauseMenu");
		pauseMenu = GetNode<CanvasLayer>("/root/Game/inScreen/UI/PauseMenu");
		menuPrincipal = GetNode<MenuPrincipal>("/root/Game/inScreen/UI/MenuPrincipal");

		asignarLimitesCamera();
	}

	public void asignarLimitesCamera()
	{
		var map = mapContainer.GetChildren();
		StaticBody2D limites = map[0].GetNode<StaticBody2D>("Limites");

		if (limites == null)
		{
			GD.PrintErr($"No se encontró el nodo 'Limites' en el mapa.");
			return;
		}

		//StaticBody2D limites = mapContainer.GetNode<StaticBody2D>("Limites");

		CollisionShape2D colIzq = limites.GetNode<CollisionShape2D>("Izquierda");
		CollisionShape2D colArriba = limites.GetNode<CollisionShape2D>("Arriba");
		CollisionShape2D colDer = limites.GetNode<CollisionShape2D>("Derecha");
		CollisionShape2D colAbajo = limites.GetNode<CollisionShape2D>("Abajo");

		Godot.Vector2 GetXMin(CollisionShape2D col)
		{
			var shape = col.Shape as SegmentShape2D;
			var p1 = col.ToGlobal(shape.A);
			var p2 = col.ToGlobal(shape.B);
			return new Godot.Vector2(Mathf.Min(p1.X, p2.X), 0);
		}

		Godot.Vector2 GetXMax(CollisionShape2D col)
		{
			var shape = col.Shape as SegmentShape2D;
			var p1 = col.ToGlobal(shape.A);
			var p2 = col.ToGlobal(shape.B);
			return new Godot.Vector2(Mathf.Max(p1.X, p2.X), 0);
		}

		Godot.Vector2 GetYMin(CollisionShape2D col)
		{
			var shape = col.Shape as SegmentShape2D;
			var p1 = col.ToGlobal(shape.A);
			var p2 = col.ToGlobal(shape.B);
			return new Godot.Vector2(0, Mathf.Min(p1.Y, p2.Y));
		}

		Godot.Vector2 GetYMax(CollisionShape2D col)
		{
			var shape = col.Shape as SegmentShape2D;
			var p1 = col.ToGlobal(shape.A);
			var p2 = col.ToGlobal(shape.B);
			return new Godot.Vector2(0, Mathf.Max(p1.Y, p2.Y));
		}

		cameraFollow.LimitTop = (int)GetYMin(colArriba).Y;
		cameraFollow.LimitBottom = (int)GetYMax(colAbajo).Y;
		cameraFollow.LimitLeft = (int)GetXMin(colIzq).X;
		cameraFollow.LimitRight = (int)GetXMax(colDer).X;
	}
}
