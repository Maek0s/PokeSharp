using Newtonsoft.Json;

public class Pokemon {
    [JsonProperty("pok_id")]
    public int Id { get; set; }

    [JsonProperty("pok_name")]
    public string Nombre { get; set; }

    [JsonProperty("pok_height")]
    public int Altura { get; set; }

    [JsonProperty("pok_weight")]
    public int Peso { get; set; }

    [JsonProperty("pok_base_experience")]
    public int ExperienciaBase { get; set; }

    public int Nivel { get; set; }
}