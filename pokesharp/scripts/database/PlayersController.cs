using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Godot;
using Newtonsoft.Json;

public partial class PlayersControllers {

    public static async Task<Player> Login(string nickname, string password)
    {
        if (!MgtDatabase.ConexionEstablecida)
        {
            GD.PrintErr("Conexión no establecida todavía.");
            return null;
        }

        Player player = null;
        MgtDatabase mgtDatabase = new MgtDatabase();

        GD.Print($"input nickname '{nickname}'");

        var listPlayers = await mgtDatabase.GetFromTable<Player>("players", $"nickname=ilike.{nickname.Trim()}");

        if (listPlayers == null) return null;

        if (listPlayers.Length > 0)
        {
            player = listPlayers[0];

            if (player.password == password)
            {
                GD.Print($"[✅] Login exitoso para '{nickname}'.");
                return player;
            }
            else
            {
                GD.PrintErr($"[❌] Contraseña incorrecta para '{nickname}'.");
                return null;
            }
        }
        else
        {
            GD.PrintErr($"[❌] Player '{nickname}' no encontrado.");
            return null;
        }
    }

    public static async Task<Player> GetPlayerByNickname(string nickname) {
        if (!MgtDatabase.ConexionEstablecida) return null;

        Player player = null;
        MgtDatabase mgtDatabase = new MgtDatabase();

        GD.Print($"input nickname '{nickname}'");

        var listPlayers = await mgtDatabase.GetFromTable<Player>("players", $"nickname=ilike.{nickname.Trim()}");

        if (listPlayers.Length > 0) {
            player = listPlayers[0];
        } else {
            GD.PrintErr($"[❌] Player '{nickname}' not found.");
            return player;
        }

        return player;
    }

    public async Task<bool> InsertPlayer(string nickname, string password)
    {
        MgtDatabase mgtDatabase = new MgtDatabase();

        var _supabaseKey = MgtDatabase._supabaseKey;
        var _supabaseUrl = MgtDatabase._supabaseUrl;
        var _httpRequest = MgtDatabase._httpRequest;

        string url = $"{_supabaseUrl}players?apikey={_supabaseKey}";
        GD.Print($"[📤] Insertando player: {nickname}");

        var headers = new string[]
        {
            $"Authorization: Bearer {_supabaseKey}",
            "Content-Type: application/json"
        };

        var newPlayer = new
        {
            nickname = nickname,
            password = password
        };

        string jsonBody = JsonConvert.SerializeObject(newPlayer);

        var tcs = new TaskCompletionSource<bool>();

        _httpRequest.RequestCompleted += OnRequestCompleted;

        var err = _httpRequest.Request(url, headers, HttpClient.Method.Post, jsonBody);
        if (err != Error.Ok)
        {
            GD.PrintErr("[❌] Error al enviar la solicitud de inserción.");
            tcs.SetResult(false);
        }

        return await tcs.Task;

        void OnRequestCompleted(long result, long responseCode, string[] responseHeaders, byte[] body)
        {
            _httpRequest.RequestCompleted -= OnRequestCompleted;

            if (responseCode == 201)
            {
                GD.Print("[✅] Player insertado correctamente.");
                tcs.TrySetResult(true);
            }
            else
            {
                string errorMsg = System.Text.Encoding.UTF8.GetString(body);
                GD.PrintErr($"[❌] Error al insertar player: {responseCode} → {errorMsg}");
                tcs.TrySetResult(false);
            }
        }
    }

}