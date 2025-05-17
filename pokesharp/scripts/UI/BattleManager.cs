using Godot;
using System;
using System.Threading.Tasks;

public partial class BattleManager : CanvasLayer {

    Pokemon pokemonAlly;
    Pokemon pokemonEnemy;
    PackedScene transitionScene;
    CanvasLayer battle;
    public int TurnoActual { get; set; } = 0;
    public bool isFight = false;
    TransitionManager transitionManager;

    private Control moveset;
    private RichTextLabel combatlog;

    private Control bag;
    private Panel panelObjetos;
    private Panel panelObjetosCuracion;
    private Panel panelObjetosCaptura;
    private Panel panelButtons;
    private Panel tuEquipoPanel;

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

    Button btnMove1;
    Button btnMove2;
    Button btnMove3;
    Button btnMove4;

    Button btnChangePoke1;
    Button btnChangePoke2;
    Button btnChangePoke3;
    Button btnChangePoke4;
    Button btnChangePoke5;
    Button btnChangePoke6;

    String stringTypePokeball = "Pokeball";

    public enum TurnoEstado
    {
        Jugador,
        Enemigo,
        EsperandoAccion,
        EsperandoChange,
        CombateTerminado
    }

    public TurnoEstado turnoActual = TurnoEstado.Jugador;

    public override void _Ready()
    {
        if (!Game.fight)
            pokemonEnemy = Game.PokemonInFight;

        if (Game.PlayerPlaying.listPokemonsTeam.Count == 0) {
            pokemonAlly = new Pokemon();
            pokemonAlly.nivel = 1;
            pokemonAlly.Nombre = "charmander";
            pokemonAlly.NombreCamelCase = "Charmander";
        } else {
            pokemonAlly = Game.PlayerPlaying.listPokemonsTeam[0];
            foreach (Pokemon pokeTeam in Game.PlayerPlaying.listPokemonsTeam) {
                if (pokeTeam.currentHP > 0) {
                    pokemonAlly = pokeTeam;
                    RefreshHPBar(false);
                    break;
                }
            }
        }

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
        btnTeam.Pressed += async () => await OnTeamButtonPressedAsync();
        btnRun.Pressed += OnRunButtonPressed;

        panelObjetos = GetNode<Panel>("ParteInferior/BoxResultCombat/Bag/PanelesObjetos");
        panelButtons = bag.GetNode<Panel>("PanelesObjetos/PanelButtons");
        panelObjetosCuracion = panelObjetos.GetNode<Panel>("PanelObjetosCuracion");
        panelObjetosCaptura = panelObjetos.GetNode<Panel>("PanelObjetosCaptura");

        tuEquipoPanel = GetNode<Panel>("ParteInferior/TuEquipo");

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

        var movesetControl = GetNode<Control>("ParteInferior/BoxResultCombat/Moveset");

        btnMove1 = movesetControl.GetNode<Button>("firstMove");
        btnMove2 = movesetControl.GetNode<Button>("secondMove");
        btnMove3 = movesetControl.GetNode<Button>("thirdMove");
        btnMove4 = movesetControl.GetNode<Button>("fourthMove");

        btnMove1.Pressed += () => OnPressAttack(0);
        btnMove2.Pressed += () => OnPressAttack(1);
        btnMove3.Pressed += () => OnPressAttack(2);
        btnMove4.Pressed += () => OnPressAttack(3);

        btnChangePoke1 = tuEquipoPanel.GetNode<Button>("Poke1/btnChangePoke");
        btnChangePoke2 = tuEquipoPanel.GetNode<Button>("Poke2/btnChangePoke");
        btnChangePoke3 = tuEquipoPanel.GetNode<Button>("Poke3/btnChangePoke");
        btnChangePoke4 = tuEquipoPanel.GetNode<Button>("Poke4/btnChangePoke");
        btnChangePoke5 = tuEquipoPanel.GetNode<Button>("Poke5/btnChangePoke");
        btnChangePoke6 = tuEquipoPanel.GetNode<Button>("Poke6/btnChangePoke");

        btnChangePoke1.Pressed += () => changePokemonAlly(0);
        btnChangePoke2.Pressed += () => changePokemonAlly(1);
        btnChangePoke3.Pressed += () => changePokemonAlly(2);
        btnChangePoke4.Pressed += () => changePokemonAlly(3);
        btnChangePoke5.Pressed += () => changePokemonAlly(4);
        btnChangePoke6.Pressed += () => changePokemonAlly(5);

        var namePokeEnemy = char.ToUpper(LblNamePokeEnemy.Text[0]) + LblNamePokeEnemy.Text.Substring(1).ToLower();

        combatlog.BbcodeEnabled = true;
        combatlog.Text += $"¡Ha  aparecido  un \u2009 [b]{namePokeEnemy}[/b] \u2009 salvaje!";

        combatlog.Visible = true;

        _ = setVisualTeam();

        setVisualMoveset();

        EmpezarBatalla();
    }

