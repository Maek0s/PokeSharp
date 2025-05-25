using Godot;

public partial class LevelSystem
{
    // Para acelerar el juego
    public static float multiplicadorExp = 1.5f;

    public static int CalcularExperienciaGanada(int expBase, int nivelRival)
    {
        return Mathf.FloorToInt((expBase * nivelRival / 7f) * multiplicadorExp);
    }

    // Formula para saber la experiencia necesaria para subir de nivel
    public static int ExperienciaParaSubirNivel(int nivel)
    {
        return 100 + (nivel * 20);
    }
}