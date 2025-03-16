using Godot;
using System;
using System.Threading.Tasks;

public partial class Hierbas : Area2D
{
    private MainCharacter _player;
    private AnimatedSprite2D _animacion;
    private int _grassCount = 0;
    private int porcentage = 25;

	public override void _Ready()
    {
        _player = GetNode<MainCharacter>("/root/Game/Player");
        _animacion = _player.GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        foreach (Node child in GetChildren())
        {
            if (child is Area2D area)
            {
                area.BodyEntered += (body) => OnBodyEntered(body, area);
                area.BodyExited += (body) => OnBodyExited(body, area);
            }
        }
    }

    private async void OnBodyEntered(Node body, Node area)
    {
        if (body.IsInGroup("player"))
        {
            _grassCount++;
            
            Random rnd = new Random();

            int numRnd = rnd.Next(1, 100);
            GD.Print(numRnd);

            if (numRnd > 0 && numRnd <= porcentage)
            {
                GD.Print("Pokémon!");

                // Sacamos el nodo del juego para consultar sus variables
                var gameNode = GetNode<Game>("/root/Game");
                var playerNode = GetNode<MainCharacter>("/root/Game/Player");
                var transitionNode = GetNode<BattleTransition>("/root/Transitions/BattleTransition");

                playerNode.FreezePlayer();
                gameNode.estadoJuego = 2;
                await transitionNode.StartTransition();

                await Task.Delay(1000);  // 3000 milisegundos = 3 segundos

                GameManager gameManager = (GameManager)GetNode("/root/GameManager");
                gameManager.IniciarCombate();
            }

            _player.in_grass = true;
        }
    }

    private void OnBodyExited(Node body, Node area)
    {
        if (body.IsInGroup("player"))
        {
            _grassCount--;

            if (_grassCount <= 0)
            {
                _player.in_grass = false;
                _grassCount = 0;
            }
        }
    }

}
