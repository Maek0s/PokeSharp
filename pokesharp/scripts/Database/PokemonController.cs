using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Godot;

public partial class PokemonController {

    // Método para obtener un Pokémon por ID desde Supabase
    public static async Task<Pokemon> GetPokemonById(int id)
    {
        Pokemon pokemon = new Pokemon();
        MgtDatabase mgtDatabase = new MgtDatabase();

        // (TABLE pokemon) Pokemon attributes
        var listPokemons = await mgtDatabase.GetFromTable<Pokemon>("pokemon", $"pok_id=eq.{id}");
        pokemon = listPokemons[0];

        if (listPokemons.Length > 0)
        {
            GD.Print($"[🎮] Pokémon encontrado: {pokemon.Nombre}");
        }
        else
        {
            GD.PrintErr("[❌] No se encontró el Pokémon.");
        }

        // (TABLE base_stats) Stats base
        var statsData = await mgtDatabase.GetFromTable<Pokemon>("base_stats", $"pok_id=eq.{id}");
        if (statsData.Length > 0)
        {
            var stats = statsData[0];
            pokemon.b_hp = stats.b_hp;
            pokemon.b_atk = stats.b_atk;
            pokemon.b_def = stats.b_def;
            pokemon.b_sp_atk = stats.b_sp_atk;
            pokemon.b_sp_def = stats.b_sp_def;
            pokemon.b_speed = stats.b_speed;
        }

        // (TABLE pokemon_evolution_matchup) Capture rate

        var catchRateEvolution = await mgtDatabase.GetFromTable<Pokemon>("pokemon_evolution_matchup", $"pok_id=eq.{id}");
        if (catchRateEvolution.Length > 0) {
            var element = catchRateEvolution[0];
            pokemon.catchRate = element.catchRate;
        }

        GD.Print($"Pokemon: {pokemon}");

        return pokemon;
    }
}