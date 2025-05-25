using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Newtonsoft.Json;

public class Player
{
    [JsonProperty("player_id")]
    public int id { get; set; }
    [JsonProperty("nickname")]
    public string nickname { get; set; }
    [JsonProperty("password")]
    public string password { get; set; }
    [JsonProperty("skin")]
    public string skin { get; set; }
    public List<Pokemon> listPokemonsTeam { get; set; } = new List<Pokemon>();
    public List<Pokemon> listPokemonsCaja { get; set; } = new List<Pokemon>();
    public int MediaPoke = 0;

    public override string ToString()
    {
        return $"Player ID: {id}, Nickname: {nickname}, Skin: {skin}, Team Count: {listPokemonsTeam?.Count ?? 0}, Box Count: {listPokemonsCaja?.Count ?? 0}" +
               $"ListPokemonsTeam: {listPokemonsTeam}, ListPokemonsCaja: {listPokemonsCaja}";
    }

    public bool checkVivos()
    {
        foreach (Pokemon pokeInTeam in listPokemonsTeam)
        {
            if (pokeInTeam.currentHP > 0)
            {
                return true;
            }
        }

        return false;
    }

    public async Task<bool> AddPokeTeamAsync(Pokemon pokemon, Node sceneRoot)
    {
        await pokemon.getMovesetDB();

        listPokemonsTeam.Add(pokemon);

        GD.Print($"Pokémon entrado a AddPokeTeamAsync() \n:{pokemon}");

        var scriptMenuPrincipal = sceneRoot.GetNode<MenuPrincipal>("/root/Game/inScreen/UI/MenuPrincipal");

        var pokemonPlayersController = new PokemonPlayersController();

        int maxIdTeam = 1;

        bool[] slotsOcupados = new bool[6];

        // Marcar los slots ocupados según el valor de inTeam
        foreach (Pokemon poke in Game.PlayerPlaying.listPokemonsTeam)
        {
            if (poke.inTeam >= 1 && poke.inTeam <= 6)
            {
                slotsOcupados[poke.inTeam - 1] = true;
            }
        }

        // Buscar el primer slot libre
        for (int i = 0; i < 6; i++)
        {
            if (!slotsOcupados[i])
            {
                maxIdTeam = i + 1;
            }
        }

        pokemon.inTeam = maxIdTeam;
        scriptMenuPrincipal.ColocarPokemonsVisual();

        bool result = await pokemonPlayersController.UpdatePokemonInTeam(Game.PlayerPlaying.id, pokemon.IdPK, maxIdTeam);

        if (result)
        {
            GD.Print($"(UpdatePokemonInTeam) Ejecutado correctamente de playerId {Game.PlayerPlaying.id} - Id {pokemon.IdPK} - idTeam {maxIdTeam}");
        }

        return result;
    }

    public void RemovePokeTeam(Pokemon pokemon, Node sceneRoot)
    {
        GD.Print($"RemovePokeTeam() Pokémon a eliminar del equipo: \n{pokemon}");
        listPokemonsTeam.Remove(pokemon);

        var scriptMenuPrincipal = sceneRoot.GetNode<MenuPrincipal>("/root/Game/inScreen/UI/MenuPrincipal");
        scriptMenuPrincipal.ColocarPokemonsVisual();

        /*var pokemonPlayersController = new PokemonPlayersController();
        await pokemonPlayersController.UpdatePokemonInTeam(Game.PlayerPlaying.id, pokemon.IdPK, -1);*/
    }

    public async Task<bool> AddPokeBoxAsync(Pokemon pokemon, Node sceneRoot)
    {
        GD.Print($"Añadiendo pokemon {pokemon.NombreCamelCase} a la caja.");
        listPokemonsCaja.Add(pokemon);
        var scriptPcPokemons = sceneRoot.GetNode<PcPokemons>("/root/Game/inScreen/UI/MenuPrincipal/PCPokemons");

        scriptPcPokemons.updateBox();

        var pokemonPlayersController = new PokemonPlayersController();

        bool result = await pokemonPlayersController.UpdatePokemonInTeam(Game.PlayerPlaying.id, pokemon.IdPK, -1);

        return result;
    }

    public void RemovePokeBox(Pokemon pokemon, Node sceneRoot)
    {
        GD.Print($"Eliminando pokemon {pokemon.NombreCamelCase} de la caja.");
        listPokemonsCaja.Remove(pokemon);
        var scriptPcPokemons = sceneRoot.GetNode<PcPokemons>("/root/Game/inScreen/UI/MenuPrincipal/PCPokemons");
        scriptPcPokemons.updateBox();
    }

    public async Task asignarPokemons()
    {
        var pokemonPlayersController = new PokemonPlayersController();
        var listaAllPokemons = await pokemonPlayersController.GetPokemonsByPlayer(id);

        foreach (Pokemon poke in listaAllPokemons)
        {
            poke.CalcularStats();

            if (poke.inTeam == -1)
            {
                if (!listPokemonsCaja.Contains(poke))
                {
                    listPokemonsCaja.Add(poke);
                }
            }
            else
            {
                if (!listPokemonsTeam.Contains(poke))
                {
                    await poke.getMovesetDB();

                    if (poke.Movimientos == null || poke.Movimientos.Count < 4)
                    {
                        GD.Print($"Pokémon {poke.NombreCamelCase} con menos de 4 moves ({poke.Movimientos.Count} moves), generando moveset.");

                        await poke.generateMoveset(true);
                    }

                    GD.Print($"Pokémon en team:\n {poke}");
                    listPokemonsTeam.Add(poke);
                }
            }
        }
    }

    public static int CalcularNivelReferencia(List<Pokemon> equipo)
    {
        // Ordenar de mayor a menor nivel
        var niveles = equipo.Select(p => p.nivel).OrderByDescending(n => n).ToList();

        // Tomar los 3 niveles más altos
        var nivelesConsiderados = niveles.Take(Math.Min(3, niveles.Count));

        // Sacar la media de esos
        var media = (int)Mathf.Round(nivelesConsiderados.Average() * 0.90f);

        GD.Print("media ", media);

        return media;
    }

    public static void CurarEquipo(List<Pokemon> equipo)
    {
        foreach (Pokemon pokeTeam in equipo)
        {
            pokeTeam.currentHP = pokeTeam.maxHP;
        }
    }
}