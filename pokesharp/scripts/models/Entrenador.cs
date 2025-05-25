using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;
using System.Threading.Tasks;

public partial class Entrenador : CharacterBody2D
{
    [Export] public string Nombre = "Entrenador";
    [Export] public Godot.Collections.Array equipoResources = new Godot.Collections.Array();
    public List<Pokemon> EquipoPokemon { get; set; } = new List<Pokemon>();
    [Export] public string[] Dialogos;
    [Export] public string TextoFight;
    [Export] public string TextoDerrota;
    [Export] public string TextoVictoria;
    [Export] private int condicion;
    [Export] public string textCondicionInvalida;
    [Export] public SpriteFrames spriteFrames;
    [Export] public string animacionInicial = "idle";
    [Export] private int levelPokemon;
    [Export] private int dificultad;
    [Export] private int totalPokemon;
    [Export] private int movesTutor;
    [Export] private int movesType;
    [Export] private int movesRandom;

    private AnimatedSprite2D animatedSprite;
    private int dialogoActual = 0;
    private bool jugadorCerca = false;
    private bool jugadorZonaLoad = false;
    private bool sacandoPokemon = false;
    private bool cargandoEquipo = false;
    private int mediaRecibida = -1;

    public override void _Ready()
    {
        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animatedSprite.SpriteFrames = spriteFrames;

        animatedSprite.Play(animacionInicial);

        var area = GetNode<Area2D>("Area2D");
        area.BodyEntered += OnJugadorEntrar;
        area.BodyExited += OnJugadorSalir;

        var loadArea = GetNode<Area2D>("LoadArea");
        loadArea.BodyEntered += OnLoadAreaJugadorEntrar;
        loadArea.BodyExited += OnLoadAreaJugadorSalir;
    }

    private void OnLoadAreaJugadorEntrar(Node body)
    {
        if (body.IsInGroup("jugador"))
        {
            GD.Print("Jugador in load");
            jugadorZonaLoad = true;
        }
    }

    private void OnLoadAreaJugadorSalir(Node body)
    {
        if (body.IsInGroup("jugador"))
        {
            GD.Print("Jugador out load");
            jugadorZonaLoad = false;
        }
    }

    private async void LoadEquipoPokemonAsync()
    {
        int mediaNiveles = 0;

        if (Game.PlayerPlaying.listPokemonsTeam.Count > 0)
        {
            mediaNiveles = Player.CalcularNivelReferencia(Game.PlayerPlaying.listPokemonsTeam);

            Game.PlayerPlaying.MediaPoke = mediaNiveles;

            if (mediaNiveles < levelPokemon + 4)
            {
                mediaNiveles = 0;
            }
            else
            {
                int variacionNivel = GD.RandRange(-6, 0);

                GD.Print("variacion ", variacionNivel);

                if (variacionNivel + mediaNiveles > 100)
                {
                    mediaNiveles = 100;
                }
                else
                {
                    mediaNiveles += variacionNivel;
                }
            }
        }

        foreach (var resource in equipoResources)
        {
            Pokemon pokemonResource = resource.As<Pokemon>();
            if (pokemonResource != null)
            {
                Pokemon pokemonInstancia = (Pokemon)pokemonResource.Duplicate();

                pokemonInstancia = await PokemonController.GetPokemonById(pokemonInstancia.Id);

                if (mediaNiveles != 0)
                {
                    pokemonInstancia.nivel = mediaNiveles;
                }
                else
                {
                    pokemonInstancia.nivel = levelPokemon;
                }

                GD.Print("pokeInst xp base", pokemonInstancia.ExperienciaBase);
                GD.Print("pokeResorc xp base", pokemonResource.ExperienciaBase);

                pokemonInstancia.ExperienciaBase = pokemonResource.ExperienciaBase;
                pokemonInstancia.b_atk += dificultad;
                pokemonInstancia.b_def += dificultad;
                pokemonInstancia.b_speed += dificultad;
                pokemonInstancia.b_hp += dificultad;

                pokemonInstancia.CalcularStats();

                MovesController movesController = new MovesController();

                for (int i = 0; i < 4; i++)
                {
                    pokemonInstancia.Movimientos.Add(await movesController.GetMovimientoByCustomPorcentage(pokemonInstancia.Id, movesTutor, movesType, movesRandom));
                }

                EquipoPokemon.Add(pokemonInstancia);
            }
        }

        if (totalPokemon != equipoResources.Count)
        {
            GD.Print("totalPokemon != equipoResources.Count");
            int randomPoke = totalPokemon - equipoResources.Count;

            if (randomPoke > 0)
            {
                for (int i = 0; i < randomPoke; i++)
                {
                    Random rnd = new Random();
                    int idPoke = rnd.Next(1, 649);
                    Pokemon pokeRandom = await PokemonController.GetPokemonById(idPoke);
                    MovesController movesController = new MovesController();

                    for (int j = 0; j < 4; j++)
                    {
                        pokeRandom.Movimientos.Add(await movesController.GetMovimientoByCustomPorcentage(idPoke, movesTutor, movesType, movesRandom));
                    }

                    if (mediaNiveles != 0)
                    {
                        pokeRandom.nivel = mediaNiveles;
                    }
                    else
                    {
                        pokeRandom.nivel = levelPokemon;
                    }

                    pokeRandom.CalcularStats();

                    EquipoPokemon.Add(pokeRandom);
                }
            }
        }

        foreach (var pokeEnemyTeam in EquipoPokemon)
        {
            GD.Print("pokeEnemyTeam: ", pokeEnemyTeam);
        }
    }

