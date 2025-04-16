using Godot;
using System;

public partial class BattleManager : CanvasLayer {

    PackedScene transitionScene;
    CanvasLayer battle;
    TransitionManager transitionManager;

    private Control moveset;
    private RichTextLabel combatlog;

    public override void _Ready()
    {
        Panel parteInferior = GetNode<Panel>("ParteInferior");
        Panel combatLogMoveset = parteInferior.GetNode<Panel>("CombatLogMoveset");

        Control botones = parteInferior.GetNode<Control>("Buttons");

        Button btnFight = botones.GetNode<Button>("Fight");
        Button btnBag = botones.GetNode<Button>("Bag");
        Button btnTeam = botones.GetNode<Button>("Team");
        Button btnRun = botones.GetNode<Button>("Run");

        var LblNamePokeEnemy = GetNode<Label>("InfoEnemy/namePokemon");

        combatlog = combatLogMoveset.GetNode<RichTextLabel>("CombatLog");
        moveset = combatLogMoveset.GetNode<Control>("Moveset");

        btnFight.Pressed += OnFightButtonPressed;
        btnBag.Pressed += OnBagButtonPressed;
        btnTeam.Pressed += OnTeamButtonPressed;
        btnRun.Pressed += OnRunButtonPressed;

        var namePokeEnemy = char.ToUpper(LblNamePokeEnemy.Text[0]) + LblNamePokeEnemy.Text.Substring(1).ToLower();

        combatlog.BbcodeEnabled = true;
        combatlog.Text += $"¡Ha  aparecido  un \u2009 [b]{namePokeEnemy}[/b] \u2009 salvaje!";
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

    public void OnFightButtonPressed() {
        GD.Print("Button Fight pressed");

        if (moveset.Visible) {
            moveset.Visible = false;
            combatlog.Visible = true;
        } else {
            moveset.Visible = true;
            combatlog.Visible = false;
        }
    }
    
    public void OnBagButtonPressed() {
        GD.Print("Button Bag pressed");
        
    }

    public void OnTeamButtonPressed() {
        GD.Print("Button Team pressed");
        
    }

    public void OnRunButtonPressed() {
        GD.Print("Button Huir pressed");

        TerminarCombate();
    }
}