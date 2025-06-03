using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Host;
using Newtonsoft.Json;


//[JsonObject(MemberSerialization.OptIn)]
[Tool]
public partial class Pokemon : Resource
{
    [JsonProperty("id")]
    public int IdPK { get; set; }

    [JsonProperty("pok_id")]
    [Export]
    public int Id { get; set; }

    [JsonProperty("pok_name")]
   // [Export]
    public string Nombre { get; set; }
    public string NombreCamelCase { get; set; }

    [JsonProperty("pok_height")]
    public int Altura { get; set; }

    [JsonProperty("pok_weight")]
    public int Peso { get; set; } // El último dígito es decimal realmente

    [JsonProperty("pok_base_experience")]
    [Export]
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
    [JsonProperty("experienciaActual")]
    public int experienciaActual { get; set; }

    public int nivel { get; set; }

    public List<Movimiento> Movimientos { get; set; } = new List<Movimiento>();

    public override string ToString()
    {
        return $"Pokemon {Nombre} (Pokedex ID: {Id})\n" +
               $"Nivel: {nivel}, HP: {currentHP}/{maxHP}, inTeam: {inTeam} \n" +
               $"Stats: ATK: {atk}, DEF: {def}, SP_ATK: {sp_atk}, SP_DEF: {sp_def}, SPEED: {speed}, MAX_HP: {maxHP}\n" +
               $"Base Stats: B_ATK: {b_atk}, B_DEF: {b_def}, B_SP_ATK: {b_sp_atk}, B_SP_DEF: {b_sp_def}, B_SPEED: {b_speed}, B_HP: {b_hp} \n" +
               $"Altura: {Altura}, Peso: {Peso}, Experiencia Base: {ExperienciaBase}, ExperienciaActual: {experienciaActual}, Tasa de Captura: {catchRate}\n" +
               $"Movimientos: \n- {string.Join("\n- ", Movimientos)}\n";
    }

    public void CalcularStats()
    {
        CalcularStatsBattle();
        currentHP = maxHP;
    }

    public void CalcularStatsBattle()
    {
        maxHP = (int)(((2 * b_hp * nivel) / 100) + nivel + 10);

        atk = (int)(((2 * b_atk * nivel) / 100) + 5);
        def = (int)(((2 * b_def * nivel) / 100) + 5);
        sp_atk = (int)(((2 * b_sp_atk * nivel) / 100) + 5);
        sp_def = (int)(((2 * b_sp_def * nivel) / 100) + 5);
        speed = (int)(((2 * b_speed * nivel) / 100) + 5);

        // Asigna el nombre
        NombreCamelCase = char.ToUpper(Nombre[0]) + Nombre.Substring(1).ToLower();
    }

    public async Task<bool> getMovesetDB()
    {
        PokePlayerMovesController pokePlayerMovesController = new PokePlayerMovesController();
        Movimientos = await pokePlayerMovesController.GetMovesetByPokeId(IdPK);

        //GD.Print($"Movimientos conseguidos de DB de {NombreCamelCase} - Movimientos:\n- {string.Join("\n- ", Movimientos)}\n");

        return true;
    }

    public async Task<bool> generateMoveset(bool insertDB)
    {
        MovesController movesController = new MovesController();

        var move1 = await movesController.GetMovimientoByPorcentage(Id);
        move1.setTypeName();

        var move2 = await movesController.GetMovimientoByPorcentage(Id);
        move2.setTypeName();

        var move3 = await movesController.GetMovimientoByPorcentage(Id);
        move3.setTypeName();

        var move4 = await movesController.GetMovimientoByPorcentage(Id);
        move4.setTypeName();

        Movimientos.Add(move1);
        Movimientos.Add(move2);
        Movimientos.Add(move3);
        Movimientos.Add(move4);

        GD.Print(Movimientos);

        if (insertDB)
        {
            var mgtDatabase = new MgtDatabase();

            for (int i = 0; i < Movimientos.Count; i++)
            {
                var movimiento = Movimientos[i];

                var data = new Dictionary<string, object>()
                {
                    { "poke_player_id", IdPK },
                    { "move_id", movimiento.move_id },
                    { "inSlot", i + 1 }
                };

                bool insertSuccess = await mgtDatabase.InsertIntoTable("poke_players_moves", data);

                if (insertSuccess)
                    GD.Print($"✅ Movimiento insertado correctamente en el slot {i}: {movimiento.move_name}");
                else
                    GD.PrintErr($"❌ Error al insertar el movimiento en el slot {i}: {movimiento.move_name}");
            }
        }
        return true;
    }

