using Godot;
using System;
using System.Threading.Tasks;

public partial class Hierbas : Area2D
{
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

    private async void OnBodyEntered(Node body, Node area)
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

        // Sacamos el nodo del juego para consultar sus variables
        var gameNode = GetNode<Game>("/root/Game");
        var playerNode = GetNode<MainCharacter>("/root/Game/Player");
        var transitionNode = GetNode<BattleTransition>("/root/Transitions/BattleTransition");

        int idPoke = getRandom(1, 649);
        int levelPoke = getRandom(MinLevel, MaxLevel);
        Pokemon pokemon = await PokemonController.GetPokemonById(idPoke);

        pokemon.Nivel = levelPoke;

        playerNode.FreezePlayer();
        Game.EstadoJuego = 2;

        await transitionNode.StartTransition();

        await Task.Delay(750);

        PackedScene transitionScene = (PackedScene)GD.Load("res://scenes/interfaces/combate.tscn");
        CanvasLayer battle = transitionScene.Instantiate<CanvasLayer>();

        // FALTA OPTIMIZACIÓN
        String namePokeUpper = pokemon.Nombre.ToUpper();
        var namePokeEnemy = battle.GetNode<Label>("InfoEnemy/namePokemon");

        namePokeEnemy.Text = namePokeUpper;
        var spriteEnemy = battle.GetNode<Sprite2D>("PokeEnemy");

        var levelEnemy = battle.GetNode<Label>("InfoEnemy/levelPokemon");
        levelEnemy.Text = $"Lv{levelPoke}";

        GD.Print($"{namePokeUpper} - Nv{levelPoke}");
        Texture2D nuevoSprite = null;

        try {
            nuevoSprite = (Texture2D)GD.Load($"res://assets/pokemons/front/{namePokeUpper}.png");
        } catch (Exception e) {
            GD.Print(e.Message);
            playerNode.UnfreezePlayer();
            Game.EstadoJuego = 1;
            return;
        }

        if (nuevoSprite == null){
            playerNode.UnfreezePlayer();
            Game.EstadoJuego = 1;
            transitionNode.LetCamera();
            return;
        }
        
        spriteEnemy.Texture = nuevoSprite;
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
