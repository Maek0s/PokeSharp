using Godot;
using System;

public partial class BattleManager : CanvasLayer {

    Pokemon pokemonAlly;
    Pokemon pokemonEnemy;
    PackedScene transitionScene;
    CanvasLayer battle;
    public int TurnoActual { get; set; } = 0;
    TransitionManager transitionManager;

    private Control moveset;
    private RichTextLabel combatlog;

    private Control bag;
    private Panel panelObjetos;
    private Panel panelObjetosCuracion;
    private Panel panelObjetosCaptura;
    private Panel panelButtons;

    Button btnFight;
    Button btnBag;
    Button btnTeam;
    Button btnRun;

    Button btnPokeball;
    Button btnSuperball;
    Button btnUltraball;
    Button btnMasterball;

    Button btnSiConfirmarLanzar;
    Button btnNoConfirmarLanzar;

    String stringTypePokeball = "Pokeball";
    public override void _Ready()
    {
        pokemonEnemy = Game.PokemonInFight;
        pokemonAlly = Game.PlayerPlaying.listPokemonsTeam[0];

        Panel parteInferior = GetNode<Panel>("ParteInferior");
        Panel combatLogMoveset = parteInferior.GetNode<Panel>("BoxResultCombat");

        Control botones = parteInferior.GetNode<Control>("Buttons");

        var LblNamePokeEnemy = GetNode<Label>("InfoEnemy/namePokemon");

        combatlog = combatLogMoveset.GetNode<RichTextLabel>("CombatLog");
        moveset = combatLogMoveset.GetNode<Control>("Moveset");
        bag = combatLogMoveset.GetNode<Control>("Bag");

        btnFight = botones.GetNode<Button>("Fight");
        btnBag = botones.GetNode<Button>("Bag");
        btnTeam = botones.GetNode<Button>("Team");
        btnRun = botones.GetNode<Button>("Run");

        btnFight.Pressed += OnFightButtonPressed;
        btnBag.Pressed += OnBagButtonPressed;
        btnTeam.Pressed += OnTeamButtonPressed;
        btnRun.Pressed += OnRunButtonPressed;

        panelObjetos = GetNode<Panel>("ParteInferior/BoxResultCombat/Bag/PanelesObjetos");
        panelButtons = bag.GetNode<Panel>("PanelesObjetos/PanelButtons");
        panelObjetosCuracion = panelObjetos.GetNode<Panel>("PanelObjetosCuracion");
        panelObjetosCaptura = panelObjetos.GetNode<Panel>("PanelObjetosCaptura");

        Button btnShowObjectsCaptura = panelObjetos.GetNode<Button>("PanelButtons/ButtonObjetosCaptura");
        Button btnShowObjectsCuracion = panelObjetos.GetNode<Button>("PanelButtons/ButtonObjetosCuracion");

        btnShowObjectsCaptura.Pressed += OnShowCapturaItemsButtonPressed;

        btnPokeball = panelObjetosCaptura.GetNode<Button>("Pokeballs/Pokeball");
        btnSuperball = panelObjetosCaptura.GetNode<Button>("Pokeballs/Superball");
        btnUltraball = panelObjetosCaptura.GetNode<Button>("Pokeballs/Ultraball");
        btnMasterball = panelObjetosCaptura.GetNode<Button>("Pokeballs/Masterball");

        btnPokeball.Pressed += OnLanzarPokeball;
        btnSuperball.Pressed += OnLanzarSuperball;
        btnUltraball.Pressed += OnLanzarUltraball;
        btnMasterball.Pressed += OnLanzarMasterball;

        btnSiConfirmarLanzar = panelObjetosCaptura.GetNode<Button>("ConfirmacionCaptura/btnConfirmar");
        btnNoConfirmarLanzar = panelObjetosCaptura.GetNode<Button>("ConfirmacionCaptura/btnDenegar");

        btnSiConfirmarLanzar.Pressed += OnYesLanzarPokeball;
        btnNoConfirmarLanzar.Pressed += OnNoLanzarPokeball;

        var namePokeEnemy = char.ToUpper(LblNamePokeEnemy.Text[0]) + LblNamePokeEnemy.Text.Substring(1).ToLower();

        combatlog.BbcodeEnabled = true;
        combatlog.Text += $"¡Ha  aparecido  un \u2009 [b]{namePokeEnemy}[/b] \u2009 salvaje!";
    }

