using Godot;
using System;

public partial class Dialogo : CanvasLayer
{
    private Panel caja;
    private RichTextLabel textLabel;
    private Label lblNameNpc;
    private Label lblContinueNpc;
    public override void _Ready()
    {
        textLabel = GetNode<RichTextLabel>("Caja/MarginContainer/Text");
        textLabel.Text = "";

        caja = GetNode<Panel>("Caja");
        caja.Visible = false;

        lblNameNpc = GetNode<Label>("Caja/PanelName/lblName");
        lblNameNpc.Text = "";

        lblContinueNpc = GetNode<Label>("Caja/PanelName/lblContinuar");

        // Obtenemos la letra asignada a continuar
        var inputs = InputMap.ActionGetEvents("interact");
        lblContinueNpc.Text = "";

        foreach (InputEvent ev in inputs)
        {
            if (ev is InputEventKey key)
            {
                string tecla = OS.GetKeycodeString(key.PhysicalKeycode);
                lblContinueNpc.Text = $"Presiona \u2009 {tecla} \u2009 para \u2009 seguir";
                break;
            }
        }

        if (lblContinueNpc.Text == "")
            lblContinueNpc.Text = $"Presiona \u2009 E \u2009 para \u2009 seguir";

    }

    public void MostrarTexto(string texto, string npcName)
    {
        var textVisible = "";

        Visible = true;

        var list = texto.Split(" ");

        foreach (var word in list)
        {
            textVisible += $"   {word}";
        }

        GD.Print($"Mostrando {texto}");

        caja.Visible = true;
        
        textLabel.Text = textVisible;
        lblNameNpc.Text = npcName;
    }

    public void OcultarTexto()
    {
        Visible = false;
        caja.Visible = false;
        textLabel.Text = "";
    }
}
