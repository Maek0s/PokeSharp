using Godot;
using System;

public partial class PauseMenuInterface : CanvasLayer
{
    public override void _Ready()
    {
        Button btnContinue = GetNode<Button>("Panel/btnContinue");
        Button btnSettings = GetNode<Button>("Panel/btnSettings");
        Button btnExit = GetNode<Button>("Panel/btnExit");

        btnContinue.Pressed += OnContinueButtonPressed;
        btnSettings.Pressed += OnSettingsButtonPressed;
        btnExit.Pressed += OnExitButtonPressed;
    }

    public void OnContinueButtonPressed() {
        var gameNode = GetNode<Game>("/root/Game");

        gameNode.TogglePauseMenu();
    }

    public void OnSettingsButtonPressed() {
        GD.Print("Settings pressed.");
    }

    public void OnExitButtonPressed() {
        Game.ChangeState(-1);
    }
}
