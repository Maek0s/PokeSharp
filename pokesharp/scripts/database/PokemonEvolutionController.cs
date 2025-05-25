using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Godot;

public partial class PokemonEvolutionController
{
    MgtDatabase mgtDatabase = new MgtDatabase();

    // Método para obtener un Pokémon por ID desde Supabase
    public async Task<int?> GetEvolutionFrom(int idPokemon)
    {
        int? intId = null;

        intId = await mgtDatabase.GetEvolvedSpeciesId(idPokemon);

        return intId;
    }
}