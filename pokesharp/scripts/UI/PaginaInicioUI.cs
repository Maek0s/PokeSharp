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

    public override void _Ready()
    {
        var botones = GetNode<Control>("Buttons");

        btnStart = botones.GetNode<Button>("StartButton");
        btnSettings = botones.GetNode<Button>("AjustesButton");
        btnExit = botones.GetNode<Button>("SalirButton");

        btnStart.Pressed += OnStartButtonPressed;
        btnSettings.Pressed += OnSettingsButtonPressed;
        btnExit.Pressed += OnExitButtonPressed;

        // Inicio y Registro de usuario

        var credentialsPanel = GetNode<Control>("Credenciales/Panel");
        var credentialsButtons = GetNode<Control>("Credenciales/Panel/Botones");

        // variables de textedits

        lblError = credentialsPanel.GetNode<Label>("Error");
        nickname = credentialsPanel.GetNode<LineEdit>("Nickname");
        password = credentialsPanel.GetNode<LineEdit>("Password");

        nickname.Text = "";
        password.Text = "";

        btnRegistrarse = credentialsButtons.GetNode<Button>("Registrar");
        btnIniciarSesion = credentialsButtons.GetNode<Button>("Entrar");
        btnNoAccount = credentialsButtons.GetNode<Button>("btnNoAccount");
        btnYesAccount = credentialsButtons.GetNode<Button>("btnYesAccount");

        btnRegistrarse.Pressed += OnBtnRegistrarsePressedAsync;
        btnIniciarSesion.Pressed += OnBtnIniciarSesionPressed;
        btnNoAccount.Pressed += OnBtnNoAccountPressed;
        btnYesAccount.Pressed += OnBtnYesAccountPressed;
    }

    public void OnStartButtonPressed() {
        Game.ChangeState(1);
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
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr("Error durante el registro: ", ex.Message);
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

