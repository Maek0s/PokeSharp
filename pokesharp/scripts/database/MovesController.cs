using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Godot;
using Newtonsoft.Json;
using Supabase;

public partial class MovesController {

    public int movesTutorPorcentage = 45;
    public int movesTypePorcentage = 35;
    public int movesRandomPorcenatage = 20;

    private Client _client;
    private string _supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InJ2emhqdHN2b211b2lsdHl4Y3N1Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzYzNzkwNjYsImV4cCI6MjA1MTk1NTA2Nn0.VfQI9ol0Z9X1dw0hRxmOugdiR2g1DeMoZj4nE6x_65w";

    public MovesController()
    {
        var options = new SupabaseOptions
        {
            AutoConnectRealtime = false
        };

        // Creamos un cliente para poder usar las funciones de dentro de la base de datos
        _client = new Client("https://rvzhjtsvomuoiltyxcsu.supabase.co", _supabaseKey, options);
        _client.InitializeAsync().Wait();
    }

    public async Task<Movimiento> GetMovimientoByPorcentage(int idPokemon)
    {
        Random rnd = new Random();
        Movimiento movimiento = new Movimiento();

        int numRnd = rnd.Next(1, 100);

        GD.Print($"Número random de GetMovement() {numRnd} - IDPokemon {idPokemon}");

        if (numRnd <= movesRandomPorcenatage) {
            GD.Print("move random");
            movimiento = await GetRandomMovement();

            GD.Print(movimiento);
        } else if (numRnd > movesRandomPorcenatage && movesTypePorcentage <= numRnd) {
            GD.Print("move type");
            movimiento = await GetRandomTypeMovement(idPokemon);

            GD.Print(movimiento);
        } else {
            GD.Print("move tutor");
            movimiento = await GetRandomTutorMovement(idPokemon);

            GD.Print(movimiento);
        }

        if (movimiento == null) {
            movimiento = await GetRandomMovement();
            GD.Print("Movement was null, generando random, ", movimiento);
        }

        return movimiento;
    }

    public async Task<Movimiento> GetMovimientoByCustomPorcentage(int idPokemon, int movesTutor, int movesType, int movesRandom)
    {
        Random rnd = new Random();
        Movimiento movimiento = new Movimiento();

        int numRnd = rnd.Next(1, 100);

        GD.Print($"Número random de GetMovement() {numRnd} - IDPokemon {idPokemon}");

        if (numRnd <= movesRandom) {
            GD.Print("move random");
            movimiento = await GetRandomMovement();

            GD.Print(movimiento);
        } else if (numRnd > movesRandom && movesType <= numRnd) {
            GD.Print("move type");
            movimiento = await GetRandomTypeMovement(idPokemon);

            GD.Print(movimiento);
        } else {
            GD.Print("move tutor");
            movimiento = await GetRandomTutorMovement(idPokemon);

            GD.Print(movimiento);
        }

        if (movimiento == null) {
            movimiento = await GetRandomMovement();
            GD.Print("Movement was null, generando random, ", movimiento);
        }

        return movimiento;
    }

    public async Task<Movimiento> GetRandomMovement()
    {
        var response = await _client.Rpc("get_random_move", null);
        var resultList = JsonConvert.DeserializeObject<List<Movimiento>>(response.Content.ToString());

        var movement = resultList[0];
        movement.setTypeName();

        return movement;
    }

    public async Task<Movimiento> GetRandomTutorMovement(int pokemonId)
    {
        try
        {
            // Llamamos a la función de Supabase para obtener un movimiento aleatorio aprendido por tutor
            var response = await _client.Rpc("get_random_move_of_tutor", new { pokemon_id = pokemonId });

            // Si la respuesta no es null
            if (response.Content != null)
            {
                // Deserializamos la respuesta en un objeto Movimiento
                var resultList = JsonConvert.DeserializeObject<List<Movimiento>>(response.Content.ToString());

                if (resultList != null && resultList.Count > 0)
                {
                    Movimiento movimiento = resultList[0];
                    movimiento.setTypeName();

                    return movimiento;
                }
                else
                {
                    GD.Print("No se encontraron movimientos para este Pokémon.");
                    return null;
                }
            }
            else
            {
                GD.Print("No se encontraron movimientos para este Pokémon.");
                return null;
            }
        }
        catch (System.Exception ex)
        {
            GD.PrintErr("Error al obtener el movimiento: " + ex.Message);
            return null;
        }
    }

    public async Task<Movimiento> GetRandomTypeMovement(int pokemonId)
    {
        try
        {
            // Llamamos a la función de Supabase para obtener un movimiento aleatorio aprendido por tutor
            var response = await _client.Rpc("get_random_move_by_pokemon_type", new { pokemon_id = pokemonId });

            // Si la respuesta no es null
            if (response.Content != null)
            {
                // Deserializamos la respuesta en un objeto Movimiento
                var resultList = JsonConvert.DeserializeObject<List<Movimiento>>(response.Content.ToString());

                if (resultList != null && resultList.Count > 0)
                {
                    Movimiento movimiento = resultList[0];
                    movimiento.setTypeName();

                    return movimiento;
                }
                else
                {
                    GD.Print("No se encontraron movimientos para este Pokémon.");
                    return null;
                }
            }
            else
            {
                GD.Print("No se encontraron movimientos para este Pokémon.");
                return null;
            }
        }
        catch (System.Exception ex)
        {
            GD.PrintErr("Error al obtener el movimiento: " + ex.Message);
            return null;
        }
    }

}