using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MenuPrincipal : CanvasLayer
{
    private Button btnPokemonsPC;
    
    public override void _Ready()
    {
        var panelPokemons = GetNode<Panel>("Pokemons");

        // Botones
        btnPokemonsPC = GetNode<Button>("OpcionesMenu/PokemonsOpcion/btnPokemonsCaja");
        btnPokemonsPC.Pressed += OnPokemonsPCPressed;
    }

    public void OnPokemonsPCPressed()
    {
        var PCPokemons = GetNode<CanvasLayer>("PCPokemons");
        PCPokemons.Visible = true;

        var scriptPcPokemons = GetNode<PcPokemons>("/root/Game/inScreen/UI/MenuPrincipal/PCPokemons");
        scriptPcPokemons.getPokemonsCaja();
    }

    public void ColocarPokemonsVisual()
    {
        Game.PlayerPlaying.listPokemonsTeam = Game.PlayerPlaying.listPokemonsTeam
        .OrderBy(p => p.inTeam)
        .ToList();

        List<Pokemon> pokeTeam = Game.PlayerPlaying.listPokemonsTeam;

        for (int i = 1; i <= 6; i++) {
            var nodePanel = GetNode<Panel>($"Pokemons/Pokemon{i}");

            if (nodePanel != null)
                VaciarHueco(nodePanel);
            else
                break;
        }

        int contador = 1;
        foreach (Pokemon pokemon in pokeTeam) {
            //GD.Print($"Asignando Pokemon {pokemon.Nombre} Nv.{pokemon.nivel} - inTeam {pokemon.inTeam}");
            if (contador > 6) {
                GD.PrintErr("Lista equipo mayor que 6");
                break;
            }

            var nodePanel = GetNode<Panel>($"Pokemons/Pokemon{contador}");

            if (string.IsNullOrWhiteSpace(pokemon.Nombre)) {
                GD.PrintErr("Error al leer un pokémon");
                VaciarHueco(nodePanel);
                continue;
            }

            //GD.Print($"Asignando Pokemon {pokemon.Nombre} {pokemon.nivel}");
            if (contador > 6) {
                GD.PrintErr("Lista equipo mayor que 6");
                break;
            }

            var lblName = nodePanel.GetNode<Label>("LblNamePokemon");
            lblName.Text = pokemon.NombreCamelCase;

            var lblNivel = nodePanel.GetNode<Label>("LblLevelPokemon");
            lblNivel.Text = $"Nv. {pokemon.nivel}";

            var progressBar = nodePanel.GetNode<ProgressBar>("ProgressBar");
            progressBar.Visible = true;

            var hpPokemon = progressBar.GetNode<Label>("Label");

            var sprite = nodePanel.GetNode<Sprite2D>("Sprite");
            sprite.Texture = GD.Load<Texture2D>($"res://assets/spritespc/{pokemon.Nombre}.png");

            GeneralUtils.AsignValuesProgressBar(progressBar, hpPokemon, pokemon);

            contador++;
        }
    }

    public void VaciarHueco(Panel nodePanel)
    {
        var lblName = nodePanel.GetNode<Label>("LblNamePokemon");
        lblName.Text = "";

        var lblNivel = nodePanel.GetNode<Label>("LblLevelPokemon");
        lblNivel.Text = "";

        var progressBar = nodePanel.GetNode<ProgressBar>("ProgressBar");
        progressBar.Visible = false;

        var hpPokemon = progressBar.GetNode<Label>("Label");
        hpPokemon.Text = "";

        var sprite = nodePanel.GetNode<Sprite2D>("Sprite");
        sprite.Texture = null;
    }
}
