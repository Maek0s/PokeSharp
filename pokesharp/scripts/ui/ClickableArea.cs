using Godot;
using System;

public partial class ClickableArea : Control
{
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            Node parent = GetParent();

            Node parentOfParent = parent.GetParent();

            string parentName = parent.Name.ToString();
        
            string[] partes = parentName.Split("PanelPoke");

            var ruta = GetNode<CanvasLayer>("/root/Game/inScreen/UI/MenuPrincipal/PCPokemons");

            if (parentOfParent.Name.ToString() == "TuEquipo")
                PcPokemons.changeNumSelectedYourTeam(partes[1].ToInt(), ruta);
            else
                PcPokemons.changeNumSelected(partes[1].ToInt());
        }
    }
}
