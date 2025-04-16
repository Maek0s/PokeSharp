using Godot;
using System;

public partial class PaginaInicioUI : Node2D
{

    private Button btnStart;
    private Button btnSettings;
    private Button btnExit;

    public override void _Ready()
    {
        btnStart = GetNode<Button>("StartButton");
        btnSettings = GetNode<Button>("AjustesButton");
        btnExit = GetNode<Button>("SalirButton");

        btnStart.Pressed += OnStartButtonPressed;
        btnSettings.Pressed += OnSettingsButtonPressed;
        btnExit.Pressed += OnExitButtonPressed;
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
}