    public bool IntentarCaptura()
    {
        var ballBonus = 1.0f;

        switch (stringTypePokeball) {
            case "Pokeball":
                ballBonus = 1.0f;
                break;
            case "Superball":
                ballBonus = 1.5f;
                break;
            case "Ultraball":
                ballBonus = 2.0f;
                break;
            case "Masterball":
                return true;
        }

        float hpMax = pokemonEnemy.maxHP;
        float hpCurrent = Mathf.Max(pokemonEnemy.currentHP, 1); // Evita división por cero
        int catchRate = pokemonEnemy.catchRate;

        float a = ((3 * hpMax - 2 * hpCurrent) * catchRate * ballBonus) / (3 * hpMax);
        
        // Clamp para mantenerlo razonable
        a = Mathf.Clamp(a, 1f, 255f);

        if (a >= 255f)
            return true;

        float b = 1048560f / Mathf.Sqrt(Mathf.Sqrt(16711680f / a));
        
        bool capturado = true;
        for (int i = 0; i < 4; i++)
        {
            if (GD.Randi() % 65536 >= b)
            {
                capturado = false;
                break;
            }
        }

        float probabilidad = Mathf.Pow(b / 65536f, 4) * 100f;
        GD.Print($"📊 Probabilidad: {probabilidad:F2}% | Bola: {stringTypePokeball} | Vida: {((hpMax - hpCurrent) / hpMax):P0}");

        return capturado;
    }

    public void OnYesLanzarPokeball()
    {
        var spriteEnemigo = GetNode<Sprite2D>("PokeEnemy");
        var pokeball = GetNode<AnimatedSprite2D>("Pokeball");
        pokeball.Visible = true;

        GD.Print($"Pokeball elegida {stringTypePokeball}");

        ocultarTodo();
        combatlog.Visible = true;

        pokeball.Play($"lanzamiento_{stringTypePokeball}");
    
        var tween = GetTree().CreateTween();

        // Movimiento inicial hacia el punto objetivo
        Vector2 destino = new Vector2(651, 158);
        tween.TweenProperty(pokeball, "position", destino, 0.9f)
             .SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.Out);

        tween.TweenCallback(Callable.From(() =>
        {
            pokeball.Play($"abierta_{stringTypePokeball}");
        }));

        tween.TweenProperty(spriteEnemigo, "modulate", new Color(0, 0, 0, 1), 0.5f)
             .SetTrans(Tween.TransitionType.Linear)
             .SetEase(Tween.EaseType.InOut);

        tween.TweenInterval(0.1f);

        tween.TweenCallback(Callable.From(() =>
        {
            spriteEnemigo.Visible = false;
        }));

