using System.Collections.Generic;
using System.Threading.Tasks;

public partial class PokemonTypesController {
    MgtDatabase mgtDatabase = new MgtDatabase();

    public async Task<List<int>> GetTypesByPokemonId(int pokemonId) {
        List<int> typeids = await mgtDatabase.GetTypeIdsByPokemonId(pokemonId);

        return typeids;
    }

    public async Task<int> GetMultiplicadorTipo(int atacanteTypeId, int defensorTypeId) {
        int multiplicador = await mgtDatabase.GetMultiplicadorTipo(atacanteTypeId, defensorTypeId);

        return multiplicador;
    }
}

