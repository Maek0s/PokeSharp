using Godot;
using System;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

public partial class MgtDatabase : Node
{
    private static string _supabaseUrl = "https://rvzhjtsvomuoiltyxcsu.supabase.co/rest/v1/"; // URL de tu proyecto en Supabase
    private static string _supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InJ2emhqdHN2b211b2lsdHl4Y3N1Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzYzNzkwNjYsImV4cCI6MjA1MTk1NTA2Nn0.VfQI9ol0Z9X1dw0hRxmOugdiR2g1DeMoZj4nE6x_65w"; // Tu clave pública de la API

    private static HttpRequest _httpRequest;
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

    public async Task<T[]> GetFromTable<T>(string tableName, string query)
    {
        string url = $"{_supabaseUrl}{tableName}?{query}&apikey={_supabaseKey}"; // &apikey={_supabaseKey}
        GD.Print(url);
        var headers = new string[]
        {
            $"Authorization: Bearer {_supabaseKey}",
            "Content-Type: application/json"
        };

        var response = await SendRequest(url, headers);
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

        // Suscribirse al evento de la solicitud antes de hacerla
        //_httpRequest.RequestCompleted += _onRequestCompleted;

        // Espera a que se complete la solicitud antes de continuar
        string response = await SendRequest(url, headers);
        if (response != null)
        {
            GD.Print("[✅] 🌐 Database 🌐 (Testing) Conexión establecida.");
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

    private Task<string> SendRequest(string url, string[] headers)
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
        // Ensure we unsubscribe from the event before subscribing again
        _httpRequest.RequestCompleted += OnRequestCompleted;

        var err = _httpRequest.Request(url, headers);
        if (err != Error.Ok)
        {
            GD.PrintErr("[❌] Error al enviar la solicitud.");
            tcs.SetResult(null);
        }

        return tcs.Task;

        void OnRequestCompleted(long result, long responseCode, string[] responseHeaders, byte[] body)
        {
            _httpRequest.RequestCompleted -= OnRequestCompleted; // Desuscribirse después de recibir la respuesta

            if (responseCode == 200)
            {
                string json = Encoding.UTF8.GetString(body);
                tcs.TrySetResult(json); // Evita excepciones si la tarea ya se completó
            }
            else
            {
                GD.PrintErr($"[❌] Error en la solicitud: {responseCode}");
                tcs.TrySetResult(null);
            }
        }
        /*var tcs = new TaskCompletionSource<string>();
        var httpRequest = new HttpRequest(); // Crear una instancia nueva para evitar conflictos
        AddChild(httpRequest);

        _httpRequest.RequestCompleted += (result, responseCode, responseHeaders, body) =>
        {
            if (responseCode == 200)
            {
                string json = Encoding.UTF8.GetString(body);
                tcs.SetResult(json);
            }
            else
            {
                GD.PrintErr($"[❌] Error en la solicitud: {responseCode}");
                tcs.SetResult(null);
            }
        };

        var err = _httpRequest.Request(url, headers);
        if (err != Error.Ok)
        {
            GD.PrintErr("[❌] Error al enviar la solicitud.");
            tcs.SetResult(null);
        }

        return tcs.Task;*/
    }
}
