using Godot;
using System;
using System.Threading.Tasks;

public partial class PaginaInicioUI : Node2D
{
    private Button btnStart;
    private Button btnSettings;
    private Button btnExit;

    // Inicio y Registro de usuario

    private Label lblError;
    private LineEdit nickname;
    private LineEdit password;

    private Button btnRegistrarse;
    private Button btnIniciarSesion;
    private Button btnYesAccount;
    private Button btnNoAccount;
    private Button btnMuteUnmuteMusic;

    private Node2D musicManager;
    public AudioStreamPlayer2D musicBackground;
    private Sprite2D iconSpeaker;

    public override void _Ready()
    {
        var botones = GetNode<Control>("Buttons");

        musicManager = GetNode<Node2D>("MusicManager");
        musicBackground = musicManager.GetNode<AudioStreamPlayer2D>("musicBackground");
        iconSpeaker = musicManager.GetNode<Sprite2D>("iconSpeaker");

        // Inicio y Registro de usuario

        var credentialsPanel = GetNode<Control>("Credenciales/Panel");
        var credentialsButtons = GetNode<Control>("Credenciales/Panel/Botones");

        // Variables de textedits

        lblError = credentialsPanel.GetNode<Label>("Error");
        nickname = credentialsPanel.GetNode<LineEdit>("Nickname");
        password = credentialsPanel.GetNode<LineEdit>("Password");

        nickname.Text = "";
        password.Text = "";

        // Botones //

        btnStart = botones.GetNode<Button>("StartButton");
        btnSettings = botones.GetNode<Button>("AjustesButton");
        btnExit = botones.GetNode<Button>("SalirButton");

        btnStart.Pressed += OnStartButtonPressed;
        btnSettings.Pressed += OnSettingsButtonPressed;
        btnExit.Pressed += OnExitButtonPressed;

        btnRegistrarse = credentialsButtons.GetNode<Button>("Registrar");
        btnIniciarSesion = credentialsButtons.GetNode<Button>("Entrar");
        btnNoAccount = credentialsButtons.GetNode<Button>("btnNoAccount");
        btnYesAccount = credentialsButtons.GetNode<Button>("btnYesAccount");

        btnRegistrarse.Pressed += OnBtnRegistrarsePressedAsync;
        btnIniciarSesion.Pressed += OnBtnIniciarSesionPressed;
        btnNoAccount.Pressed += OnBtnNoAccountPressed;
        btnYesAccount.Pressed += OnBtnYesAccountPressed;

        btnMuteUnmuteMusic = musicManager.GetNode<Button>("btnMuteUnmute");
        btnMuteUnmuteMusic.Pressed += OnMuteUnmuteMusicPressed;
    }

    public void OnMuteUnmuteMusicPressed()
    {
        if (musicBackground.VolumeDb == -9999.0f) {
            musicBackground.VolumeDb = -10.0f;
            iconSpeaker.Texture = GD.Load<Texture2D>("res://multimedia/images/icons/speaker.png");
        }
        else {
            musicBackground.VolumeDb = -9999.0f;
            iconSpeaker.Texture = GD.Load<Texture2D>("res://multimedia/images/icons/speakerMuted.png");
        }
    }

    public void OnStartButtonPressed() {
        StartCoroutine(-80.0f);
        Game.ChangeState(1);
    }

    private async void StartCoroutine(float targetVolume)
    {
        float time = 0;
        float startVolume = musicBackground.VolumeDb;

        while (time < 2.5f)
        {
            time += (float)GetProcessDeltaTime();
            musicBackground.VolumeDb = Mathf.Lerp(startVolume, targetVolume, time / 2.5f);
            await Task.Delay(5);
        }
        musicBackground.VolumeDb = targetVolume;
    }

    public void OnSettingsButtonPressed() {
        GD.Print("Mostrando Settings...");
    }

    public void OnExitButtonPressed() {
        GetTree().Quit();
    }

    // Botones de credenciales

    public async void OnBtnRegistrarsePressedAsync()
    {
        try
        {
            // comprobar el nickname por si es único y la contraseña que sea segura
            PlayersControllers playersControllers = new PlayersControllers();
            Player player = null;

            player = await PlayersControllers.GetPlayerByNickname(nickname.Text);

            if (player != null) {
                lblError.Text = "¡El  nickname  utilizado  ya  existe!";
            } else {
                await playersControllers.InsertPlayer(nickname.Text, password.Text);

                Pokemon pokemonStarter = new Pokemon();

                pokemonStarter = await PokemonController.GetPokemonById(4);
                pokemonStarter.CalcularStats();

                Game.PlayerPlaying = player;

                var pokemonPlayersController = new PokemonPlayersController();
                bool added = await pokemonPlayersController.CapturarPokemon(pokemonStarter, GetTree().Root.GetNode("Game"));
            }

        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error durante el registro: {ex.Message} \n{ex.StackTrace}");
        }
    }

    public async void OnBtnIniciarSesionPressed()
    {
        // todo ok deja el player en Game y te muestra para darle a jugar y settings
        try
        {
            // comprobar el nickname por si es único y la contraseña que sea segura
            PlayersControllers playersControllers = new PlayersControllers();
            Player player = null;

            player = await PlayersControllers.Login(nickname.Text, password.Text);

            if (player != null) {
                await player.asignarPokemons();
                
                Game.PlayerPlaying = player;

                // Se asignan visualmente los pokemons del jugador en el menu principal
                var menuPrincipal = GetNode<MenuPrincipal>("/root/Game/inScreen/UI/MenuPrincipal");
                menuPrincipal.ColocarPokemonsVisual();


                // ventana de jugar
                var credencialesControl = GetNode<Control>("Credenciales");
                var buttonsMenu = GetNode<Control>("Buttons");

                credencialesControl.Visible = false;
                buttonsMenu.Visible = true;
            } else {
                lblError.Text = "¡Credenciales incorrectas!";
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr("Error general: ", ex.Message, ex.StackTrace);
            
            lblError.Text = "¡Error general!  Habla  con  soporte";
        }
    }

    public void OnBtnNoAccountPressed()
    {
        btnIniciarSesion.Visible = false;
        btnRegistrarse.Visible = true;

        btnNoAccount.Visible = false;
        btnYesAccount.Visible = true;
    }

    public void OnBtnYesAccountPressed()
    {
        btnRegistrarse.Visible = false;
        btnIniciarSesion.Visible = true;

        btnYesAccount.Visible = false;
        btnNoAccount.Visible = true;
    }
}

