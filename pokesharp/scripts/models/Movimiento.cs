using Newtonsoft.Json;

public partial class Movimiento {
    [JsonProperty("move_id")]
    public int move_id { get; set; }

    [JsonProperty("move_name")]
    public string move_name { get; set; }

    [JsonProperty("type_id")]
    public int type_id { get; set; }

    private string _type_name;

    public string type_name
    {
        get
        {
            // Si type_name es una cadena vacía, llama a setTypeName
            if (string.IsNullOrEmpty(_type_name))
            {
                setTypeName();
            }
            return _type_name;
        }
        set
        {
            _type_name = value;
        }
    }

    [JsonProperty("move_power")]
    public int? move_power { get; set; }

    [JsonProperty("move_pp")]
    public int? move_pp { get; set; }

    [JsonProperty("move_accuracy")]
    public int? move_accuracy { get; set; }

    public override string ToString()
    {
        return $"Move ID: {move_id}, Name: {move_name}, Type ID: {type_id}, Type Name: {type_name}, Power: {move_power}, PP: {move_pp}, Accuracy: {move_accuracy}";
    }

    public void setTypeName()
    {
        switch (type_id) {
            case 1:
                type_name = "normal";
                break;
            case 2:
                type_name = "fighting";
                break;
            case 3:
                type_name = "flying";
                break;
            case 4:
                type_name = "poison";
                break;
            case 5:
                type_name = "ground";
                break;
            case 6:
                type_name = "rock";
                break;
            case 7:
                type_name = "bug";
                break;
            case 8:
                type_name = "ghost";
                break;
            case 9:
                type_name = "steel";
                break;
            case 10:
                type_name = "fire";
                break;
            case 11:
                type_name = "water";
                break;
            case 12:
                type_name = "grass";
                break;
            case 13:
                type_name = "electric";
                break;
            case 14:
                type_name = "psychic";
                break;
            case 15:
                type_name = "ice";
                break;
            case 16:
                type_name = "dragon";
                break;
            case 17:
                type_name = "dark";
                break;
            case 18:
                type_name = "fairy";
                break;
        }
    }
}