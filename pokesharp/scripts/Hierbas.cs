using Godot;
using System;
using System.Threading.Tasks;

public partial class Hierbas : Area2D
{
    public Pokemon Pokemon;
    private MainCharacter _player;
    private AnimatedSprite2D _animacion;
    private int _grassCount = 0;
    private int porcentage = 25;
    private bool inEncounter = false;

    [Export(PropertyHint.Range, "0,100")] public int MinLevel = 1;
    [Export(PropertyHint.Range, "0,100")] public int MaxLevel = 100;

	public override void _Ready()
    {
        _player = GetNode<MainCharacter>("/root/Game/Player");
        _animacion = _player.GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        foreach (Node child in GetChildren())
        {
            if (child is Area2D area)
            {
                area.BodyEntered += (body) => OnBodyEntered(body, area);
                area.BodyExited += (body) => OnBodyExited(body, area);
            }
        }
    }

    private void OnBodyEntered(Node body, Node area)
    {
        if (body.IsInGroup("player") && !inEncounter)
        {
            _grassCount++;
            
            Random rnd = new Random();

            int numRnd = rnd.Next(1, 100);
            GD.Print(numRnd);

            if (numRnd > 0 && numRnd <= porcentage)
            {
                inEncounter = true;
                pokemonFound();
            }

            _player.in_grass = true;
        }
    }

    private async void pokemonFound() {
        GD.Print("Pokémon!");

        var musicBattle = GetNode<AudioStreamPlayer2D>("/root/Game/SFX/battleMusic");
        musicBattle.VolumeDb = -15.0f;

        // Sacamos el nodo del juego para consultar sus variables
        var gameNode = GetNode<Game>("/root/Game");
        var playerNode = GetNode<MainCharacter>("/root/Game/Player");
        var transitionNode = GetNode<BattleTransition>("/root/Transitions/BattleTransition");


        // gen 1 hasta la 5
        int idPoke = getRandom(1, 649);
        int levelPokeEnemy = getRandom(MinLevel, MaxLevel);
        Pokemon = await PokemonController.GetPokemonById(idPoke);

        Pokemon pokemonAllyFirst = new Pokemon();

        if (Game.PlayerPlaying.listPokemonsTeam.Count == 0) {
            pokemonAllyFirst.nivel = 1;
            pokemonAllyFirst.Nombre = "charmander";
            pokemonAllyFirst.NombreCamelCase = "Charmander";
        } else {
            pokemonAllyFirst = Game.PlayerPlaying.listPokemonsTeam[0];
        }

        String namePokeUpperEnemy = Pokemon.Nombre.ToUpper();
        String namePokeUpperAlly = pokemonAllyFirst.Nombre.ToUpper();
        
        Texture2D nuevoSprite = GD.Load<Texture2D>($"res://assets/pokemons/front/{namePokeUpperEnemy}.png");
        
        if (nuevoSprite == null) {
            GD.PrintErr($"No se pudo cargar el sprite para {namePokeUpperEnemy}");
            playerNode.UnfreezePlayer();
            Game.ChangeState(1);
            inEncounter = false;
            return;
        }

        Pokemon.nivel = levelPokeEnemy;
        Pokemon.CalcularStats();

        Game.PokemonInFight = Pokemon;

        GD.Print($"Pokemon Hierbas: {Pokemon}");

        playerNode.FreezePlayer();
        musicBattle.Play();
        Game.ChangeState(2);

        await transitionNode.StartTransition();

        await Task.Delay(750);

        PackedScene transitionScene = (PackedScene) GD.Load("res://scenes/interfaces/combate.tscn");
        CanvasLayer battle = transitionScene.Instantiate<CanvasLayer>();

        var namePokeEnemy = battle.GetNode<Label>("InfoEnemy/namePokemon");
        var namePokeAlly = battle.GetNode<Label>("InfoAlly/namePokemon");

        namePokeEnemy.Text = namePokeUpperEnemy;
        namePokeAlly.Text = namePokeUpperAlly;

        var spriteEnemy = battle.GetNode<Sprite2D>("PokeEnemy");
        var spriteAlly = battle.GetNode<Sprite2D>("PokeAlly");

        var levelEnemy = battle.GetNode<Label>("InfoEnemy/levelPokemon");
        levelEnemy.Text = $"Lv{levelPokeEnemy}";

        var levelAlly = battle.GetNode<Label>("InfoAlly/levelPokemon");
        levelAlly.Text = $"Lv{pokemonAllyFirst.nivel}";

        GD.Print($"{namePokeUpperEnemy} - Nv{levelPokeEnemy}");

        if (nuevoSprite == null){
            playerNode.UnfreezePlayer();
            Game.ChangeState(1);
            transitionNode.LetCamera();
            inEncounter = false;
            return;
        }
        
        Texture2D spriteTextureAlly = GD.Load<Texture2D>($"res://assets/pokemons/back/{namePokeUpperAlly}.png");

        spriteEnemy.Texture = nuevoSprite;
        spriteAlly.Texture = spriteTextureAlly;

        Texture2D texture = spriteEnemy.Texture;

        transitionNode.LetCamera();

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

        GetTree().Root.AddChild(battle);  // Lo agregamos al árbol de nodos
        TransitionManager transitionManager = battle.GetNode<TransitionManager>("TransitionManager");

        transitionManager.IniciarCombate();

        inEncounter = false;
    }

    private void OnBodyExited(Node body, Node area)
    {
        if (body.IsInGroup("player"))
        {
            _grassCount--;

            if (_grassCount <= 0)
            {
                _player.in_grass = false;
                _grassCount = 0;
            }
        }
    }

    private int getRandom(int min, int max)
    {
        Random rnd = new Random();
        return rnd.Next(min, max);
    }

}
