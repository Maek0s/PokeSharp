using Newtonsoft.Json;

public class Pokemon {
    [JsonProperty("id")]
    public int IdPK { get; set; }

    [JsonProperty("pok_id")]
    public int Id { get; set; }

    [JsonProperty("pok_name")]
    public string Nombre { get; set; }
    public string NombreCamelCase { get; set; }

    [JsonProperty("pok_height")]
    public int Altura { get; set; }

    [JsonProperty("pok_weight")]
    public int Peso { get; set; } // El último dígito es decimal realmente

    [JsonProperty("pok_base_experience")]
    public int ExperienciaBase { get; set; }

    [JsonProperty("inTeam")]
    public int inTeam { get; set; }

    // Base stats

    [JsonProperty("b_hp")]
    public int b_hp { get; set; }
    [JsonProperty("b_atk")]
    public int b_atk { get; set; }
    [JsonProperty("b_def")]
    public int b_def { get; set; }
    [JsonProperty("b_sp_atk")]
    public int b_sp_atk { get; set; }
    [JsonProperty("b_sp_def")]
    public int b_sp_def { get; set; }
    [JsonProperty("b_speed")]
    public int b_speed { get; set; }
    
    // Stats
    public int maxHP { get; set; }
    public int currentHP { get; set; }

    public int atk { get; set; }
    public int def { get; set; }
    public int sp_atk { get; set; }
    public int sp_def { get; set; }
    public int speed { get; set; }

    // Catch rate
    [JsonProperty("capture_rate")]
    public int catchRate { get; set; }

    public int nivel { get; set; }

    public override string ToString()
    {
        return $"Pokemon {Nombre} (Pokedex ID: {Id})\n" +
               $"Nivel: {nivel}, HP: {currentHP}/{maxHP}\n, inTeam: {inTeam}" +
               $"Stats: ATK: {atk}, DEF: {def}, SP_ATK: {sp_atk}, SP_DEF: {sp_def}, SPEED: {speed}\n" +
               $"Base Stats: B_ATK: {b_atk}, B_DEF: {b_def}, B_SP_ATK: {b_sp_atk}, B_SP_DEF: {b_sp_def}, B_SPEED: {b_speed}\n" +
               $"Altura: {Altura}, Peso: {Peso}, Experiencia Base: {ExperienciaBase}, Tasa de Captura: {catchRate}\n";
    }

    public void CalcularStats()
    {
        maxHP = (int)(((2 * b_hp * nivel) / 100) + nivel + 10);
        currentHP = maxHP;

        atk = (int)(((2 * b_atk * nivel) / 100) + 5);
        def = (int)(((2 * b_def * nivel) / 100) + 5);
        sp_atk = (int)(((2 * b_sp_atk * nivel) / 100) + 5);
        sp_def = (int)(((2 * b_sp_def * nivel) / 100) + 5);
        speed = (int)(((2 * b_speed * nivel) / 100) + 5);

        // asignar nombre
        NombreCamelCase = char.ToUpper(Nombre[0]) + Nombre.Substring(1).ToLower();
    }
}