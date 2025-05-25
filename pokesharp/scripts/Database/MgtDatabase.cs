using System.Collections;
using System.Collections.Generic;
using Godot;
using System;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Supabase;
using System.Linq;
using Microsoft.IdentityModel.Tokens;

public partial class MgtDatabase : Node
{
    public static string _supabaseUrl = "https://rvzhjtsvomuoiltyxcsu.supabase.co/rest/v1/";
    public static string _supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InJ2emhqdHN2b211b2lsdHl4Y3N1Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzYzNzkwNjYsImV4cCI6MjA1MTk1NTA2Nn0.VfQI9ol0Z9X1dw0hRxmOugdiR2g1DeMoZj4nE6x_65w";

    public static HttpRequest _httpRequest;
    public static HttpRequest _httpRequest2;
    private bool _isRequestInProgress = false;

    public static bool ConexionEstablecida = false;

    public override void _Ready()
    {
        if (_httpRequest == null)
        {
            _httpRequest = new HttpRequest();
            AddChild(_httpRequest);
        }

        // Probar la conexión a la base de datos
        TestConnection();
    }

    private Task<string> SendRequest(string url, string[] headers, Godot.HttpClient.Method method, string body)
    {
        var tcs = new TaskCompletionSource<string>();

        // Verificar que _httpRequest no sea nulo
        if (_httpRequest == null)
        {
            GD.PrintErr("[❌] HttpRequest no está inicializado.");
            tcs.SetResult(null);
            return tcs.Task;
        }

        // Solo desconectar si ya está conectado
        _httpRequest.RequestCompleted += OnRequestCompleted;

        byte[] bodyBytes = null;
        if (!string.IsNullOrEmpty(body))
        {
            bodyBytes = Encoding.UTF8.GetBytes(body);
        }

        var err = Error.Unavailable;

        try
        {
            err = _httpRequest.Request(url, headers, method, body);
        }
        catch (System.Exception e)
        {
            GD.PrintErr($"Error general {e.Message}");
            _httpRequest.RequestCompleted -= OnRequestCompleted;
            tcs.SetResult(null);
            return tcs.Task;
        }

        if (err != Error.Ok)
        {
            GD.PrintErr("[❌] Error al enviar la solicitud.");
            tcs.SetResult(null);
        }

        return tcs.Task;

        void OnRequestCompleted(long result, long responseCode, string[] responseHeaders, byte[] body)
        {
            _httpRequest.RequestCompleted -= OnRequestCompleted;

            string responseBody = Encoding.UTF8.GetString(body);

            if (responseCode >= 200 && responseCode < 300)
            {
                string json = Encoding.UTF8.GetString(body);
                tcs.TrySetResult(json);
            }
            else
            {
                GD.PrintErr($"[❌] Error en la solicitud: {responseCode}, Respuesta: {responseBody}");
                tcs.TrySetResult(null);
            }
        }
    }

    public async Task<T[]> GetFromTable<T>(string tableName, string query)
    {
        string url = $"{_supabaseUrl}{tableName}?{query}&apikey={_supabaseKey}";
        //GD.Print(url);
        var headers = new string[]
        {
            $"Authorization: Bearer {_supabaseKey}",
            "Content-Type: application/json"
        };

        var response = "";

        try
        {
            response = await SendRequest(url, headers, Godot.HttpClient.Method.Get, "");
        }
        catch (Exception e)
        {
            GD.PrintErr($"[❌] Error general al obtener información. {e.Message}");
        }

        if (response == null)
            return null;

        if (response.Equals(""))
            return null;

        return JsonConvert.DeserializeObject<T[]>(response);
    }

    public async Task<bool> InsertIntoTable(string tableName, Dictionary<string, object> data)
    {
        string url = $"{_supabaseUrl}{tableName}";
        string json = JsonConvert.SerializeObject(data);

        var headers = new string[]
        {
            $"apikey: {_supabaseKey}",
            $"Authorization: Bearer {_supabaseKey}",
            "Content-Type: application/json"
        };

        /*GD.Print(url);
        GD.Print(json);*/

        string response = await SendRequest(url, headers, Godot.HttpClient.Method.Post, json);

        if (response != null)
        {
            GD.Print($"✅ Insertado en {tableName}: {response}");
            return true;
        }
        else
        {
            GD.PrintErr($"[❌] Error al insertar en {tableName}");
            return false;
        }
    }


