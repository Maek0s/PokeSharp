using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public partial class PokePlayerMovesController {
    MgtDatabase mgtDatabase = new MgtDatabase();

    public async Task<List<Movimiento>> GetMovesetByPokeId(int pokeId) {
        List<Movimiento> moveset = await mgtDatabase.GetAllMovesByPokemonId(pokeId);

        if (moveset != null && moveset.Count > 4)
        {
            // Recortar la lista a solo los primeros 4 movimientos
            moveset = moveset.GetRange(0, 4);
        }

        return moveset;
    }

    public async Task<bool> InsertMovementPokePlayer(Movimiento movimiento)
    {
        bool result = await mgtDatabase.InsertPokePlayerMove(movimiento);

        if (result)
            GD.Print("Se ha insertado el movimiento ", movimiento);
        else
            GD.PrintErr("Error al insertar el movimiento ", movimiento);

        return result;
    }

}