    public async void EmpezarBatalla()
    {
        if (pokemonAlly.speed >= pokemonEnemy.speed)
            turnoActual = TurnoEstado.Jugador;
        else
            turnoActual = TurnoEstado.Enemigo;

        // Esperar 1 segundo antes de iniciar el turno
        await ToSignal(GetTree().CreateTimer(1.5f), "timeout");

        IniciarTurno();
    }

    public void IniciarTurno()
    {
        switch (turnoActual)
        {
            case TurnoEstado.Jugador:
                GD.Print("Turno del jugador");
                break;

            case TurnoEstado.Enemigo:
                GD.Print("Turno del enemigo");
                AtaqueEnemigo();
                break;
            
            case TurnoEstado.EsperandoChange:
                GD.Print("Tienes que elegir un pokémon");
                updateVisualTeam();
                
                ocultarTodo();
                tuEquipoPanel.Visible = true;
                break;

            case TurnoEstado.CombateTerminado:
                GD.Print("Combate terminado");
                var sceneRoot = GetTree().Root.GetNode("Game");
                var scriptMenuPrincipal = sceneRoot.GetNode<MenuPrincipal>("/root/Game/inScreen/UI/MenuPrincipal");
                scriptMenuPrincipal.ColocarPokemonsVisual();

                EjecutarCombateTerminadoDelay(1.5f);
                break;
        }
    }

    public async void EjecutarCombateTerminadoDelay(float segundos)
    {
        await ToSignal(GetTree().CreateTimer(segundos), "timeout");
        GD.Print("Combate terminado");
        TerminarCombate();
    }

    private async void AtaqueEnemigo()
    {
        // Pequeño delay antes de iniciar el ataque enemigo
        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

        GD.Print("pokeEnemy Moves Count", pokemonEnemy.Movimientos.Count);
        // Elegir movimiento enemigo aleatoriamente
        var movimientosEnemigo = pokemonEnemy.Movimientos;

        RandomNumberGenerator random = new RandomNumberGenerator();
        int indexMovimiento = random.RandiRange(0, 3);

        GD.Print("indexMov ", indexMovimiento);

        int daño = await pokemonEnemy.AtacarPokemon(pokemonAlly, movimientosEnemigo[indexMovimiento]);

        if (daño == -1) {
            combatlog.Text += $"\nEl enemigo \u2009 uso \u2009 {GeneralUtils.FormatearTexto(movimientosEnemigo[indexMovimiento].move_name)}.";

            combatlog.Text += $"\nPero fallo.";

            turnoActual = TurnoEstado.Jugador;
            IniciarTurno();
            
            return;
        }

        // Aplicar daño al aliado
        pokemonAlly.currentHP -= daño;

        if (pokemonAlly.currentHP < 0)
            pokemonAlly.currentHP = 0;
        RefreshHPBar(false);

        combatlog.Text += $"\nEl \u2009 enemigo \u2009 uso \u2009 {GeneralUtils.FormatearTexto(movimientosEnemigo[indexMovimiento].move_name)} \u2009 y \u2009 resto \u2009 {daño} \u2009 de \u2009 vida.";

        // Verificar si aliado quedó sin HP
        if (pokemonAlly.currentHP <= 0)
        {
            combatlog.Text += $"\nTu \u2009 {pokemonAlly.NombreCamelCase} \u2009 ha \u2009 sido \u2009 derrotado.";

            if (Game.PlayerPlaying.checkVivos())
            {
                turnoActual = TurnoEstado.EsperandoChange;
            }
            else
            {
                turnoActual = TurnoEstado.CombateTerminado;
            }
        } else {
            // Pasar turno al jugador
            turnoActual = TurnoEstado.Jugador;
        }

        IniciarTurno();
    }