    public override void _Process(double delta)
    {
        if (jugadorCerca && Input.IsActionJustPressed("interact"))
        {
            Hablar();
        }

        if (jugadorZonaLoad && EquipoPokemon.Count <= 0 && !cargandoEquipo)
        {
            cargandoEquipo = true;
            _ = EsperarYCargarEquipo();
        }
    }

    private async Task EsperarYCargarEquipo()
    {
        await ToSignal(GetTree().CreateTimer(0.3f), "timeout");

        if (MgtDatabase.ConexionEstablecida && Game.PlayerPlaying != null)
        {
            if (!sacandoPokemon)
            {
                CallDeferred(nameof(LoadEquipoPokemonAsync));
                sacandoPokemon = true;
            }
        }

        cargandoEquipo = false;
    }

    public void Hablar()
    {
        var dialogoUI = GetNode<Dialogo>("/root/Game/inScreen/UI/Dialogo");

        if (Dialogos != null && Dialogos.Length > 0)
        {
            if (dialogoActual < Dialogos.Length)
            {
                if (condicion > Game.PlayerPlaying.listPokemonsTeam.Count)
                {
                    dialogoUI.MostrarTexto(textCondicionInvalida, Nombre);
                }
                else
                {
                    if (mediaRecibida != Game.PlayerPlaying.MediaPoke)
                    {
                        if (Game.PlayerPlaying.listPokemonsTeam.Count > 0 && EquipoPokemon.Count > 0)
                        {
                            mediaRecibida = Player.CalcularNivelReferencia(Game.PlayerPlaying.listPokemonsTeam);

                            Game.PlayerPlaying.MediaPoke = mediaRecibida;

                            if (mediaRecibida < levelPokemon + 4)
                            {
                                mediaRecibida = levelPokemon;
                            }
                            else
                            {
                                int variacionNivel = GD.RandRange(-6, 1);

                                GD.Print("variacion ", variacionNivel);

                                if (variacionNivel + mediaRecibida > 100)
                                {
                                    mediaRecibida = 100;
                                }
                                else
                                {
                                    mediaRecibida += variacionNivel;
                                }
                            }

                            GD.Print("mediaRecibida:", mediaRecibida);

                            foreach (Pokemon pokeTeam in EquipoPokemon)
                            {
                                pokeTeam.nivel = mediaRecibida;
                                pokeTeam.CalcularStats();
                            }
                        }
                    }
                    else
                    {
                        GD.Print($"mediaRecibida {mediaRecibida} Game.PlayerPlaying.MediaPoke {Game.PlayerPlaying.MediaPoke}");

                        foreach (Pokemon pokeTeam in EquipoPokemon)
                        {
                            pokeTeam.nivel = mediaRecibida;
                            pokeTeam.CalcularStats();
                        }
                    }

                    dialogoUI.MostrarTexto(Dialogos[dialogoActual], Nombre);

                    if (dialogoActual == Dialogos.Length - 1)
                    {
                        if (Game.EntrenadorFighting != this)
                        {
                            _ = EjecutarAnimacionEncuentro();
                        }
                    }

                    dialogoActual = dialogoActual + 1;
                }
            }
            else
            {
                dialogoUI.OcultarTexto();
                dialogoActual = 0;
            }
        }
        else
        {
            GD.Print("Dialogos != null && Dialogos.Length > 0");
        }
    }

