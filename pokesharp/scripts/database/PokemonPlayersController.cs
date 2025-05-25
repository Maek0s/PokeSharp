using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public partial class PokemonPlayersController {

    MgtDatabase mgtDatabase = new MgtDatabase();

    public async Task<List<Pokemon>> GetPokemonsByPlayer(int idPlayer)
    {
        List<Pokemon> listPoke = await mgtDatabase.GetAllPokemons(idPlayer);

        return listPoke;
    }

    public async Task<List<Pokemon>> GetPokemonsByPlayerInBoxes(int idPlayer)
    {
        List<Pokemon> listPoke = await mgtDatabase.GetAllPokemonsInBoxes(idPlayer);

        return listPoke;
    }

    public async Task<bool> UpdatePokemonInTeam(int playerId, int pokId, int newInTeam)
    {
        return await mgtDatabase.UpdatePokemonInTeam(playerId, pokId, newInTeam);
    }

    public async Task<bool> UpdateExpPokemon(int idPKPokemon, int exp, int nivel)
    {
        return await mgtDatabase.UpdateExpPokemon(idPKPokemon, exp, nivel);
    }

    public async Task<bool> UpdateFromEvolution(int idPKPokemon, Pokemon pokemon)
    {
        return await mgtDatabase.UpdateFromEvolution(idPKPokemon, pokemon);
    }

    public async Task<bool> CapturarPokemon(Pokemon pokemon, Node sceneRoot)
    {
        pokemon.CalcularStats();

        int inTeam = -1;

        if (Game.PlayerPlaying.listPokemonsTeam.Count < 6)
        {
            inTeam = Game.PlayerPlaying.listPokemonsTeam.Count;
        }

        var data = new Dictionary<string, object>()
        {
            { "player_id", Game.PlayerPlaying.id },
            { "pok_id", pokemon.Id },
            { "pok_name", pokemon.Nombre },
            { "inTeam", inTeam},
            { "nivel", pokemon.nivel },
            { "pok_height", pokemon.Altura },
            { "pok_weight", pokemon.Peso },

            { "b_hp", pokemon.b_hp },
            { "b_atk", pokemon.b_atk },
            { "b_def", pokemon.b_def },
            { "b_sp_atk", pokemon.b_sp_atk },
            { "b_sp_def", pokemon.b_sp_def },
            { "b_speed", pokemon.b_speed },

            { "maxHP", pokemon.maxHP },
            { "currentHP", pokemon.currentHP },
            { "atk", pokemon.atk },
            { "def", pokemon.def },
            { "sp_atk", pokemon.sp_atk },
            { "sp_def", pokemon.sp_def },
            { "speed", pokemon.speed }
        };

        var mgtDatabase = new MgtDatabase();
        bool success = await mgtDatabase.InsertIntoTable("pokemon_players", data);

        if (success)
            GD.Print("Captura insertada en tabla pokemon_players correctamente.");
        else
            GD.PrintErr("Captura no insertada correctamente en tabla pokemon_players");

        bool result = false;

        if (inTeam == -1)
        {
            result = await Game.PlayerPlaying.AddPokeBoxAsync(pokemon, sceneRoot);
        }
        else
        {
            result = await Game.PlayerPlaying.AddPokeTeamAsync(pokemon, sceneRoot);
        }

        return result;
    }
}