    public void changePokemonAlly(int slotTeam)
    {
        if (turnoActual != TurnoEstado.Jugador || slotTeam >= Game.PlayerPlaying.listPokemonsTeam.Count)
        {
            if (turnoActual != TurnoEstado.EsperandoChange)
            {
                GD.Print("turnoActual != TurnoEstado.Jugador || slotTeam >= Game.PlayerPlaying.listPokemonsTeam.Count");
                return;
            }
        }

        if (turnoActual != TurnoEstado.EsperandoChange)
            turnoActual = TurnoEstado.EsperandoAccion;

        Pokemon pokemonSelected = Game.PlayerPlaying.listPokemonsTeam[slotTeam];

        if (pokemonSelected.currentHP <= 0)
        {
            GD.Print("El pokémon esta sin HP!");
            turnoActual = TurnoEstado.Jugador;
            return;
        }
        else if (pokemonSelected == pokemonAlly)
        {
            GD.Print("Pokémon en juego igual al escogido!");
            turnoActual = TurnoEstado.Jugador;
            return;
        }

        ocultarTodo();
        combatlog.Visible = true;

        pokemonAlly = pokemonSelected;

        setVisualMoveset();

        setPokeVisual(false);

        turnoActual = TurnoEstado.Enemigo;
        IniciarTurno();
    }

    public void setPokeVisual(bool isEnemyPokemon)
    {
        var panelInfo = new Panel();
        Pokemon pokemon = new Pokemon();
        var sprite = new Sprite2D();

        if (isEnemyPokemon)
        {
            panelInfo = GetNode<Panel>("InfoEnemy");
            pokemon = pokemonEnemy;
            sprite = GetNode<Sprite2D>("PokeEnemy");
            sprite.Texture = GD.Load<Texture2D>($"res://assets/pokemons/front/{pokemon.Nombre.ToUpper()}.png");
        }
        else
        {
            panelInfo = GetNode<Panel>("InfoAlly");
            pokemon = pokemonAlly;
            sprite = GetNode<Sprite2D>("PokeAlly");
            sprite.Texture = GD.Load<Texture2D>($"res://assets/pokemons/back/{pokemon.Nombre.ToUpper()}.png");
        }

        var lblLevelPoke = panelInfo.GetNode<Label>("levelPokemon");
        var lblNamePoke = panelInfo.GetNode<Label>("namePokemon");
        var progressBar = panelInfo.GetNode<ProgressBar>("Panel/ProgressBar");

        lblLevelPoke.Text = $"Lv{pokemon.nivel}";
        lblNamePoke.Text = pokemon.Nombre.ToUpper();
        GeneralUtils.AsignValuesProgressBarNoAnimation(progressBar, pokemon);
    }

    public void updateVisualTeam()
    {
        for (int i = 1; i < 7; i++)
        {
            var panelPoke = tuEquipoPanel.GetNode<Panel>($"Poke{i}");

            if (Game.PlayerPlaying.listPokemonsTeam.Count > i - 1)
            {
                Pokemon pokemon = Game.PlayerPlaying.listPokemonsTeam[i - 1];
                var lblNamePokemon = panelPoke.GetNode<Label>("namePokemon");
                var progressBar = panelPoke.GetNode<ProgressBar>("ProgressBar");

                if (pokemon.NombreCamelCase == lblNamePokemon.Text)
                {
                    progressBar = panelPoke.GetNode<ProgressBar>("ProgressBar");

                    GeneralUtils.AsignValuesProgressBarNoAnimation(progressBar, pokemon);
                    continue;
                }
            }
        }
    }