    private async Task<bool> EjecutarAnimacionEncuentro()
    {
        Game.EntrenadorFighting = this;

        GD.Print("equipoke entrenador: ", Game.EntrenadorFighting.EquipoPokemon.Count);

        await ToSignal(GetTree().CreateTimer(1.0), "timeout");

        var transitionNode = GetNode<BattleTransition>("/root/Transitions/BattleTransition");

        var dialogoUI = GetNode<Dialogo>("/root/Game/inScreen/UI/Dialogo");
        dialogoUI.OcultarTexto();

        await transitionNode.StartTransition();

        var musicBattle = GetNode<AudioStreamPlayer2D>("/root/Game/SFX/battleMusic");
        musicBattle.VolumeDb = -15.0f;

        // Sacamos el nodo del juego para consultar sus variables
        var playerNode = GetNode<MainCharacter>("/root/Game/Player");

        Texture2D nuevoSprite = GD.Load<Texture2D>($"res://assets/pokemons/front/{EquipoPokemon[0].NombreCamelCase}.png");

        if (nuevoSprite == null)
        {
            GD.PrintErr($"No se pudo cargar el sprite");
            playerNode.UnfreezePlayer();
            Game.ChangeState(1);
            return false;
        }

        playerNode.FreezePlayer();
        musicBattle.Play(0);
        Game.ChangeState(2);

        // Si alguno del equipo no tiene moveset se le genera y guarda en la base de datos
        foreach (Pokemon pokeTeam in Game.PlayerPlaying.listPokemonsTeam)
        {
            if (pokeTeam.Movimientos.Count < 4)
                await pokeTeam.generateMoveset(true);
        }

        Game.fight = true;
        Game.ListPokemonInFight = EquipoPokemon;

        await transitionNode.StartTransition();

        await Task.Delay(750);

        PackedScene transitionScene = (PackedScene)GD.Load("res://scenes/interfaces/combate.tscn");
        CanvasLayer battle = transitionScene.Instantiate<CanvasLayer>();

        var namePokeEnemy = battle.GetNode<Label>("InfoEnemy/namePokemon");
        var namePokeAlly = battle.GetNode<Label>("InfoAlly/namePokemon");

        Pokemon pokemonEnemyFirst = Game.ListPokemonInFight[0];
        Pokemon pokemonAllyFirst = Game.PlayerPlaying.listPokemonsTeam[0];

        var namePokeUpperEnemy = pokemonEnemyFirst.Nombre.ToUpper();
        var namePokeUpperAlly = Game.PlayerPlaying.listPokemonsTeam[0].Nombre.ToUpper();

        namePokeEnemy.Text = namePokeUpperEnemy;
        namePokeAlly.Text = namePokeUpperAlly;

        var spriteEnemy = battle.GetNode<Sprite2D>("PokeEnemy");
        var spriteAlly = battle.GetNode<Sprite2D>("PokeAlly");

        var levelEnemy = battle.GetNode<Label>("InfoEnemy/levelPokemon");
        levelEnemy.Text = $"Lv{pokemonEnemyFirst.nivel}";

        var levelAlly = battle.GetNode<Label>("InfoAlly/levelPokemon");
        levelAlly.Text = $"Lv{pokemonAllyFirst.nivel}";

        GD.Print($"{namePokeUpperEnemy} - Nv{pokemonEnemyFirst.nivel}");

        if (nuevoSprite == null)
        {
            playerNode.UnfreezePlayer();
            Game.ChangeState(1);
            musicBattle.Stop();
            //musicBattle.Seek(0);
            _ = transitionNode.LetCamera();
            return false;
        }

        Texture2D spriteTextureAlly = GD.Load<Texture2D>($"res://assets/pokemons/back/{namePokeUpperAlly}.png");

        spriteEnemy.Texture = nuevoSprite;
        spriteAlly.Texture = spriteTextureAlly;

        Texture2D texture = spriteEnemy.Texture;

        _ = transitionNode.LetCamera();

        /* Centrado a la plataforma (Faltan ajustes a los pequeños) */
        Image image = texture.GetImage();

        Vector2 sumPositions = Vector2.Zero;
        int count = 0;

        int width = image.GetWidth();
        int height = image.GetHeight();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color pixel = image.GetPixel(x, y);
                if (pixel.A > 0.1f)
                {
                    sumPositions += new Vector2(x, y);
                    count++;
                }
            }
        }

        if (count > 0)
        {
            Vector2 visualCenter = sumPositions / count;
            Vector2 textureCenter = new Vector2(width / 2, height / 2);

            spriteEnemy.Offset = visualCenter - textureCenter;
        }

        GetTree().Root.AddChild(battle);
        TransitionManager transitionManager = battle.GetNode<TransitionManager>("TransitionManager");

        transitionManager.IniciarCombateEntrenador(Nombre.Split(" ")[0]);

        return true;
    }

    private void OnJugadorEntrar(Node body)
    {
        if (body.IsInGroup("jugador"))
        {
            GD.Print("Jugador in");
            jugadorCerca = true;
        }
    }

    private void OnJugadorSalir(Node body)
    {
        if (body.IsInGroup("jugador"))
        {
            GD.Print("Jugador out");
            jugadorCerca = false;

            var dialogoUI = GetNode<Dialogo>("/root/Game/inScreen/UI/Dialogo");
            dialogoUI.OcultarTexto();
        }
    }

    public bool checkVivos()
    {
        foreach (Pokemon pokeInTeam in EquipoPokemon)
        {
            if (pokeInTeam.currentHP > 0)
            {
                return true;
            }
        }

        return false;
    }
}