    private async void TestConnection()
    {
        // Hacer una solicitud simple a la tabla "pokemon" para verificar la conexión
        var url = $"{_supabaseUrl}pokemon?limit=1&apikey={_supabaseKey}";
        var headers = new string[]
        {
            $"Authorization: Bearer {_supabaseKey}",
            "Content-Type: application/json"
        };

        // Espera a que se complete la solicitud antes de continuar
        string response = await SendRequest(url, headers, Godot.HttpClient.Method.Get, "");
        if (response != null)
        {
            GD.Print("[✅] 🌐 Database 🌐 Conexión establecida.");
            ConexionEstablecida = true;
            GD.Print($"Respuesta: {response}");
        }
        else
        {
            GD.PrintErr("[❌] 🌐 Database 🌐 (Testing) Error al enviar la solicitud.");
        }
    }

    public async Task<List<Pokemon>> GetAllPokemons(int idPlayer)
    {
        string url = $"{_supabaseUrl}pokemon_players?player_id=eq.{idPlayer}&apikey={_supabaseKey}";

        GD.Print($"(GetAllPokemons) URL {url}");

        var headers = new string[]
        {
            $"Authorization: Bearer {_supabaseKey}",
            "Content-Type: application/json"
        };

        string response = await SendRequest(url, headers, Godot.HttpClient.Method.Get, "");

        if (response != null)
        {
            // Deserializar la respuesta a una ArrayList de Pokémon
            List<Pokemon> pokemons = JsonConvert.DeserializeObject<List<Pokemon>>(response);
            GD.Print($"✅ Pokémon obtenidos: {pokemons.Count}");
            return pokemons;
        }
        else
        {
            GD.PrintErr("[❌] Error al obtener los Pokémon.");
            return null;
        }
    }

    public async Task<List<Pokemon>> GetAllPokemonsInBoxes(int idPlayer)
    {
        string url = $"{_supabaseUrl}pokemon_players?player_id=eq.{idPlayer}&inTeam=eq.-1&apikey={_supabaseKey}";

        GD.Print($"(GetAllPokemons) URL {url}");

        var headers = new string[]
        {
            $"Authorization: Bearer {_supabaseKey}",
            "Content-Type: application/json"
        };

        string response = await SendRequest(url, headers, Godot.HttpClient.Method.Get, "");

        if (response != null)
        {
            // Deserializar la respuesta a una ArrayList de Pokémon
            List<Pokemon> pokemons = JsonConvert.DeserializeObject<List<Pokemon>>(response);
            GD.Print($"✅ Pokémon obtenidos: {pokemons.Count}");
            return pokemons;
        }
        else
        {
            GD.PrintErr("[❌] Error al obtener los Pokémon.");
            return null;
        }
    }

    public async Task<bool> UpdatePokemonInTeam(int playerId, int pokId, int newInTeam)
    {
        string url = $"{_supabaseUrl}pokemon_players?player_id=eq.{playerId}&id=eq.{pokId}";

        var headers = new string[]
        {
            $"apikey: {_supabaseKey}",
            $"Authorization: Bearer {_supabaseKey}",
            "Content-Type: application/json",
            "Prefer: return=representation"
        };

        var data = new Dictionary<string, object>
        {
            { "inTeam", newInTeam }
        };

        string json = JsonConvert.SerializeObject(data);

        /*GD.Print($"(UpdatePokemonInTeam) PATCH URL: \n{url}\n");
        GD.Print($"Body: {json}");*/

        string response = await SendRequest(url, headers, Godot.HttpClient.Method.Patch, json);

        if (response != null)
        {
            GD.Print($"✅ Pokémon actualizado: inTeam = {newInTeam}");
            return true;
        }
        else
        {
            GD.PrintErr("[❌] Error al actualizar el campo inTeam.");
            return false;
        }
    }

