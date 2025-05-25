using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class GeneralUtils : Node
{
    private Tween tween;
    public override void _Ready()
    {
        tween = GetTree().CreateTween();
    }

    public static void AsignValuesProgressBar(ProgressBar progressBar, Label hpPokemon, Pokemon pokemon) {
        if (pokemon == null)
            return;
        float porcentaje = (pokemon.currentHP / (float)pokemon.maxHP) * 100f;

        progressBar.MaxValue = pokemon.maxHP;
        progressBar.Value = porcentaje;

        Color color;

        if (porcentaje > 60)
            color = new Color(0.443f, 0.961f, 0.424f);
        else if (porcentaje > 30)
            color = new Color(1f, 1f, 0f);
        else
            color = new Color(1f, 0f, 0f);

        // Aplica el color al StyleBox de la barra (modificamos el Fill)
        StyleBoxFlat fillStyle = new StyleBoxFlat();
        fillStyle.BgColor = color;
        progressBar.AddThemeStyleboxOverride("fill", fillStyle);

        hpPokemon.Text = $"{pokemon.currentHP}/{pokemon.maxHP}";
        progressBar.MaxValue = pokemon.maxHP;
        progressBar.Value = pokemon.currentHP;
    }
    
    public static void AsignValuesProgressBarNoAnimation(ProgressBar progressBar, Pokemon pokemon) {
        if (pokemon == null)
            return;
        float porcentaje = (pokemon.currentHP / (float)pokemon.maxHP) * 100f;

        // Establece el valor del ProgressBar
        progressBar.MaxValue = pokemon.maxHP;
        progressBar.Value = porcentaje;

        // Cambia el color según el % actual
        Color color;

        if (porcentaje > 60)
            color = new Color(0.443f, 0.961f, 0.424f);
        else if (porcentaje > 30)
            color = new Color(1f, 1f, 0f);
        else
            color = new Color(1f, 0f, 0f);

        // Aplica el color al StyleBox de la barra (modificamos el Fill)
        StyleBoxFlat fillStyle = new StyleBoxFlat();
        fillStyle.BgColor = color;
        progressBar.AddThemeStyleboxOverride("fill", fillStyle);

        progressBar.MaxValue = pokemon.maxHP;
        progressBar.Value = pokemon.currentHP;
    }

    public void AsignValuesProgressBar(ProgressBar progressBar, Pokemon pokemon)
    {
        if (pokemon == null)
            return;

        if (tween == null || !tween.IsRunning())
        {
            tween = GetTree().CreateTween();
        }

        progressBar.MaxValue = pokemon.maxHP;
        float clampedHP = Mathf.Clamp(pokemon.currentHP, 0, pokemon.maxHP);
        float endValue = clampedHP;

        float startValue = (float)progressBar.Value;
        float duration = 0.55f;

        UpdateProgressBarColor(progressBar);

        tween.TweenProperty(progressBar, "value", endValue, duration)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.InOut);
        tween.Play();

        tween.Finished += () => UpdateProgressBarColor(progressBar);
    }

    private Timer animTimer;
    private float targetValue;
    private ProgressBar progressBarRef;
    private float animationSpeed = 100f;

    public void StartHealthAnimation(ProgressBar progressBar, Pokemon pokemon)
    {
        progressBarRef = progressBar;
        targetValue = pokemon.currentHP;
        progressBar.MaxValue = pokemon.maxHP;

        if (animTimer == null)
        {
            animTimer = new Timer();
            animTimer.WaitTime = 0.02f;
            animTimer.OneShot = false;
            animTimer.Timeout += OnAnimTimerTimeout;
            AddChild(animTimer);
        }

        animTimer.Start();
    }

    private static void UpdateProgressBarColor(ProgressBar progressBar)
    {
        if (!GodotObject.IsInstanceValid(progressBar))
            return;

        float porcentaje = (float) (progressBar.Value / progressBar.MaxValue) * 100f;

        Color color;

        if (porcentaje > 60)
            color = new Color(0.443f, 0.961f, 0.424f);
        else if (porcentaje > 30)
            color = new Color(1f, 1f, 0f);
        else
            color = new Color(1f, 0f, 0f);

        StyleBoxFlat fillStyle = new StyleBoxFlat();
        fillStyle.BgColor = color;
        fillStyle.SetCornerRadiusAll(0);
        progressBar.AddThemeStyleboxOverride("fill", fillStyle);
    }

    private void OnTweenStep() {
        UpdateProgressBarColor(progressBarRef);
    }

    private void OnAnimTimerTimeout()
    {
        if (Math.Abs(progressBarRef.Value - targetValue) < 0.01f || progressBarRef.Value == targetValue)
        {
            animTimer.Stop();
            return;
        }

        progressBarRef.Value = Mathf.MoveToward(progressBarRef.Value, targetValue, animationSpeed);
    }

    public static string FormatearTexto(String text) {
        string replaced = text.Replace("-", "  ");

        string formatted = string.IsNullOrEmpty(replaced)
            ? replaced
            : char.ToUpper(replaced[0]) + replaced.Substring(1);

        return formatted;
    }

    public static Color GetColorByType(String type) {
        if (string.IsNullOrEmpty(type))
            return Colors.Gray;

        switch (type.Trim().ToLowerInvariant())
        {
            case "normal":    return new Color(0.66f, 0.66f, 0.47f);
            case "fighting":  return new Color(0.78f, 0.30f, 0.22f);
            case "flying":    return new Color(0.64f, 0.55f, 0.78f);
            case "poison":    return new Color(0.64f, 0.32f, 0.64f);
            case "ground":    return new Color(0.86f, 0.70f, 0.43f);
            case "rock":      return new Color(0.68f, 0.59f, 0.44f);
            case "bug":       return new Color(0.53f, 0.78f, 0.16f);
            case "ghost":     return new Color(0.44f, 0.31f, 0.60f);
            case "steel":     return new Color(0.70f, 0.70f, 0.76f);
            case "fire":      return new Color(1.00f, 0.459f, 0.078f);
            case "water":     return new Color(0.37f, 0.58f, 1.00f);
            case "grass":     return new Color(0.48f, 0.78f, 0.36f);
            case "electric":  return new Color(1.00f, 0.85f, 0.10f);
            case "psychic":   return new Color(1.00f, 0.32f, 0.56f);
            case "ice":       return new Color(0.53f, 0.80f, 0.88f);
            case "dragon":    return new Color(0.50f, 0.48f, 0.96f);
            case "dark":      return new Color(0.42f, 0.36f, 0.35f);
            case "fairy":     return new Color(0.92f, 0.59f, 0.79f);

            default:
                GD.PrintErr("Tipo no definido: ", type);
                return Colors.Gray;
        }
    }

    public static Button SetButtonSettings(Button button, String typeName) {
        var style1 = new StyleBoxFlat();

        style1.BgColor = GetColorByType(typeName);

        // Configurar el borde del botón
        style1.BorderWidthTop = 2;
        style1.BorderWidthBottom = 2;
        style1.BorderWidthLeft = 2;
        style1.BorderWidthRight = 2;
        style1.BorderColor = new Color(0, 0, 0); // Borde negro

        // Asignar el estilo creado al botón
        button.AddThemeStyleboxOverride("normal", style1);

        button.Visible = true;

        return button;
    }
}