    public async Task setVisualTeam()
    {
        for (int i = 1; i < 7; i++)
        {
            var panelPoke = tuEquipoPanel.GetNode<Panel>($"Poke{i}");

            if (Game.PlayerPlaying.listPokemonsTeam.Count > i - 1)
            {
                Pokemon pokemon = Game.PlayerPlaying.listPokemonsTeam[i - 1];
                var lblNamePokemon = panelPoke.GetNode<Label>("namePokemon");
                var progressBar = panelPoke.GetNode<ProgressBar>("ProgressBar");

                if (pokemon.NombreCamelCase == lblNamePokemon.Text)
                {
                    progressBar = panelPoke.GetNode<ProgressBar>("ProgressBar");

                    GeneralUtils.AsignValuesProgressBarNoAnimation(progressBar, pokemon);
                    continue;
                }

                lblNamePokemon.Text = pokemon.NombreCamelCase;

                GeneralUtils.AsignValuesProgressBarNoAnimation(progressBar, pokemon);

                PokemonTypesController pokemonTypesController = new PokemonTypesController();
                var typeList = await pokemonTypesController.GetTypesByPokemonId(pokemon.Id);

                Color colorPanel;

                if (typeList.Count <= 0 || typeList == null)
                    colorPanel = Colors.Gray;
                else
                {
                    var typeName = TypesUtils.getTypeName(typeList[0]);
                    colorPanel = GeneralUtils.GetColorByType(typeName);
                }

                StyleBoxFlat style = new StyleBoxFlat();
                style.BgColor = colorPanel;

                panelPoke.AddThemeStyleboxOverride("panel", style);

                panelPoke.Visible = true;
            }
            else
            {
                panelPoke.Visible = false;
            }
        }
    }

    public void setVisualMoveset()
    {
        btnMove1.Visible = false;
        btnMove2.Visible = false;
        btnMove3.Visible = false;
        btnMove4.Visible = false;

        for (int i = 0; i < pokemonAlly.Movimientos.Count + 1; i++)
        {
            if (i >= pokemonAlly.Movimientos.Count)
                break;

            Movimiento movimiento = pokemonAlly.Movimientos[i];

            switch (i)
            {
                case 0:
                    btnMove1.Text = GeneralUtils.FormatearTexto(pokemonAlly.Movimientos[0].move_name);

                    btnMove1 = GeneralUtils.SetButtonSettings(btnMove1, movimiento.type_name);
                    break;
                case 1:
                    btnMove2.Text = GeneralUtils.FormatearTexto(pokemonAlly.Movimientos[1].move_name);

                    btnMove2 = GeneralUtils.SetButtonSettings(btnMove2, movimiento.type_name);
                    break;
                case 2:
                    btnMove3.Text = GeneralUtils.FormatearTexto(pokemonAlly.Movimientos[2].move_name);

                    btnMove3 = GeneralUtils.SetButtonSettings(btnMove3, movimiento.type_name);
                    break;
                case 3:
                    btnMove4.Text = GeneralUtils.FormatearTexto(pokemonAlly.Movimientos[3].move_name);

                    btnMove4 = GeneralUtils.SetButtonSettings(btnMove4, movimiento.type_name);
                    break;
            }
        }
    }

