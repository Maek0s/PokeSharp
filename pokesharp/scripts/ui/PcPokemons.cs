using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class PcPokemons : CanvasLayer
{
    private static List<Pokemon> listPokemons = new List<Pokemon>();

    private Pokemon pokemonSelected = new Pokemon();
    private static Panel panelPokemonSelected;
    public static int NumPokemonSelected = -1;
    public static int NumPokemonSelectedToChange = -1;

    private Panel panelPokemons;
    private Panel panelTuEquipo;
    public static int numCaja = 1;

    private Label lblInfo;
    private Button btnSalir;
    private Button btnAddTeam;
    private Button btnConfirmarChange;
    private Button btnCancelarChange;
    private Button btnBackCaja;
    private Button btnNextCaja;

    public override async void _Ready()
    {
        panelPokemons = GetNode<Panel>("ContainerScreen/ListPokemons/Pokemons");
        panelTuEquipo = GetNode<Panel>("ContainerScreen/ListPokemons/TuEquipo");
        panelPokemonSelected = GetNode<Panel>("ContainerScreen/PanelOptions/PanelPokemonSelected");

        lblInfo = GetNode<Label>("ContainerScreen/PanelOptions/ButtonsOptions/LblInfo");

        lblInfo.Text = "";

        changeCaja();
        
        // Botones

        btnAddTeam = GetNode<Button>("ContainerScreen/PanelOptions/ButtonsOptions/btnAddTeam");
        btnAddTeam.Pressed += async () => await OnBtnAddTeamPressed();
        
        btnSalir = GetNode<Button>("ContainerScreen/PanelOptions/ButtonsOptions/btnSalir");
        btnSalir.Pressed += OnBtnSalirPressed;

        btnConfirmarChange = panelTuEquipo.GetNode<Button>("btnConfirmar");
        btnConfirmarChange.Pressed += OnBtnConfirmarChangePressed;

        btnCancelarChange = panelTuEquipo.GetNode<Button>("btnCancelar");
        btnCancelarChange.Pressed += OnBtnCancelarChangePressed;

        btnBackCaja = GetNode<Button>("ContainerScreen/ListPokemons/NumeroCaja/btnAtrasCaja");
        btnBackCaja.Pressed += OnBtnBackCajaPressed;

        btnNextCaja = GetNode<Button>("ContainerScreen/ListPokemons/NumeroCaja/btnDelanteCaja");
        btnNextCaja.Pressed += OnBtnNextCajaPressed;
    }

    public void OnBtnNextCajaPressed()
    {
        if (Math.Ceiling((double) Game.PlayerPlaying.listPokemonsCaja.Count / 24.0) > numCaja) {
            numCaja++;
            changeCaja();
        }
    }

    public void OnBtnBackCajaPressed()
    {
        if (numCaja > 1) {
            numCaja--;
            changeCaja();
        }
    }

    public void changeCaja()
    {
        var lblTextCaja = GetNode<Label>("ContainerScreen/ListPokemons/NumeroCaja/lblNumCaja");
        lblTextCaja.Text = $"Caja  {numCaja}";
        mostrarPokemons();
    }

    public async Task OnBtnAddTeamPressed()
    {
        Player player = Game.PlayerPlaying;

        if (NumPokemonSelected == -1) {
            lblInfo.Text = "¡Selecciona  un  pokemon!";
            return;
        }

        Pokemon pokemonSelected = player.listPokemonsCaja[NumPokemonSelected];

        if (player.listPokemonsTeam.Count < 6) {
            GD.Print("player.listPokemonsTeam.Count < 6");
            
            if (pokemonSelected != null && !string.IsNullOrWhiteSpace(pokemonSelected.Nombre)) {
                // 1) Añade al equipo en local + backend
                GD.Print($"Intentando añadir: \n{pokemonSelected}");
                bool added = await player.AddPokeTeamAsync(pokemonSelected, GetTree().Root.GetNode("Game"));
                if (!added) {
                    GD.PrintErr($"No se pudo añadir al equipo {pokemonSelected.NombreCamelCase}.");
                    return;
                } else {
                    GD.Print($"Se añadió al equipo {pokemonSelected.NombreCamelCase}.");
                }

                player.RemovePokeBox(pokemonSelected, GetTree().Root.GetNode("Game"));

                changeNumSelected(-1);
            }
        } else {
            GD.Print("player.listPokemonsTeam.Count >= 6");

            lblInfo.Text = "";
            panelPokemons.Visible = false;
            panelTuEquipo.Visible = true;

            for (int i = 1; i < player.listPokemonsTeam.Count + 1; i++) {
                Pokemon pokemon = player.listPokemonsTeam[i - 1];
                //GD.Print($"Pokemon mostrando {pokemon.NombreCamelCase}");

                var panelPoke = GetNode<Panel>($"ContainerScreen/ListPokemons/TuEquipo/PanelPoke{i}");
                var sprite = panelPoke.GetNode<Sprite2D>("Sprite");
                
                sprite.Texture = GD.Load<Texture2D>($"res://assets/spritespc/{pokemon.Nombre}.png");
            }
        }
    }

    public void OnBtnCancelarChangePressed()
    {
        panelTuEquipo.Visible = false;
        panelPokemons.Visible = true;

        NumPokemonSelectedToChange = -1;
    }

    public async void OnBtnConfirmarChangePressed()
    {
        GD.Print("(OnBtnConfirmarChangePressed) Entrado");

        if (NumPokemonSelectedToChange < 0 ||
            Game.PlayerPlaying.listPokemonsTeam.Count < NumPokemonSelectedToChange) {
                return;
            }
        
        Pokemon pokemonTeam = Game.PlayerPlaying.listPokemonsTeam[NumPokemonSelectedToChange];

        GD.Print($"pokemonTeam: {pokemonTeam}");

        Pokemon pokemonSelected = Game.PlayerPlaying.listPokemonsCaja[NumPokemonSelected];

        GD.Print($"NumSelected {NumPokemonSelected} pokemonSelected: {pokemonSelected}");

        var root = GetTree().Root.GetNode("Game");

        // 1) Añadimos a la caja y esperamos a que termine
        bool ok1 = await Game.PlayerPlaying.AddPokeBoxAsync(pokemonTeam, root);
        if (!ok1)
        {
            GD.PrintErr("Error al añadir a la caja el pokemon que estaba en el equipo.");
            return;
        }

        // 2) Eliminamos del equipo
        Game.PlayerPlaying.RemovePokeTeam(pokemonTeam, root);


        // 3) Añadimos al equipo al pokémon seleccionado
        GD.Print("AddPokeTeamAsync()");
        bool ok3 = await Game.PlayerPlaying.AddPokeTeamAsync(pokemonSelected, root);
        if (!ok3)
        {
            GD.PrintErr("Error al añadir al equipo el pokemon seleccionado.");
            return;
        }

        // 4) Eliminamos de la caja al pokémon que va a sustituir
        Game.PlayerPlaying.RemovePokeBox(pokemonSelected, root);

        GD.Print("Cambio completado con éxito.");

        // Ejecutamos el botón salir de forma forzosa para salir del menú
        OnBtnSalirPressed();
    }
    

    public void OnBtnSalirPressed()
    {
        panelTuEquipo.Visible = false;
        panelPokemons.Visible = true;

        changeNumSelectedYourTeam(-1, this);
        deseleccionar();

        lblInfo.Text = "";
        Visible = false;
    }

    public void getPokemonsCaja()
    {
        if (listPokemons.Count <= 0) {
            listPokemons = Game.PlayerPlaying.listPokemonsCaja;
            mostrarPokemons();
        }
    }

    public void updateBox()
    {
        listPokemons = Game.PlayerPlaying.listPokemonsCaja;

        GD.Print("updateBox()");
/*
        foreach (Pokemon item in listPokemons)
        {
            GD.Print(item);
        }
*/
        mostrarPokemons();
    }

    public void mostrarPokemons()
    {
        int inicio = 0;

        if (numCaja == 1) {
            inicio = 0;
        } else {
            inicio = 24 * numCaja - 24;
        }

        // para seguir con el orden de 1 hasta 24
        int contador = 1;

        for (int i = inicio; i < 24 * numCaja; i++) {
            if (contador > 24)
                break;

            var panelPoke = panelPokemons.GetNode<Panel>($"PanelPoke{contador}");
            var sprite = panelPoke.GetNode<Sprite2D>("Sprite");

            if (i < listPokemons.Count) {
                //GD.Print($"i: {i} - Count: {listPokemons.Count}");
                Pokemon pokemon = listPokemons[i];

                sprite.Texture = GD.Load<Texture2D>($"res://assets/spritespc/{pokemon.Nombre}.png");
            } else {
                sprite.Texture = null;
            }

            contador++;
        }

        GD.Print("");
    }

    public static void changeNumSelectedYourTeam(int newNum, Node sceneRoot)
    {
        NumPokemonSelectedToChange = newNum - 1;

        GD.Print($"selected {NumPokemonSelectedToChange} - {Game.PlayerPlaying.listPokemonsTeam.Count}");

        for (int i = 1; i < Game.PlayerPlaying.listPokemonsTeam.Count + 1; i++)
        {
            var panel = sceneRoot.GetNode<Panel>($"ContainerScreen/ListPokemons/TuEquipo/PanelPoke{i}");
            panel.Modulate = new Color(1, 1, 1);
        }

        if (NumPokemonSelectedToChange < 0 || NumPokemonSelectedToChange > 5)
            return;

        var selectedPanel = sceneRoot.GetNode<Panel>($"ContainerScreen/ListPokemons/TuEquipo/PanelPoke{NumPokemonSelectedToChange + 1}");
        selectedPanel.Modulate = new Color(0.962f, 0.576f, 0.569f);
    }

    public static void changeNumSelected(int numero)
    {
        NumPokemonSelected = numero - 1;

        if (numCaja != 1)
            NumPokemonSelected += 24 * (numCaja - 1);
        
        if (NumPokemonSelected >= listPokemons.Count)
            return;

        GD.Print($"selected {NumPokemonSelected} - {listPokemons.Count}");

        var lblPokemonNameSlctd = panelPokemonSelected.GetNode<Label>("BasicInfo/PokemonSpriteSelected/PokemonSelectedNameLevel");
        Pokemon pokemonSelected = new Pokemon();

        // Si se deselecciona
        if (numero == -1) {
            return;
        } else {
            pokemonSelected = listPokemons[NumPokemonSelected];
        }

        string nombreFormateado = char.ToUpper(pokemonSelected.Nombre[0]) + pokemonSelected.Nombre.Substring(1).ToLower();

        lblPokemonNameSlctd.Text = $"{nombreFormateado} \u2009 \u2009 Nv. \u2009 {pokemonSelected.nivel}";
        var sprite = panelPokemonSelected.GetNode<Sprite2D>("BasicInfo/PokemonSpriteSelected/Panel/Sprite");
        sprite.Texture = GD.Load<Texture2D>($"res://assets/spritespc/{pokemonSelected.Nombre}.png");

        var progressBar = panelPokemonSelected.GetNode<ProgressBar>("BasicInfo/PokemonSpriteSelected/ProgressBar");
        var hpPokemon = progressBar.GetNode<Label>("Label");

        GeneralUtils.AsignValuesProgressBar(progressBar, hpPokemon, pokemonSelected);

        var panelStatsInfo = panelPokemonSelected.GetNode<Panel>("StatsInfo");
        var atk = panelStatsInfo.GetNode<Label>("Atk");
        atk.Text = $"ATK: {pokemonSelected.atk}";
        
        var def = panelStatsInfo.GetNode<Label>("Def");
        def.Text = $"DEF: {pokemonSelected.def}";

        var spAtk = panelStatsInfo.GetNode<Label>("SpAtk");
        spAtk.Text = $"SP ATK: {pokemonSelected.sp_atk}";

        var spDef = panelStatsInfo.GetNode<Label>("SpDef");
        spDef.Text = $"SP DEF: {pokemonSelected.sp_def}";

        var speed = panelStatsInfo.GetNode<Label>("Speed");
        speed.Text = $"SPEED: {pokemonSelected.speed}";
    }

    public void deseleccionar()
    {
        pokemonSelected = new Pokemon();
        NumPokemonSelected = -1;

        var lblPokemonNameSlctd = panelPokemonSelected.GetNode<Label>("BasicInfo/PokemonSpriteSelected/PokemonSelectedNameLevel");

        lblPokemonNameSlctd.Text = $"";
        var sprite = panelPokemonSelected.GetNode<Sprite2D>("BasicInfo/PokemonSpriteSelected/Panel/Sprite");
        sprite.Texture = null;

        var progressBar = panelPokemonSelected.GetNode<ProgressBar>("BasicInfo/PokemonSpriteSelected/ProgressBar");
        var hpPokemon = progressBar.GetNode<Label>("Label");

        GeneralUtils.AsignValuesProgressBar(progressBar, hpPokemon, null);

        hpPokemon.Text = "";

        var panelStatsInfo = panelPokemonSelected.GetNode<Panel>("StatsInfo");
        var atk = panelStatsInfo.GetNode<Label>("Atk");
        atk.Text = "ATK: ";
        
        var def = panelStatsInfo.GetNode<Label>("Def");
        def.Text = "DEF: ";

        var spAtk = panelStatsInfo.GetNode<Label>("SpAtk");
        spAtk.Text = "SP ATK: ";

        var spDef = panelStatsInfo.GetNode<Label>("SpDef");
        spDef.Text = "SP DEF: ";

        var speed = panelStatsInfo.GetNode<Label>("Speed");
        speed.Text = "SPEED: ";
    }

}
