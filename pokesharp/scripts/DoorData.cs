using Godot;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public partial class DoorData : Node
{
    public Dictionary<string, Dictionary<string, string>> DoorDestinations = new();

    public override void _Ready()
    {
        string path = "res://data/jsons/doors_info.json";
        if (Godot.FileAccess.FileExists(path))
        {
            string jsonText = Godot.FileAccess.GetFileAsString(path);
            DoorDestinations = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonText);
            GD.Print("✅ Door data loaded successfully. / Información de la puerta cargada. ✅");
        }
        else
        {
            GD.PrintErr("❌ File doors_info.json was not found / No se encontró el archivo doors_info.json ❌");
        }
    }
}
