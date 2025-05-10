using Godot;
using System;
using System.Collections.Generic;

public partial class GeneralUtils
{
    public static void AsignValuesProgressBar(ProgressBar progressBar, Label hpPokemon, Pokemon pokemon)
    {
        if (pokemon == null)
            return;
        float porcentaje = (pokemon.currentHP / (float)pokemon.maxHP) * 100f;

        // Establece el valor del ProgressBar
        progressBar.MaxValue = pokemon.maxHP;
        progressBar.Value = porcentaje;

        // Cambia el color según el % actual
        Color color;

        if (porcentaje > 60)
            color = new Color(0f, 0.61f, 0.23f); // Verde
        else if (porcentaje > 30)
            color = new Color(1f, 1f, 0f); // Amarillo
        else
            color = new Color(1f, 0f, 0f); // Rojo

        // Aplica el color al StyleBox de la barra (modificamos el Fill)
        StyleBoxFlat fillStyle = new StyleBoxFlat();
        fillStyle.BgColor = color;
        progressBar.AddThemeStyleboxOverride("fill", fillStyle);

        hpPokemon.Text = $"{pokemon.currentHP}/{pokemon.maxHP}";
        progressBar.MaxValue = pokemon.maxHP;
        progressBar.Value = pokemon.currentHP;
    }
}