    public async Task<int> AtacarPokemon(Pokemon pokemonEnemy, Movimiento movUtilizado)
    {
        CalcularStatsBattle();
        pokemonEnemy.CalcularStatsBattle();

        GD.Print($"Nivel pokemon atacante {nivel} \n" +
                 $"movUtilizadoMovePower {movUtilizado.move_power} \n" +
                 $"atk {atk} \n" +
                 $"pokeEnemy DEF {pokemonEnemy.def}"
        );

        if (movUtilizado.move_accuracy != 100)
        {
            Random rnd = new Random();
            var numRandom = rnd.Next(1, 100);

            GD.Print($"Numrandom {numRandom} acc - {movUtilizado.move_accuracy}");

            // Movimiento fallido
            if (numRandom > movUtilizado.move_accuracy)
                return -1;
        }

        GD.Print($"(((2f * {nivel} / 5f) + 2f) * {movUtilizado.move_power * atk / pokemonEnemy.def}) / 50f + 2f");

        float daño = (float)(((2f * nivel / 5f) + 2f) * movUtilizado.move_power * atk / pokemonEnemy.def) / 50f + 2f;

        GD.Print($"Daño resultado formula: {daño}");

        PokemonTypesController pokemonTypesController = new PokemonTypesController();

        List<int> typeIdsAlly = await pokemonTypesController.GetTypesByPokemonId(Id);
        List<int> typeIdsEnemy = await pokemonTypesController.GetTypesByPokemonId(pokemonEnemy.Id);

        if (typeIdsAlly != null)
        {
            if (typeIdsAlly.Count > 0)
            {
                bool tieneStab = typeIdsAlly.Contains(movUtilizado.type_id);
                if (tieneStab)
                {
                    daño *= 1.3f;
                }
            }
        }

        if (GenerarCritico())
        {
            GD.Print("¡Golpe crítico!");
            daño *= 1.5f;
        }

        GD.Print($"Daño después de Stab y crit: {daño}");

        float multiplicadorTotal = 1.0f;

        try
        {
            foreach (int defensorTypeId in typeIdsEnemy)
            {
                int multiplicador = await pokemonTypesController.GetMultiplicadorTipo(movUtilizado.type_id, defensorTypeId);
                GD.Print($"Calculando multiplicador de tipo {movUtilizado.type_id} vs {defensorTypeId}, resultado {multiplicador}");
                float factor = multiplicador / 100f;
                multiplicadorTotal *= factor;
            }
        }
        catch (Exception e)
        {
            GD.PrintErr("Error al obtener los tipos ", e.Message, e.StackTrace);
        }

        daño *= multiplicadorTotal;

        GD.Print($"Con multiplicadorTotal {multiplicadorTotal} daño total: {daño}");

        return (int)daño;
    }

    public bool GenerarCritico()
    {
        Random rand = new Random();
        return rand.Next(0, 16) == 0;
    }

    public async Task AñadirExperiencia(int cantidad)
    {
        // Límite de nivel
        if (nivel == 100) return;

        experienciaActual += cantidad;
        GD.Print($"¡{NombreCamelCase} obtuvó {cantidad} de experiencia! Experiencia actual {experienciaActual}");

        while (experienciaActual >= LevelSystem.ExperienciaParaSubirNivel(nivel + 1) && nivel < 100)
        {
            experienciaActual -= LevelSystem.ExperienciaParaSubirNivel(nivel + 1);
            nivel++;
            CalcularStats();
            GD.Print($"{NombreCamelCase} subió al nivel {nivel}!");
        }

        // A partir de este nivel ningún pokémon evoluciona, así nos evitamos peticiones SQL
        if (nivel < 65)
        {
            MgtDatabase mgtDatabase = new MgtDatabase();

            var nivelEvo = await mgtDatabase.GetNivelMinimoEvolucion(Id);

            if (nivelEvo.HasValue && nivelEvo.Value <= nivel)
            {
                PokemonEvolutionController pokemonEvolutionController = new PokemonEvolutionController();
                GD.Print($"{NombreCamelCase} ha evolucionado. nivel {nivel} {nivelEvo.Value}");

                var intId = await pokemonEvolutionController.GetEvolutionFrom(Id);

                if (intId != null)
                {
                    await Evolucionar((int)intId);
                }
            }
            else if (nivelEvo.HasValue)
            {
                GD.Print($"{NombreCamelCase} no ha evolucionado todavía. nivel {nivel} {nivelEvo.Value}");
            }
            else
            {
                GD.Print($"No tendrá más evoluciones posiblemente");
            }
        }

        

        Game.PlayerPlaying.MediaPoke = Player.CalcularNivelReferencia(Game.PlayerPlaying.listPokemonsTeam);
    }

    public async Task Evolucionar(int newId)
    {
        Pokemon pokemon = await PokemonController.GetPokemonById(newId);
        pokemon.CalcularStats();

        var text = $"¡{NombreCamelCase}  ha  evolucionado  a  {pokemon.NombreCamelCase}!";

        // Cambiamos sus stats y todo lo importante
        Id = newId;
        b_atk = pokemon.b_atk;
        b_def = pokemon.b_def;
        b_hp = pokemon.b_hp;
        b_sp_atk = pokemon.b_sp_atk;
        b_sp_def = pokemon.b_sp_def;
        Peso = pokemon.Peso;
        Altura = pokemon.Altura;
        Nombre = pokemon.Nombre;

        CalcularStats();

        MainCharacter.ChangeTextFloating(text);

        PokemonPlayersController pokemonPlayersController = new PokemonPlayersController();

        // Actualizamos la base de datos
        await pokemonPlayersController.UpdateFromEvolution(IdPK, pokemon);
    }
}