    public void RefreshHPBar(bool isEnemyBar) {
        Panel infoPokemon;
        Pokemon pokemonSelected;

        if (isEnemyBar) {
            infoPokemon = GetNode<Panel>("InfoEnemy");
            pokemonSelected = pokemonEnemy;
        } else {
            infoPokemon = GetNode<Panel>("InfoAlly");
            pokemonSelected = pokemonAlly;
        }

        var progressBar = infoPokemon.GetNode<ProgressBar>("Panel/ProgressBar");

        GeneralUtils generalUtils = new GeneralUtils();

        AddChild(generalUtils);

        generalUtils.AsignValuesProgressBar(progressBar, pokemonSelected);
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

    public void Attacked(int daño)
    {
        pokemonEnemy.currentHP -= daño;
        if (pokemonEnemy.currentHP < 0)
            pokemonEnemy.currentHP = 0;

        RefreshHPBar(true);
    }

    public async void OnPressAttack(int moveIndex)
    {
        if (turnoActual != TurnoEstado.Jugador)
            return;

        moveset.Visible = false;
        combatlog.Visible = true;

        GD.Print($"Pressed move{moveIndex}");

        combatlog.Text += $"\nUsaste \u2009 {GeneralUtils.FormatearTexto(pokemonAlly.Movimientos[moveIndex].move_name)}.";

        int daño = await pokemonAlly.AtacarPokemon(pokemonEnemy, pokemonAlly.Movimientos[moveIndex]);

        if (daño == -1) {
            combatlog.Text += $"\nUsaste \u2009 {GeneralUtils.FormatearTexto(pokemonAlly.Movimientos[moveIndex].move_name)}.";

            combatlog.Text += $"\nPero fallo.";

            return;
        }

        Attacked(daño);

        GD.Print("pokeEnemy currentHP ", pokemonEnemy.currentHP);
        GD.Print($"Ataque con {pokemonAlly.Movimientos[moveIndex]} daño {daño}");

        if (pokemonEnemy.currentHP <= 0)
        {
            combatlog.Text += $"\n¡Has \u2009 derrotado \u2009 a \u2009 {pokemonEnemy.NombreCamelCase}!";
            turnoActual = TurnoEstado.CombateTerminado;
        } else {
            // Cambiar turno al enemigo
            turnoActual = TurnoEstado.Enemigo;
        }

        IniciarTurno();
    }

    public void OnYesLanzarPokeball()
    {
        if (turnoActual != TurnoEstado.Jugador)
            return;

        turnoActual = TurnoEstado.EsperandoAccion;

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
                    Game.StartCoroutine(-80.0f, GetTree().Root.GetNode("Game"));

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

                turnoActual = TurnoEstado.Enemigo;

                IniciarTurno();
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
        ocultarTodo();
        combatlog.Visible = true;
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
        showCombatLogOrNot(moveset);
    }

    public async Task OnTeamButtonPressedAsync()
    {
        GD.Print("Button Team pressed");
        await setVisualTeam();

        showCombatLogOrNot(tuEquipoPanel);
    }
    
    public void OnBagButtonPressed()
    {
        panelObjetos.Visible = true;
        showCombatLogOrNot(panelButtons);
    }
    
    public void OnRunButtonPressed()
    {
        if (turnoActual != TurnoEstado.Jugador)
            return;

        GD.Print("Button Huir pressed");
        Game.StartCoroutine(-80.0f, GetTree().Root.GetNode("Game"));

        TerminarCombate();
    }

    // Botones Panel buttons

    public void OnShowCapturaItemsButtonPressed()
    {
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

    

    public void RefreshHPBarsTeam()
    {
        for (int i = 1; i < Game.PlayerPlaying.listPokemonsTeam.Count + 1; i++)
        {
            var panelPoke = tuEquipoPanel.GetNode<Panel>($"Poke{i}");

            Pokemon pokemon = Game.PlayerPlaying.listPokemonsTeam[i - 1];

            var progressBar = panelPoke.GetNode<ProgressBar>("ProgressBar");

            GeneralUtils.AsignValuesProgressBarNoAnimation(progressBar, pokemon);
        }
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
        tuEquipoPanel.Visible = false;

        panelButtons.Visible = false;
        panelObjetosCuracion.Visible = false;
        panelObjetosCaptura.Visible = false;
        panelConfirmarCaptura.Visible = false;
    }
}