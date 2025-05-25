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
            GD.Print("[✅] 🚪 DoorSystem 🚪 Información de la puerta cargada. / Door data loaded successfully.");
        }
        else
        {
            GD.PrintErr("[❌] 🚪 DoorSystem 🚪 No se encontró el archivo doors_info.json / File doors_info.json was not found");
        }
    }
}
