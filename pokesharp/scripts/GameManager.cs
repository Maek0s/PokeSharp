using Godot;

public partial class GameManager : Node
{
    private PackedScene _combateScene = (PackedScene)GD.Load("res://scenes/Interfaces/combate.tscn"); // Ruta de la escena del combate
    private Node _combateInstance;
    
    [Export] private Sprite2D allyPokeSprite;
    [Export] private Sprite2D enemyPokeSprite;

    private Tween tween;

    public override void _Ready()
    {
        tween = CreateTween();
    }

    public void IniciarCombate()
    {
        // Oculta el mapa
        Node mapa = GetTree().GetFirstNodeInGroup("mapa");
        if (mapa is Node2D node2D)
            node2D.Visible = false;  // Ocultar el mapa


        // Instancia la escena de combate y la agrega al árbol de nodos
        _combateInstance = _combateScene.Instantiate();
        GetTree().GetRoot().AddChild(_combateInstance);

        // Mover ambos sprites con Tween (esto es lo que quieres hacer)
        MoverSprites();
    }

    public void TerminarCombate()
    {
        // Borra la escena de combate
        if (_combateInstance != null)
        {
            _combateInstance.QueueFree();
            _combateInstance = null;
        }

        // Muestra de nuevo el mapa
        Node mapa = GetTree().GetFirstNodeInGroup("mapa");
        if (mapa is Node2D node2D)
            node2D.Visible = false;
    }

    private void MoverSprites()
    {
        // Definir las posiciones de destino
        Vector2 nuevaPosAlly = new Vector2(300, 300);  // Nueva posición para el sprite aliado
        Vector2 nuevaPosEnemy = new Vector2(500, 300);  // Nueva posición para el sprite enemigo

        // Usar el Tween para animar ambos sprites a las nuevas posiciones
        tween.TweenProperty(allyPokeSprite, "position", nuevaPosAlly, 2.0f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(enemyPokeSprite, "position", nuevaPosEnemy, 2.0f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
    }
}
