using Godot;
using System;
using System.Threading.Tasks;

public partial class TransitionManager : Node2D
{
    [Export] private Sprite2D allyPokeSprite;
    [Export] private Panel infoAlly;
    [Export] private Sprite2D enemyPokeSprite;
    [Export] private Panel infoEnemy;
    [Export] private Sprite2D spriteEntrenador;
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

    public async void IniciarCombateEntrenador(String entrenadorName)
    {
        Texture2D spriteEntrenadorTexture = GD.Load<Texture2D>($"res://assets/characters/trainerImages/{entrenadorName}.png");
        spriteEntrenador = GetNode<Sprite2D>("/root/Combate/EntrenadorSprite");
        spriteEntrenador.Texture = spriteEntrenadorTexture;

        // Oculta el mapa
        Node mapa = GetTree().GetFirstNodeInGroup("mapa");
        if (mapa is Node2D node2D)
            node2D.Visible = false;  // Ocultar el mapa

        BattleManager.turnoActual = BattleManager.TurnoEstado.AnimacionEnCurso;

        await MoverSpriteEntrenador(true);

        // Intervalo para ocultar los sprites
        //await ToSignal(GetTree().CreateTimer(0.8), "timeout");
        await GeneralUtils.Esperar(0.8f);

        await MoverSpriteEntrenadorFuera();

        // Intervalo mostrar los pokémon
        //await ToSignal(GetTree().CreateTimer(0.2), "timeout");
        await GeneralUtils.Esperar(0.2f);

        await MoverSprites();
    }

    public async Task MoverSpriteEntrenador(bool versusAppear)
    {
        var tcs = new TaskCompletionSource<bool>();

        var spritePlayer = GetNode<AnimatedSprite2D>("/root/Combate/PlayerSprite");
        var spriteEntrenador = GetNode<Sprite2D>("/root/Combate/EntrenadorSprite");
        var vsImage = GetNode<Sprite2D>("/root/Combate/Versus");

        if (versusAppear)
        {
            vsImage.Visible = true;
            vsImage.Scale = new Vector2(0.1f, 0.1f);
            vsImage.Modulate = new Color(1, 1, 1, 0);
        }

        Tween tween = GetTree().CreateTween();
        tween.SetParallel(true);

        // Movimiento y desoscurecimiento
        tween.TweenProperty(spritePlayer, "position", new Vector2(177.134f, 261.034f), 1.5f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);

        tween.TweenProperty(spriteEntrenador, "position", new Vector2(656, 154), 1.5f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);

        // Desoscurecer entrenador
        spriteEntrenador.Modulate = new Color(0, 0, 0);
        tween.TweenProperty(spriteEntrenador, "modulate", new Color(1, 1, 1), 2.5f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);

        if (versusAppear)
        {
            // Tween separado para el VS
            Tween vsTween = GetTree().CreateTween();
            vsTween.SetParallel(false);
            vsTween.TweenInterval(0.3f);

            // Fade in + explosión
            vsTween.TweenProperty(vsImage, "modulate:a", 1.0f, 0.2f)
                   .SetTrans(Tween.TransitionType.Linear);

            vsTween.TweenProperty(vsImage, "scale", new Vector2(1.4f, 1.4f), 0.3f)
                   .SetTrans(Tween.TransitionType.Elastic)
                   .SetEase(Tween.EaseType.Out);

            vsTween.TweenInterval(0.2f);

            vsTween.TweenProperty(vsImage, "scale", new Vector2(1.0f, 1.0f), 0.15f)
                   .SetTrans(Tween.TransitionType.Bounce)
                   .SetEase(Tween.EaseType.Out);
        }
    
        tween.Finished += () => tcs.SetResult(true);
        await tcs.Task;
    }


    private async Task<Task<bool>> MoverSpriteEntrenadorFuera()
    {
        var tcs = new TaskCompletionSource<bool>();

        var spritePlayer = GetNode<AnimatedSprite2D>("/root/Combate/PlayerSprite");
        var vsImage = GetNode<Sprite2D>("/root/Combate/Versus");

        Tween tween = GetTree().CreateTween();
        tween.SetParallel(true);

        // Definir las posiciones de destino
        Vector2 nuevaPosAlly = new Vector2(-134.317f, 261.034f);
        Vector2 nuevaPosEnemy = new Vector2(1010.83f, 154.622f);

        spriteEntrenador.Modulate = new Color(1, 1, 1);
        vsImage.Modulate = new Color(1, 1, 1, 1);

        // Movimiento del sprite del jugador
        tween.TweenProperty(spritePlayer, "position", nuevaPosAlly, 1.5f)
             .SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.Out);

        // Movimiento del sprite del entrenador
        tween.TweenProperty(spriteEntrenador, "position", nuevaPosEnemy, 1.5f)
             .SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.Out);

        // Fade out el sprite del entrenador durante el movimiento
        tween.TweenProperty(spriteEntrenador, "modulate", new Color(0, 0, 0), 1.5f)
             .SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.Out);

        // Fade out del VS al mismo tiempo
        tween.TweenProperty(vsImage, "modulate:a", 0.0f, 1.3f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.In);

        tween.Finished += () => tcs.SetResult(true);
        await tcs.Task;

        // Finalización
        tween.Finished += () => tcs.SetResult(true);

        return tcs.Task;
    }

    public Task MoverSprites()
    {
        var tcs = new TaskCompletionSource<bool>();

        // Crear el Tween solo cuando se necesite
        Tween tween = GetTree().CreateTween();

        // Esto hace que ambas animaciones ocurran a la vez
        tween.SetParallel(true);

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

    public Task MoverSpritesFuera()
    {
        var tcs = new TaskCompletionSource<bool>();

        Tween tween = GetTree().CreateTween();
        tween.SetParallel(true);

        //  var allyPokeSprite = GetNode<Sprite2D>("/root/Combate/PokeAlly");
        // var enemyPokeSprite = GetNode<Sprite2D>("/root/Combate/PokeEnemy");

        if (allyPokeSprite == null)
            GD.Print("allypokesprite null");

        // Definir las posiciones de destino
        Vector2 nuevaPosAlly = new Vector2(-131.92f, 278.286f);
        Vector2 nuevaPosEnemy = new Vector2(1000.09f, 174);

        Vector2 nuevaInfoPosAlly = new Vector2(514, 379.626f);
        Vector2 nuevaInfoPosEnemy = new Vector2(68, -73.657f);

        // Usar el Tween para animar ambos sprites a las nuevas posiciones
        tween.TweenProperty(allyPokeSprite, "position", nuevaPosAlly, 1.5f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(infoAlly, "position", nuevaInfoPosAlly, 2.0f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);

        tween.TweenProperty(enemyPokeSprite, "position", nuevaPosEnemy, 1.5f)
             .SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.Out);

        tween.TweenProperty(infoEnemy, "position", nuevaInfoPosEnemy, 2.0f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);

        enemyPokeSprite.Modulate = new Color(1, 1, 1, 1);

        tween.TweenProperty(enemyPokeSprite, "modulate:a", 0.0f, 1.5f)
             .SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.Out);

        tween.Finished += () => tcs.SetResult(true);

        return tcs.Task;
    }
}
