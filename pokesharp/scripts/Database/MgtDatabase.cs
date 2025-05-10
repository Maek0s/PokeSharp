using System.Collections;
using System.Collections.Generic;
using Godot;
using System;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

public partial class MgtDatabase : Node
{
    public static string _supabaseUrl = "https://rvzhjtsvomuoiltyxcsu.supabase.co/rest/v1/";
    public static string _supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InJ2emhqdHN2b211b2lsdHl4Y3N1Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzYzNzkwNjYsImV4cCI6MjA1MTk1NTA2Nn0.VfQI9ol0Z9X1dw0hRxmOugdiR2g1DeMoZj4nE6x_65w";

    public static HttpRequest _httpRequest;
    public static HttpRequest _httpRequest2;
    private bool _isRequestInProgress = false;

    public override void _Ready()
    {
        //_httpRequest = GetNode<HttpRequest>("/root/Game/SupabaseService/HttpRequest");

        if (_httpRequest == null) {
            _httpRequest = new HttpRequest();
            AddChild(_httpRequest);
        }

        // Probar la conexión a la base de datos
        TestConnection();
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

        GD.Print(url);
        GD.Print(json);

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

    public async Task<T[]> GetFromTable<T>(string tableName, string query)
    {
        string url = $"{_supabaseUrl}{tableName}?{query}&apikey={_supabaseKey}";
        GD.Print(url);
        var headers = new string[]
        {
            $"Authorization: Bearer {_supabaseKey}",
            "Content-Type: application/json"
        };

        var response = "";

        try {
            response = await SendRequest(url, headers, Godot.HttpClient.Method.Get, "");
        } catch (Exception e) {
            GD.PrintErr($"[❌] Error general al obtener información. {e.Message}");
        }

        if (response.Equals(""))
            return null;

        return JsonConvert.DeserializeObject<T[]>(response);
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
            GD.Print($"Respuesta: {response}");
        }
        else
        {
            GD.PrintErr("[❌] 🌐 Database 🌐 (Testing) Error al enviar la solicitud.");
        }
        // Realizar la solicitud
       /* var err = _httpRequest.Request(url, headers);
        if (err != Error.Ok)
        {
            GD.PrintErr("[❌] 🌐 Database 🌐 Error al enviar la solicitud.");
        }*/
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

        GD.Print($"(UpdatePokemonInTeam) PATCH URL: \n{url}\n");
        GD.Print($"Body: {json}");

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
}
    
/*
    public async Task<bool> UpdatePokemonInTeam(int playerId, int pokId, int newInTeam)
    {
        string url = $"{_supabaseUrl}pokemon_players?player_id=eq.{playerId}&pok_id=eq.{pokId}";

        var headers = new string[]
        {
            $"apikey: {_supabaseKey}",
            $"Authorization: Bearer {_supabaseKey}",
            "Content-Type: application/json",
            "Prefer: return=representation"
        };

        var data = new Dictionary<string, int>
        {
            { "inTeam", newInTeam }
        };

        string json = JsonConvert.SerializeObject(data);

        GD.Print($"(UpdatePokemonInTeam) PATCH URL: {url}");
        GD.Print($"Body: {json}");

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
*/
/*
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

            if (err == Error.Busy) {
                _httpRequest2 = new HttpRequest();
                
                AddChild(_httpRequest2);
                _httpRequest2.RequestCompleted += OnRequestCompleted2;

                err = _httpRequest2.Request(url, headers, method, body);
            }
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
*/
        // void OnRequestCompleted2(long result, long responseCode, string[] responseHeaders, byte[] body)
        // {
        //     _httpRequest.RequestCompleted -= OnRequestCompleted2;

        //     string responseBody = Encoding.UTF8.GetString(body);

        //     if (responseCode >= 200 && responseCode < 300)
        //     {
        //         string json = Encoding.UTF8.GetString(body);
        //         tcs.TrySetResult(json);
        //     }
        //     else
        //     {
        //         GD.PrintErr($"[❌] Error en la solicitud: {responseCode}, Respuesta: {responseBody}");
        //         tcs.TrySetResult(null);
        //     }
        // }
    //}
  
