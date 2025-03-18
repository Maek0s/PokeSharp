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

        return pokemon;
    }
}