        // Rebote (encadenado automáticamente)
        Vector2 rebote = destino + new Vector2(0, -60);
        tween.TweenProperty(pokeball, "position", rebote, 0.9f)
             .SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.Out);

        tween.TweenCallback(Callable.From(() =>
        {
            pokeball.Play($"idle_{stringTypePokeball}");
        }));
        
        // Rebote (encadenado automáticamente)
        Vector2 bajada = destino + new Vector2(0, 60);
        tween.TweenProperty(pokeball, "position", bajada, 1.2f)
             .SetTrans(Tween.TransitionType.Bounce)
             .SetEase(Tween.EaseType.Out);
        
        // BALANCEO (ROTACIÓN)
        float balanceoGrados = 10f; // cuánto se inclina a cada lado
        float tiempo = 0.2f;
        float intervalo = 0.2f;

        bool captured = IntentarCaptura();

        RandomNumberGenerator rng = new RandomNumberGenerator();
        int toqueResultado = rng.RandiRange(0, 2);

        for (int i = 0; i < 3; i++) {
            if (!captured && toqueResultado == i)
                break;

            // Giro a la izquierda
            tween.TweenProperty(pokeball, "rotation_degrees", -balanceoGrados, tiempo)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.InOut);

            // Giro a la derecha
            tween.TweenProperty(pokeball, "rotation_degrees", balanceoGrados, tiempo * 2)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.InOut);

            // Vuelve al centro
            tween.TweenProperty(pokeball, "rotation_degrees", 0, tiempo)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.InOut);

            // Intervalo entre balanceos
            tween.TweenInterval(intervalo);
        }

        if (captured) {
            GD.Print($"Captured, toques {toqueResultado}");
            tween.TweenCallback(Callable.From(() =>
            {
                var nameEnemyPokemon = char.ToUpper(pokemonEnemy.Nombre[0]) + pokemonEnemy.Nombre.Substring(1).ToLower();
                combatlog.Text += $"\n¡Has  capturado un \u2009 {nameEnemyPokemon} \u2009 salvaje!";

                var effect = pokeball.GetNode<AnimatedSprite2D>("Effects");
                effect.Play("captured");

                // Suscribirse a la señal animation_finished
                effect.AnimationFinished += () =>
                {
                    GD.Print("Efecto 'captured' terminado");

                    // Creamos otro tween tras el efecto
                    var postTween = GetTree().CreateTween();

                    // Intervalo antes de continuar
                    postTween.TweenInterval(2.0f);

                    postTween.TweenCallback(Callable.From(() =>
                    {
                        GD.Print("[🔄️] Añadiendo Pokémon al equipo...");

                        addPokemonDB();

                        TerminarCombate();
                    }));
                };
            }));
        } else {
            tween.TweenCallback(Callable.From(() =>
            {
                GD.Print($"Not captured, toques {toqueResultado}");
                var revertTween = GetTree().CreateTween();

                revertTween.TweenProperty(spriteEnemigo, "modulate", new Color(1, 1, 1, 1), 0.5f)
                           .SetTrans(Tween.TransitionType.Sine)
                           .SetEase(Tween.EaseType.InOut);

                pokeball.Visible = false;
                spriteEnemigo.Visible = true;

                pokeball.Position = new Vector2(29.912f, 416.545f);
            }));
        }
        
    }

    public async void addPokemonDB()
    {
        var pokemonPlayersController = new PokemonPlayersController();
        bool added = await pokemonPlayersController.CapturarPokemon(pokemonEnemy, GetTree().Root.GetNode("Game"));

        if (added) {
            GD.Print($"[➕] Pokémon {pokemonEnemy.Nombre} añadido con éxito.");
        } else {
            GD.Print($"[❌] Problemas al añadir el Pokémon {pokemonEnemy.Nombre}.");
        }
    }

    public void OnNoLanzarPokeball()
    {
        var pokeball = GetNode<AnimatedSprite2D>("Pokeball");

        pokeball.Position = new Vector2(27, 365);
        pokeball.Play("idle");
    }

    public void TerminarCombate()
    {
        // Muestra de nuevo el mapa
        Node mapa = GetTree().GetFirstNodeInGroup("mapa");

        if (mapa == null)
            GD.Print("Mapa null");
        else
            GD.Print("Mapa not null");

        if (mapa is Node2D node2D) {
            node2D.Visible = true;
            GD.Print("Mostrando mapa...");
        } else {
            GD.Print("Mapa not Node2D");
        }

        Node battle = GetTree().Root.GetNodeOrNull<CanvasLayer>("Combate");
        var gameNode = GetNode<Game>("/root/Game");
        var playerNode = GetNode<MainCharacter>("/root/Game/Player");

        playerNode.UnfreezePlayer();
        Game.EstadoJuego = 1;

        // Borra la escena de combate
        if (battle != null) {
            battle.QueueFree();
            GD.Print("Combate terminado.");
        } else {
            GD.Print("Error al acabar el combate.");
        }

    }

    /* Botones */

    public void showCombatLogOrNot(Control node) {

        if (node.Visible == true) {
            ocultarTodo();
            node.Visible = false;
            combatlog.Visible = true;
        } else {
            ocultarTodo();
            GD.Print($"No visible {node.Name}");
            node.Visible = true;
        }
    }

    public void OnFightButtonPressed() {
        GD.Print("Button Fight pressed");

        showCombatLogOrNot(moveset);
    }
    
    public void OnBagButtonPressed() {
        GD.Print("Button Bag pressed");

        panelObjetos.Visible = true;
        showCombatLogOrNot(panelButtons);
    }

    // Botones Panel buttons

    public void OnShowCapturaItemsButtonPressed() {
        GD.Print("Button ShowCapturaItems pressed");
        
        var panelObjectsCaptura = panelObjetos.GetNode<Panel>("PanelObjetosCaptura");
        var panelPokeball = panelObjectsCaptura.GetNode<Panel>("Pokeballs");
        ocultarTodo(panelObjectsCaptura);

        panelButtons.Visible = false;
        panelObjectsCaptura.Visible = true;
        panelPokeball.Visible = true;
    }

    public void lanzarPokeball(Button typePokeball) {
        var typeOfPokeball = typePokeball.GetNode<Label>("LblName");
        var color = "black";

        switch (typeOfPokeball.Text) {
            case "Pokeball":
                stringTypePokeball = "Pokeball";
                color = "red";
                break;
            case "Superball":
                stringTypePokeball = "Superball";
                color = "blue";
                break;
            case "Ultraball":
                stringTypePokeball = "Ultraball";
                color = "yellow";
                break;
            case "Masterball":
                stringTypePokeball = "Masterball";
                color = "purple";
                break;
        }

        var panelObjectsCaptura = panelObjetos.GetNode<Panel>("PanelObjetosCaptura");

        var panelPokeball = panelObjectsCaptura.GetNode<Panel>("Pokeballs");
        var confirmarCaptura = panelObjectsCaptura.GetNode<Panel>("ConfirmacionCaptura");

        panelPokeball.Visible = false;

        var lblConfirmacion = confirmarCaptura.GetNode<RichTextLabel>("LblInfoConfirmacion");
        lblConfirmacion.BbcodeEnabled = true;
        lblConfirmacion.Text = $"¿Estas  seguro  de  usar la  [b][color={color}] {typeOfPokeball.Text}[/color][/b]?";

        confirmarCaptura.Visible = true;
    }

    public void OnLanzarPokeball() {
        GD.Print("Button OnLanzarPokeball pressed");

        stringTypePokeball = "Pokeball";

        lanzarPokeball(btnPokeball);
    }

    public void OnLanzarSuperball() {
        stringTypePokeball = "Superball";

        lanzarPokeball(btnSuperball);
    }

    public void OnLanzarUltraball() {
        stringTypePokeball = "Ultraball";

        lanzarPokeball(btnUltraball);
    }

    public void OnLanzarMasterball() {
        stringTypePokeball = "Masterball";

        lanzarPokeball(btnMasterball);
    }

    public void OnTeamButtonPressed() {
        GD.Print("Button Team pressed");
        
        
    }

    public void OnRunButtonPressed() {
        GD.Print("Button Huir pressed");

        TerminarCombate();
    }

    public void ocultarTodo(Node node) {
        moveset.Visible = false;
        combatlog.Visible = false;

        if (bag.IsAncestorOf(node)) {
            bag.Visible = true;
        } else {
            bag.Visible = false;
        }
    }

    public void ocultarTodo() {
        var panelConfirmarCaptura = panelObjetos.GetNode<Panel>("PanelObjetosCaptura/ConfirmacionCaptura");

        moveset.Visible = false;
        combatlog.Visible = false;

        panelButtons.Visible = false;
        panelObjetosCuracion.Visible = false;
        panelObjetosCaptura.Visible = false;
        panelConfirmarCaptura.Visible = false;
    }
}