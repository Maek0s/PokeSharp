using Godot;
using System;

public partial class Npc : CharacterBody2D
{
    [Export] public string Nombre = "NPC";
    [Export] public SpriteFrames spriteFrames;
    [Export] public string animacionInicial = "idle";
    [Export] public string[] Dialogos;
    [Export] private bool fight = false;

    private AnimatedSprite2D animatedSprite;
    private int dialogoActual = 0;
    private bool jugadorCerca = false;

    public override void _Ready()
    {
        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animatedSprite.SpriteFrames = spriteFrames;

        animatedSprite.Play(animacionInicial);

        var area = GetNode<Area2D>("Area2D");
        area.BodyEntered += OnJugadorEntrar;
        area.BodyExited += OnJugadorSalir;
    }

    public override void _Process(double delta)
    {
        if (jugadorCerca && Input.IsActionJustPressed("interact"))
        {
            Hablar();
        }
    }

    public void Hablar()
    {
        var dialogoUI = GetNode<Dialogo>("/root/Game/inScreen/UI/Dialogo");

        if (Dialogos != null && Dialogos.Length > 0)
        {
            if (dialogoActual < Dialogos.Length) {
                dialogoUI.MostrarTexto(Dialogos[dialogoActual], Nombre);
                dialogoActual = dialogoActual + 1;
            } else {
                dialogoUI.OcultarTexto();
                dialogoActual = 0;
            }
        } else {
            GD.Print("Dialogos != null && Dialogos.Length > 0");
        }
    }

    private void OnJugadorEntrar(Node body)
    {
        if (body.IsInGroup("jugador"))
        {
            GD.Print("Jugador in");
            jugadorCerca = true;
        }
    }

    private void OnJugadorSalir(Node body)
    {
        if (body.IsInGroup("jugador"))
        {
            GD.Print("Jugador out");
            jugadorCerca = false;

            var dialogoUI = GetNode<Dialogo>("/root/Game/inScreen/UI/Dialogo");
            dialogoUI.OcultarTexto();
        }
    }

}