    public async Task<bool> UpdateFromEvolution(int pokePlayerId, Pokemon pokemon)
    {
        string url = $"{_supabaseUrl}pokemon_players?id=eq.{pokePlayerId}&player_id=eq.{Game.PlayerPlaying.id}";

        var headers = new string[]
        {
            $"apikey: {_supabaseKey}",
            $"Authorization: Bearer {_supabaseKey}",
            "Content-Type: application/json",
            "Prefer: return=representation"
        };

        var data = new Dictionary<string, object>
        {
            { "pok_id", pokemon.Id },
            { "pok_name", pokemon.Nombre },
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

        string json = JsonConvert.SerializeObject(data);

        GD.Print($"(UpdateFromEvolution) PATCH URL: \n{url}\n");
        GD.Print($"Body: {json}");

        string response = await SendRequest(url, headers, Godot.HttpClient.Method.Patch, json);

        if (response != null)
        {
            GD.Print($"✅ Pokémon actualizado {pokemon.NombreCamelCase}");
            return true;
        }
        else
        {
            GD.PrintErr("[❌] Error al actualizar el campo inTeam.");
            return false;
        }
    }

    public async Task<List<Movimiento>> GetAllMovesByPokemonId(int pokePlayerId)
    {
        string url =
            $"{_supabaseUrl}moves" +
            $"?select=move_id,move_name,type_id,move_power,move_pp,move_accuracy," +
            $"poke_players_moves!inner(poke_player_id)" +
            $"&poke_players_moves.poke_player_id=eq.{pokePlayerId}" +
            $"&apikey={_supabaseKey}";

        var headers = new string[]
        {
            $"Authorization: Bearer {_supabaseKey}",
            "Content-Type: application/json"
        };

        string response = await SendRequest(url, headers, Godot.HttpClient.Method.Get, "");

        if (string.IsNullOrEmpty(response))
        {
            GD.PrintErr("❌ Error al obtener los movimientos.");
            return null;
        }

        try
        {
            var moves = JsonConvert.DeserializeObject<List<Movimiento>>(response);
            return moves;
        }
        catch (System.Exception ex)
        {
            GD.PrintErr("❌ Error al deserializar movimientos: " + ex.Message);
            return null;
        }
    }

    public async Task<bool> InsertPokePlayerMove(Movimiento movimiento)
    {
        string url = $"{_supabaseUrl}poke_players_moves?apikey={_supabaseKey}";
        var headers = new[]
        {
            $"Authorization: Bearer {_supabaseKey}",
            "Content-Type: application/json",
            "Prefer: return=minimal"
        };

        var data = new
        {
            poke_player_id = Game.PlayerPlaying.id,
            move_id = movimiento.move_id,
            inSlot = -1
        };

        string body = JsonConvert.SerializeObject(data);

        string response = await SendRequest(url, headers, Godot.HttpClient.Method.Post, body);

        if (response != null)
        {
            GD.Print($"✅ Insertado poke_players_moves: poke_player_id={Game.PlayerPlaying.id}, move_id={movimiento.move_id}, inSlot={-1}");
            return true;
        }
        else
        {
            GD.PrintErr("❌ Error al insertar en poke_players_moves.");
            return false;
        }
    }

    public async Task<List<int>> GetTypeIdsByPokemonId(int pokId)
    {
        string url =
            $"{_supabaseUrl}pokemon_types" +
            $"?select=type_id" +
            $"&pok_id=eq.{pokId}" +
            $"&apikey={_supabaseKey}";

        //GD.Print($"(GetTypeIdsByPokemonId) URL: {url}");

        var headers = new string[]
        {
            $"Authorization: Bearer {_supabaseKey}",
            "Content-Type: application/json"
        };

        string response = await SendRequest(url, headers, Godot.HttpClient.Method.Get, "");

        if (string.IsNullOrEmpty(response))
        {
            GD.PrintErr("❌ Error al obtener los tipos.");
            return null;
        }

        try
        {
            var rawList = JsonConvert.DeserializeObject<List<Dictionary<string, int>>>(response);
            return rawList.Select(x => x["type_id"]).ToList();
        }
        catch (System.Exception ex)
        {
            GD.PrintErr("❌ Error al deserializar type_ids: " + ex.Message);
            return null;
        }
    }

    public async Task<int> GetMultiplicadorTipo(int atacanteTypeId, int defensorTypeId)
    {
        string url = $"{_supabaseUrl}type_efficacy" +
                    $"?select=damage_factor" +
                    $"&damage_type_id=eq.{atacanteTypeId}&target_type_id=eq.{defensorTypeId}" +
                    $"&apikey={_supabaseKey}";

        var headers = new string[] {
            $"Authorization: Bearer {_supabaseKey}",
            "Content-Type: application/json"
        };

        string response = await SendRequest(url, headers, Godot.HttpClient.Method.Get, "");

        if (string.IsNullOrEmpty(response)) return 100;

        try
        {
            var resultado = JsonConvert.DeserializeObject<List<TipoEfectividad>>(response);

            if (resultado.IsNullOrEmpty())
            {
                GD.PrintErr("Resultado es null or empty");
                return 100;
            }

            if (resultado == null || resultado[0].multiplicador == 0)
            {
                GD.PrintErr("Null or == 0");
                return 100;
            }
            else
            {
                GD.Print("Devuelve correctamente el resultado ", resultado[0].multiplicador);
                return resultado[0].multiplicador;
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Ha ocurrido un error. {e.Message} \n {e.StackTrace}");
            return 100;
        }
    }

    public class TipoEfectividad
    {
        [JsonProperty("damage_factor")]
        public int multiplicador { get; set; }

        public override string ToString()
        {
            return $"TipoEfectividad: multiplicador {multiplicador}";
        }
    }

    public async Task<bool> UpdateExpPokemon(int pokIDPK, int exp, int nivel)
    {
        string url = $"{_supabaseUrl}pokemon_players?id=eq.{pokIDPK}";

        var headers = new string[]
        {
            $"apikey: {_supabaseKey}",
            $"Authorization: Bearer {_supabaseKey}",
            "Content-Type: application/json",
        };

        var data = new Dictionary<string, object>
        {
            { "experienciaActual", exp },
            { "nivel", nivel}
        };

        string json = JsonConvert.SerializeObject(data);

        string response = await SendRequest(url, headers, Godot.HttpClient.Method.Patch, json);

        if (response != null)
        {
            GD.Print($"✅ Pokémon actualizado {pokIDPK}: Exp = {exp} {response}");
            return true;
        }
        else
        {
            GD.PrintErr("[❌] Error al actualizar el campo exp.");
            return false;
        }
    }

    public async Task<int?> GetNivelMinimoEvolucion(int pokIdBase)
    {
        string url = $"{_supabaseUrl}pokemon_evolution_matchup?evolves_from_species_id=eq.{pokIdBase}&select=*,pokemon_evolution!fk_evolved_species(evol_minimum_level)";

        var headers = new string[]
        {
            $"apikey: {_supabaseKey}",
            $"Authorization: Bearer {_supabaseKey}",
            "Content-Type: application/json"
        };

        string response = await SendRequest(url, headers, Godot.HttpClient.Method.Get, "");

        if (!string.IsNullOrEmpty(response))
        {
            var data = JsonConvert.DeserializeObject<List<EvolutionResponse>>(response);
            if (data != null && data.Count > 0 && data[0].pokemon_evolution.Count > 0)
            {
                return data[0].pokemon_evolution[0].evol_minimum_level;
            }
        }

        GD.Print("Respuesta evolucion: ", response);

        GD.Print("No se encontró el nivel mínimo de evolución, el pokémon tal vez se encuentra en su última evolución.");
        return null;
    }

    public class EvolutionResponse
    {
        public int pok_id { get; set; }
        public int? evolves_from_species_id { get; set; }
        public List<EvolutionData> pokemon_evolution { get; set; }
    }

    public class EvolutionData
    {
        public int? evol_minimum_level { get; set; }
    }

    public async Task<int?> GetEvolvedSpeciesId(int pokIdBase)
    {
        string url = $"{_supabaseUrl}pokemon_evolution_matchup?evolves_from_species_id=eq.{pokIdBase}&select=pok_id";

        var headers = new string[]
        {
            $"apikey: {_supabaseKey}",
            $"Authorization: Bearer {_supabaseKey}",
            "Content-Type: application/json"
        };

        string response = await SendRequest(url, headers, Godot.HttpClient.Method.Get, "");

        if (!string.IsNullOrEmpty(response))
        {
            var data = JsonConvert.DeserializeObject<List<EvolutionMatchupResponse>>(response);
            if (data != null && data.Count > 0)
            {
                return data[0].pok_id;
            }
        }

        GD.PrintErr("[❌] No se encontró una especie de evolución.");
        return null;
    }
    
    public class EvolutionMatchupResponse
    {
        public int pok_id { get; set; }
    }


}