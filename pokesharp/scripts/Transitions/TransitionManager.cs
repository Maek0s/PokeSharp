using Godot;
using System;
using System.Threading.Tasks;

public partial class TransitionManager : Node2D
{
    [Export] private Sprite2D allyPokeSprite;
    [Export] private Panel infoAlly;
    [Export] private Sprite2D enemyPokeSprite;
    [Export] private Panel infoEnemy;
    private PackedScene _combateScene = (PackedScene)GD.Load("res://scenes/interfaces/combate.tscn"); // Ruta de la escena del combate
    private Node _combateInstance;
    private Tween tween;

    CanvasLayer battle;
    TransitionManager transitionManager;

    private Control moveset;
    private TextEdit combatlog;

    public override void _Ready()
	{
	}

    public async void IniciarCombate()
    {
        // Oculta el mapa
        Node mapa = GetTree().GetFirstNodeInGroup("mapa");
        if (mapa is Node2D node2D)
            node2D.Visible = false;  // Ocultar el mapa

        await MoverSprites();
    }

    private Task MoverSprites()
    {
        var tcs = new TaskCompletionSource<bool>(); // Permite esperar la animación

        // Crear el Tween solo cuando se necesite
        Tween tween = GetTree().CreateTween();
        tween.SetParallel(true); // Esto hace que ambas animaciones ocurran a la vez

        // Definir las posiciones de destino
        Godot.Vector2 nuevaPosAlly = new Godot.Vector2(207, 276);
        Godot.Vector2 nuevaPosEnemy = new Godot.Vector2(652, 174);

        Godot.Vector2 nuevaInfoPosAlly = new Godot.Vector2(514, 301);
        Godot.Vector2 nuevaInfoPosEnemy = new Godot.Vector2(68, 21);

        // Usar el Tween para animar ambos sprites a las nuevas posiciones
        tween.TweenProperty(allyPokeSprite, "position", nuevaPosAlly, 1.5f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(infoAlly, "position", nuevaInfoPosAlly, 2.0f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        
        tween.TweenProperty(enemyPokeSprite, "position", nuevaPosEnemy, 1.5f)
             .SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.Out);

        tween.TweenProperty(infoEnemy, "position", nuevaInfoPosEnemy, 2.0f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);

        enemyPokeSprite.Modulate = new Color(0, 0, 0);
        tween.TweenProperty(enemyPokeSprite, "modulate", new Color(1, 1, 1), 2.5f);

        // Capturar cuando termina la animación
        tween.Finished += () => tcs.SetResult(true);

        return tcs.Task; // Espera la animación antes de continuar
    }

    
}
